"""SSKR (Sharded Secret Key Reconstruction) share wrapper and helper functions."""

from __future__ import annotations

from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_SSKR_SHARE,
    TAG_SSKR_SHARE_V1,
)
from sskr import (
    GroupSpec as SSKRGroupSpec,
    Secret as SSKRSecret,
    Spec as SSKRSpec,
    sskr_combine as _raw_sskr_combine,
    sskr_generate_using as _raw_sskr_generate_using,
)


class SSKRShare:
    """A share of a secret split using SSKR.

    Each SSKRShare contains a 5-byte metadata header followed by the
    share value.  The metadata encodes group thresholds, member
    thresholds, and the position of this share within the overall
    structure.
    """

    __slots__ = ("_data",)

    def __init__(self, data: bytes | bytearray) -> None:
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_data(data: bytes | bytearray) -> SSKRShare:
        """Create a new SSKRShare from raw binary data."""
        return SSKRShare(data)

    @staticmethod
    def from_hex(hex_str: str) -> SSKRShare:
        """Create a new SSKRShare from a hex string."""
        return SSKRShare(bytes.fromhex(hex_str))

    # --- Properties ---

    @property
    def data(self) -> bytes:
        """The raw binary data of this share."""
        return self._data

    def hex(self) -> str:
        """Return the data as a hexadecimal string."""
        return self._data.hex()

    def identifier(self) -> int:
        """Return the 16-bit split identifier."""
        return (self._data[0] << 8) | self._data[1]

    def identifier_hex(self) -> str:
        """Return the split identifier as a hex string."""
        return self._data[:2].hex()

    def group_threshold(self) -> int:
        """Return the minimum number of groups required."""
        return (self._data[2] >> 4) + 1

    def group_count(self) -> int:
        """Return the total number of groups in the split."""
        return (self._data[2] & 0x0F) + 1

    def group_index(self) -> int:
        """Return the zero-based group index of this share."""
        return self._data[3] >> 4

    def member_threshold(self) -> int:
        """Return the minimum number of shares required within this group."""
        return (self._data[3] & 0x0F) + 1

    def member_index(self) -> int:
        """Return the zero-based member index within the group."""
        return self._data[4] & 0x0F

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SSKR_SHARE, TAG_SSKR_SHARE_V1])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

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
    def from_untagged_cbor(cbor: CBOR) -> SSKRShare:
        data = cbor.try_byte_string()
        return SSKRShare.from_data(data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> SSKRShare:
        tags = SSKRShare.cbor_tags()
        # Accept any of the registered tags
        tag, item = cbor.try_tagged_value()
        matched = False
        for t in tags:
            if tag.value == t.value:
                matched = True
                break
        if not matched:
            raise ValueError(f"Expected SSKR share tag, got {tag.value}")
        return SSKRShare.from_untagged_cbor(item)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, SSKRShare):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"SSKRShare({self.hex()})"

    def __str__(self) -> str:
        return f"SSKRShare({self.hex()})"


# ---------------------------------------------------------------------------
# Wrapper functions
# ---------------------------------------------------------------------------


def sskr_generate(
    spec: SSKRSpec,
    master_secret: SSKRSecret,
) -> list[list[SSKRShare]]:
    """Generate SSKR shares using a secure random number generator."""
    rng = SecureRandomNumberGenerator()
    return sskr_generate_using(spec, master_secret, rng)


def sskr_generate_using(
    spec: SSKRSpec,
    master_secret: SSKRSecret,
    rng: RandomNumberGenerator,
) -> list[list[SSKRShare]]:
    """Generate SSKR shares using a custom random number generator."""
    raw_shares = _raw_sskr_generate_using(spec, master_secret, rng)
    return [
        [SSKRShare.from_data(share) for share in group]
        for group in raw_shares
    ]


def sskr_combine(shares: list[SSKRShare]) -> SSKRSecret:
    """Combine SSKR shares to reconstruct the original secret."""
    raw_shares = [share.data for share in shares]
    return _raw_sskr_combine(raw_shares)
