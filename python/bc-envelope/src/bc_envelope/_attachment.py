"""Attachment extension for Gordian Envelope.

Provides vendor-specific attachments following BCR-2023-006, with an
``Attachments`` container class and an ``Attachable`` protocol.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_components import Digest
import known_values

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ===========================================================================
# Assertion-level helpers
# ===========================================================================

def _assertion_new_attachment(
    payload: object,
    vendor: str,
    conforms_to: str | None = None,
) -> Envelope:
    """Create an attachment assertion envelope."""
    from ._envelope import Envelope as Env

    wrapped = Env(payload).wrap()
    wrapped = wrapped.add_assertion(known_values.VENDOR, vendor)
    if conforms_to is not None:
        wrapped = wrapped.add_assertion(known_values.CONFORMS_TO, conforms_to)
    return Env.new_assertion(known_values.ATTACHMENT, wrapped)


def _attachment_payload(assertion_envelope: Envelope) -> Envelope:
    """Extract the payload from an attachment assertion envelope."""
    from ._error import InvalidAttachment

    case = assertion_envelope.case
    if case.case_type == CaseType.ASSERTION:
        return case.assertion.object.try_unwrap()
    raise InvalidAttachment()


def _attachment_vendor(assertion_envelope: Envelope) -> str:
    """Extract the vendor string from an attachment assertion envelope."""
    from ._error import InvalidAttachment

    case = assertion_envelope.case
    if case.case_type == CaseType.ASSERTION:
        return case.assertion.object.extract_object_for_predicate(known_values.VENDOR, str)
    raise InvalidAttachment()


def _attachment_conforms_to(assertion_envelope: Envelope) -> str | None:
    """Extract the optional conformsTo URI from an attachment assertion envelope."""
    from ._error import InvalidAttachment

    case = assertion_envelope.case
    if case.case_type == CaseType.ASSERTION:
        return case.assertion.object.extract_optional_object_for_predicate(known_values.CONFORMS_TO, str)
    raise InvalidAttachment()


def _validate_attachment(assertion_envelope: Envelope) -> None:
    """Validate that the envelope is a proper attachment assertion."""
    from ._error import InvalidAttachment

    case = assertion_envelope.case
    if case.case_type != CaseType.ASSERTION:
        raise InvalidAttachment()

    payload = _attachment_payload(assertion_envelope)
    vendor = _attachment_vendor(assertion_envelope)
    ct = _attachment_conforms_to(assertion_envelope)
    rebuilt = _assertion_new_attachment(payload, vendor, ct)
    if not rebuilt.is_equivalent_to(assertion_envelope):
        raise InvalidAttachment()


# ===========================================================================
# Envelope-level methods
# ===========================================================================

def new_attachment(
    payload: object,
    vendor: str,
    conforms_to: str | None = None,
) -> Envelope:
    """Create a new envelope whose subject is an attachment assertion."""
    return _assertion_new_attachment(payload, vendor, conforms_to)


def add_attachment(
    self: Envelope,
    payload: object,
    vendor: str,
    conforms_to: str | None = None,
) -> Envelope:
    """Add an attachment assertion to this envelope."""
    return self.add_assertion_envelope(
        _assertion_new_attachment(payload, vendor, conforms_to),
    )


def attachment_payload(self: Envelope) -> Envelope:
    """Return the payload of this attachment envelope."""
    return _attachment_payload(self)


def attachment_vendor(self: Envelope) -> str:
    """Return the vendor string of this attachment envelope."""
    return _attachment_vendor(self)


def attachment_conforms_to(self: Envelope) -> str | None:
    """Return the optional conformsTo URI of this attachment envelope."""
    return _attachment_conforms_to(self)


def validate_attachment(self: Envelope) -> None:
    """Validate that this envelope is a proper attachment."""
    _validate_attachment(self)


def attachments(self: Envelope) -> list[Envelope]:
    """Return all attachment assertion envelopes."""
    return attachments_with_vendor_and_conforms_to(self, None, None)


def attachments_with_vendor_and_conforms_to(
    self: Envelope,
    vendor: str | None = None,
    conforms_to: str | None = None,
) -> list[Envelope]:
    """Return attachment assertions matching optional vendor/conformsTo filters."""
    from ._error import InvalidAttachment

    all_assertions = self.assertions_with_predicate(known_values.ATTACHMENT)
    for a in all_assertions:
        _validate_attachment(a)

    matching: list[Envelope] = []
    for a in all_assertions:
        if vendor is not None:
            try:
                v = _attachment_vendor(a)
            except Exception:
                continue
            if v != vendor:
                continue
        if conforms_to is not None:
            try:
                c = _attachment_conforms_to(a)
            except Exception:
                continue
            if c != conforms_to:
                continue
        matching.append(a)
    return matching


def attachment_with_vendor_and_conforms_to(
    self: Envelope,
    vendor: str | None = None,
    conforms_to: str | None = None,
) -> Envelope:
    """Return the single matching attachment, or raise."""
    from ._error import AmbiguousAttachment, NonexistentAttachment

    matches = attachments_with_vendor_and_conforms_to(self, vendor, conforms_to)
    if not matches:
        raise NonexistentAttachment()
    if len(matches) > 1:
        raise AmbiguousAttachment()
    return matches[0]


# ===========================================================================
# Attachments container
# ===========================================================================

class Attachments:
    """A container for vendor-specific metadata attachments keyed by digest."""

    __slots__ = ("_envelopes",)

    def __init__(self) -> None:
        self._envelopes: dict[Digest, Envelope] = {}

    def add(
        self,
        payload: object,
        vendor: str,
        conforms_to: str | None = None,
    ) -> None:
        attachment = new_attachment(payload, vendor, conforms_to)
        self._envelopes[attachment.digest()] = attachment

    def get(self, digest: Digest) -> Envelope | None:
        return self._envelopes.get(digest)

    def remove(self, digest: Digest) -> Envelope | None:
        return self._envelopes.pop(digest, None)

    def clear(self) -> None:
        self._envelopes.clear()

    @property
    def is_empty(self) -> bool:
        return len(self._envelopes) == 0

    def add_to_envelope(self, envelope: Envelope) -> Envelope:
        result = envelope
        for e in self._envelopes.values():
            result = result.add_assertion_envelope(e)
        return result

    @staticmethod
    def from_envelope(envelope: Envelope) -> Attachments:
        att = Attachments()
        for a in attachments(envelope):
            att._envelopes[a.digest()] = a
        return att

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Attachments):
            return NotImplemented
        return self._envelopes == other._envelopes

    def __repr__(self) -> str:
        return f"Attachments({len(self._envelopes)})"


# ===========================================================================
# Attachable protocol
# ===========================================================================

@runtime_checkable
class Attachable(Protocol):
    """Protocol for types that can have metadata attachments."""

    def get_attachments(self) -> Attachments: ...
