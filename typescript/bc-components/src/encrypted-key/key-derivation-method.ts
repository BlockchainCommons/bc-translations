import { type Cbor } from '@bc/dcbor';

import { BCComponentsError } from '../error.js';

export enum KeyDerivationMethod {
    HKDF = 0,
    PBKDF2 = 1,
    Scrypt = 2,
    Argon2id = 3,
}

export function keyDerivationMethodIndex(method: KeyDerivationMethod): number {
    return method as number;
}

export function keyDerivationMethodFromIndex(index: number): KeyDerivationMethod | undefined {
    switch (index) {
        case 0:
            return KeyDerivationMethod.HKDF;
        case 1:
            return KeyDerivationMethod.PBKDF2;
        case 2:
            return KeyDerivationMethod.Scrypt;
        case 3:
            return KeyDerivationMethod.Argon2id;
        default:
            return undefined;
    }
}

export function keyDerivationMethodFromCbor(cbor: Cbor): KeyDerivationMethod {
    const index = Number(cbor.toInteger());
    const method = keyDerivationMethodFromIndex(index);
    if (method === undefined) {
        throw BCComponentsError.general('Invalid KeyDerivationMethod');
    }
    return method;
}

export function keyDerivationMethodToString(method: KeyDerivationMethod): string {
    switch (method) {
        case KeyDerivationMethod.HKDF:
            return 'HKDF';
        case KeyDerivationMethod.PBKDF2:
            return 'PBKDF2';
        case KeyDerivationMethod.Scrypt:
            return 'Scrypt';
        case KeyDerivationMethod.Argon2id:
            return 'Argon2id';
    }
}
