import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { BCComponentsError } from '../error.js';

export enum HashType {
    SHA256 = 0,
    SHA512 = 1,
}

export function hashTypeFromCbor(cbor: Cbor): HashType {
    const value = Number(cbor.toInteger());
    switch (value) {
        case 0:
            return HashType.SHA256;
        case 1:
            return HashType.SHA512;
        default:
            throw BCComponentsError.general('Invalid HashType');
    }
}

export function hashTypeToCbor(hashType: HashType): Cbor {
    return toCbor(hashType);
}

export function hashTypeToString(hashType: HashType): string {
    switch (hashType) {
        case HashType.SHA256:
            return 'SHA256';
        case HashType.SHA512:
            return 'SHA512';
    }
}
