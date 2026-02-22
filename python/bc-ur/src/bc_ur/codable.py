"""UR codable helper functions.

In Rust, UREncodable/URDecodable/URCodable are blanket-implemented traits.
In Python, we provide standalone functions that work on any type implementing
the dcbor CBORTaggedEncodable/CBORTaggedDecodable protocols.
"""

from __future__ import annotations

from typing import Protocol, TypeVar, runtime_checkable

from dcbor import CBOR, Tag

from .ur import UR

T = TypeVar("T")


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

    @staticmethod
    def cbor_tags() -> list[Tag]:
        """Return the CBOR tags associated with this type."""
        ...

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> URDecodable:
        """Construct an instance from untagged CBOR."""
        ...


def to_ur(obj: UREncodable) -> UR:
    """Convert a CBORTaggedEncodable to a UR."""
    tag = obj.cbor_tags()[0]
    name = tag.name
    if name is None:
        raise ValueError(
            f"CBOR tag {tag.value} must have a name. Did you call register_tags()?"
        )
    return UR(name, obj.untagged_cbor())


def to_ur_string(obj: UREncodable) -> str:
    """Convert a CBORTaggedEncodable to a UR string."""
    return to_ur(obj).string()


def from_ur(cls: type[T], ur: UR) -> T:
    """Decode a type from a UR, checking the type tag."""
    tags = cls.cbor_tags()  # type: ignore
    tag = tags[0]
    name = tag.name
    if name is None:
        raise ValueError(
            f"CBOR tag {tag.value} must have a name. Did you call register_tags()?"
        )
    ur.check_type(name)
    return cls.from_untagged_cbor(ur.cbor)  # type: ignore


def from_ur_string(cls: type[T], ur_string: str) -> T:
    """Decode a type from a UR string, checking the type tag."""
    ur = UR.from_ur_string(ur_string)
    return from_ur(cls, ur)
