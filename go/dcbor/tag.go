package dcbor

import "fmt"

// TagValue is the numeric value for a CBOR tag.
type TagValue = uint64

// Tag represents a CBOR tag with an optional assigned name.
type Tag struct {
	value TagValue
	name  *string
}

// NewTag constructs a tag with an explicit display name.
func NewTag(value TagValue, name string) Tag {
	nameCopy := name
	return Tag{value: value, name: &nameCopy}
}

// TagWithValue constructs an unnamed tag.
func TagWithValue(value TagValue) Tag {
	return Tag{value: value}
}

// TagWithStaticName constructs a named tag.
func TagWithStaticName(value TagValue, name string) Tag {
	return NewTag(value, name)
}

// Value returns the numeric tag value.
func (t Tag) Value() TagValue {
	return t.value
}

// Name returns the tag name, if assigned.
func (t Tag) Name() (string, bool) {
	if t.name == nil {
		return "", false
	}
	return *t.name, true
}

// Equal reports numeric equality of tags.
func (t Tag) Equal(other Tag) bool {
	return t.value == other.value
}

// String returns the name when available, otherwise numeric value text.
func (t Tag) String() string {
	if t.name == nil {
		return fmt.Sprintf("%d", t.value)
	}
	return *t.name
}

// Clone returns a deep copy of the tag.
func (t Tag) Clone() Tag {
	if t.name == nil {
		return Tag{value: t.value}
	}
	nameCopy := *t.name
	return Tag{value: t.value, name: &nameCopy}
}
