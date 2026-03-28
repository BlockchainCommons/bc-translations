"""Elision tests.

Translated from rust/bc-envelope/tests/elision_tests.rs
"""

from bc_envelope import Envelope, InvalidFormat

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    assertion_envelope,
    double_assertion_envelope,
    single_assertion_envelope,
)


# ---------------------------------------------------------------------------
# Helpers (local to this test module)
# ---------------------------------------------------------------------------

def basic_envelope() -> Envelope:
    return Envelope("Hello.")


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------


def test_envelope_elision():
    e1 = basic_envelope()

    e2 = e1.elide()
    assert e1.is_equivalent_to(e2)
    assert not e1.is_identical_to(e2)

    assert e2.format() == "ELIDED"

    assert e2.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'\n"
        ")"
    )

    e3 = e2.unelide(e1)
    assert e3.is_equivalent_to(e1)
    assert e3.format() == '"Hello."'


def test_single_assertion_remove_elision():
    e1 = single_assertion_envelope()
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        ']'
    )

    # Elide the entire envelope
    e2 = check_encoding(e1.elide_removing_target(e1))
    assert e2.format() == "ELIDED"

    # Elide just the envelope's subject
    e3 = check_encoding(e1.elide_removing_target(Envelope("Alice")))
    assert e3.format() == (
        'ELIDED [\n'
        '    "knows": "Bob"\n'
        ']'
    )

    # Elide just the assertion's predicate
    e4 = check_encoding(e1.elide_removing_target(Envelope("knows")))
    assert e4.format() == (
        '"Alice" [\n'
        '    ELIDED: "Bob"\n'
        ']'
    )

    # Elide just the assertion's object
    e5 = check_encoding(e1.elide_removing_target(Envelope("Bob")))
    assert e5.format() == (
        '"Alice" [\n'
        '    "knows": ELIDED\n'
        ']'
    )

    # Elide the entire assertion
    e6 = check_encoding(e1.elide_removing_target(assertion_envelope()))
    assert e6.format() == (
        '"Alice" [\n'
        '    ELIDED\n'
        ']'
    )


def test_double_assertion_remove_elision():
    e1 = double_assertion_envelope()
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        ']'
    )

    # Elide the entire envelope
    e2 = check_encoding(e1.elide_removing_target(e1))
    assert e2.format() == "ELIDED"

    # Elide just the envelope's subject
    e3 = check_encoding(e1.elide_removing_target(Envelope("Alice")))
    assert e3.format() == (
        'ELIDED [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        ']'
    )

    # Elide just the assertion's predicate
    e4 = check_encoding(e1.elide_removing_target(Envelope("knows")))
    assert e4.format() == (
        '"Alice" [\n'
        '    ELIDED: "Bob"\n'
        '    ELIDED: "Carol"\n'
        ']'
    )

    # Elide just the assertion's object
    e5 = check_encoding(e1.elide_removing_target(Envelope("Bob")))
    assert e5.format() == (
        '"Alice" [\n'
        '    "knows": "Carol"\n'
        '    "knows": ELIDED\n'
        ']'
    )

    # Elide the entire assertion
    e6 = check_encoding(e1.elide_removing_target(assertion_envelope()))
    assert e6.format() == (
        '"Alice" [\n'
        '    "knows": "Carol"\n'
        '    ELIDED\n'
        ']'
    )


def test_single_assertion_reveal_elision():
    e1 = single_assertion_envelope()
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        ']'
    )

    # Elide revealing nothing
    e2 = check_encoding(e1.elide_revealing_array([]))
    assert e2.format() == "ELIDED"

    # Reveal just the envelope's structure
    e3 = check_encoding(e1.elide_revealing_array([e1]))
    assert e3.format() == (
        'ELIDED [\n'
        '    ELIDED\n'
        ']'
    )

    # Reveal just the envelope's subject
    e4 = check_encoding(e1.elide_revealing_array([e1, Envelope("Alice")]))
    assert e4.format() == (
        '"Alice" [\n'
        '    ELIDED\n'
        ']'
    )

    # Reveal just the assertion's structure
    e5 = check_encoding(e1.elide_revealing_array([e1, assertion_envelope()]))
    assert e5.format() == (
        'ELIDED [\n'
        '    ELIDED: ELIDED\n'
        ']'
    )

    # Reveal just the assertion's predicate
    e6 = check_encoding(e1.elide_revealing_array([
        e1,
        assertion_envelope(),
        Envelope("knows"),
    ]))
    assert e6.format() == (
        'ELIDED [\n'
        '    "knows": ELIDED\n'
        ']'
    )

    # Reveal just the assertion's object
    e7 = check_encoding(e1.elide_revealing_array([
        e1,
        assertion_envelope(),
        Envelope("Bob"),
    ]))
    assert e7.format() == (
        'ELIDED [\n'
        '    ELIDED: "Bob"\n'
        ']'
    )


