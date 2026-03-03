import {
    type Cbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_MLKEM_CIPHERTEXT } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { bytesEqual, requireLength } from '../utils.js';
import { MLKEM } from './mlkem-level.js';

const MLKEM_CIPHERTEXT_TAG: Tag = createTag(TAG_MLKEM_CIPHERTEXT, 'mlkem-ciphertext');

export class MLKEMCiphertext {
    readonly #level: MLKEM;
    readonly #bytes: Uint8Array;

    constructor(level: MLKEM, bytes: Uint8Array) {
        this.#level = level;
        this.#bytes = requireLength(bytes, level.ciphertextSize(), 'MLKEM ciphertext');
    }

    static fromBytes(level: MLKEM, bytes: Uint8Array): MLKEMCiphertext {
        return new MLKEMCiphertext(level, bytes);
    }

    level(): MLKEM {
        return this.#level;
    }

    size(): number {
        return this.#level.ciphertextSize();
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#bytes);
    }

    equals(other: unknown): boolean {
        return (
            other instanceof MLKEMCiphertext &&
            this.#level.equals(other.#level) &&
            bytesEqual(this.#bytes, other.#bytes)
        );
    }

    cborTags(): Tag[] {
        return [MLKEM_CIPHERTEXT_TAG];
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
        return [MLKEM_CIPHERTEXT_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): MLKEMCiphertext {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('MLKEMCiphertext must have two elements');
        }
        const level = MLKEM.fromCbor(elements[0]!);
        const bytes = elements[1]!.toByteString();
        return new MLKEMCiphertext(level, bytes);
    }

    static fromCbor(cbor: Cbor): MLKEMCiphertext {
        return decodeTaggedCbor(MLKEMCiphertext, cbor);
    }
}
