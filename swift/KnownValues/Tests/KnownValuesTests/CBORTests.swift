import DCBOR
import BCTags
import Testing
@testable import KnownValues

@Suite("CBOR Tests")
struct CBORTests {
    @Test func testKnownValueCBORRoundTrip() throws {
        let kv = KnownValue(value: 1, name: "isA")
        let cbor = kv.taggedCBOR
        let decoded = try KnownValue(cbor: cbor)
        #expect(decoded.value == 1)
        #expect(decoded.assignedName == nil)
    }

    @Test func testKnownValueCBOREncoding() {
        let kv = KnownValue(42)
        let cbor = kv.untaggedCBOR
        #expect(cbor == CBOR.unsigned(42))
    }

    @Test func testKnownValueTaggedCBOR() {
        let tagged = KnownValue.isA.taggedCBOR
        if case .tagged(let tag, let inner) = tagged {
            #expect(tag == .knownValue)
            #expect(inner == CBOR.unsigned(1))
        } else {
            Issue.record("Expected tagged CBOR")
        }
    }

    @Test func testCBORTags() {
        #expect(KnownValue.cborTags == [.knownValue])
    }

    @Test func testCBORDataRoundTrip() throws {
        let original = KnownValue(value: 42, name: "answer")
        let data = original.taggedCBOR.cborData
        let cbor = try CBOR(data)
        let decoded = try KnownValue(cbor: cbor)
        #expect(decoded.value == 42)
    }

    @Test func testDigestProvider() {
        let kv1 = KnownValue.isA
        let kv2 = KnownValue(1)
        #expect(kv1.digest() == kv2.digest())

        let kv3 = KnownValue.note
        #expect(kv1.digest() != kv3.digest())
    }
}
