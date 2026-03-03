"""X25519 private key for key agreement.

Implements X25519 Diffie-Hellman key exchange (RFC 7748).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_crypto import (
    derive_agreement_private_key,
    x25519_new_private_key_using,
    x25519_public_key_from_private_key,
    x25519_shared_key,
)
from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
from bc_tags import TAG_X25519_PRIVATE_KEY, tags_for_values
from dcbor import CBOR, Tag

from .._digest import Digest
from .._error import BCComponentsError
from .._reference import Reference
from ..symmetric._symmetric_key import SymmetricKey
from ._x25519_public_key import X25519PublicKey

if TYPE_CHECKING:
    from bc_ur import UR

X25519_PRIVATE_KEY_SIZE: int = 32


class X25519PrivateKey:
    """A 32-byte X25519 private key for Diffie-Hellman key agreement."""

    KEY_SIZE: int = 32

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.KEY_SIZE:
            raise BCComponentsError.invalid_size(
                "X25519 private key", self.KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate() -> X25519PrivateKey:
        """Generate a new random X25519 private key."""
        rng = SecureRandomNumberGenerator()
        return X25519PrivateKey.generate_using(rng)

    @staticmethod
    def generate_using(rng: RandomNumberGenerator) -> X25519PrivateKey:
        """Generate a new random X25519 private key using the given RNG."""
        data = x25519_new_private_key_using(rng)
        return X25519PrivateKey(data)

    @staticmethod
    def keypair() -> tuple[X25519PrivateKey, X25519PublicKey]:
        """Generate a new random keypair."""
        private_key = X25519PrivateKey.generate()
        public_key = private_key.public_key()
        return private_key, public_key

    @staticmethod
    def keypair_using(
        rng: RandomNumberGenerator,
    ) -> tuple[X25519PrivateKey, X25519PublicKey]:
        """Generate a new random keypair using the given RNG."""
        private_key = X25519PrivateKey.generate_using(rng)
        public_key = private_key.public_key()
        return private_key, public_key

    @staticmethod
    def from_data(data: bytes | bytearray) -> X25519PrivateKey:
        """Restore from a byte sequence, validating the length."""
        return X25519PrivateKey(bytes(data))

    @staticmethod
    def from_hex(hex_str: str) -> X25519PrivateKey:
        """Restore from a 64-character hex string."""
        return X25519PrivateKey.from_data(bytes.fromhex(hex_str))

    @staticmethod
    def derive_from_key_material(
        key_material: bytes | bytearray | str,
    ) -> X25519PrivateKey:
        """Derive an X25519 private key from the given key material via HKDF."""
        return X25519PrivateKey(derive_agreement_private_key(key_material))

    # --- Accessors ---

    @property
    def data(self) -> bytes:
        """The raw 32-byte private key."""
        return self._data

    def hex(self) -> str:
        """The key as a 64-character lowercase hex string."""
        return self._data.hex()

    # --- Key operations ---

    def public_key(self) -> X25519PublicKey:
        """Derive the corresponding X25519 public key."""
        pk_bytes = x25519_public_key_from_private_key(self._data)
        return X25519PublicKey.from_data(pk_bytes)

    def shared_key_with(self, public_key: X25519PublicKey) -> SymmetricKey:
        """Compute a shared symmetric key via X25519 key agreement."""
        shared = x25519_shared_key(self._data, public_key.data)
        return SymmetricKey.from_data(shared)

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_X25519_PRIVATE_KEY])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> X25519PrivateKey:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> X25519PrivateKey:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> X25519PrivateKey:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- UR ---

    def to_ur(self) -> UR:
        from bc_ur import to_ur
        return to_ur(self)

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @classmethod
    def from_ur(cls, ur: UR) -> X25519PrivateKey:
        from bc_ur import from_ur
        return from_ur(cls, ur)

    @classmethod
    def from_ur_string(cls, ur_string: str) -> X25519PrivateKey:
        from bc_ur import from_ur_string
        return from_ur_string(cls, ur_string)

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor_data())
        )

    def ref_hex(self) -> str:
        return self.reference().ref_hex()

    def ref_hex_short(self) -> str:
        return self.reference().ref_hex_short()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, X25519PrivateKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"X25519PrivateKey({self.hex()})"

    def __str__(self) -> str:
        return f"X25519PrivateKey({self.ref_hex_short()})"

    def __bytes__(self) -> bytes:
        return self._data
