"""Blockchain Commons Secure Components for Python.

Provides cryptographic primitives, identifiers, and CBOR serialization types
for the Blockchain Commons ecosystem.
"""

from ._compressed import Compressed
from ._digest import DIGEST_SIZE, Digest
from ._digest_provider import DigestProvider
from ._encrypter import Decrypter, Encrypter
from ._error import (
    BCComponentsError,
    CborError,
    CompressionError,
    CryptoError,
    DataTooShortError,
    GeneralError,
    InvalidDataError,
    InvalidSizeError,
    LevelMismatchError,
    PostQuantumError,
    SshError,
    SskrError,
    UriError,
)
from ._hkdf_rng import HKDFRng
from ._json import JSON
from ._keypair import keypair, keypair_opt, keypair_opt_using, keypair_using
from ._nonce import NONCE_SIZE, Nonce
from ._pq_utils import expand_bytes, random_bytes
from ._private_key_base import PrivateKeyBase
from ._private_key_data_provider import PrivateKeyDataProvider
from ._private_keys import PrivateKeys, PrivateKeysProvider
from ._public_keys import PublicKeys, PublicKeysProvider
from ._reference import Reference, ReferenceProvider
from ._salt import Salt
from ._seed import Seed
from ._sskr_mod import (
    SSKRGroupSpec,
    SSKRSecret,
    SSKRShare,
    SSKRSpec,
    sskr_combine,
    sskr_generate,
    sskr_generate_using,
)
from ._tags_registry import register_tags, register_tags_in
from .ec_key import (
    ECDSA_PRIVATE_KEY_SIZE,
    ECDSA_PUBLIC_KEY_SIZE,
    ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
    ECKey,
    ECKeyBase,
    ECPrivateKey,
    ECPublicKey,
    ECPublicKeyBase,
    ECUncompressedPublicKey,
    SCHNORR_PUBLIC_KEY_SIZE,
    SchnorrPublicKey,
)
from .ed25519 import Ed25519PrivateKey, Ed25519PublicKey
from .encapsulation import (
    EncapsulationCiphertext,
    EncapsulationPrivateKey,
    EncapsulationPublicKey,
    EncapsulationScheme,
    SealedMessage,
)
from .encrypted_key import (
    Argon2idParams,
    EncryptedKey,
    HashType,
    HKDFParams,
    KeyDerivation,
    KeyDerivationMethod,
    KeyDerivationParams,
    PBKDF2Params,
    SALT_LEN,
    ScryptParams,
)
from .id import ARID, URI, UUID, XID, XIDProvider
from .mldsa import MLDSALevel, MLDSAPrivateKey, MLDSAPublicKey, MLDSASignature
from .mlkem import MLKEMCiphertext, MLKEMLevel, MLKEMPrivateKey, MLKEMPublicKey
from .signing import (
    SchnorrSigningOptions,
    Signature,
    SignatureScheme,
    Signer,
    SigningOptions,
    SigningPrivateKey,
    SigningPublicKey,
    SshSigningOptions,
    Verifier,
)
from .symmetric import (
    AUTHENTICATION_TAG_SIZE,
    SYMMETRIC_KEY_SIZE,
    AuthenticationTag,
    EncryptedMessage,
    SymmetricKey,
)
from .x25519 import X25519PrivateKey, X25519PublicKey

# Re-export sskr.Error as SSKRError
from sskr import Error as SSKRError

__all__ = [
    # Error hierarchy
    "BCComponentsError",
    "CborError",
    "CompressionError",
    "CryptoError",
    "DataTooShortError",
    "GeneralError",
    "InvalidDataError",
    "InvalidSizeError",
    "LevelMismatchError",
    "PostQuantumError",
    "SshError",
    "SskrError",
    "SSKRError",
    "UriError",
    # Protocols / Traits
    "Decrypter",
    "DigestProvider",
    "ECKey",
    "ECKeyBase",
    "ECPublicKeyBase",
    "Encrypter",
    "KeyDerivation",
    "PrivateKeyDataProvider",
    "PrivateKeysProvider",
    "PublicKeysProvider",
    "ReferenceProvider",
    "Signer",
    "Verifier",
    "XIDProvider",
    # Core value types
    "Compressed",
    "DIGEST_SIZE",
    "Digest",
    "JSON",
    # MIN_SEED_LENGTH is a class attribute on Seed
    "NONCE_SIZE",
    "Nonce",
    "Reference",
    "Salt",
    "Seed",
    # Utilities
    "expand_bytes",
    "random_bytes",
    # Symmetric cryptography
    "AUTHENTICATION_TAG_SIZE",
    "AuthenticationTag",
    "EncryptedMessage",
    "SYMMETRIC_KEY_SIZE",
    "SymmetricKey",
    # X25519 key agreement
    "X25519PrivateKey",
    "X25519PublicKey",
    # Ed25519 digital signatures
    "Ed25519PrivateKey",
    "Ed25519PublicKey",
    # EC key (secp256k1)
    "ECDSA_PRIVATE_KEY_SIZE",
    "ECDSA_PUBLIC_KEY_SIZE",
    "ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE",
    "ECPrivateKey",
    "ECPublicKey",
    "ECUncompressedPublicKey",
    "SCHNORR_PUBLIC_KEY_SIZE",
    "SchnorrPublicKey",
    # Signing framework
    "SchnorrSigningOptions",
    "Signature",
    "SignatureScheme",
    "SigningOptions",
    "SigningPrivateKey",
    "SigningPublicKey",
    "SshSigningOptions",
    # Post-quantum ML-DSA
    "MLDSALevel",
    "MLDSAPrivateKey",
    "MLDSAPublicKey",
    "MLDSASignature",
    # Post-quantum ML-KEM
    "MLKEMCiphertext",
    "MLKEMLevel",
    "MLKEMPrivateKey",
    "MLKEMPublicKey",
    # Encapsulation
    "EncapsulationCiphertext",
    "EncapsulationPrivateKey",
    "EncapsulationPublicKey",
    "EncapsulationScheme",
    "SealedMessage",
    # Key encryption
    "Argon2idParams",
    "EncryptedKey",
    "HashType",
    "HKDFParams",
    "KeyDerivationMethod",
    "KeyDerivationParams",
    "PBKDF2Params",
    "SALT_LEN",
    "ScryptParams",
    # SSKR
    "SSKRGroupSpec",
    "SSKRSecret",
    "SSKRShare",
    "SSKRSpec",
    "sskr_combine",
    "sskr_generate",
    "sskr_generate_using",
    # Key management
    "HKDFRng",
    "PrivateKeyBase",
    "PrivateKeys",
    "PublicKeys",
    "keypair",
    "keypair_opt",
    "keypair_opt_using",
    "keypair_using",
    # Identifier types
    "ARID",
    "URI",
    "UUID",
    "XID",
    # Tag registration
    "register_tags",
    "register_tags_in",
]
