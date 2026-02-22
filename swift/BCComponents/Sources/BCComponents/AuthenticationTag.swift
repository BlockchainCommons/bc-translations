import DCBOR
import Foundation

public struct AuthenticationTag: Equatable, Sendable {
    public static let authenticationTagSize = 16

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.authenticationTagSize, name: "authentication tag")
        self.value = value
    }

    public static func fromData(_ value: Data) throws(BCComponentsError) -> AuthenticationTag {
        try AuthenticationTag(value)
    }

    public var data: Data {
        value
    }

    public func asBytes() -> Data {
        value
    }
}

extension AuthenticationTag: CustomDebugStringConvertible {
    public var debugDescription: String {
        "AuthenticationTag(\(hexEncode(value)))"
    }
}

extension AuthenticationTag: CBOREncodable {
    public var cbor: CBOR {
        .bytes(value)
    }

    public var cborData: Data {
        cbor.cborData
    }
}

extension AuthenticationTag: CBORDecodable {
    public init(cbor: CBOR) throws {
        try self.init(byteString(cbor))
    }
}

extension AuthenticationTag: CBORCodable {}
