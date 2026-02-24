import Testing
import BCComponents
import BCEnvelope
import Foundation

struct SealTests {
    @Test func testSealAndUnseal() throws {
        let message = "Top secret message"
        let originalEnvelope = Envelope(message)

        let (senderPrivate, senderPublic) = SignatureScheme.ed25519.keypair()
        let (recipientPrivate, recipientPublic) = EncapsulationScheme.x25519.keypair()

        let sealedEnvelope = originalEnvelope.seal(senderPrivate, recipientPublic)
        #expect(sealedEnvelope.isSubjectEncrypted)

        let unsealedEnvelope = try sealedEnvelope.unseal(senderPublic, recipientPrivate)
        let extractedMessage = try unsealedEnvelope.extractSubject(String.self)
        #expect(extractedMessage == message)
    }

    @Test func testSealOptWithOptions() throws {
        let message = "Confidential data"
        let originalEnvelope = Envelope(message)

        let (senderPrivate, senderPublic) = SignatureScheme.ed25519.keypair()
        let (recipientPrivate, recipientPublic) = EncapsulationScheme.x25519.keypair()

        let options = SigningOptions.ssh(namespace: "test", hashAlg: .sha512)
        let sealedEnvelope = originalEnvelope.sealOpt(
            senderPrivate,
            recipientPublic,
            options
        )
        #expect(sealedEnvelope.isSubjectEncrypted)

        let unsealedEnvelope = try sealedEnvelope.unseal(senderPublic, recipientPrivate)
        let extractedMessage = try unsealedEnvelope.extractSubject(String.self)
        #expect(extractedMessage == message)
    }
}
