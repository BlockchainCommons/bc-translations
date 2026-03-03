import { argon2id } from '@bc/crypto';
import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { Salt } from '../salt.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import type { EncryptedMessage } from '../symmetric/encrypted-message.js';
import type { KeyDerivation } from './key-derivation.js';
import { KeyDerivationMethod } from './key-derivation-method.js';
import { SALT_LEN } from './hkdf-params.js';

export class Argon2idParams implements KeyDerivation {
    readonly #salt: Salt;

    constructor(salt: Salt) {
        this.#salt = salt;
    }

    static new(): Argon2idParams {
        return Argon2idParams.newOpt(Salt.newWithLen(SALT_LEN));
    }

    static newOpt(salt: Salt): Argon2idParams {
        return new Argon2idParams(salt);
    }

    salt(): Salt {
        return this.#salt;
    }

    index(): number {
        return KeyDerivationMethod.Argon2id as number;
    }

    lock(contentKey: SymmetricKey, secret: Uint8Array): EncryptedMessage {
        const derivedKey = SymmetricKey.fromData(
            argon2id(secret, this.#salt.asBytes(), 32),
        );
        const encodedMethod = this.toCbor().toData();
        return derivedKey.encrypt(contentKey.data, encodedMethod);
    }

    unlock(encryptedMessage: EncryptedMessage, secret: Uint8Array): SymmetricKey {
        const derivedKey = SymmetricKey.fromData(
            argon2id(secret, this.#salt.asBytes(), 32),
        );
        return SymmetricKey.fromData(derivedKey.decrypt(encryptedMessage));
    }

    toCbor(): Cbor {
        return toCbor([KeyDerivationMethod.Argon2id, this.#salt.taggedCbor()]);
    }

    static fromCbor(cbor: Cbor): Argon2idParams {
        const a = cbor.toArray();
        if (a.length !== 2) {
            throw new Error('Invalid Argon2idParams');
        }
        return new Argon2idParams(Salt.fromCbor(a[1]!));
    }

    toString(): string {
        return 'Argon2id';
    }
}
