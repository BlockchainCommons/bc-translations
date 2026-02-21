from __future__ import annotations

from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from .tag import Tag


class DCBORError(Exception):
    """Base exception for all dCBOR encoding/decoding errors."""
    pass


class Underrun(DCBORError):
    """CBOR data ended prematurely before a complete item could be decoded."""

    def __init__(self) -> None:
        super().__init__("early end of CBOR data")


class UnsupportedHeaderValue(DCBORError):
    """An unsupported or invalid value was encountered in a CBOR header byte."""

    def __init__(self, value: int) -> None:
        self.value = value
        super().__init__("unsupported value in CBOR header")


class NonCanonicalNumeric(DCBORError):
    """A CBOR numeric value was encoded in non-canonical form."""

    def __init__(self) -> None:
        super().__init__("a CBOR numeric value was encoded in non-canonical form")


class InvalidSimpleValue(DCBORError):
    """An invalid CBOR simple value was encountered."""

    def __init__(self) -> None:
        super().__init__("an invalid CBOR simple value was encountered")


class InvalidString(DCBORError):
    """A CBOR text string was not valid UTF-8."""

    def __init__(self, detail: str) -> None:
        self.detail = detail
        super().__init__(
            f"an invalidly-encoded UTF-8 string was encountered in the CBOR ({detail!r})"
        )


class NonCanonicalString(DCBORError):
    """A CBOR text string was not in Unicode NFC form."""

    def __init__(self) -> None:
        super().__init__(
            "a CBOR string was not encoded in Unicode Canonical Normalization Form C"
        )


class UnusedData(DCBORError):
    """The decoded CBOR item didn't consume all input data."""

    def __init__(self, count: int) -> None:
        self.count = count
        super().__init__(f"the decoded CBOR had {count} extra bytes at the end")


class MisorderedMapKey(DCBORError):
    """Map keys are not in canonical lexicographic order of their encoding."""

    def __init__(self) -> None:
        super().__init__(
            "the decoded CBOR map has keys that are not in canonical order"
        )


class DuplicateMapKey(DCBORError):
    """A CBOR map contains duplicate keys."""

    def __init__(self) -> None:
        super().__init__("the decoded CBOR map has a duplicate key")


class MissingMapKey(DCBORError):
    """A requested key was not found in a CBOR map."""

    def __init__(self) -> None:
        super().__init__("missing CBOR map key")


class OutOfRange(DCBORError):
    """A numeric value could not be represented in the target type."""

    def __init__(self) -> None:
        super().__init__(
            "the CBOR numeric value could not be represented in the specified numeric type"
        )


class WrongType(DCBORError):
    """The CBOR value is not of the expected type."""

    def __init__(self) -> None:
        super().__init__("the decoded CBOR value was not the expected type")


class WrongTag(DCBORError):
    """The CBOR tagged value had a different tag than expected."""

    def __init__(self, expected: Tag | int, actual: Tag | int) -> None:
        self.expected = expected
        self.actual = actual
        super().__init__(f"expected CBOR tag {expected}, but got {actual}")


class InvalidDate(DCBORError):
    """Invalid ISO 8601 date format."""

    def __init__(self, detail: str) -> None:
        self.detail = detail
        super().__init__(f"invalid ISO 8601 date string: {detail}")
