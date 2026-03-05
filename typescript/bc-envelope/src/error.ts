export type EnvelopeErrorCode =
    | 'already-elided'
    | 'ambiguous-predicate'
    | 'invalid-digest'
    | 'invalid-format'
    | 'missing-digest'
    | 'nonexistent-predicate'
    | 'not-wrapped'
    | 'not-leaf'
    | 'not-assertion'
    | 'invalid-assertion'
    | 'invalid-attachment'
    | 'nonexistent-attachment'
    | 'ambiguous-attachment'
    | 'edge-missing-isa'
    | 'edge-missing-source'
    | 'edge-missing-target'
    | 'edge-duplicate-isa'
    | 'edge-duplicate-source'
    | 'edge-duplicate-target'
    | 'edge-unexpected-assertion'
    | 'nonexistent-edge'
    | 'ambiguous-edge'
    | 'already-compressed'
    | 'not-compressed'
    | 'already-encrypted'
    | 'not-encrypted'
    | 'not-known-value'
    | 'unknown-recipient'
    | 'unknown-secret'
    | 'unverified-signature'
    | 'invalid-outer-signature-type'
    | 'invalid-inner-signature-type'
    | 'unverified-inner-signature'
    | 'invalid-signature-type'
    | 'invalid-shares'
    | 'invalid-type'
    | 'ambiguous-type'
    | 'subject-not-unit'
    | 'unexpected-response-id'
    | 'invalid-response'
    | 'sskr'
    | 'cbor'
    | 'components'
    | 'general';

/** `@bc/envelope` package error. */
export class EnvelopeError extends Error {
    readonly code: EnvelopeErrorCode;

    constructor(code: EnvelopeErrorCode, message: string) {
        super(message);
        this.name = 'EnvelopeError';
        this.code = code;
    }

    static alreadyElided(): EnvelopeError {
        return new EnvelopeError(
            'already-elided',
            'envelope was elided, so it cannot be compressed or encrypted',
        );
    }

    static ambiguousPredicate(): EnvelopeError {
        return new EnvelopeError('ambiguous-predicate', 'more than one assertion matches the predicate');
    }

    static invalidDigest(): EnvelopeError {
        return new EnvelopeError('invalid-digest', 'digest did not match');
    }

    static invalidFormat(message = 'invalid format'): EnvelopeError {
        return new EnvelopeError('invalid-format', message);
    }

    static missingDigest(): EnvelopeError {
        return new EnvelopeError('missing-digest', 'a digest was expected but not found');
    }

    static nonexistentPredicate(): EnvelopeError {
        return new EnvelopeError('nonexistent-predicate', 'no assertion matches the predicate');
    }

    static notWrapped(): EnvelopeError {
        return new EnvelopeError('not-wrapped', 'cannot unwrap an envelope that was not wrapped');
    }

    static notLeaf(): EnvelopeError {
        return new EnvelopeError('not-leaf', "the envelope's subject is not a leaf");
    }

    static notAssertion(): EnvelopeError {
        return new EnvelopeError('not-assertion', "the envelope's subject is not an assertion");
    }

    static invalidAssertion(): EnvelopeError {
        return new EnvelopeError('invalid-assertion', 'assertion must be a map with exactly one element');
    }

    static invalidAttachment(): EnvelopeError {
        return new EnvelopeError('invalid-attachment', 'invalid attachment');
    }

    static nonexistentAttachment(): EnvelopeError {
        return new EnvelopeError('nonexistent-attachment', 'nonexistent attachment');
    }

    static ambiguousAttachment(): EnvelopeError {
        return new EnvelopeError('ambiguous-attachment', 'ambiguous attachment');
    }

    static edgeMissingIsA(): EnvelopeError {
        return new EnvelopeError('edge-missing-isa', "edge missing 'isA' assertion");
    }

    static edgeMissingSource(): EnvelopeError {
        return new EnvelopeError('edge-missing-source', "edge missing 'source' assertion");
    }

    static edgeMissingTarget(): EnvelopeError {
        return new EnvelopeError('edge-missing-target', "edge missing 'target' assertion");
    }

