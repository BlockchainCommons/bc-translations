import type { RandomNumberGenerator } from '@bc/rand';
import { schnorr, secp256k1 } from '@noble/curves/secp256k1.js';

import { bytesToHex, type BytesLike, requireLength } from './bytes.js';
import { hkdfHmacSha256 } from './hash.js';

export const ECDSA_PRIVATE_KEY_SIZE = 32;
export const ECDSA_PUBLIC_KEY_SIZE = 33;
export const ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE = 65;
export const ECDSA_MESSAGE_HASH_SIZE = 32;
export const ECDSA_SIGNATURE_SIZE = 64;
export const SCHNORR_PUBLIC_KEY_SIZE = 32;

function requireEcdsaPrivateKey(privateKey: BytesLike): Uint8Array {
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

/** Generate a new ECDSA private key using the given random number generator. */
export function ecdsaNewPrivateKeyUsing(
    rng: RandomNumberGenerator,
): Uint8Array {
    return rng.randomData(ECDSA_PRIVATE_KEY_SIZE);
}

/** Derives the ECDSA public key from the given private key. */
export function ecdsaPublicKeyFromPrivateKey(
    privateKey: BytesLike,
): Uint8Array {
    return secp256k1.getPublicKey(requireEcdsaPrivateKey(privateKey), true);
}

/** Decompresses the given ECDSA public key. */
export function ecdsaDecompressPublicKey(
    compressedPublicKey: BytesLike,
): Uint8Array {
    const pk = requireLength(
        compressedPublicKey,
        ECDSA_PUBLIC_KEY_SIZE,
        '33 or 65 bytes, serialized according to the spec',
    );
    return secp256k1.Point.fromHex(bytesToHex(pk)).toBytes(false);
}

/** Compresses the given ECDSA public key. */
export function ecdsaCompressPublicKey(
    uncompressedPublicKey: BytesLike,
): Uint8Array {
    const pk = requireLength(
        uncompressedPublicKey,
        ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
        '33 bytes, serialized according to the spec',
    );
    return secp256k1.Point.fromHex(bytesToHex(pk)).toBytes(true);
}

/** Derives the ECDSA private key from key material with HKDF-SHA256("signing"). */
export function ecdsaDerivePrivateKey(keyMaterial: BytesLike): Uint8Array {
    return hkdfHmacSha256(keyMaterial, 'signing', ECDSA_PRIVATE_KEY_SIZE);
}

/** Derives the Schnorr public key from the given private key. */
export function schnorrPublicKeyFromPrivateKey(
    privateKey: BytesLike,
): Uint8Array {
    return schnorr.getPublicKey(requireEcdsaPrivateKey(privateKey));
}
