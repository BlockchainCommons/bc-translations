import { SecureRandomNumberGenerator } from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_ARID } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { bytesEqual, hexDecode, hexEncode, requireLength, toHexShort } from '../utils.js';

const ARID_TAG: Tag = createTag(TAG_ARID, 'arid');

export class ARID {
    static readonly ARID_SIZE = 32;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, ARID.ARID_SIZE, 'ARID');
    }

    static new(): ARID {
        const rng = new SecureRandomNumberGenerator();
        return new ARID(rng.randomData(ARID.ARID_SIZE));
    }

    static fromData(data: Uint8Array): ARID {
        return new ARID(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): ARID {
        return ARID.fromData(data);
    }

    static fromHex(hex: string): ARID {
        return ARID.fromData(hexDecode(hex));
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

    shortDescription(): string {
        return toHexShort(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof ARID && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [ARID_TAG];
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
        return [ARID_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): ARID {
        return ARID.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): ARID {
        return decodeTaggedCbor(ARID, cbor);
    }

    toString(): string {
        return `ARID(${this.shortDescription()})`;
    }
}
