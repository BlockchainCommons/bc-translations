package bcur

import "slices"

// indexKey creates a comparable string key from a sorted index slice.
func indexKey(indexes []int) string {
	sorted := make([]int, len(indexes))
	copy(sorted, indexes)
	slices.Sort(sorted)

	result := make([]byte, len(sorted)*4)
	for i, v := range sorted {
		result[i*4] = byte(v >> 24)
		result[i*4+1] = byte(v >> 16)
		result[i*4+2] = byte(v >> 8)
		result[i*4+3] = byte(v)
	}
	return string(result)
}

// fountainDecoder reconstructs messages from fountain-encoded parts.
type fountainDecoder struct {
	decoded        map[int]*fountainPart
	received       map[string]bool
	buffer         map[string]*fountainPart
	bufferIndexes  map[string][]int // store the actual index slices for buffer entries
	queue          []queueEntry
	sequenceCount  int
	messageLength  int
	checksum       uint32
	fragmentLength int
}

type queueEntry struct {
	index int
	part  *fountainPart
}

func newFountainDecoder() *fountainDecoder {
	return &fountainDecoder{
		decoded:       make(map[int]*fountainPart),
		received:      make(map[string]bool),
		buffer:        make(map[string]*fountainPart),
		bufferIndexes: make(map[string][]int),
	}
}

// receive processes a fountain part. Returns true if the part was useful.
func (d *fountainDecoder) receive(part *fountainPart) (bool, error) {
	if d.complete() {
		return false, nil
	}

	if part.sequenceCount == 0 || len(part.data) == 0 || part.messageLength == 0 {
		return false, ErrEmptyPart
	}

	if len(d.received) == 0 {
		d.sequenceCount = part.sequenceCount
		d.messageLength = part.messageLength
		d.checksum = part.checksum
		d.fragmentLength = len(part.data)
	} else if !d.validate(part) {
		return false, ErrInconsistentPart
	}

	indexes := part.indexes()
	key := indexKey(indexes)
	if d.received[key] {
		return false, nil
	}
	d.received[key] = true

	if part.isSimple() {
		if err := d.processSimple(part); err != nil {
			return false, err
		}
	} else {
		d.processComplex(part)
	}
	return true, nil
}

func (d *fountainDecoder) processSimple(part *fountainPart) error {
	indexes := part.indexes()
	index := indexes[0]
	d.decoded[index] = part.clone()
	d.queue = append(d.queue, queueEntry{index: index, part: part.clone()})
	return d.processQueue()
}

func (d *fountainDecoder) processQueue() error {
	for len(d.queue) > 0 {
		// Pop from the end (like Rust's Vec::pop)
		entry := d.queue[len(d.queue)-1]
		d.queue = d.queue[:len(d.queue)-1]

		// Find all buffer entries containing this index
		var toProcess []string
		for key, idxs := range d.bufferIndexes {
			for _, idx := range idxs {
				if idx == entry.index {
					toProcess = append(toProcess, key)
					break
				}
			}
		}

		for _, key := range toProcess {
			part := d.buffer[key]
			indexes := d.bufferIndexes[key]
			delete(d.buffer, key)
			delete(d.bufferIndexes, key)

			// Remove the decoded index
			newIndexes := make([]int, 0, len(indexes)-1)
			for _, idx := range indexes {
				if idx != entry.index {
					newIndexes = append(newIndexes, idx)
				}
			}

			xorBytes(part.data, entry.part.data)

			if len(newIndexes) == 1 {
				d.decoded[newIndexes[0]] = part.clone()
				d.queue = append(d.queue, queueEntry{index: newIndexes[0], part: part.clone()})
			} else {
				newKey := indexKey(newIndexes)
				d.buffer[newKey] = part
				d.bufferIndexes[newKey] = newIndexes
			}
		}
	}
	return nil
}

func (d *fountainDecoder) processComplex(part *fountainPart) {
	indexes := part.indexes()
	p := part.clone()

	// Remove already-decoded indexes
	var toRemove []int
	for _, idx := range indexes {
		if _, ok := d.decoded[idx]; ok {
			toRemove = append(toRemove, idx)
		}
	}

	if len(indexes) == len(toRemove) {
		return // All fragments already decoded
	}

	newIndexes := make([]int, 0, len(indexes)-len(toRemove))
	for _, idx := range indexes {
		found := false
		for _, rem := range toRemove {
			if idx == rem {
				found = true
				break
			}
		}
		if !found {
			newIndexes = append(newIndexes, idx)
		}
	}

	for _, rem := range toRemove {
		xorBytes(p.data, d.decoded[rem].data)
	}

	if len(newIndexes) == 1 {
		d.decoded[newIndexes[0]] = p.clone()
		d.queue = append(d.queue, queueEntry{index: newIndexes[0], part: p.clone()})
	} else {
		key := indexKey(newIndexes)
		d.buffer[key] = p
		d.bufferIndexes[key] = newIndexes
	}
}

// complete returns true if all fragments have been decoded.
func (d *fountainDecoder) complete() bool {
	return d.messageLength != 0 && len(d.decoded) == d.sequenceCount
}

// validate checks if a part is consistent with previously received parts.
func (d *fountainDecoder) validate(part *fountainPart) bool {
	if len(d.received) == 0 {
		return false
	}
	return part.sequenceCount == d.sequenceCount &&
		part.messageLength == d.messageLength &&
		part.checksum == d.checksum &&
		len(part.data) == d.fragmentLength
}

// message returns the decoded message if complete.
func (d *fountainDecoder) message() ([]byte, error) {
	if !d.complete() {
		return nil, nil
	}

	combined := make([]byte, 0, d.sequenceCount*d.fragmentLength)
	for i := 0; i < d.sequenceCount; i++ {
		part, ok := d.decoded[i]
		if !ok {
			return nil, ErrEmptyPart
		}
		combined = append(combined, part.data...)
	}

	// Verify padding is all zeros
	for i := d.messageLength; i < len(combined); i++ {
		if combined[i] != 0 {
			return nil, ErrInvalidPadding
		}
	}

	return combined[:d.messageLength], nil
}
