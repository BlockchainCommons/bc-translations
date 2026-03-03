import { type Cbor, cbor as toCbor } from '@bc/dcbor';

import { BCComponentsError } from '../error.js';
import { MLKEMPrivateKey } from './mlkem-private-key.js';
import { MLKEMPublicKey } from './mlkem-public-key.js';

export const MLKEM_LEVEL_VALUE = {
    mlkem512: 512,
    mlkem768: 768,
    mlkem1024: 1024,
} as const;

export type MLKEMLevelName = keyof typeof MLKEM_LEVEL_VALUE;

export class MLKEM {
    static readonly mlkem512 = new MLKEM('mlkem512');
    static readonly mlkem768 = new MLKEM('mlkem768');
    static readonly mlkem1024 = new MLKEM('mlkem1024');

    readonly name: MLKEMLevelName;

    private constructor(name: MLKEMLevelName) {
        this.name = name;
    }

    value(): number {
        return MLKEM_LEVEL_VALUE[this.name];
    }

    static fromValue(value: number): MLKEM {
        switch (value) {
            case 512:
                return MLKEM.mlkem512;
            case 768:
                return MLKEM.mlkem768;
            case 1024:
                return MLKEM.mlkem1024;
            default:
                throw BCComponentsError.postQuantum(`Invalid MLKEM level: ${value}`);
        }
    }

    static fromCbor(cbor: Cbor): MLKEM {
        return MLKEM.fromValue(Number(cbor.toInteger()));
    }

    privateKeySize(): number {
        switch (this.name) {
            case 'mlkem512':
                return 1632;
            case 'mlkem768':
                return 2400;
            case 'mlkem1024':
                return 3168;
        }
    }

    publicKeySize(): number {
        switch (this.name) {
            case 'mlkem512':
                return 800;
            case 'mlkem768':
                return 1184;
            case 'mlkem1024':
                return 1568;
        }
    }

    sharedSecretSize(): number {
        return 32;
    }

    ciphertextSize(): number {
        switch (this.name) {
            case 'mlkem512':
                return 768;
            case 'mlkem768':
                return 1088;
            case 'mlkem1024':
                return 1568;
        }
    }

    keypair(): [MLKEMPrivateKey, MLKEMPublicKey] {
        return MLKEMPrivateKey.generate(this);
    }

    toCbor(): Cbor {
        return toCbor(this.value());
    }

    equals(other: unknown): boolean {
        return other instanceof MLKEM && this.name === other.name;
    }

    toString(): string {
        switch (this.name) {
            case 'mlkem512':
                return 'MLKEM512';
            case 'mlkem768':
                return 'MLKEM768';
            case 'mlkem1024':
                return 'MLKEM1024';
        }
    }
}
