"""Signer and Verifier protocols for digital signatures."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from ._signature import Signature
    from ._signing_private_key import SigningOptions


@runtime_checkable
class Signer(Protocol):
    """Protocol for types capable of creating digital signatures."""

    def sign_with_options(
        self,
        message: bytes | bytearray,
        options: SigningOptions | None = None,
    ) -> Signature:
        """Sign a message with optional algorithm-specific parameters."""
        ...

    def sign(self, message: bytes | bytearray) -> Signature:
        """Sign a message using default options."""
        ...


@runtime_checkable
class Verifier(Protocol):
    """Protocol for types capable of verifying digital signatures."""

    def verify(
        self,
        signature: Signature,
        message: bytes | bytearray,
    ) -> bool:
        """Verify a signature against a message.

        Returns True if the signature is valid, False otherwise.
        """
        ...
