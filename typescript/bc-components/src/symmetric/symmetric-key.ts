import {
    aeadChaCha20Poly1305DecryptWithAad,
    aeadChaCha20Poly1305EncryptWithAad,
    SYMMETRIC_KEY_SIZE,
} from '@bc/crypto';
import { SecureRandomNumberGenerator, type RandomNumberGenerator } from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_SYMMETRIC_KEY } from '@bc/tags';

import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { bytesEqual, hexDecode, hexEncode, requireLength } from '../utils.js';
import { Nonce } from '../nonce.js';
import { AuthenticationTag } from './authentication-tag.js';
import { EncryptedMessage } from './encrypted-message.js';

const SYMMETRIC_KEY_TAG: Tag = createTag(TAG_SYMMETRIC_KEY, 'crypto-key');

export class SymmetricKey implements ReferenceProvider {
    static readonly SYMMETRIC_KEY_SIZE = 32;

    readonly #data: Uint8Array;

    constructor(data?: Uint8Array) {
        if (data === undefined) {
            const rng = new SecureRandomNumberGenerator();
            this.#data = rng.randomData(SYMMETRIC_KEY_SIZE);
            return;
        }
        this.#data = requireLength(data, SYMMETRIC_KEY_SIZE, 'SymmetricKey');
    }

    static new(): SymmetricKey {
        return new SymmetricKey();
    }

    static newUsing(rng: RandomNumberGenerator): SymmetricKey {
        return new SymmetricKey(rng.randomData(SYMMETRIC_KEY_SIZE));
    }

    static fromData(data: Uint8Array): SymmetricKey {
        return new SymmetricKey(new Uint8Array(data));
    }

    static fromDataRef(data: Uint8Array): SymmetricKey {
        return SymmetricKey.fromData(data);
    }

    static fromHex(hex: string): SymmetricKey {
        return SymmetricKey.fromData(hexDecode(hex));
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

    encrypt(
        plaintext: Uint8Array,
        aad?: Uint8Array,
        nonce?: Nonce,
    ): EncryptedMessage {
        const usedNonce = nonce ?? Nonce.new();
        const usedAad = aad ?? new Uint8Array(0);
        const [ciphertext, auth] = aeadChaCha20Poly1305EncryptWithAad(
            plaintext,
            this.#data,
            usedNonce.data,
            usedAad,
        );
        return new EncryptedMessage(
            ciphertext,
            usedAad,
            usedNonce,
            AuthenticationTag.fromData(auth),
        );
    }

    encryptWithDigest(
        plaintext: Uint8Array,
        digest: Digest,
        nonce?: Nonce,
    ): EncryptedMessage {
        return this.encrypt(plaintext, digest.taggedCborData(), nonce);
    }

    decrypt(message: EncryptedMessage): Uint8Array {
        return aeadChaCha20Poly1305DecryptWithAad(
            message.ciphertext,
            this.#data,
            message.nonce.data,
            message.aad,
            message.authenticationTag.data,
        );
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return other instanceof SymmetricKey && bytesEqual(this.#data, other.#data);
    }

    cborTags(): Tag[] {
        return [SYMMETRIC_KEY_TAG];
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
        return [SYMMETRIC_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): SymmetricKey {
        return SymmetricKey.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): SymmetricKey {
        return decodeTaggedCbor(SymmetricKey, cbor);
    }

    toString(): string {
        return `SymmetricKey(${this.reference().refHexShort()})`;
    }
}
