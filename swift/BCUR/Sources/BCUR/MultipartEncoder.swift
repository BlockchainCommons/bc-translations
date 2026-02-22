/// A multipart UR encoder built on the fountain encoder.
public final class MultipartEncoder {
    private var encoder: FountainEncoder
    private let urType: String

    /// Creates a multipart encoder for a UR payload.
    public init(_ ur: UR, _ maxFragmentLen: Int) throws {
        do {
            self.encoder = try FountainEncoder(
                message: Array(ur.cbor.cborData),
                maxFragmentLength: maxFragmentLen
            )
        } catch let error as FountainError {
            throw URError(fountain: error)
        }
        self.urType = ur.urTypeStr
    }

    /// Returns the next multipart UR string.
    public func nextPart() throws -> String {
        let part = encoder.nextPart()

        do {
            let body = Bytewords.encode(try part.cbor(), style: .minimal)
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
}
