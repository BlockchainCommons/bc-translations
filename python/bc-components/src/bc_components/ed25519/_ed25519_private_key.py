"""Ed25519 private key for digital signatures.

Ed25519 provides high-performance digital signatures with 128-bit security.
"""

from __future__ import annotations

from bc_crypto import (
    ED25519_PRIVATE_KEY_SIZE,
    ED25519_SIGNATURE_SIZE,
    derive_signing_private_key,
    ed25519_public_key_from_private_key,
    ed25519_sign,
)
from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator

from .._digest import Digest
from .._error import BCComponentsError
from .._reference import Reference
from ._ed25519_public_key import Ed25519PublicKey


class Ed25519PrivateKey:
    """A 32-byte Ed25519 private signing key."""

    KEY_SIZE: int = ED25519_PRIVATE_KEY_SIZE

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.KEY_SIZE:
            raise BCComponentsError.invalid_size(
                "Ed25519 private key", self.KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate() -> Ed25519PrivateKey:
        """Create a new random Ed25519 private key."""
        rng = SecureRandomNumberGenerator()
        return Ed25519PrivateKey.generate_using(rng)

    @staticmethod
    def generate_using(rng: RandomNumberGenerator) -> Ed25519PrivateKey:
        """Create a new random Ed25519 private key using the given RNG."""
        data = rng.random_data(Ed25519PrivateKey.KEY_SIZE)
        return Ed25519PrivateKey.from_data(bytes(data))

    @staticmethod
    def from_data(data: bytes | bytearray) -> Ed25519PrivateKey:
        """Restore from a byte sequence, validating the length."""
        return Ed25519PrivateKey(bytes(data))

    @staticmethod
    def from_hex(hex_str: str) -> Ed25519PrivateKey:
        """Restore from a 64-character hex string."""
        return Ed25519PrivateKey.from_data(bytes.fromhex(hex_str))

    @staticmethod
    def derive_from_key_material(
        key_material: bytes | bytearray | str,
    ) -> Ed25519PrivateKey:
        """Derive an Ed25519 private key from the given key material via HKDF."""
        return Ed25519PrivateKey(derive_signing_private_key(key_material))

    # --- Accessors ---

    @property
    def data(self) -> bytes:
        """The raw 32-byte private key."""
        return self._data

    def hex(self) -> str:
        """The key as a 64-character lowercase hex string."""
        return self._data.hex()

    # --- Key operations ---

    def public_key(self) -> Ed25519PublicKey:
        """Derive the corresponding Ed25519 public key."""
        pk_bytes = ed25519_public_key_from_private_key(self._data)
        return Ed25519PublicKey.from_data(pk_bytes)

    def sign(self, message: bytes | bytearray) -> bytes:
        """Sign *message* and return the 64-byte signature."""
        return ed25519_sign(self._data, bytes(message))

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(Digest.from_image(self._data))

    def ref_hex(self) -> str:
        return self.reference().ref_hex()

    def ref_hex_short(self) -> str:
        return self.reference().ref_hex_short()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, Ed25519PrivateKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"Ed25519PrivateKey({self.hex()})"

    def __str__(self) -> str:
        return f"Ed25519PrivateKey({self.ref_hex_short()})"

    def __bytes__(self) -> bytes:
        return self._data
