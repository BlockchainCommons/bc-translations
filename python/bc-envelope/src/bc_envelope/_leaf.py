"""Leaf-related methods for Envelope.

Provides boolean checks, type checks, and extraction helpers for
envelopes whose subjects are CBOR leaf values.
"""

from __future__ import annotations

from dcbor import CBOR, Map, Simple

import known_values
from known_values import KnownValue

from ._envelope import Envelope
from ._envelope_case import CaseType
from ._error import NotKnownValue, SubjectNotUnit


# ---------------------------------------------------------------------------
# Boolean helpers
# ---------------------------------------------------------------------------

def is_false(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is CBOR ``false``."""
    cbor = envelope.subject().as_leaf()
    return cbor is not None and cbor.is_false


def is_true(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is CBOR ``true``."""
    cbor = envelope.subject().as_leaf()
    return cbor is not None and cbor.is_true


def is_bool(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is CBOR ``true`` or ``false``."""
    cbor = envelope.subject().as_leaf()
    return cbor is not None and cbor.is_bool


# ---------------------------------------------------------------------------
# Numeric helpers
# ---------------------------------------------------------------------------

def is_number(envelope: Envelope) -> bool:
    """``True`` if the envelope is a leaf containing a number."""
    cbor = envelope.as_leaf()
    return cbor is not None and cbor.is_number


def is_subject_number(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is a number."""
    return is_number(envelope.subject())


def is_nan(envelope: Envelope) -> bool:
    """``True`` if the envelope is a leaf containing NaN."""
    cbor = envelope.as_leaf()
    return cbor is not None and cbor.is_nan


def is_subject_nan(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is NaN."""
    return is_nan(envelope.subject())


# ---------------------------------------------------------------------------
# Null
# ---------------------------------------------------------------------------

def is_null(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is CBOR ``null``."""
    cbor = envelope.subject().as_leaf()
    return cbor is not None and cbor.is_null


# ---------------------------------------------------------------------------
# Byte string / array / map / text extraction
# ---------------------------------------------------------------------------

def try_byte_string(envelope: Envelope) -> bytes:
    """The leaf as a byte string, raising on failure."""
    from ._error import NotLeaf
    cbor = envelope.try_leaf()
    return cbor.try_byte_string()


def as_byte_string(envelope: Envelope) -> bytes | None:
    """The leaf as a byte string, or ``None``."""
    cbor = envelope.as_leaf()
    if cbor is None:
        return None
    return cbor.as_byte_string()


def as_array(envelope: Envelope) -> list[CBOR] | None:
    """The leaf as a CBOR array, or ``None``."""
    cbor = envelope.as_leaf()
    if cbor is None:
        return None
    return cbor.as_array()


def as_map(envelope: Envelope) -> Map | None:
    """The leaf as a CBOR Map, or ``None``."""
    cbor = envelope.as_leaf()
    if cbor is None:
        return None
    return cbor.as_map()


def as_text(envelope: Envelope) -> str | None:
    """The leaf as a text string, or ``None``."""
    cbor = envelope.as_leaf()
    if cbor is None:
        return None
    return cbor.as_text()


# ---------------------------------------------------------------------------
# Known value helpers
# ---------------------------------------------------------------------------

def as_known_value(envelope: Envelope) -> KnownValue | None:
    """The envelope's KnownValue, or ``None`` if not a KnownValue case."""
    if envelope.case_type == CaseType.KNOWN_VALUE:
        return envelope.case.value
    return None


def try_known_value(envelope: Envelope) -> KnownValue:
    """The envelope's KnownValue, raising ``NotKnownValue`` if not."""
    kv = as_known_value(envelope)
    if kv is None:
        raise NotKnownValue()
    return kv


def is_known_value_case(envelope: Envelope) -> bool:
    """``True`` if the envelope is a KnownValue case."""
    return envelope.case_type == CaseType.KNOWN_VALUE


def is_subject_unit(envelope: Envelope) -> bool:
    """``True`` if the envelope's subject is the unit known value."""
    from ._envelope_encodable import extract_subject
    try:
        kv = extract_subject(envelope, KnownValue)
        return kv.value == known_values.UNIT.value
    except Exception:
        return False


def check_subject_unit(envelope: Envelope) -> Envelope:
    """Return *envelope* if its subject is unit, otherwise raise ``SubjectNotUnit``."""
    if not is_subject_unit(envelope):
        raise SubjectNotUnit()
    return envelope
