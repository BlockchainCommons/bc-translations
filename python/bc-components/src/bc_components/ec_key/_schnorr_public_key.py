"""Schnorr (x-only) elliptic curve public key."""

from __future__ import annotations

from typing import TYPE_CHECKING

import bc_crypto

from .._error import InvalidSizeError

if TYPE_CHECKING:
    from .._digest import Digest
    from .._reference import Reference, ReferenceProvider

SCHNORR_PUBLIC_KEY_SIZE: int = bc_crypto.SCHNORR_PUBLIC_KEY_SIZE


class SchnorrPublicKey:
    """A 32-byte x-only public key for BIP-340 Schnorr signatures.

    Schnorr public keys only contain the x-coordinate of the elliptic curve
    point, unlike compressed ECDSA public keys (33 bytes) that include a
    prefix byte indicating the parity of the y-coordinate.
    """

    KEY_SIZE: int = SCHNORR_PUBLIC_KEY_SIZE

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != SCHNORR_PUBLIC_KEY_SIZE:
            raise InvalidSizeError(
                "Schnorr public key", SCHNORR_PUBLIC_KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    @staticmethod
    def from_data(data: bytes | bytearray) -> SchnorrPublicKey:
        """Create a Schnorr public key from binary data, with size validation."""
        return SchnorrPublicKey(bytes(data))

    @classmethod
    def from_hex(cls, hex_str: str) -> SchnorrPublicKey:
        """Create a Schnorr public key from a hexadecimal string."""
        return cls.from_data(bytes.fromhex(hex_str))

    @property
    def data(self) -> bytes:
        """Return the key's binary data."""
        return self._data

    def hex(self) -> str:
        """Return the key as a hexadecimal string."""
        return self._data.hex()

    def schnorr_verify(
        self,
        signature: bytes,
        message: bytes | bytearray,
    ) -> bool:
        """Verify a BIP-340 Schnorr signature for a message.

        Args:
            signature: A 64-byte Schnorr signature.
            message: The message that was signed.

        Returns:
            True if the signature is valid, False otherwise.
        """
        return bc_crypto.schnorr_verify(self._data, signature, bytes(message))

    def reference(self) -> Reference:
        """Return a Reference for this key."""
        from .._digest import Digest
        from .._reference import Reference

        return Reference.from_digest(Digest.from_image(self._data))

    def ref_hex_short(self) -> str:
        """Return a short hex string of the reference."""
        return self.reference().ref_hex_short()

    def __eq__(self, other: object) -> bool:
        if isinstance(other, SchnorrPublicKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"SchnorrPublicKey({self.hex()})"

    def __str__(self) -> str:
        return f"SchnorrPublicKey({self.ref_hex_short()})"
