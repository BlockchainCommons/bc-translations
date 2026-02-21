import { describe, test, expect } from "vitest";
import {
  TagsStore,
  getGlobalTagsStore,
  TAG_DATE,
  TAG_NAME_DATE,
  TAG_POSITIVE_BIGNUM,
  TAG_NAME_POSITIVE_BIGNUM,
  TAG_NEGATIVE_BIGNUM,
  TAG_NAME_NEGATIVE_BIGNUM,
} from "@bc/dcbor";
import {
  TAG_URI,
  TAG_NAME_URI,
  TAG_UUID,
  TAG_NAME_UUID,
  TAG_ENCODED_CBOR,
  TAG_NAME_ENCODED_CBOR,
  TAG_ENVELOPE,
  TAG_NAME_ENVELOPE,
  TAG_LEAF,
  TAG_NAME_LEAF,
  TAG_JSON,
  TAG_NAME_JSON,
  TAG_KNOWN_VALUE,
  TAG_NAME_KNOWN_VALUE,
  TAG_DIGEST,
  TAG_NAME_DIGEST,
  TAG_ENCRYPTED,
  TAG_NAME_ENCRYPTED,
  TAG_COMPRESSED,
  TAG_NAME_COMPRESSED,
  TAG_REQUEST,
  TAG_NAME_REQUEST,
  TAG_RESPONSE,
  TAG_NAME_RESPONSE,
  TAG_FUNCTION,
  TAG_NAME_FUNCTION,
  TAG_PARAMETER,
  TAG_NAME_PARAMETER,
  TAG_PLACEHOLDER,
  TAG_NAME_PLACEHOLDER,
  TAG_REPLACEMENT,
  TAG_NAME_REPLACEMENT,
  TAG_X25519_PRIVATE_KEY,
  TAG_NAME_X25519_PRIVATE_KEY,
  TAG_X25519_PUBLIC_KEY,
  TAG_NAME_X25519_PUBLIC_KEY,
  TAG_ARID,
  TAG_NAME_ARID,
  TAG_PRIVATE_KEYS,
  TAG_NAME_PRIVATE_KEYS,
  TAG_NONCE,
  TAG_NAME_NONCE,
  TAG_PASSWORD,
  TAG_NAME_PASSWORD,
  TAG_PRIVATE_KEY_BASE,
  TAG_NAME_PRIVATE_KEY_BASE,
  TAG_PUBLIC_KEYS,
  TAG_NAME_PUBLIC_KEYS,
  TAG_SALT,
  TAG_NAME_SALT,
  TAG_SEALED_MESSAGE,
  TAG_NAME_SEALED_MESSAGE,
  TAG_SIGNATURE,
  TAG_NAME_SIGNATURE,
  TAG_SIGNING_PRIVATE_KEY,
  TAG_NAME_SIGNING_PRIVATE_KEY,
  TAG_SIGNING_PUBLIC_KEY,
  TAG_NAME_SIGNING_PUBLIC_KEY,
  TAG_SYMMETRIC_KEY,
  TAG_NAME_SYMMETRIC_KEY,
  TAG_XID,
  TAG_NAME_XID,
  TAG_REFERENCE,
  TAG_NAME_REFERENCE,
  TAG_EVENT,
  TAG_NAME_EVENT,
  TAG_ENCRYPTED_KEY,
  TAG_NAME_ENCRYPTED_KEY,
  TAG_MLKEM_PRIVATE_KEY,
  TAG_NAME_MLKEM_PRIVATE_KEY,
  TAG_MLKEM_PUBLIC_KEY,
  TAG_NAME_MLKEM_PUBLIC_KEY,
  TAG_MLKEM_CIPHERTEXT,
  TAG_NAME_MLKEM_CIPHERTEXT,
  TAG_MLDSA_PRIVATE_KEY,
  TAG_NAME_MLDSA_PRIVATE_KEY,
  TAG_MLDSA_PUBLIC_KEY,
  TAG_NAME_MLDSA_PUBLIC_KEY,
  TAG_MLDSA_SIGNATURE,
  TAG_NAME_MLDSA_SIGNATURE,
  TAG_SEED,
  TAG_NAME_SEED,
  TAG_HDKEY,
  TAG_NAME_HDKEY,
  TAG_DERIVATION_PATH,
  TAG_NAME_DERIVATION_PATH,
  TAG_USE_INFO,
  TAG_NAME_USE_INFO,
  TAG_EC_KEY,
  TAG_NAME_EC_KEY,
  TAG_ADDRESS,
  TAG_NAME_ADDRESS,
  TAG_OUTPUT_DESCRIPTOR,
  TAG_NAME_OUTPUT_DESCRIPTOR,
  TAG_SSKR_SHARE,
  TAG_NAME_SSKR_SHARE,
  TAG_PSBT,
  TAG_NAME_PSBT,
  TAG_ACCOUNT_DESCRIPTOR,
  TAG_NAME_ACCOUNT_DESCRIPTOR,
  TAG_SSH_TEXT_PRIVATE_KEY,
  TAG_NAME_SSH_TEXT_PRIVATE_KEY,
  TAG_SSH_TEXT_PUBLIC_KEY,
  TAG_NAME_SSH_TEXT_PUBLIC_KEY,
  TAG_SSH_TEXT_SIGNATURE,
  TAG_NAME_SSH_TEXT_SIGNATURE,
  TAG_SSH_TEXT_CERTIFICATE,
  TAG_NAME_SSH_TEXT_CERTIFICATE,
  TAG_PROVENANCE_MARK,
  TAG_NAME_PROVENANCE_MARK,
  TAG_SEED_V1,
  TAG_NAME_SEED_V1,
  TAG_EC_KEY_V1,
  TAG_NAME_EC_KEY_V1,
  TAG_SSKR_SHARE_V1,
  TAG_NAME_SSKR_SHARE_V1,
  TAG_HDKEY_V1,
  TAG_NAME_HDKEY_V1,
  TAG_DERIVATION_PATH_V1,
  TAG_NAME_DERIVATION_PATH_V1,
  TAG_USE_INFO_V1,
  TAG_NAME_USE_INFO_V1,
  TAG_OUTPUT_DESCRIPTOR_V1,
  TAG_NAME_OUTPUT_DESCRIPTOR_V1,
  TAG_PSBT_V1,
  TAG_NAME_PSBT_V1,
  TAG_ACCOUNT_V1,
  TAG_NAME_ACCOUNT_V1,
  TAG_OUTPUT_SCRIPT_HASH,
  TAG_NAME_OUTPUT_SCRIPT_HASH,
  TAG_OUTPUT_WITNESS_SCRIPT_HASH,
  TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH,
  TAG_OUTPUT_PUBLIC_KEY,
  TAG_NAME_OUTPUT_PUBLIC_KEY,
  TAG_OUTPUT_PUBLIC_KEY_HASH,
  TAG_NAME_OUTPUT_PUBLIC_KEY_HASH,
  TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH,
  TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH,
  TAG_OUTPUT_COMBO,
  TAG_NAME_OUTPUT_COMBO,
  TAG_OUTPUT_MULTISIG,
  TAG_NAME_OUTPUT_MULTISIG,
  TAG_OUTPUT_SORTED_MULTISIG,
  TAG_NAME_OUTPUT_SORTED_MULTISIG,
  TAG_OUTPUT_RAW_SCRIPT,
  TAG_NAME_OUTPUT_RAW_SCRIPT,
  TAG_OUTPUT_TAPROOT,
  TAG_NAME_OUTPUT_TAPROOT,
  TAG_OUTPUT_COSIGNER,
  TAG_NAME_OUTPUT_COSIGNER,
  bcTags,
  registerTagsIn,
  registerTags,
} from "../src/index.js";

