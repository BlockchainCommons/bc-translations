import {
    type Cbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_MLDSA_PUBLIC_KEY } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { BCComponentsError } from '../error.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, concatBytes, requireLength } from '../utils.js';
import { expandBytes } from '../pq-utils.js';
import { MLDSA } from './mldsa-level.js';
import { MLDSASignature } from './mldsa-signature.js';

const MLDSA_PUBLIC_KEY_TAG: Tag = createTag(TAG_MLDSA_PUBLIC_KEY, 'mldsa-public-key');

export class MLDSAPublicKey implements ReferenceProvider {
    readonly #level: MLDSA;
    readonly #bytes: Uint8Array;

    constructor(level: MLDSA, bytes: Uint8Array) {
        this.#level = level;
        this.#bytes = requireLength(bytes, level.publicKeySize(), 'MLDSA public key');
    }

    static fromBytes(level: MLDSA, bytes: Uint8Array): MLDSAPublicKey {
        return new MLDSAPublicKey(level, bytes);
    }

    level(): MLDSA {
        return this.#level;
    }

    size(): number {
        return this.#level.publicKeySize();
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#bytes);
    }

    verify(signature: MLDSASignature, message: Uint8Array): boolean {
        if (!signature.level().equals(this.#level)) {
            throw BCComponentsError.levelMismatch(this.#level.toString(), signature.level().toString());
        }
        const signatureBytes = signature.asBytes();
        const nonce = signatureBytes.slice(0, 32);
        const tail = signatureBytes.slice(32);
        const digest = expandBytes(
            concatBytes([this.#bytes, message, nonce]),
            `mldsa:${this.#level.value()}:digest`,
            64,
        );
        const expected = expandBytes(
            digest,
            `mldsa:${this.#level.value()}:sig`,
            this.#level.signatureSize() - nonce.length,
        );
        return bytesEqual(tail, expected);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return (
            other instanceof MLDSAPublicKey &&
            this.#level.equals(other.#level) &&
            bytesEqual(this.#bytes, other.#bytes)
        );
    }

    cborTags(): Tag[] {
        return [MLDSA_PUBLIC_KEY_TAG];
    }

    untaggedCbor(): Cbor {
        return toCbor([this.#level.toCbor(), toByteString(this.#bytes)]);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return [MLDSA_PUBLIC_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): MLDSAPublicKey {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('MLDSAPublicKey must have two elements');
        }
        const level = MLDSA.fromCbor(elements[0]!);
        const bytes = elements[1]!.toByteString();
        return new MLDSAPublicKey(level, bytes);
    }

    static fromCbor(cbor: Cbor): MLDSAPublicKey {
        return decodeTaggedCbor(MLDSAPublicKey, cbor);
    }

    toString(): string {
        return `${this.#level.toString()}PublicKey(${this.reference().refHexShort()})`;
    }
}
