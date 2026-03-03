import {
    ed25519Verify,
    ED25519_PUBLIC_KEY_SIZE,
} from '@bc/crypto';

import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from '../utils.js';

export class Ed25519PublicKey implements ReferenceProvider {
    static readonly KEY_SIZE = ED25519_PUBLIC_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, ED25519_PUBLIC_KEY_SIZE, 'Ed25519 public key');
    }

    static fromData(data: Uint8Array): Ed25519PublicKey {
        return new Ed25519PublicKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): Ed25519PublicKey {
        return Ed25519PublicKey.fromData(data);
    }

    static fromHex(hex: string): Ed25519PublicKey {
        return Ed25519PublicKey.fromData(hexDecode(hex));
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

    verify(signature: Uint8Array, message: Uint8Array): boolean {
        return ed25519Verify(this.#data, message, signature);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.#data));
    }

    equals(other: unknown): boolean {
        return other instanceof Ed25519PublicKey && bytesEqual(this.#data, other.#data);
    }

    toString(): string {
        return `Ed25519PublicKey(${this.reference().refHexShort()})`;
    }
}
