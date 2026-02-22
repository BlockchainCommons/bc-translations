/// Internal fountain encoder that emits an unbounded stream of parts.
internal struct FountainEncoder: Sendable {
    private let parts: [[UInt8]]
    private let messageLength: Int
    private let checksum: UInt32
    private(set) var currentSequence: Int = 0

    init(message: [UInt8], maxFragmentLength: Int) throws {
        guard !message.isEmpty else {
            throw FountainError.emptyMessage
        }
        guard maxFragmentLength > 0 else {
            throw FountainError.invalidFragmentLen
        }

        let fragmentLength = FountainUtils.fragmentLength(
            dataLength: message.count,
            maxFragmentLength: maxFragmentLength
        )

        self.parts = FountainUtils.partition(message, fragmentLength: fragmentLength)
        self.messageLength = message.count
        self.checksum = Crc32.checksum(message)
    }

    var fragmentCount: Int {
        parts.count
    }

    var complete: Bool {
        currentSequence >= parts.count
    }

    mutating func nextPart() -> FountainPart {
        currentSequence += 1
        let indexes = FountainUtils.chooseFragments(
            sequence: currentSequence,
            fragmentCount: parts.count,
            checksum: checksum
        )

        var mixed = Array(repeating: UInt8(0), count: parts[0].count)
        for index in indexes {
            FountainUtils.xorInPlace(&mixed, with: parts[index])
        }

        return FountainPart(
            sequence: currentSequence,
            sequenceCount: parts.count,
            messageLength: messageLength,
            checksum: checksum,
            data: mixed
        )
    }
}
