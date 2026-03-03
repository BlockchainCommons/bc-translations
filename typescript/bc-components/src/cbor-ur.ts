import {
    type Cbor,
    type CborTaggedDecodable,
    type CborTaggedEncodable,
    type Tag,
    cborData,
    createTaggedCbor,
    decodeCbor,
    extractTaggedContent,
    validateTag,
} from '@bc/dcbor';
import { UR, urString } from '@bc/ur';

export interface TaggedCborEncodable {
    cborTags(): Tag[];
    untaggedCbor(): Cbor;
    taggedCbor(): Cbor;
    taggedCborData(): Uint8Array;
    cbor(): Cbor;
}

export interface TaggedCborDecodable<T> {
    cborTags(): Tag[];
    fromUntaggedCbor(cbor: Cbor): T;
}

export function defaultTaggedCbor(value: {
    cborTags(): Tag[];
    untaggedCbor(): Cbor;
    taggedCbor(): Cbor;
}): Cbor {
    return createTaggedCbor(value as CborTaggedEncodable);
}

export function defaultTaggedCborData(value: {
    cborTags(): Tag[];
    untaggedCbor(): Cbor;
    taggedCbor(): Cbor;
}): Uint8Array {
    return cborData(defaultTaggedCbor(value));
}

export function decodeTaggedCbor<T>(
    codec: TaggedCborDecodable<T>,
    cbor: Cbor,
): T {
    validateTag(cbor, codec.cborTags());
    return codec.fromUntaggedCbor(extractTaggedContent(cbor));
}

export function decodeTaggedCborData<T>(
    codec: TaggedCborDecodable<T>,
    data: Uint8Array,
): T {
    return decodeTaggedCbor(codec, decodeCbor(data));
}

export function toUrString(value: {
    cborTags(): Tag[];
    untaggedCbor(): Cbor;
    taggedCbor(): Cbor;
}): string {
    return urString(value);
}

export function fromTaggedUrString<T>(
    codec: TaggedCborDecodable<T>,
    value: string,
): T {
    const parsed = UR.fromUrString(value);
    const primaryTag = codec.cborTags()[0];
    if (primaryTag?.name === undefined) {
        throw new Error('Primary tag has no registered name.');
    }
    parsed.checkType(primaryTag.name);
    return (codec as CborTaggedDecodable<T>).fromUntaggedCbor(parsed.cbor());
}
