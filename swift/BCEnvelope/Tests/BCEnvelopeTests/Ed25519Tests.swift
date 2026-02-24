import Testing
import BCComponents
import BCEnvelope
import Foundation

struct Ed25519Tests {
    @Test func testEd25519SignedPlaintext() throws {
        let alicePrivateKey = alicePrivateKeys.ed25519SigningPrivateKey()
        let alicePublicKey = try alicePrivateKey.publicKey()

        let envelope = try Envelope(plaintextHello)
            .addSignature(alicePrivateKey)
            .checkEncoding()
        let ur = envelope.ur

        #expect(envelope.format() ==
        """
        "Hello." [
            'signed': Signature
        ]
        """)

        let receivedEnvelope = try Envelope(ur: ur).checkEncoding()
        let receivedPlaintext = try receivedEnvelope
            .verifySignatureFrom(alicePublicKey)
            .extractSubject(String.self)
        #expect(receivedPlaintext == plaintextHello)

        let carolPublicKey = try carolPrivateKeys
            .ed25519SigningPrivateKey()
            .publicKey()

        #expect(throws: (any Swift.Error).self) {
            try receivedEnvelope.verifySignatureFrom(carolPublicKey)
        }

        let verifiers: [any Verifier] = [alicePublicKey, carolPublicKey]
        try receivedEnvelope.verifySignaturesFromThreshold(verifiers, threshold: 1)

        #expect(throws: (any Swift.Error).self) {
            try receivedEnvelope.verifySignaturesFromThreshold(verifiers, threshold: 2)
        }
    }
}
