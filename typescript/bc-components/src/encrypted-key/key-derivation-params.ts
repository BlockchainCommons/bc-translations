import { type Cbor } from '@bc/dcbor';

import type { EncryptedMessage } from '../symmetric/encrypted-message.js';
import type { SymmetricKey } from '../symmetric/symmetric-key.js';
import { Argon2idParams } from './argon2id-params.js';
import { HKDFParams } from './hkdf-params.js';
import { KeyDerivationMethod, keyDerivationMethodFromIndex } from './key-derivation-method.js';
import { PBKDF2Params } from './pbkdf2-params.js';
import { ScryptParams } from './scrypt-params.js';
import { BCComponentsError } from '../error.js';

type KeyDerivationParamsVariant =
    | { type: 'hkdf'; params: HKDFParams }
    | { type: 'pbkdf2'; params: PBKDF2Params }
    | { type: 'scrypt'; params: ScryptParams }
    | { type: 'argon2id'; params: Argon2idParams };

export class KeyDerivationParams {
    readonly #variant: KeyDerivationParamsVariant;

    private constructor(variant: KeyDerivationParamsVariant) {
        this.#variant = variant;
    }

    static hkdf(params: HKDFParams): KeyDerivationParams {
        return new KeyDerivationParams({ type: 'hkdf', params });
    }

    static pbkdf2(params: PBKDF2Params): KeyDerivationParams {
        return new KeyDerivationParams({ type: 'pbkdf2', params });
    }

    static scrypt(params: ScryptParams): KeyDerivationParams {
        return new KeyDerivationParams({ type: 'scrypt', params });
    }

    static argon2id(params: Argon2idParams): KeyDerivationParams {
        return new KeyDerivationParams({ type: 'argon2id', params });
    }

    method(): KeyDerivationMethod {
        switch (this.#variant.type) {
            case 'hkdf':
                return KeyDerivationMethod.HKDF;
            case 'pbkdf2':
                return KeyDerivationMethod.PBKDF2;
            case 'scrypt':
                return KeyDerivationMethod.Scrypt;
            case 'argon2id':
                return KeyDerivationMethod.Argon2id;
        }
    }

    isPasswordBased(): boolean {
        return this.#variant.type !== 'hkdf';
    }

    isSshAgent(): boolean {
        return false;
    }

    lock(contentKey: SymmetricKey, secret: Uint8Array): EncryptedMessage {
        switch (this.#variant.type) {
            case 'hkdf':
                return this.#variant.params.lock(contentKey, secret);
            case 'pbkdf2':
                return this.#variant.params.lock(contentKey, secret);
            case 'scrypt':
                return this.#variant.params.lock(contentKey, secret);
            case 'argon2id':
                return this.#variant.params.lock(contentKey, secret);
        }
    }

    unlock(encryptedMessage: EncryptedMessage, secret: Uint8Array): SymmetricKey {
        switch (this.#variant.type) {
            case 'hkdf':
                return this.#variant.params.unlock(encryptedMessage, secret);
            case 'pbkdf2':
                return this.#variant.params.unlock(encryptedMessage, secret);
            case 'scrypt':
                return this.#variant.params.unlock(encryptedMessage, secret);
            case 'argon2id':
                return this.#variant.params.unlock(encryptedMessage, secret);
        }
    }

    toCbor(): Cbor {
        switch (this.#variant.type) {
            case 'hkdf':
                return this.#variant.params.toCbor();
            case 'pbkdf2':
                return this.#variant.params.toCbor();
            case 'scrypt':
                return this.#variant.params.toCbor();
            case 'argon2id':
                return this.#variant.params.toCbor();
        }
    }

    static fromCbor(cbor: Cbor): KeyDerivationParams {
        const a = cbor.toArray();
        const index = Number(a[0]!.toInteger());
        const method = keyDerivationMethodFromIndex(index);
        switch (method) {
            case KeyDerivationMethod.HKDF:
                return KeyDerivationParams.hkdf(HKDFParams.fromCbor(cbor));
            case KeyDerivationMethod.PBKDF2:
                return KeyDerivationParams.pbkdf2(PBKDF2Params.fromCbor(cbor));
            case KeyDerivationMethod.Scrypt:
                return KeyDerivationParams.scrypt(ScryptParams.fromCbor(cbor));
            case KeyDerivationMethod.Argon2id:
                return KeyDerivationParams.argon2id(Argon2idParams.fromCbor(cbor));
            default:
                throw BCComponentsError.general('Invalid KeyDerivationMethod');
        }
    }

    toString(): string {
        switch (this.#variant.type) {
            case 'hkdf':
                return this.#variant.params.toString();
            case 'pbkdf2':
                return this.#variant.params.toString();
            case 'scrypt':
                return this.#variant.params.toString();
            case 'argon2id':
                return this.#variant.params.toString();
        }
    }
}
