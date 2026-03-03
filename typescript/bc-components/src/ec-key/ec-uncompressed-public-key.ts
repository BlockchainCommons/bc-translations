import {
    ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
    ecdsaCompressPublicKey,
} from '@bc/crypto';
import {
    type Cbor,
    CborMap,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_EC_KEY, TAG_EC_KEY_V1 } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexEncode, requireLength } from '../utils.js';
import { ECPublicKey } from './ec-public-key.js';

const EC_TAGS: Tag[] = [
    createTag(TAG_EC_KEY, 'eckey'),
    createTag(TAG_EC_KEY_V1, 'eckey-v1'),
];

export class ECUncompressedPublicKey implements ReferenceProvider {
    static readonly KEY_SIZE = ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(
            data,
            ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
            'ECDSA uncompressed public key',
        );
    }

    static fromData(data: Uint8Array): ECUncompressedPublicKey {
        return new ECUncompressedPublicKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): ECUncompressedPublicKey {
        return ECUncompressedPublicKey.fromData(data);
    }

    data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data();
    }

    publicKey(): ECPublicKey {
        return ECPublicKey.fromData(ecdsaCompressPublicKey(this.#data));
    }

    uncompressedPublicKey(): ECUncompressedPublicKey {
        return this;
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    equals(other: unknown): boolean {
        return (
            other instanceof ECUncompressedPublicKey &&
            bytesEqual(this.#data, other.#data)
        );
    }

    cborTags(): Tag[] {
        return EC_TAGS;
    }

    untaggedCbor(): Cbor {
        const map = new CborMap();
        map.set(3, toByteString(this.#data));
        return toCbor(map);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return EC_TAGS;
    }

    static fromUntaggedCbor(cbor: Cbor): ECUncompressedPublicKey {
        const map = cbor.toMap();
        const key = map.extract<3, Uint8Array>(3);
        return ECUncompressedPublicKey.fromData(key);
    }

    static fromCbor(cbor: Cbor): ECUncompressedPublicKey {
        return decodeTaggedCbor(ECUncompressedPublicKey, cbor);
    }

    toString(): string {
        return `ECUncompressedPublicKey(${this.reference().refHexShort()})`;
    }
}
