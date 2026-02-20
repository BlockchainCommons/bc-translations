"""Tests translated from Rust hash.rs."""

from bc_crypto.hash import (
    crc32,
    crc32_data,
    crc32_data_opt,
    hkdf_hmac_sha256,
    hmac_sha256,
    hmac_sha512,
    pbkdf2_hmac_sha256,
    sha256,
    sha512,
)


def test_crc32() -> None:
    input_data = b"Hello, world!"
    assert crc32(input_data) == 0xEBE6C6E6
    assert crc32_data(input_data) == bytes.fromhex("ebe6c6e6")
    assert crc32_data_opt(input_data, True) == bytes.fromhex("e6c6e6eb")


def test_sha256() -> None:
    input_data = b"abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
    expected = bytes.fromhex(
        "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1"
    )
    assert sha256(input_data) == expected


def test_sha512() -> None:
    input_data = b"abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
    expected = bytes.fromhex(
        "204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c335"
        "96fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445"
    )
    assert sha512(input_data) == expected


def test_hmac_sha() -> None:
    key = bytes.fromhex("0b" * 20)
    message = b"Hi There"
    assert hmac_sha256(key, message) == bytes.fromhex(
        "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7"
    )
    assert hmac_sha512(key, message) == bytes.fromhex(
        "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cde"
        "daa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854"
    )


def test_pbkdf2_hmac_sha256() -> None:
    assert pbkdf2_hmac_sha256("password", "salt", 1, 32) == bytes.fromhex(
        "120fb6cffcf8b32c43e7225256c4f837a86548c92ccc35480805987cb70be17b"
    )


def test_hkdf_hmac_sha256() -> None:
    key_material = b"hello"
    salt = bytes.fromhex("8e94ef805b93e683ff18")
    assert hkdf_hmac_sha256(key_material, salt, 32) == bytes.fromhex(
        "13485067e21af17c0900f70d885f02593c0e61e46f86450e4a0201a54c14db76"
    )
