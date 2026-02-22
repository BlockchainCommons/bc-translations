package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.toCbor
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith

class URTest {

    /**
     * Generates a deterministic message via Xoshiro256 and wraps it as a CBOR byte string.
     */
    private fun makeTestPayload(length: Int, seed: String): Cbor {
        val message = Xoshiro256.makeMessage(seed, length)
        return Cbor.fromByteString(message)
    }

    @Test
    fun testUr() {
        val cbor = listOf(
            Cbor.fromInt(1),
            Cbor.fromInt(2),
            Cbor.fromInt(3)
        ).toCbor()
        val ur = UR.create("test", cbor)
        val urString = ur.string
        assertEquals("ur:test/lsadaoaxjygonesw", urString)

        // Round-trip
        val decoded = UR.fromUrString(urString)
        assertEquals("test", decoded.urTypeStr)
        assertEquals(cbor, decoded.cbor)

        // Uppercase input is accepted
        val capsUrString = "UR:TEST/LSADAOAXJYGONESW"
        val capsDecoded = UR.fromUrString(capsUrString)
        assertEquals("test", capsDecoded.urTypeStr)
        assertEquals(cbor, capsDecoded.cbor)
    }

    @Test
    fun testSinglePartUr() {
        val ur = makeTestPayload(50, "Wolf")
        val encoded = UREncoding.encode(ur.toCborData(), "bytes")
        val expected = "ur:bytes/hdeymejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtgwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsdwkbrkch"
        assertEquals(expected, encoded)

        val (kind, data) = UREncoding.decode(encoded)
        assertEquals(UREncoding.Kind.SinglePart, kind)
        assertEquals(ur.toCborData().toList(), data.toList())
    }

    @Test
    fun testUrEncoder() {
        val ur = makeTestPayload(256, "Wolf")
        val urObj = UR.create("bytes", ur)
        val encoder = MultipartEncoder(urObj, 30)

        val expected = listOf(
            "ur:bytes/1-9/lpadascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtdkgslpgh",
            "ur:bytes/2-9/lpaoascfadaxcywenbpljkhdcagwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsgmghhkhstlrdcxaefz",
            "ur:bytes/3-9/lpaxascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjksopdzmol",
            "ur:bytes/4-9/lpaaascfadaxcywenbpljkhdcasotkhemthydawydtaxneurlkosgwcekonertkbrlwmplssjtammdplolsbrdzcrtas",
            "ur:bytes/5-9/lpahascfadaxcywenbpljkhdcatbbdfmssrkzmcwnezelennjpfzbgmuktrhtejscktelgfpdlrkfyfwdajldejokbwf",
            "ur:bytes/6-9/lpamascfadaxcywenbpljkhdcackjlhkhybssklbwefectpfnbbectrljectpavyrolkzczcpkmwidmwoxkilghdsowp",
            "ur:bytes/7-9/lpatascfadaxcywenbpljkhdcavszmwnjkwtclrtvaynhpahrtoxmwvwatmedibkaegdosftvandiodagdhthtrlnnhy",
            "ur:bytes/8-9/lpayascfadaxcywenbpljkhdcadmsponkkbbhgsoltjntegepmttmoonftnbuoiyrehfrtsabzsttorodklubbuyaetk",
            "ur:bytes/9-9/lpasascfadaxcywenbpljkhdcajskecpmdckihdyhphfotjojtfmlnwmadspaxrkytbztpbauotbgtgtaeaevtgavtny",
            "ur:bytes/10-9/lpbkascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtwdkiplzs",
            "ur:bytes/11-9/lpbdascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjkvetiiapk",
            "ur:bytes/12-9/lpbnascfadaxcywenbpljkhdcarllaluzmdmgstospeyiefmwejlwtpedamktksrvlcygmzemovovllarodtmtbnptrs",
            "ur:bytes/13-9/lpbtascfadaxcywenbpljkhdcamtkgtpknghchchyketwsvwgwfdhpgmgtylctotzopdrpayoschcmhplffziachrfgd",
            "ur:bytes/14-9/lpbaascfadaxcywenbpljkhdcapazewnvonnvdnsbyleynwtnsjkjndeoldydkbkdslgjkbbkortbelomueekgvstegt",
            "ur:bytes/15-9/lpbsascfadaxcywenbpljkhdcaynmhpddpzmversbdqdfyrehnqzlugmjzmnmtwmrouohtstgsbsahpawkditkckynwt",
            "ur:bytes/16-9/lpbeascfadaxcywenbpljkhdcawygekobamwtlihsnpalnsghenskkiynthdzotsimtojetprsttmukirlrsbtamjtpd",
            "ur:bytes/17-9/lpbyascfadaxcywenbpljkhdcamklgftaxykpewyrtqzhydntpnytyisincxmhtbceaykolduortotiaiaiafhiaoyce",
            "ur:bytes/18-9/lpbgascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtntwkbkwy",
            "ur:bytes/19-9/lpbwascfadaxcywenbpljkhdcadekicpaajootjzpsdrbalpeywllbdsnbinaerkurspbncxgslgftvtsrjtksplcpeo",
            "ur:bytes/20-9/lpbbascfadaxcywenbpljkhdcayapmrleeleaxpasfrtrdkncffwjyjzgyetdmlewtkpktgllepfrltataztksmhkbot"
        )

        assertEquals(9, encoder.partCount)
        for ((index, e) in expected.withIndex()) {
            assertEquals(index, encoder.currentIndex)
            assertEquals(e, encoder.nextPart())
        }
    }

    @Test
    fun testDecoderErrors() {
        // Invalid scheme (not "ur:")
        assertFailsWith<URException.InvalidScheme> {
            UREncoding.decode("uhr:bytes/aeadaolazmjendeoti")
        }

        // Missing type (no slash after scheme)
        assertFailsWith<URException.TypeUnspecified> {
            UREncoding.decode("ur:aeadaolazmjendeoti")
        }

        // Invalid characters in type
        assertFailsWith<URException.DecoderError> {
            UREncoding.decode("ur:bytes#4/aeadaolazmjendeoti")
        }

        // Invalid indices (non-numeric total)
        assertFailsWith<URException.DecoderError> {
            UREncoding.decode("ur:bytes/1-1a/aeadaolazmjendeoti")
        }

        // Invalid indices (too many slashes — implies three-segment multi-part with extra slash)
        assertFailsWith<URException.DecoderError> {
            UREncoding.decode("ur:bytes/1-1/toomuch/aeadaolazmjendeoti")
        }

        // These should succeed
        UREncoding.decode("ur:bytes/aeadaolazmjendeoti")
        UREncoding.decode("ur:whatever-12/aeadaolazmjendeoti")
    }

    @Test
    fun testCustomEncoder() {
        // The Rust ur crate's Encoder::new() takes raw bytes (not CBOR-wrapped).
        // We replicate this by using FountainEncoder directly with raw bytes and
        // formatting the UR string in the same way MultipartEncoder does.
        val data = "Ten chars!".toByteArray()
        val maxLength = 5
        val encoder = FountainEncoder(data, maxLength)
        val part = encoder.nextPart()
        val body = Bytewords.encode(part.toCbor(), BytewordsStyle.Minimal)
        val urString = "ur:my-scheme/${part.sequenceId}/$body"
        assertEquals(
            "ur:my-scheme/1-2/lpadaobkcywkwmhfwnfeghihjtcxiansvomopr",
            urString
        )
    }
}
