import Foundation
import DCBOR

/// A part emitted by the fountain encoder.
internal struct FountainPart: Equatable, Sendable {
    var sequence: Int
    var sequenceCount: Int
    var messageLength: Int
    var checksum: UInt32
    var data: [UInt8]

    var indexes: [Int] {
        FountainUtils.chooseFragments(
            sequence: sequence,
            fragmentCount: sequenceCount,
            checksum: checksum
        )
    }

    var isSimple: Bool {
        indexes.count == 1
    }

    var sequenceId: String {
        "\(sequence)-\(sequenceCount)"
    }

    func deepCopy() -> FountainPart {
        self
    }

    func cbor() throws -> [UInt8] {
        let sequence = try u32(sequence)
        let sequenceCount = try u32(sequenceCount)
        let messageLength = try u32(messageLength)

        let cborArray: [CBOR] = [
            sequence.cbor,
            sequenceCount.cbor,
            messageLength.cbor,
            checksum.cbor,
            Data(data).cbor,
        ]

        return Array(cborArray.cborData)
    }

    static func fromCbor(_ bytes: [UInt8]) throws -> FountainPart {
        do {
            let cbor = try CBOR(Data(bytes))
            guard case .array(let values) = cbor else {
                throw FountainError.cborDecode("unexpected type: expected array")
            }

            guard values.count == 5 else {
                throw FountainError.cborDecode("decode error: invalid CBOR array length")
            }

            let sequence = try intFromU32(values[0])
            let sequenceCount = try intFromU32(values[1])
            let messageLength = try intFromU32(values[2])
            let checksum = try decodeU32(values[3])
            let data = try decodeBytes(values[4])

            return FountainPart(
                sequence: sequence,
                sequenceCount: sequenceCount,
                messageLength: messageLength,
                checksum: checksum,
                data: data
            )
        } catch let error as FountainError {
            throw error
        } catch {
            throw FountainError.cborDecode(
                (error as? LocalizedError)?.errorDescription ?? String(describing: error)
            )
        }
    }

    private func u32(_ value: Int) throws -> UInt32 {
        guard let converted = UInt32(exactly: value) else {
            throw FountainError.cborEncode("converting usize to u32")
        }
        return converted
    }

    private static func intFromU32(_ cbor: CBOR) throws -> Int {
        let value = try decodeU32(cbor)
        guard let converted = Int(exactly: value) else {
            throw FountainError.expectedItem
        }
        return converted
    }

    private static func decodeU32(_ cbor: CBOR) throws -> UInt32 {
        do {
            let value = try UInt64(cbor: cbor)
            guard value <= UInt64(UInt32.max) else {
                throw FountainError.cborDecode("converting u64 to u32")
            }
            return UInt32(value)
        } catch let error as FountainError {
            throw error
        } catch {
            throw FountainError.cborDecode(
                (error as? LocalizedError)?.errorDescription ?? String(describing: error)
            )
        }
    }

    private static func decodeBytes(_ cbor: CBOR) throws -> [UInt8] {
        do {
            return Array(try Data(cbor: cbor))
        } catch {
            throw FountainError.cborDecode(
                (error as? LocalizedError)?.errorDescription ?? String(describing: error)
            )
        }
    }
}
