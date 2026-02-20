import Foundation
import XCTest
@testable import BCCrypto

final class HashTests: XCTestCase {
    func testCRC32() {
        let input = Data("Hello, world!".utf8)
        XCTAssertEqual(crc32(input), 0xEBE6C6E6)
        XCTAssertEqual(crc32Data(input), Data(testHex: "ebe6c6e6"))
        XCTAssertEqual(
            crc32DataOpt(input, littleEndian: true),
            Data(testHex: "e6c6e6eb")
        )
    }

    func testSHA256() {
        let input = Data(
            "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq".utf8
        )
        let expected = Data(
            testHex: "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1"
        )
        XCTAssertEqual(sha256(input), expected)
    }

    func testSHA512() {
        let input = Data(
            "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq".utf8
        )
        let expected = Data(
            testHex: "204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c33596fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445"
        )
        XCTAssertEqual(sha512(input), expected)
    }

    func testHMACSHA() {
        let key = Data(testHex: "0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b")
        let message = Data("Hi There".utf8)

        XCTAssertEqual(
            hmacSHA256(key, message),
            Data(
                testHex: "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7"
            )
        )
        XCTAssertEqual(
            hmacSHA512(key, message),
            Data(
                testHex: "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854"
            )
        )
    }

    func testPBKDF2HMACSHA256() {
        XCTAssertEqual(
            pbkdf2HmacSHA256(Data("password".utf8), Data("salt".utf8), 1, 32),
            Data(
                testHex: "120fb6cffcf8b32c43e7225256c4f837a86548c92ccc35480805987cb70be17b"
            )
        )
    }

    func testHKDFHMACSHA256() {
        let keyMaterial = Data("hello".utf8)
        let salt = Data(testHex: "8e94ef805b93e683ff18")
        XCTAssertEqual(
            hkdfHmacSHA256(keyMaterial, salt, 32),
            Data(
                testHex: "13485067e21af17c0900f70d885f02593c0e61e46f86450e4a0201a54c14db76"
            )
        )
    }
}
