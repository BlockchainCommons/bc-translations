import BCUR
import Foundation

public protocol ECKeyBase:
    Hashable,
    CustomStringConvertible,
    CustomDebugStringConvertible
{
    static var keySize: Int { get }
    static func fromDataRef(_ data: some DataProtocol) throws(BCComponentsError) -> Self
    var data: Data { get }
    func hex() -> String
    static func fromHex(_ hex: String) throws(BCComponentsError) -> Self
}

public extension ECKeyBase {
    func hex() -> String {
        hexEncode(data)
    }

    static func fromHex(_ hex: String) throws(BCComponentsError) -> Self {
        try fromDataRef(parseHex(hex))
    }
}

public protocol ECKey: ECKeyBase, URCodable {
    func publicKey() -> ECPublicKey
}

public protocol ECPublicKeyBase: ECKey {
    func uncompressedPublicKey() -> ECUncompressedPublicKey
}
