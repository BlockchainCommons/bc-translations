import Testing
import DCBOR
@testable import BCUR

private struct TestLeaf: URCodable, Equatable {
    static let cborTags: [BCTags.Tag] = [BCTags.Tag(24, "leaf")]

    let value: String

    init(_ value: String) {
        self.value = value
    }

    var untaggedCBOR: CBOR {
        value.cbor
    }

    init(untaggedCBOR: CBOR) throws {
        self.value = try String(cbor: untaggedCBOR)
    }
}

struct URCodableTests {
    @Test func testUrCodable() throws {
        let test = TestLeaf("test")
        let ur = test.ur()
        #expect(ur.urString == "ur:leaf/iejyihjkjygupyltla")

        let decoded = try TestLeaf.fromURString(ur.urString)
        #expect(decoded == test)
    }
}
