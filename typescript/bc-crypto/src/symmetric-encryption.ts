import {
    createCipheriv,
    createDecipheriv,
} from 'node:crypto';

import { type BytesLike, requireLength, toBytes } from './bytes.js';
import { AeadError } from './error.js';

export const SYMMETRIC_KEY_SIZE = 32;
export const SYMMETRIC_NONCE_SIZE = 12;
export const SYMMETRIC_AUTH_SIZE = 16;

/**
 * Symmetrically encrypts plaintext with ChaCha20-Poly1305 and additional
 * authenticated data (AAD).
 */
export function aeadChaCha20Poly1305EncryptWithAad(
    plaintext: BytesLike,
    key: BytesLike,
    nonce: BytesLike,
    aad: BytesLike,
): [Uint8Array, Uint8Array] {
    const k = requireLength(
        key,
        SYMMETRIC_KEY_SIZE,
        'invalid ChaCha20-Poly1305 key length',
    );
    const n = requireLength(
        nonce,
        SYMMETRIC_NONCE_SIZE,
        'invalid ChaCha20-Poly1305 nonce length',
    );
    const plain = toBytes(plaintext);
    const aadBytes = toBytes(aad);

    const cipher = createCipheriv('chacha20-poly1305', k, n, {
        authTagLength: SYMMETRIC_AUTH_SIZE,
    });
    if (aadBytes.length > 0) {
        cipher.setAAD(aadBytes, { plaintextLength: plain.length });
    }

    const ciphertext = Buffer.concat([cipher.update(plain), cipher.final()]);
    const auth = cipher.getAuthTag();

    return [new Uint8Array(ciphertext), new Uint8Array(auth)];
}

/**
 * Symmetrically encrypts plaintext with ChaCha20-Poly1305.
 */
export function aeadChaCha20Poly1305Encrypt(
    plaintext: BytesLike,
    key: BytesLike,
    nonce: BytesLike,
): [Uint8Array, Uint8Array] {
    return aeadChaCha20Poly1305EncryptWithAad(
        plaintext,
        key,
        nonce,
        new Uint8Array(0),
    );
}

/**
 * Symmetrically decrypts ciphertext with ChaCha20-Poly1305 and additional
 * authenticated data (AAD).
 */
export function aeadChaCha20Poly1305DecryptWithAad(
    ciphertext: BytesLike,
    key: BytesLike,
    nonce: BytesLike,
    aad: BytesLike,
    auth: BytesLike,
): Uint8Array {
    const k = requireLength(
        key,
        SYMMETRIC_KEY_SIZE,
        'invalid ChaCha20-Poly1305 key length',
    );
    const n = requireLength(
        nonce,
        SYMMETRIC_NONCE_SIZE,
        'invalid ChaCha20-Poly1305 nonce length',
    );
    const tag = requireLength(
        auth,
        SYMMETRIC_AUTH_SIZE,
        'invalid ChaCha20-Poly1305 auth length',
    );

    const cipher = toBytes(ciphertext);
    const aadBytes = toBytes(aad);

    const decipher = createDecipheriv('chacha20-poly1305', k, n, {
        authTagLength: SYMMETRIC_AUTH_SIZE,
    });
    if (aadBytes.length > 0) {
        decipher.setAAD(aadBytes, { plaintextLength: cipher.length });
    }
    decipher.setAuthTag(tag);

    try {
        const plaintext = Buffer.concat([
            decipher.update(cipher),
            decipher.final(),
        ]);
        return new Uint8Array(plaintext);
    } catch {
        throw new AeadError();
    }
}

/**
 * Symmetrically decrypts ciphertext with ChaCha20-Poly1305.
 */
export function aeadChaCha20Poly1305Decrypt(
    ciphertext: BytesLike,
    key: BytesLike,
    nonce: BytesLike,
    auth: BytesLike,
): Uint8Array {
    return aeadChaCha20Poly1305DecryptWithAad(
        ciphertext,
        key,
        nonce,
        new Uint8Array(0),
        auth,
    );
}
