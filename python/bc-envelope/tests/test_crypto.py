"""Crypto tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/crypto_tests.rs
"""

import known_values
from bc_components import SymmetricKey
from bc_envelope import Envelope, extract_subject

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    PLAINTEXT_HELLO,
    alice_private_keys,
    alice_public_keys,
    bob_private_keys,
    bob_public_keys,
    carol_private_keys,
    carol_public_keys,
    hello_envelope,
)


def test_plaintext():
    """Alice sends a plaintext message to Bob."""
    envelope = hello_envelope()
    cbor = envelope.tagged_cbor()

    expected_format = '"Hello."'
    assert envelope.format() == expected_format

    # Alice -> Bob
    received_plaintext = extract_subject(
        Envelope.from_tagged_cbor(cbor),
        str,
    )
    assert received_plaintext == PLAINTEXT_HELLO


def test_symmetric_encryption():
    """Alice and Bob share a symmetric key."""
    key = SymmetricKey.generate()

    # Alice sends an encrypted message
    envelope = check_encoding(hello_envelope().encrypt_subject(key))
    cbor = envelope.tagged_cbor()

    expected_format = "ENCRYPTED"
    assert envelope.format() == expected_format

    # Alice -> Bob
    received_envelope = check_encoding(Envelope.from_tagged_cbor(cbor))

    # Bob decrypts and reads the message
    received_plaintext = extract_subject(
        received_envelope.decrypt_subject(key),
        str,
    )
    assert received_plaintext == PLAINTEXT_HELLO

    # Can't read with no key
    try:
        extract_subject(received_envelope, str)
        assert False, "Should have raised"
    except Exception:
        pass

    # Can't read with incorrect key
    try:
        received_envelope.decrypt_subject(SymmetricKey.generate())
        assert False, "Should have raised"
    except Exception:
        pass


def _round_trip_test(envelope: Envelope):
    key = SymmetricKey.generate()
    plaintext_subject = check_encoding(envelope)
    encrypted_subject = plaintext_subject.encrypt_subject(key)
    assert encrypted_subject.is_equivalent_to(plaintext_subject)
    plaintext_subject2 = check_encoding(encrypted_subject.decrypt_subject(key))
    assert encrypted_subject.is_equivalent_to(plaintext_subject2)
    assert plaintext_subject.is_identical_to(plaintext_subject2)


def test_encrypt_decrypt():
    """Encrypt/decrypt round-trip for various envelope types."""
    # leaf
    _round_trip_test(Envelope(PLAINTEXT_HELLO))

    # node
    _round_trip_test(Envelope("Alice").add_assertion("knows", "Bob"))

    # wrapped
    _round_trip_test(Envelope("Alice").wrap())

    # known value
    _round_trip_test(Envelope(known_values.IS_A))

    # assertion
    _round_trip_test(Envelope.new_assertion("knows", "Bob"))

    # compressed
    _round_trip_test(Envelope(PLAINTEXT_HELLO).compress())


def test_sign_then_encrypt():
    """Alice signs a plaintext message, then encrypts it."""
    key = SymmetricKey.generate()

    envelope = check_encoding(
        check_encoding(
            hello_envelope()
            .add_signature(alice_private_keys())
        )
        .wrap()
        .encrypt_subject(key)
    )
    cbor = envelope.tagged_cbor()

    expected_format = "ENCRYPTED"
    assert envelope.format() == expected_format

    # Alice -> Bob
    received_plaintext = extract_subject(
        check_encoding(
            check_encoding(Envelope.from_tagged_cbor(cbor))
            .decrypt_subject(key)
        )
        .try_unwrap()
        .verify_signature_from(alice_public_keys()),
        str,
    )
    assert received_plaintext == PLAINTEXT_HELLO


