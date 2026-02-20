import Foundation
import XCTest
@testable import BCLifeHash

final class TestVectorsTests: XCTestCase {
    private struct TestVector: Decodable {
        let input: String
        let inputType: String
        let version: String
        let moduleSize: Int
        let hasAlpha: Bool
        let width: Int
        let height: Int
        let colors: [UInt8]

        enum CodingKeys: String, CodingKey {
            case input
            case inputType = "input_type"
            case version
            case moduleSize = "module_size"
            case hasAlpha = "has_alpha"
            case width
            case height
            case colors
        }
    }

    private func parseVersion(_ string: String) throws -> Version {
        switch string {
        case "version1":
            return .version1
        case "version2":
            return .version2
        case "detailed":
            return .detailed
        case "fiducial":
            return .fiducial
        case "grayscale_fiducial":
            return .grayscaleFiducial
        default:
            throw NSError(domain: "BCLifeHashTests", code: 1, userInfo: [NSLocalizedDescriptionKey: "Unknown version: \(string)"])
        }
    }

    private func decodeHex(_ hex: String) throws -> [UInt8] {
        if hex.count.isMultiple(of: 2) == false {
            throw NSError(domain: "BCLifeHashTests", code: 2, userInfo: [NSLocalizedDescriptionKey: "Invalid hex length"])
        }

        var bytes: [UInt8] = []
        bytes.reserveCapacity(hex.count / 2)

        var index = hex.startIndex
        while index < hex.endIndex {
            let nextIndex = hex.index(index, offsetBy: 2)
            let chunk = hex[index..<nextIndex]
            guard let byte = UInt8(chunk, radix: 16) else {
                throw NSError(domain: "BCLifeHashTests", code: 3, userInfo: [NSLocalizedDescriptionKey: "Invalid hex byte: \(chunk)"])
            }
            bytes.append(byte)
            index = nextIndex
        }

        return bytes
    }

    func testAllVectors() throws {
        let url = try XCTUnwrap(Bundle.module.url(forResource: "test-vectors", withExtension: "json"))
        let jsonData = try Data(contentsOf: url)
        let vectors = try JSONDecoder().decode([TestVector].self, from: jsonData)

        XCTAssertEqual(vectors.count, 35, "Expected 35 test vectors")

        for (i, tv) in vectors.enumerated() {
            let version = try parseVersion(tv.version)

            let image: Image
            if tv.inputType == "hex" {
                if tv.input.isEmpty {
                    image = makeFromData([], version: version, moduleSize: tv.moduleSize, hasAlpha: tv.hasAlpha)
                } else {
                    let data = try decodeHex(tv.input)
                    image = makeFromData(data, version: version, moduleSize: tv.moduleSize, hasAlpha: tv.hasAlpha)
                }
            } else {
                image = makeFromUTF8(tv.input, version: version, moduleSize: tv.moduleSize, hasAlpha: tv.hasAlpha)
            }

            XCTAssertEqual(
                image.width,
                tv.width,
                "Vector \(i): width mismatch for input=\(tv.input.debugDescription) version=\(tv.version)"
            )

            XCTAssertEqual(
                image.height,
                tv.height,
                "Vector \(i): height mismatch for input=\(tv.input.debugDescription) version=\(tv.version)"
            )

            XCTAssertEqual(
                image.colors.count,
                tv.colors.count,
                "Vector \(i): colors length mismatch for input=\(tv.input.debugDescription) version=\(tv.version)"
            )

            if image.colors != tv.colors {
                let components = tv.hasAlpha ? 4 : 3
                for j in 0..<min(image.colors.count, tv.colors.count) {
                    let got = image.colors[j]
                    let expected = tv.colors[j]
                    if got != expected {
                        let pixel = j / components
                        let component = j % components
                        let compName = ["R", "G", "B", "A"][component]
                        XCTFail(
                            "Vector \(i): pixel data mismatch for input=\(tv.input.debugDescription) version=\(tv.version)\n" +
                                "First diff at byte \(j) (pixel \(pixel), \(compName)): got \(got), expected \(expected)"
                        )
                        return
                    }
                }
                XCTFail("Vector \(i): pixel data mismatch")
                return
            }
        }
    }
}
