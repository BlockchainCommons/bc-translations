import Testing
import BCComponents
import BCEnvelope
import Foundation

struct SignatureTests {
    @Test func testSignedPlaintext() throws {
        let envelope = try Envelope(plaintextHello)
            .addSignature(alicePrivateKeys)
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
            .verifySignatureFrom(alicePublicKeys)
            .extractSubject(String.self)
        #expect(receivedPlaintext == plaintextHello)

        #expect(throws: (any Swift.Error).self) {
            try receivedEnvelope.verifySignatureFrom(carolPublicKeys)
        }

        let signers: [any Verifier] = [alicePublicKeys, carolPublicKeys]
        try receivedEnvelope.verifySignaturesFromThreshold(signers, threshold: 1)

        #expect(throws: (any Swift.Error).self) {
            try receivedEnvelope.verifySignaturesFromThreshold(signers, threshold: 2)
        }
    }

    @Test func testMultisignedPlaintext() throws {
        let signers: [any Signer] = [alicePrivateKeys, carolPrivateKeys]
        let verifiers: [any Verifier] = [alicePublicKeys, carolPublicKeys]

        let envelope = try Envelope(plaintextHello)
            .addSignatures(signers)
            .checkEncoding()
        let ur = envelope.ur

        #expect(envelope.format() ==
        """
        "Hello." [
            'signed': Signature
            'signed': Signature
        ]
        """)

        let receivedPlaintext = try Envelope(ur: ur)
            .checkEncoding()
            .verifySignaturesFrom(verifiers)
            .extractSubject(String.self)
        #expect(receivedPlaintext == plaintextHello)
    }

    @Test func testSignedWithMetadata() throws {
        let metadata = SignatureMetadata()
            .withAssertion(KnownValue.note, "Alice signed this.")

        let envelope = try Envelope(plaintextHello)
            .wrap()
            .addSignatureOpt(alicePrivateKeys, options: nil, metadata: metadata)
            .checkEncoding()
        let ur = envelope.ur

        #expect(envelope.format() ==
        """
        {
            "Hello."
        } [
            'signed': {
                Signature [
                    'note': "Alice signed this."
                ]
            } [
                'signed': Signature
            ]
        ]
        """)

        let (receivedPlaintextEnvelope, metadataEnvelope) = try Envelope(ur: ur)
            .checkEncoding()
            .verifyReturningMetadata(alicePublicKeys)

        #expect(metadataEnvelope.format() ==
        """
        Signature [
            'note': "Alice signed this."
        ]
        """)

        let note = try metadataEnvelope.extractObject(String.self, forPredicate: .note)
        #expect(note == "Alice signed this.")

        let receivedPlaintext = try receivedPlaintextEnvelope.extractSubject(String.self)
        #expect(receivedPlaintext == plaintextHello)
    }
}
