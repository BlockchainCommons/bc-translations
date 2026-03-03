import {
    pbkdf2HmacSha256,
    pbkdf2HmacSha512,
} from '@bc/crypto';
import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { Salt } from '../salt.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import type { EncryptedMessage } from '../symmetric/encrypted-message.js';
import { HashType, hashTypeFromCbor, hashTypeToCbor, hashTypeToString } from './hash-type.js';
import type { KeyDerivation } from './key-derivation.js';
import { KeyDerivationMethod } from './key-derivation-method.js';
import { SALT_LEN } from './hkdf-params.js';

export class PBKDF2Params implements KeyDerivation {
    readonly #salt: Salt;
    readonly #iterations: number;
    readonly #hashType: HashType;

    constructor(salt: Salt, iterations: number, hashType: HashType) {
        this.#salt = salt;
        this.#iterations = iterations;
        this.#hashType = hashType;
    }

    static new(): PBKDF2Params {
        return PBKDF2Params.newOpt(
            Salt.newWithLen(SALT_LEN),
            100_000,
            HashType.SHA256,
        );
    }

    static newOpt(
        salt: Salt,
        iterations: number,
        hashType: HashType,
    ): PBKDF2Params {
        return new PBKDF2Params(salt, iterations, hashType);
    }

    salt(): Salt {
        return this.#salt;
    }

    iterations(): number {
        return this.#iterations;
    }

    hashType(): HashType {
        return this.#hashType;
    }

    index(): number {
        return KeyDerivationMethod.PBKDF2 as number;
    }

    lock(contentKey: SymmetricKey, secret: Uint8Array): EncryptedMessage {
        const derivedKey = SymmetricKey.fromData(
            this.#hashType === HashType.SHA256
                ? pbkdf2HmacSha256(
                    secret,
                    this.#salt.asBytes(),
                    this.#iterations,
                    32,
                )
                : pbkdf2HmacSha512(
                    secret,
                    this.#salt.asBytes(),
                    this.#iterations,
                    32,
                ),
        );
        const encodedMethod = this.toCbor().toData();
        return derivedKey.encrypt(contentKey.data, encodedMethod);
    }

    unlock(encryptedMessage: EncryptedMessage, secret: Uint8Array): SymmetricKey {
        const derivedKey = SymmetricKey.fromData(
            this.#hashType === HashType.SHA256
                ? pbkdf2HmacSha256(
                    secret,
                    this.#salt.asBytes(),
                    this.#iterations,
                    32,
                )
                : pbkdf2HmacSha512(
                    secret,
                    this.#salt.asBytes(),
                    this.#iterations,
                    32,
                ),
        );
        return SymmetricKey.fromData(derivedKey.decrypt(encryptedMessage));
    }

    toCbor(): Cbor {
        return toCbor([
            KeyDerivationMethod.PBKDF2,
            this.#salt.taggedCbor(),
            this.#iterations,
            hashTypeToCbor(this.#hashType),
        ]);
    }

    static fromCbor(cbor: Cbor): PBKDF2Params {
        const a = cbor.toArray();
        if (a.length !== 4) {
            throw new Error('Invalid PBKDF2Params');
        }
        const salt = Salt.fromCbor(a[1]!);
        const iterations = Number(a[2]!.toInteger());
        const hashType = hashTypeFromCbor(a[3]!);
        return new PBKDF2Params(salt, iterations, hashType);
    }

    toString(): string {
        return `PBKDF2(${hashTypeToString(this.#hashType)})`;
    }
}
