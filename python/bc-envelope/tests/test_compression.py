"""Compression tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/compression_tests.rs
"""

import known_values
from bc_envelope import Envelope

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    alice_private_keys,
)

SOURCE = (
    "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh "
    "ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula "
    "nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis "
    "imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit "
    "ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque "
    "iaculis blandit montes nunc pretium."
)


def test_compress():
    """Compress/decompress round-trip preserving digests."""
    original = Envelope(SOURCE)
    assert len(original.tagged_cbor().to_cbor_data()) == 371

    compressed = check_encoding(original.compress())
    assert len(compressed.tagged_cbor().to_cbor_data()) == 283

    assert original.digest() == compressed.digest()

    decompressed = check_encoding(compressed.decompress())
    assert decompressed.digest() == original.digest()
    assert decompressed.structural_digest() == original.structural_digest()


def test_compress_subject():
    """Compress subject of a signed envelope, preserving signature."""
    from bc_rand import make_fake_random_number_generator
    from bc_components import SchnorrSigningOptions

    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng)
    original = (
        Envelope("Alice")
        .add_assertion(known_values.NOTE, SOURCE)
        .wrap()
        .add_signature_opt(alice_private_keys(), options, None)
    )
    assert len(original.tagged_cbor().to_cbor_data()) == 458

    s = original.tree_format()
    expected_tree = (
        "ec608f27 NODE\n"
        "    d7183f04 subj WRAPPED\n"
        "        7f35e345 cont NODE\n"
        '            13941b48 subj "Alice"\n'
        "            9fb69539 ASSERTION\n"
        "                0fcd6a39 pred 'note'\n"
        '                e343c9b4 obj "Lorem ipsum dolor sit amet consectetur a\u2026"\n'
        "    0db2ee20 ASSERTION\n"
        "        d0e39e78 pred 'signed'\n"
        "        f0d3ce4c obj Signature"
    )
    assert s == expected_tree

    compressed = check_encoding(original.compress_subject())
    assert len(compressed.tagged_cbor().to_cbor_data()) == 374

    s = compressed.tree_format()
    expected_compressed_tree = (
        "ec608f27 NODE\n"
        "    d7183f04 subj COMPRESSED\n"
        "    0db2ee20 ASSERTION\n"
        "        d0e39e78 pred 'signed'\n"
        "        f0d3ce4c obj Signature"
    )
    assert s == expected_compressed_tree

    s = compressed.mermaid_format()
    expected_mermaid = (
        "%%{ init: { 'theme': 'default', 'flowchart': { 'curve': 'basis' } } }%%\n"
        "graph LR\n"
        '0(("NODE<br>ec608f27"))\n'
        '    0 -- subj --> 1[["COMPRESSED<br>d7183f04"]]\n'
        '    0 --> 2(["ASSERTION<br>0db2ee20"])\n'
        """        2 -- pred --> 3[/"'signed'<br>d0e39e78"/]\n"""
        '        2 -- obj --> 4["Signature<br>f0d3ce4c"]\n'
        "style 0 stroke:red,stroke-width:4px\n"
        "style 1 stroke:purple,stroke-width:4px\n"
        "style 2 stroke:green,stroke-width:4px\n"
        "style 3 stroke:goldenrod,stroke-width:4px\n"
        "style 4 stroke:teal,stroke-width:4px\n"
        "linkStyle 0 stroke:red,stroke-width:2px\n"
        "linkStyle 1 stroke-width:2px\n"
        "linkStyle 2 stroke:cyan,stroke-width:2px\n"
        "linkStyle 3 stroke:magenta,stroke-width:2px"
    )
    assert s == expected_mermaid

    decompressed = check_encoding(compressed.decompress_subject())
    assert decompressed.digest() == original.digest()
    assert decompressed.structural_digest() == original.structural_digest()
