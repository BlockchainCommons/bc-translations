import {
    type RandomNumberGenerator,
    SecureRandomNumberGenerator,
} from '@bc/rand';
import { schnorr, secp256k1 } from '@noble/curves/secp256k1.js';

import { type BytesLike, requireLength, toBytes } from './bytes.js';
import {
    ECDSA_PRIVATE_KEY_SIZE,
    SCHNORR_PUBLIC_KEY_SIZE,
} from './ecdsa-keys.js';

export const SCHNORR_SIGNATURE_SIZE = 64;

function bytesToBigInt(bytes: Uint8Array): bigint {
    let result = 0n;
    for (const byte of bytes) {
        result = (result << 8n) | BigInt(byte);
    }
    return result;
}

function requirePrivateKey(privateKey: BytesLike): Uint8Array {
    const key = requireLength(
        privateKey,
        ECDSA_PRIVATE_KEY_SIZE,
        '32 bytes, within curve order',
    );
    if (!secp256k1.utils.isValidSecretKey(key)) {
        throw new RangeError('32 bytes, within curve order');
    }
    return key;
}

function requireXOnlyPublicKey(publicKey: BytesLike): Uint8Array {
    const key = requireLength(
        publicKey,
        SCHNORR_PUBLIC_KEY_SIZE,
        '32 bytes, serialized according to the spec',
    );
    // Mirror Rust panic behavior for invalid x-only public keys.
    schnorr.utils.lift_x(bytesToBigInt(key));
    return key;
}

/** Signs with secure auxiliary randomness. */
export function schnorrSign(
    ecdsaPrivateKey: BytesLike,
    message: BytesLike,
): Uint8Array {
    const rng = new SecureRandomNumberGenerator();
    return schnorrSignUsing(ecdsaPrivateKey, message, rng);
}

/** Signs using RNG-provided auxiliary randomness. */
export function schnorrSignUsing(
    ecdsaPrivateKey: BytesLike,
    message: BytesLike,
    rng: RandomNumberGenerator,
): Uint8Array {
    const auxRand = rng.randomData(32);
    return schnorrSignWithAuxRand(ecdsaPrivateKey, message, auxRand);
}

/** Signs using explicit 32-byte auxiliary randomness. */
export function schnorrSignWithAuxRand(
    ecdsaPrivateKey: BytesLike,
    message: BytesLike,
    auxRand: BytesLike,
): Uint8Array {
    const key = requirePrivateKey(ecdsaPrivateKey);
    const aux = requireLength(auxRand, 32, 'invalid aux rand length');
    return schnorr.sign(toBytes(message), key, aux);
}

/** Verifies a BIP340 Schnorr signature. */
export function schnorrVerify(
    schnorrPublicKey: BytesLike,
    schnorrSignature: BytesLike,
    message: BytesLike,
): boolean {
    const pk = requireXOnlyPublicKey(schnorrPublicKey);
    const sig = requireLength(
        schnorrSignature,
        SCHNORR_SIGNATURE_SIZE,
        'invalid schnorr signature length',
    );
    return schnorr.verify(sig, toBytes(message), pk);
}
