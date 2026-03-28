"""Envelope encoding and decoding utilities.

``envelope_encodable(value)`` converts any supported type to an Envelope.
``extract_subject(envelope, type_class)`` extracts a typed value from an
envelope's subject.
"""

from __future__ import annotations

from typing import Any, TypeVar

from bc_components import (
    ARID,
    URI,
    UUID,
    XID,
    Compressed,
    Digest,
    EncryptedMessage,
    JSON,
    Nonce,
    PrivateKeyBase,
    PrivateKeys,
    PublicKeys,
    Reference,
    Salt,
    SealedMessage,
    Signature,
    SSKRShare,
)
from bc_components.encrypted_key import EncryptedKey
from dcbor import CBOR, ByteString, Date, Map, Set
from known_values import KnownValue

from ._assertion import Assertion
from ._envelope_case import CaseType

T = TypeVar("T")

# Re-export the main function from _envelope to keep a single implementation
from ._envelope import envelope_encodable  # noqa: F401


def extract_subject(envelope: Any, type_class: type[T]) -> T:
    """Extract the envelope's subject as the given type.

    Works by examining the envelope case and either extracting directly
    (for special types like Assertion, Digest, KnownValue, etc.) or
    converting via the leaf CBOR.

    Raises ``EnvelopeError`` subclasses on type mismatch.
    """
    from ._envelope import Envelope
    from ._error import InvalidFormat

    if not isinstance(envelope, Envelope):
        raise TypeError(f"Expected Envelope, got {type(envelope).__name__}")

    case = envelope.case
    ct = case.case_type

    # Node → recurse into subject
    if ct == CaseType.NODE:
        return extract_subject(case.subject, type_class)

    # Wrapped → extract Envelope
    if ct == CaseType.WRAPPED:
        if type_class is Envelope:
            return case.envelope  # type: ignore[return-value]
        raise InvalidFormat(f"expected Envelope, got wrapped envelope")

    # Assertion
    if ct == CaseType.ASSERTION:
        if type_class is Assertion:
            return case.assertion  # type: ignore[return-value]
        raise InvalidFormat(f"expected {type_class.__name__}, got assertion")

    # Elided → Digest
    if ct == CaseType.ELIDED:
        if type_class is Digest:
            return case.digest  # type: ignore[return-value]
        raise InvalidFormat(f"expected {type_class.__name__}, got elided")

    # KnownValue
    if ct == CaseType.KNOWN_VALUE:
        if type_class is KnownValue:
            return case.value  # type: ignore[return-value]
        raise InvalidFormat(f"expected {type_class.__name__}, got known value")

    # Encrypted
    if ct == CaseType.ENCRYPTED:
        if type_class is EncryptedMessage:
            return case.encrypted_message  # type: ignore[return-value]
        raise InvalidFormat(f"expected {type_class.__name__}, got encrypted")

    # Compressed
    if ct == CaseType.COMPRESSED:
        if type_class is Compressed:
            return case.compressed  # type: ignore[return-value]
        raise InvalidFormat(f"expected {type_class.__name__}, got compressed")

    # Leaf → extract from CBOR
    if ct == CaseType.LEAF:
        return _extract_from_cbor(case.cbor, type_class)

    raise InvalidFormat(f"unexpected case type: {ct}")  # pragma: no cover


def _auto_extract_subject(envelope: Any) -> Any:
    """Auto-detect the subject type and extract it.

    Used by internal library code that calls ``envelope.extract_subject()``
    without specifying a type.
    """
    from ._envelope import Envelope

    case = envelope.case
    ct = case.case_type

    if ct == CaseType.NODE:
        return _auto_extract_subject(case.subject)
    if ct == CaseType.WRAPPED:
        return case.envelope
    if ct == CaseType.ASSERTION:
        return case.assertion
    if ct == CaseType.ELIDED:
        return case.digest
    if ct == CaseType.KNOWN_VALUE:
        return case.value
    if ct == CaseType.ENCRYPTED:
        return case.encrypted_message
    if ct == CaseType.COMPRESSED:
        return case.compressed
    if ct == CaseType.LEAF:
        return _auto_extract_from_cbor(case.cbor)

    from ._error import InvalidFormat
    raise InvalidFormat(f"unexpected case type: {ct}")


def _auto_extract_from_cbor(cbor: CBOR) -> Any:
    """Auto-detect and extract a typed value from a CBOR leaf.

    Tries tagged types first (via CBOR tag), then falls back to primitives.
    """
    # Try tagged types
    _tagged_types: list[type] = [
        Digest, Salt, Nonce, ARID, URI, UUID, XID, Reference,
        PublicKeys, PrivateKeys, PrivateKeyBase, SealedMessage,
        Signature, SSKRShare, EncryptedMessage, Compressed, JSON,
        EncryptedKey,
    ]
    for tt in _tagged_types:
        if hasattr(tt, "from_tagged_cbor"):
            try:
                return tt.from_tagged_cbor(cbor)
            except Exception:
                continue

    # Try Date
    try:
        return Date.from_tagged_cbor(cbor)
    except Exception:
        pass

    # Primitives
    try:
        return cbor.try_text()
    except Exception:
        pass
    try:
        return cbor.try_int()
    except Exception:
        pass
    try:
        return cbor.try_float()
    except Exception:
        pass
    try:
        return cbor.try_bool()
    except Exception:
        pass
    try:
        return cbor.try_byte_string()
    except Exception:
        pass

    # Return raw CBOR as fallback
    return cbor


def _extract_from_cbor(cbor: CBOR, type_class: type[T]) -> T:
    """Extract a typed value from a CBOR leaf."""
    from ._error import InvalidFormat

    # Python primitives
    if type_class is str:
        return cbor.try_text()  # type: ignore[return-value]
    if type_class is int:
        return cbor.try_int()  # type: ignore[return-value]
    if type_class is float:
        return cbor.try_float()  # type: ignore[return-value]
    if type_class is bool:
        return cbor.try_bool()  # type: ignore[return-value]
    if type_class is bytes:
        return cbor.try_byte_string()  # type: ignore[return-value]

    # CBOR itself
    if type_class is CBOR:
        return cbor  # type: ignore[return-value]

    # dcbor types
    if type_class is ByteString:
        data = cbor.try_byte_string()
        return ByteString(data)  # type: ignore[return-value]
    if type_class is Date:
        return Date.from_tagged_cbor(cbor)  # type: ignore[return-value]

    # bc_components types with from_tagged_cbor
    _tagged_types: list[type] = [
        Digest, Salt, Nonce, ARID, URI, UUID, XID, Reference,
        PublicKeys, PrivateKeys, PrivateKeyBase, SealedMessage,
        Signature, SSKRShare, EncryptedMessage, Compressed, JSON,
        EncryptedKey,
    ]
    for tt in _tagged_types:
        if type_class is tt:
            if hasattr(tt, "from_tagged_cbor"):
                return tt.from_tagged_cbor(cbor)  # type: ignore[return-value]

    # dcbor Map / Set
    if type_class is Map:
        return cbor.try_map()  # type: ignore[return-value]
    if type_class is Set:
        from dcbor import Set as DcborSet
        # Set is stored as tagged array
        return DcborSet.from_tagged_cbor(cbor)  # type: ignore[return-value]

    raise InvalidFormat(f"cannot extract {type_class.__name__} from CBOR leaf")
