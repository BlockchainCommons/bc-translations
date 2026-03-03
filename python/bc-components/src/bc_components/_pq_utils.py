"""Post-quantum utility functions.

Provides SHA-256 counter-mode expansion for simulated PQ crypto key
generation, and secure random byte generation.
"""

from __future__ import annotations

import struct

from bc_crypto import sha256
from bc_rand import SecureRandomNumberGenerator


def expand_bytes(seed: bytes, label: str, length: int) -> bytes:
    """Expand *seed* into *length* bytes using SHA-256 in counter mode.

    Concatenates ``sha256(seed || label_bytes || counter_be32)`` blocks until
    *length* bytes have been produced, then truncates.
    """
    label_bytes = label.encode("utf-8")
    chunks: list[bytes] = []
    total = 0
    counter = 0
    while total < length:
        counter_bytes = struct.pack(">I", counter)
        chunk = sha256(seed + label_bytes + counter_bytes)
        chunks.append(chunk)
        total += len(chunk)
        counter += 1
    return b"".join(chunks)[:length]


def random_bytes(length: int) -> bytes:
    """Return *length* cryptographically secure random bytes."""
    rng = SecureRandomNumberGenerator()
    return rng.random_data(length)
