"""Tests for identifier types: ARID, URI, UUID, and XID.

Translated from Kotlin ARIDTest, URITest, UUIDTest, XIDTest
and Rust id/xid.rs inline tests.
"""

import pytest

from bc_components import (
    ARID,
    URI,
    UUID,
    XID,
    ECPrivateKey,
    PrivateKeyBase,
    SigningPrivateKey,
    register_tags,
)
from bc_components._error import InvalidDataError, InvalidSizeError


# ---------------------------------------------------------------------------
# ARID tests
# ---------------------------------------------------------------------------


def test_arid_create():
    """Test that ARID.generate() creates a 32-byte identifier."""
    arid = ARID.generate()
    assert len(arid.data) == 32


def test_arid_uniqueness():
    """Test that two random ARIDs are different."""
    arid1 = ARID.generate()
    arid2 = ARID.generate()
    assert arid1 != arid2


def test_arid_from_data():
    """Test creating an ARID from a 32-byte array."""
    data = bytes(range(32))
    arid = ARID.from_data(data)
    assert len(arid.data) == 32


def test_arid_invalid_size():
    """Test that creating an ARID from wrong-size data raises an error."""
    data = bytes(16)
    with pytest.raises(InvalidSizeError):
        ARID.from_data(data)


def test_arid_hex_roundtrip():
    """Test ARID hex encoding roundtrip."""
    arid = ARID.generate()
    hex_str = arid.hex()
    assert len(hex_str) == 64  # 32 bytes = 64 hex chars
    arid2 = ARID.from_hex(hex_str)
    assert arid == arid2


def test_arid_short_description():
    """Test ARID short description is first 4 bytes as hex."""
    data = bytes(range(32))
    arid = ARID.from_data(data)
    assert arid.short_description() == "00010203"


def test_arid_cbor_roundtrip():
    """Test ARID CBOR serialization roundtrip."""
    register_tags()
    arid = ARID.generate()
    cbor = arid.tagged_cbor()
    decoded = ARID.from_tagged_cbor(cbor)
    assert arid == decoded


def test_arid_ur_roundtrip():
    """Test ARID UR encoding roundtrip."""
    register_tags()
    arid = ARID.generate()
    ur_string = arid.ur_string()
    decoded = ARID.from_ur_string(ur_string)
    assert arid == decoded


def test_arid_equality():
    """Test ARID equality from identical hex data."""
    hex_str = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
    arid1 = ARID.from_hex(hex_str)
    arid2 = ARID.from_hex(hex_str)
    assert arid1 == arid2


def test_arid_comparable():
    """Test ARID ordering."""
    data1 = bytes(32)  # all zeros
    data2 = bytes([1] * 32)  # all ones
    arid1 = ARID.from_data(data1)
    arid2 = ARID.from_data(data2)
    assert arid1 < arid2


def test_arid_to_string():
    """Test ARID string representation."""
    hex_str = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
    arid = ARID.from_hex(hex_str)
    assert str(arid) == f"ARID({hex_str})"


# ---------------------------------------------------------------------------
# URI tests
# ---------------------------------------------------------------------------


def test_uri_creation():
    """Test URI creation and string access."""
    uri = URI.new("https://example.com")
    assert uri.as_str() == "https://example.com"
    assert str(uri) == "https://example.com"


def test_uri_invalid():
    """Test that URI without scheme raises an error."""
    with pytest.raises(InvalidDataError):
        URI.new("not a valid uri")


def test_uri_no_scheme():
    """Test that bare domain without scheme is rejected."""
    with pytest.raises(InvalidDataError):
        URI.new("example.com")


def test_uri_cbor_roundtrip():
    """Test URI CBOR serialization roundtrip."""
    register_tags()
    uri = URI.new("https://example.com/path?query=value")
    cbor = uri.tagged_cbor()
    decoded = URI.from_tagged_cbor(cbor)
    assert uri == decoded


def test_uri_equality():
    """Test URI equality."""
    uri1 = URI.new("https://example.com")
    uri2 = URI.new("https://example.com")
    assert uri1 == uri2


def test_uri_various_schemes():
    """Test URIs with different schemes."""
    http_uri = URI.new("http://example.com")
    assert str(http_uri) == "http://example.com"

    ftp_uri = URI.new("ftp://files.example.com")
    assert str(ftp_uri) == "ftp://files.example.com"

    mailto_uri = URI.new("mailto:user@example.com")
    assert str(mailto_uri) == "mailto:user@example.com"


# ---------------------------------------------------------------------------
# UUID tests
# ---------------------------------------------------------------------------


def test_uuid_create():
    """Test UUID.generate() creates a 16-byte identifier."""
    uuid = UUID.generate()
    assert len(uuid.data) == 16


def test_uuid_uniqueness():
    """Test that two random UUIDs are different."""
    uuid1 = UUID.generate()
    uuid2 = UUID.generate()
    assert uuid1 != uuid2


def test_uuid_version4():
    """Test that UUID type 4 version nibble is set correctly."""
    uuid = UUID.generate()
    data = uuid.data
    version_nibble = (data[6] & 0xF0) >> 4
    assert version_nibble == 4, "UUID version must be 4"


def test_uuid_variant2():
    """Test that UUID RFC 4122 variant bits are set correctly."""
    uuid = UUID.generate()
    data = uuid.data
    variant_bits = (data[8] & 0xC0) >> 6
    assert variant_bits == 2, "UUID variant must be 2 (RFC 4122)"


def test_uuid_string_format():
    """Test UUID canonical string format: 8-4-4-4-12."""
    data = bytes.fromhex("0123456789abcdef0123456789abcdef")
    uuid = UUID.from_data(data)
    s = str(uuid)
    assert s == "01234567-89ab-cdef-0123-456789abcdef"


