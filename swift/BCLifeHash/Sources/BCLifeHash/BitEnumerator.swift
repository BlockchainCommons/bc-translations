final class BitEnumerator {
    private let data: [UInt8]
    private var index: Int
    private var mask: UInt8

    init(data: [UInt8]) {
        self.data = data
        self.index = 0
        self.mask = 0x80
    }

    var hasNext: Bool {
        guard !data.isEmpty else { return false }
        return mask != 0 || index != data.count - 1
    }

    func next() -> Bool {
        precondition(hasNext, "BitEnumerator underflow")

        if mask == 0 {
            mask = 0x80
            index += 1
        }

        let b = (data[index] & mask) != 0
        mask >>= 1
        return b
    }

    func nextUInt2() -> UInt32 {
        var bitMask: UInt32 = 0x02
        var value: UInt32 = 0
        for _ in 0..<2 {
            if next() {
                value |= bitMask
            }
            bitMask >>= 1
        }
        return value
    }

    func nextUInt8() -> UInt32 {
        var bitMask: UInt32 = 0x80
        var value: UInt32 = 0
        for _ in 0..<8 {
            if next() {
                value |= bitMask
            }
            bitMask >>= 1
        }
        return value
    }

    func nextUInt16() -> UInt32 {
        var bitMask: UInt32 = 0x8000
        var value: UInt32 = 0
        for _ in 0..<16 {
            if next() {
                value |= bitMask
            }
            bitMask >>= 1
        }
        return value
    }

    func nextFrac() -> Double {
        Double(nextUInt16()) / 65535.0
    }

    func forAll(_ f: (Bool) -> Void) {
        while hasNext {
            f(next())
        }
    }
}

final class BitAggregator {
    private var data: [UInt8] = []
    private var bitMask: UInt8 = 0

    func append(_ bit: Bool) {
        if bitMask == 0 {
            bitMask = 0x80
            data.append(0)
        }

        if bit {
            let last = data.count - 1
            data[last] |= bitMask
        }

        bitMask >>= 1
    }

    func valueData() -> [UInt8] {
        data
    }
}
