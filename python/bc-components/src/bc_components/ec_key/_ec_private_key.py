"""EC private key (32 bytes) on secp256k1."""

from __future__ import annotations

from typing import TYPE_CHECKING

import bc_crypto
from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator

from .._error import InvalidSizeError
from bc_tags import (
    CBOR,
    Map,
    Tag,
    TAG_EC_KEY,
    TAG_EC_KEY_V1,
    tags_for_values,
)

if TYPE_CHECKING:
    from .._digest import Digest
    from .._reference import Reference

from ._ec_public_key import ECPublicKey
from ._schnorr_public_key import SchnorrPublicKey

ECDSA_PRIVATE_KEY_SIZE: int = bc_crypto.ECDSA_PRIVATE_KEY_SIZE


class ECPrivateKey:
    """A 32-byte private key for secp256k1 elliptic curve algorithms.

    Can be used to:
    - Generate its corresponding public key
    - Sign messages using the ECDSA signature scheme
    - Sign messages using the Schnorr signature scheme (BIP-340)
    """

    KEY_SIZE: int = ECDSA_PRIVATE_KEY_SIZE

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != ECDSA_PRIVATE_KEY_SIZE:
            raise InvalidSizeError(
                "EC private key", ECDSA_PRIVATE_KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    @staticmethod
    def generate() -> ECPrivateKey:
        """Create a new random EC private key using a secure RNG."""
        rng = SecureRandomNumberGenerator()
        return ECPrivateKey.generate_using(rng)

    @staticmethod
    def generate_using(rng: RandomNumberGenerator) -> ECPrivateKey:
        """Create a new random EC private key using the given RNG."""
        key_bytes = rng.random_data(ECDSA_PRIVATE_KEY_SIZE)
        return ECPrivateKey.from_data(bytes(key_bytes))

    @staticmethod
    def from_data(data: bytes | bytearray) -> ECPrivateKey:
        """Create from binary data, with size validation."""
        return ECPrivateKey(bytes(data))

    @classmethod
    def from_hex(cls, hex_str: str) -> ECPrivateKey:
        """Create from a hexadecimal string."""
        return cls.from_data(bytes.fromhex(hex_str))

    @staticmethod
    def derive_from_key_material(key_material: bytes | bytearray) -> ECPrivateKey:
        """Derive a private key from the given key material.

        Uses HKDF to deterministically generate a valid private key
        for the secp256k1 curve.
        """
        return ECPrivateKey.from_data(
            bc_crypto.derive_signing_private_key(bytes(key_material))
        )

    @property
    def data(self) -> bytes:
        """Return the key's binary data."""
        return self._data

    def hex(self) -> str:
        """Return the key as a hexadecimal string."""
        return self._data.hex()

    def public_key(self) -> ECPublicKey:
        """Derive the corresponding ECDSA compressed public key."""
        return ECPublicKey.from_data(
            bc_crypto.ecdsa_public_key_from_private_key(self._data)
        )

    def schnorr_public_key(self) -> SchnorrPublicKey:
        """Derive the Schnorr (x-only) public key from this private key."""
        return SchnorrPublicKey.from_data(
            bc_crypto.schnorr_public_key_from_private_key(self._data)
        )

    def ecdsa_sign(self, message: bytes | bytearray) -> bytes:
        """Sign a message using ECDSA. Returns a 64-byte compact signature."""
        return bc_crypto.ecdsa_sign(self._data, bytes(message))

    def schnorr_sign_using(
        self,
        message: bytes | bytearray,
        rng: RandomNumberGenerator,
    ) -> bytes:
        """Sign using Schnorr with a custom RNG. Returns a 64-byte signature."""
        return bc_crypto.schnorr_sign_using(self._data, bytes(message), rng)

    def schnorr_sign(self, message: bytes | bytearray) -> bytes:
        """Sign using Schnorr with a secure RNG. Returns a 64-byte signature."""
        rng = SecureRandomNumberGenerator()
        return self.schnorr_sign_using(message, rng)

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_EC_KEY, TAG_EC_KEY_V1])

    def untagged_cbor(self) -> CBOR:
        """CBOR map with key 2 = true (private) and key 3 = byte string of key data."""
        m = Map()
        m.insert(2, True)
        m.insert(3, CBOR.from_bytes(self._data))
        return CBOR.from_map(m)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        cbor = self.untagged_cbor()
        for tag in reversed(tags):
            cbor = CBOR.from_tagged_value(tag, cbor)
        return cbor

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> ECPrivateKey:
        m = cbor.try_map()
        key_data = m.extract(3).try_byte_string()
        return ECPrivateKey.from_data(key_data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> ECPrivateKey:
        tags = ECPrivateKey.cbor_tags()
        inner = cbor
        for tag in tags:
            inner = inner.try_expected_tagged_value(tag.value)
        return ECPrivateKey.from_untagged_cbor(inner)

    @staticmethod
    def from_tagged_cbor_data(data: bytes) -> ECPrivateKey:
        return ECPrivateKey.from_tagged_cbor(CBOR.from_data(data))

    # --- Reference ---

    def reference(self) -> Reference:
        from .._digest import Digest
        from .._reference import Reference

        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor_data())
        )

    def ref_hex_short(self) -> str:
        return self.reference().ref_hex_short()

    def __eq__(self, other: object) -> bool:
        if isinstance(other, ECPrivateKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"ECPrivateKey({self.hex()})"

    def __str__(self) -> str:
        return f"ECPrivateKey({self.ref_hex_short()})"
