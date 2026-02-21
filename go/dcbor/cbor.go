package dcbor

import (
	"bytes"
	"encoding/binary"
	"encoding/hex"
	"fmt"
	"math"
	"math/big"
	"reflect"
	"strconv"
	"strings"
	"unicode/utf8"

	"github.com/fxamacker/cbor/v2"
	"golang.org/x/text/unicode/norm"
)

type majorType byte

const (
	majorUnsigned majorType = 0
	majorNegative majorType = 1
	majorBytes    majorType = 2
	majorText     majorType = 3
	majorArray    majorType = 4
	majorMap      majorType = 5
	majorTagged   majorType = 6
	majorSimple   majorType = 7
)

// CBORKind identifies the CBOR variant represented by a CBOR value.
type CBORKind int

const (
	CBORKindUnsigned CBORKind = iota
	CBORKindNegative
	CBORKindByteString
	CBORKindText
	CBORKindArray
	CBORKindMap
	CBORKindTagged
	CBORKindSimple
)

// CBORCase is a copyable tagged-union view of a CBOR value.
type CBORCase struct {
	Kind  CBORKind
	Value any
}

// TaggedValue stores tag/content pair.
type TaggedValue struct {
	Tag   Tag
	Value CBOR
}

// CBOR is the core deterministic CBOR symbolic type.
type CBOR struct {
	kind  CBORKind
	value any
}

func NewCBORUnsigned(value uint64) CBOR {
	return CBOR{kind: CBORKindUnsigned, value: value}
}

func NewCBORNegative(encodedMagnitude uint64) CBOR {
	return CBOR{kind: CBORKindNegative, value: encodedMagnitude}
}

func NewCBORByteString(value ByteString) CBOR {
	return CBOR{kind: CBORKindByteString, value: value}
}

func NewCBORText(value string) CBOR {
	return CBOR{kind: CBORKindText, value: norm.NFC.String(value)}
}

func NewCBORArray(value []CBOR) CBOR {
	copied := make([]CBOR, len(value))
	for i, v := range value {
		copied[i] = v.Clone()
	}
	return CBOR{kind: CBORKindArray, value: copied}
}

func NewCBORMap(value Map) CBOR {
	return CBOR{kind: CBORKindMap, value: value.Clone()}
}

func NewCBORTagged(tag Tag, value CBOR) CBOR {
	return CBOR{kind: CBORKindTagged, value: TaggedValue{Tag: tag.clone(), Value: value.Clone()}}
}

func NewCBORSimple(simple Simple) CBOR {
	return CBOR{kind: CBORKindSimple, value: simple}
}

func False() CBOR {
	return NewCBORSimple(SimpleFalseValue())
}

func True() CBOR {
	return NewCBORSimple(SimpleTrueValue())
}

func Null() CBOR {
	return NewCBORSimple(SimpleNullValue())
}

func NaN() CBOR {
	return NewCBORSimple(SimpleFloatValue(math.NaN()))
}

func ToByteString(data []byte) CBOR {
	return NewCBORByteString(NewByteString(data))
}

func ToByteStringFromHex(value string) (CBOR, error) {
	bytesValue, err := hex.DecodeString(value)
	if err != nil {
		return CBOR{}, err
	}
	return ToByteString(bytesValue), nil
}

func ToTaggedValue(tag Tag, value CBOR) CBOR {
	return NewCBORTagged(tag, value)
}

// FromAny converts common Go values into CBOR.
func FromAny(value any) (CBOR, error) {
	if encodable, ok := value.(CBOREncodable); ok {
		return encodable.ToCBOR().Clone(), nil
	}

	switch v := value.(type) {
	case CBOR:
		return v.Clone(), nil
	case *CBOR:
		if v == nil {
			return Null(), nil
		}
		return v.Clone(), nil
	case bool:
		if v {
			return True(), nil
		}
		return False(), nil
	case nil:
		return Null(), nil
	case string:
		return NewCBORText(v), nil
	case []byte:
		return NewCBORByteString(NewByteString(v)), nil
	case ByteString:
		return NewCBORByteString(v), nil
	case []CBOR:
		return NewCBORArray(v), nil
	case Map:
		return NewCBORMap(v), nil
	case *Map:
		if v == nil {
			return NewCBORMap(NewMap()), nil
		}
		return NewCBORMap(v.Clone()), nil
	case Set:
		return NewCBORArray(v.AsVec()), nil
	case Date:
		return v.TaggedCBOR(), nil
	case Simple:
		return NewCBORSimple(v), nil
	case uint8:
		return NewCBORUnsigned(uint64(v)), nil
	case uint16:
		return NewCBORUnsigned(uint64(v)), nil
	case uint32:
		return NewCBORUnsigned(uint64(v)), nil
	case uint64:
		return NewCBORUnsigned(v), nil
	case uint:
		return NewCBORUnsigned(uint64(v)), nil
	case int8:
		return fromSignedInt(int64(v)), nil
	case int16:
		return fromSignedInt(int64(v)), nil
	case int32:
		return fromSignedInt(int64(v)), nil
	case int64:
		return fromSignedInt(v), nil
	case int:
		return fromSignedInt(int64(v)), nil
	case float32:
		return newCBORFromFloat(float64(v)), nil
	case float64:
		return newCBORFromFloat(v), nil
	default:
		return fromReflectValue(reflect.ValueOf(value))
	}
}

