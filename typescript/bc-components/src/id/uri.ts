import { type Cbor, createTag, toByteString, cbor as toCbor } from '@bc/dcbor';
import { TAG_URI } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { BCComponentsError } from '../error.js';
import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';

const URI_TAG: Tag = createTag(TAG_URI, 'url');

export class URI {
    readonly #uri: string;

    constructor(uri: string) {
        try {
            new URL(uri);
            this.#uri = uri;
        } catch {
            throw BCComponentsError.invalidData('URI', 'invalid URI format');
        }
    }

    static new(uri: string): URI {
        return new URI(uri);
    }

    get value(): string {
        return this.#uri;
    }

    cborTags(): Tag[] {
        return [URI_TAG];
    }

    untaggedCbor(): Cbor {
        return toCbor(this.#uri);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return [URI_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): URI {
        return new URI(cbor.toText());
    }

    static fromCbor(cbor: Cbor): URI {
        return decodeTaggedCbor(URI, cbor);
    }

    toString(): string {
        return this.#uri;
    }

    equals(other: unknown): boolean {
        return other instanceof URI && other.#uri === this.#uri;
    }
}
