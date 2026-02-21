"""A uniform API for cryptographic primitives used in Blockchain Commons projects.

The various providers listed below may change, but the API this package
provides should be stable.

| Category                            | Algorithm              | Provider              |
|-------------------------------------|------------------------|-----------------------|
| Cryptographic digest                | SHA-256                | hashlib               |
| Cryptographic digest                | SHA-512                | hashlib               |
| Checksum                            | CRC-32                 | binascii              |
| Hashed Message Authentication Codes | HMAC-SHA-256           | hmac                  |
| Hashed Message Authentication Codes | HMAC-SHA-512           | hmac                  |
| Password Expansion                  | PBKDF2-HMAC-SHA-256   | hashlib               |
| Password Expansion                  | PBKDF2-HMAC-SHA-512   | hashlib               |
| Key Derivation                      | HKDF-HMAC-SHA-256     | hmac                  |
| Key Derivation                      | HKDF-HMAC-SHA-512     | hmac                  |
| Symmetric Encryption                | ChaCha20-Poly1305     | cryptography          |
| Key Agreement                       | X25519                | cryptography          |
| Memory Zeroing                      | N/A                   | built-in              |
| Password KDF                        | scrypt                | hashlib               |
"""

from .argon import argon2id
from .ecdsa_keys import (
    ECDSA_MESSAGE_HASH_SIZE,
    ECDSA_PRIVATE_KEY_SIZE,
    ECDSA_PUBLIC_KEY_SIZE,
    ECDSA_SIGNATURE_SIZE,
    ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
    SCHNORR_PUBLIC_KEY_SIZE,
    ecdsa_compress_public_key,
    ecdsa_decompress_public_key,
    ecdsa_derive_private_key,
    ecdsa_new_private_key_using,
    ecdsa_public_key_from_private_key,
    schnorr_public_key_from_private_key,
)
from .ecdsa_signing import ecdsa_sign, ecdsa_verify
from .ed25519_signing import (
    ED25519_PRIVATE_KEY_SIZE,
    ED25519_PUBLIC_KEY_SIZE,
    ED25519_SIGNATURE_SIZE,
    ed25519_new_private_key_using,
    ed25519_public_key_from_private_key,
    ed25519_sign,
    ed25519_verify,
)
from .error import AeadError, Error
from .hash import (
    CRC32_SIZE,
    SHA256_SIZE,
    SHA512_SIZE,
    crc32,
    crc32_data,
    crc32_data_opt,
    double_sha256,
    hkdf_hmac_sha256,
    hkdf_hmac_sha512,
    hmac_sha256,
    hmac_sha512,
    pbkdf2_hmac_sha256,
    pbkdf2_hmac_sha512,
    sha256,
    sha512,
)
from .memzero import memzero, memzero_vec_vec_u8
from .public_key_encryption import (
    GENERIC_PRIVATE_KEY_SIZE,
    GENERIC_PUBLIC_KEY_SIZE,
    X25519_PRIVATE_KEY_SIZE,
    X25519_PUBLIC_KEY_SIZE,
    derive_agreement_private_key,
    derive_signing_private_key,
    x25519_new_private_key_using,
    x25519_public_key_from_private_key,
    x25519_shared_key,
)
from .schnorr_signing import (
    SCHNORR_SIGNATURE_SIZE,
    schnorr_sign,
    schnorr_sign_using,
    schnorr_sign_with_aux_rand,
    schnorr_verify,
)
from .scrypt import scrypt, scrypt_opt
from .symmetric_encryption import (
    SYMMETRIC_AUTH_SIZE,
    SYMMETRIC_KEY_SIZE,
    SYMMETRIC_NONCE_SIZE,
    aead_chacha20_poly1305_decrypt,
    aead_chacha20_poly1305_decrypt_with_aad,
    aead_chacha20_poly1305_encrypt,
    aead_chacha20_poly1305_encrypt_with_aad,
)

__all__ = [
    "AeadError",
    "CRC32_SIZE",
    "ECDSA_MESSAGE_HASH_SIZE",
    "ECDSA_PRIVATE_KEY_SIZE",
    "ECDSA_PUBLIC_KEY_SIZE",
    "ECDSA_SIGNATURE_SIZE",
    "ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE",
    "ED25519_PRIVATE_KEY_SIZE",
    "ED25519_PUBLIC_KEY_SIZE",
    "ED25519_SIGNATURE_SIZE",
    "Error",
    "GENERIC_PRIVATE_KEY_SIZE",
    "GENERIC_PUBLIC_KEY_SIZE",
    "SCHNORR_PUBLIC_KEY_SIZE",
    "SCHNORR_SIGNATURE_SIZE",
    "SHA256_SIZE",
    "SHA512_SIZE",
    "SYMMETRIC_AUTH_SIZE",
    "SYMMETRIC_KEY_SIZE",
    "SYMMETRIC_NONCE_SIZE",
    "X25519_PRIVATE_KEY_SIZE",
    "X25519_PUBLIC_KEY_SIZE",
    "aead_chacha20_poly1305_decrypt",
    "aead_chacha20_poly1305_decrypt_with_aad",
    "aead_chacha20_poly1305_encrypt",
    "aead_chacha20_poly1305_encrypt_with_aad",
    "argon2id",
    "crc32",
    "crc32_data",
    "crc32_data_opt",
    "derive_agreement_private_key",
    "derive_signing_private_key",
    "double_sha256",
    "ecdsa_compress_public_key",
    "ecdsa_decompress_public_key",
    "ecdsa_derive_private_key",
    "ecdsa_new_private_key_using",
    "ecdsa_public_key_from_private_key",
    "ecdsa_sign",
    "ecdsa_verify",
    "ed25519_new_private_key_using",
    "ed25519_public_key_from_private_key",
    "ed25519_sign",
    "ed25519_verify",
    "hkdf_hmac_sha256",
    "hkdf_hmac_sha512",
    "hmac_sha256",
    "hmac_sha512",
    "memzero",
    "memzero_vec_vec_u8",
    "pbkdf2_hmac_sha256",
    "pbkdf2_hmac_sha512",
    "schnorr_public_key_from_private_key",
    "schnorr_sign",
    "schnorr_sign_using",
    "schnorr_sign_with_aux_rand",
    "schnorr_verify",
    "scrypt",
    "scrypt_opt",
    "sha256",
    "sha512",
    "x25519_new_private_key_using",
    "x25519_public_key_from_private_key",
    "x25519_shared_key",
]