/**
 * All 75 expected tags in registration order as [value, name] pairs.
 * Mirrors the Go `expectedTags` slice and the Rust registration vector.
 */
const expectedTags: [number, string][] = [
  [TAG_URI, TAG_NAME_URI],
  [TAG_UUID, TAG_NAME_UUID],
  [TAG_ENCODED_CBOR, TAG_NAME_ENCODED_CBOR],
  [TAG_ENVELOPE, TAG_NAME_ENVELOPE],
  [TAG_LEAF, TAG_NAME_LEAF],
  [TAG_JSON, TAG_NAME_JSON],
  [TAG_KNOWN_VALUE, TAG_NAME_KNOWN_VALUE],
  [TAG_DIGEST, TAG_NAME_DIGEST],
  [TAG_ENCRYPTED, TAG_NAME_ENCRYPTED],
  [TAG_COMPRESSED, TAG_NAME_COMPRESSED],
  [TAG_REQUEST, TAG_NAME_REQUEST],
  [TAG_RESPONSE, TAG_NAME_RESPONSE],
  [TAG_FUNCTION, TAG_NAME_FUNCTION],
  [TAG_PARAMETER, TAG_NAME_PARAMETER],
  [TAG_PLACEHOLDER, TAG_NAME_PLACEHOLDER],
  [TAG_REPLACEMENT, TAG_NAME_REPLACEMENT],
  [TAG_EVENT, TAG_NAME_EVENT],
  [TAG_SEED_V1, TAG_NAME_SEED_V1],
  [TAG_EC_KEY_V1, TAG_NAME_EC_KEY_V1],
  [TAG_SSKR_SHARE_V1, TAG_NAME_SSKR_SHARE_V1],
  [TAG_SEED, TAG_NAME_SEED],
  [TAG_EC_KEY, TAG_NAME_EC_KEY],
  [TAG_SSKR_SHARE, TAG_NAME_SSKR_SHARE],
  [TAG_X25519_PRIVATE_KEY, TAG_NAME_X25519_PRIVATE_KEY],
  [TAG_X25519_PUBLIC_KEY, TAG_NAME_X25519_PUBLIC_KEY],
  [TAG_ARID, TAG_NAME_ARID],
  [TAG_PRIVATE_KEYS, TAG_NAME_PRIVATE_KEYS],
  [TAG_NONCE, TAG_NAME_NONCE],
  [TAG_PASSWORD, TAG_NAME_PASSWORD],
  [TAG_PRIVATE_KEY_BASE, TAG_NAME_PRIVATE_KEY_BASE],
  [TAG_PUBLIC_KEYS, TAG_NAME_PUBLIC_KEYS],
  [TAG_SALT, TAG_NAME_SALT],
  [TAG_SEALED_MESSAGE, TAG_NAME_SEALED_MESSAGE],
  [TAG_SIGNATURE, TAG_NAME_SIGNATURE],
  [TAG_SIGNING_PRIVATE_KEY, TAG_NAME_SIGNING_PRIVATE_KEY],
  [TAG_SIGNING_PUBLIC_KEY, TAG_NAME_SIGNING_PUBLIC_KEY],
  [TAG_SYMMETRIC_KEY, TAG_NAME_SYMMETRIC_KEY],
  [TAG_XID, TAG_NAME_XID],
  [TAG_REFERENCE, TAG_NAME_REFERENCE],
  [TAG_ENCRYPTED_KEY, TAG_NAME_ENCRYPTED_KEY],
  [TAG_MLKEM_PRIVATE_KEY, TAG_NAME_MLKEM_PRIVATE_KEY],
  [TAG_MLKEM_PUBLIC_KEY, TAG_NAME_MLKEM_PUBLIC_KEY],
  [TAG_MLKEM_CIPHERTEXT, TAG_NAME_MLKEM_CIPHERTEXT],
  [TAG_MLDSA_PRIVATE_KEY, TAG_NAME_MLDSA_PRIVATE_KEY],
  [TAG_MLDSA_PUBLIC_KEY, TAG_NAME_MLDSA_PUBLIC_KEY],
  [TAG_MLDSA_SIGNATURE, TAG_NAME_MLDSA_SIGNATURE],
  [TAG_HDKEY_V1, TAG_NAME_HDKEY_V1],
  [TAG_DERIVATION_PATH_V1, TAG_NAME_DERIVATION_PATH_V1],
  [TAG_USE_INFO_V1, TAG_NAME_USE_INFO_V1],
  [TAG_OUTPUT_DESCRIPTOR_V1, TAG_NAME_OUTPUT_DESCRIPTOR_V1],
  [TAG_PSBT_V1, TAG_NAME_PSBT_V1],
  [TAG_ACCOUNT_V1, TAG_NAME_ACCOUNT_V1],
  [TAG_HDKEY, TAG_NAME_HDKEY],
  [TAG_DERIVATION_PATH, TAG_NAME_DERIVATION_PATH],
  [TAG_USE_INFO, TAG_NAME_USE_INFO],
  [TAG_ADDRESS, TAG_NAME_ADDRESS],
  [TAG_OUTPUT_DESCRIPTOR, TAG_NAME_OUTPUT_DESCRIPTOR],
  [TAG_PSBT, TAG_NAME_PSBT],
  [TAG_ACCOUNT_DESCRIPTOR, TAG_NAME_ACCOUNT_DESCRIPTOR],
  [TAG_SSH_TEXT_PRIVATE_KEY, TAG_NAME_SSH_TEXT_PRIVATE_KEY],
  [TAG_SSH_TEXT_PUBLIC_KEY, TAG_NAME_SSH_TEXT_PUBLIC_KEY],
  [TAG_SSH_TEXT_SIGNATURE, TAG_NAME_SSH_TEXT_SIGNATURE],
  [TAG_SSH_TEXT_CERTIFICATE, TAG_NAME_SSH_TEXT_CERTIFICATE],
  [TAG_OUTPUT_SCRIPT_HASH, TAG_NAME_OUTPUT_SCRIPT_HASH],
  [TAG_OUTPUT_WITNESS_SCRIPT_HASH, TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH],
  [TAG_OUTPUT_PUBLIC_KEY, TAG_NAME_OUTPUT_PUBLIC_KEY],
  [TAG_OUTPUT_PUBLIC_KEY_HASH, TAG_NAME_OUTPUT_PUBLIC_KEY_HASH],
  [TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH, TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH],
  [TAG_OUTPUT_COMBO, TAG_NAME_OUTPUT_COMBO],
  [TAG_OUTPUT_MULTISIG, TAG_NAME_OUTPUT_MULTISIG],
  [TAG_OUTPUT_SORTED_MULTISIG, TAG_NAME_OUTPUT_SORTED_MULTISIG],
  [TAG_OUTPUT_RAW_SCRIPT, TAG_NAME_OUTPUT_RAW_SCRIPT],
  [TAG_OUTPUT_TAPROOT, TAG_NAME_OUTPUT_TAPROOT],
  [TAG_OUTPUT_COSIGNER, TAG_NAME_OUTPUT_COSIGNER],
  [TAG_PROVENANCE_MARK, TAG_NAME_PROVENANCE_MARK],
];

