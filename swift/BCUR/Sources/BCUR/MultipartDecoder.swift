import Foundation
import DCBOR

/// A multipart UR decoder that reconstructs a full UR from received parts.
public final class MultipartDecoder {
    private var urType: URType?
    private var decoder = FountainDecoder()

    /// Creates an empty multipart decoder.
    public init() { }

    /// Receives a multipart UR part string.
    ///
    /// Input is normalized to lowercase to support uppercase QR payloads.
    public func receive(_ value: String) throws {
        let normalized = value.lowercased()
        let decodedType = try decodeType(normalized)

        if let existing = urType {
            if existing != decodedType {
                throw URError.unexpectedType(existing.string, decodedType.string)
            }
        } else {
            urType = decodedType
        }

        let decoded: (UREncoding.Kind, [UInt8])
        do {
            decoded = try UREncoding.decode(normalized)
        } catch let error as URCodecError {
            throw URError(ur: error)
        }

        guard decoded.0 == .multiPart else {
            throw URError(ur: .notMultiPart)
        }

        let part: FountainPart
        do {
            part = try FountainPart.fromCbor(decoded.1)
        } catch let error as FountainError {
            throw URError(fountain: error)
        }

        do {
            _ = try decoder.receive(part)
        } catch let error as FountainError {
            throw URError(fountain: error)
        }
    }

    /// Indicates whether all fragments have been received.
    public var isComplete: Bool {
        decoder.complete
    }

    /// Returns the reconstructed UR if complete, else `nil`.
    public func message() throws -> UR? {
        let messageData: [UInt8]?
        do {
            messageData = try decoder.message()
        } catch let error as FountainError {
            throw URError(fountain: error)
        }

        guard let messageData else {
            return nil
        }

        guard let urType else {
            throw URError.invalidType
        }

        do {
            let cbor = try CBOR(Data(messageData))
            return UR(urType, cbor)
        } catch {
            throw URError.fromCBORError(error)
        }
    }

    private func decodeType(_ urString: String) throws -> URType {
        guard urString.hasPrefix("ur:") else {
            throw URError.invalidScheme
        }

        let withoutScheme = urString.dropFirst(3)
        let firstComponent = String(withoutScheme.prefix { $0 != "/" })
        return try URType(firstComponent)
    }
}
