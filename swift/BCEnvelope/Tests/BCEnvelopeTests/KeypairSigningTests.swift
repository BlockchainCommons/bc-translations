import Testing
import BCComponents
import BCEnvelope
import Foundation

struct KeypairSigningTests {
    private func testScheme(
        _ scheme: SignatureScheme,
        options: SigningOptions? = nil
    ) throws {
        let (privateKey, publicKey) = scheme.keypair()
        let envelope = try Envelope(plaintextHello)
            .signOpt(privateKey, options)
            .checkEncoding()
        _ = try envelope.verify(publicKey)
    }

    @Test func testKeypairSigning() throws {
        try testScheme(.schnorr)
        try testScheme(.ecdsa)
        try testScheme(.ed25519)
        try testScheme(.mldsa44)
        try testScheme(.mldsa65)
        try testScheme(.mldsa87)
    }

    @Test func testKeypairSigningSSH() throws {
        let options = SigningOptions.ssh(namespace: "test", hashAlg: .sha512)
        try testScheme(.sshEd25519, options: options)
        try testScheme(.sshDsa, options: options)
        try testScheme(.sshEcdsaP256, options: options)
        try testScheme(.sshEcdsaP384, options: options)
    }
}