def test_encrypt_then_sign():
    """Alice encrypts a plaintext message, then signs it."""
    key = SymmetricKey.generate()

    envelope = check_encoding(
        hello_envelope()
        .encrypt_subject(key)
        .add_signature(alice_private_keys())
    )
    cbor = envelope.tagged_cbor()

    expected_format = (
        "ENCRYPTED [\n"
        "    'signed': Signature\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob
    received_plaintext = extract_subject(
        check_encoding(
            check_encoding(Envelope.from_tagged_cbor(cbor))
            .verify_signature_from(alice_public_keys())
            .decrypt_subject(key)
        ),
        str,
    )
    assert received_plaintext == PLAINTEXT_HELLO


def test_multi_recipient():
    """Alice encrypts a message for Bob and Carol."""
    content_key = SymmetricKey.generate()
    envelope = check_encoding(
        hello_envelope()
        .encrypt_subject(content_key)
        .add_recipient(bob_public_keys(), content_key)
        .add_recipient(carol_public_keys(), content_key)
    )
    cbor = envelope.tagged_cbor()

    expected_format = (
        "ENCRYPTED [\n"
        "    'hasRecipient': SealedMessage\n"
        "    'hasRecipient': SealedMessage\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob, Carol
    received_envelope = Envelope.from_tagged_cbor(cbor)

    # Bob decrypts and reads the message
    bob_received_plaintext = extract_subject(
        check_encoding(
            received_envelope
            .decrypt_subject_to_recipient(bob_private_keys())
        ),
        str,
    )
    assert bob_received_plaintext == PLAINTEXT_HELLO

    # Carol decrypts and reads the message
    carol_received_plaintext = extract_subject(
        check_encoding(
            received_envelope
            .decrypt_subject_to_recipient(carol_private_keys())
        ),
        str,
    )
    assert carol_received_plaintext == PLAINTEXT_HELLO

    # Alice didn't encrypt it to herself
    try:
        received_envelope.decrypt_subject_to_recipient(alice_private_keys())
        assert False, "Should have raised"
    except Exception:
        pass


def test_visible_signature_multi_recipient():
    """Alice signs, then encrypts for Bob and Carol."""
    content_key = SymmetricKey.generate()
    envelope = check_encoding(
        hello_envelope()
        .add_signature(alice_private_keys())
        .encrypt_subject(content_key)
        .add_recipient(bob_public_keys(), content_key)
        .add_recipient(carol_public_keys(), content_key)
    )
    cbor = envelope.tagged_cbor()

    expected_format = (
        "ENCRYPTED [\n"
        "    'hasRecipient': SealedMessage\n"
        "    'hasRecipient': SealedMessage\n"
        "    'signed': Signature\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob, Carol
    received_envelope = Envelope.from_tagged_cbor(cbor)

    # Bob validates Alice's signature, then decrypts
    bob_received_plaintext = extract_subject(
        check_encoding(
            received_envelope
            .verify_signature_from(alice_public_keys())
            .decrypt_subject_to_recipient(bob_private_keys())
        ),
        str,
    )
    assert bob_received_plaintext == PLAINTEXT_HELLO

    # Carol validates Alice's signature, then decrypts
    carol_received_plaintext = extract_subject(
        check_encoding(
            received_envelope
            .verify_signature_from(alice_public_keys())
            .decrypt_subject_to_recipient(carol_private_keys())
        ),
        str,
    )
    assert carol_received_plaintext == PLAINTEXT_HELLO

    # Alice didn't encrypt it to herself
    try:
        received_envelope.decrypt_subject_to_recipient(alice_private_keys())
        assert False, "Should have raised"
    except Exception:
        pass


def test_hidden_signature_multi_recipient():
    """Alice signs, wraps, then encrypts for Bob and Carol (hidden signature)."""
    content_key = SymmetricKey.generate()
    envelope = check_encoding(
        hello_envelope()
        .add_signature(alice_private_keys())
        .wrap()
        .encrypt_subject(content_key)
        .add_recipient(bob_public_keys(), content_key)
        .add_recipient(carol_public_keys(), content_key)
    )
    cbor = envelope.tagged_cbor()

    expected_format = (
        "ENCRYPTED [\n"
        "    'hasRecipient': SealedMessage\n"
        "    'hasRecipient': SealedMessage\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob, Carol
    received_envelope = Envelope.from_tagged_cbor(cbor)

    # Bob decrypts, unwraps, validates Alice's signature, reads message
    bob_received_plaintext = extract_subject(
        check_encoding(
            received_envelope
            .decrypt_subject_to_recipient(bob_private_keys())
            .try_unwrap()
            .verify_signature_from(alice_public_keys())
        ),
        str,
    )
    assert bob_received_plaintext == PLAINTEXT_HELLO

    # Carol decrypts, unwraps, validates Alice's signature, reads message
    carol_received_plaintext = extract_subject(
        check_encoding(
            received_envelope
            .decrypt_subject_to_recipient(carol_private_keys())
            .try_unwrap()
            .verify_signature_from(alice_public_keys())
        ),
        str,
    )
    assert carol_received_plaintext == PLAINTEXT_HELLO

    # Alice didn't encrypt it to herself
    try:
        received_envelope.decrypt_subject_to_recipient(alice_private_keys())
        assert False, "Should have raised"
    except Exception:
        pass


def test_secret_with_hkdf():
    """Alice encrypts a message with a password using HKDF."""
    from bc_components.encrypted_key import KeyDerivationMethod

    bob_password = b"correct horse battery staple"

    envelope = hello_envelope().lock(KeyDerivationMethod.HKDF, bob_password)
    check_encoding(envelope)
    ur = envelope.ur()

    expected_format = (
        "ENCRYPTED [\n"
        "    'hasSecret': EncryptedKey(HKDF(SHA256))\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob, Eve
    received_envelope = Envelope.from_ur(ur)

    # Bob decrypts and reads the message
    bob_received_plaintext = extract_subject(
        check_encoding(
            received_envelope.unlock(bob_password)
        ),
        str,
    )
    assert bob_received_plaintext == PLAINTEXT_HELLO

    # Eve tries with a wrong password
    try:
        received_envelope.unlock(b"wrong password")
        assert False, "Should have raised"
    except Exception:
        pass


def test_secret_with_scrypt():
    """Alice encrypts with multiple passwords using different KDFs."""
    from bc_components.encrypted_key import KeyDerivationMethod

    bob_password = b"correct horse battery staple"
    carol_password = b"Able was I ere I saw Elba"
    gracy_password = b"Madam, in Eden, I'm Adam"
    content_key = SymmetricKey.generate()

    envelope = check_encoding(
        hello_envelope()
        .encrypt_subject(content_key)
        .add_secret(KeyDerivationMethod.HKDF, bob_password, content_key)
        .add_secret(KeyDerivationMethod.SCRYPT, carol_password, content_key)
        .add_secret(KeyDerivationMethod.ARGON2ID, gracy_password, content_key)
    )
    ur = envelope.ur()

    expected_format = (
        "ENCRYPTED [\n"
        "    'hasSecret': EncryptedKey(Argon2id)\n"
        "    'hasSecret': EncryptedKey(HKDF(SHA256))\n"
        "    'hasSecret': EncryptedKey(Scrypt)\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob, Carol, Gracy, Eve
    received_envelope = Envelope.from_ur(ur)

    # Bob decrypts and reads
    bob_received_plaintext = extract_subject(
        check_encoding(
            received_envelope.unlock_subject(bob_password)
        ),
        str,
    )
    assert bob_received_plaintext == PLAINTEXT_HELLO

    # Carol decrypts and reads
    carol_received_plaintext = extract_subject(
        check_encoding(
            received_envelope.unlock_subject(carol_password)
        ),
        str,
    )
    assert carol_received_plaintext == PLAINTEXT_HELLO

    # Gracy decrypts and reads
    gracy_received_plaintext = extract_subject(
        check_encoding(
            received_envelope.unlock_subject(gracy_password)
        ),
        str,
    )
    assert gracy_received_plaintext == PLAINTEXT_HELLO

    # Eve tries with a wrong password
    try:
        received_envelope.unlock_subject(b"wrong password")
        assert False, "Should have raised"
    except Exception:
        pass
