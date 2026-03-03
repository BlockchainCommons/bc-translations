"""PrivateKeys: container for signing and encapsulation private keys."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_PRIVATE_KEYS,
)

from ._digest import Digest
from ._reference import Reference, ReferenceProvider
from .encapsulation._encapsulation_private_key import EncapsulationPrivateKey
from .signing._signing_private_key import SigningPrivateKey

if TYPE_CHECKING:
    from ._public_keys import PublicKeys
    from .signing._signature import Signature
    from .signing._signing_private_key import SigningOptions


@runtime_checkable
class PrivateKeysProvider(Protocol):
    """A type that can provide a complete set of private cryptographic keys."""

    def private_keys(self) -> PrivateKeys: ...


class PrivateKeys(ReferenceProvider):
    """A container for an entity's private cryptographic keys.

    Combines a signing key for creating digital signatures with an
    encapsulation key for decrypting messages.
    """

    __slots__ = ("_signing_private_key", "_encapsulation_private_key")

    def __init__(
        self,
        signing_private_key: SigningPrivateKey,
        encapsulation_private_key: EncapsulationPrivateKey,
    ) -> None:
        self._signing_private_key = signing_private_key
        self._encapsulation_private_key = encapsulation_private_key

    # --- Construction ---

    @staticmethod
    def with_keys(
        signing_private_key: SigningPrivateKey,
        encapsulation_private_key: EncapsulationPrivateKey,
    ) -> PrivateKeys:
        """Create PrivateKeys from signing and encapsulation keys."""
        return PrivateKeys(signing_private_key, encapsulation_private_key)

    # --- Properties ---

    @property
    def signing_private_key(self) -> SigningPrivateKey:
        return self._signing_private_key

    @property
    def encapsulation_private_key(self) -> EncapsulationPrivateKey:
        return self._encapsulation_private_key

    def public_keys(self) -> PublicKeys:
        """Derive the corresponding PublicKeys."""
        from ._public_keys import PublicKeys
        return PublicKeys(
            self._signing_private_key.public_key(),
            self._encapsulation_private_key.public_key(),
        )

    # --- PrivateKeysProvider ---

    def private_keys(self) -> PrivateKeys:
        return self

    # --- Signer ---

    def sign(self, message: bytes | bytearray) -> Signature:
        return self._signing_private_key.sign(message)

    def sign_with_options(
        self,
        message: bytes | bytearray,
        options: SigningOptions | None = None,
    ) -> Signature:
        return self._signing_private_key.sign_with_options(message, options)

    # --- Decrypter ---

    def encapsulation_private_key_for_decryption(self) -> EncapsulationPrivateKey:
        return self._encapsulation_private_key

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor().to_cbor_data())
        )

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_PRIVATE_KEYS])

    def untagged_cbor(self) -> CBOR:
        signing_key_cbor = self._signing_private_key.to_cbor()
        encapsulation_key_cbor = self._encapsulation_private_key.to_cbor()
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
    def from_untagged_cbor(cbor: CBOR) -> PrivateKeys:
        a = cbor.try_array()
        if len(a) != 2:
            raise ValueError("PrivateKeys must have two elements")
        signing_private_key = SigningPrivateKey.from_tagged_cbor(a[0])
        encapsulation_private_key = EncapsulationPrivateKey.from_tagged_cbor(a[1])
        return PrivateKeys(signing_private_key, encapsulation_private_key)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> PrivateKeys:
        tags = PrivateKeys.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return PrivateKeys.from_untagged_cbor(item)

    # --- UR ---

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @staticmethod
    def from_ur_string(ur_string: str) -> PrivateKeys:
        from bc_ur import from_ur_string
        return from_ur_string(PrivateKeys, ur_string)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, PrivateKeys):
            return (
                self._signing_private_key == other._signing_private_key
                and self._encapsulation_private_key == other._encapsulation_private_key
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._signing_private_key, self._encapsulation_private_key))

    def __repr__(self) -> str:
        return (
            f"PrivateKeys({self.reference().ref_hex_short()}, "
            f"{self._signing_private_key}, {self._encapsulation_private_key})"
        )

    def __str__(self) -> str:
        return (
            f"PrivateKeys({self.reference().ref_hex_short()}, "
            f"{self._signing_private_key}, {self._encapsulation_private_key})"
        )
