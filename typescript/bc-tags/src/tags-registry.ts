/**
 * Blockchain Commons CBOR Tags Registry.
 *
 * Defines 75 CBOR semantic tags used across the Blockchain Commons ecosystem,
 * including Gordian Envelope, cryptographic primitives, distributed function
 * calls, HD wallet derivation, SSH keys, and output descriptors.
 *
 * @see https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2020-006-urtypes.md
 * @see https://www.iana.org/assignments/cbor-tags/cbor-tags.xhtml
 *
 * @module tags-registry
 */

import {
  type Tag,
  createTag,
  type TagsStore,
  getGlobalTagsStore,
  registerTagsIn as dcborRegisterTagsIn,
} from "@bc/dcbor";

// ============================================================================
// Standard IANA Tags
// ============================================================================

/** CBOR tag 32: URI. */
export const TAG_URI = 32;
/** Human-readable name for TAG_URI. */
export const TAG_NAME_URI = "url";

/** CBOR tag 37: UUID. */
export const TAG_UUID = 37;
/** Human-readable name for TAG_UUID. */
export const TAG_NAME_UUID = "uuid";

// ============================================================================
// Core Envelope Tags
// ============================================================================

/** CBOR tag 24: Encoded CBOR data item. */
export const TAG_ENCODED_CBOR = 24;
/** Human-readable name for TAG_ENCODED_CBOR. */
export const TAG_NAME_ENCODED_CBOR = "encoded-cbor";

/** CBOR tag 200: Gordian Envelope. */
export const TAG_ENVELOPE = 200;
/** Human-readable name for TAG_ENVELOPE. */
export const TAG_NAME_ENVELOPE = "envelope";

/** CBOR tag 201: dCBOR/Envelope leaf data item. */
export const TAG_LEAF = 201;
/** Human-readable name for TAG_LEAF. */
export const TAG_NAME_LEAF = "leaf";

/** CBOR tag 262: JSON text embedded in a byte string. */
export const TAG_JSON = 262;
/** Human-readable name for TAG_JSON. */
export const TAG_NAME_JSON = "json";

// ============================================================================
// Envelope Extension Tags
// ============================================================================

/** CBOR tag 40000: Known value. */
export const TAG_KNOWN_VALUE = 40000;
/** Human-readable name for TAG_KNOWN_VALUE. */
export const TAG_NAME_KNOWN_VALUE = "known-value";

/** CBOR tag 40001: Cryptographic digest. */
export const TAG_DIGEST = 40001;
/** Human-readable name for TAG_DIGEST. */
export const TAG_NAME_DIGEST = "digest";

/** CBOR tag 40002: Encrypted data. */
export const TAG_ENCRYPTED = 40002;
/** Human-readable name for TAG_ENCRYPTED. */
export const TAG_NAME_ENCRYPTED = "encrypted";

/** CBOR tag 40003: Compressed data. */
export const TAG_COMPRESSED = 40003;
/** Human-readable name for TAG_COMPRESSED. */
export const TAG_NAME_COMPRESSED = "compressed";

// ============================================================================
// Distributed Function Call Tags
// ============================================================================

/** CBOR tag 40004: Function call request. */
export const TAG_REQUEST = 40004;
/** Human-readable name for TAG_REQUEST. */
export const TAG_NAME_REQUEST = "request";

/** CBOR tag 40005: Function call response. */
export const TAG_RESPONSE = 40005;
/** Human-readable name for TAG_RESPONSE. */
export const TAG_NAME_RESPONSE = "response";

/** CBOR tag 40006: Function identifier. */
export const TAG_FUNCTION = 40006;
/** Human-readable name for TAG_FUNCTION. */
export const TAG_NAME_FUNCTION = "function";

/** CBOR tag 40007: Function parameter. */
export const TAG_PARAMETER = 40007;
/** Human-readable name for TAG_PARAMETER. */
export const TAG_NAME_PARAMETER = "parameter";

/** CBOR tag 40008: Response placeholder. */
export const TAG_PLACEHOLDER = 40008;
/** Human-readable name for TAG_PLACEHOLDER. */
export const TAG_NAME_PLACEHOLDER = "placeholder";

