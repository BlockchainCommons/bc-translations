"""SSKR tests for bc-envelope.

Translated from rust/bc-envelope/tests/sskr_tests.rs
"""

from textwrap import dedent

from bc_components import SSKRGroupSpec, SSKRSpec, SymmetricKey
from bc_envelope import Envelope
from dcbor import Date

from tests.common.test_seed import Seed


def test_sskr():
    # Dan has a seed he wants to back up using social recovery.
    dan_seed = Seed(
        data=bytes.fromhex("59f2293a5bce7d4de59e71b4207ac5d2"),
        name="Dark Purple Aqua Love",
        creation_date=Date.from_string("2021-02-24"),
        note=(
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, "
            "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
        ),
    )

    content_key = SymmetricKey.generate()
    seed_envelope = dan_seed.to_envelope()
    encrypted_seed_envelope = seed_envelope.wrap().encrypt_subject(content_key)

    group = SSKRGroupSpec(2, 3)
    spec = SSKRSpec(1, [group])
    envelopes = encrypted_seed_envelope.sskr_split(spec, content_key)

    # Flatten
    sent_envelopes = [e for group in envelopes for e in group]

    expected_format = dedent("""\
        ENCRYPTED [
            'sskrShare': SSKRShare
        ]""")
    assert sent_envelopes[0].format() == expected_format

    # Round-trip through CBOR (simulating UR send/receive)
    bob_cbor = sent_envelopes[1].tagged_cbor()
    carol_cbor = sent_envelopes[2].tagged_cbor()
    bob_envelope = Envelope.from_tagged_cbor(bob_cbor)
    carol_envelope = Envelope.from_tagged_cbor(carol_cbor)

    # Recover with 2 of 3 shares
    recovered_seed_envelope = (
        Envelope.sskr_join([bob_envelope, carol_envelope]).try_unwrap()
    )

    recovered_seed = Seed.from_envelope(recovered_seed_envelope)

    assert dan_seed.data == recovered_seed.data
    assert dan_seed.creation_date == recovered_seed.creation_date
    assert dan_seed.name == recovered_seed.name
    assert dan_seed.note == recovered_seed.note

    # Recovery with only 1 share fails
    try:
        Envelope.sskr_join([bob_envelope])
        assert False, "Expected error with insufficient shares"
    except Exception:
        pass
