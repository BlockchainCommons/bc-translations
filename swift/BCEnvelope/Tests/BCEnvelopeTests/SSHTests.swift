import Testing
import BCComponents
import BCEnvelope
import Foundation

struct SSHTests {
    @Test func testSSHSignedPlaintext() throws {
        let aliceSSHPrivateKey = try alicePrivateKeys
            .sshSigningPrivateKey(.ed25519, comment: "alice@example.com")
        let aliceSSHPublicKey = try aliceSSHPrivateKey.publicKey()

        let options = SigningOptions.ssh(namespace: "test", hashAlg: .sha256)
        let envelope = try Envelope(plaintextHello)
            .addSignatureOpt(aliceSSHPrivateKey, options: options, metadata: nil)
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
            .verifySignatureFrom(aliceSSHPublicKey)
            .extractSubject(String.self)
        #expect(receivedPlaintext == plaintextHello)

        let carolSSHPublicKey = try carolPrivateKeys
            .sshSigningPrivateKey(.ed25519, comment: "carol@example.com")
            .publicKey()

        #expect(throws: (any Swift.Error).self) {
            try receivedEnvelope.verifySignatureFrom(carolSSHPublicKey)
        }

        let verifiers: [any Verifier] = [aliceSSHPublicKey, carolSSHPublicKey]
        try receivedEnvelope.verifySignaturesFromThreshold(verifiers, threshold: 1)

        #expect(throws: (any Swift.Error).self) {
            try receivedEnvelope.verifySignaturesFromThreshold(verifiers, threshold: 2)
        }
    }
}
