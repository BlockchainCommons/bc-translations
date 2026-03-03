import {
    type Cbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_MLDSA_SIGNATURE } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { bytesEqual, requireLength } from '../utils.js';
import { MLDSA } from './mldsa-level.js';

const MLDSA_SIGNATURE_TAG: Tag = createTag(TAG_MLDSA_SIGNATURE, 'mldsa-signature');

export class MLDSASignature {
    readonly #level: MLDSA;
    readonly #bytes: Uint8Array;

    constructor(level: MLDSA, bytes: Uint8Array) {
        this.#level = level;
        this.#bytes = requireLength(bytes, level.signatureSize(), 'MLDSA signature');
    }

    static fromBytes(level: MLDSA, bytes: Uint8Array): MLDSASignature {
        return new MLDSASignature(level, bytes);
    }

    level(): MLDSA {
        return this.#level;
    }

    size(): number {
        return this.#level.signatureSize();
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#bytes);
    }

    equals(other: unknown): boolean {
        return (
            other instanceof MLDSASignature &&
            this.#level.equals(other.#level) &&
            bytesEqual(this.#bytes, other.#bytes)
        );
    }

    cborTags(): Tag[] {
        return [MLDSA_SIGNATURE_TAG];
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
        return [MLDSA_SIGNATURE_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): MLDSASignature {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('MLDSASignature must have two elements');
        }
        const level = MLDSA.fromCbor(elements[0]!);
        const bytes = elements[1]!.toByteString();
        return new MLDSASignature(level, bytes);
    }

    static fromCbor(cbor: Cbor): MLDSASignature {
        return decodeTaggedCbor(MLDSASignature, cbor);
    }
}