func MustFromAny(value any) CBOR {
	cbor, err := FromAny(value)
	if err != nil {
		panic(err)
	}
	return cbor
}

func fromReflectValue(v reflect.Value) (CBOR, error) {
	if !v.IsValid() {
		return Null(), nil
	}

	for v.Kind() == reflect.Interface || v.Kind() == reflect.Pointer {
		if v.IsNil() {
			return Null(), nil
		}
		v = v.Elem()
	}

	switch v.Kind() {
	case reflect.Bool:
		return FromAny(v.Bool())
	case reflect.String:
		return FromAny(v.String())
	case reflect.Int, reflect.Int8, reflect.Int16, reflect.Int32, reflect.Int64:
		return fromBigSigned(v.Int())
	case reflect.Uint, reflect.Uint8, reflect.Uint16, reflect.Uint32, reflect.Uint64, reflect.Uintptr:
		return FromAny(v.Uint())
	case reflect.Float32, reflect.Float64:
		return FromAny(v.Float())
	case reflect.Slice:
		if v.Type().Elem().Kind() == reflect.Uint8 {
			data := make([]byte, v.Len())
			reflect.Copy(reflect.ValueOf(data), v)
			return FromAny(data)
		}
		items := make([]CBOR, 0, v.Len())
		for i := 0; i < v.Len(); i++ {
			item, err := fromReflectValue(v.Index(i))
			if err != nil {
				return CBOR{}, err
			}
			items = append(items, item)
		}
		return NewCBORArray(items), nil
	case reflect.Array:
		items := make([]CBOR, 0, v.Len())
		for i := 0; i < v.Len(); i++ {
			item, err := fromReflectValue(v.Index(i))
			if err != nil {
				return CBOR{}, err
			}
			items = append(items, item)
		}
		return NewCBORArray(items), nil
	case reflect.Map:
		if v.IsNil() {
			return NewCBORMap(NewMap()), nil
		}
		m := NewMap()
		iter := v.MapRange()
		for iter.Next() {
			key, err := fromReflectValue(iter.Key())
			if err != nil {
				return CBOR{}, err
			}
			value, err := fromReflectValue(iter.Value())
			if err != nil {
				return CBOR{}, err
			}
			m.Insert(key, value)
		}
		return NewCBORMap(m), nil
	default:
		return CBOR{}, Errorf("unsupported conversion to CBOR: %T", v.Interface())
	}
}

func fromBigSigned(value int64) (CBOR, error) {
	if value >= 0 {
		return NewCBORUnsigned(uint64(value)), nil
	}
	return NewCBORNegative(uint64(-1 - value)), nil
}

func fromSignedInt(v int64) CBOR {
	if v >= 0 {
		return NewCBORUnsigned(uint64(v))
	}
	return NewCBORNegative(uint64(-1 - v))
}

func newCBORFromFloat(value float64) CBOR {
	if reduced, ok := reduceFloatToIntegerCBOR(value); ok {
		return reduced
	}
	return NewCBORSimple(SimpleFloatValue(value))
}

func reduceFloatToIntegerCBOR(value float64) (CBOR, bool) {
	if !isIntegralFloat(value) {
		return CBOR{}, false
	}

	asInteger, accuracy := big.NewFloat(value).Int(nil)
	if accuracy != big.Exact {
		return CBOR{}, false
	}

	if asInteger.Sign() >= 0 {
		if asInteger.BitLen() > 64 {
			return CBOR{}, false
		}
		return NewCBORUnsigned(asInteger.Uint64()), true
	}

	encodedMagnitude := new(big.Int).Neg(asInteger)
	encodedMagnitude.Sub(encodedMagnitude, big.NewInt(1))
	if encodedMagnitude.Sign() < 0 || encodedMagnitude.BitLen() > 64 {
		return CBOR{}, false
	}
	return NewCBORNegative(encodedMagnitude.Uint64()), true
}

func (c CBOR) Kind() CBORKind {
	return c.kind
}

func (c CBOR) ToCBOR() CBOR {
	return c.Clone()
}

func (c CBOR) Clone() CBOR {
	switch c.kind {
	case CBORKindUnsigned, CBORKindNegative:
		return CBOR{kind: c.kind, value: c.value.(uint64)}
	case CBORKindByteString:
		return NewCBORByteString(c.value.(ByteString))
	case CBORKindText:
		return NewCBORText(c.value.(string))
	case CBORKindArray:
		array := c.value.([]CBOR)
		copied := make([]CBOR, len(array))
		for i, item := range array {
			copied[i] = item.Clone()
		}
		return NewCBORArray(copied)
	case CBORKindMap:
		return NewCBORMap(c.value.(Map).Clone())
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		return NewCBORTagged(tagged.Tag.clone(), tagged.Value.Clone())
	case CBORKindSimple:
		return NewCBORSimple(c.value.(Simple))
	default:
		return c
	}
}

func (c CBOR) AsCase() CBORCase {
	return CBORCase{Kind: c.kind, Value: c.caseValueCopy()}
}

func (c CBOR) IntoCase() CBORCase {
	return c.AsCase()
}

func (c CBOR) caseValueCopy() any {
	switch c.kind {
	case CBORKindArray:
		array := c.value.([]CBOR)
		copied := make([]CBOR, len(array))
		for i, item := range array {
			copied[i] = item.Clone()
		}
		return copied
	case CBORKindMap:
		return c.value.(Map).Clone()
	case CBORKindTagged:
		tv := c.value.(TaggedValue)
		return TaggedValue{Tag: tv.Tag.clone(), Value: tv.Value.Clone()}
	case CBORKindByteString:
		return c.value.(ByteString)
	default:
		return c.value
	}
}

