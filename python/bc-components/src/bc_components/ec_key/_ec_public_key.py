"""Compressed ECDSA public key (33 bytes) on secp256k1."""

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
    from ._ec_uncompressed_public_key import ECUncompressedPublicKey

ECDSA_PUBLIC_KEY_SIZE: int = bc_crypto.ECDSA_PUBLIC_KEY_SIZE


class ECPublicKey:
    """A 33-byte compressed public key on the secp256k1 curve.

    The first byte is a prefix (0x02 or 0x03) that indicates the parity of
    the y-coordinate, followed by the 32-byte x-coordinate.

    Used to verify ECDSA signatures and identify key owners.
    """

    KEY_SIZE: int = ECDSA_PUBLIC_KEY_SIZE

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != ECDSA_PUBLIC_KEY_SIZE:
            raise InvalidSizeError(
                "ECDSA public key", ECDSA_PUBLIC_KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    @staticmethod
    def from_data(data: bytes | bytearray) -> ECPublicKey:
        """Create from binary data, with size validation."""
        return ECPublicKey(bytes(data))

    @classmethod
    def from_hex(cls, hex_str: str) -> ECPublicKey:
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
        """Return self (already a compressed public key)."""
        return self

    def verify(
        self,
        signature: bytes,
        message: bytes | bytearray,
    ) -> bool:
        """Verify an ECDSA signature for a message.

        Args:
            signature: A 64-byte compact ECDSA signature.
            message: The message that was signed.

        Returns:
            True if the signature is valid, False otherwise.
        """
        return bc_crypto.ecdsa_verify(self._data, signature, bytes(message))

    def uncompressed_public_key(self) -> ECUncompressedPublicKey:
        """Convert to uncompressed (65-byte) form."""
        from ._ec_uncompressed_public_key import ECUncompressedPublicKey

        return ECUncompressedPublicKey.from_data(
            bc_crypto.ecdsa_decompress_public_key(self._data)
        )

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_EC_KEY, TAG_EC_KEY_V1])

    def untagged_cbor(self) -> CBOR:
        """CBOR map with key 3 = byte string of key data (no key 2 = not private)."""
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
    def from_untagged_cbor(cbor: CBOR) -> ECPublicKey:
        m = cbor.try_map()
        key_data = m.extract(3).try_byte_string()
        return ECPublicKey.from_data(key_data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> ECPublicKey:
        tags = ECPublicKey.cbor_tags()
        inner = cbor
        for tag in tags:
            inner = inner.try_expected_tagged_value(tag.value)
        return ECPublicKey.from_untagged_cbor(inner)

    @staticmethod
    def from_tagged_cbor_data(data: bytes) -> ECPublicKey:
        return ECPublicKey.from_tagged_cbor(CBOR.from_data(data))

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
        if isinstance(other, ECPublicKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"ECPublicKey({self.hex()})"

    def __str__(self) -> str:
        return f"ECPublicKey({self.ref_hex_short()})"
