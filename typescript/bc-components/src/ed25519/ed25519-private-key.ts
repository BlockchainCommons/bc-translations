import {
    deriveSigningPrivateKey,
    ed25519NewPrivateKeyUsing,
    ed25519PublicKeyFromPrivateKey,
    ed25519Sign,
    ED25519_PRIVATE_KEY_SIZE,
} from '@bc/crypto';
import { SecureRandomNumberGenerator, type RandomNumberGenerator } from '@bc/rand';

import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from '../utils.js';
import { Ed25519PublicKey } from './ed25519-public-key.js';

export class Ed25519PrivateKey implements ReferenceProvider {
    static readonly KEY_SIZE = ED25519_PRIVATE_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data?: Uint8Array) {
        if (data === undefined) {
            const rng = new SecureRandomNumberGenerator();
            this.#data = ed25519NewPrivateKeyUsing(rng);
            return;
        }
        this.#data = requireLength(data, ED25519_PRIVATE_KEY_SIZE, 'Ed25519 private key');
    }

    static new(): Ed25519PrivateKey {
        return new Ed25519PrivateKey();
    }

    static newUsing(rng: RandomNumberGenerator): Ed25519PrivateKey {
        return new Ed25519PrivateKey(ed25519NewPrivateKeyUsing(rng));
    }

    static fromData(data: Uint8Array): Ed25519PrivateKey {
        return new Ed25519PrivateKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): Ed25519PrivateKey {
        return Ed25519PrivateKey.fromData(data);
    }

    static deriveFromKeyMaterial(material: Uint8Array): Ed25519PrivateKey {
        return Ed25519PrivateKey.fromData(deriveSigningPrivateKey(material));
    }

    static fromHex(hex: string): Ed25519PrivateKey {
        return Ed25519PrivateKey.fromData(hexDecode(hex));
    }

    data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data();
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    publicKey(): Ed25519PublicKey {
        return Ed25519PublicKey.fromData(ed25519PublicKeyFromPrivateKey(this.#data));
    }

    sign(message: Uint8Array): Uint8Array {
        return ed25519Sign(this.#data, message);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.#data));
    }

    equals(other: unknown): boolean {
        return other instanceof Ed25519PrivateKey && bytesEqual(this.#data, other.#data);
    }

    toString(): string {
        return `Ed25519PrivateKey(${this.reference().refHexShort()})`;
    }
}
