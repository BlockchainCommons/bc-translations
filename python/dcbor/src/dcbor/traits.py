"""Protocol definitions for CBOR encoding/decoding.

Defines structural typing protocols for types that can be
encoded to, decoded from, or tagged with CBOR values.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from .cbor import CBOR
    from .tag import Tag


@runtime_checkable
class CBOREncodable(Protocol):
    """A type that can be encoded to CBOR."""

    def to_cbor(self) -> CBOR: ...

    def to_cbor_data(self) -> bytes: ...


@runtime_checkable
class CBORDecodable(Protocol):
    """A type that can be decoded from CBOR."""

    @staticmethod
    def from_cbor(cbor: CBOR) -> CBORDecodable: ...


@runtime_checkable
class CBORCodable(CBOREncodable, CBORDecodable, Protocol):
    """Marker protocol for types that are both encodable and decodable."""


@runtime_checkable
class CBORTagged(Protocol):
    """A type associated with one or more CBOR tags."""

    @staticmethod
    def cbor_tags() -> list[Tag]: ...


@runtime_checkable
class CBORTaggedEncodable(CBORTagged, Protocol):
    """A type that can be encoded as a tagged CBOR value."""

    def untagged_cbor(self) -> CBOR: ...

    def tagged_cbor(self) -> CBOR: ...

    def tagged_cbor_data(self) -> bytes: ...


@runtime_checkable
class CBORTaggedDecodable(CBORTagged, Protocol):
    """A type that can be decoded from a tagged CBOR value."""

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> CBORTaggedDecodable: ...

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> CBORTaggedDecodable: ...

    @staticmethod
    def from_tagged_cbor_data(data: bytes) -> CBORTaggedDecodable: ...

    @staticmethod
    def from_untagged_cbor_data(data: bytes) -> CBORTaggedDecodable: ...


@runtime_checkable
class CBORTaggedCodable(CBORTaggedEncodable, CBORTaggedDecodable, Protocol):
    """Marker protocol for types that are both tagged-encodable and tagged-decodable."""