    static edgeDuplicateIsA(): EnvelopeError {
        return new EnvelopeError('edge-duplicate-isa', "edge has duplicate 'isA' assertions");
    }

    static edgeDuplicateSource(): EnvelopeError {
        return new EnvelopeError('edge-duplicate-source', "edge has duplicate 'source' assertions");
    }

    static edgeDuplicateTarget(): EnvelopeError {
        return new EnvelopeError('edge-duplicate-target', "edge has duplicate 'target' assertions");
    }

    static edgeUnexpectedAssertion(): EnvelopeError {
        return new EnvelopeError('edge-unexpected-assertion', 'edge has unexpected assertion');
    }

    static nonexistentEdge(): EnvelopeError {
        return new EnvelopeError('nonexistent-edge', 'nonexistent edge');
    }

    static ambiguousEdge(): EnvelopeError {
        return new EnvelopeError('ambiguous-edge', 'ambiguous edge');
    }

    static alreadyCompressed(): EnvelopeError {
        return new EnvelopeError('already-compressed', 'envelope was already compressed');
    }

    static notCompressed(): EnvelopeError {
        return new EnvelopeError('not-compressed', 'cannot decompress an envelope that was not compressed');
    }

    static alreadyEncrypted(): EnvelopeError {
        return new EnvelopeError(
            'already-encrypted',
            'envelope was already encrypted or compressed, so it cannot be encrypted',
        );
    }

    static notEncrypted(): EnvelopeError {
        return new EnvelopeError('not-encrypted', 'cannot decrypt an envelope that was not encrypted');
    }

    static notKnownValue(): EnvelopeError {
        return new EnvelopeError('not-known-value', "the envelope's subject is not a known value");
    }

    static unknownRecipient(): EnvelopeError {
        return new EnvelopeError('unknown-recipient', 'unknown recipient');
    }

    static unknownSecret(): EnvelopeError {
        return new EnvelopeError('unknown-secret', 'secret not found');
    }

    static unverifiedSignature(): EnvelopeError {
        return new EnvelopeError('unverified-signature', 'could not verify a signature');
    }

    static invalidOuterSignatureType(): EnvelopeError {
        return new EnvelopeError('invalid-outer-signature-type', 'unexpected outer signature object type');
    }

    static invalidInnerSignatureType(): EnvelopeError {
        return new EnvelopeError('invalid-inner-signature-type', 'unexpected inner signature object type');
    }

    static unverifiedInnerSignature(): EnvelopeError {
        return new EnvelopeError(
            'unverified-inner-signature',
            'inner signature not made with same key as outer signature',
        );
    }

    static invalidSignatureType(): EnvelopeError {
        return new EnvelopeError('invalid-signature-type', 'unexpected signature object type');
    }

    static invalidShares(): EnvelopeError {
        return new EnvelopeError('invalid-shares', 'invalid SSKR shares');
    }

    static invalidType(): EnvelopeError {
        return new EnvelopeError('invalid-type', 'invalid type');
    }

    static ambiguousType(): EnvelopeError {
        return new EnvelopeError('ambiguous-type', 'ambiguous type');
    }

    static subjectNotUnit(): EnvelopeError {
        return new EnvelopeError('subject-not-unit', 'the subject of the envelope is not the unit value');
    }

    static unexpectedResponseId(): EnvelopeError {
        return new EnvelopeError('unexpected-response-id', 'unexpected response ID');
    }

    static invalidResponse(): EnvelopeError {
        return new EnvelopeError('invalid-response', 'invalid response');
    }

    static sskr(cause: unknown): EnvelopeError {
        return new EnvelopeError('sskr', `sskr error: ${String(cause)}`);
    }

    static cbor(cause: unknown): EnvelopeError {
        return new EnvelopeError('cbor', `cbor error: ${String(cause)}`);
    }

    static components(cause: unknown): EnvelopeError {
        return new EnvelopeError('components', `components error: ${String(cause)}`);
    }

    static general(message: string): EnvelopeError {
        return new EnvelopeError('general', `general error: ${message}`);
    }
}
