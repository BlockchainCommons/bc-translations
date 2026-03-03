import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { BCComponentsError } from '../error.js';
import { MLDSAPrivateKey } from './mldsa-private-key.js';
import { MLDSAPublicKey } from './mldsa-public-key.js';

export const MLDSA_LEVEL_VALUE = {
    mldsa44: 2,
    mldsa65: 3,
    mldsa87: 5,
} as const;

export type MLDSALevelName = keyof typeof MLDSA_LEVEL_VALUE;

export class MLDSA {
    static readonly mldsa44 = new MLDSA('mldsa44');
    static readonly mldsa65 = new MLDSA('mldsa65');
    static readonly mldsa87 = new MLDSA('mldsa87');

    readonly name: MLDSALevelName;

    private constructor(name: MLDSALevelName) {
        this.name = name;
    }

    value(): number {
        return MLDSA_LEVEL_VALUE[this.name];
    }

    static values(): MLDSA[] {
        return [MLDSA.mldsa44, MLDSA.mldsa65, MLDSA.mldsa87];
    }

    privateKeySize(): number {
        switch (this.name) {
            case 'mldsa44':
                return 2560;
            case 'mldsa65':
                return 4032;
            case 'mldsa87':
                return 4896;
        }
    }

    publicKeySize(): number {
        switch (this.name) {
            case 'mldsa44':
                return 1312;
            case 'mldsa65':
                return 1952;
            case 'mldsa87':
                return 2592;
        }
    }

    signatureSize(): number {
        switch (this.name) {
            case 'mldsa44':
                return 2420;
            case 'mldsa65':
                return 3293;
            case 'mldsa87':
                return 4595;
        }
    }

    keypair(): [MLDSAPrivateKey, MLDSAPublicKey] {
        return MLDSAPrivateKey.generate(this);
    }

    toCbor(): Cbor {
        return toCbor(this.value());
    }

    static fromCbor(cbor: Cbor): MLDSA {
        const value = Number(cbor.toInteger());
        return MLDSA.fromValue(value);
    }

    static fromValue(value: number): MLDSA {
        switch (value) {
            case 2:
                return MLDSA.mldsa44;
            case 3:
                return MLDSA.mldsa65;
            case 5:
                return MLDSA.mldsa87;
            default:
                throw BCComponentsError.postQuantum(`Invalid MLDSA level: ${value}`);
        }
    }

    equals(other: unknown): boolean {
        return other instanceof MLDSA && this.name === other.name;
    }

    toString(): string {
        switch (this.name) {
            case 'mldsa44':
                return 'MLDSA44';
            case 'mldsa65':
                return 'MLDSA65';
            case 'mldsa87':
                return 'MLDSA87';
        }
    }
}
