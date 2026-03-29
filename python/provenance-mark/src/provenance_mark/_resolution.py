"""Resolution helpers for provenance-mark."""

from __future__ import annotations

from enum import IntEnum

from dcbor import CBOR, Date

from ._date import (
    deserialize_2_bytes,
    deserialize_4_bytes,
    deserialize_6_bytes,
    serialize_2_bytes,
    serialize_4_bytes,
    serialize_6_bytes,
)
from ._error import Error


class ProvenanceMarkResolution(IntEnum):
    """Resolution level controlling mark link widths and date precision."""

    Low = 0
    Medium = 1
    Quartile = 2
    High = 3

    def link_length(self) -> int:
        return {
            ProvenanceMarkResolution.Low: 4,
            ProvenanceMarkResolution.Medium: 8,
            ProvenanceMarkResolution.Quartile: 16,
            ProvenanceMarkResolution.High: 32,
        }[self]

    def seq_bytes_length(self) -> int:
        return 2 if self is ProvenanceMarkResolution.Low else 4

    def date_bytes_length(self) -> int:
        return {
            ProvenanceMarkResolution.Low: 2,
            ProvenanceMarkResolution.Medium: 4,
            ProvenanceMarkResolution.Quartile: 6,
            ProvenanceMarkResolution.High: 6,
        }[self]

    def fixed_length(self) -> int:
        return (
            self.link_length() * 3
            + self.seq_bytes_length()
            + self.date_bytes_length()
        )

    def key_range(self) -> range:
        return range(0, self.link_length())

    def chain_id_range(self) -> range:
        return range(0, self.link_length())

    def hash_range(self) -> range:
        start = self.link_length()
        return range(start, start + self.link_length())

    def seq_bytes_range(self) -> range:
        start = self.hash_range().stop
        return range(start, start + self.seq_bytes_length())

    def date_bytes_range(self) -> range:
        start = self.seq_bytes_range().stop
        return range(start, start + self.date_bytes_length())

    def info_range(self) -> range:
        return range(self.date_bytes_range().stop, 1 << 30)

    def serialize_date(self, date: Date) -> bytes:
        if self is ProvenanceMarkResolution.Low:
            return serialize_2_bytes(date)
        if self is ProvenanceMarkResolution.Medium:
            return serialize_4_bytes(date)
        return serialize_6_bytes(date)

    def deserialize_date(self, data: bytes | bytearray) -> Date:
        if self is ProvenanceMarkResolution.Low and len(data) == 2:
            return deserialize_2_bytes(data)
        if self is ProvenanceMarkResolution.Medium and len(data) == 4:
            return deserialize_4_bytes(data)
        if self in (
            ProvenanceMarkResolution.Quartile,
            ProvenanceMarkResolution.High,
        ) and len(data) == 6:
            return deserialize_6_bytes(data)
        raise Error(
            "ResolutionError",
            f"invalid date length: expected 2, 4, or 6 bytes, got {len(data)}",
        )

    def serialize_seq(self, seq: int) -> bytes:
        if self.seq_bytes_length() == 2:
            if seq > 0xFFFF:
                raise Error(
                    "ResolutionError",
                    (
                        f"sequence number {seq} out of range for 2-byte format "
                        "(max 65535)"
                    ),
                )
            return seq.to_bytes(2, "big")
        return seq.to_bytes(4, "big")

    def deserialize_seq(self, data: bytes | bytearray) -> int:
        expected = self.seq_bytes_length()
        if len(data) != expected:
            raise Error(
                "ResolutionError",
                (
                    "invalid sequence number length: expected 2 or 4 bytes, "
                    f"got {len(data)}"
                ),
            )
        return int.from_bytes(bytes(data), "big")

    def to_cbor(self) -> CBOR:
        return CBOR.from_int(int(self))

    def __str__(self) -> str:
        return self.name.lower()


def resolution_from_u8(value: int) -> ProvenanceMarkResolution:
    try:
        return ProvenanceMarkResolution(value)
    except ValueError as exc:
        raise Error(
            "ResolutionError",
            f"invalid provenance mark resolution value: {value}",
        ) from exc


def resolution_from_cbor(cbor: CBOR) -> ProvenanceMarkResolution:
    return resolution_from_u8(cbor.try_int())


def resolution_to_cbor(resolution: ProvenanceMarkResolution) -> CBOR:
    return resolution.to_cbor()


def link_length(resolution: ProvenanceMarkResolution) -> int:
    return resolution.link_length()


def seq_bytes_length(resolution: ProvenanceMarkResolution) -> int:
    return resolution.seq_bytes_length()


def date_bytes_length(resolution: ProvenanceMarkResolution) -> int:
    return resolution.date_bytes_length()


def fixed_length(resolution: ProvenanceMarkResolution) -> int:
    return resolution.fixed_length()


def key_range(resolution: ProvenanceMarkResolution) -> range:
    return resolution.key_range()


def chain_id_range(resolution: ProvenanceMarkResolution) -> range:
    return resolution.chain_id_range()


def hash_range(resolution: ProvenanceMarkResolution) -> range:
    return resolution.hash_range()


def seq_bytes_range(resolution: ProvenanceMarkResolution) -> range:
    return resolution.seq_bytes_range()


def date_bytes_range(resolution: ProvenanceMarkResolution) -> range:
    return resolution.date_bytes_range()


def info_range(resolution: ProvenanceMarkResolution) -> range:
    return resolution.info_range()


def serialize_date(resolution: ProvenanceMarkResolution, date: Date) -> bytes:
    return resolution.serialize_date(date)


def deserialize_date(
    resolution: ProvenanceMarkResolution,
    data: bytes | bytearray,
) -> Date:
    return resolution.deserialize_date(data)


def serialize_seq(resolution: ProvenanceMarkResolution, seq: int) -> bytes:
    return resolution.serialize_seq(seq)


def deserialize_seq(
    resolution: ProvenanceMarkResolution,
    data: bytes | bytearray,
) -> int:
    return resolution.deserialize_seq(data)

