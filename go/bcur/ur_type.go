package bcur

// URType represents a validated UR type string.
type URType struct {
	value string
}

// NewURType creates a new URType from the given string.
// Returns an error if the string contains invalid characters.
func NewURType(urType string) (URType, error) {
	if !isValidURType(urType) {
		return URType{}, ErrInvalidType
	}
	return URType{value: urType}, nil
}

// String returns the string representation of the URType.
func (t URType) String() string {
	return t.value
}
