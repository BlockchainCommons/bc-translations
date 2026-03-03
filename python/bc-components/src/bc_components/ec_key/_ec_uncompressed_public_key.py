"""Uncompressed elliptic curve public key (65 bytes)."""

from __future__ import annotations

from typing import TYPE_CHECKING

import bc_crypto

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

ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE: int = bc_crypto.ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE


class ECUncompressedPublicKey:
    """A 65-byte uncompressed public key on the secp256k1 curve.

    Consists of:
    - 1 byte prefix (0x04)
    - 32 bytes for the x-coordinate
    - 32 bytes for the y-coordinate

    This is considered a legacy key type. The compressed format (ECPublicKey)
    is more space-efficient and provides the same cryptographic security.
    """

    KEY_SIZE: int = ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE:
            raise InvalidSizeError(
                "ECDSA uncompressed public key",
                ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
                len(data),
            )
        self._data = bytes(data)

    @staticmethod
    def from_data(data: bytes | bytearray) -> ECUncompressedPublicKey:
        """Create from binary data, with size validation."""
        return ECUncompressedPublicKey(bytes(data))

    @classmethod
    def from_hex(cls, hex_str: str) -> ECUncompressedPublicKey:
        """Create from a hexadecimal string."""
        return cls.from_data(bytes.fromhex(hex_str))

    @property
    def data(self) -> bytes:
        """Return the key's binary data."""
        return self._data

    def hex(self) -> str:
        """Return the key as a hexadecimal string."""
        return self._data.hex()

    def public_key(self) -> ECPublicKey:
        """Convert this uncompressed public key to its compressed form."""
        from ._ec_public_key import ECPublicKey

        return ECPublicKey.from_data(
            bc_crypto.ecdsa_compress_public_key(self._data)
        )

    def uncompressed_public_key(self) -> ECUncompressedPublicKey:
        """Return self (already uncompressed)."""
        return self

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_EC_KEY, TAG_EC_KEY_V1])

    def untagged_cbor(self) -> CBOR:
        m = Map()
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
    def from_untagged_cbor(cbor: CBOR) -> ECUncompressedPublicKey:
        m = cbor.try_map()
        key_data = m.extract(3).try_byte_string()
        return ECUncompressedPublicKey.from_data(key_data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> ECUncompressedPublicKey:
        tags = ECUncompressedPublicKey.cbor_tags()
        inner = cbor
        for tag in tags:
            inner = inner.try_expected_tagged_value(tag.value)
        return ECUncompressedPublicKey.from_untagged_cbor(inner)

    @staticmethod
    def from_tagged_cbor_data(data: bytes) -> ECUncompressedPublicKey:
        return ECUncompressedPublicKey.from_tagged_cbor(CBOR.from_data(data))

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
        if isinstance(other, ECUncompressedPublicKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"ECUncompressedPublicKey({self.hex()})"

    def __str__(self) -> str:
        return f"ECUncompressedPublicKey({self.ref_hex_short()})"
