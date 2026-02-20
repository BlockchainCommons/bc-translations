import type { RandomNumberGenerator } from '@bc/rand';
import { ed25519 } from '@noble/curves/ed25519.js';

import { type BytesLike, requireLength, toBytes } from './bytes.js';

export const ED25519_PUBLIC_KEY_SIZE = 32;
export const ED25519_PRIVATE_KEY_SIZE = 32;
export const ED25519_SIGNATURE_SIZE = 64;

function requirePrivateKey(privateKey: BytesLike): Uint8Array {
    const key = requireLength(
        privateKey,
        ED25519_PRIVATE_KEY_SIZE,
        'invalid ed25519 private key length',
    );
    if (!ed25519.utils.isValidSecretKey(key)) {
        throw new RangeError('invalid ed25519 private key');
    }
    return key;
}

/** Creates a new Ed25519 private key seed using rng. */
export function ed25519NewPrivateKeyUsing(
    rng: RandomNumberGenerator,
): Uint8Array {
    return rng.randomData(ED25519_PRIVATE_KEY_SIZE);
}

/** Derives Ed25519 public key from private key seed. */
export function ed25519PublicKeyFromPrivateKey(
    privateKey: BytesLike,
): Uint8Array {
    return ed25519.getPublicKey(requirePrivateKey(privateKey));
}

/** Signs message using Ed25519 private key seed. */
export function ed25519Sign(
    privateKey: BytesLike,
    message: BytesLike,
): Uint8Array {
    return ed25519.sign(toBytes(message), requirePrivateKey(privateKey));
}

/** Verifies Ed25519 signature for message and public key. */
export function ed25519Verify(
    publicKey: BytesLike,
    message: BytesLike,
    signature: BytesLike,
): boolean {
    const pk = requireLength(
        publicKey,
        ED25519_PUBLIC_KEY_SIZE,
        'invalid ed25519 public key length',
    );
    const sig = requireLength(
        signature,
        ED25519_SIGNATURE_SIZE,
        'invalid ed25519 signature length',
    );

    if (!ed25519.utils.isValidPublicKey(pk, false)) {
        throw new RangeError('invalid ed25519 public key');
    }

    return ed25519.verify(sig, toBytes(message), pk, { zip215: false });
}
