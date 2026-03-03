"""Ed25519 public key for signature verification.

Used to verify digital signatures created with the corresponding
Ed25519 private key.
"""

from __future__ import annotations

from bc_crypto import ED25519_PUBLIC_KEY_SIZE, ED25519_SIGNATURE_SIZE, ed25519_verify

from .._digest import Digest
from .._error import BCComponentsError
from .._reference import Reference


class Ed25519PublicKey:
    """A 32-byte Ed25519 public verification key."""

    KEY_SIZE: int = ED25519_PUBLIC_KEY_SIZE

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.KEY_SIZE:
            raise BCComponentsError.invalid_size(
                "Ed25519 public key", self.KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_data(data: bytes | bytearray) -> Ed25519PublicKey:
        """Restore from a byte sequence, validating the length."""
        return Ed25519PublicKey(bytes(data))

    @staticmethod
    def from_hex(hex_str: str) -> Ed25519PublicKey:
        """Restore from a 64-character hex string."""
        return Ed25519PublicKey.from_data(bytes.fromhex(hex_str))

    # --- Accessors ---

    @property
    def data(self) -> bytes:
        """The raw 32-byte public key."""
        return self._data

    def hex(self) -> str:
        """The key as a 64-character lowercase hex string."""
        return self._data.hex()

    # --- Verification ---

    def verify(self, signature: bytes, message: bytes | bytearray) -> bool:
        """Verify an Ed25519 *signature* against *message*.

        Returns ``True`` if the signature is valid, ``False`` otherwise.
        """
        return ed25519_verify(self._data, bytes(message), signature)

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(Digest.from_image(self._data))

    def ref_hex(self) -> str:
        return self.reference().ref_hex()

    def ref_hex_short(self) -> str:
        return self.reference().ref_hex_short()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, Ed25519PublicKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"Ed25519PublicKey({self.hex()})"

    def __str__(self) -> str:
        return f"Ed25519PublicKey({self.ref_hex_short()})"

    def __bytes__(self) -> bytes:
        return self._data