func (c CBOR) AsUnsigned() (uint64, bool) {
	if c.kind != CBORKindUnsigned {
		return 0, false
	}
	return c.value.(uint64), true
}

func (c CBOR) AsInt64() (int64, bool) {
	switch c.kind {
	case CBORKindUnsigned:
		u := c.value.(uint64)
		if u > math.MaxInt64 {
			return 0, false
		}
		return int64(u), true
	case CBORKindNegative:
		u := c.value.(uint64)
		if u > math.MaxInt64 {
			return 0, false
		}
		return -1 - int64(u), true
	default:
		return 0, false
	}
}

func (c CBOR) AsFloat64() (float64, bool) {
	switch c.kind {
	case CBORKindUnsigned:
		return float64(c.value.(uint64)), true
	case CBORKindNegative:
		return -1.0 - float64(c.value.(uint64)), true
	case CBORKindSimple:
		simple := c.value.(Simple)
		if simple.Kind() != SimpleFloat {
			return 0, false
		}
		value, _ := simple.Float64()
		return value, true
	default:
		return 0, false
	}
}

func (c CBOR) TryIntoUInt64() (uint64, error) {
	if c.kind == CBORKindUnsigned {
		return c.value.(uint64), nil
	}
	if c.kind == CBORKindNegative {
		return 0, ErrOutOfRange
	}
	return 0, ErrWrongType
}

func (c CBOR) TryUInt64() (uint64, error) {
	return c.TryIntoUInt64()
}

