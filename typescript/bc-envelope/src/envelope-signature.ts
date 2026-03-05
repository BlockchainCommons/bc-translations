import {
    type Signer,
    type Signature,
    type SigningOptions,
    type Verifier,
} from '@bc/components';
import { SIGNED } from '@bc/known-values';

import { asEnvelope } from './envelope-encodable.js';
import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';
import { Assertion } from './assertion.js';

export class SignatureMetadata {
    readonly #assertions: Assertion[] = [];

    assertions(): Assertion[] {
        return [...this.#assertions];
    }

    hasAssertions(): boolean {
        return this.#assertions.length > 0;
    }

    addAssertion(assertion: Assertion): SignatureMetadata {
        this.#assertions.push(assertion);
        return this;
    }

    withAssertion(predicate: unknown, objectValue: unknown): SignatureMetadata {
        this.#assertions.push(Assertion.create(predicate, objectValue));
        return this;
    }
}

function isSignatureFromKey(envelope: Envelope, signature: Signature, key: Verifier): boolean {
    return key.verify(signature, envelope.subject().digest().data);
}

function hasSomeSignatureFromKeyReturningMetadata(envelope: Envelope, key: Verifier): Envelope | undefined {
    const signatureObjects = envelope.objectsForPredicate(SIGNED);
    for (const signatureObject of signatureObjects) {
        const signatureObjectSubject = signatureObject.subject();

        if (signatureObjectSubject.isWrapped()) {
            const outerSignatureObject = signatureObject.objectForPredicate(SIGNED);
            const outerSignature = outerSignatureObject.extractSubject<Signature>();

            if (!isSignatureFromKey(signatureObjectSubject, outerSignature, key)) {
                continue;
            }

            const signatureMetadataEnvelope = signatureObjectSubject.unwrap();
            const signature = signatureMetadataEnvelope.extractSubject<Signature>();
            if (!isSignatureFromKey(envelope, signature, key)) {
                throw EnvelopeError.unverifiedSignature();
            }
            return signatureMetadataEnvelope;
        }

        const signature = signatureObject.extractSubject<Signature>();
        if (isSignatureFromKey(envelope, signature, key)) {
            return signatureObject;
        }
    }

    return undefined;
}

export function addSignature(envelope: Envelope, signer: Signer): Envelope {
    return addSignatureOpt(envelope, signer);
}

export function addSignatureOpt(
    envelope: Envelope,
    signer: Signer,
    options?: SigningOptions,
    metadata?: SignatureMetadata,
): Envelope {
    const digest = envelope.subject().digest().data;
    let signature = asEnvelope(signer.signWithOptions(digest, options));

    if (metadata != null && metadata.hasAssertions()) {
        let signatureWithMetadata = signature;
        for (const assertion of metadata.assertions()) {
            signatureWithMetadata = signatureWithMetadata.addAssertionEnvelope(assertion.toEnvelope());
        }

        signatureWithMetadata = signatureWithMetadata.wrap();
        const outerSignature = asEnvelope(
            signer.signWithOptions(signatureWithMetadata.digest().data, options),
        );
        signature = signatureWithMetadata.addAssertion(SIGNED, outerSignature);
    }

    return envelope.addAssertion(SIGNED, signature);
}

export function addSignatures(envelope: Envelope, signers: Signer[]): Envelope {
    return signers.reduce((current, signer) => current.addSignature(signer), envelope);
}

export function hasSignatureFrom(envelope: Envelope, verifier: Verifier): boolean {
    return hasSomeSignatureFromKeyReturningMetadata(envelope, verifier) != null;
}

export function verifySignature(envelope: Envelope, verifier: Verifier): void {
    if (!hasSignatureFrom(envelope, verifier)) {
        throw EnvelopeError.unverifiedSignature();
    }
}

export function verifySignatureFrom(envelope: Envelope, verifier: Verifier): Envelope {
    verifySignature(envelope, verifier);
    return envelope;
}

export function verifySignatureFromReturningMetadata(envelope: Envelope, verifier: Verifier): Envelope {
    const metadata = hasSomeSignatureFromKeyReturningMetadata(envelope, verifier);
    if (metadata === undefined) throw EnvelopeError.unverifiedSignature();
    return metadata;
}

export function hasSignaturesFromThreshold(
    envelope: Envelope,
    verifiers: Verifier[],
    threshold?: number,
): boolean {
    const required = threshold ?? verifiers.length;
    let count = 0;
    for (const verifier of verifiers) {
        if (hasSomeSignatureFromKeyReturningMetadata(envelope, verifier) != null) {
            count += 1;
            if (count >= required) {
                return true;
            }
        }
    }
    return false;
}

export function hasSignaturesFrom(envelope: Envelope, verifiers: Verifier[]): boolean {
    return hasSignaturesFromThreshold(envelope, verifiers);
}

export function verifySignaturesFromThreshold(
    envelope: Envelope,
    verifiers: Verifier[],
    threshold?: number,
): Envelope {
    if (!hasSignaturesFromThreshold(envelope, verifiers, threshold)) {
        throw EnvelopeError.unverifiedSignature();
    }
    return envelope;
}

export function verifySignaturesFrom(envelope: Envelope, verifiers: Verifier[]): Envelope {
    return verifySignaturesFromThreshold(envelope, verifiers);
}

export function sign(envelope: Envelope, signer: Signer): Envelope {
    return signOpt(envelope, signer);
}

export function signOpt(envelope: Envelope, signer: Signer, options?: SigningOptions): Envelope {
    return envelope.wrap().addSignatureOpt(signer, options);
}

export function verify(envelope: Envelope, verifier: Verifier): Envelope {
    return envelope.verifySignatureFrom(verifier).unwrap();
}

export function verifyReturningMetadata(envelope: Envelope, verifier: Verifier): [Envelope, Envelope] {
    const metadata = envelope.verifySignatureFromReturningMetadata(verifier);
    return [envelope.unwrap(), metadata];
}

declare module './envelope.js' {
    interface Envelope {
        addSignature(signer: Signer): Envelope;
        addSignatureOpt(signer: Signer, options?: SigningOptions, metadata?: SignatureMetadata): Envelope;
        addSignatures(signers: Signer[]): Envelope;
        hasSignatureFrom(verifier: Verifier): boolean;
        verifySignature(verifier: Verifier): void;
        verifySignatureFrom(verifier: Verifier): Envelope;
        verifySignatureFromReturningMetadata(verifier: Verifier): Envelope;
        hasSignaturesFromThreshold(verifiers: Verifier[], threshold?: number): boolean;
        hasSignaturesFrom(verifiers: Verifier[]): boolean;
        verifySignaturesFromThreshold(verifiers: Verifier[], threshold?: number): Envelope;
        verifySignaturesFrom(verifiers: Verifier[]): Envelope;
        sign(signer: Signer): Envelope;
        signOpt(signer: Signer, options?: SigningOptions): Envelope;
        verify(verifier: Verifier): Envelope;
        verifyReturningMetadata(verifier: Verifier): [Envelope, Envelope];
    }
}

Envelope.prototype.addSignature = function addSignatureProto(this: Envelope, signer: Signer): Envelope {
    return addSignature(this, signer);
};
Envelope.prototype.addSignatureOpt = function addSignatureOptProto(
    this: Envelope,
    signer: Signer,
    options?: SigningOptions,
    metadata?: SignatureMetadata,
): Envelope {
    return addSignatureOpt(this, signer, options, metadata);
};
Envelope.prototype.addSignatures = function addSignaturesProto(this: Envelope, signers: Signer[]): Envelope {
    return addSignatures(this, signers);
};
Envelope.prototype.hasSignatureFrom = function hasSignatureFromProto(this: Envelope, verifier: Verifier): boolean {
    return hasSignatureFrom(this, verifier);
};
Envelope.prototype.verifySignature = function verifySignatureProto(this: Envelope, verifier: Verifier): void {
    verifySignature(this, verifier);
};
Envelope.prototype.verifySignatureFrom = function verifySignatureFromProto(this: Envelope, verifier: Verifier): Envelope {
    return verifySignatureFrom(this, verifier);
};
Envelope.prototype.verifySignatureFromReturningMetadata = function verifySignatureFromReturningMetadataProto(
    this: Envelope,
    verifier: Verifier,
): Envelope {
    return verifySignatureFromReturningMetadata(this, verifier);
};
Envelope.prototype.hasSignaturesFromThreshold = function hasSignaturesFromThresholdProto(
    this: Envelope,
    verifiers: Verifier[],
    threshold?: number,
): boolean {
    return hasSignaturesFromThreshold(this, verifiers, threshold);
};
Envelope.prototype.hasSignaturesFrom = function hasSignaturesFromProto(this: Envelope, verifiers: Verifier[]): boolean {
    return hasSignaturesFrom(this, verifiers);
};
Envelope.prototype.verifySignaturesFromThreshold = function verifySignaturesFromThresholdProto(
    this: Envelope,
    verifiers: Verifier[],
    threshold?: number,
): Envelope {
    return verifySignaturesFromThreshold(this, verifiers, threshold);
};
Envelope.prototype.verifySignaturesFrom = function verifySignaturesFromProto(this: Envelope, verifiers: Verifier[]): Envelope {
    return verifySignaturesFrom(this, verifiers);
};
Envelope.prototype.sign = function signProto(this: Envelope, signer: Signer): Envelope {
    return sign(this, signer);
};
Envelope.prototype.signOpt = function signOptProto(this: Envelope, signer: Signer, options?: SigningOptions): Envelope {
    return signOpt(this, signer, options);
};
Envelope.prototype.verify = function verifyProto(this: Envelope, verifier: Verifier): Envelope {
    return verify(this, verifier);
};
Envelope.prototype.verifyReturningMetadata = function verifyReturningMetadataProto(
    this: Envelope,
    verifier: Verifier,
): [Envelope, Envelope] {
    return verifyReturningMetadata(this, verifier);
};
