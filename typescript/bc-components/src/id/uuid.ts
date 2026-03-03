import { randomBytes } from 'node:crypto';

import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_UUID } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from '../utils.js';

const UUID_TAG: Tag = createTag(TAG_UUID, 'uuid');

function toCanonicalUuid(data: Uint8Array): string {
    const hex = hexEncode(data);
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

export class UUID {
    static readonly UUID_SIZE = 16;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, UUID.UUID_SIZE, 'UUID');
    }

    static new(): UUID {
        const data = randomBytes(UUID.UUID_SIZE);
        // RFC 4122 variant + version 4 bits.
        data[6] = (data[6] & 0x0f) | 0x40;
        data[8] = (data[8] & 0x3f) | 0x80;
        return new UUID(data);
    }

    static fromData(data: Uint8Array): UUID {
        return new UUID(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): UUID | null {
        if (data.length !== UUID.UUID_SIZE) {
            return null;
        }
        return UUID.fromData(data);
    }

    static fromString(value: string): UUID {
        const hex = value.replace(/-/g, '');
        return UUID.fromData(hexDecode(hex));
    }

    get data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data;
    }

    toString(): string {
        return toCanonicalUuid(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof UUID && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [UUID_TAG];
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
        return [UUID_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): UUID {
        return UUID.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): UUID {
        return decodeTaggedCbor(UUID, cbor);
    }
}