describe("constant values", () => {
  test("spot-check representative constant values and names", () => {
    expect(TAG_URI).toBe(32);
    expect(TAG_NAME_URI).toBe("url");
    expect(TAG_UUID).toBe(37);
    expect(TAG_NAME_UUID).toBe("uuid");
    expect(TAG_ENVELOPE).toBe(200);
    expect(TAG_NAME_ENVELOPE).toBe("envelope");
    expect(TAG_KNOWN_VALUE).toBe(40000);
    expect(TAG_NAME_KNOWN_VALUE).toBe("known-value");
    expect(TAG_REQUEST).toBe(40004);
    expect(TAG_NAME_REQUEST).toBe("request");
    expect(TAG_X25519_PRIVATE_KEY).toBe(40010);
    expect(TAG_NAME_X25519_PRIVATE_KEY).toBe("agreement-private-key");
    expect(TAG_MLKEM_PRIVATE_KEY).toBe(40100);
    expect(TAG_NAME_MLKEM_PRIVATE_KEY).toBe("mlkem-private-key");
    expect(TAG_SEED).toBe(40300);
    expect(TAG_NAME_SEED).toBe("seed");
    expect(TAG_SSH_TEXT_PRIVATE_KEY).toBe(40800);
    expect(TAG_NAME_SSH_TEXT_PRIVATE_KEY).toBe("ssh-private");
    expect(TAG_PROVENANCE_MARK).toBe(1347571542);
    expect(TAG_NAME_PROVENANCE_MARK).toBe("provenance");
    expect(TAG_SEED_V1).toBe(300);
    expect(TAG_NAME_SEED_V1).toBe("crypto-seed");
    expect(TAG_OUTPUT_SCRIPT_HASH).toBe(400);
    expect(TAG_NAME_OUTPUT_SCRIPT_HASH).toBe("output-script-hash");
  });
});

