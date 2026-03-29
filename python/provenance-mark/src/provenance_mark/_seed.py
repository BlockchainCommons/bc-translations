"""Provenance seed wrapper."""

from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator, rng_random_data
from dcbor import CBOR

from ._crypto_utils import extend_key
from ._error import Error

PROVENANCE_SEED_LENGTH = 32


@dataclass(frozen=True, slots=True)
class ProvenanceSeed:
    """A 32-byte seed used to derive provenance marks."""

    _data: bytes

    def __post_init__(self) -> None:
        if len(self._data) != PROVENANCE_SEED_LENGTH:
            raise Error(
                "InvalidSeedLength",
                (
                    "invalid seed length: expected 32 bytes, "
                    f"got {len(self._data)} bytes"
                ),
            )

    @staticmethod
    def new() -> ProvenanceSeed:
        return ProvenanceSeed.new_using(SecureRandomNumberGenerator())

    @staticmethod
    def new_using(rng: RandomNumberGenerator) -> ProvenanceSeed:
        return ProvenanceSeed.from_bytes(rng_random_data(rng, PROVENANCE_SEED_LENGTH))

    @staticmethod
    def new_with_passphrase(passphrase: str) -> ProvenanceSeed:
        return ProvenanceSeed.from_bytes(extend_key(passphrase.encode("utf-8")))

    @staticmethod
    def from_bytes(data: bytes | bytearray) -> ProvenanceSeed:
        return ProvenanceSeed(bytes(data))

    @staticmethod
    def from_slice(data: bytes | bytearray) -> ProvenanceSeed:
        return ProvenanceSeed.from_bytes(data)

    def to_bytes(self) -> bytes:
        return bytes(self._data)

    def hex(self) -> str:
        return self._data.hex()

    def to_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def to_cbor_data(self) -> bytes:
        return self.to_cbor().to_cbor_data()

    @staticmethod
    def from_cbor(cbor: CBOR) -> ProvenanceSeed:
        return ProvenanceSeed.from_bytes(cbor.try_byte_string())

    def to_json(self) -> str:
        return _b64encode(self._data)

    @staticmethod
    def from_json(value: str | dict[str, Any]) -> ProvenanceSeed:
        if isinstance(value, dict):
            raise TypeError("ProvenanceSeed JSON representation is a string")
        return ProvenanceSeed.from_bytes(_b64decode(value))


def _b64encode(data: bytes) -> str:
    import base64

    return base64.b64encode(data).decode("ascii")


def _b64decode(value: str) -> bytes:
    import base64

    try:
        return base64.b64decode(value, validate=True)
    except Exception as exc:  # pragma: no cover - stdlib detail
        raise Error("Base64", f"base64 decoding error: {exc}") from exc

