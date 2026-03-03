import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { bytemojiIdentifier, identifier as bytewordsIdentifier } from '@bc/ur';
import { TAG_REFERENCE } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from './cbor-ur.js';
import { type Digest } from './digest.js';
import { BCComponentsError } from './error.js';
import { bytesEqual, hexDecode, hexEncode, requireLength, toHexShort } from './utils.js';

const REFERENCE_TAG: Tag = createTag(TAG_REFERENCE, 'reference');

export interface ReferenceProvider {
    reference(): Reference;
}

export class Reference {
    static readonly REFERENCE_SIZE = 32;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, Reference.REFERENCE_SIZE, 'Reference');
    }

    static fromData(data: Uint8Array): Reference {
        return new Reference(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): Reference {
        return Reference.fromData(data);
    }

    static fromDigest(digest: Digest): Reference {
        return Reference.fromData(digest.data);
    }

    static fromHex(hex: string): Reference {
        return Reference.fromData(hexDecode(hex));
    }

    get data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data;
    }

    refHex(): string {
        return hexEncode(this.#data);
    }

    refDataShort(): Uint8Array {
        return this.#data.slice(0, 4);
    }

    refHexShort(): string {
        return toHexShort(this.#data);
    }

    bytewordsIdentifier(prefix?: string): string {
        const core = bytewordsIdentifier(this.refDataShort()).toUpperCase();
        return prefix ? `${prefix} ${core}` : core;
    }

    bytemojiIdentifier(prefix?: string): string {
        const core = bytemojiIdentifier(this.refDataShort());
        return prefix ? `${prefix} ${core}` : core;
    }

    equals(other: unknown): boolean {
        return other instanceof Reference && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [REFERENCE_TAG];
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

    reference(): Reference {
        return this;
    }

    toString(): string {
        return `Reference(${this.refHexShort()})`;
    }

    static cborTags(): Tag[] {
        return [REFERENCE_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): Reference {
        try {
            return Reference.fromData(cbor.toByteString());
        } catch (error) {
            throw BCComponentsError.invalidData('Reference', `${error}`);
        }
    }

    static fromCbor(cbor: Cbor): Reference {
        return decodeTaggedCbor(Reference, cbor);
    }
}
