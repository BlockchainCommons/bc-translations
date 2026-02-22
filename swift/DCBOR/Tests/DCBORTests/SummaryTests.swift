import DCBOR
import Testing

struct SummaryTests {
    @Test
    @MainActor
    func usesTagSummarizerInSummary() throws {
        let customTag = Tag(987654321, "custom-summary")
        globalTags.setSummarizer(customTag) { payload, _ in
            guard let cbor = payload as? CBOR else {
                throw TestError.invalidPayload
            }
            return "S(\(cbor.diagnosticFlat))"
        }

        let value = CBOR.tagged(customTag, "hello")
        #expect(value.summary == #"S("hello")"#)
    }

    @Test
    @MainActor
    func reportsSummarizerErrorsInSummary() {
        let errorTag = Tag(987654322, "custom-summary-error")
        globalTags.setSummarizer(errorTag) { _, _ in
            throw TestError.expectedFailure
        }

        let value = CBOR.tagged(errorTag, "hello")
        #expect(value.summary.contains("<error:"))
    }
}

private enum TestError: Error {
    case invalidPayload
    case expectedFailure
}
