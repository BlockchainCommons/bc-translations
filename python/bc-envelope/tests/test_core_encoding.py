"""Core encoding tests.

Translated from rust/bc-envelope/tests/core_encoding_tests.rs
"""

from bc_components import Digest
from bc_envelope import Envelope
from dcbor import CBOR

from tests.common.check_encoding import check_encoding


def test_digest():
    check_encoding(Envelope(Digest.from_image(b"Hello.")))


def test_1():
    e = Envelope("Hello.")

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        '    201("Hello.")   / leaf /\n'
        ")"
    )


def test_2():
    array = CBOR.from_array([CBOR.from_value(1), CBOR.from_value(2), CBOR.from_value(3)])
    e = Envelope(array)

    assert e.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    201(   / leaf /\n"
        "        [1, 2, 3]\n"
        "    )\n"
        ")"
    )


def test_3():
    e1 = check_encoding(Envelope.new_assertion("A", "B"))
    e2 = check_encoding(Envelope.new_assertion("C", "D"))
    e3 = check_encoding(Envelope.new_assertion("E", "F"))

    e4 = e2.add_assertion_envelope(e3)
    assert e4.format() == (
        "{\n"
        '    "C": "D"\n'
        "} [\n"
        '    "E": "F"\n'
        "]"
    )

    assert e4.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    [\n"
        "        {\n"
        '            201("C"):   / leaf /\n'
        '            201("D")   / leaf /\n'
        "        },\n"
        "        {\n"
        '            201("E"):   / leaf /\n'
        '            201("F")   / leaf /\n'
        "        }\n"
        "    ]\n"
        ")"
    )

    check_encoding(e4)

    e5 = check_encoding(e1.add_assertion_envelope(e4))

    assert e5.format() == (
        "{\n"
        '    "A": "B"\n'
        "} [\n"
        "    {\n"
        '        "C": "D"\n'
        "    } [\n"
        '        "E": "F"\n'
        "    ]\n"
        "]"
    )

    assert e5.diagnostic_annotated() == (
        "200(   / envelope /\n"
        "    [\n"
        "        {\n"
        '            201("A"):   / leaf /\n'
        '            201("B")   / leaf /\n'
        "        },\n"
        "        [\n"
        "            {\n"
        '                201("C"):   / leaf /\n'
        '                201("D")   / leaf /\n'
        "            },\n"
        "            {\n"
        '                201("E"):   / leaf /\n'
        '                201("F")   / leaf /\n'
        "            }\n"
        "        ]\n"
        "    ]\n"
        ")"
    )