/** CBOR tag 40009: Response replacement. */
export const TAG_REPLACEMENT = 40009;
/** Human-readable name for TAG_REPLACEMENT. */
export const TAG_NAME_REPLACEMENT = "replacement";

// ============================================================================
// Cryptographic Key and Identity Tags
// ============================================================================

/** CBOR tag 40010: X25519 agreement private key. */
export const TAG_X25519_PRIVATE_KEY = 40010;
/** Human-readable name for TAG_X25519_PRIVATE_KEY. */
export const TAG_NAME_X25519_PRIVATE_KEY = "agreement-private-key";

/** CBOR tag 40011: X25519 agreement public key. */
export const TAG_X25519_PUBLIC_KEY = 40011;
/** Human-readable name for TAG_X25519_PUBLIC_KEY. */
export const TAG_NAME_X25519_PUBLIC_KEY = "agreement-public-key";

/** CBOR tag 40012: Apparently Random Identifier. */
export const TAG_ARID = 40012;
/** Human-readable name for TAG_ARID. */
export const TAG_NAME_ARID = "arid";

/** CBOR tag 40013: Private key bundle. */
export const TAG_PRIVATE_KEYS = 40013;
/** Human-readable name for TAG_PRIVATE_KEYS. */
export const TAG_NAME_PRIVATE_KEYS = "crypto-prvkeys";

/** CBOR tag 40014: Cryptographic nonce. */
export const TAG_NONCE = 40014;
/** Human-readable name for TAG_NONCE. */
export const TAG_NAME_NONCE = "nonce";

/** CBOR tag 40015: Password. */
export const TAG_PASSWORD = 40015;
/** Human-readable name for TAG_PASSWORD. */
export const TAG_NAME_PASSWORD = "password";

/** CBOR tag 40016: Private key base material. */
export const TAG_PRIVATE_KEY_BASE = 40016;
/** Human-readable name for TAG_PRIVATE_KEY_BASE. */
export const TAG_NAME_PRIVATE_KEY_BASE = "crypto-prvkey-base";

/** CBOR tag 40017: Public key bundle. */
export const TAG_PUBLIC_KEYS = 40017;
/** Human-readable name for TAG_PUBLIC_KEYS. */
export const TAG_NAME_PUBLIC_KEYS = "crypto-pubkeys";

/** CBOR tag 40018: Cryptographic salt. */
export const TAG_SALT = 40018;
/** Human-readable name for TAG_SALT. */
export const TAG_NAME_SALT = "salt";

/** CBOR tag 40019: Sealed (encrypted+signed) message. */
export const TAG_SEALED_MESSAGE = 40019;
/** Human-readable name for TAG_SEALED_MESSAGE. */
export const TAG_NAME_SEALED_MESSAGE = "crypto-sealed";

/** CBOR tag 40020: Cryptographic signature. */
export const TAG_SIGNATURE = 40020;
/** Human-readable name for TAG_SIGNATURE. */
export const TAG_NAME_SIGNATURE = "signature";

/** CBOR tag 40021: Signing private key. */
export const TAG_SIGNING_PRIVATE_KEY = 40021;
/** Human-readable name for TAG_SIGNING_PRIVATE_KEY. */
export const TAG_NAME_SIGNING_PRIVATE_KEY = "signing-private-key";

/** CBOR tag 40022: Signing public key. */
export const TAG_SIGNING_PUBLIC_KEY = 40022;
/** Human-readable name for TAG_SIGNING_PUBLIC_KEY. */
export const TAG_NAME_SIGNING_PUBLIC_KEY = "signing-public-key";

/** CBOR tag 40023: Symmetric encryption key. */
export const TAG_SYMMETRIC_KEY = 40023;
/** Human-readable name for TAG_SYMMETRIC_KEY. */
export const TAG_NAME_SYMMETRIC_KEY = "crypto-key";

/** CBOR tag 40024: Extended Identifier. */
export const TAG_XID = 40024;
/** Human-readable name for TAG_XID. */
export const TAG_NAME_XID = "xid";

/** CBOR tag 40025: Reference. */
export const TAG_REFERENCE = 40025;
/** Human-readable name for TAG_REFERENCE. */
export const TAG_NAME_REFERENCE = "reference";

/** CBOR tag 40026: Event. */
export const TAG_EVENT = 40026;
/** Human-readable name for TAG_EVENT. */
export const TAG_NAME_EVENT = "event";

