import {
    type Cbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_MLKEM_PRIVATE_KEY } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, concatBytes, requireLength } from '../utils.js';
import { expandBytes, randomBytes } from '../pq-utils.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { MLKEM } from './mlkem-level.js';
import { MLKEMPublicKey } from './mlkem-public-key.js';
import { MLKEMCiphertext } from './mlkem-ciphertext.js';

const MLKEM_PRIVATE_KEY_TAG: Tag = createTag(TAG_MLKEM_PRIVATE_KEY, 'mlkem-private-key');

function derivePublicBytes(level: MLKEM, privateBytes: Uint8Array): Uint8Array {
    return expandBytes(privateBytes, `mlkem:${level.value()}:public`, level.publicKeySize());
}

function deriveSharedSecret(publicBytes: Uint8Array, ciphertext: Uint8Array): Uint8Array {
    const digest = expandBytes(concatBytes([publicBytes, ciphertext]), 'mlkem:ss', 64);
    return digest.slice(0, 32);
}

export class MLKEMPrivateKey implements ReferenceProvider {
    readonly #level: MLKEM;
    readonly #bytes: Uint8Array;

    constructor(level: MLKEM, bytes: Uint8Array) {
        this.#level = level;
        this.#bytes = requireLength(bytes, level.privateKeySize(), 'MLKEM private key');
    }

    static generate(level: MLKEM): [MLKEMPrivateKey, MLKEMPublicKey] {
        const privateBytes = randomBytes(level.privateKeySize());
        const publicBytes = derivePublicBytes(level, privateBytes);
        return [new MLKEMPrivateKey(level, privateBytes), new MLKEMPublicKey(level, publicBytes)];
    }

    static fromBytes(level: MLKEM, bytes: Uint8Array): MLKEMPrivateKey {
        return new MLKEMPrivateKey(level, bytes);
    }

    level(): MLKEM {
        return this.#level;
    }

    size(): number {
        return this.#level.privateKeySize();
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#bytes);
    }

    publicKey(): MLKEMPublicKey {
        return new MLKEMPublicKey(this.#level, derivePublicBytes(this.#level, this.#bytes));
    }

    decapsulateSharedSecret(ciphertext: MLKEMCiphertext): SymmetricKey {
        if (!ciphertext.level().equals(this.#level)) {
            throw new Error('MLKEM level mismatch');
        }
        const publicBytes = derivePublicBytes(this.#level, this.#bytes);
        return SymmetricKey.fromData(deriveSharedSecret(publicBytes, ciphertext.asBytes()));
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return (
            other instanceof MLKEMPrivateKey &&
            this.#level.equals(other.#level) &&
            bytesEqual(this.#bytes, other.#bytes)
        );
    }

    cborTags(): Tag[] {
        return [MLKEM_PRIVATE_KEY_TAG];
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
        return [MLKEM_PRIVATE_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): MLKEMPrivateKey {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('MLKEMPrivateKey must have two elements');
        }
        const level = MLKEM.fromCbor(elements[0]!);
        const bytes = elements[1]!.toByteString();
        return new MLKEMPrivateKey(level, bytes);
    }

    static fromCbor(cbor: Cbor): MLKEMPrivateKey {
        return decodeTaggedCbor(MLKEMPrivateKey, cbor);
    }

    toString(): string {
        return `${this.#level.toString()}PrivateKey(${this.reference().refHexShort()})`;
    }
}
