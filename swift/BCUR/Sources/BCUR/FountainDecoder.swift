/// Internal fountain decoder that reconstructs a message from fountain parts.
internal struct FountainDecoder: Sendable {
    private var decoded: [Int: FountainPart] = [:]
    private var received: Swift.Set<[Int]> = []
    private var buffer: [[Int]: FountainPart] = [:]
    private var queue: [(Int, FountainPart)] = []

    private var sequenceCount: Int = 0
    private var messageLength: Int = 0
    private var checksum: UInt32 = 0
    private var fragmentLength: Int = 0

    var complete: Bool {
        messageLength != 0 && decoded.count == sequenceCount
    }

    /// Number of fragments that have been fully decoded.
    var decodedCount: Int { decoded.count }

    /// Total number of fragments needed. Returns 0 before the first part is received.
    var expectedCount: Int { sequenceCount }

    /// Set of fragment indexes that have been fully decoded.
    var decodedIndexes: Swift.Set<Int> { Swift.Set(decoded.keys) }

    /// Partial progress credit from buffered mixed-degree parts.
    ///
    /// Each buffered part with reduced degree d contributes 1/d,
    /// reflecting that it will deliver one full decoded fragment
    /// once d-1 of its unknowns are resolved from other sources.
    var bufferContribution: Double {
        buffer.keys.reduce(0.0) { sum, indexes in sum + 1.0 / Double(indexes.count) }
    }

    func validate(_ part: FountainPart) -> Bool {
        if received.isEmpty {
            return false
        }

        if part.sequenceCount != sequenceCount {
            return false
        }
        if part.messageLength != messageLength {
            return false
        }
        if part.checksum != checksum {
            return false
        }
        if part.data.count != fragmentLength {
            return false
        }

        return true
    }

    mutating func receive(_ part: FountainPart) throws -> Bool {
        if complete {
            return false
        }

        if part.sequenceCount == 0 || part.data.isEmpty || part.messageLength == 0 {
            throw FountainError.emptyPart
        }

        if received.isEmpty {
            sequenceCount = part.sequenceCount
            messageLength = part.messageLength
            checksum = part.checksum
            fragmentLength = part.data.count
        } else if !validate(part) {
            throw FountainError.inconsistentPart
        }

        let indexes = part.indexes
        if received.contains(indexes) {
            return false
        }

        received.insert(indexes)

        if part.isSimple {
            try processSimple(part)
        } else {
            try processComplex(part)
        }

        return true
    }

    mutating func message() throws -> [UInt8]? {
        if !complete {
            return nil
        }

        var combined: [UInt8] = []
        combined.reserveCapacity(sequenceCount * fragmentLength)

        for index in 0..<sequenceCount {
            guard let part = decoded[index] else {
                throw FountainError.expectedItem
            }
            combined.append(contentsOf: part.data)
        }

        guard messageLength <= combined.count else {
            throw FountainError.expectedItem
        }

        if messageLength < combined.count {
            for byte in combined[messageLength...] {
                if byte != 0 {
                    throw FountainError.invalidPadding
                }
            }
        }

        return Array(combined[..<messageLength])
    }

    private mutating func processSimple(_ part: FountainPart) throws {
        guard let index = part.indexes.first else {
            throw FountainError.expectedItem
        }

        decoded[index] = part
        queue.append((index, part))
        try processQueue()
    }

    private mutating func processQueue() throws {
        while !queue.isEmpty {
            let (index, simple) = queue.removeLast()

            let toProcess = buffer.keys.filter { indexes in
                indexes.contains(index)
            }

            for indexes in toProcess {
                guard var part = buffer.removeValue(forKey: indexes) else {
                    throw FountainError.expectedItem
                }

                var newIndexes = indexes
                guard let removeIndex = newIndexes.firstIndex(of: index) else {
                    throw FountainError.expectedItem
                }
                newIndexes.remove(at: removeIndex)

                FountainUtils.xorInPlace(&part.data, with: simple.data)

                if newIndexes.count == 1 {
                    guard let newIndex = newIndexes.first else {
                        throw FountainError.expectedItem
                    }
                    decoded[newIndex] = part
                    queue.append((newIndex, part))
                } else {
                    buffer[newIndexes] = part
                }
            }
        }
    }

    private mutating func processComplex(_ inputPart: FountainPart) throws {
        var part = inputPart
        var indexes = part.indexes

        let toRemove = indexes.filter { decoded[$0] != nil }
        if indexes.count == toRemove.count {
            return
        }

        for remove in toRemove {
            guard let removeIndex = indexes.firstIndex(of: remove) else {
                throw FountainError.expectedItem
            }
            indexes.remove(at: removeIndex)

            guard let decodedPart = decoded[remove] else {
                throw FountainError.expectedItem
            }

            FountainUtils.xorInPlace(&part.data, with: decodedPart.data)
        }

        if indexes.count == 1 {
            guard let index = indexes.first else {
                throw FountainError.expectedItem
            }
            decoded[index] = part
            queue.append((index, part))
        } else {
            buffer[indexes] = part
        }
    }
}
