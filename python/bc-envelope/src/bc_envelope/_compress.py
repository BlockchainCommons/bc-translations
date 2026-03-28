"""Compression and decompression extension for Gordian Envelope.

Uses DEFLATE compression via ``bc_components.Compressed``, preserving
the envelope's digest tree so signatures and proofs remain valid.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Compressed
from dcbor import CBOR

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# compress / decompress (entire envelope)
# ---------------------------------------------------------------------------

def compress(self: Envelope) -> Envelope:
    """Return a compressed version of this envelope."""
    from ._envelope import Envelope as Env
    from ._error import AlreadyElided, AlreadyEncrypted

    ct = self.case.case_type
    if ct == CaseType.COMPRESSED:
        return self
    if ct == CaseType.ENCRYPTED:
        raise AlreadyEncrypted()
    if ct == CaseType.ELIDED:
        raise AlreadyElided()

    compressed = Compressed.from_decompressed_data(
        self.tagged_cbor().to_cbor_data(),
        self.digest(),
    )
    return Env.new_with_compressed(compressed)


def decompress(self: Envelope) -> Envelope:
    """Return the decompressed form of this envelope."""
    from ._envelope import Envelope as Env
    from ._error import InvalidDigest, MissingDigest, NotCompressed

    case = self.case
    if case.case_type != CaseType.COMPRESSED:
        raise NotCompressed()

    comp = case.compressed
    digest_opt = comp.digest_opt
    if digest_opt is None:
        raise MissingDigest()
    if digest_opt != self.digest():
        raise InvalidDigest()

    decompressed_data = comp.decompress()
    cbor = CBOR.from_data(decompressed_data)
    envelope = Env.from_tagged_cbor(cbor)
    if envelope.digest() != digest_opt:
        raise InvalidDigest()
    return envelope


# ---------------------------------------------------------------------------
# compress_subject / decompress_subject
# ---------------------------------------------------------------------------

def compress_subject(self: Envelope) -> Envelope:
    """Return this envelope with its subject compressed."""
    if self.subject().is_compressed():
        return self
    subject = compress(self.subject())
    return self.replace_subject(subject)


def decompress_subject(self: Envelope) -> Envelope:
    """Return this envelope with its subject decompressed."""
    if not self.subject().is_compressed():
        return self
    subject = decompress(self.subject())
    return self.replace_subject(subject)
