"""CBOR encoding and decoding for Envelope.

The Gordian Envelope format uses CBOR with tag 200 (``TAG_ENVELOPE``).
Each envelope case has a unique CBOR signature enabling unambiguous
round-trip serialization.
"""

from __future__ import annotations

from bc_components import Compressed, Digest, EncryptedMessage
from bc_tags import (
    TAG_COMPRESSED,
    TAG_ENCODED_CBOR,
    TAG_ENCRYPTED,
    TAG_ENVELOPE,
    TAG_LEAF,
    tags_for_values,
)
from dcbor import CBOR, CBORCase, Map, Tag

from ._assertion import Assertion
from ._envelope import Envelope
from ._envelope_case import CaseType
from ._error import InvalidFormat
from known_values import KnownValue


# ---------------------------------------------------------------------------
# Tag constants
# ---------------------------------------------------------------------------

def envelope_tags() -> list[Tag]:
    """The CBOR tags identifying an Envelope."""
    return tags_for_values([TAG_ENVELOPE])


# ---------------------------------------------------------------------------
# Encoding
# ---------------------------------------------------------------------------

def untagged_cbor(envelope: Envelope) -> CBOR:
    """Encode *envelope* as untagged CBOR (the content inside the envelope tag)."""
    ct = envelope.case_type
    case = envelope.case

    if ct == CaseType.NODE:
        items: list[CBOR] = [untagged_cbor(case.subject)]
        for a in case.assertions:
            items.append(untagged_cbor(a))
        return CBOR.from_array(items)

    if ct == CaseType.LEAF:
        return CBOR.from_tagged_value(TAG_LEAF, case.cbor)

    if ct == CaseType.WRAPPED:
        return tagged_cbor(case.envelope)

    if ct == CaseType.ASSERTION:
        return case.assertion.to_cbor()

    if ct == CaseType.ELIDED:
        return case.digest.untagged_cbor()

    if ct == CaseType.KNOWN_VALUE:
        return case.value.untagged_cbor()

    if ct == CaseType.ENCRYPTED:
        return case.encrypted_message.tagged_cbor()

    if ct == CaseType.COMPRESSED:
        return case.compressed.tagged_cbor()

    raise AssertionError(f"unknown case type: {ct}")  # pragma: no cover


def tagged_cbor(envelope: Envelope) -> CBOR:
    """Encode *envelope* as tagged CBOR (``#6.200(...)``.)"""
    tags = envelope_tags()
    return CBOR.from_tagged_value(tags[0], untagged_cbor(envelope))


def cbor_data(envelope: Envelope) -> bytes:
    """Encode *envelope* to raw CBOR bytes."""
    return tagged_cbor(envelope).to_cbor_data()


# ---------------------------------------------------------------------------
# Decoding
# ---------------------------------------------------------------------------

def from_tagged_cbor(cbor: CBOR) -> Envelope:
    """Decode an Envelope from tagged CBOR (expects ``#6.200(...)``.)"""
    item = cbor.try_expected_tagged_value(envelope_tags()[0])
    return from_untagged_cbor(item)


def from_untagged_cbor(cbor: CBOR) -> Envelope:
    """Decode an Envelope from untagged CBOR content."""
    case = cbor.case

    # Tagged values
    if case == CBORCase.TAGGED:
        tag, item = cbor.try_tagged_value()
        tag_val = tag.value

        if tag_val in (TAG_LEAF, TAG_ENCODED_CBOR):
            return Envelope.new_leaf(item)

        if tag_val == TAG_ENVELOPE:
            inner = from_tagged_cbor(cbor)
            return Envelope.new_wrapped(inner)

        if tag_val == TAG_ENCRYPTED:
            encrypted = EncryptedMessage.from_untagged_cbor(item)
            return Envelope.new_with_encrypted(encrypted)

        if tag_val == TAG_COMPRESSED:
            compressed = Compressed.from_untagged_cbor(item)
            return Envelope.new_with_compressed(compressed)

        raise InvalidFormat(f"unknown envelope tag: {tag_val}")

    # Byte string → elided (32-byte digest)
    if case == CBORCase.BYTE_STRING:
        data = cbor.try_byte_string()
        return Envelope.new_elided(Digest.from_data(data))

    # Array → node (subject + assertions)
    if case == CBORCase.ARRAY:
        elements = cbor.try_array()
        if len(elements) < 2:
            raise InvalidFormat("node must have at least two elements")
        subject = from_untagged_cbor(elements[0])
        assertions = [from_untagged_cbor(e) for e in elements[1:]]
        return Envelope.new_with_assertions(subject, assertions)

    # Map → assertion
    if case == CBORCase.MAP:
        return Envelope.new_with_assertion(Assertion.from_cbor(cbor))

    # Unsigned integer → known value
    if case == CBORCase.UNSIGNED:
        kv = KnownValue(cbor.try_int())
        return Envelope.new_with_known_value(kv)

    raise InvalidFormat("invalid envelope")


def from_cbor_data(data: bytes | bytearray) -> Envelope:
    """Decode an Envelope from raw CBOR bytes."""
    cbor = CBOR.from_data(data)
    return from_tagged_cbor(cbor)


# ---------------------------------------------------------------------------
# Monkey-patch Envelope with CBOR methods
# ---------------------------------------------------------------------------

def _env_untagged_cbor(self: Envelope) -> CBOR:
    return untagged_cbor(self)


def _env_tagged_cbor(self: Envelope) -> CBOR:
    return tagged_cbor(self)


def _env_cbor_data(self: Envelope) -> bytes:
    return cbor_data(self)


Envelope.untagged_cbor = _env_untagged_cbor  # type: ignore[attr-defined]
Envelope.tagged_cbor = _env_tagged_cbor  # type: ignore[attr-defined]
Envelope.cbor_data = _env_cbor_data  # type: ignore[attr-defined]
Envelope.from_tagged_cbor = staticmethod(from_tagged_cbor)  # type: ignore[attr-defined]
Envelope.from_untagged_cbor = staticmethod(from_untagged_cbor)  # type: ignore[attr-defined]
Envelope.from_cbor_data = staticmethod(from_cbor_data)  # type: ignore[attr-defined]
