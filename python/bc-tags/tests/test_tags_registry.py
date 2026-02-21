"""Tests for bc_tags constants and registry behavior."""

from __future__ import annotations

from dcbor import TAG_DATE, TagsStore, with_tags

from bc_tags import (
    CBOR,
    TAG_ADDRESS,
    TAG_ENVELOPE,
    TAG_NAME_ADDRESS,
    TAG_NAME_ENVELOPE,
    TAG_NAME_URI,
    TAG_URI,
    register_tags,
    register_tags_in,
)
import bc_tags.tags_registry as tags_registry

EXPECTED_TAGS = [
    ("URI", 32, "url"),
    ("UUID", 37, "uuid"),
    ("ENCODED_CBOR", 24, "encoded-cbor"),
    ("ENVELOPE", 200, "envelope"),
    ("LEAF", 201, "leaf"),
    ("JSON", 262, "json"),
    ("KNOWN_VALUE", 40000, "known-value"),
    ("DIGEST", 40001, "digest"),
    ("ENCRYPTED", 40002, "encrypted"),
    ("COMPRESSED", 40003, "compressed"),
    ("REQUEST", 40004, "request"),
    ("RESPONSE", 40005, "response"),
    ("FUNCTION", 40006, "function"),
    ("PARAMETER", 40007, "parameter"),
    ("PLACEHOLDER", 40008, "placeholder"),
    ("REPLACEMENT", 40009, "replacement"),
    ("X25519_PRIVATE_KEY", 40010, "agreement-private-key"),
    ("X25519_PUBLIC_KEY", 40011, "agreement-public-key"),
    ("ARID", 40012, "arid"),
    ("PRIVATE_KEYS", 40013, "crypto-prvkeys"),
    ("NONCE", 40014, "nonce"),
    ("PASSWORD", 40015, "password"),
    ("PRIVATE_KEY_BASE", 40016, "crypto-prvkey-base"),
    ("PUBLIC_KEYS", 40017, "crypto-pubkeys"),
    ("SALT", 40018, "salt"),
    ("SEALED_MESSAGE", 40019, "crypto-sealed"),
    ("SIGNATURE", 40020, "signature"),
    ("SIGNING_PRIVATE_KEY", 40021, "signing-private-key"),
    ("SIGNING_PUBLIC_KEY", 40022, "signing-public-key"),
    ("SYMMETRIC_KEY", 40023, "crypto-key"),
    ("XID", 40024, "xid"),
    ("REFERENCE", 40025, "reference"),
    ("EVENT", 40026, "event"),
    ("ENCRYPTED_KEY", 40027, "encrypted-key"),
    ("MLKEM_PRIVATE_KEY", 40100, "mlkem-private-key"),
    ("MLKEM_PUBLIC_KEY", 40101, "mlkem-public-key"),
    ("MLKEM_CIPHERTEXT", 40102, "mlkem-ciphertext"),
    ("MLDSA_PRIVATE_KEY", 40103, "mldsa-private-key"),
    ("MLDSA_PUBLIC_KEY", 40104, "mldsa-public-key"),
    ("MLDSA_SIGNATURE", 40105, "mldsa-signature"),
    ("SEED", 40300, "seed"),
    ("HDKEY", 40303, "hdkey"),
    ("DERIVATION_PATH", 40304, "keypath"),
    ("USE_INFO", 40305, "coin-info"),
    ("EC_KEY", 40306, "eckey"),
    ("ADDRESS", 40307, "address"),
    ("OUTPUT_DESCRIPTOR", 40308, "output-descriptor"),
    ("SSKR_SHARE", 40309, "sskr"),
    ("PSBT", 40310, "psbt"),
    ("ACCOUNT_DESCRIPTOR", 40311, "account-descriptor"),
    ("SSH_TEXT_PRIVATE_KEY", 40800, "ssh-private"),
    ("SSH_TEXT_PUBLIC_KEY", 40801, "ssh-public"),
    ("SSH_TEXT_SIGNATURE", 40802, "ssh-signature"),
    ("SSH_TEXT_CERTIFICATE", 40803, "ssh-certificate"),
    ("PROVENANCE_MARK", 1347571542, "provenance"),
    ("OUTPUT_SCRIPT_HASH", 400, "output-script-hash"),
    ("OUTPUT_WITNESS_SCRIPT_HASH", 401, "output-witness-script-hash"),
    ("OUTPUT_PUBLIC_KEY", 402, "output-public-key"),
    ("OUTPUT_PUBLIC_KEY_HASH", 403, "output-public-key-hash"),
    ("OUTPUT_WITNESS_PUBLIC_KEY_HASH", 404, "output-witness-public-key-hash"),
    ("OUTPUT_COMBO", 405, "output-combo"),
    ("OUTPUT_MULTISIG", 406, "output-multisig"),
    ("OUTPUT_SORTED_MULTISIG", 407, "output-sorted-multisig"),
    ("OUTPUT_RAW_SCRIPT", 408, "output-raw-script"),
    ("OUTPUT_TAPROOT", 409, "output-taproot"),
    ("OUTPUT_COSIGNER", 410, "output-cosigner"),
]


def test_public_reexport_from_dcbor_available() -> None:
    assert CBOR.from_int(42).hex() == "182a"


def test_constants_match_expected_values() -> None:
    assert len(EXPECTED_TAGS) == 66

    for suffix, expected_value, expected_name in EXPECTED_TAGS:
        assert getattr(tags_registry, f"TAG_{suffix}") == expected_value
        assert getattr(tags_registry, f"TAG_NAME_{suffix}") == expected_name
        assert f"TAG_{suffix}" in tags_registry.__all__
        assert f"TAG_NAME_{suffix}" in tags_registry.__all__


def test_register_tags_in_registers_dcbor_and_bc_tags() -> None:
    store = TagsStore()
    register_tags_in(store)

    assert store.name_for_value(TAG_DATE) == "date"

    for _, value, name in EXPECTED_TAGS:
        assert store.name_for_value(value) == name
        by_name = store.tag_for_name(name)
        assert by_name is not None
        assert by_name.value == value


def test_register_tags_in_is_idempotent() -> None:
    store = TagsStore()

    register_tags_in(store)
    register_tags_in(store)

    assert store.name_for_value(TAG_URI) == TAG_NAME_URI
    assert store.name_for_value(TAG_ENVELOPE) == TAG_NAME_ENVELOPE
    assert store.name_for_value(TAG_ADDRESS) == TAG_NAME_ADDRESS


def test_register_tags_updates_global_store() -> None:
    register_tags()

    def check(store: TagsStore) -> None:
        assert store.name_for_value(TAG_DATE) == "date"
        assert store.name_for_value(TAG_URI) == TAG_NAME_URI

    with_tags(check)
