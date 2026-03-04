import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_JSON } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from './cbor-ur.js';
import { bytesEqual, fromUtf8, hexDecode, hexEncode, toUtf8 } from './utils.js';

const JSON_TAG: Tag = createTag(TAG_JSON, 'json');

export class JSON {
    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = new Uint8Array(data);
    }

    static fromData(data: Uint8Array): JSON {
        return new JSON(data);
    }

    static fromString(value: string): JSON {
        return new JSON(fromUtf8(value));
    }

    static fromHex(value: string): JSON {
        return new JSON(hexDecode(value));
    }

    get data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data;
    }

    get stringValue(): string {
        return toUtf8(this.#data);
    }

    get count(): number {
        return this.#data.length;
    }

    get length(): number {
        return this.#data.length;
    }

    get isEmpty(): boolean {
        return this.#data.length === 0;
    }

    hex(): string {
        return hexEncode(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof JSON && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [JSON_TAG];
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

    static cborTags(): Tag[] {
        return [JSON_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): JSON {
        return new JSON(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): JSON {
        return decodeTaggedCbor(JSON, cbor);
    }

    toString(): string {
        return `JSON(${this.stringValue})`;
    }
}
