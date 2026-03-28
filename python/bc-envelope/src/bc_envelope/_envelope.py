"""Envelope — the primary Gordian Envelope data structure.

A flexible container for structured data with built-in integrity
verification, supporting selective disclosure through elision, encryption,
and compression.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Any

from bc_components import Compressed, Digest, EncryptedMessage
from dcbor import CBOR
from known_values import KnownValue

from ._assertion import Assertion
from ._envelope_case import CaseType, EnvelopeCase
from ._error import InvalidFormat, MissingDigest

if TYPE_CHECKING:
    pass


class Envelope:
    """A Gordian Envelope -- an immutable, digest-tree container.

    Envelopes are immutable.  Operations that appear to modify an envelope
    actually create a new one.  Copying is cheap because envelopes use
    Python's reference semantics -- child envelopes are shared, not duplicated.
    """

    __slots__ = ("_case",)

    def __init__(self, subject: Any = None, *, _case: EnvelopeCase | None = None) -> None:
        """Create an Envelope.

        If *_case* is provided (internal), use it directly.
        Otherwise auto-detect *subject* type and create the appropriate case.
        """
        if _case is not None:
            self._case = _case
            return
        if subject is None:
            raise TypeError("Envelope() requires a subject or _case keyword")
        # Delegate to envelope_encodable
        env = envelope_encodable(subject)
        self._case = env._case

    # --- Internal case access ---------------------------------------------

    @property
    def case(self) -> EnvelopeCase:
        """The underlying EnvelopeCase variant."""
        return self._case

    @property
    def case_type(self) -> CaseType:
        """Shorthand for ``self.case.case_type``."""
        return self._case.case_type

    # --- Internal constructors --------------------------------------------

    @staticmethod
    def _from_case(case: EnvelopeCase) -> Envelope:
        """Wrap an EnvelopeCase into an Envelope (no type dispatch)."""
        return Envelope(_case=case)

    @staticmethod
    def new_leaf(value: Any) -> Envelope:
        """Create a leaf envelope from a CBOR-encodable value."""
        if isinstance(value, CBOR):
            cbor = value
        else:
            cbor = CBOR.from_value(value)
        digest = Digest.from_image(cbor.to_cbor_data())
        return Envelope._from_case(EnvelopeCase.make_leaf(cbor, digest))

    @staticmethod
    def new_wrapped(envelope: Envelope) -> Envelope:
        """Create a wrapped envelope whose digest derives from the inner envelope."""
        digest = Digest.from_digests([envelope.digest()])
        return Envelope._from_case(EnvelopeCase.make_wrapped(envelope, digest))

    @staticmethod
    def new_elided(digest: Digest) -> Envelope:
        """Create an elided envelope containing only a digest."""
        return Envelope._from_case(EnvelopeCase.make_elided(digest))

    @staticmethod
    def new_with_assertion(assertion: Assertion) -> Envelope:
        """Create an assertion envelope."""
        return Envelope._from_case(EnvelopeCase.make_assertion(assertion))

    @staticmethod
    def new_assertion(predicate: Any, obj: Any) -> Envelope:
        """Create an assertion envelope from a predicate and object."""
        pred_env = envelope_encodable(predicate)
        obj_env = envelope_encodable(obj)
        return Envelope.new_with_assertion(Assertion(pred_env, obj_env))

    @staticmethod
    def new_with_known_value(value: KnownValue) -> Envelope:
        """Create a known-value envelope."""
        digest = value.digest()
        return Envelope._from_case(EnvelopeCase.make_known_value(value, digest))

    @staticmethod
    def new_with_encrypted(encrypted_message: EncryptedMessage) -> Envelope:
        """Create an encrypted envelope. Raises if the message has no digest."""
        if not encrypted_message.has_digest:
            raise MissingDigest()
        return Envelope._from_case(EnvelopeCase.make_encrypted(encrypted_message))

    @staticmethod
    def new_with_compressed(compressed: Compressed) -> Envelope:
        """Create a compressed envelope. Raises if the compressed data has no digest."""
        if not compressed.has_digest:
            raise MissingDigest()
        return Envelope._from_case(EnvelopeCase.make_compressed(compressed))

    @staticmethod
    def new_with_unchecked_assertions(
        subject: Envelope,
        unchecked_assertions: list[Envelope],
    ) -> Envelope:
        """Create a node envelope with subject and assertions (sorted by digest)."""
        assert unchecked_assertions, "assertions must not be empty"
        sorted_assertions = sorted(unchecked_assertions, key=lambda a: a.digest().data)
        digests = [subject.digest()]
        digests.extend(a.digest() for a in sorted_assertions)
        digest = Digest.from_digests(digests)
        return Envelope._from_case(
            EnvelopeCase.make_node(subject, sorted_assertions, digest)
        )

    @staticmethod
    def new_with_assertions(
        subject: Envelope,
        assertions: list[Envelope],
    ) -> Envelope:
        """Create a node envelope, validating all assertions are assertions or obscured."""
        for a in assertions:
            if not (a.is_subject_assertion() or a.is_subject_obscured()):
                raise InvalidFormat()
        return Envelope.new_with_unchecked_assertions(subject, assertions)

    # --- Convenience constructors -----------------------------------------

    @staticmethod
    def null() -> Envelope:
        """Create an envelope containing CBOR null."""
        return Envelope.new_leaf(CBOR.null())

    @staticmethod
    def true_value() -> Envelope:
        """Create an envelope containing CBOR true."""
        return Envelope.new_leaf(True)

    @staticmethod
    def false_value() -> Envelope:
        """Create an envelope containing CBOR false."""
        return Envelope.new_leaf(False)

    @staticmethod
    def unit() -> Envelope:
        """Create a unit envelope (known value '')."""
        import known_values
        return Envelope(known_values.UNIT)

    @staticmethod
    def new_or_null(subject: Any | None) -> Envelope:
        """Create an envelope from *subject*, or null if *subject* is ``None``."""
        if subject is None:
            return Envelope.null()
        return Envelope(subject)

    @staticmethod
    def new_or_none(subject: Any | None) -> Envelope | None:
        """Create an envelope from *subject*, or ``None`` if *subject* is ``None``."""
        if subject is None:
            return None
        return Envelope(subject)

    # --- Subject / assertions access --------------------------------------

    def subject(self) -> Envelope:
        """The envelope's subject.

        For a node envelope, returns the subject child.
        For all other cases, returns ``self``.
        """
        if self._case.case_type == CaseType.NODE:
            return self._case.subject
        return self

    def assertions(self) -> list[Envelope]:
        """The envelope's assertions (empty list if not a node)."""
        if self._case.case_type == CaseType.NODE:
            return list(self._case.assertions)
        return []

    def has_assertions(self) -> bool:
        """``True`` if the envelope has at least one assertion."""
        if self._case.case_type == CaseType.NODE:
            return len(self._case.assertions) > 0
        return False

    # --- DigestProvider protocol ------------------------------------------

    def digest(self) -> Digest:
        """The envelope's digest."""
        ct = self._case.case_type
        if ct == CaseType.NODE:
            return self._case.digest
        if ct == CaseType.LEAF:
            return self._case.digest
        if ct == CaseType.WRAPPED:
            return self._case.digest
        if ct == CaseType.ASSERTION:
            return self._case.assertion.digest()
        if ct == CaseType.ELIDED:
            return self._case.digest
        if ct == CaseType.KNOWN_VALUE:
            return self._case.digest
        if ct == CaseType.ENCRYPTED:
            return self._case.encrypted_message.digest()
        if ct == CaseType.COMPRESSED:
            return self._case.compressed.digest()
        raise AssertionError(f"unknown case type: {ct}")  # pragma: no cover

    # --- Type queries -----------------------------------------------------

    def is_node(self) -> bool:
        return self._case.case_type == CaseType.NODE

    def is_leaf(self) -> bool:
        return self._case.case_type == CaseType.LEAF

    def is_wrapped(self) -> bool:
        return self._case.case_type == CaseType.WRAPPED

    def is_assertion(self) -> bool:
        return self._case.case_type == CaseType.ASSERTION

    def is_elided(self) -> bool:
        return self._case.case_type == CaseType.ELIDED

    def is_known_value(self) -> bool:
        return self._case.case_type == CaseType.KNOWN_VALUE

    def is_encrypted(self) -> bool:
        return self._case.case_type == CaseType.ENCRYPTED

    def is_compressed(self) -> bool:
        return self._case.case_type == CaseType.COMPRESSED

    # --- Subject-level type queries ---------------------------------------

    def is_subject_assertion(self) -> bool:
        """``True`` if the envelope's subject is an assertion."""
        ct = self._case.case_type
        if ct == CaseType.ASSERTION:
            return True
        if ct == CaseType.NODE:
            return self._case.subject.is_subject_assertion()
        return False

    def is_subject_encrypted(self) -> bool:
        ct = self._case.case_type
        if ct == CaseType.ENCRYPTED:
            return True
        if ct == CaseType.NODE:
            return self._case.subject.is_subject_encrypted()
        return False

    def is_subject_compressed(self) -> bool:
        ct = self._case.case_type
        if ct == CaseType.COMPRESSED:
            return True
        if ct == CaseType.NODE:
            return self._case.subject.is_subject_compressed()
        return False

    def is_subject_elided(self) -> bool:
        ct = self._case.case_type
        if ct == CaseType.ELIDED:
            return True
        if ct == CaseType.NODE:
            return self._case.subject.is_subject_elided()
        return False

    def is_subject_obscured(self) -> bool:
        """``True`` if the subject is elided, encrypted, or compressed."""
        return (
            self.is_subject_elided()
            or self.is_subject_encrypted()
            or self.is_subject_compressed()
        )

    def is_internal(self) -> bool:
        """``True`` if the envelope has child elements (node, wrapped, or assertion)."""
        return self._case.case_type in (CaseType.NODE, CaseType.WRAPPED, CaseType.ASSERTION)

    def is_obscured(self) -> bool:
        """``True`` if the envelope is elided, encrypted, or compressed."""
        return (
            self.is_elided()
            or self.is_encrypted()
            or self.is_compressed()
        )

    # --- Leaf accessors ---------------------------------------------------

    def as_leaf(self) -> CBOR | None:
        """The leaf CBOR, or ``None`` if not a leaf."""
        if self._case.case_type == CaseType.LEAF:
            return self._case.cbor
        return None

    def try_leaf(self) -> CBOR:
        """The leaf CBOR, raising ``NotLeaf`` if not a leaf."""
        from ._error import NotLeaf
        cbor = self.as_leaf()
        if cbor is None:
            raise NotLeaf()
        return cbor

    # --- Assertion accessors ----------------------------------------------

    def as_assertion(self) -> Envelope | None:
        if self._case.case_type == CaseType.ASSERTION:
            return self
        return None

    def try_assertion(self) -> Envelope:
        from ._error import NotAssertion
        result = self.as_assertion()
        if result is None:
            raise NotAssertion()
        return result

    def as_predicate(self) -> Envelope | None:
        subj = self.subject()
        if subj._case.case_type == CaseType.ASSERTION:
            return subj._case.assertion.predicate
        return None

    def try_predicate(self) -> Envelope:
        from ._error import NotAssertion
        result = self.as_predicate()
        if result is None:
            raise NotAssertion()
        return result

    def as_object(self) -> Envelope | None:
        subj = self.subject()
        if subj._case.case_type == CaseType.ASSERTION:
            return subj._case.assertion.object
        return None

    def try_object(self) -> Envelope:
        from ._error import NotAssertion
        result = self.as_object()
        if result is None:
            raise NotAssertion()
        return result

    # --- Equality ---------------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Envelope):
            return NotImplemented
        return self.is_identical_to(other)

    def __hash__(self) -> int:
        return hash(self.digest().data)

    def __repr__(self) -> str:
        return f"Envelope({self._case!r})"


