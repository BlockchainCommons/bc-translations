"""Proof tests for bc-envelope.

Translated from rust/bc-envelope/tests/proof_tests.rs
"""

from textwrap import dedent

from bc_components import ARID, PrivateKeyBase
from bc_envelope import Envelope
from dcbor import Date
import known_values

from tests.common.check_encoding import check_encoding
from tests.common.test_seed import Seed


def test_friends_list():
    # Each "knows" assertion is salted so guessing associates is hard.
    alice_friends = Envelope("Alice")
    for name in ("Bob", "Carol", "Dan"):
        assertion = Envelope.new_assertion("knows", name)
        salted = assertion.add_salt()
        alice_friends = alice_friends.add_assertion_envelope(salted)

    expected = dedent("""\
        "Alice" [
            {
                "knows": "Bob"
            } [
                'salt': Salt
            ]
            {
                "knows": "Carol"
            } [
                'salt': Salt
            ]
            {
                "knows": "Dan"
            } [
                'salt': Salt
            ]
        ]""")
    assert alice_friends.format() == expected

    # Fully elided root
    alice_friends_root = alice_friends.elide_revealing_set(set())
    assert alice_friends_root.format() == "ELIDED"

    # Prove "knows Bob" is contained
    knows_bob_assertion = Envelope.new_assertion("knows", "Bob")
    alice_knows_bob_proof = check_encoding(
        alice_friends.proof_contains_target(knows_bob_assertion)
    )

    expected_proof = dedent("""\
        ELIDED [
            ELIDED [
                ELIDED
            ]
            ELIDED (2)
        ]""")
    assert alice_knows_bob_proof.format() == expected_proof

    # Confirm using the trusted root
    assert alice_friends_root.confirm_contains_target(
        knows_bob_assertion, alice_knows_bob_proof
    )


def test_multi_position():
    alice_friends = Envelope("Alice")
    for name in ("Bob", "Carol", "Dan"):
        assertion = Envelope.new_assertion("knows", name)
        salted = assertion.add_salt()
        alice_friends = alice_friends.add_assertion_envelope(salted)

    # "knows" as a subject envelope exists at three positions
    knows_proof = check_encoding(
        alice_friends.proof_contains_target(Envelope("knows"))
    )

    expected = dedent("""\
        ELIDED [
            {
                ELIDED: ELIDED
            } [
                ELIDED
            ]
            {
                ELIDED: ELIDED
            } [
                ELIDED
            ]
            {
                ELIDED: ELIDED
            } [
                ELIDED
            ]
        ]""")
    assert knows_proof.format() == expected


def test_verifiable_credential():
    alice_seed = Seed(data=bytes.fromhex("82f32c855d3d542256180810797e0073"))
    alice_private_key = PrivateKeyBase(alice_seed.data)
    arid = Envelope(
        ARID.from_data(bytes.fromhex(
            "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d"
        ))
    )

    # Build salted credential assertions
    salted_assertions = [
        ("firstName", "John"),
        ("lastName", "Smith"),
        ("address", "123 Main St."),
    ]

    credential = arid
    for pred, obj in salted_assertions:
        a = Envelope.new_assertion(pred, obj).add_salt()
        credential = credential.add_assertion_envelope(a)

    # birthDate salted
    birth_a = Envelope.new_assertion(
        "birthDate", Date.from_string("1970-01-01").to_tagged_cbor()
    ).add_salt()
    credential = credential.add_assertion_envelope(birth_a)

    # More salted string assertions
    more_salted = [
        ("photo", "This is John Smith's photo."),
        ("dlNumber", "123-456-789"),
    ]
    for pred, obj in more_salted:
        a = Envelope.new_assertion(pred, obj).add_salt()
        credential = credential.add_assertion_envelope(a)

    # Salted bool assertions
    for pred in ("nonCommercialVehicleEndorsement", "motorocycleEndorsement"):
        a = Envelope.new_assertion(pred, True).add_salt()
        credential = credential.add_assertion_envelope(a)

    # Non-salted assertions
    credential = credential.add_assertion(known_values.ISSUER, "State of Example")
    credential = credential.add_assertion(known_values.CONTROLLER, "State of Example")

    # Wrap, sign, add note
    credential = (
        credential.wrap()
        .add_signature(alice_private_key)
        .add_assertion(known_values.NOTE, "Signed by the State of Example")
    )

    credential_root = credential.elide_revealing_set(set())

    # Prove address assertion
    address_assertion = Envelope.new_assertion("address", "123 Main St.")
    address_proof = check_encoding(
        credential.proof_contains_target(address_assertion)
    )

    expected_proof = dedent("""\
        {
            ELIDED [
                ELIDED [
                    ELIDED
                ]
                ELIDED (9)
            ]
        } [
            ELIDED (2)
        ]""")
    assert address_proof.format() == expected_proof

    # Address confirmed
    assert credential_root.confirm_contains_target(
        address_assertion, address_proof
    )

    # Non-salted assertion also confirmed
    issuer_assertion = Envelope.new_assertion(
        known_values.ISSUER, "State of Example"
    )
    assert credential_root.confirm_contains_target(
        issuer_assertion, address_proof
    )

    # Salted assertion NOT confirmed through the address proof
    first_name_assertion = Envelope.new_assertion("firstName", "John")
    assert not credential_root.confirm_contains_target(
        first_name_assertion, address_proof
    )
