import Foundation
import BCComponents

public extension Envelope {
    func encrypt(_ key: SymmetricKey) -> Envelope {
        try! wrap().encryptSubject(with: key)
    }

    func decrypt(_ key: SymmetricKey) throws -> Envelope {
        try decryptSubject(with: key).unwrap()
    }
}
