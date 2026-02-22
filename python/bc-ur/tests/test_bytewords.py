"""Tests for bytewords encoding and decoding."""

from bc_ur.bytewords import BytewordsStyle, decode, encode
from bc_ur._bytewords_constants import BYTEMOJIS
from bc_ur.error import BytewordsError

import pytest


def test_bytewords():
    input_data = bytes([0, 1, 2, 128, 255])

    # Encode
    assert encode(input_data, BytewordsStyle.STANDARD) == \
        "able acid also lava zoom jade need echo taxi"
    assert encode(input_data, BytewordsStyle.URI) == \
        "able-acid-also-lava-zoom-jade-need-echo-taxi"
    assert encode(input_data, BytewordsStyle.MINIMAL) == \
        "aeadaolazmjendeoti"

    # Decode roundtrip
    assert decode(
        "able acid also lava zoom jade need echo taxi",
        BytewordsStyle.STANDARD,
    ) == input_data
    assert decode(
        "able-acid-also-lava-zoom-jade-need-echo-taxi",
        BytewordsStyle.URI,
    ) == input_data
    assert decode("aeadaolazmjendeoti", BytewordsStyle.MINIMAL) == input_data

    # Empty payload roundtrip
    empty_encoded = encode(b"", BytewordsStyle.MINIMAL)
    decode(empty_encoded, BytewordsStyle.MINIMAL)

    # Bad checksum errors
    with pytest.raises(BytewordsError, match="invalid checksum"):
        decode(
            "able acid also lava zero jade need echo wolf",
            BytewordsStyle.STANDARD,
        )
    with pytest.raises(BytewordsError, match="invalid checksum"):
        decode(
            "able-acid-also-lava-zero-jade-need-echo-wolf",
            BytewordsStyle.URI,
        )
    with pytest.raises(BytewordsError, match="invalid checksum"):
        decode("aeadaolazojendeowf", BytewordsStyle.MINIMAL)

    # Too short
    with pytest.raises(BytewordsError, match="invalid checksum"):
        decode("wolf", BytewordsStyle.STANDARD)

    # Empty standard
    with pytest.raises(BytewordsError, match="invalid word"):
        decode("", BytewordsStyle.STANDARD)

    # Invalid length for minimal
    with pytest.raises(BytewordsError, match="invalid length"):
        decode("aea", BytewordsStyle.MINIMAL)

    # Non-ASCII errors
    with pytest.raises(BytewordsError, match="non-ASCII"):
        decode("\u20bf", BytewordsStyle.STANDARD)
    with pytest.raises(BytewordsError, match="non-ASCII"):
        decode("\u20bf", BytewordsStyle.URI)
    with pytest.raises(BytewordsError, match="non-ASCII"):
        decode("\u20bf", BytewordsStyle.MINIMAL)


def test_encoding_100_bytes():
    input_data = bytes([
        245, 215, 20, 198, 241, 235, 69, 59, 209, 205,
        165, 18, 150, 158, 116, 135, 229, 212, 19, 159,
        17, 37, 239, 240, 253, 11, 109, 191, 37, 242,
        38, 120, 223, 41, 156, 189, 242, 254, 147, 204,
        66, 163, 216, 175, 191, 72, 169, 54, 32, 60,
        144, 230, 210, 137, 184, 197, 33, 113, 88, 14,
        157, 31, 177, 46, 1, 115, 205, 69, 225, 150,
        65, 235, 58, 144, 65, 240, 133, 69, 113, 247,
        63, 53, 242, 165, 160, 144, 26, 13, 79, 237,
        133, 71, 82, 69, 254, 165, 138, 41, 85, 24,
    ])

    expected_standard = (
        "yank toys bulb skew when warm free fair tent swan "
        "open brag mint noon jury list view tiny brew note "
        "body data webs what zinc bald join runs data whiz "
        "days keys user diet news ruby whiz zone menu surf "
        "flew omit trip pose runs fund part even crux fern "
        "math visa tied loud redo silk curl jugs hard beta "
        "next cost puma drum acid junk swan free very mint "
        "flap warm fact math flap what limp free jugs yell "
        "fish epic whiz open numb math city belt glow wave "
        "limp fuel grim free zone open love diet gyro cats "
        "fizz holy city puff"
    )

    expected_minimal = (
        "yktsbbswwnwmfefrttsnonbgmtnnjyltvwtybwne"
        "bydawswtzcbdjnrsdawzdsksurdtnsrywzzemusf"
        "fwottppersfdptencxfnmhvatdldroskcljshdba"
        "ntctpadmadjksnfevymtfpwmftmhfpwtlpfejsyl"
        "fhecwzonnbmhcybtgwwelpflgmfezeonledtgocs"
        "fzhycypf"
    )

    assert decode(expected_standard, BytewordsStyle.STANDARD) == input_data
    assert decode(expected_minimal, BytewordsStyle.MINIMAL) == input_data
    assert encode(input_data, BytewordsStyle.STANDARD) == expected_standard
    assert encode(input_data, BytewordsStyle.MINIMAL) == expected_minimal


def test_bytemoji_uniqueness():
    seen = {}
    for emoji in BYTEMOJIS:
        count = seen.get(emoji, 0) + 1
        seen[emoji] = count
    duplicates = {k: v for k, v in seen.items() if v > 1}
    assert not duplicates, f"Duplicates: {duplicates}"


def test_bytemoji_lengths():
    over_length = [(emoji, len(emoji.encode("utf-8")))
                   for emoji in BYTEMOJIS
                   if len(emoji.encode("utf-8")) > 4]
    assert not over_length, f"Some bytemojis are over 4 bytes: {over_length}"