def test_double_assertion_reveal_elision():
    e1 = double_assertion_envelope()
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        ']'
    )

    # Elide revealing nothing
    e2 = check_encoding(e1.elide_revealing_array([]))
    assert e2.format() == "ELIDED"

    # Reveal just the envelope's structure
    e3 = check_encoding(e1.elide_revealing_array([e1]))
    assert e3.format() == (
        'ELIDED [\n'
        '    ELIDED (2)\n'
        ']'
    )

    # Reveal just the envelope's subject
    e4 = check_encoding(e1.elide_revealing_array([e1, Envelope("Alice")]))
    assert e4.format() == (
        '"Alice" [\n'
        '    ELIDED (2)\n'
        ']'
    )

    # Reveal just the assertion's structure
    e5 = check_encoding(e1.elide_revealing_array([e1, assertion_envelope()]))
    assert e5.format() == (
        'ELIDED [\n'
        '    ELIDED: ELIDED\n'
        '    ELIDED\n'
        ']'
    )

    # Reveal just the assertion's predicate
    e6 = check_encoding(e1.elide_revealing_array([
        e1,
        assertion_envelope(),
        Envelope("knows"),
    ]))
    assert e6.format() == (
        'ELIDED [\n'
        '    "knows": ELIDED\n'
        '    ELIDED\n'
        ']'
    )

    # Reveal just the assertion's object
    e7 = check_encoding(e1.elide_revealing_array([
        e1,
        assertion_envelope(),
        Envelope("Bob"),
    ]))
    assert e7.format() == (
        'ELIDED [\n'
        '    ELIDED: "Bob"\n'
        '    ELIDED\n'
        ']'
    )


def test_digests():
    e1 = double_assertion_envelope()
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        ']'
    )

    e2 = check_encoding(e1.elide_revealing_set(e1.digests(0)))
    assert e2.format() == "ELIDED"

    e3 = check_encoding(e1.elide_revealing_set(e1.digests(1)))
    assert e3.format() == (
        '"Alice" [\n'
        '    ELIDED (2)\n'
        ']'
    )

    e4 = check_encoding(e1.elide_revealing_set(e1.digests(2)))
    assert e4.format() == (
        '"Alice" [\n'
        '    ELIDED: ELIDED\n'
        '    ELIDED: ELIDED\n'
        ']'
    )

    e5 = check_encoding(e1.elide_revealing_set(e1.digests(3)))
    assert e5.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        ']'
    )


def test_target_reveal():
    e1 = double_assertion_envelope().add_assertion("livesAt", "123 Main St.")
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        '    "livesAt": "123 Main St."\n'
        ']'
    )

    target = set()
    # Reveal the Envelope structure
    target.update(e1.digests(1))
    # Reveal everything about the subject
    target.update(e1.subject().deep_digests())
    # Reveal everything about one of the assertions
    target.update(assertion_envelope().deep_digests())
    # Reveal the specific `livesAt` assertion
    target.update(e1.assertion_with_predicate("livesAt").deep_digests())

    e2 = check_encoding(e1.elide_revealing_set(target))
    assert e2.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "livesAt": "123 Main St."\n'
        '    ELIDED\n'
        ']'
    )


def test_targeted_remove():
    e1 = double_assertion_envelope().add_assertion("livesAt", "123 Main St.")
    assert e1.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        '    "livesAt": "123 Main St."\n'
        ']'
    )

    # Hide one of the assertions
    target2 = set()
    target2.update(assertion_envelope().digests(1))
    e2 = check_encoding(e1.elide_removing_set(target2))
    assert e2.format() == (
        '"Alice" [\n'
        '    "knows": "Carol"\n'
        '    "livesAt": "123 Main St."\n'
        '    ELIDED\n'
        ']'
    )

    # Hide one of the assertions by finding its predicate
    target3 = set()
    target3.update(e1.assertion_with_predicate("livesAt").deep_digests())
    e3 = check_encoding(e1.elide_removing_set(target3))
    assert e3.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        '    ELIDED\n'
        ']'
    )

    # Semantically equivalent
    assert e1.is_equivalent_to(e3)

    # Structurally different
    assert not e1.is_identical_to(e3)


