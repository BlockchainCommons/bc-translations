"""PRNG state wrapper."""

from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from dcbor import CBOR

RNG_STATE_LENGTH = 32


@dataclass(frozen=True, slots=True)
class RngState:
    """A 32-byte serialized Xoshiro256** state."""

    _data: bytes

    def __post_init__(self) -> None:
        if len(self._data) != RNG_STATE_LENGTH:
            raise ValueError(
                (
                    "invalid RNG state length: expected 32 bytes, "
                    f"got {len(self._data)} bytes"
                )
            )

    @staticmethod
    def from_bytes(data: bytes | bytearray) -> RngState:
        return RngState(bytes(data))

    @staticmethod
    def from_slice(data: bytes | bytearray) -> RngState:
        return RngState.from_bytes(data)

    def to_bytes(self) -> bytes:
        return bytes(self._data)

    def hex(self) -> str:
        return self._data.hex()

    def to_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def to_cbor_data(self) -> bytes:
        return self.to_cbor().to_cbor_data()

    @staticmethod
    def from_cbor(cbor: CBOR) -> RngState:
        return RngState.from_bytes(cbor.try_byte_string())

    def to_json(self) -> str:
        import base64

        return base64.b64encode(self._data).decode("ascii")

    @staticmethod
    def from_json(value: str | dict[str, Any]) -> RngState:
        import base64

        if isinstance(value, dict):
            raise TypeError("RngState JSON representation is a string")
        return RngState.from_bytes(base64.b64decode(value, validate=True))

