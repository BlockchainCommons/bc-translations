import Foundation
import DCBOR

/// A Uniform Resource (UR): a URI-encoded CBOR object.
public struct UR: Equatable, Sendable {
    private let typeValue: URType
    private let value: CBOR

    /// Creates a UR from a validated type and a CBOR-encodable value.
    public init(_ urType: URType, _ cbor: some CBOREncodable) {
        self.typeValue = urType
        self.value = cbor.cbor
    }

    /// Creates a UR from a type string and a CBOR-encodable value.
    public init(_ urType: String, _ cbor: some CBOREncodable) throws {
        self.init(try URType(urType), cbor)
    }

    /// Parses a UR from a UR string.
    ///
    /// Input is normalized to lowercase before decoding to match Rust behavior.
    public static func fromURString(_ urString: String) throws -> UR {
        let lower = urString.lowercased()

        guard let stripScheme = lower.stripPrefix("ur:") else {
            throw URError.invalidScheme
        }

        guard let slashIndex = stripScheme.firstIndex(of: "/") else {
            throw URError.typeUnspecified
        }

        let typeString = String(stripScheme[..<slashIndex])
        let urType = try URType(typeString)

        let decoded: (UREncoding.Kind, [UInt8])
        do {
            decoded = try UREncoding.decode(lower)
        } catch let error as URCodecError {
            throw URError(ur: error)
        }

        guard decoded.0 == .singlePart else {
            throw URError.notSinglePart
        }

        do {
            let cbor = try CBOR(Data(decoded.1))
            return UR(urType, cbor)
        } catch {
            throw URError.fromCBORError(error)
        }
    }

    /// The UR string representation.
    public var string: String {
        UREncoding.encode(Array(value.cborData), urType: typeValue.string)
    }

    /// The UR string in uppercase, optimized for QR payload density.
    public var qrString: String {
        string.uppercased()
    }

    /// The uppercase QR string as UTF-8 bytes.
    public var qrData: [UInt8] {
        Array(qrString.utf8)
    }

    /// Validates that this UR has the expected type.
    public func checkType(_ otherType: URType) throws {
        if typeValue != otherType {
            throw URError.unexpectedType(otherType.string, typeValue.string)
        }
    }

    /// Validates that this UR has the expected type string.
    public func checkType(_ otherType: String) throws {
        try checkType(URType(otherType))
    }

    /// The UR type.
    public var urType: URType {
        typeValue
    }

    /// The UR type string.
    public var urTypeStr: String {
        typeValue.string
    }

    /// The CBOR payload.
    public var cbor: CBOR {
        value
    }
}

extension UR: CustomStringConvertible {
    public var description: String {
        string
    }
}

private extension String {
    func stripPrefix(_ prefix: String) -> String? {
        guard hasPrefix(prefix) else {
            return nil
        }
        return String(dropFirst(prefix.count))
    }
}
