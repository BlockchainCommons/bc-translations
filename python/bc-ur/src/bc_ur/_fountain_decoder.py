"""Fountain decoder for reconstructing messages from fountain-coded parts."""

from __future__ import annotations

from ._fountain_part import FountainPart
from ._fountain_utils import xor_bytes
from .error import FountainError


class FountainDecoder:
    """Decodes fountain-coded parts to reconstruct the original message."""

    __slots__ = (
        "_decoded",
        "_received",
        "_buffer",
        "_queue",
        "_sequence_count",
        "_message_length",
        "_checksum",
        "_fragment_length",
    )

    def __init__(self) -> None:
        self._decoded: dict[int, FountainPart] = {}
        self._received: set[tuple[int, ...]] = set()
        self._buffer: dict[tuple[int, ...], FountainPart] = {}
        self._queue: list[tuple[int, FountainPart]] = []
        self._sequence_count = 0
        self._message_length = 0
        self._checksum = 0
        self._fragment_length = 0

    @property
    def is_complete(self) -> bool:
        return (
            self._message_length != 0
            and len(self._decoded) == self._sequence_count
        )

    def validate(self, part: FountainPart) -> bool:
        """Check if a part is consistent with previously received parts."""
        if not self._received:
            return False
        return (
            part.sequence_count == self._sequence_count
            and part.message_length == self._message_length
            and part.checksum == self._checksum
            and len(part.data) == self._fragment_length
        )

    def receive(self, part: FountainPart) -> bool:
        """Receive a fountain part. Returns True if the part was new and useful."""
        if self.is_complete:
            return False

        if part.sequence_count == 0 or not part.data or part.message_length == 0:
            raise FountainError("expected non-empty part")

        if not self._received:
            self._sequence_count = part.sequence_count
            self._message_length = part.message_length
            self._checksum = part.checksum
            self._fragment_length = len(part.data)
        elif not self.validate(part):
            raise FountainError("part is inconsistent with previous ones")

        indexes = tuple(part.indexes())
        if indexes in self._received:
            return False
        self._received.add(indexes)

        if part.is_simple():
            self._process_simple(part)
        else:
            self._process_complex(part)
        return True

    def message(self) -> bytes | None:
        """Return the decoded message if complete, else None."""
        if not self.is_complete:
            return None

        combined = bytearray()
        for idx in range(self._sequence_count):
            part = self._decoded.get(idx)
            if part is None:
                raise FountainError("missing decoded fragment")
            combined.extend(part.data)

        # Verify padding is all zeros
        if not all(b == 0 for b in combined[self._message_length :]):
            raise FountainError("invalid padding")

        return bytes(combined[: self._message_length])

    def _process_simple(self, part: FountainPart) -> None:
        indexes = part.indexes()
        index = indexes[0]
        self._decoded[index] = part
        self._queue.append((index, part))
        self._process_queue()

    def _process_queue(self) -> None:
        while self._queue:
            index, simple = self._queue.pop()
            keys_to_process = sorted(
                key for key in self._buffer if index in key
            )
            for key in keys_to_process:
                buffered = self._buffer.pop(key)
                new_indexes = list(key)
                new_indexes.remove(index)
                new_data = bytearray(buffered.data)
                xor_bytes(new_data, simple.data)
                new_part = FountainPart(
                    buffered.sequence,
                    buffered.sequence_count,
                    buffered.message_length,
                    buffered.checksum,
                    bytes(new_data),
                )
                if len(new_indexes) == 1:
                    self._decoded[new_indexes[0]] = new_part
                    self._queue.append((new_indexes[0], new_part))
                else:
                    self._buffer[tuple(new_indexes)] = new_part

    def _process_complex(self, part: FountainPart) -> None:
        indexes = list(part.indexes())
        to_remove = [idx for idx in indexes if idx in self._decoded]

        if len(indexes) == len(to_remove):
            return

        data = bytearray(part.data)
        for idx in to_remove:
            indexes.remove(idx)
            xor_bytes(data, self._decoded[idx].data)

        new_part = FountainPart(
            part.sequence,
            part.sequence_count,
            part.message_length,
            part.checksum,
            bytes(data),
        )

        if len(indexes) == 1:
            self._decoded[indexes[0]] = new_part
            self._queue.append((indexes[0], new_part))
        else:
            self._buffer[tuple(indexes)] = new_part
