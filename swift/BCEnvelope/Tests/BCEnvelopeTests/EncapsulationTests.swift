import Testing
import BCComponents
import BCEnvelope
import Foundation

struct EncapsulationTests {
    private func testScheme(_ scheme: EncapsulationScheme) throws {
        let (privateKey, publicKey) = scheme.keypair()
        let envelope = Envelope(plaintextHello)

        let encryptedEnvelope = try envelope
            .encryptToRecipient(publicKey)
            .checkEncoding()

        let decryptedEnvelope = try encryptedEnvelope.decryptToRecipient(privateKey)
        #expect(envelope.structuralDigest == decryptedEnvelope.structuralDigest)
    }

    @Test func testEncapsulation() throws {
        try testScheme(.x25519)
        try testScheme(.mlkem512)
        try testScheme(.mlkem768)
        try testScheme(.mlkem1024)
    }
}
