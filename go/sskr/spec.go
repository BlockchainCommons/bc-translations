package sskr

import (
	"fmt"
	"strconv"
	"strings"
)

// Spec is a specification for an SSKR split.
type Spec struct {
	groupThreshold int
	groups         []GroupSpec
}

// NewSpec creates a new Spec with the given group threshold and groups.
func NewSpec(groupThreshold int, groups []GroupSpec) (Spec, error) {
	if groupThreshold == 0 {
		return Spec{}, ErrGroupThresholdInvalid
	}
	if groupThreshold > len(groups) {
		return Spec{}, ErrGroupThresholdInvalid
	}
	if len(groups) > MaxShareCount {
		return Spec{}, ErrGroupCountInvalid
	}
	copied := append([]GroupSpec(nil), groups...)
	return Spec{groupThreshold: groupThreshold, groups: copied}, nil
}

// GroupThreshold returns the number of groups required for reconstruction.
func (s Spec) GroupThreshold() int {
	return s.groupThreshold
}

// Groups returns a copy of the group specifications.
func (s Spec) Groups() []GroupSpec {
	return append([]GroupSpec(nil), s.groups...)
}

// GroupCount returns the number of groups in the spec.
func (s Spec) GroupCount() int {
	return len(s.groups)
}

// ShareCount returns the total number of member shares across all groups.
func (s Spec) ShareCount() int {
	total := 0
	for _, group := range s.groups {
		total += group.MemberCount()
	}
	return total
}

// GroupSpec is a specification for one group of shares.
type GroupSpec struct {
	memberThreshold int
	memberCount     int
}

// NewGroupSpec creates a new GroupSpec with the given member threshold and count.
func NewGroupSpec(memberThreshold, memberCount int) (GroupSpec, error) {
	if memberCount == 0 {
		return GroupSpec{}, ErrMemberCountInvalid
	}
	if memberCount > MaxShareCount {
		return GroupSpec{}, ErrMemberCountInvalid
	}
	if memberThreshold > memberCount {
		return GroupSpec{}, ErrMemberThresholdInvalid
	}
	return GroupSpec{memberThreshold: memberThreshold, memberCount: memberCount}, nil
}

// MemberThreshold returns the member threshold for the group.
func (g GroupSpec) MemberThreshold() int {
	return g.memberThreshold
}

// MemberCount returns the number of members in the group.
func (g GroupSpec) MemberCount() int {
	return g.memberCount
}

// ParseGroupSpec parses a string in the form "<memberThreshold>-of-<memberCount>".
func ParseGroupSpec(s string) (GroupSpec, error) {
	parts := strings.Split(s, "-")
	if len(parts) != 3 {
		return GroupSpec{}, ErrGroupSpecInvalid
	}
	memberThreshold, err := strconv.Atoi(parts[0])
	if err != nil {
		return GroupSpec{}, ErrGroupSpecInvalid
	}
	if parts[1] != "of" {
		return GroupSpec{}, ErrGroupSpecInvalid
	}
	memberCount, err := strconv.Atoi(parts[2])
	if err != nil {
		return GroupSpec{}, ErrGroupSpecInvalid
	}
	return NewGroupSpec(memberThreshold, memberCount)
}

// DefaultGroupSpec returns the default 1-of-1 group spec.
func DefaultGroupSpec() GroupSpec {
	group, err := NewGroupSpec(1, 1)
	if err != nil {
		panic(err)
	}
	return group
}

// String renders the group spec as "<threshold>-of-<count>".
func (g GroupSpec) String() string {
	return fmt.Sprintf("%d-of-%d", g.memberThreshold, g.memberCount)
}
