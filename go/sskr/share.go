package sskr

// sskrShare is the internal deserialized representation of an SSKR share.
type sskrShare struct {
	identifier      uint16
	groupIndex      int
	groupThreshold  int
	groupCount      int
	memberIndex     int
	memberThreshold int
	value           Secret
}

func newSSKRShare(
	identifier uint16,
	groupIndex int,
	groupThreshold int,
	groupCount int,
	memberIndex int,
	memberThreshold int,
	value Secret,
) sskrShare {
	return sskrShare{
		identifier:      identifier,
		groupIndex:      groupIndex,
		groupThreshold:  groupThreshold,
		groupCount:      groupCount,
		memberIndex:     memberIndex,
		memberThreshold: memberThreshold,
		value:           value,
	}
}
