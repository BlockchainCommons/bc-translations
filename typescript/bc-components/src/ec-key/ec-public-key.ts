import {
    ECDSA_PUBLIC_KEY_SIZE,
    ecdsaDecompressPublicKey,
    ecdsaVerify,
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
import type { ECPublicKeyBase } from './ec-public-key-base.js';
import { ECUncompressedPublicKey } from './ec-uncompressed-public-key.js';

const EC_TAGS: Tag[] = [
    createTag(TAG_EC_KEY, 'eckey'),
    createTag(TAG_EC_KEY_V1, 'eckey-v1'),
];

export class ECPublicKey implements ECPublicKeyBase, ReferenceProvider {
    static readonly KEY_SIZE = ECDSA_PUBLIC_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, ECDSA_PUBLIC_KEY_SIZE, 'ECDSA public key');
    }

    static fromData(data: Uint8Array): ECPublicKey {
        return new ECPublicKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): ECPublicKey {
        return ECPublicKey.fromData(data);
    }

    data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data();
    }

    verify(signature: Uint8Array, message: Uint8Array): boolean {
        return ecdsaVerify(this.#data, signature, message);
    }

    uncompressedPublicKey(): ECUncompressedPublicKey {
        return new ECUncompressedPublicKey(ecdsaDecompressPublicKey(this.#data));
    }

    publicKey(): ECPublicKey {
        return this;
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof ECPublicKey && bytesEqual(this.#data, other.#data);
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

    static fromUntaggedCbor(cbor: Cbor): ECPublicKey {
        const map = cbor.toMap();
        const key = map.extract<3, Uint8Array>(3);
        return ECPublicKey.fromData(key);
    }

    static fromCbor(cbor: Cbor): ECPublicKey {
        return decodeTaggedCbor(ECPublicKey, cbor);
    }

    toString(): string {
        return `ECPublicKey(${this.reference().refHexShort()})`;
    }
}
