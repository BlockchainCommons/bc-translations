import {
    type Cbor,
    decodeCbor,
    cbor as toCbor,
    createTag,
    toByteString,
} from '@bc/dcbor';
import { TAG_ENCRYPTED } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData, fromTaggedUrString, toUrString } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type DigestProvider } from '../digest-provider.js';
import { bytesEqual, hexEncode } from '../utils.js';
import { AuthenticationTag } from './authentication-tag.js';
import { Nonce } from '../nonce.js';

const ENCRYPTED_TAG: Tag = createTag(TAG_ENCRYPTED, 'encrypted');

export class EncryptedMessage implements DigestProvider {
    readonly #ciphertext: Uint8Array;
    readonly #aad: Uint8Array;
    readonly #nonce: Nonce;
    readonly #auth: AuthenticationTag;

    constructor(
        ciphertext: Uint8Array,
        aad: Uint8Array,
        nonce: Nonce,
        auth: AuthenticationTag,
    ) {
        this.#ciphertext = new Uint8Array(ciphertext);
        this.#aad = new Uint8Array(aad);
        this.#nonce = nonce;
        this.#auth = auth;
    }

    static new(
        ciphertext: Uint8Array,
        aad: Uint8Array,
        nonce: Nonce,
        auth: AuthenticationTag,
    ): EncryptedMessage {
        return new EncryptedMessage(ciphertext, aad, nonce, auth);
    }

    get ciphertext(): Uint8Array {
        return new Uint8Array(this.#ciphertext);
    }

    get aad(): Uint8Array {
        return new Uint8Array(this.#aad);
    }

    get nonce(): Nonce {
        return this.#nonce;
    }

    get authenticationTag(): AuthenticationTag {
        return this.#auth;
    }

    aadCbor(): Cbor | undefined {
        if (this.#aad.length === 0) {
            return undefined;
        }
        try {
            return decodeCbor(this.#aad);
        } catch {
            return undefined;
        }
    }

    aadDigest(): Digest | undefined {
        const value = this.aadCbor();
        if (value === undefined) {
            return undefined;
        }
        try {
            return Digest.fromCbor(value);
        } catch {
            return undefined;
        }
    }

    hasDigest(): boolean {
        return this.aadDigest() !== undefined;
    }

    digest(): Digest {
        return this.aadDigest() ?? Digest.fromImage(this.#ciphertext);
    }

    equals(other: unknown): boolean {
        return (
            other instanceof EncryptedMessage &&
            bytesEqual(this.#ciphertext, other.#ciphertext) &&
            bytesEqual(this.#aad, other.#aad) &&
            this.#nonce.equals(other.#nonce) &&
            this.#auth.equals(other.#auth)
        );
    }

    cborTags(): Tag[] {
        return [ENCRYPTED_TAG];
    }

    untaggedCbor(): Cbor {
        const arr: Cbor[] = [
            toByteString(this.#ciphertext),
            toByteString(this.#nonce.data),
            toByteString(this.#auth.data),
        ];
        if (this.#aad.length > 0) {
            arr.push(toByteString(this.#aad));
        }
        return toCbor(arr);
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

    static fromURString(value: string): EncryptedMessage {
        return fromTaggedUrString(EncryptedMessage, value);
    }

    static cborTags(): Tag[] {
        return [ENCRYPTED_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): EncryptedMessage {
        const arr = cbor.toArray();
        if (arr.length < 3) {
            throw new Error('EncryptedMessage must have at least 3 elements');
        }
        const ciphertext = arr[0]!.toByteString();
        const nonce = Nonce.fromData(arr[1]!.toByteString());
        const auth = AuthenticationTag.fromData(arr[2]!.toByteString());
        const aad = arr[3]?.toByteString() ?? new Uint8Array(0);
        return new EncryptedMessage(ciphertext, aad, nonce, auth);
    }

    static fromCbor(cbor: Cbor): EncryptedMessage {
        return decodeTaggedCbor(EncryptedMessage, cbor);
    }

    toString(): string {
        return `EncryptedMessage(ciphertext=${hexEncode(this.#ciphertext)})`;
    }
}
