import {
    X25519_PUBLIC_KEY_SIZE,
} from '@bc/crypto';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_X25519_PUBLIC_KEY } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData, fromTaggedUrString, toUrString } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from '../utils.js';

const X25519_PUBLIC_KEY_TAG: Tag = createTag(
    TAG_X25519_PUBLIC_KEY,
    'agreement-public-key',
);

export class X25519PublicKey implements ReferenceProvider {
    static readonly KEY_SIZE = X25519_PUBLIC_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, X25519_PUBLIC_KEY_SIZE, 'X25519 public key');
    }

    static fromData(data: Uint8Array): X25519PublicKey {
        return new X25519PublicKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): X25519PublicKey {
        return X25519PublicKey.fromData(data);
    }

    static fromHex(hex: string): X25519PublicKey {
        return X25519PublicKey.fromData(hexDecode(hex));
    }

    get data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data;
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return other instanceof X25519PublicKey && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [X25519_PUBLIC_KEY_TAG];
    }

    untaggedCbor(): Cbor {
        return toByteString(this.#data);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    urString(): string {
        return toUrString(this);
    }

    static fromURString(value: string): X25519PublicKey {
        return fromTaggedUrString(X25519PublicKey, value);
    }

    static cborTags(): Tag[] {
        return [X25519_PUBLIC_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): X25519PublicKey {
        return X25519PublicKey.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): X25519PublicKey {
        return decodeTaggedCbor(X25519PublicKey, cbor);
    }

    toString(): string {
        return `X25519PublicKey(${this.reference().refHexShort()})`;
    }
}
