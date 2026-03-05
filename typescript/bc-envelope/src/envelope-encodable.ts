import {
    ARID,
    Digest,
    EncryptedKey,
    Nonce,
    PrivateKeyBase,
    PrivateKeys,
    PublicKeys,
    Reference,
    SSKRShare,
    Salt,
    SealedMessage,
    Signature,
    URI,
    UUID,
    XID,
} from '@bc/components';
import {
    cbor as toCbor,
    CborDate,
    type Cbor,
} from '@bc/dcbor';
import { KnownValue } from '@bc/known-values';

import { Assertion } from './assertion.js';
import {
    type EnvelopeFunction,
    functionTaggedCbor,
} from './function.js';
import {
    type EnvelopeParameter,
    parameterTaggedCbor,
} from './parameter.js';
import { Envelope } from './envelope.js';

export interface EnvelopeEncodable {
    toEnvelope(): import('./envelope.js').Envelope;
}

function isEnvelopeEncodable(value: unknown): value is EnvelopeEncodable {
    return value !== null && typeof value === 'object' && 'toEnvelope' in value && typeof (value as { toEnvelope: unknown }).toEnvelope === 'function';
}

function isFunction(value: unknown): value is EnvelopeFunction {
    return value !== null && typeof value === 'object' && 'kind' in value && ((value as { kind: unknown }).kind === 'known' || (value as { kind: unknown }).kind === 'named');
}

function isParameter(value: unknown): value is EnvelopeParameter {
    // EnvelopeParameter shares the same discriminated union shape as EnvelopeFunction
    return value !== null && typeof value === 'object' && 'kind' in value && ((value as { kind: unknown }).kind === 'known' || (value as { kind: unknown }).kind === 'named');
}

export function asEnvelope(value: unknown): import('./envelope.js').Envelope {
    if (value instanceof Envelope) {
        return value;
    }

    if (isEnvelopeEncodable(value)) {
        return value.toEnvelope();
    }

    if (typeof value === 'string' || typeof value === 'number' || typeof value === 'bigint' || typeof value === 'boolean') {
        return Envelope.newLeaf(toCbor(value));
    }

    if (value instanceof Uint8Array) {
        return Envelope.newLeaf(toCbor(value));
    }

    if (value instanceof CborDate) {
        return Envelope.newLeaf(value.taggedCbor());
    }

    if (value != null && typeof value === 'object' && 'isCbor' in (value as Record<string, unknown>)) {
        return Envelope.newLeaf(value as Cbor);
    }

    if (value instanceof Digest
        || value instanceof Salt
        || value instanceof Nonce
        || value instanceof ARID
        || value instanceof URI
        || value instanceof UUID
        || value instanceof XID
        || value instanceof Reference
        || value instanceof PublicKeys
        || value instanceof PrivateKeys
        || value instanceof PrivateKeyBase
        || value instanceof SealedMessage
        || value instanceof EncryptedKey
        || value instanceof Signature
        || value instanceof SSKRShare) {
        return Envelope.newLeaf(value.taggedCbor());
    }

    if (value instanceof KnownValue) {
        return Envelope.newWithKnownValue(value);
    }

    if (isFunction(value)) {
        return Envelope.newLeaf(functionTaggedCbor(value));
    }

    if (isParameter(value)) {
        return Envelope.newLeaf(parameterTaggedCbor(value));
    }

    if (value instanceof Assertion) {
        return Envelope.newWithAssertion(value);
    }

    throw new Error(`Type ${typeof value} is not EnvelopeEncodable`);
}
