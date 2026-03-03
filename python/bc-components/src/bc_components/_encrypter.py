"""Encrypter and Decrypter protocols for key encapsulation."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from .encapsulation._encapsulation_ciphertext import EncapsulationCiphertext
    from .encapsulation._encapsulation_private_key import EncapsulationPrivateKey
    from .encapsulation._encapsulation_public_key import EncapsulationPublicKey
    from .symmetric._symmetric_key import SymmetricKey


@runtime_checkable
class Encrypter(Protocol):
    """A type that can encapsulate shared secrets for public-key encryption.

    Implementors provide access to an encapsulation public key and the
    ability to generate and encapsulate new shared secrets.
    """

    def encapsulation_public_key(self) -> EncapsulationPublicKey:
        """Return the encapsulation public key for this encrypter."""
        ...

    def encapsulate_new_shared_secret(
        self,
    ) -> tuple[SymmetricKey, EncapsulationCiphertext]:
        """Generate and encapsulate a new shared secret.

        Returns a tuple of (shared_secret, ciphertext).
        """
        ...


@runtime_checkable
class Decrypter(Protocol):
    """A type that can decapsulate shared secrets for public-key decryption.

    Implementors provide access to an encapsulation private key and the
    ability to recover shared secrets from ciphertexts.
    """

    def encapsulation_private_key(self) -> EncapsulationPrivateKey:
        """Return the encapsulation private key for this decrypter."""
        ...

    def decapsulate_shared_secret(
        self,
        ciphertext: EncapsulationCiphertext,
    ) -> SymmetricKey:
        """Decapsulate a shared secret from a ciphertext.

        Raises BCComponentsError on failure.
        """
        ...
