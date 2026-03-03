import { type Cbor, toByteString } from '@bc/dcbor';

import { requireLength, bytesEqual, hexEncode } from '../utils.js';

export class AuthenticationTag {
    static readonly AUTHENTICATION_TAG_SIZE = 16;

    readonly #data: Uint8Array;

    constructor(data: Uint8Array) {
        this.#data = requireLength(
            data,
            AuthenticationTag.AUTHENTICATION_TAG_SIZE,
            'AuthenticationTag',
        );
    }

    static fromData(data: Uint8Array): AuthenticationTag {
        return new AuthenticationTag(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): AuthenticationTag {
        return AuthenticationTag.fromData(data);
    }

    get data(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    asBytes(): Uint8Array {
        return this.data;
    }

    equals(other: unknown): boolean {
        return (
            other instanceof AuthenticationTag &&
            bytesEqual(this.#data, other.#data)
        );
    }

    toCbor(): Cbor {
        return toByteString(this.#data);
    }

    static fromCbor(cbor: Cbor): AuthenticationTag {
        return AuthenticationTag.fromData(cbor.toByteString());
    }

    toString(): string {
        return `AuthenticationTag(${hexEncode(this.#data)})`;
    }
}