/** CBOR tag 40027: Encrypted key. */
export const TAG_ENCRYPTED_KEY = 40027;
/** Human-readable name for TAG_ENCRYPTED_KEY. */
export const TAG_NAME_ENCRYPTED_KEY = "encrypted-key";

// ============================================================================
// Post-Quantum Cryptography Tags
// ============================================================================

/** CBOR tag 40100: ML-KEM private key. */
export const TAG_MLKEM_PRIVATE_KEY = 40100;
/** Human-readable name for TAG_MLKEM_PRIVATE_KEY. */
export const TAG_NAME_MLKEM_PRIVATE_KEY = "mlkem-private-key";

/** CBOR tag 40101: ML-KEM public key. */
export const TAG_MLKEM_PUBLIC_KEY = 40101;
/** Human-readable name for TAG_MLKEM_PUBLIC_KEY. */
export const TAG_NAME_MLKEM_PUBLIC_KEY = "mlkem-public-key";

/** CBOR tag 40102: ML-KEM ciphertext. */
export const TAG_MLKEM_CIPHERTEXT = 40102;
/** Human-readable name for TAG_MLKEM_CIPHERTEXT. */
export const TAG_NAME_MLKEM_CIPHERTEXT = "mlkem-ciphertext";

/** CBOR tag 40103: ML-DSA private key. */
export const TAG_MLDSA_PRIVATE_KEY = 40103;
/** Human-readable name for TAG_MLDSA_PRIVATE_KEY. */
export const TAG_NAME_MLDSA_PRIVATE_KEY = "mldsa-private-key";

/** CBOR tag 40104: ML-DSA public key. */
export const TAG_MLDSA_PUBLIC_KEY = 40104;
/** Human-readable name for TAG_MLDSA_PUBLIC_KEY. */
export const TAG_NAME_MLDSA_PUBLIC_KEY = "mldsa-public-key";

/** CBOR tag 40105: ML-DSA signature. */
export const TAG_MLDSA_SIGNATURE = 40105;
/** Human-readable name for TAG_MLDSA_SIGNATURE. */
export const TAG_NAME_MLDSA_SIGNATURE = "mldsa-signature";

// ============================================================================
// Key and Descriptor Tags
// ============================================================================

/** CBOR tag 40300: Cryptographic seed. */
export const TAG_SEED = 40300;
/** Human-readable name for TAG_SEED. */
export const TAG_NAME_SEED = "seed";

/** CBOR tag 40303: HD (hierarchical deterministic) key. */
export const TAG_HDKEY = 40303;
/** Human-readable name for TAG_HDKEY. */
export const TAG_NAME_HDKEY = "hdkey";

/** CBOR tag 40304: Key derivation path. */
export const TAG_DERIVATION_PATH = 40304;
/** Human-readable name for TAG_DERIVATION_PATH. */
export const TAG_NAME_DERIVATION_PATH = "keypath";

/** CBOR tag 40305: Coin/network use info. */
export const TAG_USE_INFO = 40305;
/** Human-readable name for TAG_USE_INFO. */
export const TAG_NAME_USE_INFO = "coin-info";

/** CBOR tag 40306: Elliptic curve key. */
export const TAG_EC_KEY = 40306;
/** Human-readable name for TAG_EC_KEY. */
export const TAG_NAME_EC_KEY = "eckey";

/** CBOR tag 40307: Cryptocurrency address. */
export const TAG_ADDRESS = 40307;
/** Human-readable name for TAG_ADDRESS. */
export const TAG_NAME_ADDRESS = "address";

/** CBOR tag 40308: Output descriptor. */
export const TAG_OUTPUT_DESCRIPTOR = 40308;
/** Human-readable name for TAG_OUTPUT_DESCRIPTOR. */
export const TAG_NAME_OUTPUT_DESCRIPTOR = "output-descriptor";

/** CBOR tag 40309: SSKR share. */
export const TAG_SSKR_SHARE = 40309;
/** Human-readable name for TAG_SSKR_SHARE. */
export const TAG_NAME_SSKR_SHARE = "sskr";

/** CBOR tag 40310: Partially Signed Bitcoin Transaction. */
export const TAG_PSBT = 40310;
/** Human-readable name for TAG_PSBT. */
export const TAG_NAME_PSBT = "psbt";

