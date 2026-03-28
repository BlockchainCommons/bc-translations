"""Core envelope tests.

Translated from rust/bc-envelope/tests/core_tests.rs
"""

import dcbor
from bc_components import Digest
from bc_envelope import Envelope, extract_subject
from known_values import KnownValue, NOTE, UNIT

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    PLAINTEXT_HELLO,
    assertion_envelope,
    double_assertion_envelope,
    double_wrapped_envelope,
    hello_envelope,
    known_value_envelope,
    single_assertion_envelope,
    wrapped_envelope,
)


def test_read_legacy_leaf():
    """Envelopes encoded with old tag #6.24 should still decode correctly."""
    legacy_data = bytes.fromhex("d8c8d818182a")
    cbor = dcbor.CBOR.from_data(legacy_data)
    legacy_envelope = Envelope.from_tagged_cbor(cbor)
    e = Envelope(42)
    assert legacy_envelope.is_identical_to(e)
    assert legacy_envelope.is_equivalent_to(e)


def test_int_subject():
    e = check_encoding(Envelope(42))

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    201(42)   / leaf /\n"
        ")"
    )

    assert str(e.digest()) == \
        "Digest(7f83f7bda2d63959d34767689f06d47576683d378d9eb8d09386c9a020395c53)"

    assert e.format() == "42"

    assert extract_subject(e, int) == 42


def test_negative_int_subject():
    e = check_encoding(Envelope(-42))

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    201(-42)   / leaf /\n"
        ")"
    )

    assert str(e.digest()) == \
        "Digest(9e0ad272780de7aa1dbdfbc99058bb81152f623d3b95b5dfb0a036badfcc9055)"

    assert e.format() == "-42"

    assert extract_subject(e, int) == -42


def test_cbor_encodable_subject():
    e = check_encoding(hello_envelope())

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        '    201("Hello.")   / leaf /\n'
        ")"
    )

    assert str(e.digest()) == \
        "Digest(8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59)"

    assert e.format() == '"Hello."'

    assert extract_subject(e, str) == PLAINTEXT_HELLO


def test_known_value_subject():
    e = check_encoding(known_value_envelope())

    assert e.diagnostic_annotated() == \
        "200(4)   / envelope /"

    assert str(e.digest()) == \
        "Digest(0fcd6a39d6ed37f2e2efa6a96214596f1b28a5cd42a5a27afc32162aaf821191)"

    assert e.format() == "'note'"

    assert extract_subject(e, KnownValue) == NOTE


def test_assertion_subject():
    e = check_encoding(assertion_envelope())

    assert str(e.as_predicate().digest()) == \
        "Digest(db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba)"
    assert str(e.as_object().digest()) == \
        "Digest(13b741949c37b8e09cc3daa3194c58e4fd6b2f14d4b1d0f035a46d6d5a1d3f11)"
    assert str(e.subject().digest()) == \
        "Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)"
    assert str(e.digest()) == \
        "Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)"

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    {\n"
        '        201("knows"):   / leaf /\n'
        '        201("Bob")   / leaf /\n'
        "    }\n"
        ")"
    )

    assert e.format() == '"knows": "Bob"'

    assert e.digest() == Envelope.new_assertion("knows", "Bob").digest()


def test_subject_with_assertion():
    e = check_encoding(single_assertion_envelope())

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    [\n"
        '        201("Alice"),   / leaf /\n'
        "        {\n"
        '            201("knows"):   / leaf /\n'
        '            201("Bob")   / leaf /\n'
        "        }\n"
        "    ]\n"
        ")"
    )

    assert str(e.digest()) == \
        "Digest(8955db5e016affb133df56c11fe6c5c82fa3036263d651286d134c7e56c0e9f2)"

    assert e.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        "]"
    )

    assert extract_subject(e, str) == "Alice"


def test_subject_with_two_assertions():
    e = check_encoding(double_assertion_envelope())

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    [\n"
        '        201("Alice"),   / leaf /\n'
        "        {\n"
        '            201("knows"):   / leaf /\n'
        '            201("Carol")   / leaf /\n'
        "        },\n"
        "        {\n"
        '            201("knows"):   / leaf /\n'
        '            201("Bob")   / leaf /\n'
        "        }\n"
        "    ]\n"
        ")"
    )

    assert str(e.digest()) == \
        "Digest(b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471)"

    assert e.format() == (
        '"Alice" [\n'
        '    "knows": "Bob"\n'
        '    "knows": "Carol"\n'
        "]"
    )

    assert extract_subject(e, str) == "Alice"


def test_wrapped():
    e = check_encoding(wrapped_envelope())

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    200(   / envelope /\n"
        '        201("Hello.")   / leaf /\n'
        "    )\n"
        ")"
    )

    assert str(e.digest()) == \
        "Digest(172a5e51431062e7b13525cbceb8ad8475977444cf28423e21c0d1dcbdfcaf47)"

    assert e.format() == (
        "{\n"
        '    "Hello."\n'
        "}"
    )


def test_double_wrapped():
    e = check_encoding(double_wrapped_envelope())

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    200(   / envelope /\n"
        "        200(   / envelope /\n"
        '            201("Hello.")   / leaf /\n'
        "        )\n"
        "    )\n"
        ")"
    )

    assert str(e.digest()) == \
        "Digest(8b14f3bcd7c05aac8f2162e7047d7ef5d5eab7d82ee3f9dc4846c70bae4d200b)"

    assert e.format() == (
        "{\n"
        "    {\n"
        '        "Hello."\n'
        "    }\n"
        "}"
    )


def test_assertion_with_assertions():
    a = Envelope.new_assertion(1, 2) \
        .add_assertion(3, 4) \
        .add_assertion(5, 6)
    e = Envelope(7).add_assertion_envelope(a)

    assert e.format() == (
        "7 [\n"
        "    {\n"
        "        1: 2\n"
        "    } [\n"
        "        3: 4\n"
        "        5: 6\n"
        "    ]\n"
        "]"
    )


def test_digest_leaf():
    digest = hello_envelope().digest()
    e = check_encoding(Envelope(digest))

    assert e.format() == "Digest(8cc96cdb)"

    assert str(e.digest()) == \
        "Digest(07b518af92a6196bc153752aabefedb34ff8e1a7d820c01ef978dfc3e7e52e05)"

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    201(   / leaf /\n"
        "        40001(   / digest /\n"
        "            h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'\n"
        "        )\n"
        "    )\n"
        ")"
    )


def test_true():
    e = check_encoding(Envelope(True))
    assert e.is_bool()
    assert e.is_true()
    assert not e.is_false()
    assert e == Envelope.true_value()
    assert e.format() == "true"


def test_false():
    e = check_encoding(Envelope(False))
    assert e.is_bool()
    assert not e.is_true()
    assert e.is_false()
    assert e == Envelope.false_value()
    assert e.format() == "false"


def test_unit():
    e = check_encoding(Envelope.unit())
    assert e.is_subject_unit()
    assert e.format() == "''"

    e = e.add_assertion("foo", "bar")
    assert e.is_subject_unit()
    assert e.format() == (
        "'' [\n"
        '    "foo": "bar"\n'
        "]"
    )

    subject = extract_subject(e, KnownValue)
    assert subject == UNIT
