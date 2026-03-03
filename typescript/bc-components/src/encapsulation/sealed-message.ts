import {
    type Cbor,
    cbor as toCbor,
    createTag,
} from '@bc/dcbor';
import { TAG_SEALED_MESSAGE } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import type { Decrypter, Encrypter } from '../encrypter.js';
import { EncryptedMessage } from '../symmetric/encrypted-message.js';
import { Nonce } from '../nonce.js';
import { EncapsulationCiphertext } from './encapsulation-ciphertext.js';
import { EncapsulationScheme } from './encapsulation-scheme.js';

const SEALED_MESSAGE_TAG: Tag = createTag(TAG_SEALED_MESSAGE, 'crypto-sealed');

export class SealedMessage {
    readonly #message: EncryptedMessage;
    readonly #encapsulatedKey: EncapsulationCiphertext;

    constructor(message: EncryptedMessage, encapsulatedKey: EncapsulationCiphertext) {
        this.#message = message;
        this.#encapsulatedKey = encapsulatedKey;
    }

    static new(plaintext: Uint8Array, recipient: Encrypter): SealedMessage {
        return SealedMessage.newWithAad(plaintext, recipient);
    }

    static newWithAad(
        plaintext: Uint8Array,
        recipient: Encrypter,
        aad?: Uint8Array,
    ): SealedMessage {
        return SealedMessage.newOpt(plaintext, recipient, aad, undefined);
    }

    static newOpt(
        plaintext: Uint8Array,
        recipient: Encrypter,
        aad?: Uint8Array,
        testNonce?: Nonce,
    ): SealedMessage {
        const [sharedKey, encapsulatedKey] = recipient.encapsulateNewSharedSecret();
        const message = sharedKey.encrypt(plaintext, aad, testNonce);
        return new SealedMessage(message, encapsulatedKey);
    }

    decrypt(privateKey: Decrypter): Uint8Array {
        const sharedKey = privateKey.decapsulateSharedSecret(this.#encapsulatedKey);
        return sharedKey.decrypt(this.#message);
    }

    encapsulationScheme(): EncapsulationScheme {
        return this.#encapsulatedKey.encapsulationScheme();
    }

    get message(): EncryptedMessage {
        return this.#message;
    }

    get encapsulatedKey(): EncapsulationCiphertext {
        return this.#encapsulatedKey;
    }

    cborTags(): Tag[] {
        return [SEALED_MESSAGE_TAG];
    }

    untaggedCbor(): Cbor {
        return toCbor([this.#message.cbor(), this.#encapsulatedKey.toCbor()]);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return [SEALED_MESSAGE_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): SealedMessage {
        const elements = cbor.toArray();
        if (elements.length !== 2) {
            throw new Error('SealedMessage must have two elements');
        }
        const message = EncryptedMessage.fromCbor(elements[0]!);
        const key = EncapsulationCiphertext.fromCbor(elements[1]!);
        return new SealedMessage(message, key);
    }

    static fromCbor(cbor: Cbor): SealedMessage {
        return decodeTaggedCbor(SealedMessage, cbor);
    }

    equals(other: unknown): boolean {
        return (
            other instanceof SealedMessage &&
            this.#message.equals(other.#message) &&
            this.#encapsulatedKey.equals(other.#encapsulatedKey)
        );
    }
}
