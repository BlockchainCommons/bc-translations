import Foundation
import Sodium

public func argon2id(_ pass: Data, _ salt: Data, _ outputLen: Int) -> Data {
    let sodium = Sodium()
    let saltBytes = sodium.pwHash.SaltBytes
    var normalizedSalt = Array(salt)

    if normalizedSalt.count < saltBytes {
        normalizedSalt.append(
            contentsOf: repeatElement(0, count: saltBytes - normalizedSalt.count)
        )
    } else if normalizedSalt.count > saltBytes {
        normalizedSalt = Array(normalizedSalt.prefix(saltBytes))
    }

    let hashed = sodium.pwHash.hash(
        outputLength: outputLen,
        passwd: Array(pass),
        salt: normalizedSalt,
        opsLimit: 2,
        memLimit: 19 * 1024 * 1024,
        alg: .Argon2ID13
    )!

    return Data(hashed)
}
