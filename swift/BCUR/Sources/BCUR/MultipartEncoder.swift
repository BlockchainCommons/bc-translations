/// A multipart UR encoder built on the fountain encoder.
public final class MultipartEncoder {
    private var encoder: FountainEncoder
    private let urType: String
    private let messageData: [UInt8]

    /// Creates a multipart encoder for a UR payload.
    public init(_ ur: UR, _ maxFragmentLen: Int) throws {
        let data = Array(ur.cbor.cborData)
        do {
            self.encoder = try FountainEncoder(
                message: data,
                maxFragmentLength: maxFragmentLen
            )
        } catch let error as FountainError {
            throw URError(fountain: error)
        }
        self.urType = ur.urTypeString
        self.messageData = data
    }

    /// Returns the next UR string. Single-part URs use the simple
    /// `ur:type/payload` format; multi-part URs use `ur:type/seq-total/payload`.
    public func nextPart() throws -> String {
        let part = encoder.nextPart()

        if partsCount == 1 {
            return UREncoding.encode(messageData, urType: urType)
        }

        do {
            let body = Bytewords.encode(try part.cborEncoded(), style: .minimal)
            return "ur:\(urType)/\(part.sequenceId)/\(body)"
        } catch let error as FountainError {
            throw URError(fountain: error)
        }
    }

    /// The count of emitted parts so far.
    public var currentIndex: Int {
        encoder.currentSequence
    }

    /// The number of source fragments in the underlying message.
    public var partsCount: Int {
        encoder.fragmentCount
    }

    /// The fragment indexes included in the most recently emitted part.
    public var lastFragmentIndexes: [Int] {
        encoder.lastFragmentIndexes
    }
}