# ---------------------------------------------------------------------------
# envelope_encodable — free function that converts any supported type to Envelope
# ---------------------------------------------------------------------------


def envelope_encodable(value: Any) -> Envelope:
    """Convert any supported type into an Envelope.

    Supports: Envelope, Assertion, KnownValue, EncryptedMessage, Compressed,
    CBOR, str, int, float, bool, bytes/bytearray, and bc_components types
    that implement ``tagged_cbor()`` / ``to_cbor()``.
    """
    # Already an envelope
    if isinstance(value, Envelope):
        return value

    # Assertion
    if isinstance(value, Assertion):
        return Envelope.new_with_assertion(value)

    # KnownValue
    if isinstance(value, KnownValue):
        return Envelope.new_with_known_value(value)

    # EncryptedMessage (try — may raise if no digest)
    if isinstance(value, EncryptedMessage):
        return Envelope.new_with_encrypted(value)

    # Compressed (try — may raise if no digest)
    if isinstance(value, Compressed):
        return Envelope.new_with_compressed(value)

    # CBOR (raw)
    if isinstance(value, CBOR):
        return Envelope.new_leaf(value)

    # Python primitives → CBOR leaf
    if isinstance(value, bool):
        return Envelope.new_leaf(value)
    if isinstance(value, int):
        return Envelope.new_leaf(value)
    if isinstance(value, float):
        return Envelope.new_leaf(value)
    if isinstance(value, str):
        return Envelope.new_leaf(value)
    if isinstance(value, (bytes, bytearray)):
        return Envelope.new_leaf(value)

    # bc_components / dcbor types with tagged_cbor() → leaf with tagged CBOR
    if hasattr(value, "tagged_cbor") and callable(value.tagged_cbor):
        return Envelope.new_leaf(value.tagged_cbor())

    # dcbor types with to_tagged_cbor() → leaf with tagged CBOR
    if hasattr(value, "to_tagged_cbor") and callable(value.to_tagged_cbor):
        return Envelope.new_leaf(value.to_tagged_cbor())

    # dcbor types with to_cbor() → leaf
    if hasattr(value, "to_cbor") and callable(value.to_cbor):
        return Envelope.new_leaf(value.to_cbor())

    raise TypeError(f"Cannot convert {type(value).__name__} to Envelope")
