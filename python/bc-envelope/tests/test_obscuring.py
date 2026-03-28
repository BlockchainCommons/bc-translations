"""Obscuring tests.

Translated from rust/bc-envelope/tests/obscuring_tests.rs

Tests the transformation of different kinds of "obscured" envelopes into
others.  Some transformations are allowed, some are idempotent, and some
raise errors.

| Operation > | Encrypt | Elide      | Compress   |
|:------------|:--------|:-----------|:-----------|
| Encrypted   | ERROR   | OK         | ERROR      |
| Elided      | ERROR   | IDEMPOTENT | ERROR      |
| Compressed  | OK      | OK         | IDEMPOTENT |
"""

from bc_components import Digest, SymmetricKey
from bc_envelope import Envelope, ObscureAction, ObscureType

from tests.common.test_data import PLAINTEXT_HELLO


def test_obscuring():
    key = SymmetricKey.generate()
    envelope = Envelope(PLAINTEXT_HELLO)
    assert not envelope.is_obscured()

    encrypted = envelope.encrypt_subject(key)
    assert encrypted.is_obscured()

    elided = envelope.elide()
    assert elided.is_obscured()

    compressed = envelope.compress()
    assert compressed.is_obscured()

    # --- ENCRYPTION ---

    # Cannot encrypt an encrypted envelope.
    try:
        encrypted.encrypt_subject(key)
        assert False, "Expected error"
    except Exception:
        pass

    # Cannot encrypt an elided envelope.
    try:
        elided.encrypt_subject(key)
        assert False, "Expected error"
    except Exception:
        pass

    # OK to encrypt a compressed envelope.
    encrypted_compressed = compressed.encrypt_subject(key)
    assert encrypted_compressed.is_encrypted()

    # --- ELISION ---

    # OK to elide an encrypted envelope.
    elided_encrypted = encrypted.elide()
    assert elided_encrypted.is_elided()

    # Eliding an elided envelope is idempotent.
    elided_elided = elided.elide()
    assert elided_elided.is_elided()

    # OK to elide a compressed envelope.
    elided_compressed = compressed.elide()
    assert elided_compressed.is_elided()

    # --- COMPRESSION ---

    # Cannot compress an encrypted envelope.
    try:
        encrypted.compress()
        assert False, "Expected error"
    except Exception:
        pass

    # Cannot compress an elided envelope.
    try:
        elided.compress()
        assert False, "Expected error"
    except Exception:
        pass

    # Compressing a compressed envelope is idempotent.
    compressed_compressed = compressed.compress()
    assert compressed_compressed.is_compressed()


def test_nodes_matching():
    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("age", 30)
        .add_assertion("city", "Boston")
    )

    # Get some digests for targeting
    knows_assertion = envelope.assertion_with_predicate("knows")
    knows_digest = knows_assertion.digest()

    age_assertion = envelope.assertion_with_predicate("age")
    age_digest = age_assertion.digest()

    # Elide one assertion, compress another
    elide_target = {knows_digest}
    compress_target = {age_digest}

    obscured = envelope.elide_removing_set(elide_target)
    obscured = obscured.elide_removing_set_with_action(
        compress_target,
        ObscureAction.compress(),
    )

    # Verify the structure with elided and compressed nodes
    assert obscured.format() == (
        '"Alice" [\n'
        '    "city": "Boston"\n'
        '    COMPRESSED\n'
        '    ELIDED\n'
        ']'
    )

    # Test finding elided nodes
    elided_nodes = obscured.nodes_matching(None, [ObscureType.ELIDED])
    assert knows_digest in elided_nodes

    # Test finding compressed nodes
    compressed_nodes = obscured.nodes_matching(None, [ObscureType.COMPRESSED])
    assert age_digest in compressed_nodes

    # Test finding with target filter
    target_filter = {knows_digest}
    filtered = obscured.nodes_matching(target_filter, [ObscureType.ELIDED])
    assert len(filtered) == 1
    assert knows_digest in filtered

    # Test finding all obscured nodes (no type filter)
    all_in_target = obscured.nodes_matching(elide_target, [])
    assert len(all_in_target) == 1
    assert knows_digest in all_in_target

    # Test with no matches
    no_match_target = {Digest.from_image(b"nonexistent")}
    no_matches = obscured.nodes_matching(no_match_target, [ObscureType.ELIDED])
    assert len(no_matches) == 0


def test_walk_unelide():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    carol = Envelope("Carol")

    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("friend", "Carol")
    )

    # Elide multiple parts
    elided = envelope.elide_removing_target(alice).elide_removing_target(bob)

    assert elided.format() == (
        'ELIDED [\n'
        '    "friend": "Carol"\n'
        '    "knows": ELIDED\n'
        ']'
    )

    # Restore with walk_unelide
    restored = elided.walk_unelide([alice, bob, carol])
    assert restored.format() == (
        '"Alice" [\n'
        '    "friend": "Carol"\n'
        '    "knows": "Bob"\n'
        ']'
    )

    # Test with partial restoration (only some envelopes provided)
    partial = elided.walk_unelide([alice])
    assert partial.format() == (
        '"Alice" [\n'
        '    "friend": "Carol"\n'
        '    "knows": ELIDED\n'
        ']'
    )

    # Test with no matching envelopes
    unchanged = elided.walk_unelide([])
    assert unchanged.is_identical_to(elided)


