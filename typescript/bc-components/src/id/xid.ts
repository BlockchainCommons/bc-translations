import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_XID } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { identifier as bytewordsId, bytemojiIdentifier as bytemojiId } from '@bc/ur';

import { Digest } from '../digest.js';
import { Reference } from '../reference.js';
import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData, fromTaggedUrString, toUrString } from '../cbor-ur.js';
import { bytesEqual, hexDecode, hexEncode, requireLength, toHexShort } from '../utils.js';

const XID_TAG: Tag = createTag(TAG_XID, 'xid');

export interface XIDProvider {
    xid(): XID;
}

export interface SigningPublicKeyLike {
    taggedCborData(): Uint8Array;
}

export class XID {
    static readonly XID_SIZE = 32;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, XID.XID_SIZE, 'XID');
    }

    static fromData(data: Uint8Array): XID {
        return new XID(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): XID {
        return XID.fromData(data);
    }

    static new(genesisKey: SigningPublicKeyLike): XID {
        return XID.fromData(Digest.fromImage(genesisKey.taggedCborData()).data);
    }

    validate(genesisKey: SigningPublicKeyLike): boolean {
        return bytesEqual(this.#data, XID.new(genesisKey).#data);
    }

    static fromHex(hex: string): XID {
        return XID.fromData(hexDecode(hex));
    }

    toHex(): string {
        return hexEncode(this.#data);
    }

    get shortDescription(): string {
        return toHexShort(this.#data);
    }

    bytewordsIdentifier(prefix: boolean): string {
        const core = bytewordsId(this.#data.slice(0, 4)).toUpperCase();
        return prefix ? `🅧 ${core}` : core;
    }

    bytemojiIdentifier(prefix: boolean): string {
        const core = bytemojiId(this.#data.slice(0, 4));
        return prefix ? `🅧 ${core}` : core;
    }

    get data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data;
    }

    reference(): Reference {
        return Reference.fromData(this.#data);
    }

    xid(): XID {
        return this;
    }

    equals(other: unknown): boolean {
        return other instanceof XID && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [XID_TAG];
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

    urString(): string {
        return toUrString(this);
    }

    static fromURString(value: string): XID {
        return fromTaggedUrString(XID, value);
    }

    static cborTags(): Tag[] {
        return [XID_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): XID {
        return XID.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): XID {
        return decodeTaggedCbor(XID, cbor);
    }

    toString(): string {
        return `XID(${this.shortDescription})`;
    }
}