/** CBOR tag 40311: Account descriptor. */
export const TAG_ACCOUNT_DESCRIPTOR = 40311;
/** Human-readable name for TAG_ACCOUNT_DESCRIPTOR. */
export const TAG_NAME_ACCOUNT_DESCRIPTOR = "account-descriptor";

// ============================================================================
// SSH Tags
// ============================================================================

/** CBOR tag 40800: SSH text-format private key. */
export const TAG_SSH_TEXT_PRIVATE_KEY = 40800;
/** Human-readable name for TAG_SSH_TEXT_PRIVATE_KEY. */
export const TAG_NAME_SSH_TEXT_PRIVATE_KEY = "ssh-private";

/** CBOR tag 40801: SSH text-format public key. */
export const TAG_SSH_TEXT_PUBLIC_KEY = 40801;
/** Human-readable name for TAG_SSH_TEXT_PUBLIC_KEY. */
export const TAG_NAME_SSH_TEXT_PUBLIC_KEY = "ssh-public";

/** CBOR tag 40802: SSH text-format signature. */
export const TAG_SSH_TEXT_SIGNATURE = 40802;
/** Human-readable name for TAG_SSH_TEXT_SIGNATURE. */
export const TAG_NAME_SSH_TEXT_SIGNATURE = "ssh-signature";

/** CBOR tag 40803: SSH text-format certificate. */
export const TAG_SSH_TEXT_CERTIFICATE = 40803;
/** Human-readable name for TAG_SSH_TEXT_CERTIFICATE. */
export const TAG_NAME_SSH_TEXT_CERTIFICATE = "ssh-certificate";

// ============================================================================
// Provenance Tag
// ============================================================================

/** CBOR tag 1347571542: Provenance mark. */
export const TAG_PROVENANCE_MARK = 1347571542;
/** Human-readable name for TAG_PROVENANCE_MARK. */
export const TAG_NAME_PROVENANCE_MARK = "provenance";

// ============================================================================
// Deprecated Tags (V1)
//
// These tags are deprecated and should not be used in new code.
// They remain registered for backward compatibility with external
// implementations that may still use them.
// ============================================================================

/** @deprecated CBOR tag 300: Cryptographic seed (V1). Use TAG_SEED instead. */
export const TAG_SEED_V1 = 300;
/** @deprecated Human-readable name for TAG_SEED_V1. */
export const TAG_NAME_SEED_V1 = "crypto-seed";

/** @deprecated CBOR tag 306: Elliptic curve key (V1). Use TAG_EC_KEY instead. */
export const TAG_EC_KEY_V1 = 306;
/** @deprecated Human-readable name for TAG_EC_KEY_V1. */
export const TAG_NAME_EC_KEY_V1 = "crypto-eckey";

/** @deprecated CBOR tag 309: SSKR share (V1). Use TAG_SSKR_SHARE instead. */
export const TAG_SSKR_SHARE_V1 = 309;
/** @deprecated Human-readable name for TAG_SSKR_SHARE_V1. */
export const TAG_NAME_SSKR_SHARE_V1 = "crypto-sskr";

/** @deprecated CBOR tag 303: HD key (V1). Use TAG_HDKEY instead. */
export const TAG_HDKEY_V1 = 303;
/** @deprecated Human-readable name for TAG_HDKEY_V1. */
export const TAG_NAME_HDKEY_V1 = "crypto-hdkey";

/** @deprecated CBOR tag 304: Key derivation path (V1). Use TAG_DERIVATION_PATH instead. */
export const TAG_DERIVATION_PATH_V1 = 304;
/** @deprecated Human-readable name for TAG_DERIVATION_PATH_V1. */
export const TAG_NAME_DERIVATION_PATH_V1 = "crypto-keypath";

/** @deprecated CBOR tag 305: Coin/network use info (V1). Use TAG_USE_INFO instead. */
export const TAG_USE_INFO_V1 = 305;
/** @deprecated Human-readable name for TAG_USE_INFO_V1. */
export const TAG_NAME_USE_INFO_V1 = "crypto-coin-info";

