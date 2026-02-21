/**
 * `@bc/crypto` exposes a uniform API for cryptographic primitives used in
 * higher-level Blockchain Commons projects.
 *
 * @packageDocumentation
 */

export { BCryptoError, AeadError } from './error.js';
export type { BytesLike } from './bytes.js';

export {
    CRC32_SIZE,
    SHA256_SIZE,
    SHA512_SIZE,
    crc32,
    crc32Data,
    crc32Bytes,
    sha256,
    doubleSha256,
    sha512,
    hmacSha256,
    hmacSha512,
    pbkdf2HmacSha256,
    pbkdf2HmacSha512,
    hkdfHmacSha256,
    hkdfHmacSha512,
} from './hash.js';

export { memzero, memzeroAll } from './memzero.js';

export {
    SYMMETRIC_KEY_SIZE,
    SYMMETRIC_NONCE_SIZE,
    SYMMETRIC_AUTH_SIZE,
    aeadChaCha20Poly1305EncryptWithAad,
    aeadChaCha20Poly1305Encrypt,
    aeadChaCha20Poly1305DecryptWithAad,
    aeadChaCha20Poly1305Decrypt,
} from './symmetric-encryption.js';

export {
    GENERIC_PRIVATE_KEY_SIZE,
    GENERIC_PUBLIC_KEY_SIZE,
    X25519_PRIVATE_KEY_SIZE,
    X25519_PUBLIC_KEY_SIZE,
    deriveAgreementPrivateKey,
    deriveSigningPrivateKey,
    x25519NewPrivateKeyUsing,
    x25519PublicKeyFromPrivateKey,
    x25519SharedKey,
} from './public-key-encryption.js';

export {
    ECDSA_PRIVATE_KEY_SIZE,
    ECDSA_PUBLIC_KEY_SIZE,
    ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
    ECDSA_MESSAGE_HASH_SIZE,
    ECDSA_SIGNATURE_SIZE,
    SCHNORR_PUBLIC_KEY_SIZE,
    ecdsaNewPrivateKeyUsing,
    ecdsaPublicKeyFromPrivateKey,
    ecdsaDecompressPublicKey,
    ecdsaCompressPublicKey,
    ecdsaDerivePrivateKey,
    schnorrPublicKeyFromPrivateKey,
} from './ecdsa-keys.js';

export { ecdsaSign, ecdsaVerify } from './ecdsa-signing.js';

export {
    SCHNORR_SIGNATURE_SIZE,
    schnorrSign,
    schnorrSignUsing,
    schnorrSignWithAuxRand,
    schnorrVerify,
} from './schnorr-signing.js';

export {
    ED25519_PRIVATE_KEY_SIZE,
    ED25519_PUBLIC_KEY_SIZE,
    ED25519_SIGNATURE_SIZE,
    ed25519NewPrivateKeyUsing,
    ed25519PublicKeyFromPrivateKey,
    ed25519Sign,
    ed25519Verify,
} from './ed25519-signing.js';

export { scrypt, scryptWithParams } from './scrypt.js';

export { argon2id } from './argon.js';
