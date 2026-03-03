"""Tests for symmetric cryptography, translated from Rust symmetric/mod.rs tests."""

from bc_components import (
    AuthenticationTag,
    EncryptedMessage,
    Nonce,
    SymmetricKey,
    register_tags,
)

# RFC 8439 test vectors
PLAINTEXT = (
    b"Ladies and Gentlemen of the class of '99: "
    b"If I could offer you only one tip for the future, sunscreen would be it."
)
AAD = bytes.fromhex("50515253c0c1c2c3c4c5c6c7")
KEY = SymmetricKey.from_data(
    bytes.fromhex(
        "808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f"
    )
)
NONCE_DATA = Nonce.from_data(bytes.fromhex("070000004041424344454647"))
CIPHERTEXT = bytes.fromhex(
    "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d6"
    "3dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b36"
    "92ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc"
    "3ff4def08e4b7a9de576d26586cec64b6116"
)
AUTH = AuthenticationTag.from_data(
    bytes.fromhex("1ae10b594f09e26a7e902ecbd0600691")
)


def _encrypted_message() -> EncryptedMessage:
    return KEY.encrypt(PLAINTEXT, AAD, NONCE_DATA)


def test_rfc_test_vector():
    """Test RFC 8439 ChaCha20-Poly1305 test vector."""
    encrypted_message = _encrypted_message()
    assert encrypted_message.ciphertext == CIPHERTEXT
    assert encrypted_message.aad == AAD
    assert encrypted_message.nonce == NONCE_DATA
    assert encrypted_message.authentication_tag == AUTH

    decrypted_plaintext = KEY.decrypt(encrypted_message)
    assert decrypted_plaintext == PLAINTEXT


def test_random_key_and_nonce():
    """Test encrypt/decrypt with randomly generated key and nonce."""
    key = SymmetricKey.generate()
    nonce = Nonce.generate()
    encrypted_message = key.encrypt(PLAINTEXT, AAD, nonce)
    decrypted_plaintext = key.decrypt(encrypted_message)
    assert decrypted_plaintext == PLAINTEXT


def test_empty_data():
    """Test encrypt/decrypt with empty plaintext and no AAD."""
    key = SymmetricKey.generate()
    encrypted_message = key.encrypt(b"", None, None)
    decrypted_plaintext = key.decrypt(encrypted_message)
    assert decrypted_plaintext == b""


def test_cbor():
    """Test CBOR encoding/decoding roundtrip for EncryptedMessage."""
    register_tags()
    encrypted_message = _encrypted_message()
    cbor = encrypted_message.tagged_cbor()
    decoded = EncryptedMessage.from_tagged_cbor(cbor)
    assert encrypted_message == decoded


def test_cbor_data():
    """Test CBOR binary data matches exact expected bytes from Rust."""
    register_tags()
    encrypted_message = _encrypted_message()
    cbor = encrypted_message.tagged_cbor()
    data = cbor.to_cbor_data()
    expected = bytes.fromhex(
        "d99c42845872d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2"
        "b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6"
        "a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585"
        "808b4831d7bc3ff4def08e4b7a9de576d26586cec64b61164c070000004041424"
        "344454647501ae10b594f09e26a7e902ecbd06006914c50515253c0c1c2c3c4c5"
        "c6c7"
    )
    assert data == expected


def test_ur():
    """Test UR encoding/decoding roundtrip with exact expected UR string."""
    register_tags()
    encrypted_message = _encrypted_message()
    ur_string = encrypted_message.ur_string()
    expected_ur = (
        "ur:encrypted/lrhdjptecylgeeiemnhnuykglnperfguwskbsaoxpmwegydtjtay"
        "zeptvoreosenwyidtbfsrnoxhylkptiobglfzszointnmojplucyjsuebknnambddt"
        "ahtbonrpkbsnfrenmoutrylbdpktlulkmkaxplvldeascwhdzsqddkvezstbkpmwgo"
        "lplalufdehtsrffhwkuewtmngrknntvwkotdihlntoswgrhscmgsataeaeaefzfpfw"
        "fxfyfefgflgdcyvybdhkgwasvoimkbmhdmsbtihnammegsgdgygmgurtsesasrsss"
        "kswstcfnbpdct"
    )
    assert ur_string == expected_ur
    decoded = EncryptedMessage.from_ur_string(ur_string)
    assert encrypted_message == decoded
