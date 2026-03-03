"""Sealed message: public-key encryption using key encapsulation."""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_SEALED_MESSAGE,
)

from .._nonce import Nonce
from ..symmetric._encrypted_message import EncryptedMessage
from ._encapsulation_ciphertext import EncapsulationCiphertext
from ._encapsulation_scheme import EncapsulationScheme

if TYPE_CHECKING:
    from .._encrypter import Decrypter, Encrypter


class SealedMessage:
    """A sealed message that can only be decrypted by the intended recipient.

    Internally contains:
    - An ``EncryptedMessage`` with the encrypted data
    - An ``EncapsulationCiphertext`` with the encapsulated shared secret

    The sender's identity is not revealed (anonymous sender).
    Each message uses a different ephemeral key (forward secrecy).
    """

    __slots__ = ("_message", "_encapsulated_key")

    def __init__(
        self,
        message: EncryptedMessage,
        encapsulated_key: EncapsulationCiphertext,
    ) -> None:
        self._message = message
        self._encapsulated_key = encapsulated_key

    # --- Construction ---

    @staticmethod
    def new(
        plaintext: bytes | bytearray,
        recipient: Encrypter,
    ) -> SealedMessage:
        """Create a new sealed message for the given recipient.

        Generates a shared secret, encrypts the plaintext, and
        encapsulates the key for the recipient.
        """
        return SealedMessage.new_with_aad(plaintext, recipient)

    @staticmethod
    def new_with_aad(
        plaintext: bytes | bytearray,
        recipient: Encrypter,
        aad: bytes | bytearray | None = None,
    ) -> SealedMessage:
        """Create a new sealed message with optional additional authenticated data."""
        return SealedMessage.new_opt(plaintext, recipient, aad)

    @staticmethod
    def new_opt(
        plaintext: bytes | bytearray,
        recipient: Encrypter,
        aad: bytes | bytearray | None = None,
        test_nonce: Nonce | None = None,
    ) -> SealedMessage:
        """Create a new sealed message with full control over parameters.

        Args:
            plaintext: The message data to encrypt.
            recipient: Encrypter (provides encapsulation public key).
            aad: Optional additional authenticated data.
            test_nonce: Optional nonce for deterministic testing.
        """
        shared_key, encapsulated_key = recipient.encapsulate_new_shared_secret()
        message = shared_key.encrypt(bytes(plaintext), aad, test_nonce)
        return SealedMessage(message, encapsulated_key)

    # --- Decryption ---

    def decrypt(self, private_key: Decrypter) -> bytes:
        """Decrypt this sealed message using the recipient's private key.

        Decapsulates the shared secret and uses it to decrypt the message.

        Raises an error if decryption fails (wrong key, tampered data, etc.).
        """
        shared_key = private_key.decapsulate_shared_secret(
            self._encapsulated_key
        )
        return shared_key.decrypt(self._message)

    # --- Accessors ---

    @property
    def message(self) -> EncryptedMessage:
        """The encrypted message content."""
        return self._message

    @property
    def encapsulated_key(self) -> EncapsulationCiphertext:
        """The encapsulated key used to encrypt the message."""
        return self._encapsulated_key

    def encapsulation_scheme(self) -> EncapsulationScheme:
        """Return the encapsulation scheme used for this sealed message."""
        return self._encapsulated_key.encapsulation_scheme()

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SEALED_MESSAGE])

    def untagged_cbor(self) -> CBOR:
        message_cbor = self._message.tagged_cbor()
        key_cbor = self._encapsulated_key.to_cbor()
        return CBOR.from_array([message_cbor, key_cbor])

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> SealedMessage:
        elements = cbor.try_array()
        if len(elements) != 2:
            raise ValueError("SealedMessage must have two elements")
        message = EncryptedMessage.from_tagged_cbor(elements[0])
        encapsulated_key = EncapsulationCiphertext.from_cbor(elements[1])
        return SealedMessage(message, encapsulated_key)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> SealedMessage:
        tags = SealedMessage.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return SealedMessage.from_untagged_cbor(item)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, SealedMessage):
            return (
                self._message == other._message
                and self._encapsulated_key == other._encapsulated_key
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._message, self._encapsulated_key))

    def __repr__(self) -> str:
        return f"SealedMessage({self._message!r}, {self._encapsulated_key!r})"

    def __str__(self) -> str:
        return f"SealedMessage({self.encapsulation_scheme()})"
