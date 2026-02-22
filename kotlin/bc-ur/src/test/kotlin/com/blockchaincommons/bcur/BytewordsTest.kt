package com.blockchaincommons.bcur

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith

class BytewordsTest {

    @Test
    fun testBytewords() {
        val input = byteArrayOf(0, 1, 2, 128.toByte(), 255.toByte())

        // Encode in all three styles
        assertEquals(
            "able acid also lava zoom jade need echo taxi",
            Bytewords.encode(input, BytewordsStyle.Standard)
        )
        assertEquals(
            "able-acid-also-lava-zoom-jade-need-echo-taxi",
            Bytewords.encode(input, BytewordsStyle.Uri)
        )
        assertEquals(
            "aeadaolazmjendeoti",
            Bytewords.encode(input, BytewordsStyle.Minimal)
        )

        // Decode in all three styles
        assertEquals(
            input.toList(),
            Bytewords.decode("able acid also lava zoom jade need echo taxi", BytewordsStyle.Standard).toList()
        )
        assertEquals(
            input.toList(),
            Bytewords.decode("able-acid-also-lava-zoom-jade-need-echo-taxi", BytewordsStyle.Uri).toList()
        )
        assertEquals(
            input.toList(),
            Bytewords.decode("aeadaolazmjendeoti", BytewordsStyle.Minimal).toList()
        )

        // Empty payload is allowed
        val emptyEncoded = Bytewords.encode(byteArrayOf(), BytewordsStyle.Minimal)
        Bytewords.decode(emptyEncoded, BytewordsStyle.Minimal)

        // Bad checksum
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("able acid also lava zero jade need echo wolf", BytewordsStyle.Standard)
        }
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("able-acid-also-lava-zero-jade-need-echo-wolf", BytewordsStyle.Uri)
        }
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("aeadaolazojendeowf", BytewordsStyle.Minimal)
        }

        // Too short (less than 4 checksum bytes)
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("wolf", BytewordsStyle.Standard)
        }
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("", BytewordsStyle.Standard)
        }

        // Invalid length (minimal must be even)
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("aea", BytewordsStyle.Minimal)
        }

        // Non-ASCII input
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("₿", BytewordsStyle.Standard)
        }
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("₿", BytewordsStyle.Uri)
        }
        assertFailsWith<URException.BytewordsError> {
            Bytewords.decode("₿", BytewordsStyle.Minimal)
        }
    }

    @Test
    fun testEncoding() {
        val input = byteArrayOf(
            245.toByte(), 215.toByte(), 20, 198.toByte(), 241.toByte(), 235.toByte(), 69, 59,
            209.toByte(), 205.toByte(), 165.toByte(), 18, 150.toByte(), 158.toByte(), 116, 135.toByte(),
            229.toByte(), 212.toByte(), 19, 159.toByte(), 17, 37, 239.toByte(), 240.toByte(),
            253.toByte(), 11, 109, 191.toByte(), 37, 242.toByte(), 38, 120, 223.toByte(), 41,
            156.toByte(), 189.toByte(), 242.toByte(), 254.toByte(), 147.toByte(), 204.toByte(), 66,
            163.toByte(), 216.toByte(), 175.toByte(), 191.toByte(), 72, 169.toByte(), 54, 32, 60,
            144.toByte(), 230.toByte(), 210.toByte(), 137.toByte(), 184.toByte(), 197.toByte(), 33,
            113, 88, 14, 157.toByte(), 31, 177.toByte(), 46, 1, 115, 205.toByte(), 69,
            225.toByte(), 150.toByte(), 65, 235.toByte(), 58, 144.toByte(), 65, 240.toByte(),
            133.toByte(), 69, 113, 247.toByte(), 63, 53, 242.toByte(), 165.toByte(), 160.toByte(),
            144.toByte(), 26, 13, 79, 237.toByte(), 133.toByte(), 71, 82, 69, 254.toByte(),
            165.toByte(), 138.toByte(), 41, 85, 24
        )

        val standardEncoded = "yank toys bulb skew when warm free fair tent swan " +
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

        val minimalEncoded = "yktsbbswwnwmfefrttsnonbgmtnnjyltvwtybwne" +
            "bydawswtzcbdjnrsdawzdsksurdtnsrywzzemusf" +
            "fwottppersfdptencxfnmhvatdldroskcljshdba" +
            "ntctpadmadjksnfevymtfpwmftmhfpwtlpfejsyl" +
            "fhecwzonnbmhcybtgwwelpflgmfezeonledtgocs" +
            "fzhycypf"

        assertEquals(input.toList(), Bytewords.decode(standardEncoded, BytewordsStyle.Standard).toList())
        assertEquals(input.toList(), Bytewords.decode(minimalEncoded, BytewordsStyle.Minimal).toList())
        assertEquals(standardEncoded, Bytewords.encode(input, BytewordsStyle.Standard))
        assertEquals(minimalEncoded, Bytewords.encode(input, BytewordsStyle.Minimal))
    }

    @Test
    fun testBytemojiUniqueness() {
        val bytemojis = BytewordsConstants.BYTEMOJIS.toList()
        val counts = mutableMapOf<String, Int>()
        for (bytemoji in bytemojis) {
            counts[bytemoji] = (counts[bytemoji] ?: 0) + 1
        }
        val duplicates = counts.filter { it.value > 1 }
        assertEquals(emptyMap<String, Int>(), duplicates, "Bytemojis must be unique, but duplicates found: $duplicates")
    }

    @Test
    fun testBytemojiLengths() {
        val overLength = BytewordsConstants.BYTEMOJIS.filter { it.toByteArray(Charsets.UTF_8).size > 4 }
        assertEquals(
            emptyList<String>(),
            overLength,
            "Some bytemojis are over 4 bytes: $overLength"
        )
    }
}
