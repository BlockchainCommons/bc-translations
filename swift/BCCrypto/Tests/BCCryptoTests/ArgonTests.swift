import Foundation
import XCTest
@testable import BCCrypto

final class ArgonTests: XCTestCase {
    func testArgon2idBasic() {
        let pass = Data("password".utf8)
        let salt = Data("example salt".utf8)
        let output = argon2id(pass, salt, 32)

        XCTAssertEqual(output.count, 32)
        XCTAssertEqual(output, argon2id(pass, salt, 32))
    }

    func testArgon2idDifferentSalt() {
        let pass = Data("password".utf8)
        let salt1 = Data("example salt".utf8)
        let salt2 = Data("example salt2".utf8)

        XCTAssertNotEqual(argon2id(pass, salt1, 32), argon2id(pass, salt2, 32))
    }
}
