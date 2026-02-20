package bclifehash

// bitEnumerator reads bits from a byte slice one at a time, MSB first.
type bitEnumerator struct {
	data  []byte
	index int
	mask  byte
}

func newBitEnumerator(data []byte) bitEnumerator {
	return bitEnumerator{
		data:  data,
		index: 0,
		mask:  0x80,
	}
}

func (e *bitEnumerator) hasNext() bool {
	return e.mask != 0 || e.index != len(e.data)-1
}

func (e *bitEnumerator) next() bool {
	if !e.hasNext() {
		panic("bitEnumerator underflow")
	}

	if e.mask == 0 {
		e.mask = 0x80
		e.index++
	}

	b := (e.data[e.index] & e.mask) != 0
	e.mask >>= 1
	return b
}

func (e *bitEnumerator) nextUint2() uint32 {
	var bitMask uint32 = 0x02
	var value uint32
	for i := 0; i < 2; i++ {
		if e.next() {
			value |= bitMask
		}
		bitMask >>= 1
	}
	return value
}

func (e *bitEnumerator) nextUint8() uint32 {
	var bitMask uint32 = 0x80
	var value uint32
	for i := 0; i < 8; i++ {
		if e.next() {
			value |= bitMask
		}
		bitMask >>= 1
	}
	return value
}

func (e *bitEnumerator) nextUint16() uint32 {
	var bitMask uint32 = 0x8000
	var value uint32
	for i := 0; i < 16; i++ {
		if e.next() {
			value |= bitMask
		}
		bitMask >>= 1
	}
	return value
}

func (e *bitEnumerator) nextFrac() float64 {
	return float64(e.nextUint16()) / 65535.0
}

func (e *bitEnumerator) forAll(f func(bool)) {
	for e.hasNext() {
		f(e.next())
	}
}

// bitAggregator collects bits into bytes, MSB first.
type bitAggregator struct {
	data    []byte
	bitMask byte
}

func newBitAggregator() bitAggregator {
	return bitAggregator{}
}

func (a *bitAggregator) append(bit bool) {
	if a.bitMask == 0 {
		a.bitMask = 0x80
		a.data = append(a.data, 0)
	}

	if bit {
		a.data[len(a.data)-1] |= a.bitMask
	}

	a.bitMask >>= 1
}

func (a *bitAggregator) bytes() []byte {
	result := make([]byte, len(a.data))
	copy(result, a.data)
	return result
}
