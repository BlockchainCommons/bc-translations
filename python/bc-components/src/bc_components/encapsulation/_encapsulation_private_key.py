"""Encapsulation private key (union of X25519 and ML-KEM variants)."""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_tags import (
    CBOR,
    TAG_MLKEM_PRIVATE_KEY,
    TAG_X25519_PRIVATE_KEY,
)

from .._digest import Digest
from .._error import CryptoError
from .._reference import Reference, ReferenceProvider
from ..symmetric._symmetric_key import SymmetricKey
from ._encapsulation_ciphertext import EncapsulationCiphertext
from ._encapsulation_kind import EncapsulationKind
from ._encapsulation_scheme import EncapsulationScheme

if TYPE_CHECKING:
    from ..mlkem._mlkem_private_key import MLKEMPrivateKey
    from ..x25519._x25519_private_key import X25519PrivateKey
    from ._encapsulation_public_key import EncapsulationPublicKey


class EncapsulationPrivateKey(ReferenceProvider):
    """A private key used for key encapsulation mechanisms (KEM).

    Wraps either an X25519PrivateKey or an MLKEMPrivateKey.
    Implements the Decrypter protocol.
    """

    __slots__ = ("_kind", "_key")

    def __init__(
        self,
        kind: EncapsulationKind,
        key: X25519PrivateKey | MLKEMPrivateKey,
    ) -> None:
        self._kind = kind
        self._key = key

    # --- Construction ---

    @staticmethod
    def from_x25519(private_key: X25519PrivateKey) -> EncapsulationPrivateKey:
        """Create from an X25519 private key."""
        return EncapsulationPrivateKey(EncapsulationKind.X25519, private_key)

    @staticmethod
    def from_mlkem(private_key: MLKEMPrivateKey) -> EncapsulationPrivateKey:
        """Create from an ML-KEM private key."""
        return EncapsulationPrivateKey(EncapsulationKind.MLKEM, private_key)

    # --- Accessors ---

    def encapsulation_scheme(self) -> EncapsulationScheme:
        """Return the encapsulation scheme for this private key."""
        if self._kind == EncapsulationKind.X25519:
            return EncapsulationScheme.X25519
        from ..mlkem._mlkem_level import MLKEMLevel

        level = self._key.level  # type: ignore[union-attr]
        if level == MLKEMLevel.MLKEM512:
            return EncapsulationScheme.MLKEM512
        if level == MLKEMLevel.MLKEM768:
            return EncapsulationScheme.MLKEM768
        return EncapsulationScheme.MLKEM1024

    def decapsulate_shared_secret(
        self,
        ciphertext: EncapsulationCiphertext,
    ) -> SymmetricKey:
        """Decapsulate a shared secret from a ciphertext.

        Raises CryptoError if the ciphertext type does not match.
        """
        if self._kind == EncapsulationKind.X25519 and ciphertext.is_x25519():
            from ..x25519._x25519_private_key import X25519PrivateKey

            priv: X25519PrivateKey = self._key  # type: ignore[assignment]
            return priv.shared_key_with(ciphertext.x25519_public_key())

        if self._kind == EncapsulationKind.MLKEM and ciphertext.is_mlkem():
            from ..mlkem._mlkem_private_key import MLKEMPrivateKey

            mlkem_priv: MLKEMPrivateKey = self._key  # type: ignore[assignment]
            return mlkem_priv.decapsulate_shared_secret(
                ciphertext.mlkem_ciphertext()
            )

        raise CryptoError(
            f"Mismatched key encapsulation types. "
            f"private key: {self.encapsulation_scheme()}, "
            f"ciphertext: {ciphertext.encapsulation_scheme()}"
        )

    def public_key(self) -> EncapsulationPublicKey:
        """Derive the corresponding public key.

        Only supported for X25519. Raises CryptoError for ML-KEM.
        """
        from ._encapsulation_public_key import EncapsulationPublicKey

        if self._kind == EncapsulationKind.X25519:
            from ..x25519._x25519_private_key import X25519PrivateKey

            priv: X25519PrivateKey = self._key  # type: ignore[assignment]
            return EncapsulationPublicKey.from_x25519(priv.public_key())

        raise CryptoError("Deriving ML-KEM public key not supported")

    # --- Decrypter protocol ---

    def encapsulation_private_key(self) -> EncapsulationPrivateKey:
        """Return self (implements Decrypter protocol)."""
        return self

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(
            Digest.from_image(self.to_cbor().to_cbor_data())
        )

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode as tagged CBOR (delegates to the inner type)."""
        return self._key.tagged_cbor()  # type: ignore[union-attr]

    @staticmethod
    def from_cbor(cbor: CBOR) -> EncapsulationPrivateKey:
        """Decode from tagged CBOR.

        Dispatches on the outer CBOR tag to determine the variant.
        """
        tag, inner = cbor.try_tagged_value()
        tag_value = tag.value if hasattr(tag, "value") else int(tag)

        if tag_value == TAG_X25519_PRIVATE_KEY:
            from ..x25519._x25519_private_key import X25519PrivateKey

            pk = X25519PrivateKey.from_tagged_cbor(cbor)
            return EncapsulationPrivateKey.from_x25519(pk)

        if tag_value == TAG_MLKEM_PRIVATE_KEY:
            from ..mlkem._mlkem_private_key import MLKEMPrivateKey

            pk = MLKEMPrivateKey.from_tagged_cbor(cbor)
            return EncapsulationPrivateKey.from_mlkem(pk)

        raise ValueError("Invalid encapsulation private key")

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, EncapsulationPrivateKey):
            return self._kind == other._kind and self._key == other._key
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._kind, self._key))

    def __repr__(self) -> str:
        return f"EncapsulationPrivateKey({self._kind.name}, {self._key!r})"

    def __str__(self) -> str:
        return (
            f"EncapsulationPrivateKey("
            f"{self.reference().ref_hex_short()}, "
            f"{self.encapsulation_scheme()})"
        )
