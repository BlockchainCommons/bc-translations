import Foundation
import Testing
import DCBOR
@testable import BCUR

struct URTests {
    @Test func testUr() throws {
        let cbor: CBOR = [1, 2, 3]
        let ur = try UR("test", cbor)

        #expect(ur.urString == "ur:test/lsadaoaxjygonesw")

        let decoded = try UR(urString:ur.urString)
        #expect(decoded.urTypeString == "test")
        #expect(decoded.cbor == cbor)

        let capsDecoded = try UR(urString:"UR:TEST/LSADAOAXJYGONESW")
        #expect(capsDecoded.urTypeString == "test")
        #expect(capsDecoded.cbor == cbor)
    }

    @Test func testSinglePartUr() throws {
        let payload = makeMessageUR(length: 50, seed: "Wolf")
        let encoded = UREncoding.encode(payload, urType: "bytes")

        let expected = "ur:bytes/hdeymejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtgwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsdwkbrkch"
        #expect(encoded == expected)

        let decoded = try UREncoding.decode(encoded)
        #expect(decoded.0 == .singlePart)
        #expect(decoded.1 == payload)
    }

    @Test func testUrEncoder() throws {
        let payload = makeMessageUR(length: 256, seed: "Wolf")
        let urPayload = try CBOR(Data(payload))
        let ur = try UR("bytes", urPayload)
        let encoder = try MultipartEncoder(ur, 30)

        let expected = [
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
            "ur:bytes/20-9/lpbbascfadaxcywenbpljkhdcayapmrleeleaxpasfrtrdkncffwjyjzgyetdmlewtkpktgllepfrltataztksmhkbot",
        ]

        #expect(encoder.partsCount == 9)
        for (index, item) in expected.enumerated() {
            #expect(encoder.currentIndex == index)
            #expect(try encoder.nextPart() == item)
        }
    }

    @Test func testUrEncoderDecoderBcCryptoRequest() throws {
        let uuid = hexToBytes("020C223A86F7464693FC650EF3CAC047")
        let seedDigest = hexToBytes("E824467CAFFEAF3BBC3E0CA095E660A9BAD80DDB6A919433A37161908B9A3986")

        var innerMap = Map()
        innerMap.insert(1, CBOR.tagged(BCTags.Tag(600), Data(seedDigest).cbor))

        var outerMap = Map()
        outerMap.insert(1, CBOR.tagged(BCTags.Tag(37), Data(uuid).cbor))
        outerMap.insert(2, CBOR.tagged(BCTags.Tag(500), innerMap.cbor))

        let data = Array(outerMap.cborData)
        let encoded = UREncoding.encode(data, urType: "crypto-request")
        let expected = "ur:crypto-request/oeadtpdagdaobncpftlnylfgfgmuztihbawfsgrtflaotaadwkoyadtaaohdhdcxvsdkfgkepezepefrrffmbnnbmdvahnptrdtpbtuyimmemweootjshsmhlunyeslnameyhsdi"
        #expect(encoded == expected)

        let decoded = try UREncoding.decode(encoded)
        #expect(decoded.0 == .singlePart)
        #expect(decoded.1 == data)
    }

    @Test func testMultipartUr() throws {
        let payload = makeMessageUR(length: 32_767, seed: "Wolf")
        let cbor = try CBOR(Data(payload))
        let ur = try UR("bytes", cbor)

        let encoder = try MultipartEncoder(ur, 1_000)
        let decoder = MultipartDecoder()

        while !decoder.isComplete {
            #expect(try decoder.message() == nil)
            try decoder.receive(encoder.nextPart())
        }

        #expect(try decoder.message() == ur)
    }

    @Test func testDecoder() throws {
        #expect(throws: URCodecError.self) {
            _ = try UREncoding.decode("uhr:bytes/aeadaolazmjendeoti")
        }
        #expect(throws: URCodecError.self) {
            _ = try UREncoding.decode("ur:aeadaolazmjendeoti")
        }
        #expect(throws: URCodecError.self) {
            _ = try UREncoding.decode("ur:bytes#4/aeadaolazmjendeoti")
        }
        #expect(throws: URCodecError.self) {
            _ = try UREncoding.decode("ur:bytes/1-1a/aeadaolazmjendeoti")
        }
        #expect(throws: URCodecError.self) {
            _ = try UREncoding.decode("ur:bytes/1-1/toomuch/aeadaolazmjendeoti")
        }

        _ = try UREncoding.decode("ur:bytes/aeadaolazmjendeoti")
        _ = try UREncoding.decode("ur:whatever-12/aeadaolazmjendeoti")
    }

    @Test func testCustomEncoder() throws {
        let data = Array("Ten chars!".utf8)
        var encoder = try FountainEncoder(message: data, maxFragmentLength: 5)

        let part = encoder.nextPart()
        let body = Bytewords.encode(try part.cborEncoded(), style: .minimal)
        let urString = "ur:my-scheme/\(part.sequenceId)/\(body)"

        #expect(urString == "ur:my-scheme/1-2/lpadaobkcywkwmhfwnfeghihjtcxiansvomopr")
    }

    @Test func testMultipartDecoderAcceptsUppercaseQrParts() throws {
        let cbor = CBOR.bytes(Data(Xoshiro256.makeMessage(seed: "Wolf", size: 128)))
        let ur = try UR("bytes", cbor)

        let encoder = try MultipartEncoder(ur, 100)
        let decoder = MultipartDecoder()

        while !decoder.isComplete {
            let upper = try encoder.nextPart().uppercased()
            try decoder.receive(upper)
        }

        #expect(try decoder.message() == ur)
    }
}
