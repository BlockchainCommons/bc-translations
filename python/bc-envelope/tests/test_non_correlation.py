"""Non-correlation (salt) tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/non_correlation_tests.rs
"""

import known_values
from bc_rand import make_fake_random_number_generator
from bc_envelope import Envelope

from tests.common.check_encoding import check_encoding


def test_envelope_non_correlation():
    """Adding salt prevents correlation between equivalent envelopes."""
    e1 = Envelope("Hello.")

    # e1 correlates with its elision
    assert e1.is_equivalent_to(e1.elide())

    # e2 is the same message, but with random salt
    rng = make_fake_random_number_generator()
    e2 = check_encoding(e1.add_salt_using(rng))

    expected_format = (
        '"Hello." [\n'
        "    'salt': Salt\n"
        "]"
    )
    assert e2.format() == expected_format

    expected_diagnostic = (
        "200(   / envelope /\n"
        "    [\n"
        '        201("Hello."),   / leaf /\n'
        "        {\n"
        "            15:\n"
        "            201(   / leaf /\n"
        "                40018(h'b559bbbf6cce2632')   / salt /\n"
        "            )\n"
        "        }\n"
        "    ]\n"
        ")"
    )
    assert e2.diagnostic_annotated() == expected_diagnostic

    expected_tree = (
        "4f0f2d55 NODE\n"
        '    8cc96cdb subj "Hello."\n'
        "    dd412f1d ASSERTION\n"
        "        618975ce pred 'salt'\n"
        "        7915f200 obj Salt"
    )
    assert e2.tree_format() == expected_tree

    # Same content but doesn't correlate
    assert not e1.is_equivalent_to(e2)

    # Neither does its elision
    assert not e1.is_equivalent_to(e2.elide())


def test_predicate_correlation():
    """Envelopes with same predicates correlate at the predicate level."""
    e1 = check_encoding(
        Envelope("Foo").add_assertion("note", "Bar")
    )
    e2 = check_encoding(
        Envelope("Baz").add_assertion("note", "Quux")
    )

    expected_format = (
        '"Foo" [\n'
        '    "note": "Bar"\n'
        "]"
    )
    assert e1.format() == expected_format

    # e1 and e2 have the same predicate
    assert (
        e1.assertions()[0].as_predicate()
        .is_equivalent_to(
            e2.assertions()[0].as_predicate()
        )
    )

    # Redact entire contents of e1 without redacting the envelope itself
    e1_elided = check_encoding(e1.elide_revealing_target(e1))

    redacted_expected_format = (
        "ELIDED [\n"
        "    ELIDED\n"
        "]"
    )
    assert e1_elided.format() == redacted_expected_format


def test_add_salt():
    """Add salt to predicates and objects to prevent correlation."""
    source = (
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do "
        "eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim "
        "ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut "
        "aliquip ex ea commodo consequat. Duis aute irure dolor in "
        "reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla "
        "pariatur. Excepteur sint occaecat cupidatat non proident, sunt in "
        "culpa qui officia deserunt mollit anim id est laborum."
    )
    e1 = check_encoding(
        check_encoding(
            Envelope("Alpha")
            .add_salt()
        )
        .wrap()
        .add_assertion(
            check_encoding(Envelope(known_values.NOTE).add_salt()),
            check_encoding(Envelope(source).add_salt()),
        )
    )

    expected_format = (
        "{\n"
        '    "Alpha" [\n'
        "        'salt': Salt\n"
        "    ]\n"
        "} [\n"
        "    'note' [\n"
        "        'salt': Salt\n"
        "    ]\n"
        '    : "Lorem ipsum dolor sit amet, consectetur adipiscing elit, '
        "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. "
        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris "
        "nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in "
        "reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla "
        "pariatur. Excepteur sint occaecat cupidatat non proident, sunt in "
        'culpa qui officia deserunt mollit anim id est laborum." [\n'
        "        'salt': Salt\n"
        "    ]\n"
        "]"
    )
    assert e1.format() == expected_format

    e1_elided = check_encoding(e1.elide_revealing_target(e1))

    redacted_expected_format = (
        "ELIDED [\n"
        "    ELIDED\n"
        "]"
    )
    assert e1_elided.format() == redacted_expected_format
