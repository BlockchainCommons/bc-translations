"""Fountain encoder for splitting messages into an unbounded stream of parts."""

from __future__ import annotations

from ._crc32 import crc32
from ._fountain_part import FountainPart
from ._fountain_utils import choose_fragments, fragment_length, partition, xor_bytes
from .error import FountainError


class FountainEncoder:
    """Encodes a message into an unbounded stream of fountain-coded parts."""

    __slots__ = ("_parts", "_message_length", "_checksum", "_current_sequence")

    def __init__(self, message: bytes, max_fragment_length: int) -> None:
        if not message:
            raise FountainError("expected non-empty message")
        if max_fragment_length <= 0:
            raise FountainError("expected positive maximum fragment length")

        frag_len = fragment_length(len(message), max_fragment_length)
        self._parts = partition(message, frag_len)
        self._message_length = len(message)
        self._checksum = crc32(message)
        self._current_sequence = 0

    @property
    def current_sequence(self) -> int:
        return self._current_sequence

    @property
    def fragment_count(self) -> int:
        return len(self._parts)

    @property
    def is_complete(self) -> bool:
        """Whether all original fragments have been emitted at least once."""
        return self._current_sequence >= len(self._parts)

    def next_part(self) -> FountainPart:
        """Emit the next fountain-encoded part."""
        self._current_sequence += 1
        indexes = choose_fragments(
            self._current_sequence, len(self._parts), self._checksum
        )

        mixed = bytearray(len(self._parts[0]))
        for idx in indexes:
            xor_bytes(mixed, self._parts[idx])

        return FountainPart(
            sequence=self._current_sequence,
            sequence_count=len(self._parts),
            message_length=self._message_length,
            checksum=self._checksum,
            data=bytes(mixed),
        )
