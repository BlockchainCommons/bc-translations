"""Core envelope nesting tests.

Translated from rust/bc-envelope/tests/core_nesting_tests.rs
"""

from bc_envelope import Envelope

from tests.common.check_encoding import check_encoding


def test_predicate_enclosures():
    alice = Envelope("Alice")
    knows = Envelope("knows")
    bob = Envelope("Bob")

    a = Envelope("A")
    b = Envelope("B")

    knows_bob = Envelope.new_assertion(knows, bob)
    assert knows_bob.format() == '"knows": "Bob"'

    ab = Envelope.new_assertion(a, b)
    assert ab.format() == '"A": "B"'

    knows_ab_bob = check_encoding(
        Envelope.new_assertion(
            knows.add_assertion_envelope(ab),
            bob,
        )
    )
    assert knows_ab_bob.format() == (
        '"knows" [\n'
        '    "A": "B"\n'
        "]\n"
        ': "Bob"'
    )

    knows_bob_ab = check_encoding(
        Envelope.new_assertion(
            knows,
            bob.add_assertion_envelope(ab),
        )
    )
    assert knows_bob_ab.format() == (
        '"knows": "Bob" [\n'
        '    "A": "B"\n'
        "]"
    )

    knows_bob_enclose_ab = check_encoding(
        knows_bob.add_assertion_envelope(ab)
    )
    assert knows_bob_enclose_ab.format() == (
        "{\n"
        '    "knows": "Bob"\n'
        "} [\n"
        '    "A": "B"\n'
        "]"
    )

    alice_knows_bob = check_encoding(
        alice.add_assertion_envelope(knows_bob)
    )
    assert alice_knows_bob.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        "]"
    )

    alice_ab_knows_bob = check_encoding(
        alice_knows_bob.add_assertion_envelope(ab)
    )
    assert alice_ab_knows_bob.format() == (
        '"Alice" [\n'
        '    "A": "B"\n'
        '    "knows": "Bob"\n'
        "]"
    )

    alice_knows_ab_bob = check_encoding(
        alice.add_assertion_envelope(
            Envelope.new_assertion(
                knows.add_assertion_envelope(ab),
                bob,
            )
        )
    )
    assert alice_knows_ab_bob.format() == (
        '"Alice" [\n'
        '    "knows" [\n'
        '        "A": "B"\n'
        "    ]\n"
        '    : "Bob"\n'
        "]"
    )

    alice_knows_bob_ab = check_encoding(
        alice.add_assertion_envelope(
            Envelope.new_assertion(
                knows,
                bob.add_assertion_envelope(ab),
            )
        )
    )
    assert alice_knows_bob_ab.format() == (
        '"Alice" [\n'
        '    "knows": "Bob" [\n'
        '        "A": "B"\n'
        "    ]\n"
        "]"
    )

    alice_knows_ab_bob_ab = check_encoding(
        alice.add_assertion_envelope(
            Envelope.new_assertion(
                knows.add_assertion_envelope(ab),
                bob.add_assertion_envelope(ab),
            )
        )
    )
    assert alice_knows_ab_bob_ab.format() == (
        '"Alice" [\n'
        '    "knows" [\n'
        '        "A": "B"\n'
        "    ]\n"
        '    : "Bob" [\n'
        '        "A": "B"\n'
        "    ]\n"
        "]"
    )

    alice_ab_knows_ab_bob_ab = check_encoding(
        alice
        .add_assertion_envelope(ab)
        .add_assertion_envelope(
            Envelope.new_assertion(
                knows.add_assertion_envelope(ab),
                bob.add_assertion_envelope(ab),
            )
        )
    )
    assert alice_ab_knows_ab_bob_ab.format() == (
        '"Alice" [\n'
        '    "A": "B"\n'
        '    "knows" [\n'
        '        "A": "B"\n'
        "    ]\n"
        '    : "Bob" [\n'
        '        "A": "B"\n'
        "    ]\n"
        "]"
    )

    alice_ab_knows_ab_bob_ab_enclose_ab = check_encoding(
        alice
        .add_assertion_envelope(ab)
        .add_assertion_envelope(
            Envelope.new_assertion(
                knows.add_assertion_envelope(ab),
                bob.add_assertion_envelope(ab),
            )
            .add_assertion_envelope(ab)
        )
    )
    assert alice_ab_knows_ab_bob_ab_enclose_ab.format() == (
        '"Alice" [\n'
        "    {\n"
        '        "knows" [\n'
        '            "A": "B"\n'
        "        ]\n"
        '        : "Bob" [\n'
        '            "A": "B"\n'
        "        ]\n"
        "    } [\n"
        '        "A": "B"\n'
        "    ]\n"
        '    "A": "B"\n'
        "]"
    )


def test_nesting_plaintext():
    envelope = Envelope("Hello.")
    assert envelope.format() == '"Hello."'

    elided_envelope = envelope.elide()
    assert elided_envelope.is_equivalent_to(envelope)
    assert elided_envelope.format() == "ELIDED"


def test_nesting_once():
    envelope = check_encoding(Envelope("Hello.").wrap())

    assert envelope.format() == (
        "{\n"
        '    "Hello."\n'
        "}"
    )

    elided_envelope = check_encoding(
        Envelope("Hello.").elide().wrap()
    )
    assert elided_envelope.is_equivalent_to(envelope)
    assert elided_envelope.format() == (
        "{\n"
        "    ELIDED\n"
        "}"
    )


def test_nesting_twice():
    envelope = check_encoding(
        Envelope("Hello.").wrap().wrap()
    )

    assert envelope.format() == (
        "{\n"
        "    {\n"
        '        "Hello."\n'
        "    }\n"
        "}"
    )

    target = envelope.try_unwrap().try_unwrap()
    elided_envelope = envelope.elide_removing_target(target)

    assert elided_envelope.format() == (
        "{\n"
        "    {\n"
        "        ELIDED\n"
        "    }\n"
        "}"
    )
    assert envelope.is_equivalent_to(elided_envelope)


def test_assertions_on_all_parts_of_envelope():
    predicate = Envelope("predicate") \
        .add_assertion("predicate-predicate", "predicate-object")
    obj = Envelope("object") \
        .add_assertion("object-predicate", "object-object")
    envelope = check_encoding(
        Envelope("subject").add_assertion(predicate, obj)
    )

    assert envelope.format() == (
        '"subject" [\n'
        '    "predicate" [\n'
        '        "predicate-predicate": "predicate-object"\n'
        "    ]\n"
        '    : "object" [\n'
        '        "object-predicate": "object-object"\n'
        "    ]\n"
        "]"
    )


def test_assertion_on_bare_assertion():
    envelope = Envelope.new_assertion("predicate", "object") \
        .add_assertion("assertion-predicate", "assertion-object")

    assert envelope.format() == (
        "{\n"
        '    "predicate": "object"\n'
        "} [\n"
        '    "assertion-predicate": "assertion-object"\n'
        "]"
    )
