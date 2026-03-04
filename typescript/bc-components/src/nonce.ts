import { SecureRandomNumberGenerator } from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_NONCE } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { BCComponentsError } from './error.js';
import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData, fromTaggedUrString, toUrString } from './cbor-ur.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from './utils.js';

const NONCE_TAG: Tag = createTag(TAG_NONCE, 'nonce');

export class Nonce {
    static readonly NONCE_SIZE = 12;

    readonly #data: Uint8Array;

    constructor(data?: Uint8Array) {
        if (data === undefined) {
            const rng = new SecureRandomNumberGenerator();
            this.#data = rng.randomData(Nonce.NONCE_SIZE);
            return;
        }
        this.#data = requireLength(data, Nonce.NONCE_SIZE, 'Nonce');
    }

    static new(): Nonce {
        return new Nonce();
    }

    static fromData(data: Uint8Array): Nonce {
        return new Nonce(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): Nonce {
        return Nonce.fromData(data);
    }

    static fromHex(hex: string): Nonce {
        try {
            return Nonce.fromData(hexDecode(hex));
        } catch {
            throw BCComponentsError.invalidData('Nonce', 'invalid hex');
        }
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

    equals(other: unknown): boolean {
        return other instanceof Nonce && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [NONCE_TAG];
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

    cbor(): Cbor {
        return this.taggedCbor();
    }

    urString(): string {
        return toUrString(this);
    }

    static fromURString(value: string): Nonce {
        return fromTaggedUrString(Nonce, value);
    }

    static cborTags(): Tag[] {
        return [NONCE_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): Nonce {
        return Nonce.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): Nonce {
        return decodeTaggedCbor(Nonce, cbor);
    }

    toString(): string {
        return `Nonce(${this.hex()})`;
    }
}
