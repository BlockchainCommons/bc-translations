import Foundation
import BCComponents

extension EnvelopeError {
    static let invalidOuterSignatureType = EnvelopeError("invalidOuterSignatureType")
    static let invalidInnerSignatureType = EnvelopeError("invalidInnerSignatureType")
    static let unverifiedInnerSignature = EnvelopeError("unverifiedInnerSignature")
    static let invalidSignatureType = EnvelopeError("invalidSignatureType")
}

public extension Envelope {
    func addSignature(_ privateKey: any Signer) -> Envelope {
        addSignatureOpt(privateKey, options: nil, metadata: nil)
    }

    func addSignatureOpt(
        _ privateKey: any Signer,
        options: SigningOptions? = nil,
        metadata: SignatureMetadata? = nil
    ) -> Envelope {
        let digestData = subject.digest.data
        var signature = Envelope(try! privateKey.signWithOptions(digestData, options: options))

        if let metadata, metadata.hasAssertions {
            var signatureWithMetadata = signature
            for assertion in metadata.assertions {
                signatureWithMetadata = try! signatureWithMetadata.addAssertion(assertion.envelope)
            }
            signatureWithMetadata = signatureWithMetadata.wrap()

            let outerSignature = Envelope(
                try! privateKey.signWithOptions(
                    signatureWithMetadata.digest.data,
                    options: options
                )
            )
            signature = signatureWithMetadata.addAssertion(.signed, outerSignature)
        }

        return addAssertion(.signed, signature)
    }

    func addSignatures(_ privateKeys: [any Signer]) -> Envelope {
        privateKeys.reduce(self) { partial, privateKey in
            partial.addSignature(privateKey)
        }
    }

    func addSignaturesOpt(
        _ privateKeys: [(any Signer, SigningOptions?, SignatureMetadata?)]
    ) -> Envelope {
        privateKeys.reduce(self) { partial, tuple in
            partial.addSignatureOpt(tuple.0, options: tuple.1, metadata: tuple.2)
        }
    }

    func hasSignatureFrom(_ publicKey: any Verifier) throws -> Bool {
        try hasSomeSignatureFromKey(publicKey)
    }

    func hasSignatureFromReturningMetadata(
        _ publicKey: any Verifier
    ) throws -> Envelope? {
        try hasSomeSignatureFromKeyReturningMetadata(publicKey)
    }

    @discardableResult
    func verifySignatureFrom(_ publicKey: any Verifier) throws -> Envelope {
        guard try hasSomeSignatureFromKey(publicKey) else {
            throw EnvelopeError.unverifiedSignature
        }
        return self
    }

    func verifySignatureFromReturningMetadata(
        _ publicKey: any Verifier
    ) throws -> Envelope {
        guard let metadata = try hasSomeSignatureFromKeyReturningMetadata(publicKey) else {
            throw EnvelopeError.unverifiedSignature
        }
        return metadata
    }

    func hasSignaturesFrom(_ publicKeys: [any Verifier]) throws -> Bool {
        try hasSignaturesFromThreshold(publicKeys, threshold: nil)
    }

    func hasSignaturesFromThreshold(
        _ publicKeys: [any Verifier],
        threshold: Int? = nil
    ) throws -> Bool {
        let threshold = threshold ?? publicKeys.count
        var count = 0
        for key in publicKeys {
            if try hasSomeSignatureFromKey(key) {
                count += 1
                if count >= threshold {
                    return true
                }
            }
        }
        return false
    }

    @discardableResult
    func verifySignaturesFromThreshold(
        _ publicKeys: [any Verifier],
        threshold: Int? = nil
    ) throws -> Envelope {
        guard try hasSignaturesFromThreshold(publicKeys, threshold: threshold) else {
            throw EnvelopeError.unverifiedSignature
        }
        return self
    }

    @discardableResult
    func verifySignaturesFrom(_ publicKeys: [any Verifier]) throws -> Envelope {
        try verifySignaturesFromThreshold(publicKeys, threshold: nil)
    }
}

private extension Envelope {
    func isSignatureFromKey(
        _ signature: Signature,
        key: any Verifier
    ) -> Bool {
        key.verify(signature, subject.digest.data)
    }

    func hasSomeSignatureFromKey(_ key: any Verifier) throws -> Bool {
        try hasSomeSignatureFromKeyReturningMetadata(key) != nil
    }

    func hasSomeSignatureFromKeyReturningMetadata(
        _ key: any Verifier
    ) throws -> Envelope? {
        let signatureObjects = objects(forPredicate: .signed)

        for signatureObject in signatureObjects {
            let signatureObjectSubject = signatureObject.subject
            if signatureObjectSubject.isWrapped {
                if let outerSignatureObject = try? signatureObject.object(forPredicate: .signed) {
                    guard let outerSignature = try? outerSignatureObject.extractSubject(Signature.self) else {
                        throw EnvelopeError.invalidOuterSignatureType
                    }
                    guard signatureObjectSubject.isSignatureFromKey(outerSignature, key: key) else {
                        continue
                    }
                }

                let signatureMetadataEnvelope = try signatureObjectSubject.unwrap()
                guard let signature = try? signatureMetadataEnvelope.extractSubject(Signature.self) else {
                    throw EnvelopeError.invalidInnerSignatureType
                }

                guard subject.isSignatureFromKey(signature, key: key) else {
                    throw EnvelopeError.unverifiedInnerSignature
                }

                return signatureMetadataEnvelope
            } else if let signature = try? signatureObject.extractSubject(Signature.self) {
                guard isSignatureFromKey(signature, key: key) else {
                    continue
                }
                return signatureObject
            } else {
                throw EnvelopeError.invalidSignatureType
            }
        }

        return nil
    }
}

public extension Envelope {
    func sign(_ signer: any Signer) -> Envelope {
        signOpt(signer, nil)
    }

    func signOpt(
        _ signer: any Signer,
        _ options: SigningOptions?
    ) -> Envelope {
        wrap().addSignatureOpt(signer, options: options, metadata: nil)
    }

    func verify(_ verifier: any Verifier) throws -> Envelope {
        try verifySignatureFrom(verifier).unwrap()
    }

    func verifyReturningMetadata(
        _ verifier: any Verifier
    ) throws -> (Envelope, Envelope) {
        let metadata = try verifySignatureFromReturningMetadata(verifier)
        return (try unwrap(), metadata)
    }
}
