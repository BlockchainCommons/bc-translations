import Foundation
import XCTest
@testable import BCCrypto

final class ScryptTests: XCTestCase {
    func testScryptBasic() {
        let pass = Data("password".utf8)
        let salt = Data("salt".utf8)
        let output = scrypt(password: pass, salt: salt, outputLength: 32)
        XCTAssertEqual(output.count, 32)
        XCTAssertEqual(output, scrypt(password: pass, salt: salt, outputLength: 32))
    }

    func testScryptDifferentSalt() {
        let pass = Data("password".utf8)
        let salt1 = Data("salt1".utf8)
        let salt2 = Data("salt2".utf8)

        XCTAssertNotEqual(
            scrypt(password: pass, salt: salt1, outputLength: 32),
            scrypt(password: pass, salt: salt2, outputLength: 32)
        )
    }

    func testScryptCustomParams() {
        let pass = Data("password".utf8)
        let salt = Data("salt".utf8)
        let output = scrypt(
            password: pass,
            salt: salt,
            outputLength: 32,
            logN: 15,
            r: 8,
            p: 1
        )
        XCTAssertEqual(output.count, 32)
    }

    func testScryptOutputLength() {
        let pass = Data("password".utf8)
        let salt = Data("salt".utf8)

        for length in [16, 24, 32, 64] {
            XCTAssertEqual(
                scrypt(password: pass, salt: salt, outputLength: length).count,
                length
            )
        }
    }
}
