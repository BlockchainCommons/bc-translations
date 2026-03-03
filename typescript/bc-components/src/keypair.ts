import type { RandomNumberGenerator } from '@bc/rand';

import { EncapsulationScheme } from './encapsulation/encapsulation-scheme.js';
import { type Result } from './error.js';
import { PrivateKeys } from './private-keys.js';
import { PublicKeys } from './public-keys.js';
import { SignatureScheme } from './signing/signature-scheme.js';

export function keypair(): [PrivateKeys, PublicKeys] {
    return keypairOpt(SignatureScheme.default(), EncapsulationScheme.default());
}

export function keypairUsing(
    rng: RandomNumberGenerator,
): Result<[PrivateKeys, PublicKeys]> {
    return keypairOptUsing(
        SignatureScheme.default(),
        EncapsulationScheme.default(),
        rng,
    );
}

export function keypairOpt(
    signatureScheme: SignatureScheme,
    encapsulationScheme: EncapsulationScheme,
): [PrivateKeys, PublicKeys] {
    const [signingPrivateKey, signingPublicKey] = signatureScheme.keypair();
    const [encapsulationPrivateKey, encapsulationPublicKey] = encapsulationScheme.keypair();
    const privateKeys = PrivateKeys.withKeys(
        signingPrivateKey,
        encapsulationPrivateKey,
    );
    const publicKeys = PublicKeys.new(signingPublicKey, encapsulationPublicKey);
    return [privateKeys, publicKeys];
}

export function keypairOptUsing(
    signatureScheme: SignatureScheme,
    encapsulationScheme: EncapsulationScheme,
    rng: RandomNumberGenerator,
): Result<[PrivateKeys, PublicKeys]> {
    const [signingPrivateKey, signingPublicKey] = signatureScheme.keypairUsing(rng, '');
    const [encapsulationPrivateKey, encapsulationPublicKey] = encapsulationScheme.keypairUsing(rng);
    const privateKeys = PrivateKeys.withKeys(
        signingPrivateKey,
        encapsulationPrivateKey,
    );
    const publicKeys = PublicKeys.new(signingPublicKey, encapsulationPublicKey);
    return [privateKeys, publicKeys];
}
