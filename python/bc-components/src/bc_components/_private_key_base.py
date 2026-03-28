"""PrivateKeyBase: secure foundation for deriving multiple cryptographic keys."""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_rand import (
    RandomNumberGenerator,
    SecureRandomNumberGenerator,
    rng_random_data,
)
from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_PRIVATE_KEY_BASE,
)

from ._digest import Digest
from ._private_key_data_provider import PrivateKeyDataProvider
from ._reference import Reference, ReferenceProvider
from .ec_key._ec_private_key import ECPrivateKey
from .encapsulation._encapsulation_private_key import EncapsulationPrivateKey
from .encapsulation._encapsulation_public_key import EncapsulationPublicKey
from .signing._signing_private_key import SigningPrivateKey
from .x25519._x25519_private_key import X25519PrivateKey

if TYPE_CHECKING:
    from ._private_keys import PrivateKeys
    from ._public_keys import PublicKeys
    from .signing._signature import Signature
    from .signing._signature_scheme import SignatureScheme
    from .signing._signing_private_key import SigningOptions


class PrivateKeyBase(ReferenceProvider):
    """A secure foundation for deriving multiple cryptographic keys.

    PrivateKeyBase serves as a root of cryptographic material from which
    various types of keys can be deterministically derived.  It supports:

    - Deterministic derivation of signing keys (Schnorr, ECDSA, Ed25519)
    - Deterministic derivation of encryption keys (X25519)
    - Key pair generation for both signing and encryption

    A single master seed can generate multiple secure keys for different
    cryptographic operations, similar to the concept of an HD wallet.
    """

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate() -> PrivateKeyBase:
        """Generate a new random PrivateKeyBase."""
        rng = SecureRandomNumberGenerator()
        return PrivateKeyBase.generate_using(rng)

    @staticmethod
    def from_data(data: bytes | bytearray) -> PrivateKeyBase:
        """Restore a PrivateKeyBase from bytes."""
        return PrivateKeyBase(bytes(data))

    @staticmethod
    def from_optional_data(data: bytes | bytearray | None) -> PrivateKeyBase:
        """Restore from optional data, or generate a new random key base."""
        if data is not None:
            return PrivateKeyBase.from_data(data)
        return PrivateKeyBase.generate()

    @staticmethod
    def generate_using(rng: RandomNumberGenerator) -> PrivateKeyBase:
        """Generate a new random PrivateKeyBase using the given RNG."""
        return PrivateKeyBase.from_data(rng_random_data(rng, 32))

    @staticmethod
    def new_with_provider(provider: PrivateKeyDataProvider) -> PrivateKeyBase:
        """Create a new PrivateKeyBase from a private key data provider."""
        return PrivateKeyBase.from_data(provider.private_key_data())

    # --- Accessors ---

    @property
    def data(self) -> bytes:
        """The raw key data."""
        return self._data

    # --- Key derivation ---

    def ecdsa_signing_private_key(self) -> SigningPrivateKey:
        """Derive a new ECDSA SigningPrivateKey from this key base."""
        return SigningPrivateKey.new_ecdsa(
            ECPrivateKey.derive_from_key_material(self._data)
        )

    def schnorr_signing_private_key(self) -> SigningPrivateKey:
        """Derive a new Schnorr SigningPrivateKey from this key base."""
        return SigningPrivateKey.new_schnorr(
            ECPrivateKey.derive_from_key_material(self._data)
        )

    def ed25519_signing_private_key(self) -> SigningPrivateKey:
        """Derive a new Ed25519 SigningPrivateKey from this key base."""
        from .ed25519._ed25519_private_key import Ed25519PrivateKey
        return SigningPrivateKey.new_ed25519(
            Ed25519PrivateKey.derive_from_key_material(self._data)
        )

    def ssh_signing_private_key(
        self,
        scheme: SignatureScheme,
        comment: str = "",
    ) -> SigningPrivateKey:
        """Derive a new SSH SigningPrivateKey from this key base.

        Generates an SSH key of the type indicated by the scheme.
        The comment parameter is currently unused (the cryptography library
        does not embed comments in generated keys).
        """
        from .signing._signature_scheme import _generate_ssh_private_key

        ssh_key = _generate_ssh_private_key(scheme)
        return SigningPrivateKey.new_ssh(ssh_key)

    def x25519_private_key(self) -> X25519PrivateKey:
        """Derive a new X25519PrivateKey from this key base."""
        return X25519PrivateKey.derive_from_key_material(self._data)

    # --- Key pairs ---

    def schnorr_private_keys(self) -> PrivateKeys:
        """Derive PrivateKeys with a Schnorr signing key and X25519 encryption key."""
        from ._private_keys import PrivateKeys
        return PrivateKeys.with_keys(
            self.schnorr_signing_private_key(),
            EncapsulationPrivateKey.from_x25519(self.x25519_private_key()),
        )

    def schnorr_public_keys(self) -> PublicKeys:
        """Derive PublicKeys with a Schnorr public key and X25519 public key."""
        from ._public_keys import PublicKeys
        return PublicKeys(
            self.schnorr_signing_private_key().public_key(),
            EncapsulationPublicKey.from_x25519(
                self.x25519_private_key().public_key()
            ),
        )

    def ecdsa_private_keys(self) -> PrivateKeys:
        """Derive PrivateKeys with an ECDSA signing key and X25519 encryption key."""
        from ._private_keys import PrivateKeys
        return PrivateKeys.with_keys(
            self.ecdsa_signing_private_key(),
            EncapsulationPrivateKey.from_x25519(self.x25519_private_key()),
        )

    def ecdsa_public_keys(self) -> PublicKeys:
        """Derive PublicKeys with an ECDSA public key and X25519 public key."""
        from ._public_keys import PublicKeys
        return PublicKeys(
            self.ecdsa_signing_private_key().public_key(),
            EncapsulationPublicKey.from_x25519(
                self.x25519_private_key().public_key()
            ),
        )

    # --- PrivateKeysProvider ---

    def private_keys(self) -> PrivateKeys:
        """Return the default private keys (Schnorr + X25519)."""
        return self.schnorr_private_keys()

    # --- PublicKeysProvider ---

    def public_keys(self) -> PublicKeys:
        """Return the default public keys (Schnorr + X25519)."""
        return self.schnorr_public_keys()

    # --- Signer ---

    def sign(self, message: bytes | bytearray) -> Signature:
        """Sign a message using the derived Schnorr key."""
        return self.schnorr_signing_private_key().sign(message)

    def sign_with_options(
        self,
        message: bytes | bytearray,
        options: SigningOptions | None = None,
    ) -> Signature:
        """Sign a message using the derived Schnorr key with options."""
        return self.schnorr_signing_private_key().sign_with_options(
            message, options
        )

    # --- Verifier ---

    def verify(
        self,
        signature: Signature,
        message: bytes | bytearray,
    ) -> bool:
        """Verify a signature using the derived Schnorr public key."""
        pub_key = self.schnorr_signing_private_key().public_key()
        return pub_key.verify(signature, message)

    # --- Decrypter ---

    def encapsulation_private_key(self) -> EncapsulationPrivateKey:
        """Return the X25519 encapsulation private key."""
        return EncapsulationPrivateKey.from_x25519(self.x25519_private_key())

    def decapsulate_shared_secret(self, ciphertext):
        """Decapsulate a shared secret from a ciphertext."""
        return self.encapsulation_private_key().decapsulate_shared_secret(ciphertext)

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor().to_cbor_data())
        )

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_PRIVATE_KEY_BASE])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> PrivateKeyBase:
        data = cbor.try_byte_string()
        return PrivateKeyBase.from_data(data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> PrivateKeyBase:
        tags = PrivateKeyBase.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return PrivateKeyBase.from_untagged_cbor(item)

    # --- UR ---

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @staticmethod
    def from_ur_string(ur_string: str) -> PrivateKeyBase:
        from bc_ur import from_ur_string
        return from_ur_string(PrivateKeyBase, ur_string)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, PrivateKeyBase):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return "PrivateKeyBase"

    def __bytes__(self) -> bytes:
        return self._data

    def __str__(self) -> str:
        return f"PrivateKeyBase({self.reference().ref_hex_short()})"
