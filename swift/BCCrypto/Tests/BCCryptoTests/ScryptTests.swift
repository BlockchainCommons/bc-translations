import Foundation
import XCTest
@testable import BCCrypto

final class ScryptTests: XCTestCase {
    func testScryptBasic() {
        let pass = Data("password".utf8)
        let salt = Data("salt".utf8)
        let output = scrypt(pass, salt, 32)
        XCTAssertEqual(output.count, 32)
        XCTAssertEqual(output, scrypt(pass, salt, 32))
    }

    func testScryptDifferentSalt() {
        let pass = Data("password".utf8)
        let salt1 = Data("salt1".utf8)
        let salt2 = Data("salt2".utf8)

        XCTAssertNotEqual(scrypt(pass, salt1, 32), scrypt(pass, salt2, 32))
    }

    func testScryptOptBasic() {
        let pass = Data("password".utf8)
        let salt = Data("salt".utf8)
        let output = scryptOpt(pass, salt, 32, 15, 8, 1)
        XCTAssertEqual(output.count, 32)
    }

    func testScryptOutputLength() {
        let pass = Data("password".utf8)
        let salt = Data("salt".utf8)

        for length in [16, 24, 32, 64] {
            XCTAssertEqual(scrypt(pass, salt, length).count, length)
        }
    }
}