/** @deprecated CBOR tag 307: Output descriptor (V1). Use TAG_OUTPUT_DESCRIPTOR instead. */
export const TAG_OUTPUT_DESCRIPTOR_V1 = 307;
/** @deprecated Human-readable name for TAG_OUTPUT_DESCRIPTOR_V1. */
export const TAG_NAME_OUTPUT_DESCRIPTOR_V1 = "crypto-output";

/** @deprecated CBOR tag 310: PSBT (V1). Use TAG_PSBT instead. */
export const TAG_PSBT_V1 = 310;
/** @deprecated Human-readable name for TAG_PSBT_V1. */
export const TAG_NAME_PSBT_V1 = "crypto-psbt";

/** @deprecated CBOR tag 311: Account descriptor (V1). Use TAG_ACCOUNT_DESCRIPTOR instead. */
export const TAG_ACCOUNT_V1 = 311;
/** @deprecated Human-readable name for TAG_ACCOUNT_V1. */
export const TAG_NAME_ACCOUNT_V1 = "crypto-account";

// ============================================================================
// Output Descriptor Sub-Tags (for AccountBundle)
// ============================================================================

/** CBOR tag 400: Output script hash descriptor. */
export const TAG_OUTPUT_SCRIPT_HASH = 400;
/** Human-readable name for TAG_OUTPUT_SCRIPT_HASH. */
export const TAG_NAME_OUTPUT_SCRIPT_HASH = "output-script-hash";

/** CBOR tag 401: Output witness script hash descriptor. */
export const TAG_OUTPUT_WITNESS_SCRIPT_HASH = 401;
/** Human-readable name for TAG_OUTPUT_WITNESS_SCRIPT_HASH. */
export const TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH = "output-witness-script-hash";

/** CBOR tag 402: Output public key descriptor. */
export const TAG_OUTPUT_PUBLIC_KEY = 402;
/** Human-readable name for TAG_OUTPUT_PUBLIC_KEY. */
export const TAG_NAME_OUTPUT_PUBLIC_KEY = "output-public-key";

/** CBOR tag 403: Output public key hash descriptor. */
export const TAG_OUTPUT_PUBLIC_KEY_HASH = 403;
/** Human-readable name for TAG_OUTPUT_PUBLIC_KEY_HASH. */
export const TAG_NAME_OUTPUT_PUBLIC_KEY_HASH = "output-public-key-hash";

/** CBOR tag 404: Output witness public key hash descriptor. */
export const TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH = 404;
/** Human-readable name for TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH. */
export const TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH = "output-witness-public-key-hash";

/** CBOR tag 405: Output combo descriptor. */
export const TAG_OUTPUT_COMBO = 405;
/** Human-readable name for TAG_OUTPUT_COMBO. */
export const TAG_NAME_OUTPUT_COMBO = "output-combo";

/** CBOR tag 406: Output multisig descriptor. */
export const TAG_OUTPUT_MULTISIG = 406;
/** Human-readable name for TAG_OUTPUT_MULTISIG. */
export const TAG_NAME_OUTPUT_MULTISIG = "output-multisig";

/** CBOR tag 407: Output sorted multisig descriptor. */
export const TAG_OUTPUT_SORTED_MULTISIG = 407;
/** Human-readable name for TAG_OUTPUT_SORTED_MULTISIG. */
export const TAG_NAME_OUTPUT_SORTED_MULTISIG = "output-sorted-multisig";

/** CBOR tag 408: Output raw script descriptor. */
export const TAG_OUTPUT_RAW_SCRIPT = 408;
/** Human-readable name for TAG_OUTPUT_RAW_SCRIPT. */
export const TAG_NAME_OUTPUT_RAW_SCRIPT = "output-raw-script";

/** CBOR tag 409: Output taproot descriptor. */
export const TAG_OUTPUT_TAPROOT = 409;
/** Human-readable name for TAG_OUTPUT_TAPROOT. */
export const TAG_NAME_OUTPUT_TAPROOT = "output-taproot";

/** CBOR tag 410: Output cosigner descriptor. */
export const TAG_OUTPUT_COSIGNER = 410;
/** Human-readable name for TAG_OUTPUT_COSIGNER. */
export const TAG_NAME_OUTPUT_COSIGNER = "output-cosigner";

// ============================================================================
// Tag Registration
// ============================================================================

