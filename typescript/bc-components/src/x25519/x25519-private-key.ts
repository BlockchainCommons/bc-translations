import {
    deriveAgreementPrivateKey,
    x25519NewPrivateKeyUsing,
    x25519PublicKeyFromPrivateKey,
    x25519SharedKey,
    X25519_PRIVATE_KEY_SIZE,
} from '@bc/crypto';
import { SecureRandomNumberGenerator, type RandomNumberGenerator } from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_X25519_PRIVATE_KEY } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData, fromTaggedUrString, toUrString } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from '../utils.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { X25519PublicKey } from './x25519-public-key.js';

const X25519_PRIVATE_KEY_TAG: Tag = createTag(
    TAG_X25519_PRIVATE_KEY,
    'agreement-private-key',
);

export class X25519PrivateKey implements ReferenceProvider {
    static readonly KEY_SIZE = X25519_PRIVATE_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data?: Uint8Array) {
        if (data === undefined) {
            const rng = new SecureRandomNumberGenerator();
            this.#data = x25519NewPrivateKeyUsing(rng);
            return;
        }
        this.#data = requireLength(data, X25519_PRIVATE_KEY_SIZE, 'X25519 private key');
    }

    static new(): X25519PrivateKey {
        return new X25519PrivateKey();
    }

    static keypair(): [X25519PrivateKey, X25519PublicKey] {
        const privateKey = X25519PrivateKey.new();
        return [privateKey, privateKey.publicKey()];
    }

    static keypairUsing(rng: RandomNumberGenerator): [X25519PrivateKey, X25519PublicKey] {
        const privateKey = X25519PrivateKey.newUsing(rng);
        return [privateKey, privateKey.publicKey()];
    }

    static newUsing(rng: RandomNumberGenerator): X25519PrivateKey {
        return new X25519PrivateKey(x25519NewPrivateKeyUsing(rng));
    }

    static fromData(data: Uint8Array): X25519PrivateKey {
        return new X25519PrivateKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): X25519PrivateKey {
        return X25519PrivateKey.fromData(data);
    }

    static fromHex(hex: string): X25519PrivateKey {
        return X25519PrivateKey.fromData(hexDecode(hex));
    }

    static deriveFromKeyMaterial(material: Uint8Array): X25519PrivateKey {
        return X25519PrivateKey.fromData(deriveAgreementPrivateKey(material));
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

    publicKey(): X25519PublicKey {
        return X25519PublicKey.fromData(x25519PublicKeyFromPrivateKey(this.#data));
    }

    sharedKeyWith(publicKey: X25519PublicKey): SymmetricKey {
        return SymmetricKey.fromData(x25519SharedKey(this.#data, publicKey.data));
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return other instanceof X25519PrivateKey && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [X25519_PRIVATE_KEY_TAG];
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

    static fromURString(value: string): X25519PrivateKey {
        return fromTaggedUrString(X25519PrivateKey, value);
    }

    static cborTags(): Tag[] {
        return [X25519_PRIVATE_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): X25519PrivateKey {
        return X25519PrivateKey.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): X25519PrivateKey {
        return decodeTaggedCbor(X25519PrivateKey, cbor);
    }

    toString(): string {
        return `X25519PrivateKey(${this.reference().refHexShort()})`;
    }
}