def test_walk_replace_basic():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    charlie = Envelope("Charlie")

    envelope = alice.add_assertion("knows", bob).add_assertion("likes", bob)
    assert envelope.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "likes": "Bob"\n'
        ']'
    )

    # Replace all instances of Bob with Charlie
    target = {bob.digest()}
    modified = envelope.walk_replace(target, charlie)
    assert modified.format() == (
        '"Alice" [\n'
        '    "knows": "Charlie"\n'
        '    "likes": "Charlie"\n'
        ']'
    )

    assert not modified.is_equivalent_to(envelope)


def test_walk_replace_subject():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    carol = Envelope("Carol")

    envelope = alice.add_assertion("knows", bob)
    assert envelope.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        ']'
    )

    target = {alice.digest()}
    modified = envelope.walk_replace(target, carol)
    assert modified.format() == (
        '"Carol" [\n'
        '    "knows": "Bob"\n'
        ']'
    )


def test_walk_replace_nested():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    charlie = Envelope("Charlie")

    inner = bob.add_assertion("friend", bob)
    envelope = alice.add_assertion("knows", inner)
    assert envelope.format() == (
        '"Alice" [\n'
        '    "knows": "Bob" [\n'
        '        "friend": "Bob"\n'
        '    ]\n'
        ']'
    )

    target = {bob.digest()}
    modified = envelope.walk_replace(target, charlie)
    assert modified.format() == (
        '"Alice" [\n'
        '    "knows": "Charlie" [\n'
        '        "friend": "Charlie"\n'
        '    ]\n'
        ']'
    )


def test_walk_replace_wrapped():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    charlie = Envelope("Charlie")

    wrapped = bob.wrap()
    envelope = alice.add_assertion("data", wrapped)
    assert envelope.format() == (
        '"Alice" [\n'
        '    "data": {\n'
        '        "Bob"\n'
        '    }\n'
        ']'
    )

    target = {bob.digest()}
    modified = envelope.walk_replace(target, charlie)
    assert modified.format() == (
        '"Alice" [\n'
        '    "data": {\n'
        '        "Charlie"\n'
        '    }\n'
        ']'
    )


def test_walk_replace_no_match():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    charlie = Envelope("Charlie")
    dave = Envelope("Dave")

    envelope = alice.add_assertion("knows", bob)
    assert envelope.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        ']'
    )

    target = {dave.digest()}
    modified = envelope.walk_replace(target, charlie)
    assert modified.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        ']'
    )

    assert modified.is_identical_to(envelope)


def test_walk_replace_multiple_targets():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    carol = Envelope("Carol")
    replacement = Envelope("REDACTED")

    envelope = (
        alice
        .add_assertion("knows", bob)
        .add_assertion("likes", carol)
    )
    assert envelope.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "likes": "Carol"\n'
        ']'
    )

    target = {bob.digest(), carol.digest()}
    modified = envelope.walk_replace(target, replacement)
    assert modified.format() == (
        '"Alice" [\n'
        '    "knows": "REDACTED"\n'
        '    "likes": "REDACTED"\n'
        ']'
    )


def test_walk_replace_elided():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    charlie = Envelope("Charlie")

    envelope = alice.add_assertion("knows", bob).add_assertion("likes", bob)
    assert envelope.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "likes": "Bob"\n'
        ']'
    )

    # Elide Bob
    elided = envelope.elide_removing_target(bob)
    assert elided.format() == (
        '"Alice" [\n'
        '    "knows": ELIDED\n'
        '    "likes": ELIDED\n'
        ']'
    )

    # Replace the elided Bob with Charlie
    target = {bob.digest()}
    modified = elided.walk_replace(target, charlie)
    assert modified.format() == (
        '"Alice" [\n'
        '    "knows": "Charlie"\n'
        '    "likes": "Charlie"\n'
        ']'
    )

    assert not modified.is_equivalent_to(envelope)
    assert not modified.is_equivalent_to(elided)


def test_walk_replace_assertion_with_non_assertion_fails():
    alice = Envelope("Alice")
    bob = Envelope("Bob")
    charlie = Envelope("Charlie")

    envelope = alice.add_assertion("knows", bob)

    knows_assertion = envelope.assertion_with_predicate("knows")
    assertion_digest = knows_assertion.digest()

    target = {assertion_digest}

    try:
        envelope.walk_replace(target, charlie)
        assert False, "Expected InvalidFormat error"
    except InvalidFormat:
        pass
