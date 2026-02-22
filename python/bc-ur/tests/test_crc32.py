"""Tests for CRC32/ISO-HDLC checksum computation."""

from bc_ur._crc32 import crc32


def test_hello_world():
    assert crc32(b"Hello, world!") == 0xEBE6C6E6


def test_wolf():
    assert crc32(b"Wolf") == 0x598C84DC