describe("bcTags array", () => {
  test("has exactly 75 entries", () => {
    expect(bcTags).toHaveLength(75);
  });

  test("matches expected tags in registration order", () => {
    expect(expectedTags).toHaveLength(75);
    for (let i = 0; i < 75; i++) {
      const tag = bcTags[i];
      const [expectedValue, expectedName] = expectedTags[i];
      expect(tag.value).toBe(expectedValue);
      expect(tag.name).toBe(expectedName);
    }
  });
});

describe("registerTagsIn", () => {
  test("registers dcbor base tags", () => {
    const store = new TagsStore();
    registerTagsIn(store);

    const dcborBaseTags: [number, string][] = [
      [TAG_DATE, TAG_NAME_DATE],
      [TAG_POSITIVE_BIGNUM, TAG_NAME_POSITIVE_BIGNUM],
      [TAG_NEGATIVE_BIGNUM, TAG_NAME_NEGATIVE_BIGNUM],
    ];

    for (const [value, name] of dcborBaseTags) {
      const tag = store.tagForValue(value);
      expect(tag).toBeDefined();
      expect(tag!.name).toBe(name);
    }
  });

  test("forward lookup: all bc-tags registered by value", () => {
    const store = new TagsStore();
    registerTagsIn(store);

    for (const [value, name] of expectedTags) {
      const tag = store.tagForValue(value);
      expect(tag).toBeDefined();
      expect(tag!.name).toBe(name);
    }
  });

  test("reverse lookup: all bc-tags registered by name", () => {
    const store = new TagsStore();
    registerTagsIn(store);

    for (const [value, name] of expectedTags) {
      const tag = store.tagForName(name);
      expect(tag).toBeDefined();
      expect(tag!.value).toBe(value);
    }
  });
});

