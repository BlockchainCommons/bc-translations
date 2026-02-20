import CryptoSwift
import Foundation

public func scrypt(_ pass: Data, _ salt: Data, _ outputLen: Int) -> Data {
    let params = (logN: 15, r: 8, p: 1)
    let scrypt = try! Scrypt(
        password: Array(pass),
        salt: Array(salt),
        dkLen: outputLen,
        N: 1 << params.logN,
        r: params.r,
        p: params.p
    )
    return Data(try! scrypt.calculate())
}

public func scryptOpt(
    _ pass: Data,
    _ salt: Data,
    _ outputLen: Int,
    _ logN: UInt8,
    _ r: UInt32,
    _ p: UInt32
) -> Data {
    let scrypt = try! Scrypt(
        password: Array(pass),
        salt: Array(salt),
        dkLen: outputLen,
        N: 1 << Int(logN),
        r: Int(r),
        p: Int(p)
    )
    return Data(try! scrypt.calculate())
}