/**
 * All 75 bc-tags Tag objects in registration order.
 *
 * This array preserves the exact registration order from the Rust reference
 * implementation and is used by {@link registerTagsIn}.
 */
export const bcTags: Tag[] = [
  createTag(TAG_URI, TAG_NAME_URI),
  createTag(TAG_UUID, TAG_NAME_UUID),
  createTag(TAG_ENCODED_CBOR, TAG_NAME_ENCODED_CBOR),
  createTag(TAG_ENVELOPE, TAG_NAME_ENVELOPE),
  createTag(TAG_LEAF, TAG_NAME_LEAF),
  createTag(TAG_JSON, TAG_NAME_JSON),
  createTag(TAG_KNOWN_VALUE, TAG_NAME_KNOWN_VALUE),
  createTag(TAG_DIGEST, TAG_NAME_DIGEST),
  createTag(TAG_ENCRYPTED, TAG_NAME_ENCRYPTED),
  createTag(TAG_COMPRESSED, TAG_NAME_COMPRESSED),
  createTag(TAG_REQUEST, TAG_NAME_REQUEST),
  createTag(TAG_RESPONSE, TAG_NAME_RESPONSE),
  createTag(TAG_FUNCTION, TAG_NAME_FUNCTION),
  createTag(TAG_PARAMETER, TAG_NAME_PARAMETER),
  createTag(TAG_PLACEHOLDER, TAG_NAME_PLACEHOLDER),
  createTag(TAG_REPLACEMENT, TAG_NAME_REPLACEMENT),
  createTag(TAG_EVENT, TAG_NAME_EVENT),
  createTag(TAG_SEED_V1, TAG_NAME_SEED_V1),
  createTag(TAG_EC_KEY_V1, TAG_NAME_EC_KEY_V1),
  createTag(TAG_SSKR_SHARE_V1, TAG_NAME_SSKR_SHARE_V1),
  createTag(TAG_SEED, TAG_NAME_SEED),
  createTag(TAG_EC_KEY, TAG_NAME_EC_KEY),
  createTag(TAG_SSKR_SHARE, TAG_NAME_SSKR_SHARE),
  createTag(TAG_X25519_PRIVATE_KEY, TAG_NAME_X25519_PRIVATE_KEY),
  createTag(TAG_X25519_PUBLIC_KEY, TAG_NAME_X25519_PUBLIC_KEY),
  createTag(TAG_ARID, TAG_NAME_ARID),
  createTag(TAG_PRIVATE_KEYS, TAG_NAME_PRIVATE_KEYS),
  createTag(TAG_NONCE, TAG_NAME_NONCE),
  createTag(TAG_PASSWORD, TAG_NAME_PASSWORD),
  createTag(TAG_PRIVATE_KEY_BASE, TAG_NAME_PRIVATE_KEY_BASE),
  createTag(TAG_PUBLIC_KEYS, TAG_NAME_PUBLIC_KEYS),
  createTag(TAG_SALT, TAG_NAME_SALT),
  createTag(TAG_SEALED_MESSAGE, TAG_NAME_SEALED_MESSAGE),
  createTag(TAG_SIGNATURE, TAG_NAME_SIGNATURE),
  createTag(TAG_SIGNING_PRIVATE_KEY, TAG_NAME_SIGNING_PRIVATE_KEY),
  createTag(TAG_SIGNING_PUBLIC_KEY, TAG_NAME_SIGNING_PUBLIC_KEY),
  createTag(TAG_SYMMETRIC_KEY, TAG_NAME_SYMMETRIC_KEY),
  createTag(TAG_XID, TAG_NAME_XID),
  createTag(TAG_REFERENCE, TAG_NAME_REFERENCE),
  createTag(TAG_ENCRYPTED_KEY, TAG_NAME_ENCRYPTED_KEY),
  createTag(TAG_MLKEM_PRIVATE_KEY, TAG_NAME_MLKEM_PRIVATE_KEY),
  createTag(TAG_MLKEM_PUBLIC_KEY, TAG_NAME_MLKEM_PUBLIC_KEY),
  createTag(TAG_MLKEM_CIPHERTEXT, TAG_NAME_MLKEM_CIPHERTEXT),
  createTag(TAG_MLDSA_PRIVATE_KEY, TAG_NAME_MLDSA_PRIVATE_KEY),
  createTag(TAG_MLDSA_PUBLIC_KEY, TAG_NAME_MLDSA_PUBLIC_KEY),
  createTag(TAG_MLDSA_SIGNATURE, TAG_NAME_MLDSA_SIGNATURE),
  createTag(TAG_HDKEY_V1, TAG_NAME_HDKEY_V1),
  createTag(TAG_DERIVATION_PATH_V1, TAG_NAME_DERIVATION_PATH_V1),
  createTag(TAG_USE_INFO_V1, TAG_NAME_USE_INFO_V1),
  createTag(TAG_OUTPUT_DESCRIPTOR_V1, TAG_NAME_OUTPUT_DESCRIPTOR_V1),
  createTag(TAG_PSBT_V1, TAG_NAME_PSBT_V1),
  createTag(TAG_ACCOUNT_V1, TAG_NAME_ACCOUNT_V1),
  createTag(TAG_HDKEY, TAG_NAME_HDKEY),
  createTag(TAG_DERIVATION_PATH, TAG_NAME_DERIVATION_PATH),
  createTag(TAG_USE_INFO, TAG_NAME_USE_INFO),
  createTag(TAG_ADDRESS, TAG_NAME_ADDRESS),
  createTag(TAG_OUTPUT_DESCRIPTOR, TAG_NAME_OUTPUT_DESCRIPTOR),
  createTag(TAG_PSBT, TAG_NAME_PSBT),
  createTag(TAG_ACCOUNT_DESCRIPTOR, TAG_NAME_ACCOUNT_DESCRIPTOR),
  createTag(TAG_SSH_TEXT_PRIVATE_KEY, TAG_NAME_SSH_TEXT_PRIVATE_KEY),
  createTag(TAG_SSH_TEXT_PUBLIC_KEY, TAG_NAME_SSH_TEXT_PUBLIC_KEY),
  createTag(TAG_SSH_TEXT_SIGNATURE, TAG_NAME_SSH_TEXT_SIGNATURE),
  createTag(TAG_SSH_TEXT_CERTIFICATE, TAG_NAME_SSH_TEXT_CERTIFICATE),
  createTag(TAG_OUTPUT_SCRIPT_HASH, TAG_NAME_OUTPUT_SCRIPT_HASH),
  createTag(TAG_OUTPUT_WITNESS_SCRIPT_HASH, TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH),
  createTag(TAG_OUTPUT_PUBLIC_KEY, TAG_NAME_OUTPUT_PUBLIC_KEY),
  createTag(TAG_OUTPUT_PUBLIC_KEY_HASH, TAG_NAME_OUTPUT_PUBLIC_KEY_HASH),
  createTag(TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH, TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH),
  createTag(TAG_OUTPUT_COMBO, TAG_NAME_OUTPUT_COMBO),
  createTag(TAG_OUTPUT_MULTISIG, TAG_NAME_OUTPUT_MULTISIG),
  createTag(TAG_OUTPUT_SORTED_MULTISIG, TAG_NAME_OUTPUT_SORTED_MULTISIG),
  createTag(TAG_OUTPUT_RAW_SCRIPT, TAG_NAME_OUTPUT_RAW_SCRIPT),
  createTag(TAG_OUTPUT_TAPROOT, TAG_NAME_OUTPUT_TAPROOT),
  createTag(TAG_OUTPUT_COSIGNER, TAG_NAME_OUTPUT_COSIGNER),
  createTag(TAG_PROVENANCE_MARK, TAG_NAME_PROVENANCE_MARK),
];

/**
 * Register all bc-tags (plus dcbor base tags) into the provided tag store.
 *
 * Calls dcbor's `registerTagsIn` first to ensure base tags (date, bignum)
 * are registered, then inserts all 75 bc-tags.
 *
 * @param tagsStore - The tag store to register into
 */
export const registerTagsIn = (tagsStore: TagsStore): void => {
  dcborRegisterTagsIn(tagsStore);
  tagsStore.insertAll(bcTags);
};

/**
 * Register all bc-tags (plus dcbor base tags) in the global tag store.
 *
 * This function is idempotent — calling it multiple times is safe.
 */
export const registerTags = (): void => {
  registerTagsIn(getGlobalTagsStore());
};
