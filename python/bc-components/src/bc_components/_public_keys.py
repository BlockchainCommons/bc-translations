"""PublicKeys: container for signing and encapsulation public keys."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_PUBLIC_KEYS,
)

from ._digest import Digest
from ._reference import Reference, ReferenceProvider
from .encapsulation._encapsulation_public_key import EncapsulationPublicKey
from .signing._signing_public_key import SigningPublicKey

if TYPE_CHECKING:
    from .signing._signature import Signature


@runtime_checkable
class PublicKeysProvider(Protocol):
    """A type that can provide a complete set of public cryptographic keys."""

    def public_keys(self) -> PublicKeys: ...


class PublicKeys(ReferenceProvider):
    """A container for an entity's public cryptographic keys.

    Combines a verification key for checking digital signatures with an
    encapsulation key for encrypting messages.  This type is designed to
    be freely shared across networks and systems.
    """

    __slots__ = ("_signing_public_key", "_encapsulation_public_key")

    def __init__(
        self,
        signing_public_key: SigningPublicKey,
        encapsulation_public_key: EncapsulationPublicKey,
    ) -> None:
        self._signing_public_key = signing_public_key
        self._encapsulation_public_key = encapsulation_public_key

    # --- Construction ---

    @staticmethod
    def new(
        signing_public_key: SigningPublicKey,
        encapsulation_public_key: EncapsulationPublicKey,
    ) -> PublicKeys:
        """Create PublicKeys from signing and encapsulation public keys."""
        return PublicKeys(signing_public_key, encapsulation_public_key)

    # --- Properties ---

    @property
    def signing_public_key(self) -> SigningPublicKey:
        return self._signing_public_key

    @property
    def encapsulation_public_key(self) -> EncapsulationPublicKey:
        return self._encapsulation_public_key

    # --- PublicKeysProvider ---

    def public_keys(self) -> PublicKeys:
        return self

    # --- Verifier ---

    def verify(
        self,
        signature: Signature,
        message: bytes | bytearray,
    ) -> bool:
        """Verify a signature against a message."""
        return self._signing_public_key.verify(signature, message)

    # --- Encrypter ---

    def encapsulation_public_key_for_encryption(self) -> EncapsulationPublicKey:
        return self._encapsulation_public_key

    def encapsulation_public_key(self) -> EncapsulationPublicKey:
        """Return the encapsulation public key for this key set."""
        return self._encapsulation_public_key

    def encapsulate_new_shared_secret(self):
        """Generate and encapsulate a new shared secret using the encapsulation public key."""
        return self._encapsulation_public_key.encapsulate_new_shared_secret()

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor().to_cbor_data())
        )

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_PUBLIC_KEYS])

    def untagged_cbor(self) -> CBOR:
        signing_key_cbor = self._signing_public_key.to_cbor()
        encapsulation_key_cbor = self._encapsulation_public_key.to_cbor()
        return CBOR.from_array([signing_key_cbor, encapsulation_key_cbor])

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> PublicKeys:
        a = cbor.try_array()
        if len(a) != 2:
            raise ValueError("PublicKeys must have two elements")
        signing_public_key = SigningPublicKey.from_tagged_cbor(a[0])
        encapsulation_public_key = EncapsulationPublicKey.from_tagged_cbor(a[1])
        return PublicKeys(signing_public_key, encapsulation_public_key)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> PublicKeys:
        tags = PublicKeys.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return PublicKeys.from_untagged_cbor(item)

    # --- UR ---

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @staticmethod
    def from_ur_string(ur_string: str) -> PublicKeys:
        from bc_ur import from_ur_string
        return from_ur_string(PublicKeys, ur_string)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, PublicKeys):
            return (
                self._signing_public_key == other._signing_public_key
                and self._encapsulation_public_key == other._encapsulation_public_key
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._signing_public_key, self._encapsulation_public_key))

    def __repr__(self) -> str:
        return (
            f"PublicKeys({self.reference().ref_hex_short()}, "
            f"{self._signing_public_key}, {self._encapsulation_public_key})"
        )

    def __str__(self) -> str:
        return (
            f"PublicKeys({self.reference().ref_hex_short()}, "
            f"{self._signing_public_key}, {self._encapsulation_public_key})"
        )
