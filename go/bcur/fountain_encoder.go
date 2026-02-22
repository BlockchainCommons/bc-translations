package bcur

// fountainEncoder produces fountain-encoded parts from a message.
type fountainEncoder struct {
	parts           [][]byte
	messageLength   int
	checksum        uint32
	currentSequence int
}

// newFountainEncoder creates a new fountain encoder.
func newFountainEncoder(message []byte, maxFragmentLength int) (*fountainEncoder, error) {
	if len(message) == 0 {
		return nil, ErrEmptyMessage
	}
	if maxFragmentLength == 0 {
		return nil, ErrInvalidFragmentLen
	}
	fragLen := fragmentLength(len(message), maxFragmentLength)
	fragments := partition(message, fragLen)
	return &fountainEncoder{
		parts:         fragments,
		messageLength: len(message),
		checksum:      crc32Checksum(message),
	}, nil
}

// nextPart returns the next fountain part.
func (e *fountainEncoder) nextPart() *fountainPart {
	e.currentSequence++
	indexes := chooseFragments(e.currentSequence, len(e.parts), e.checksum)

	mixed := make([]byte, len(e.parts[0]))
	for _, idx := range indexes {
		xorBytes(mixed, e.parts[idx])
	}

	return &fountainPart{
		sequence:      e.currentSequence,
		sequenceCount: len(e.parts),
		messageLength: e.messageLength,
		checksum:      e.checksum,
		data:          mixed,
	}
}

// fragmentCount returns the number of fragments.
func (e *fountainEncoder) fragmentCount() int {
	return len(e.parts)
}

// complete returns true if all original parts have been emitted.
func (e *fountainEncoder) complete() bool {
	return e.currentSequence >= len(e.parts)
}
