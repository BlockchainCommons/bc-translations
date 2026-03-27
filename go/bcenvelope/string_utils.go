package bcenvelope

import "fmt"

// FlankedBy returns s enclosed by the given left and right delimiter strings.
func FlankedBy(s, left, right string) string {
	return fmt.Sprintf("%s%s%s", left, s, right)
}
