"""Tests for SSKRShare, sskr_generate, and sskr_combine.

Translated from Rust sskr_mod.rs and Kotlin SSKRShareTest.
"""

from bc_components import (
    SSKRGroupSpec,
    SSKRSecret,
    SSKRShare,
    SSKRSpec,
    register_tags,
    sskr_combine,
    sskr_generate,
)


def test_share_metadata():
    """Test SSKRShare metadata parsing from known bytes."""
    data = bytes([
        0x12, 0x34,        # identifier: 0x1234
        0x21,              # group_threshold-1=2, group_count-1=1
        0x31,              # group_index=3, member_threshold-1=1
        0x01,              # member_index=1
        0xAA, 0xBB, 0xCC,  # share value
    ])
    share = SSKRShare.from_data(data)

    assert share.identifier() == 0x1234
    assert share.identifier_hex() == "1234"
    assert share.group_threshold() == 3
    assert share.group_count() == 2
    assert share.group_index() == 3
    assert share.member_threshold() == 2
    assert share.member_index() == 1


def test_share_hex_roundtrip():
    """Test SSKRShare hex encoding roundtrip."""
    hex_str = "1234213101aabbcc"
    share = SSKRShare.from_hex(hex_str)
    assert share.hex() == hex_str


def test_generate_and_combine_simple():
    """Test single group, 1 of 1 SSKR generate and combine."""
    secret_data = b"0123456789abcdef"  # 16 bytes
    master_secret = SSKRSecret(secret_data)
    group = SSKRGroupSpec(1, 1)
    spec = SSKRSpec(1, [group])

    shares = sskr_generate(spec, master_secret)
    assert len(shares) == 1
    assert len(shares[0]) == 1

    recovered = sskr_combine(shares[0])
    assert recovered.data == secret_data


def test_generate_and_combine_2of3():
    """Test 2-of-3 SSKR generate and combine."""
    secret_data = b"0123456789abcdef"  # 16 bytes
    master_secret = SSKRSecret(secret_data)
    group = SSKRGroupSpec(2, 3)
    spec = SSKRSpec(1, [group])

    shares = sskr_generate(spec, master_secret)
    assert len(shares) == 1
    assert len(shares[0]) == 3

    # Use first 2 shares to recover
    recovery_shares = [shares[0][0], shares[0][1]]
    recovered = sskr_combine(recovery_shares)
    assert recovered.data == secret_data


def test_generate_and_combine_multi_group():
    """Test multi-group SSKR: 2-of-2 groups, group1=2of3, group2=3of5."""
    secret_data = b"0123456789abcdef"  # 16 bytes
    master_secret = SSKRSecret(secret_data)
    group1 = SSKRGroupSpec(2, 3)
    group2 = SSKRGroupSpec(3, 5)
    spec = SSKRSpec(2, [group1, group2])

    shares = sskr_generate(spec, master_secret)
    assert len(shares) == 2
    assert len(shares[0]) == 3
    assert len(shares[1]) == 5

    # Collect shares meeting threshold: 2 from group1 + 3 from group2
    recovery_shares = [
        shares[0][0], shares[0][1],
        shares[1][0], shares[1][1], shares[1][2],
    ]
    recovered = sskr_combine(recovery_shares)
    assert recovered.data == secret_data


def test_share_cbor_roundtrip():
    """Test SSKRShare CBOR serialization roundtrip."""
    register_tags()
    secret_data = b"0123456789abcdef"
    master_secret = SSKRSecret(secret_data)
    group = SSKRGroupSpec(1, 1)
    spec = SSKRSpec(1, [group])

    shares = sskr_generate(spec, master_secret)
    share = shares[0][0]

    cbor = share.tagged_cbor()
    decoded = SSKRShare.from_tagged_cbor(cbor)
    assert share == decoded