def test_uuid_from_string():
    """Test parsing a UUID from the standard string format."""
    uuid_string = "01234567-89ab-cdef-0123-456789abcdef"
    uuid = UUID.from_string(uuid_string)
    assert str(uuid) == uuid_string


def test_uuid_from_data():
    """Test creating a UUID from exactly 16 bytes."""
    data = bytes(range(16))
    uuid = UUID.from_data(data)
    assert len(uuid.data) == 16


def test_uuid_cbor_roundtrip():
    """Test UUID CBOR serialization roundtrip."""
    register_tags()
    uuid = UUID.generate()
    cbor = uuid.tagged_cbor()
    decoded = UUID.from_tagged_cbor(cbor)
    assert uuid == decoded


def test_uuid_equality():
    """Test UUID equality from identical data."""
    data = bytes.fromhex("0123456789abcdef0123456789abcdef")
    uuid1 = UUID.from_data(data)
    uuid2 = UUID.from_data(data)
    assert uuid1 == uuid2


# ---------------------------------------------------------------------------
# XID tests
# ---------------------------------------------------------------------------


SEED_HEX = "59f2293a5bce7d4de59e71b4207ac5d2"


def test_xid_from_key():
    """Test creating a XID from a signing public key."""
    seed_data = bytes.fromhex(SEED_HEX)
    pkb = PrivateKeyBase.from_data(seed_data)
    public_keys = pkb.public_keys()
    xid = XID.new(public_keys.signing_public_key)
    assert xid is not None
    assert len(xid.data) == 32


def test_xid_validate():
    """Test XID validates against the key it was created from."""
    register_tags()
    seed_data = bytes.fromhex(SEED_HEX)
    pkb = PrivateKeyBase.from_data(seed_data)
    signing_pub_key = pkb.schnorr_signing_private_key().public_key()
    xid = XID.new(signing_pub_key)
    assert xid.validate(signing_pub_key)


def test_xid_cbor_roundtrip():
    """Test XID CBOR serialization roundtrip."""
    register_tags()
    seed_data = bytes.fromhex(SEED_HEX)
    pkb = PrivateKeyBase.from_data(seed_data)
    xid = XID.new(pkb.schnorr_signing_private_key().public_key())

    cbor = xid.tagged_cbor()
    decoded = XID.from_tagged_cbor(cbor)
    assert xid == decoded


def test_xid_from_data():
    """Test creating XID from raw hex data and CBOR roundtrip."""
    register_tags()
    xid_hex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037"
    xid = XID.from_data(bytes.fromhex(xid_hex))
    assert len(xid.data) == 32

    cbor = xid.tagged_cbor()
    decoded = XID.from_tagged_cbor(cbor)
    assert xid == decoded


def test_xid_from_hex():
    """Test creating XID from hex string."""
    register_tags()
    xid_hex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037"
    xid = XID.from_hex(xid_hex)
    assert xid.hex() == xid_hex


def test_xid_equality():
    """Test XID equality from the same key."""
    seed_data = bytes.fromhex(SEED_HEX)
    pkb = PrivateKeyBase.from_data(seed_data)
    signing_pub_key = pkb.schnorr_signing_private_key().public_key()

    xid1 = XID.new(signing_pub_key)
    xid2 = XID.new(signing_pub_key)
    assert xid1 == xid2


def test_xid_comparable():
    """Test XID ordering."""
    xid1 = XID.from_hex(
        "0000000000000000000000000000000000000000000000000000000000000001"
    )
    xid2 = XID.from_hex(
        "0000000000000000000000000000000000000000000000000000000000000002"
    )
    assert xid1 < xid2


def test_xid_rust_vectors():
    """Test XID against exact Rust test vectors."""
    register_tags()
    xid_hex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037"
    xid = XID.from_data(bytes.fromhex(xid_hex))
    assert xid.hex() == xid_hex
    assert xid.short_description() == "de285368"
    assert repr(xid) == f"XID({xid_hex})"
    assert str(xid) == "XID(de285368)"

    # UR roundtrip
    xid_ur = xid.ur_string()
    assert xid_ur == (
        "ur:xid/hdcxuedeguisgevwhdaxnbluenutlbglhfiygamsamadmojkdydtnetee"
        "owffhwprtemcaatledk"
    )
    assert XID.from_ur_string(xid_ur) == xid

    # Bytewords identifier
    assert xid.bytewords_identifier(prefix=True) == "\U0001F167 URGE DICE GURU IRIS"
    assert xid.bytemoji_identifier(prefix=True) == "\U0001F167 \U0001F43B \U0001F63B \U0001F35E \U0001F490"


def test_xid_from_key_rust_vectors():
    """Test XID from key with exact Rust test vectors (secp256k1 / Schnorr)."""
    register_tags()
    private_key = SigningPrivateKey.new_schnorr(
        ECPrivateKey.from_data(bytes.fromhex(
            "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
        ))
    )
    public_key = private_key.public_key()

    # Verify the CBOR-encoded public key data matches Rust
    key_cbor_data = public_key.tagged_cbor_data()
    assert key_cbor_data == bytes.fromhex(
        "d99c565820e8251dc3a17e0f2c07865ed191139ecbcddcbdd070ec1ff65df5148c7ef4005a"
    )

    from bc_components import Digest

    digest = Digest.from_image(key_cbor_data)
    assert digest.data == bytes.fromhex(
        "d40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47"
    )

    xid = XID.new(public_key)
    assert xid.hex() == (
        "d40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47"
    )
    assert xid.validate(public_key)
    assert str(xid) == "XID(d40e0602)"
