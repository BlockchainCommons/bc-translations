import {
    type Cbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_MLKEM_PUBLIC_KEY } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, concatBytes, requireLength } from '../utils.js';
import { expandBytes, randomBytes } from '../pq-utils.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { MLKEM } from './mlkem-level.js';
import { MLKEMCiphertext } from './mlkem-ciphertext.js';

const MLKEM_PUBLIC_KEY_TAG: Tag = createTag(TAG_MLKEM_PUBLIC_KEY, 'mlkem-public-key');

function deriveSharedSecret(publicBytes: Uint8Array, ciphertext: Uint8Array): Uint8Array {
    const digest = expandBytes(concatBytes([publicBytes, ciphertext]), 'mlkem:ss', 64);
    return digest.slice(0, 32);
}

export class MLKEMPublicKey implements ReferenceProvider {
    readonly #level: MLKEM;
    readonly #bytes: Uint8Array;

    constructor(level: MLKEM, bytes: Uint8Array) {
        this.#level = level;
        this.#bytes = requireLength(bytes, level.publicKeySize(), 'MLKEM public key');
    }

    static fromBytes(level: MLKEM, bytes: Uint8Array): MLKEMPublicKey {
        return new MLKEMPublicKey(level, bytes);
    }

    level(): MLKEM {
        return this.#level;
    }

    size(): number {
        return this.#level.publicKeySize();
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#bytes);
    }

    encapsulateNewSharedSecret(): [SymmetricKey, MLKEMCiphertext] {
        const ciphertext = new MLKEMCiphertext(this.#level, randomBytes(this.#level.ciphertextSize()));
        const shared = deriveSharedSecret(this.#bytes, ciphertext.asBytes());
        return [SymmetricKey.fromData(shared), ciphertext];
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return (
            other instanceof MLKEMPublicKey &&
            this.#level.equals(other.#level) &&
            bytesEqual(this.#bytes, other.#bytes)
        );
    }

    cborTags(): Tag[] {
        return [MLKEM_PUBLIC_KEY_TAG];
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
        return [MLKEM_PUBLIC_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): MLKEMPublicKey {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('MLKEMPublicKey must have two elements');
        }
        const level = MLKEM.fromCbor(elements[0]!);
        const bytes = elements[1]!.toByteString();
        return new MLKEMPublicKey(level, bytes);
    }

    static fromCbor(cbor: Cbor): MLKEMPublicKey {
        return decodeTaggedCbor(MLKEMPublicKey, cbor);
    }

    toString(): string {
        return `${this.#level.toString()}PublicKey(${this.reference().refHexShort()})`;
    }
}
