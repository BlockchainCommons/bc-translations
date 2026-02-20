import type { RandomNumberGenerator } from '@bc/rand';
import { x25519 } from '@noble/curves/ed25519.js';

import { type BytesLike, requireLength } from './bytes.js';
import { SYMMETRIC_KEY_SIZE } from './symmetric-encryption.js';
import { hkdfHmacSha256 } from './hash.js';

export const GENERIC_PRIVATE_KEY_SIZE = 32;
export const GENERIC_PUBLIC_KEY_SIZE = 32;
export const X25519_PRIVATE_KEY_SIZE = 32;
export const X25519_PUBLIC_KEY_SIZE = 32;

/**
 * Derive a 32-byte agreement private key from key material.
 */
export function deriveAgreementPrivateKey(
    keyMaterial: BytesLike,
): Uint8Array {
    return hkdfHmacSha256(keyMaterial, 'agreement', GENERIC_PRIVATE_KEY_SIZE);
}

/**
 * Derive a 32-byte signing private key from key material.
 */
export function deriveSigningPrivateKey(
    keyMaterial: BytesLike,
): Uint8Array {
    return hkdfHmacSha256(keyMaterial, 'signing', GENERIC_PUBLIC_KEY_SIZE);
}

/**
 * Create a new X25519 private key using the given random number generator.
 */
export function x25519NewPrivateKeyUsing(
    rng: RandomNumberGenerator,
): Uint8Array {
    return rng.randomData(X25519_PRIVATE_KEY_SIZE);
}

/**
 * Derive an X25519 public key from a private key.
 */
export function x25519PublicKeyFromPrivateKey(
    x25519PrivateKey: BytesLike,
): Uint8Array {
    const sk = requireLength(
        x25519PrivateKey,
        X25519_PRIVATE_KEY_SIZE,
        'invalid X25519 private key length',
    );
    return x25519.getPublicKey(sk);
}

/**
 * Compute the shared symmetric key from an X25519 private/public key pair.
 */
export function x25519SharedKey(
    x25519PrivateKey: BytesLike,
    x25519PublicKey: BytesLike,
): Uint8Array {
    const sk = requireLength(
        x25519PrivateKey,
        X25519_PRIVATE_KEY_SIZE,
        'invalid X25519 private key length',
    );
    const pk = requireLength(
        x25519PublicKey,
        X25519_PUBLIC_KEY_SIZE,
        'invalid X25519 public key length',
    );
    const sharedSecret = x25519.getSharedSecret(sk, pk);
    return hkdfHmacSha256(sharedSecret, 'agreement', SYMMETRIC_KEY_SIZE);
}
