"""Encapsulation public key (union of X25519 and ML-KEM variants)."""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_tags import (
    CBOR,
    TAG_MLKEM_PUBLIC_KEY,
    TAG_X25519_PUBLIC_KEY,
)

from .._digest import Digest
from .._reference import Reference, ReferenceProvider
from ..symmetric._symmetric_key import SymmetricKey
from ._encapsulation_ciphertext import EncapsulationCiphertext
from ._encapsulation_kind import EncapsulationKind
from ._encapsulation_scheme import EncapsulationScheme

if TYPE_CHECKING:
    from ..mlkem._mlkem_public_key import MLKEMPublicKey
    from ..x25519._x25519_public_key import X25519PublicKey


class EncapsulationPublicKey(ReferenceProvider):
    """A public key used for key encapsulation mechanisms (KEM).

    Wraps either an X25519PublicKey or an MLKEMPublicKey.
    Implements the Encrypter protocol.
    """

    __slots__ = ("_kind", "_key")

    def __init__(
        self,
        kind: EncapsulationKind,
        key: X25519PublicKey | MLKEMPublicKey,
    ) -> None:
        self._kind = kind
        self._key = key

    # --- Construction ---

    @staticmethod
    def from_x25519(public_key: X25519PublicKey) -> EncapsulationPublicKey:
        """Create from an X25519 public key."""
        return EncapsulationPublicKey(EncapsulationKind.X25519, public_key)

    @staticmethod
    def from_mlkem(public_key: MLKEMPublicKey) -> EncapsulationPublicKey:
        """Create from an ML-KEM public key."""
        return EncapsulationPublicKey(EncapsulationKind.MLKEM, public_key)

    # --- Accessors ---

    def encapsulation_scheme(self) -> EncapsulationScheme:
        """Return the encapsulation scheme for this public key."""
        if self._kind == EncapsulationKind.X25519:
            return EncapsulationScheme.X25519
        from ..mlkem._mlkem_level import MLKEMLevel

        level = self._key.level  # type: ignore[union-attr]
        if level == MLKEMLevel.MLKEM512:
            return EncapsulationScheme.MLKEM512
        if level == MLKEMLevel.MLKEM768:
            return EncapsulationScheme.MLKEM768
        return EncapsulationScheme.MLKEM1024

    def encapsulate_new_shared_secret(
        self,
    ) -> tuple[SymmetricKey, EncapsulationCiphertext]:
        """Encapsulate a new shared secret using this public key.

        For X25519: generates an ephemeral keypair, computes a DH shared
        secret, and returns the shared key plus the ephemeral public key
        as the ciphertext.

        For ML-KEM: delegates to the ML-KEM public key's encapsulation.

        Returns a tuple of (shared_secret, ciphertext).
        """
        if self._kind == EncapsulationKind.X25519:
            from ..x25519._x25519_private_key import X25519PrivateKey
            from ..x25519._x25519_public_key import X25519PublicKey

            recipient_pub: X25519PublicKey = self._key  # type: ignore[assignment]
            ephemeral_priv = X25519PrivateKey.generate()
            ephemeral_pub = ephemeral_priv.public_key()
            shared_key = ephemeral_priv.shared_key_with(recipient_pub)
            return (
                shared_key,
                EncapsulationCiphertext.from_x25519(ephemeral_pub),
            )

        from ..mlkem._mlkem_public_key import MLKEMPublicKey

        mlkem_pub: MLKEMPublicKey = self._key  # type: ignore[assignment]
        shared_key, mlkem_ct = mlkem_pub.encapsulate_new_shared_secret()
        return shared_key, EncapsulationCiphertext.from_mlkem(mlkem_ct)

    # --- Encrypter protocol ---

    def encapsulation_public_key(self) -> EncapsulationPublicKey:
        """Return self (implements Encrypter protocol)."""
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
    def from_cbor(cbor: CBOR) -> EncapsulationPublicKey:
        """Decode from tagged CBOR.

        Dispatches on the outer CBOR tag to determine the variant.
        """
        tag, inner = cbor.try_tagged_value()
        tag_value = tag.value if hasattr(tag, "value") else int(tag)

        if tag_value == TAG_X25519_PUBLIC_KEY:
            from ..x25519._x25519_public_key import X25519PublicKey

            pk = X25519PublicKey.from_tagged_cbor(cbor)
            return EncapsulationPublicKey.from_x25519(pk)

        if tag_value == TAG_MLKEM_PUBLIC_KEY:
            from ..mlkem._mlkem_public_key import MLKEMPublicKey

            pk = MLKEMPublicKey.from_tagged_cbor(cbor)
            return EncapsulationPublicKey.from_mlkem(pk)

        raise ValueError("Invalid encapsulation public key")

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, EncapsulationPublicKey):
            return self._kind == other._kind and self._key == other._key
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._kind, self._key))

    def __repr__(self) -> str:
        return f"EncapsulationPublicKey({self._kind.name}, {self._key!r})"

    def __str__(self) -> str:
        return (
            f"EncapsulationPublicKey("
            f"{self.reference().ref_hex_short()}, "
            f"{self.encapsulation_scheme()})"
        )
