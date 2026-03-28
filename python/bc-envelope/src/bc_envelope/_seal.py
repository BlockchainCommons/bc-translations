"""Seal and unseal extension for Gordian Envelope.

Combines signing and recipient encryption in a single step to create
authenticated, encrypted envelopes.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import SigningOptions
from bc_components._encrypter import Decrypter, Encrypter
from bc_components.signing import Signer, Verifier

if TYPE_CHECKING:
    from ._envelope import Envelope


def seal(
    self: Envelope,
    sender: Signer,
    recipient: Encrypter,
) -> Envelope:
    """Sign the envelope and encrypt it to *recipient*."""
    from ._recipient import encrypt_to_recipient
    from ._signature import sign

    return encrypt_to_recipient(sign(self, sender), recipient)


def seal_opt(
    self: Envelope,
    sender: Signer,
    recipient: Encrypter,
    options: SigningOptions | None = None,
) -> Envelope:
    """Sign with options and encrypt to *recipient*."""
    from ._recipient import encrypt_to_recipient
    from ._signature import sign_opt

    return encrypt_to_recipient(sign_opt(self, sender, options), recipient)


def unseal(
    self: Envelope,
    sender: Verifier,
    recipient: Decrypter,
) -> Envelope:
    """Decrypt and verify the sender's signature."""
    from ._recipient import decrypt_to_recipient
    from ._signature import verify

    return verify(decrypt_to_recipient(self, recipient), sender)
