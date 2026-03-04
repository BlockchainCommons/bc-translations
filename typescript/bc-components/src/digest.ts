import {
    type Cbor,
    createTag,
    decodeCbor,
    toByteString,
} from '@bc/dcbor';
import { sha256 } from '@bc/crypto';
import { TAG_DIGEST } from '@bc/tags';

import { type Tag } from '@bc/dcbor';

import { BCComponentsError } from './error.js';
import {
    decodeTaggedCbor,
    decodeTaggedCborData,
    defaultTaggedCbor,
    defaultTaggedCborData,
    fromTaggedUrString,
    toUrString,
} from './cbor-ur.js';
import { bytesEqual, concatBytes, hexDecode, hexEncode, requireLength, toHexShort } from './utils.js';

const DIGEST_TAG: Tag = createTag(TAG_DIGEST, 'digest');

export class Digest {
    static readonly DIGEST_SIZE = 32;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(data, Digest.DIGEST_SIZE, 'Digest');
    }

    static fromData(data: Uint8Array): Digest {
        return new Digest(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): Digest {
        return Digest.fromData(data);
    }

    static fromImage(image: Uint8Array): Digest {
        return Digest.fromData(sha256(image));
    }

    static fromImageParts(parts: readonly Uint8Array[]): Digest {
        return Digest.fromImage(concatBytes(parts));
    }

    static fromDigests(digests: readonly Digest[]): Digest {
        return Digest.fromImage(concatBytes(digests.map((d) => d.data)));
    }

    static fromHex(hex: string): Digest {
        return Digest.fromData(hexDecode(hex));
    }

    static validateOpt(args: { image: Uint8Array; digest?: Digest | null }): boolean {
        if (args.digest == null) {
            return true;
        }
        return args.digest.validate(args.image);
    }

    validate(image: Uint8Array): boolean {
        return bytesEqual(this.#data, Digest.fromImage(image).#data);
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

    get shortDescription(): string {
        return toHexShort(this.#data);
    }

    equals(other: unknown): boolean {
        return other instanceof Digest && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [DIGEST_TAG];
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

    toString(): string {
        return `Digest(${this.hex()})`;
    }

    static cborTags(): Tag[] {
        return [DIGEST_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): Digest {
        try {
            return Digest.fromData(cbor.toByteString());
        } catch (error) {
            throw BCComponentsError.invalidData('Digest', `${error}`);
        }
    }

    static fromCbor(cbor: Cbor): Digest {
        return decodeTaggedCbor(Digest, cbor);
    }

    static fromCborData(data: Uint8Array): Digest {
        return decodeTaggedCborData(Digest, data);
    }

    static fromURString(value: string): Digest {
        return fromTaggedUrString(Digest, value);
    }

    static fromTaggedCborData(data: Uint8Array): Digest {
        return Digest.fromCbor(decodeCbor(data));
    }
}
