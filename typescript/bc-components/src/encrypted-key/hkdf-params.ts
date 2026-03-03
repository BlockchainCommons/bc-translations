import {
    hkdfHmacSha256,
    hkdfHmacSha512,
} from '@bc/crypto';
import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { Salt } from '../salt.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { type EncryptedMessage } from '../symmetric/encrypted-message.js';
import { HashType, hashTypeFromCbor, hashTypeToCbor, hashTypeToString } from './hash-type.js';
import { type KeyDerivation } from './key-derivation.js';
import { KeyDerivationMethod } from './key-derivation-method.js';

export const SALT_LEN = 16;

export class HKDFParams implements KeyDerivation {
    readonly #salt: Salt;
    readonly #hashType: HashType;

    constructor(salt: Salt, hashType: HashType) {
        this.#salt = salt;
        this.#hashType = hashType;
    }

    static new(): HKDFParams {
        return HKDFParams.newOpt(Salt.newWithLen(SALT_LEN), HashType.SHA256);
    }

    static newOpt(salt: Salt, hashType: HashType): HKDFParams {
        return new HKDFParams(salt, hashType);
    }

    salt(): Salt {
        return this.#salt;
    }

    hashType(): HashType {
        return this.#hashType;
    }

    index(): number {
        return KeyDerivationMethod.HKDF as number;
    }

    lock(contentKey: SymmetricKey, secret: Uint8Array): EncryptedMessage {
        const derivedKey = SymmetricKey.fromData(
            this.#hashType === HashType.SHA256
                ? hkdfHmacSha256(secret, this.#salt.asBytes(), 32)
                : hkdfHmacSha512(secret, this.#salt.asBytes(), 32),
        );
        const encodedMethod = this.toCbor().toData();
        return derivedKey.encrypt(contentKey.data, encodedMethod);
    }

    unlock(encryptedMessage: EncryptedMessage, secret: Uint8Array): SymmetricKey {
        const derivedKey = SymmetricKey.fromData(
            this.#hashType === HashType.SHA256
                ? hkdfHmacSha256(secret, this.#salt.asBytes(), 32)
                : hkdfHmacSha512(secret, this.#salt.asBytes(), 32),
        );
        return SymmetricKey.fromData(derivedKey.decrypt(encryptedMessage));
    }

    toCbor(): Cbor {
        return toCbor([
            KeyDerivationMethod.HKDF,
            this.#salt.taggedCbor(),
            hashTypeToCbor(this.#hashType),
        ]);
    }

    static fromCbor(cbor: Cbor): HKDFParams {
        const a = cbor.toArray();
        if (a.length !== 3) {
            throw new Error('Invalid HKDFParams');
        }
        const salt = Salt.fromCbor(a[1]!);
        const hashType = hashTypeFromCbor(a[2]!);
        return new HKDFParams(salt, hashType);
    }

    toString(): string {
        return `HKDF(${hashTypeToString(this.#hashType)})`;
    }
}
