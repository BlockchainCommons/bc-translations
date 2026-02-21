import { secp256k1 } from '@noble/curves/secp256k1.js';

import {
    bytesToHex,
    type BytesLike,
    requireLength,
    toBytes,
} from './bytes.js';
import {
    ECDSA_PUBLIC_KEY_SIZE,
    ECDSA_SIGNATURE_SIZE,
    requireEcdsaPrivateKey,
} from './ecdsa-keys.js';
import { doubleSha256 } from './hash.js';

/** ECDSA signs the given message using the given private key. */
export function ecdsaSign(
    privateKey: BytesLike,
    message: BytesLike,
): Uint8Array {
    const key = requireEcdsaPrivateKey(privateKey);
    const hash = doubleSha256(message);
    return secp256k1.sign(hash, key, {
        format: 'compact',
        prehash: false,
        lowS: true,
    });
}

/**
 * Verifies the given ECDSA signature using the given public key.
 * Returns `true` if the signature is valid, `false` otherwise.
 */
export function ecdsaVerify(
    publicKey: BytesLike,
    signature: BytesLike,
    message: BytesLike,
): boolean {
    const pk = requireLength(
        publicKey,
        ECDSA_PUBLIC_KEY_SIZE,
        '33 or 65 bytes, serialized according to the spec',
    );
    const sig = requireLength(
        signature,
        ECDSA_SIGNATURE_SIZE,
        '64 bytes, signature according to the spec',
    );

    // Validate public key encoding to mirror Rust parsing behavior.
    secp256k1.Point.fromHex(bytesToHex(pk));

    const hash = doubleSha256(toBytes(message));
    return secp256k1.verify(sig, hash, pk, {
        format: 'compact',
        prehash: false,
        lowS: true,
    });
}
