"""CRC32/ISO-HDLC checksum computation.

Python's binascii.crc32 implements CRC-32/ISO-HDLC (polynomial 0xEDB88320,
init 0xFFFFFFFF, final XOR 0xFFFFFFFF), which matches the Rust `crc` crate's
CRC_32_ISO_HDLC algorithm used by the `ur` crate.
"""

import binascii


def crc32(data: bytes) -> int:
    """Compute CRC32/ISO-HDLC checksum of data."""
    return binascii.crc32(data) & 0xFFFFFFFF
