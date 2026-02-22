"""Tests for UR codable protocol."""

from dcbor import CBOR, Tag

from bc_ur.codable import from_ur, from_ur_string, to_ur, to_ur_string


class Leaf:
    """A simple test type that implements UREncodable/URDecodable."""

    def __init__(self, s: str) -> None:
        self.s = s

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return [Tag(24, "leaf")]

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_value(self.s)

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> "Leaf":
        return Leaf(cbor.value)


def test_ur_codable_roundtrip():
    # Register the tag
    Tag(24, "leaf")

    leaf = Leaf("test")
    ur = to_ur(leaf)
    ur_string = ur.string()
    assert ur_string == "ur:leaf/iejyihjkjygupyltla"

    leaf2 = from_ur_string(Leaf, ur_string)
    assert leaf.s == leaf2.s
