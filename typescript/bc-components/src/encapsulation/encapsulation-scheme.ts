import type { RandomNumberGenerator } from '@bc/rand';

import { BCComponentsError } from '../error.js';
import { X25519PrivateKey } from '../x25519/x25519-private-key.js';
import { EncapsulationPrivateKey } from './encapsulation-private-key.js';
import { EncapsulationPublicKey } from './encapsulation-public-key.js';
import { MLKEM } from '../mlkem/mlkem-level.js';

export type EncapsulationSchemeName =
    | 'x25519'
    | 'mlkem512'
    | 'mlkem768'
    | 'mlkem1024';

export class EncapsulationScheme {
    static readonly x25519 = new EncapsulationScheme('x25519');
    static readonly mlkem512 = new EncapsulationScheme('mlkem512');
    static readonly mlkem768 = new EncapsulationScheme('mlkem768');
    static readonly mlkem1024 = new EncapsulationScheme('mlkem1024');

    readonly name: EncapsulationSchemeName;

    private constructor(name: EncapsulationSchemeName) {
        this.name = name;
    }

    static default(): EncapsulationScheme {
        return EncapsulationScheme.x25519;
    }

    keypair(): [EncapsulationPrivateKey, EncapsulationPublicKey] {
        switch (this.name) {
            case 'x25519': {
                const [privateKey, publicKey] = X25519PrivateKey.keypair();
                return [
                    EncapsulationPrivateKey.fromX25519(privateKey),
                    EncapsulationPublicKey.fromX25519(publicKey),
                ];
            }
            case 'mlkem512': {
                const [privateKey, publicKey] = MLKEM.mlkem512.keypair();
                return [
                    EncapsulationPrivateKey.fromMlkem(privateKey),
                    EncapsulationPublicKey.fromMlkem(publicKey),
                ];
            }
            case 'mlkem768': {
                const [privateKey, publicKey] = MLKEM.mlkem768.keypair();
                return [
                    EncapsulationPrivateKey.fromMlkem(privateKey),
                    EncapsulationPublicKey.fromMlkem(publicKey),
                ];
            }
            case 'mlkem1024': {
                const [privateKey, publicKey] = MLKEM.mlkem1024.keypair();
                return [
                    EncapsulationPrivateKey.fromMlkem(privateKey),
                    EncapsulationPublicKey.fromMlkem(publicKey),
                ];
            }
        }
    }

    keypairUsing(rng: RandomNumberGenerator): [EncapsulationPrivateKey, EncapsulationPublicKey] {
        if (this.name !== 'x25519') {
            throw BCComponentsError.general(
                'Deterministic keypair generation not supported for this encapsulation scheme',
            );
        }
        const [privateKey, publicKey] = X25519PrivateKey.keypairUsing(rng);
        return [
            EncapsulationPrivateKey.fromX25519(privateKey),
            EncapsulationPublicKey.fromX25519(publicKey),
        ];
    }

    equals(other: unknown): boolean {
        return other instanceof EncapsulationScheme && this.name === other.name;
    }

    toString(): string {
        switch (this.name) {
            case 'x25519':
                return 'X25519';
            case 'mlkem512':
                return 'MLKEM512';
            case 'mlkem768':
                return 'MLKEM768';
            case 'mlkem1024':
                return 'MLKEM1024';
        }
    }
}