func (c CBOR) IntoUInt64() (uint64, bool) {
	value, err := c.TryIntoUInt64()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryIntoInt64() (int64, error) {
	switch c.kind {
	case CBORKindUnsigned:
		u := c.value.(uint64)
		if u > math.MaxInt64 {
			return 0, ErrOutOfRange
		}
		return int64(u), nil
	case CBORKindNegative:
		u := c.value.(uint64)
		if u > math.MaxInt64 {
			return 0, ErrOutOfRange
		}
		return -1 - int64(u), nil
	default:
		return 0, ErrWrongType
	}
}

func (c CBOR) TryInt64() (int64, error) {
	return c.TryIntoInt64()
}

func (c CBOR) IntoInt64() (int64, bool) {
	value, err := c.TryIntoInt64()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryIntoInt32() (int32, error) {
	value, err := c.TryIntoInt64()
	if err != nil {
		return 0, err
	}
	if value < math.MinInt32 || value > math.MaxInt32 {
		return 0, ErrOutOfRange
	}
	return int32(value), nil
}

func (c CBOR) TryInt32() (int32, error) {
	return c.TryIntoInt32()
}

func (c CBOR) IntoInt32() (int32, bool) {
	value, err := c.TryIntoInt32()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryIntoInt16() (int16, error) {
	value, err := c.TryIntoInt64()
	if err != nil {
		return 0, err
	}
	if value < math.MinInt16 || value > math.MaxInt16 {
		return 0, ErrOutOfRange
	}
	return int16(value), nil
}

func (c CBOR) TryInt16() (int16, error) {
	return c.TryIntoInt16()
}

func (c CBOR) IntoInt16() (int16, bool) {
	value, err := c.TryIntoInt16()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryIntoUInt32() (uint32, error) {
	value, err := c.TryIntoUInt64()
	if err != nil {
		return 0, err
	}
	if value > math.MaxUint32 {
		return 0, ErrOutOfRange
	}
	return uint32(value), nil
}

func (c CBOR) TryUInt32() (uint32, error) {
	return c.TryIntoUInt32()
}

func (c CBOR) IntoUInt32() (uint32, bool) {
	value, err := c.TryIntoUInt32()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryIntoUInt16() (uint16, error) {
	value, err := c.TryIntoUInt64()
	if err != nil {
		return 0, err
	}
	if value > math.MaxUint16 {
		return 0, ErrOutOfRange
	}
	return uint16(value), nil
}

func (c CBOR) TryUInt16() (uint16, error) {
	return c.TryIntoUInt16()
}

func (c CBOR) IntoUInt16() (uint16, bool) {
	value, err := c.TryIntoUInt16()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryIntoBigUint() (*big.Int, error) {
	switch c.kind {
	case CBORKindUnsigned:
		u := c.value.(uint64)
		return new(big.Int).SetUint64(u), nil
	case CBORKindNegative:
		return nil, ErrOutOfRange
	default:
		return nil, ErrWrongType
	}
}

func (c CBOR) TryBigUint() (*big.Int, error) {
	return c.TryIntoBigUint()
}

func (c CBOR) IntoBigUint() (*big.Int, bool) {
	value, err := c.TryIntoBigUint()
	if err != nil {
		return nil, false
	}
	return value, true
}

func (c CBOR) TryIntoBigInt() (*big.Int, error) {
	switch c.kind {
	case CBORKindUnsigned:
		u := c.value.(uint64)
		return new(big.Int).SetUint64(u), nil
	case CBORKindNegative:
		u := c.value.(uint64)
		magnitude := new(big.Int).SetUint64(u)
		magnitude.Add(magnitude, big.NewInt(1))
		magnitude.Neg(magnitude)
		return magnitude, nil
	default:
		return nil, ErrWrongType
	}
}

func (c CBOR) TryBigInt() (*big.Int, error) {
	return c.TryIntoBigInt()
}

func (c CBOR) IntoBigInt() (*big.Int, bool) {
	value, err := c.TryIntoBigInt()
	if err != nil {
		return nil, false
	}
	return value, true
}

func (c CBOR) TryIntoFloat64() (float64, error) {
	switch c.kind {
	case CBORKindUnsigned:
		u := c.value.(uint64)
		f := float64(u)
		if uint64(f) != u {
			return 0, ErrOutOfRange
		}
		return f, nil
	case CBORKindNegative:
		u := c.value.(uint64)
		f := float64(u)
		if uint64(f) != u {
			return 0, ErrOutOfRange
		}
		return -1.0 - f, nil
	case CBORKindSimple:
		s := c.value.(Simple)
		if s.Kind() != SimpleFloat {
			return 0, ErrWrongType
		}
		f, _ := s.Float64()
		return f, nil
	default:
		return 0, ErrWrongType
	}
}

func (c CBOR) TryIntoFloat32() (float32, error) {
	switch c.kind {
	case CBORKindUnsigned:
		u := c.value.(uint64)
		f := float32(u)
		if uint64(f) != u {
			return 0, ErrOutOfRange
		}
		return f, nil
	case CBORKindNegative:
		u := c.value.(uint64)
		f := float32(u)
		if uint64(f) != u {
			return 0, ErrOutOfRange
		}
		return -1.0 - f, nil
	case CBORKindSimple:
		s := c.value.(Simple)
		if s.Kind() != SimpleFloat {
			return 0, ErrWrongType
		}
		n, _ := s.Float64()
		if math.IsNaN(n) {
			return float32(math.NaN()), nil
		}
		f := float32(n)
		if float64(f) != n {
			return 0, ErrOutOfRange
		}
		return f, nil
	default:
		return 0, ErrWrongType
	}
}

func (c CBOR) TryFloat32() (float32, error) {
	return c.TryIntoFloat32()
}

func (c CBOR) IntoFloat32() (float32, bool) {
	value, err := c.TryIntoFloat32()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) TryFloat64() (float64, error) {
	return c.TryIntoFloat64()
}

func (c CBOR) IntoFloat64() (float64, bool) {
	value, err := c.TryIntoFloat64()
	if err != nil {
		return 0, false
	}
	return value, true
}

func (c CBOR) AsByteString() ([]byte, bool) {
	if c.kind != CBORKindByteString {
		return nil, false
	}
	return c.value.(ByteString).Data(), true
}

func (c CBOR) AsText() (string, bool) {
	if c.kind != CBORKindText {
		return "", false
	}
	return c.value.(string), true
}

func (c CBOR) AsArray() ([]CBOR, bool) {
	if c.kind != CBORKindArray {
		return nil, false
	}
	array := c.value.([]CBOR)
	copied := make([]CBOR, len(array))
	for i, item := range array {
		copied[i] = item.Clone()
	}
	return copied, true
}

func (c CBOR) AsMap() (Map, bool) {
	if c.kind != CBORKindMap {
		return Map{}, false
	}
	return c.value.(Map).Clone(), true
}

func (c CBOR) AsTaggedValue() (Tag, CBOR, bool) {
	if c.kind != CBORKindTagged {
		return Tag{}, CBOR{}, false
	}
	tagged := c.value.(TaggedValue)
	return tagged.Tag.clone(), tagged.Value.Clone(), true
}

func (c CBOR) AsSimpleValue() (Simple, bool) {
	if c.kind != CBORKindSimple {
		return Simple{}, false
	}
	return c.value.(Simple), true
}

func (c CBOR) IsByteString() bool {
	return c.kind == CBORKindByteString
}

func (c CBOR) TryIntoByteString() ([]byte, error) {
	if value, ok := c.AsByteString(); ok {
		return value, nil
	}
	return nil, ErrWrongType
}

func (c CBOR) IntoByteString() ([]byte, bool) {
	value, err := c.TryIntoByteString()
	if err != nil {
		return nil, false
	}
	return value, true
}

func (c CBOR) TryByteString() ([]byte, error) {
	return c.TryIntoByteString()
}

func (c CBOR) IsTaggedValue() bool {
	return c.kind == CBORKindTagged
}

func (c CBOR) TryIntoTaggedValue() (Tag, CBOR, error) {
	tag, value, ok := c.AsTaggedValue()
	if !ok {
		return Tag{}, CBOR{}, ErrWrongType
	}
	return tag, value, nil
}

func (c CBOR) TryTaggedValue() (Tag, CBOR, error) {
	return c.TryIntoTaggedValue()
}

func (c CBOR) TryIntoExpectedTaggedValue(expected Tag) (CBOR, error) {
	tag, value, err := c.TryIntoTaggedValue()
	if err != nil {
		return CBOR{}, err
	}
	if !tag.Equal(expected) {
		return CBOR{}, WrongTagError{Expected: expected, Actual: tag}
	}
	return value, nil
}

func (c CBOR) TryExpectedTaggedValue(expected Tag) (CBOR, error) {
	return c.TryIntoExpectedTaggedValue(expected)
}

func (c CBOR) TryIntoText() (string, error) {
	if value, ok := c.AsText(); ok {
		return value, nil
	}
	return "", ErrWrongType
}

func (c CBOR) IsText() bool {
	return c.kind == CBORKindText
}

func (c CBOR) TryText() (string, error) {
	return c.TryIntoText()
}

func (c CBOR) IntoText() (string, bool) {
	value, err := c.TryIntoText()
	if err != nil {
		return "", false
	}
	return value, true
}

func (c CBOR) TryIntoArray() ([]CBOR, error) {
	if value, ok := c.AsArray(); ok {
		return value, nil
	}
	return nil, ErrWrongType
}

func (c CBOR) IsArray() bool {
	return c.kind == CBORKindArray
}

func (c CBOR) TryArray() ([]CBOR, error) {
	return c.TryIntoArray()
}

func (c CBOR) IntoArray() ([]CBOR, bool) {
	value, err := c.TryIntoArray()
	if err != nil {
		return nil, false
	}
	return value, true
}

func (c CBOR) TryIntoMap() (Map, error) {
	if value, ok := c.AsMap(); ok {
		return value, nil
	}
	return Map{}, ErrWrongType
}

func (c CBOR) IsMap() bool {
	return c.kind == CBORKindMap
}

func (c CBOR) TryMap() (Map, error) {
	return c.TryIntoMap()
}

func (c CBOR) IntoMap() (Map, bool) {
	value, err := c.TryIntoMap()
	if err != nil {
		return Map{}, false
	}
	return value, true
}

func (c CBOR) TryIntoSimpleValue() (Simple, error) {
	if value, ok := c.AsSimpleValue(); ok {
		return value, nil
	}
	return Simple{}, ErrWrongType
}

func (c CBOR) TrySimpleValue() (Simple, error) {
	return c.TryIntoSimpleValue()
}

func (c CBOR) IntoSimpleValue() (Simple, bool) {
	value, err := c.TryIntoSimpleValue()
	if err != nil {
		return Simple{}, false
	}
	return value, true
}

func (c CBOR) IsBool() bool {
	simple, ok := c.AsSimpleValue()
	if !ok {
		return false
	}
	return simple.Kind() == SimpleFalse || simple.Kind() == SimpleTrue
}

func (c CBOR) AsBool() (bool, bool) {
	simple, ok := c.AsSimpleValue()
	if !ok {
		return false, false
	}
	switch simple.Kind() {
	case SimpleFalse:
		return false, true
	case SimpleTrue:
		return true, true
	default:
		return false, false
	}
}

func (c CBOR) TryIntoBool() (bool, error) {
	if value, ok := c.AsBool(); ok {
		return value, nil
	}
	return false, ErrWrongType
}

func (c CBOR) TryBool() (bool, error) {
	return c.TryIntoBool()
}

func (c CBOR) IsTrue() bool {
	value, ok := c.AsBool()
	return ok && value
}

func (c CBOR) IsFalse() bool {
	value, ok := c.AsBool()
	return ok && !value
}

func (c CBOR) IsNull() bool {
	simple, ok := c.AsSimpleValue()
	return ok && simple.Kind() == SimpleNull
}

func (c CBOR) IsNumber() bool {
	if c.kind == CBORKindUnsigned || c.kind == CBORKindNegative {
		return true
	}
	simple, ok := c.AsSimpleValue()
	if !ok {
		return false
	}
	return simple.Kind() == SimpleFloat
}

func (c CBOR) IsNaN() bool {
	simple, ok := c.AsSimpleValue()
	if !ok {
		return false
	}
	return simple.IsNaN()
}

func (c CBOR) String() string {
	return c.displayFlat()
}

func (c CBOR) displayFlat() string {
	switch c.kind {
	case CBORKindUnsigned:
		return fmt.Sprintf("%d", c.value.(uint64))
	case CBORKindNegative:
		return formatNegativeDisplay(c.value.(uint64))
	case CBORKindByteString:
		return fmt.Sprintf("h'%x'", c.value.(ByteString).AsRef())
	case CBORKindText:
		return fmt.Sprintf("%q", c.value.(string))
	case CBORKindArray:
		items := c.value.([]CBOR)
		parts := make([]string, 0, len(items))
		for _, item := range items {
			parts = append(parts, item.displayFlat())
		}
		return fmt.Sprintf("[%s]", strings.Join(parts, ", "))
	case CBORKindMap:
		m := c.value.(Map)
		parts := make([]string, 0, len(m.entries))
		for _, entry := range m.entries {
			parts = append(parts, fmt.Sprintf("%s: %s", entry.key.displayFlat(), entry.value.displayFlat()))
		}
		return fmt.Sprintf("{%s}", strings.Join(parts, ", "))
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		return fmt.Sprintf("%s(%s)", tagged.Tag.String(), tagged.Value.displayFlat())
	case CBORKindSimple:
		return c.value.(Simple).Name()
	default:
		return "<unknown>"
	}
}

func (c CBOR) DebugString() string {
	switch c.kind {
	case CBORKindUnsigned:
		return fmt.Sprintf("unsigned(%d)", c.value.(uint64))
	case CBORKindNegative:
		return fmt.Sprintf("negative(%s)", formatNegativeDisplay(c.value.(uint64)))
	case CBORKindByteString:
		return fmt.Sprintf("bytes(%s)", hex.EncodeToString(c.value.(ByteString).AsRef()))
	case CBORKindText:
		return fmt.Sprintf("text(%q)", c.value.(string))
	case CBORKindArray:
		items := c.value.([]CBOR)
		parts := make([]string, 0, len(items))
		for _, item := range items {
			parts = append(parts, item.DebugString())
		}
		return fmt.Sprintf("array([%s])", strings.Join(parts, ", "))
	case CBORKindMap:
		m := c.value.(Map)
		parts := make([]string, 0, len(m.entries))
		for _, entry := range m.entries {
			parts = append(parts, fmt.Sprintf("0x%s: (%s, %s)", hex.EncodeToString(entry.keyData), entry.key.DebugString(), entry.value.DebugString()))
		}
		return fmt.Sprintf("map({%s})", strings.Join(parts, ", "))
	case CBORKindTagged:
		t := c.value.(TaggedValue)
		return fmt.Sprintf("tagged(%s, %s)", t.Tag.String(), t.Value.DebugString())
	case CBORKindSimple:
		s := c.value.(Simple)
		return fmt.Sprintf("simple(%s)", s.Name())
	default:
		return "<unknown>"
	}
}

func (c CBOR) Diagnostic() string {
	return c.DiagnosticOpt(DiagFormatOpts{})
}

func (c CBOR) DiagnosticAnnotated() string {
	opts := DiagFormatOpts{}
	opts = opts.Annotate(true)
	return c.DiagnosticOpt(opts)
}

func (c CBOR) DiagnosticFlat() string {
	opts := DiagFormatOpts{}
	opts = opts.Flat(true)
	return c.DiagnosticOpt(opts)
}

func (c CBOR) Summary() string {
	opts := DiagFormatOpts{}
	opts = opts.Summarize(true)
	return c.DiagnosticOpt(opts)
}

func (c CBOR) Hex() string {
	return hex.EncodeToString(c.ToCBORData())
}

func (c CBOR) HexAnnotated() string {
	opts := HexFormatOpts{}
	opts = opts.Annotate(true)
	return c.HexOpt(opts)
}

func (c CBOR) ToCBORData() []byte {
	data, err := c.toCBORDataNoPanic()
	if err != nil {
		panic(err)
	}
	return data
}

func (c CBOR) toCBORDataNoPanic() ([]byte, error) {
	switch c.kind {
	case CBORKindUnsigned:
		return encodeHead(majorUnsigned, c.value.(uint64)), nil
	case CBORKindNegative:
		return encodeHead(majorNegative, c.value.(uint64)), nil
	case CBORKindByteString:
		bytesValue := c.value.(ByteString).AsRef()
		buf := encodeHead(majorBytes, uint64(len(bytesValue)))
		buf = append(buf, bytesValue...)
		return buf, nil
	case CBORKindText:
		s := c.value.(string)
		s = norm.NFC.String(s)
		buf := encodeHead(majorText, uint64(len([]byte(s))))
		buf = append(buf, []byte(s)...)
		return buf, nil
	case CBORKindArray:
		items := c.value.([]CBOR)
		buf := encodeHead(majorArray, uint64(len(items)))
		for _, item := range items {
			itemData, err := item.toCBORDataNoPanic()
			if err != nil {
				return nil, err
			}
			buf = append(buf, itemData...)
		}
		return buf, nil
	case CBORKindMap:
		return c.value.(Map).CBORData(), nil
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		header := encodeHead(majorTagged, tagged.Tag.Value())
		body, err := tagged.Value.toCBORDataNoPanic()
		if err != nil {
			return nil, err
		}
		return append(header, body...), nil
	case CBORKindSimple:
		simple := c.value.(Simple)
		return encodeSimple(simple)
	default:
		return nil, Errorf("unsupported CBOR kind: %d", c.kind)
	}
}

func encodeSimple(simple Simple) ([]byte, error) {
	switch simple.Kind() {
	case SimpleFalse:
		return []byte{0xf4}, nil
	case SimpleTrue:
		return []byte{0xf5}, nil
	case SimpleNull:
		return []byte{0xf6}, nil
	case SimpleFloat:
		f, _ := simple.Float64()
		return encodeCanonicalFloat(f)
	default:
		return nil, ErrInvalidSimpleValue
	}
}

func encodeCanonicalFloat(value float64) ([]byte, error) {
	if math.IsNaN(value) {
		return []byte{0xf9, 0x7e, 0x00}, nil
	}

	if reduced, ok := reduceFloatToIntegerCBOR(value); ok {
		return reduced.toCBORDataNoPanic()
	}

	f32 := float32(value)
	if float64(f32) == value {
		h := float32ToHalfBits(f32)
		if halfBitsToFloat32(h) == f32 {
			return []byte{0xf9, byte(h >> 8), byte(h)}, nil
		}
		bits := math.Float32bits(f32)
		buf := make([]byte, 5)
		buf[0] = 0xfa
		binary.BigEndian.PutUint32(buf[1:], bits)
		return buf, nil
	}

	bits := math.Float64bits(value)
	buf := make([]byte, 9)
	buf[0] = 0xfb
	binary.BigEndian.PutUint64(buf[1:], bits)
	return buf, nil
}

func isIntegralFloat(v float64) bool {
	if math.IsNaN(v) || math.IsInf(v, 0) {
		return false
	}
	_, frac := math.Modf(v)
	return frac == 0
}

func (c CBOR) Equal(other CBOR) bool {
	return bytes.Equal(c.ToCBORData(), other.ToCBORData())
}

func TryFromData(data []byte) (CBOR, error) {
	value, n, err := decodeCBORInternal(data)
	if err != nil {
		return CBOR{}, err
	}
	if n != len(data) {
		return CBOR{}, Errorf("the decoded CBOR had %d extra bytes at the end", len(data)-n)
	}
	return value, nil
}

func TryFromHex(value string) (CBOR, error) {
	data, err := hex.DecodeString(value)
	if err != nil {
		return CBOR{}, err
	}
	return TryFromData(data)
}

func decodeCBORInternal(data []byte) (CBOR, int, error) {
	if len(data) == 0 {
		return CBOR{}, 0, ErrUnderrun
	}
	header := data[0]
	major := majorType(header >> 5)
	ai := header & 0x1f

	switch major {
	case majorUnsigned:
		value, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		return NewCBORUnsigned(value), 1 + consumed, nil
	case majorNegative:
		value, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		return NewCBORNegative(value), 1 + consumed, nil
	case majorBytes:
		length, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		need := int(length)
		if len(data) < 1+consumed+need {
			return CBOR{}, 0, ErrUnderrun
		}
		payload := data[1+consumed : 1+consumed+need]
		return NewCBORByteString(NewByteString(payload)), 1 + consumed + need, nil
	case majorText:
		length, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		need := int(length)
		if len(data) < 1+consumed+need {
			return CBOR{}, 0, ErrUnderrun
		}
		payload := data[1+consumed : 1+consumed+need]
		if !utf8.Valid(payload) {
			return CBOR{}, 0, ErrInvalidString
		}
		text := string(payload)
		if !norm.NFC.IsNormalString(text) {
			return CBOR{}, 0, ErrNonCanonicalString
		}
		return NewCBORText(text), 1 + consumed + need, nil
	case majorArray:
		length, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		pos := 1 + consumed
		items := make([]CBOR, 0, int(length))
		for i := uint64(0); i < length; i++ {
			item, n, err := decodeCBORInternal(data[pos:])
			if err != nil {
				return CBOR{}, 0, err
			}
			items = append(items, item)
			pos += n
		}
		return NewCBORArray(items), pos, nil
	case majorMap:
		length, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		pos := 1 + consumed
		m := NewMap()
		for i := uint64(0); i < length; i++ {
			key, kn, err := decodeCBORInternal(data[pos:])
			if err != nil {
				return CBOR{}, 0, err
			}
			pos += kn
			value, vn, err := decodeCBORInternal(data[pos:])
			if err != nil {
				return CBOR{}, 0, err
			}
			pos += vn
			if err := m.insertNext(key, value); err != nil {
				return CBOR{}, 0, err
			}
		}
		return NewCBORMap(m), pos, nil
	case majorTagged:
		tagValue, consumed, err := decodeHeadValue(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		item, itemN, err := decodeCBORInternal(data[1+consumed:])
		if err != nil {
			return CBOR{}, 0, err
		}
		return NewCBORTagged(TagWithValue(tagValue), item), 1 + consumed + itemN, nil
	case majorSimple:
		simple, consumed, err := decodeSimple(ai, data[1:])
		if err != nil {
			return CBOR{}, 0, err
		}
		return NewCBORSimple(simple), 1 + consumed, nil
	default:
		return CBOR{}, 0, ErrUnsupportedHeader
	}
}

func decodeSimple(ai byte, data []byte) (Simple, int, error) {
	switch ai {
	case 20:
		return SimpleFalseValue(), 0, nil
	case 21:
		return SimpleTrueValue(), 0, nil
	case 22:
		return SimpleNullValue(), 0, nil
	case 25:
		if len(data) < 2 {
			return Simple{}, 0, ErrUnderrun
		}
		bits := binary.BigEndian.Uint16(data[:2])
		if (bits&0x7c00) == 0x7c00 && (bits&0x03ff) != 0 {
			if bits != 0x7e00 {
				return Simple{}, 0, ErrNonCanonicalNumeric
			}
		}
		value := float64(halfBitsToFloat32(bits))
		if _, ok := reduceFloatToIntegerCBOR(value); ok {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		return SimpleFloatValue(value), 2, nil
	case 26:
		if len(data) < 4 {
			return Simple{}, 0, ErrUnderrun
		}
		bits := binary.BigEndian.Uint32(data[:4])
		value32 := math.Float32frombits(bits)
		if math.IsNaN(float64(value32)) {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		if halfBitsToFloat32(float32ToHalfBits(value32)) == value32 {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		value := float64(value32)
		if _, ok := reduceFloatToIntegerCBOR(value); ok {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		return SimpleFloatValue(value), 4, nil
	case 27:
		if len(data) < 8 {
			return Simple{}, 0, ErrUnderrun
		}
		bits := binary.BigEndian.Uint64(data[:8])
		value := math.Float64frombits(bits)
		if math.IsNaN(value) {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		if float64(float32(value)) == value {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		if _, ok := reduceFloatToIntegerCBOR(value); ok {
			return Simple{}, 0, ErrNonCanonicalNumeric
		}
		return SimpleFloatValue(value), 8, nil
	default:
		return Simple{}, 0, ErrInvalidSimpleValue
	}
}

func decodeHeadValue(ai byte, data []byte) (uint64, int, error) {
	switch {
	case ai <= 23:
		return uint64(ai), 0, nil
	case ai == 24:
		if len(data) < 1 {
			return 0, 0, ErrUnderrun
		}
		value := uint64(data[0])
		if value < 24 {
			return 0, 0, ErrNonCanonicalNumeric
		}
		return value, 1, nil
	case ai == 25:
		if len(data) < 2 {
			return 0, 0, ErrUnderrun
		}
		value := uint64(binary.BigEndian.Uint16(data[:2]))
		if value <= math.MaxUint8 {
			return 0, 0, ErrNonCanonicalNumeric
		}
		return value, 2, nil
	case ai == 26:
		if len(data) < 4 {
			return 0, 0, ErrUnderrun
		}
		value := uint64(binary.BigEndian.Uint32(data[:4]))
		if value <= math.MaxUint16 {
			return 0, 0, ErrNonCanonicalNumeric
		}
		return value, 4, nil
	case ai == 27:
		if len(data) < 8 {
			return 0, 0, ErrUnderrun
		}
		value := binary.BigEndian.Uint64(data[:8])
		if value <= math.MaxUint32 {
			return 0, 0, ErrNonCanonicalNumeric
		}
		return value, 8, nil
	default:
		return 0, 0, ErrUnsupportedHeader
	}
}

func encodeHead(major majorType, value uint64) []byte {
	head := byte(major) << 5
	switch {
	case value <= 23:
		return []byte{head | byte(value)}
	case value <= math.MaxUint8:
		return []byte{head | 24, byte(value)}
	case value <= math.MaxUint16:
		return []byte{head | 25, byte(value >> 8), byte(value)}
	case value <= math.MaxUint32:
		buf := []byte{head | 26, 0, 0, 0, 0}
		binary.BigEndian.PutUint32(buf[1:], uint32(value))
		return buf
	default:
		buf := []byte{head | 27, 0, 0, 0, 0, 0, 0, 0, 0}
		binary.BigEndian.PutUint64(buf[1:], value)
		return buf
	}
}

func formatNegativeDisplay(encodedMagnitude uint64) string {
	n := new(big.Int).SetUint64(encodedMagnitude)
	n.Add(n, big.NewInt(1))
	n.Neg(n)
	return n.String()
}

func formatFloatDiagnostic(v float64) string {
	if math.IsNaN(v) {
		return "NaN"
	}
	if math.IsInf(v, 1) {
		return "Infinity"
	}
	if math.IsInf(v, -1) {
		return "-Infinity"
	}
	text := strconv.FormatFloat(v, 'g', -1, 64)
	if strings.ContainsAny(text, "eE") {
		abs := math.Abs(v)
		if abs >= 1e-4 && abs < 1e15 {
			text = strconv.FormatFloat(v, 'f', -1, 64)
		}
	}
	return normalizeExponent(strings.ReplaceAll(strings.ToLower(text), "e+", "e"))
}

func normalizeExponent(text string) string {
	index := strings.IndexByte(text, 'e')
	if index < 0 || index+1 >= len(text) {
		return text
	}
	sign := byte(0)
	start := index + 1
	if text[start] == '+' || text[start] == '-' {
		sign = text[start]
		start++
	}
	if start >= len(text) {
		return text
	}
	exponent := strings.TrimLeft(text[start:], "0")
	if exponent == "" {
		exponent = "0"
	}
	if sign == 0 {
		return text[:index+1] + exponent
	}
	return text[:index+1] + string(sign) + exponent
}

// ToNative returns a generic representation suitable for JSON-like handling.
func (c CBOR) ToNative() any {
	switch c.kind {
	case CBORKindUnsigned:
		return c.value.(uint64)
	case CBORKindNegative:
		if n, ok := c.AsInt64(); ok {
			return n
		}
		return formatNegativeDisplay(c.value.(uint64))
	case CBORKindByteString:
		return c.value.(ByteString).Data()
	case CBORKindText:
		return c.value.(string)
	case CBORKindArray:
		items := c.value.([]CBOR)
		out := make([]any, 0, len(items))
		for _, item := range items {
			out = append(out, item.ToNative())
		}
		return out
	case CBORKindMap:
		iter := c.value.(Map).Iter()
		out := make(map[string]any)
		for {
			k, v, ok := iter.Next()
			if !ok {
				break
			}
			out[k.DiagnosticFlat()] = v.ToNative()
		}
		return out
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		return map[string]any{"tag": tagged.Tag.Value(), "value": tagged.Value.ToNative()}
	case CBORKindSimple:
		s := c.value.(Simple)
		switch s.Kind() {
		case SimpleFalse:
			return false
		case SimpleTrue:
			return true
		case SimpleNull:
			return nil
		default:
			f, _ := s.Float64()
			return f
		}
	default:
		return nil
	}
}

// NormalizeViaFxamacker is a helper for interoperability checks while this
// translation is in progress.
func NormalizeViaFxamacker(data []byte) ([]byte, error) {
	var v any
	if err := cbor.Unmarshal(data, &v); err != nil {
		return nil, err
	}
	encMode, err := cbor.CoreDetEncOptions().EncMode()
	if err != nil {
		return nil, err
	}
	return encMode.Marshal(v)
}

func (c CBOR) MustEqual(other CBOR) {
	if !c.Equal(other) {
		panic(fmt.Sprintf("CBOR values differ\nactual:   %s\nexpected: %s", c.DiagnosticFlat(), other.DiagnosticFlat()))
	}
}
