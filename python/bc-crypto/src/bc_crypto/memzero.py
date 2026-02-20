"""Memory zeroing helpers.

Note: Python does not provide strong guarantees about securely erasing all
copies of immutable data. These helpers operate on mutable containers.
"""

from __future__ import annotations


def memzero(buffer) -> None:
    """Zero out a mutable buffer of bytes."""
    if isinstance(buffer, bytearray):
        buffer[:] = b"\x00" * len(buffer)
        return

    for i in range(len(buffer)):
        buffer[i] = 0


def memzero_vec_vec_u8(buffers) -> None:
    """Zero out a list of mutable byte buffers."""
    for item in buffers:
        memzero(item)


__all__ = ["memzero", "memzero_vec_vec_u8"]
