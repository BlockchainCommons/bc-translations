import {
    SCHNORR_PUBLIC_KEY_SIZE,
    schnorrVerify,
} from '@bc/crypto';

import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexEncode, requireLength } from '../utils.js';

export class SchnorrPublicKey implements ReferenceProvider {
    static readonly KEY_SIZE = SCHNORR_PUBLIC_KEY_SIZE;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, SCHNORR_PUBLIC_KEY_SIZE, 'Schnorr public key');
    }

    static fromData(data: Uint8Array): SchnorrPublicKey {
        return new SchnorrPublicKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): SchnorrPublicKey {
        return SchnorrPublicKey.fromData(data);
    }

    data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data();
    }

    schnorrVerify(signature: Uint8Array, message: Uint8Array): boolean {
        return schnorrVerify(this.#data, signature, message);
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.#data));
    }

    equals(other: unknown): boolean {
        return other instanceof SchnorrPublicKey && bytesEqual(this.#data, other.#data);
    }

    toString(): string {
        return `SchnorrPublicKey(${this.reference().refHexShort()})`;
    }
}
