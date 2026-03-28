"""Multi-permit encryption tests for bc-envelope.

Translated from rust/bc-envelope/tests/multi_permit_tests.rs
"""

from textwrap import dedent

from bc_components import (
    KeyDerivationMethod,
    SSKRGroupSpec,
    SSKRSpec,
    SymmetricKey,
    keypair,
)
from bc_envelope import Envelope
from dcbor import Date
import known_values


def test_multi_permit():
    # Alice composes a poem.
    poem_text = "At midnight, the clocks sang lullabies to the wandering teacups."

    original_envelope = (
        Envelope(poem_text)
        .add_type("poem")
        .add_assertion("title", "A Song of Ice Cream")
        .add_assertion("author", "Plonkus the Iridescent")
        .add_assertion(known_values.DATE, Date.from_ymd(2025, 5, 15).to_tagged_cbor())
    )

    # Sign with Alice's private key.
    alice_private_keys, alice_public_keys = keypair()
    signed_envelope = original_envelope.sign(alice_private_keys)

    expected = dedent("""\
        {
            "At midnight, the clocks sang lullabies to the wandering teacups." [
                'isA': "poem"
                "author": "Plonkus the Iridescent"
                "title": "A Song of Ice Cream"
                'date': 2025-05-15
            ]
        } [
            'signed': Signature
        ]""")
    assert signed_envelope.format() == expected

    # Encrypt with a random content key.
    content_key = SymmetricKey.generate()
    encrypted_envelope = signed_envelope.encrypt(content_key)
    assert encrypted_envelope.format() == "ENCRYPTED"

    # Permit 1: password-based secret (Argon2id)
    password = b"unicorns_dance_on_mars_while_eating_pizza"
    locked_envelope = encrypted_envelope.add_secret(
        KeyDerivationMethod.ARGON2ID, password, content_key
    )

    expected_locked = dedent("""\
        ENCRYPTED [
            'hasSecret': EncryptedKey(Argon2id)
        ]""")
    assert locked_envelope.format() == expected_locked

    # Permit 2 & 3: recipients (Alice and Bob)
    bob_private_keys, bob_public_keys = keypair()
    locked_envelope = (
        locked_envelope
        .add_recipient(alice_public_keys, content_key)
        .add_recipient(bob_public_keys, content_key)
    )

    expected_recipients = dedent("""\
        ENCRYPTED [
            'hasRecipient': SealedMessage
            'hasRecipient': SealedMessage
            'hasSecret': EncryptedKey(Argon2id)
        ]""")
    assert locked_envelope.format() == expected_recipients

    # Permit 4: SSKR shares (2-of-3)
    sskr_group = SSKRGroupSpec(2, 3)
    spec = SSKRSpec(1, [sskr_group])
    sharded_envelopes = locked_envelope.sskr_split_flattened(spec, content_key)

    expected_sharded = dedent("""\
        ENCRYPTED [
            'hasRecipient': SealedMessage
            'hasRecipient': SealedMessage
            'hasSecret': EncryptedKey(Argon2id)
            'sskrShare': SSKRShare
        ]""")
    assert sharded_envelopes[0].format() == expected_sharded

    # --- Unlock via content key ---
    received = sharded_envelopes[0]
    unlocked = received.decrypt(content_key)
    assert unlocked == signed_envelope

    # --- Unlock via password ---
    unlocked = received.unlock(password)
    assert unlocked == signed_envelope

    # --- Unlock via Alice's private key ---
    unlocked = received.decrypt_to_recipient(alice_private_keys)
    assert unlocked == signed_envelope

    # --- Unlock via Bob's private key ---
    unlocked = received.decrypt_to_recipient(bob_private_keys)
    assert unlocked == signed_envelope

    # --- Unlock via 2-of-3 SSKR shares ---
    unlocked = (
        Envelope.sskr_join([sharded_envelopes[0], sharded_envelopes[2]])
        .try_unwrap()
    )
    assert unlocked == signed_envelope

    # Verify Alice's signature
    unlocked.verify(alice_public_keys)
