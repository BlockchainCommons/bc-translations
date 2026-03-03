import { SecureRandomNumberGenerator, type RandomNumberGenerator } from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_SALT } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from './cbor-ur.js';
import { bytesEqual, hexDecode, hexEncode } from './utils.js';

const SALT_TAG: Tag = createTag(TAG_SALT, 'salt');

export class Salt {
    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = new Uint8Array(data);
    }

    static fromData(data: Uint8Array): Salt {
        return new Salt(data);
    }

    static newWithLen(length: number): Salt {
        const rng = new SecureRandomNumberGenerator();
        return Salt.newWithLenUsing(length, rng);
    }

    static newWithLenUsing(length: number, rng: RandomNumberGenerator): Salt {
        return new Salt(rng.randomData(length));
    }

    static newForSize(size: number): Salt {
        const len = Math.max(16, Math.ceil(Math.log2(Math.max(1, size))) + 4);
        return Salt.newWithLen(len);
    }

    static fromHex(hex: string): Salt {
        return new Salt(hexDecode(hex));
    }

    get length(): number {
        return this.#data.length;
    }

    isEmpty(): boolean {
        return this.#data.length === 0;
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    get data(): Uint8Array {
        return this.asBytes();
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof Salt && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [SALT_TAG];
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

    static cborTags(): Tag[] {
        return [SALT_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): Salt {
        return new Salt(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): Salt {
        return decodeTaggedCbor(Salt, cbor);
    }

    toString(): string {
        return `Salt(${this.#data.length})`;
    }
}
