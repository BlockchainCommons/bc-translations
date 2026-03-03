"""Encapsulation ciphertext (union of X25519 and ML-KEM variants)."""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_tags import (
    CBOR,
    TAG_MLKEM_CIPHERTEXT,
    TAG_X25519_PUBLIC_KEY,
)

from .._error import CryptoError
from ._encapsulation_kind import EncapsulationKind
from ._encapsulation_scheme import EncapsulationScheme

if TYPE_CHECKING:
    from ..mlkem._mlkem_ciphertext import MLKEMCiphertext
    from ..x25519._x25519_public_key import X25519PublicKey


class EncapsulationCiphertext:
    """A ciphertext produced by a key encapsulation mechanism.

    For X25519, this wraps an ephemeral public key.
    For ML-KEM, this wraps an MLKEMCiphertext.
    """

    __slots__ = ("_kind", "_inner")

    def __init__(
        self,
        kind: EncapsulationKind,
        inner: X25519PublicKey | MLKEMCiphertext,
    ) -> None:
        self._kind = kind
        self._inner = inner

    # --- Construction ---

    @staticmethod
    def from_x25519(public_key: X25519PublicKey) -> EncapsulationCiphertext:
        """Create from an X25519 ephemeral public key."""
        return EncapsulationCiphertext(EncapsulationKind.X25519, public_key)

    @staticmethod
    def from_mlkem(ciphertext: MLKEMCiphertext) -> EncapsulationCiphertext:
        """Create from an ML-KEM ciphertext."""
        return EncapsulationCiphertext(EncapsulationKind.MLKEM, ciphertext)

    # --- Accessors ---

    def x25519_public_key(self) -> X25519PublicKey:
        """Return the X25519 public key if this is an X25519 ciphertext.

        Raises CryptoError if this is not an X25519 ciphertext.
        """
        if self._kind != EncapsulationKind.X25519:
            raise CryptoError("Invalid key encapsulation type")
        from ..x25519._x25519_public_key import X25519PublicKey

        return self._inner  # type: ignore[return-value]

    def mlkem_ciphertext(self) -> MLKEMCiphertext:
        """Return the ML-KEM ciphertext if this is an ML-KEM ciphertext.

        Raises CryptoError if this is not an ML-KEM ciphertext.
        """
        if self._kind != EncapsulationKind.MLKEM:
            raise CryptoError("Invalid key encapsulation type")
        from ..mlkem._mlkem_ciphertext import MLKEMCiphertext

        return self._inner  # type: ignore[return-value]

    def is_x25519(self) -> bool:
        """Return True if this is an X25519 ciphertext."""
        return self._kind == EncapsulationKind.X25519

    def is_mlkem(self) -> bool:
        """Return True if this is an ML-KEM ciphertext."""
        return self._kind == EncapsulationKind.MLKEM

    def encapsulation_scheme(self) -> EncapsulationScheme:
        """Return the encapsulation scheme of this ciphertext."""
        if self._kind == EncapsulationKind.X25519:
            return EncapsulationScheme.X25519
        from ..mlkem._mlkem_level import MLKEMLevel

        level = self._inner.level  # type: ignore[union-attr]
        if level == MLKEMLevel.MLKEM512:
            return EncapsulationScheme.MLKEM512
        if level == MLKEMLevel.MLKEM768:
            return EncapsulationScheme.MLKEM768
        return EncapsulationScheme.MLKEM1024

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode as tagged CBOR (delegates to the inner type)."""
        return self._inner.tagged_cbor()  # type: ignore[union-attr]

    @staticmethod
    def from_cbor(cbor: CBOR) -> EncapsulationCiphertext:
        """Decode from tagged CBOR.

        Dispatches on the outer CBOR tag to determine the variant.
        """
        tag, inner = cbor.try_tagged_value()
        tag_value = tag.value if hasattr(tag, "value") else int(tag)

        if tag_value == TAG_X25519_PUBLIC_KEY:
            from ..x25519._x25519_public_key import X25519PublicKey

            pk = X25519PublicKey.from_tagged_cbor(cbor)
            return EncapsulationCiphertext.from_x25519(pk)

        if tag_value == TAG_MLKEM_CIPHERTEXT:
            from ..mlkem._mlkem_ciphertext import MLKEMCiphertext

            ct = MLKEMCiphertext.from_tagged_cbor(cbor)
            return EncapsulationCiphertext.from_mlkem(ct)

        raise ValueError("Invalid encapsulation ciphertext")

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, EncapsulationCiphertext):
            return self._kind == other._kind and self._inner == other._inner
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._kind, self._inner))

    def __repr__(self) -> str:
        return f"EncapsulationCiphertext({self._kind.name}, {self._inner!r})"

    def __str__(self) -> str:
        return f"EncapsulationCiphertext({self.encapsulation_scheme()})"
