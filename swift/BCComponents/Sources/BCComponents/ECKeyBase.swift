import BCUR
import Foundation

public protocol ECKeyBase:
    Hashable,
    CustomStringConvertible,
    CustomDebugStringConvertible
{
    static var keySize: Int { get }
    init(_ data: Data) throws(BCComponentsError)
    var data: Data { get }
    var hex: String { get }
    static func fromHex(_ hex: String) throws(BCComponentsError) -> Self
}

public extension ECKeyBase {
    var hex: String {
        hexEncode(data)
    }

    static func fromHex(_ hex: String) throws(BCComponentsError) -> Self {
        try Self(parseHex(hex))
    }
}

public protocol ECKey: ECKeyBase, URCodable {
    func publicKey() -> ECPublicKey
}

public protocol ECPublicKeyBase: ECKey {
    func uncompressedPublicKey() -> ECUncompressedPublicKey
}
