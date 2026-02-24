import Testing
import BCComponents
import BCEnvelope
import WolfBase
import Foundation

struct SSKRTests {
    @Test func testSSKR() throws {
        var danSeed = Seed(data: ‡"59f2293a5bce7d4de59e71b4207ac5d2")!
        danSeed.name = "Dark Purple Aqua Love"
        danSeed.creationDate = try Date(iso8601: "2021-02-24")
        danSeed.note = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."

        let contentKey = SymmetricKey()
        let seedEnvelope = danSeed.envelope
        let encryptedSeedEnvelope = try seedEnvelope.wrap().encryptSubject(with: contentKey)

        let group = try SSKRGroupSpec(memberThreshold: 2, memberCount: 3)
        let spec = try SSKRSpec(groupThreshold: 1, groups: [group])
        let envelopes = try encryptedSeedEnvelope.sskrSplit(spec, contentKey)

        let sentEnvelopes = envelopes.flatMap { $0 }

        #expect(sentEnvelopes[0].format() ==
        """
        ENCRYPTED [
            'sskrShare': SSKRShare
        ]
        """)

        let bobEnvelope = sentEnvelopes[1]
        let carolEnvelope = sentEnvelopes[2]
        let recoveredSeedEnvelope = try Envelope
            .sskrJoin([bobEnvelope, carolEnvelope])
            .unwrap()

        let recoveredSeed = try Seed(recoveredSeedEnvelope)
        #expect(danSeed.data == recoveredSeed.data)
        #expect(danSeed.creationDate == recoveredSeed.creationDate)
        #expect(danSeed.name == recoveredSeed.name)
        #expect(danSeed.note == recoveredSeed.note)

        #expect(throws: (any Swift.Error).self) {
            _ = try Envelope.sskrJoin([bobEnvelope])
        }
    }
}
