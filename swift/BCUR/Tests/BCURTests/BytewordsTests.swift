import Foundation
import Testing
@testable import BCUR

struct BytewordsTests {
    @Test func testBytewords() throws {
        let input: [UInt8] = [0, 1, 2, 128, 255]

        #expect(
            Bytewords.encode(input, style: .standard)
                == "able acid also lava zoom jade need echo taxi"
        )
        #expect(
            Bytewords.encode(input, style: .uri)
                == "able-acid-also-lava-zoom-jade-need-echo-taxi"
        )
        #expect(
            Bytewords.encode(input, style: .minimal)
                == "aeadaolazmjendeoti"
        )

        #expect(
            try Bytewords.decode("able acid also lava zoom jade need echo taxi", style: .standard)
                == input
        )
        #expect(
            try Bytewords.decode("able-acid-also-lava-zoom-jade-need-echo-taxi", style: .uri)
                == input
        )
        #expect(
            try Bytewords.decode("aeadaolazmjendeoti", style: .minimal)
                == input
        )

        // Empty payload is allowed.
        let empty = Bytewords.encode([], style: .minimal)
        #expect(try Bytewords.decode(empty, style: .minimal) == [])

        // Bad checksum.
        #expect(throws: (any Error).self) {
            try Bytewords.decode("able acid also lava zero jade need echo wolf", style: .standard)
        }
        #expect(throws: (any Error).self) {
            try Bytewords.decode("able-acid-also-lava-zero-jade-need-echo-wolf", style: .uri)
        }
        #expect(throws: (any Error).self) {
            try Bytewords.decode("aeadaolazojendeowf", style: .minimal)
        }

        // Too short.
        #expect(throws: (any Error).self) {
            try Bytewords.decode("wolf", style: .standard)
        }
        #expect(throws: (any Error).self) {
            try Bytewords.decode("", style: .standard)
        }

        // Invalid length for minimal encoding.
        #expect(throws: (any Error).self) {
            try Bytewords.decode("aea", style: .minimal)
        }

        // Non-ASCII input.
        #expect(throws: (any Error).self) {
            try Bytewords.decode("₿", style: .standard)
        }
        #expect(throws: (any Error).self) {
            try Bytewords.decode("₿", style: .uri)
        }
        #expect(throws: (any Error).self) {
            try Bytewords.decode("₿", style: .minimal)
        }
    }

    @Test func testEncoding() throws {
        let input: [UInt8] = [
            245, 215, 20, 198, 241, 235, 69, 59,
            209, 205, 165, 18, 150, 158, 116, 135,
            229, 212, 19, 159, 17, 37, 239, 240,
            253, 11, 109, 191, 37, 242, 38, 120,
            223, 41, 156, 189, 242, 254, 147, 204,
            66, 163, 216, 175, 191, 72, 169, 54,
            32, 60, 144, 230, 210, 137, 184, 197,
            33, 113, 88, 14, 157, 31, 177, 46,
            1, 115, 205, 69, 225, 150, 65, 235,
            58, 144, 65, 240, 133, 69, 113, 247,
            63, 53, 242, 165, 160, 144, 26, 13,
            79, 237, 133, 71, 82, 69, 254, 165,
            138, 41, 85, 24,
        ]

        let standardEncoded =
            "yank toys bulb skew when warm free fair tent swan " +
            "open brag mint noon jury list view tiny brew note " +
            "body data webs what zinc bald join runs data whiz " +
            "days keys user diet news ruby whiz zone menu surf " +
            "flew omit trip pose runs fund part even crux fern " +
            "math visa tied loud redo silk curl jugs hard beta " +
            "next cost puma drum acid junk swan free very mint " +
            "flap warm fact math flap what limp free jugs yell " +
            "fish epic whiz open numb math city belt glow wave " +
            "limp fuel grim free zone open love diet gyro cats " +
            "fizz holy city puff"

        let minimalEncoded =
            "yktsbbswwnwmfefrttsnonbgmtnnjyltvwtybwne" +
            "bydawswtzcbdjnrsdawzdsksurdtnsrywzzemusf" +
            "fwottppersfdptencxfnmhvatdldroskcljshdba" +
            "ntctpadmadjksnfevymtfpwmftmhfpwtlpfejsyl" +
            "fhecwzonnbmhcybtgwwelpflgmfezeonledtgocs" +
            "fzhycypf"

        #expect(try Bytewords.decode(standardEncoded, style: .standard) == input)
        #expect(try Bytewords.decode(minimalEncoded, style: .minimal) == input)
        #expect(Bytewords.encode(input, style: .standard) == standardEncoded)
        #expect(Bytewords.encode(input, style: .minimal) == minimalEncoded)
    }

    @Test func testBytemojiUniqueness() {
        #expect(bytemojis.count == 256)
        #expect(Set(bytemojis).count == bytemojis.count)
    }

    @Test func testBytemojiLengths() {
        let overLength = bytemojis.filter { emoji in
            emoji.utf8.count > 4
        }
        #expect(overLength.isEmpty)
    }
}
