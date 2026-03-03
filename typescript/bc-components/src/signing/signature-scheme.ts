import type { RandomNumberGenerator } from '@bc/rand';

import { BCComponentsError, type Result } from '../error.js';
import { ECPrivateKey } from '../ec-key/ec-private-key.js';
import { Ed25519PrivateKey } from '../ed25519/ed25519-private-key.js';
import { MLDSA } from '../mldsa/mldsa-level.js';
import { SigningPrivateKey } from './signing-private-key.js';
import { SigningPublicKey } from './signing-public-key.js';

export type SignatureSchemeName =
    | 'schnorr'
    | 'ecdsa'
    | 'ed25519'
    | 'mldsa44'
    | 'mldsa65'
    | 'mldsa87'
    | 'ssh-ed25519'
    | 'ssh-dsa'
    | 'ssh-ecdsa-p256'
    | 'ssh-ecdsa-p384';

export class SignatureScheme {
    static readonly schnorr = new SignatureScheme('schnorr');
    static readonly ecdsa = new SignatureScheme('ecdsa');
    static readonly ed25519 = new SignatureScheme('ed25519');
    static readonly mldsa44 = new SignatureScheme('mldsa44');
    static readonly mldsa65 = new SignatureScheme('mldsa65');
    static readonly mldsa87 = new SignatureScheme('mldsa87');
    static readonly sshEd25519 = new SignatureScheme('ssh-ed25519');
    static readonly sshDsa = new SignatureScheme('ssh-dsa');
    static readonly sshEcdsaP256 = new SignatureScheme('ssh-ecdsa-p256');
    static readonly sshEcdsaP384 = new SignatureScheme('ssh-ecdsa-p384');

    readonly name: SignatureSchemeName;

    private constructor(name: SignatureSchemeName) {
        this.name = name;
    }

    static default(): SignatureScheme {
        return SignatureScheme.schnorr;
    }

    keypair(): [SigningPrivateKey, SigningPublicKey] {
        return this.keypairOpt('');
    }

    keypairOpt(_comment: string): [SigningPrivateKey, SigningPublicKey] {
        switch (this.name) {
            case 'schnorr': {
                const privateKey = SigningPrivateKey.newSchnorr(ECPrivateKey.new());
                return [privateKey, privateKey.publicKey()];
            }
            case 'ecdsa': {
                const privateKey = SigningPrivateKey.newEcdsa(ECPrivateKey.new());
                return [privateKey, privateKey.publicKey()];
            }
            case 'ed25519': {
                const privateKey = SigningPrivateKey.newEd25519(Ed25519PrivateKey.new());
                return [privateKey, privateKey.publicKey()];
            }
            case 'mldsa44': {
                const [privateKey, publicKey] = MLDSA.mldsa44.keypair();
                return [SigningPrivateKey.fromMlDsa(privateKey), SigningPublicKey.fromMlDsa(publicKey)];
            }
            case 'mldsa65': {
                const [privateKey, publicKey] = MLDSA.mldsa65.keypair();
                return [SigningPrivateKey.fromMlDsa(privateKey), SigningPublicKey.fromMlDsa(publicKey)];
            }
            case 'mldsa87': {
                const [privateKey, publicKey] = MLDSA.mldsa87.keypair();
                return [SigningPrivateKey.fromMlDsa(privateKey), SigningPublicKey.fromMlDsa(publicKey)];
            }
            case 'ssh-ed25519':
            case 'ssh-dsa':
            case 'ssh-ecdsa-p256':
            case 'ssh-ecdsa-p384':
                throw BCComponentsError.unsupported(
                    'SSH signature schemes are not supported in this translation',
                );
        }
    }

    keypairUsing(
        rng: RandomNumberGenerator,
        _comment = '',
    ): Result<[SigningPrivateKey, SigningPublicKey]> {
        switch (this.name) {
            case 'schnorr': {
                const privateKey = SigningPrivateKey.newSchnorr(ECPrivateKey.newUsing(rng));
                return [privateKey, privateKey.publicKey()];
            }
            case 'ecdsa': {
                const privateKey = SigningPrivateKey.newEcdsa(ECPrivateKey.newUsing(rng));
                return [privateKey, privateKey.publicKey()];
            }
            case 'ed25519': {
                const privateKey = SigningPrivateKey.newEd25519(Ed25519PrivateKey.newUsing(rng));
                return [privateKey, privateKey.publicKey()];
            }
            case 'mldsa44':
            case 'mldsa65':
            case 'mldsa87':
                throw BCComponentsError.general(
                    'Deterministic keypair generation not supported for this signature scheme',
                );
            case 'ssh-ed25519':
            case 'ssh-dsa':
            case 'ssh-ecdsa-p256':
            case 'ssh-ecdsa-p384':
                throw BCComponentsError.unsupported(
                    'SSH signature schemes are not supported in this translation',
                );
        }
    }

    equals(other: unknown): boolean {
        return other instanceof SignatureScheme && this.name === other.name;
    }

    toString(): string {
        switch (this.name) {
            case 'schnorr':
                return 'Schnorr';
            case 'ecdsa':
                return 'Ecdsa';
            case 'ed25519':
                return 'Ed25519';
            case 'mldsa44':
                return 'MLDSA44';
            case 'mldsa65':
                return 'MLDSA65';
            case 'mldsa87':
                return 'MLDSA87';
            case 'ssh-ed25519':
                return 'SshEd25519';
            case 'ssh-dsa':
                return 'SshDsa';
            case 'ssh-ecdsa-p256':
                return 'SshEcdsaP256';
            case 'ssh-ecdsa-p384':
                return 'SshEcdsaP384';
        }
    }
}
