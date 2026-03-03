"""Tests for Compressed, translated from Rust bc-components/src/compressed.rs tests."""

import math

from bc_components import Compressed


def test_1():
    """Test compression of a moderately sized text blob."""
    source = (
        b"Lorem ipsum dolor sit amet consectetur adipiscing elit mi "
        b"nibh ornare proin blandit diam ridiculus, faucibus mus dui eu "
        b"vehicula nam donec dictumst sed vivamus bibendum aliquet "
        b"efficitur. Felis imperdiet sodales dictum morbi vivamus augue "
        b"dis duis aliquet velit ullamcorper porttitor, lobortis dapibus "
        b"hac purus aliquam natoque iaculis blandit montes nunc pretium."
    )
    compressed = Compressed.from_decompressed_data(source)
    # Verify the debug representation matches Rust output
    debug = repr(compressed)
    assert "checksum: 3eeb10a0" in debug
    assert "size: 217/364" in debug
    assert "ratio: 0.60" in debug
    assert "digest: None" in debug
    assert compressed.decompress() == source


def test_2():
    """Test compression of a small text blob."""
    source = b"Lorem ipsum dolor sit amet consectetur adipiscing"
    compressed = Compressed.from_decompressed_data(source)
    debug = repr(compressed)
    assert "checksum: 29db1793" in debug
    assert "size: 47/49" in debug
    assert "ratio: 0.96" in debug
    assert "digest: None" in debug
    assert compressed.decompress() == source


def test_3():
    """Test compression of a very short string (no effective compression)."""
    source = b"Lorem"
    compressed = Compressed.from_decompressed_data(source)
    debug = repr(compressed)
    assert "checksum: 44989b39" in debug
    assert "size: 5/5" in debug
    assert "ratio: 1.00" in debug
    assert "digest: None" in debug
    assert compressed.decompress() == source


def test_4():
    """Test compression of empty data."""
    source = b""
    compressed = Compressed.from_decompressed_data(source)
    debug = repr(compressed)
    assert "checksum: 00000000" in debug
    assert "size: 0/0" in debug
    assert math.isnan(compressed.compression_ratio)
    assert "digest: None" in debug
    assert compressed.decompress() == source
