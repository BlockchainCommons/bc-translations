import {
    type Cbor,
    createTag,
    decodeCbor,
} from '@bc/dcbor';
import { TAG_ENCRYPTED_KEY } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { BCComponentsError, type Result } from '../error.js';
import { EncryptedMessage } from '../symmetric/encrypted-message.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { Argon2idParams } from './argon2id-params.js';
import { HKDFParams } from './hkdf-params.js';
import { KeyDerivationMethod, keyDerivationMethodFromCbor } from './key-derivation-method.js';
import { KeyDerivationParams } from './key-derivation-params.js';
import { PBKDF2Params } from './pbkdf2-params.js';
import { ScryptParams } from './scrypt-params.js';

const ENCRYPTED_KEY_TAG: Tag = createTag(TAG_ENCRYPTED_KEY, 'encrypted-key');

export class EncryptedKey {
    readonly #params: KeyDerivationParams;
    readonly #encryptedMessage: EncryptedMessage;

    constructor(params: KeyDerivationParams, encryptedMessage: EncryptedMessage) {
        this.#params = params;
        this.#encryptedMessage = encryptedMessage;
    }

    static lockOpt(
        params: KeyDerivationParams,
        secret: Uint8Array,
        contentKey: SymmetricKey,
    ): Result<EncryptedKey> {
        const encryptedMessage = params.lock(contentKey, secret);
        return new EncryptedKey(params, encryptedMessage);
    }

    static lock(
        method: KeyDerivationMethod,
        secret: Uint8Array,
        contentKey: SymmetricKey,
    ): Result<EncryptedKey> {
        switch (method) {
            case KeyDerivationMethod.HKDF:
                return EncryptedKey.lockOpt(
                    KeyDerivationParams.hkdf(HKDFParams.new()),
                    secret,
                    contentKey,
                );
            case KeyDerivationMethod.PBKDF2:
                return EncryptedKey.lockOpt(
                    KeyDerivationParams.pbkdf2(PBKDF2Params.new()),
                    secret,
                    contentKey,
                );
            case KeyDerivationMethod.Scrypt:
                return EncryptedKey.lockOpt(
                    KeyDerivationParams.scrypt(ScryptParams.new()),
                    secret,
                    contentKey,
                );
            case KeyDerivationMethod.Argon2id:
                return EncryptedKey.lockOpt(
                    KeyDerivationParams.argon2id(Argon2idParams.new()),
                    secret,
                    contentKey,
                );
        }
    }

    encryptedMessage(): EncryptedMessage {
        return this.#encryptedMessage;
    }

    aadCbor(): Cbor {
        const aad = this.#encryptedMessage.aad;
        if (aad.length === 0) {
            throw BCComponentsError.general('Missing AAD CBOR in EncryptedMessage');
        }
        return decodeCbor(aad);
    }

    unlock(secret: Uint8Array): Result<SymmetricKey> {
        const encryptedMessage = this.#encryptedMessage;
        const cbor = this.aadCbor();
        const array = cbor.toArray();
        const method = keyDerivationMethodFromCbor(array[0]!);
        switch (method) {
            case KeyDerivationMethod.HKDF:
                return HKDFParams.fromCbor(cbor).unlock(encryptedMessage, secret);
            case KeyDerivationMethod.PBKDF2:
                return PBKDF2Params.fromCbor(cbor).unlock(encryptedMessage, secret);
            case KeyDerivationMethod.Scrypt:
                return ScryptParams.fromCbor(cbor).unlock(encryptedMessage, secret);
            case KeyDerivationMethod.Argon2id:
                return Argon2idParams.fromCbor(cbor).unlock(encryptedMessage, secret);
        }
    }

    isPasswordBased(): boolean {
        return this.#params.isPasswordBased();
    }

    isSshAgent(): boolean {
        return this.#params.isSshAgent();
    }

    cborTags(): Tag[] {
        return [ENCRYPTED_KEY_TAG];
    }

    untaggedCbor(): Cbor {
        return this.#encryptedMessage.cbor();
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return [ENCRYPTED_KEY_TAG];
    }

    static fromUntaggedCbor(untaggedCbor: Cbor): EncryptedKey {
        const encryptedKey = EncryptedMessage.fromCbor(untaggedCbor);
        const params = KeyDerivationParams.fromCbor(decodeCbor(encryptedKey.aad));
        return new EncryptedKey(params, encryptedKey);
    }

    static fromCbor(cbor: Cbor): EncryptedKey {
        return decodeTaggedCbor(EncryptedKey, cbor);
    }

    toString(): string {
        return `EncryptedKey(${this.#params.toString()})`;
    }
}