describe("registerTagsIn idempotent", () => {
  test("calling registerTagsIn twice does not throw", () => {
    const store = new TagsStore();
    registerTagsIn(store);
    registerTagsIn(store);

    const tag = store.tagForValue(TAG_ENVELOPE);
    expect(tag).toBeDefined();
    expect(tag!.name).toBe(TAG_NAME_ENVELOPE);
  });
});

describe("registerTags (global store)", () => {
  test("registers tags in the global store", () => {
    registerTags();

    const globalStore = getGlobalTagsStore();
    const name = globalStore.nameForValue(TAG_ENVELOPE);
    expect(name).toBe(TAG_NAME_ENVELOPE);

    const dateName = globalStore.nameForValue(TAG_DATE);
    expect(dateName).toBe(TAG_NAME_DATE);
  });
});

describe("unique values and names", () => {
  test("all tag values are unique", () => {
    const values = new Set<number>();
    for (const [value] of expectedTags) {
      expect(values.has(value)).toBe(false);
      values.add(value);
    }
  });

  test("all tag names are unique", () => {
    const names = new Set<string>();
    for (const [, name] of expectedTags) {
      expect(names.has(name)).toBe(false);
      names.add(name);
    }
  });
});

describe("nameForValue lookups", () => {
  test("symmetric key value maps to crypto-key", () => {
    const store = new TagsStore();
    registerTagsIn(store);
    expect(store.nameForValue(40023)).toBe("crypto-key");
  });

  test("provenance mark value maps to provenance", () => {
    const store = new TagsStore();
    registerTagsIn(store);
    expect(store.nameForValue(1347571542)).toBe("provenance");
  });
});