def test_walk_decrypt():
    key1 = SymmetricKey.generate()
    key2 = SymmetricKey.generate()
    key3 = SymmetricKey.generate()

    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("age", 30)
        .add_assertion("city", "Boston")
    )

    # Encrypt different parts with different keys
    knows_assertion = envelope.assertion_with_predicate("knows")
    age_assertion = envelope.assertion_with_predicate("age")

    encrypt1_target = {knows_assertion.digest()}
    encrypt2_target = {age_assertion.digest()}

    encrypted = envelope.elide_removing_set_with_action(
        encrypt1_target, ObscureAction.encrypt(key1),
    ).elide_removing_set_with_action(
        encrypt2_target, ObscureAction.encrypt(key2),
    )

    assert encrypted.format() == (
        '"Alice" [\n'
        '    "city": "Boston"\n'
        '    ENCRYPTED (2)\n'
        ']'
    )

    # Decrypt with all keys
    decrypted = encrypted.walk_decrypt([key1, key2])
    assert decrypted.format() == (
        '"Alice" [\n'
        '    "age": 30\n'
        '    "city": "Boston"\n'
        '    "knows": "Bob"\n'
        ']'
    )

    # Decrypt with only one key (partial decryption)
    partial = encrypted.walk_decrypt([key1])
    assert not partial.is_identical_to(encrypted)
    assert partial.is_equivalent_to(envelope)

    assert partial.format() == (
        '"Alice" [\n'
        '    "city": "Boston"\n'
        '    "knows": "Bob"\n'
        '    ENCRYPTED\n'
        ']'
    )

    # Decrypt with wrong key (should be unchanged)
    unchanged = encrypted.walk_decrypt([key3])
    assert unchanged.is_identical_to(encrypted)


def test_walk_decompress():
    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("bio", "A" * 1000)
        .add_assertion("description", "B" * 1000)
    )

    # Compress multiple parts
    bio_assertion = envelope.assertion_with_predicate("bio")
    desc_assertion = envelope.assertion_with_predicate("description")

    bio_digest = bio_assertion.digest()
    desc_digest = desc_assertion.digest()

    compress_target = {bio_digest, desc_digest}
    compressed = envelope.elide_removing_set_with_action(
        compress_target, ObscureAction.compress(),
    )

    assert compressed.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    COMPRESSED (2)\n'
        ']'
    )

    # decompress all
    decompressed = compressed.walk_decompress(None)
    assert decompressed.is_equivalent_to(envelope)

    # Decompress with target filter (only one node)
    target = {bio_digest}
    partial = compressed.walk_decompress(target)
    assert not partial.is_identical_to(compressed)
    assert partial.is_equivalent_to(envelope)

    # Bio should be decompressed but description still compressed
    still_compressed = partial.nodes_matching(None, [ObscureType.COMPRESSED])
    assert desc_digest in still_compressed
    assert bio_digest not in still_compressed

    # Decompress with non-matching target (should be unchanged)
    no_match = {Digest.from_image(b"nonexistent")}
    unchanged = compressed.walk_decompress(no_match)
    assert unchanged.is_identical_to(compressed)


def test_mixed_obscuration_operations():
    key = SymmetricKey.generate()

    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("age", 30)
        .add_assertion("bio", "A" * 1000)
    )

    knows_assertion = envelope.assertion_with_predicate("knows")
    age_assertion = envelope.assertion_with_predicate("age")
    bio_assertion = envelope.assertion_with_predicate("bio")

    knows_digest = knows_assertion.digest()
    age_digest = age_assertion.digest()
    bio_digest = bio_assertion.digest()

    # Apply different obscuration types
    elide_target = {knows_digest}
    encrypt_target = {age_digest}
    compress_target = {bio_digest}

    obscured = (
        envelope
        .elide_removing_set(elide_target)
        .elide_removing_set_with_action(
            encrypt_target, ObscureAction.encrypt(key),
        )
        .elide_removing_set_with_action(
            compress_target, ObscureAction.compress(),
        )
    )

    # Verify different obscuration types
    elided = obscured.nodes_matching(None, [ObscureType.ELIDED])
    encrypted = obscured.nodes_matching(None, [ObscureType.ENCRYPTED])
    compressed = obscured.nodes_matching(None, [ObscureType.COMPRESSED])

    assert knows_digest in elided
    assert age_digest in encrypted
    assert bio_digest in compressed

    # Restore everything
    restored = (
        obscured
        .walk_unelide([knows_assertion])
        .walk_decrypt([key])
        .walk_decompress(None)
    )

    assert restored.is_equivalent_to(envelope)
