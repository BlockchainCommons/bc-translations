"""UR codable helper functions.

In Rust, UREncodable/URDecodable/URCodable are blanket-implemented traits.
In Python, we provide standalone functions that work on any type implementing
the dcbor CBORTaggedEncodable/CBORTaggedDecodable protocols.
"""

from __future__ import annotations

from typing import Protocol, TypeVar, runtime_checkable

from dcbor import CBOR, Tag

from .ur import UR

TDecodable = TypeVar("TDecodable", bound="URDecodable")


@runtime_checkable
class UREncodable(Protocol):
    """Protocol for types that can be encoded to a UR."""

    @staticmethod
    def cbor_tags() -> list[Tag]:
        """Return the CBOR tags associated with this type."""
        ...

    def untagged_cbor(self) -> CBOR:
        """Return the CBOR representation without the outer tag."""
        ...


@runtime_checkable
class URDecodable(Protocol):
    """Protocol for types that can be decoded from a UR."""

    @classmethod
    def cbor_tags(cls) -> list[Tag]:
        """Return the CBOR tags associated with this type."""
        ...

    @classmethod
    def from_untagged_cbor(cls: type[TDecodable], cbor: CBOR) -> TDecodable:
        """Construct an instance from untagged CBOR."""
        ...


def _first_tag_name(tags: list[Tag]) -> str:
    if not tags:
        raise ValueError("CBOR tag list must contain at least one tag")
    tag = tags[0]
    name = tag.name
    if name is None:
        raise ValueError(
            f"CBOR tag {tag.value} must have a name. Did you call register_tags()?"
        )
    return name


def to_ur(obj: UREncodable) -> UR:
    """Convert a CBORTaggedEncodable to a UR."""
    name = _first_tag_name(obj.cbor_tags())
    return UR(name, obj.untagged_cbor())


def to_ur_string(obj: UREncodable) -> str:
    """Convert a CBORTaggedEncodable to a UR string."""
    return to_ur(obj).string()


def from_ur(cls: type[TDecodable], ur: UR) -> TDecodable:
    """Decode a type from a UR, checking the type tag."""
    name = _first_tag_name(cls.cbor_tags())
    ur.check_type(name)
    return cls.from_untagged_cbor(ur.cbor)


def from_ur_string(cls: type[TDecodable], ur_string: str) -> TDecodable:
    """Decode a type from a UR string, checking the type tag."""
    ur = UR.from_ur_string(ur_string)
    return from_ur(cls, ur)
