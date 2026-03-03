import {
    ECDSA_PRIVATE_KEY_SIZE,
    ecdsaDerivePrivateKey,
    ecdsaNewPrivateKeyUsing,
    ecdsaPublicKeyFromPrivateKey,
    ecdsaSign,
    schnorrPublicKeyFromPrivateKey,
    schnorrSign,
    schnorrSignUsing,
} from '@bc/crypto';
import { SecureRandomNumberGenerator, type RandomNumberGenerator } from '@bc/rand';
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
import { SchnorrPublicKey } from './schnorr-public-key.js';

const EC_TAGS: Tag[] = [
    createTag(TAG_EC_KEY, 'eckey'),
    createTag(TAG_EC_KEY_V1, 'eckey-v1'),
];

export class ECPrivateKey implements ReferenceProvider {
    static readonly KEY_SIZE = ECDSA_PRIVATE_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data?: Uint8Array) {
        if (data === undefined) {
            const rng = new SecureRandomNumberGenerator();
            this.#data = ecdsaNewPrivateKeyUsing(rng);
            return;
        }
        this.#data = requireLength(data, ECDSA_PRIVATE_KEY_SIZE, 'EC private key');
    }

    static new(): ECPrivateKey {
        return new ECPrivateKey();
    }

    static newUsing(rng: RandomNumberGenerator): ECPrivateKey {
        return new ECPrivateKey(ecdsaNewPrivateKeyUsing(rng));
    }

    static fromData(data: Uint8Array): ECPrivateKey {
        return new ECPrivateKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): ECPrivateKey {
        return ECPrivateKey.fromData(data);
    }

    static deriveFromKeyMaterial(material: Uint8Array): ECPrivateKey {
        return ECPrivateKey.fromData(ecdsaDerivePrivateKey(material));
    }

    data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data();
    }

    publicKey(): ECPublicKey {
        return ECPublicKey.fromData(ecdsaPublicKeyFromPrivateKey(this.#data));
    }

    schnorrPublicKey(): SchnorrPublicKey {
        return SchnorrPublicKey.fromData(
            schnorrPublicKeyFromPrivateKey(this.#data),
        );
    }

    ecdsaSign(message: Uint8Array): Uint8Array {
        return ecdsaSign(this.#data, message);
    }

    schnorrSignUsing(
        message: Uint8Array,
        rng: RandomNumberGenerator,
    ): Uint8Array {
        return schnorrSignUsing(this.#data, message, rng);
    }

    schnorrSign(message: Uint8Array): Uint8Array {
        return schnorrSign(this.#data, message);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof ECPrivateKey && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return EC_TAGS;
    }

    untaggedCbor(): Cbor {
        const map = new CborMap();
        map.set(2, true);
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

    static fromUntaggedCbor(cbor: Cbor): ECPrivateKey {
        const map = cbor.toMap();
        const key = map.extract<3, Uint8Array>(3);
        return ECPrivateKey.fromData(key);
    }

    static fromCbor(cbor: Cbor): ECPrivateKey {
        return decodeTaggedCbor(ECPrivateKey, cbor);
    }

    toString(): string {
        return `ECPrivateKey(${this.reference().refHexShort()})`;
    }
}
