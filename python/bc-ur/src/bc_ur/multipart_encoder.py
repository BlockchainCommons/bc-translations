"""Multipart UR encoder using fountain codes."""

from __future__ import annotations

from ._fountain_encoder import FountainEncoder
from .bytewords import BytewordsStyle, encode as bw_encode
from .ur import UR


class MultipartEncoder:
    """Encodes a UR as a stream of multipart UR strings using fountain codes."""

    __slots__ = ("_encoder", "_ur_type")

    def __init__(self, ur: UR, max_fragment_len: int) -> None:
        data = ur.cbor.to_cbor_data()
        self._encoder = FountainEncoder(data, max_fragment_len)
        self._ur_type = ur.ur_type_str

    def next_part(self) -> str:
        """Emit the next multipart UR string."""
        part = self._encoder.next_part()
        body = bw_encode(part.to_cbor(), BytewordsStyle.MINIMAL)
        return f"ur:{self._ur_type}/{part.sequence_id()}/{body}"

    @property
    def current_index(self) -> int:
        """The current sequence number (1-based, incremented after each part)."""
        return self._encoder.current_sequence

    @property
    def parts_count(self) -> int:
        """The number of original fragments (minimum parts for full coverage)."""
        return self._encoder.fragment_count
