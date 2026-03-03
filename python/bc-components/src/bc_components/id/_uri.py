"""URI (Uniform Resource Identifier)."""

from __future__ import annotations

from urllib.parse import urlparse

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_URI,
)

from .._error import InvalidDataError


class URI:
    """A Uniform Resource Identifier (URI).

    A URI is a string of characters that unambiguously identifies a
    particular resource.  This implementation validates URIs using
    ``urllib.parse`` to ensure basic conformance.
    """

    __slots__ = ("_uri",)

    def __init__(self, uri: str) -> None:
        parsed = urlparse(uri)
        if not parsed.scheme:
            raise InvalidDataError("URI", "invalid URI format")
        self._uri = uri

    # --- Construction ---

    @staticmethod
    def new(uri: str) -> URI:
        """Create a new URI from a string, validating the format."""
        return URI(uri)

    # --- Accessors ---

    def as_str(self) -> str:
        return self._uri

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_URI])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_text(self._uri)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> URI:
        text = cbor.try_text()
        return URI.new(text)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> URI:
        tags = URI.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return URI.from_untagged_cbor(item)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, URI):
            return self._uri == other._uri
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._uri)

    def __repr__(self) -> str:
        return f"URI({self._uri!r})"

    def __str__(self) -> str:
        return self._uri
