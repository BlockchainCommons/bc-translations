import DCBOR
import Foundation

public protocol KeyDerivation: CBORCodable {
    static var index: Int { get }

    mutating func lock(
        _ contentKey: SymmetricKey,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> EncryptedMessage

    func unlock(
        _ encryptedMessage: EncryptedMessage,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> SymmetricKey
}
