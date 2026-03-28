"""Assertion — a predicate-object relationship.

An assertion is a statement about a subject, consisting of a predicate
(what is being asserted) and an object (the value of the assertion).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Digest
from dcbor import CBOR, Map

from ._error import InvalidAssertion

if TYPE_CHECKING:
    from ._envelope import Envelope


class Assertion:
    """A predicate-object pair with a computed digest.

    The digest is ``Digest.from_digests([predicate.digest(), object.digest()])``.
    """

    __slots__ = ("_predicate", "_object", "_digest")

    def __init__(self, predicate: Envelope, obj: Envelope) -> None:
        self._predicate = predicate
        self._object = obj
        self._digest = Digest.from_digests([predicate.digest(), obj.digest()])

    # --- Accessors --------------------------------------------------------

    @property
    def predicate(self) -> Envelope:
        """The predicate envelope."""
        return self._predicate

    @property
    def object(self) -> Envelope:
        """The object envelope."""
        return self._object

    def digest(self) -> Digest:
        """The assertion's digest (DigestProvider protocol)."""
        return self._digest

    # --- Equality (digest-based) ------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Assertion):
            return NotImplemented
        return self._digest == other._digest

    def __hash__(self) -> int:
        return hash(self._digest.data)

    # --- CBOR encoding ----------------------------------------------------

    def to_cbor(self) -> CBOR:
        """Encode as a single-element CBOR map ``{predicate: object}``."""
        m = Map()
        m.insert(self._predicate.untagged_cbor(), self._object.untagged_cbor())
        return CBOR.from_map(m)

    @staticmethod
    def from_cbor(cbor: CBOR) -> Assertion:
        """Decode from a CBOR map with exactly one entry."""
        from ._envelope import Envelope

        m = cbor.as_map()
        if m is not None:
            return Assertion.from_map(m)
        raise InvalidAssertion()

    @staticmethod
    def from_map(m: Map) -> Assertion:
        """Decode from a CBOR Map with exactly one entry."""
        from ._envelope import Envelope

        if len(m) != 1:
            raise InvalidAssertion()
        key, value = next(iter(m))
        predicate = Envelope.from_untagged_cbor(key)
        obj = Envelope.from_untagged_cbor(value)
        return Assertion(predicate, obj)

    def to_envelope(self) -> Envelope:
        """Wrap this assertion into an Envelope."""
        from ._envelope import Envelope
        from ._envelope_case import EnvelopeCase

        return Envelope._from_case(EnvelopeCase.make_assertion(self))

    def __repr__(self) -> str:
        return f"Assertion({self._predicate!r}, {self._object!r})"
