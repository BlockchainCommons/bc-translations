"""Envelope case variants — the internal representation of an envelope's structure.

Each variant represents a different structural form an envelope can take,
as defined in the Gordian Envelope specification.
"""

from __future__ import annotations

from enum import Enum, auto
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from bc_components import Compressed, Digest, EncryptedMessage
    from dcbor import CBOR
    from known_values import KnownValue

    from ._assertion import Assertion
    from ._envelope import Envelope


class CaseType(Enum):
    """Discriminant identifying the envelope variant."""

    NODE = auto()
    LEAF = auto()
    WRAPPED = auto()
    ASSERTION = auto()
    ELIDED = auto()
    KNOWN_VALUE = auto()
    ENCRYPTED = auto()
    COMPRESSED = auto()


class EnvelopeCase:
    """The core structural variants of a Gordian Envelope.

    This is a discriminated-union style class.  Use the ``case_type`` property
    to determine which variant is active, then access the appropriate attributes.

    Attributes present per variant:

    * ``NODE``: ``subject`` (Envelope), ``assertions`` (list[Envelope]), ``digest`` (Digest)
    * ``LEAF``: ``cbor`` (CBOR), ``digest`` (Digest)
    * ``WRAPPED``: ``envelope`` (Envelope), ``digest`` (Digest)
    * ``ASSERTION``: ``assertion`` (Assertion)
    * ``ELIDED``: ``digest`` (Digest)
    * ``KNOWN_VALUE``: ``value`` (KnownValue), ``digest`` (Digest)
    * ``ENCRYPTED``: ``encrypted_message`` (EncryptedMessage)
    * ``COMPRESSED``: ``compressed`` (Compressed)
    """

    __slots__ = (
        "_case_type",
        "_subject",
        "_assertions",
        "_digest",
        "_cbor",
        "_envelope",
        "_assertion",
        "_value",
        "_encrypted_message",
        "_compressed",
    )

    def __init__(self, case_type: CaseType, **kwargs: object) -> None:
        self._case_type = case_type
        self._subject: Envelope | None = kwargs.get("subject")  # type: ignore[assignment]
        self._assertions: list[Envelope] | None = kwargs.get("assertions")  # type: ignore[assignment]
        self._digest: Digest | None = kwargs.get("digest")  # type: ignore[assignment]
        self._cbor: CBOR | None = kwargs.get("cbor")  # type: ignore[assignment]
        self._envelope: Envelope | None = kwargs.get("envelope")  # type: ignore[assignment]
        self._assertion: Assertion | None = kwargs.get("assertion")  # type: ignore[assignment]
        self._value: KnownValue | None = kwargs.get("value")  # type: ignore[assignment]
        self._encrypted_message: EncryptedMessage | None = kwargs.get("encrypted_message")  # type: ignore[assignment]
        self._compressed: Compressed | None = kwargs.get("compressed")  # type: ignore[assignment]

    # --- Case type --------------------------------------------------------

    @property
    def case_type(self) -> CaseType:
        return self._case_type

    # --- Variant-specific accessors ---------------------------------------

    @property
    def subject(self) -> Envelope:
        assert self._subject is not None
        return self._subject

    @property
    def assertions(self) -> list[Envelope]:
        assert self._assertions is not None
        return self._assertions

    @property
    def digest(self) -> Digest:
        assert self._digest is not None
        return self._digest

    @property
    def cbor(self) -> CBOR:
        assert self._cbor is not None
        return self._cbor

    @property
    def envelope(self) -> Envelope:
        assert self._envelope is not None
        return self._envelope

    @property
    def assertion(self) -> Assertion:
        assert self._assertion is not None
        return self._assertion

    @property
    def value(self) -> KnownValue:
        assert self._value is not None
        return self._value

    @property
    def encrypted_message(self) -> EncryptedMessage:
        assert self._encrypted_message is not None
        return self._encrypted_message

    @property
    def compressed(self) -> Compressed:
        assert self._compressed is not None
        return self._compressed

    # --- Factory methods --------------------------------------------------

    @staticmethod
    def make_node(subject: Envelope, assertions: list[Envelope], digest: Digest) -> EnvelopeCase:
        return EnvelopeCase(
            CaseType.NODE,
            subject=subject,
            assertions=assertions,
            digest=digest,
        )

    @staticmethod
    def make_leaf(cbor: CBOR, digest: Digest) -> EnvelopeCase:
        return EnvelopeCase(CaseType.LEAF, cbor=cbor, digest=digest)

    @staticmethod
    def make_wrapped(envelope: Envelope, digest: Digest) -> EnvelopeCase:
        return EnvelopeCase(CaseType.WRAPPED, envelope=envelope, digest=digest)

    @staticmethod
    def make_assertion(assertion: Assertion) -> EnvelopeCase:
        return EnvelopeCase(CaseType.ASSERTION, assertion=assertion)

    @staticmethod
    def make_elided(digest: Digest) -> EnvelopeCase:
        return EnvelopeCase(CaseType.ELIDED, digest=digest)

    @staticmethod
    def make_known_value(value: KnownValue, digest: Digest) -> EnvelopeCase:
        return EnvelopeCase(CaseType.KNOWN_VALUE, value=value, digest=digest)

    @staticmethod
    def make_encrypted(encrypted_message: EncryptedMessage) -> EnvelopeCase:
        return EnvelopeCase(CaseType.ENCRYPTED, encrypted_message=encrypted_message)

    @staticmethod
    def make_compressed(compressed: Compressed) -> EnvelopeCase:
        return EnvelopeCase(CaseType.COMPRESSED, compressed=compressed)

    # --- Equality ---------------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, EnvelopeCase):
            return NotImplemented
        return self._case_type == other._case_type and self._digest == other._digest

    def __repr__(self) -> str:
        return f"EnvelopeCase({self._case_type.name})"
