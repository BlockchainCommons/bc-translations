import Testing
import BCComponents
import BCEnvelope
import Foundation

struct MultiPermitTests {
    @Test func testMultiPermit() throws {
        let poemText = "At midnight, the clocks sang lullabies to the wandering teacups."

        let originalEnvelope = Envelope(poemText)
            .addType(Envelope("poem"))
            .addAssertion("title", "A Song of Ice Cream")
            .addAssertion("author", "Plonkus the Iridescent")
            .addAssertion(.date, try Date(iso8601: "2025-05-15"))

        let (alicePrivateKeys, alicePublicKeys) = keypair()
        let signedEnvelope = originalEnvelope.sign(alicePrivateKeys)

        #expect(signedEnvelope.format() ==
        """
        {
            "At midnight, the clocks sang lullabies to the wandering teacups." [
                'isA': "poem"
                "author": "Plonkus the Iridescent"
                "title": "A Song of Ice Cream"
                'date': 2025-05-15
            ]
        } [
            'signed': Signature
        ]
        """)

        let contentKey = SymmetricKey()
        let encryptedEnvelope = signedEnvelope.encrypt(contentKey)

        #expect(encryptedEnvelope.format() ==
        """
        ENCRYPTED
        """)

        let password = Data("unicorns_dance_on_mars_while_eating_pizza".utf8)
        let withSecret = try encryptedEnvelope.addSecret(
            method: .argon2id,
            secret: password,
            contentKey: contentKey
        )

        #expect(withSecret.format().contains("'hasSecret':"))

        let (bobPrivateKeys, bobPublicKeys) = keypair()
        let withRecipients = withSecret
            .addRecipient(alicePublicKeys, contentKey: contentKey)
            .addRecipient(bobPublicKeys, contentKey: contentKey)

        #expect(withRecipients.format().contains("'hasRecipient': SealedMessage"))
        #expect(withRecipients.format().contains("'hasSecret':"))

        let sskrGroup = try SSKRGroupSpec(memberThreshold: 2, memberCount: 3)
        let spec = try SSKRSpec(groupThreshold: 1, groups: [sskrGroup])
        let shardedEnvelopes = try withRecipients.sskrSplitFlattened(spec, contentKey)

        #expect(shardedEnvelopes[0].format().contains("'hasRecipient': SealedMessage"))
        #expect(shardedEnvelopes[0].format().contains("'hasSecret':"))
        #expect(shardedEnvelopes[0].format().contains("'sskrShare': SSKRShare"))

        let receivedEnvelope = shardedEnvelopes[0]

        let unlockedWithContentKey = try receivedEnvelope.decrypt(contentKey)
        #expect(unlockedWithContentKey.isIdentical(to: signedEnvelope))

        let unlockedWithPassword = try receivedEnvelope.unlock(secret: password)
        #expect(unlockedWithPassword.isIdentical(to: signedEnvelope))

        let unlockedWithAlice = try receivedEnvelope.decryptToRecipient(alicePrivateKeys)
        #expect(unlockedWithAlice.isIdentical(to: signedEnvelope))

        let unlockedWithBob = try receivedEnvelope.decryptToRecipient(bobPrivateKeys)
        #expect(unlockedWithBob.isIdentical(to: signedEnvelope))

        let unlockedWithSSKR = try Envelope
            .sskrJoin([shardedEnvelopes[0], shardedEnvelopes[2]])
            .unwrap()
        #expect(unlockedWithSSKR.isIdentical(to: signedEnvelope))

        _ = try unlockedWithSSKR.verify(alicePublicKeys)
    }
}
