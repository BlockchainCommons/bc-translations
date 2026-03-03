import { scryptWithParams } from '@bc/crypto';
import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { Salt } from '../salt.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import type { EncryptedMessage } from '../symmetric/encrypted-message.js';
import type { KeyDerivation } from './key-derivation.js';
import { KeyDerivationMethod } from './key-derivation-method.js';
import { SALT_LEN } from './hkdf-params.js';

export class ScryptParams implements KeyDerivation {
    readonly #salt: Salt;
    readonly #logN: number;
    readonly #r: number;
    readonly #p: number;

    constructor(salt: Salt, logN: number, r: number, p: number) {
        this.#salt = salt;
        this.#logN = logN;
        this.#r = r;
        this.#p = p;
    }

    static new(): ScryptParams {
        return ScryptParams.newOpt(Salt.newWithLen(SALT_LEN), 15, 8, 1);
    }

    static newOpt(salt: Salt, logN: number, r: number, p: number): ScryptParams {
        return new ScryptParams(salt, logN, r, p);
    }

    salt(): Salt {
        return this.#salt;
    }

    logN(): number {
        return this.#logN;
    }

    r(): number {
        return this.#r;
    }

    p(): number {
        return this.#p;
    }

    index(): number {
        return KeyDerivationMethod.Scrypt as number;
    }

    lock(contentKey: SymmetricKey, secret: Uint8Array): EncryptedMessage {
        const derivedKey = SymmetricKey.fromData(
            scryptWithParams(
                secret,
                this.#salt.asBytes(),
                32,
                this.#logN,
                this.#r,
                this.#p,
            ),
        );
        const encodedMethod = this.toCbor().toData();
        return derivedKey.encrypt(contentKey.data, encodedMethod);
    }

    unlock(encryptedMessage: EncryptedMessage, secret: Uint8Array): SymmetricKey {
        const derivedKey = SymmetricKey.fromData(
            scryptWithParams(
                secret,
                this.#salt.asBytes(),
                32,
                this.#logN,
                this.#r,
                this.#p,
            ),
        );
        return SymmetricKey.fromData(derivedKey.decrypt(encryptedMessage));
    }

    toCbor(): Cbor {
        return toCbor([
            KeyDerivationMethod.Scrypt,
            this.#salt.taggedCbor(),
            this.#logN,
            this.#r,
            this.#p,
        ]);
    }

    static fromCbor(cbor: Cbor): ScryptParams {
        const a = cbor.toArray();
        if (a.length !== 5) {
            throw new Error('Invalid ScryptParams');
        }
        const salt = Salt.fromCbor(a[1]!);
        const logN = Number(a[2]!.toInteger());
        const r = Number(a[3]!.toInteger());
        const p = Number(a[4]!.toInteger());
        return new ScryptParams(salt, logN, r, p);
    }

    toString(): string {
        return 'Scrypt';
    }
}
