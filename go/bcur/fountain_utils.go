package bcur

import "hash/crc32"

// divCeil returns the ceiling division of a by b.
func divCeil(a, b int) int {
	d := a / b
	if a%b > 0 {
		d++
	}
	return d
}

// fragmentLength calculates the effective fragment length.
func fragmentLength(dataLength, maxFragmentLength int) int {
	fragmentCount := divCeil(dataLength, maxFragmentLength)
	return divCeil(dataLength, fragmentCount)
}

// partition splits data into equal-length fragments, zero-padding the last.
func partition(data []byte, fragLength int) [][]byte {
	padLen := (fragLength - (len(data) % fragLength)) % fragLength
	padded := make([]byte, len(data)+padLen)
	copy(padded, data)

	count := len(padded) / fragLength
	result := make([][]byte, count)
	for i := 0; i < count; i++ {
		frag := make([]byte, fragLength)
		copy(frag, padded[i*fragLength:(i+1)*fragLength])
		result[i] = frag
	}
	return result
}

// chooseFragments deterministically selects fragment indices for a given sequence number.
func chooseFragments(sequence, fragmentCount int, checksum uint32) []int {
	if sequence <= fragmentCount {
		return []int{sequence - 1}
	}

	var seed [8]byte
	seed[0] = byte(sequence >> 24)
	seed[1] = byte(sequence >> 16)
	seed[2] = byte(sequence >> 8)
	seed[3] = byte(sequence)
	seed[4] = byte(checksum >> 24)
	seed[5] = byte(checksum >> 16)
	seed[6] = byte(checksum >> 8)
	seed[7] = byte(checksum)

	rng := newXoshiro256FromBytes(seed[:])
	degree := rng.chooseDegree(fragmentCount)

	indexes := make([]int, fragmentCount)
	for i := 0; i < fragmentCount; i++ {
		indexes[i] = i
	}
	shuffled := rng.shuffled(indexes)
	if degree < len(shuffled) {
		shuffled = shuffled[:degree]
	}
	return shuffled
}

// xorBytes XORs v2 into v1 in-place. They must be the same length.
func xorBytes(v1 []byte, v2 []byte) {
	for i := range v1 {
		v1[i] ^= v2[i]
	}
}

// crc32Checksum returns the CRC-32/ISO-HDLC checksum of data.
func crc32Checksum(data []byte) uint32 {
	return crc32.ChecksumIEEE(data)
}
