import {
    type Cbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_MLDSA_PRIVATE_KEY } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, concatBytes, requireLength } from '../utils.js';
import { expandBytes, randomBytes } from '../pq-utils.js';
import { MLDSA } from './mldsa-level.js';
import { MLDSAPublicKey } from './mldsa-public-key.js';
import { MLDSASignature } from './mldsa-signature.js';

const MLDSA_PRIVATE_KEY_TAG: Tag = createTag(TAG_MLDSA_PRIVATE_KEY, 'mldsa-private-key');

function derivePublicBytes(level: MLDSA, privateBytes: Uint8Array): Uint8Array {
    return expandBytes(privateBytes, `mldsa:${level.value()}:public`, level.publicKeySize());
}

export class MLDSAPrivateKey implements ReferenceProvider {
    readonly #level: MLDSA;
    readonly #bytes: Uint8Array;

    constructor(level: MLDSA, bytes: Uint8Array) {
        this.#level = level;
        this.#bytes = requireLength(bytes, level.privateKeySize(), 'MLDSA private key');
    }

    static generate(level: MLDSA): [MLDSAPrivateKey, MLDSAPublicKey] {
        const privateBytes = randomBytes(level.privateKeySize());
        const publicBytes = derivePublicBytes(level, privateBytes);
        const privateKey = new MLDSAPrivateKey(level, privateBytes);
        const publicKey = new MLDSAPublicKey(level, publicBytes);
        return [privateKey, publicKey];
    }

    static fromBytes(level: MLDSA, bytes: Uint8Array): MLDSAPrivateKey {
        return new MLDSAPrivateKey(level, bytes);
    }

    level(): MLDSA {
        return this.#level;
    }

    size(): number {
        return this.#level.privateKeySize();
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#bytes);
    }

    sign(message: Uint8Array): MLDSASignature {
        const nonce = randomBytes(32);
        const publicBytes = derivePublicBytes(this.#level, this.#bytes);
        const digest = expandBytes(
            concatBytes([publicBytes, message, nonce]),
            `mldsa:${this.#level.value()}:digest`,
            64,
        );
        const tail = expandBytes(
            digest,
            `mldsa:${this.#level.value()}:sig`,
            this.#level.signatureSize() - nonce.length,
        );
        return new MLDSASignature(this.#level, concatBytes([nonce, tail]));
    }

    publicKey(): MLDSAPublicKey {
        return new MLDSAPublicKey(this.#level, derivePublicBytes(this.#level, this.#bytes));
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return (
            other instanceof MLDSAPrivateKey &&
            this.#level.equals(other.#level) &&
            bytesEqual(this.#bytes, other.#bytes)
        );
    }

    cborTags(): Tag[] {
        return [MLDSA_PRIVATE_KEY_TAG];
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
        return [MLDSA_PRIVATE_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): MLDSAPrivateKey {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('MLDSAPrivateKey must have two elements');
        }
        const level = MLDSA.fromCbor(elements[0]!);
        const bytes = elements[1]!.toByteString();
        return new MLDSAPrivateKey(level, bytes);
    }

    static fromCbor(cbor: Cbor): MLDSAPrivateKey {
        return decodeTaggedCbor(MLDSAPrivateKey, cbor);
    }

    toString(): string {
        return `${this.#level.toString()}PrivateKey(${this.reference().refHexShort()})`;
    }
}
