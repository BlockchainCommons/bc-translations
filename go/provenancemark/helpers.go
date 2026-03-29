package provenancemark

import "bytes"

func cloneBytes(data []byte) []byte {
	if len(data) == 0 {
		return []byte{}
	}
	cloned := make([]byte, len(data))
	copy(cloned, data)
	return cloned
}

func sliceByteRange(data []byte, span ByteRange) []byte {
	start := span.Start
	if start < 0 {
		start = 0
	}
	end := span.End
	if end < 0 || end > len(data) {
		end = len(data)
	}
	if start > end {
		start = end
	}
	return cloneBytes(data[start:end])
}

func cloneMarks(marks []ProvenanceMark) []ProvenanceMark {
	if len(marks) == 0 {
		return []ProvenanceMark{}
	}
	cloned := make([]ProvenanceMark, len(marks))
	copy(cloned, marks)
	return cloned
}

func compareBytes(a, b []byte) int {
	return bytes.Compare(a, b)
}
