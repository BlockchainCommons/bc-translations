"""Shared test fixtures for bc-ur tests."""

from bc_ur._xoshiro256 import Xoshiro256, make_message
from bc_ur._crc32 import crc32


def make_message_ur(length: int, seed: str) -> bytes:
    """Generate a pseudo-random message and wrap it as a CBOR byte string.

    This matches the `make_message_ur` test helper in the Rust ur crate,
    which wraps the message bytes in a minicbor ByteVec CBOR encoding.
    """
    message = make_message(seed, length)
    # Encode as CBOR byte string (major type 2)
    if length <= 23:
        header = bytes([0x40 + length])
    elif length <= 0xFF:
        header = bytes([0x58, length])
    elif length <= 0xFFFF:
        header = bytes([0x59]) + length.to_bytes(2, "big")
    else:
        header = bytes([0x5A]) + length.to_bytes(4, "big")
    return header + message
