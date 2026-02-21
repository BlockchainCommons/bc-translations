package dcbor

import "fmt"

// TagValue is the numeric value for a CBOR tag.
type TagValue = uint64

// Tag represents a CBOR tag with an optional assigned name.
type Tag struct {
	value TagValue
	name  *string
}

func NewTag(value TagValue, name string) Tag {
	nameCopy := name
	return Tag{value: value, name: &nameCopy}
}

func TagWithValue(value TagValue) Tag {
	return Tag{value: value}
}

func TagWithStaticName(value TagValue, name string) Tag {
	return NewTag(value, name)
}

func (t Tag) Value() TagValue {
	return t.value
}

func (t Tag) Name() (string, bool) {
	if t.name == nil {
		return "", false
	}
	return *t.name, true
}

func (t Tag) Equal(other Tag) bool {
	return t.value == other.value
}

func (t Tag) String() string {
	if t.name == nil {
		return fmt.Sprintf("%d", t.value)
	}
	return *t.name
}

func (t Tag) clone() Tag {
	if t.name == nil {
		return Tag{value: t.value}
	}
	nameCopy := *t.name
	return Tag{value: t.value, name: &nameCopy}
}
