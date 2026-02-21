package com.blockchaincommons.bctags

import com.blockchaincommons.dcbor.GlobalTags
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.TagsStore

/**
 * Blockchain Commons CBOR tag registry.
 *
 * This package defines Blockchain Commons tag constants and helpers to
 * register them in a [TagsStore] or in dCBOR's global tags store.
 */

const val TAG_URI: ULong = 32uL
const val TAG_NAME_URI: String = "url"

const val TAG_UUID: ULong = 37uL
const val TAG_NAME_UUID: String = "uuid"

const val TAG_ENCODED_CBOR: ULong = 24uL
const val TAG_NAME_ENCODED_CBOR: String = "encoded-cbor"

const val TAG_ENVELOPE: ULong = 200uL
const val TAG_NAME_ENVELOPE: String = "envelope"

const val TAG_LEAF: ULong = 201uL
const val TAG_NAME_LEAF: String = "leaf"

const val TAG_JSON: ULong = 262uL
const val TAG_NAME_JSON: String = "json"

const val TAG_KNOWN_VALUE: ULong = 40000uL
const val TAG_NAME_KNOWN_VALUE: String = "known-value"

const val TAG_DIGEST: ULong = 40001uL
const val TAG_NAME_DIGEST: String = "digest"

const val TAG_ENCRYPTED: ULong = 40002uL
const val TAG_NAME_ENCRYPTED: String = "encrypted"

const val TAG_COMPRESSED: ULong = 40003uL
const val TAG_NAME_COMPRESSED: String = "compressed"

const val TAG_REQUEST: ULong = 40004uL
const val TAG_NAME_REQUEST: String = "request"

const val TAG_RESPONSE: ULong = 40005uL
const val TAG_NAME_RESPONSE: String = "response"

const val TAG_FUNCTION: ULong = 40006uL
const val TAG_NAME_FUNCTION: String = "function"

const val TAG_PARAMETER: ULong = 40007uL
const val TAG_NAME_PARAMETER: String = "parameter"

const val TAG_PLACEHOLDER: ULong = 40008uL
const val TAG_NAME_PLACEHOLDER: String = "placeholder"

const val TAG_REPLACEMENT: ULong = 40009uL
const val TAG_NAME_REPLACEMENT: String = "replacement"

const val TAG_X25519_PRIVATE_KEY: ULong = 40010uL
const val TAG_NAME_X25519_PRIVATE_KEY: String = "agreement-private-key"

const val TAG_X25519_PUBLIC_KEY: ULong = 40011uL
const val TAG_NAME_X25519_PUBLIC_KEY: String = "agreement-public-key"

const val TAG_ARID: ULong = 40012uL
const val TAG_NAME_ARID: String = "arid"

const val TAG_PRIVATE_KEYS: ULong = 40013uL
const val TAG_NAME_PRIVATE_KEYS: String = "crypto-prvkeys"

const val TAG_NONCE: ULong = 40014uL
const val TAG_NAME_NONCE: String = "nonce"

const val TAG_PASSWORD: ULong = 40015uL
const val TAG_NAME_PASSWORD: String = "password"

const val TAG_PRIVATE_KEY_BASE: ULong = 40016uL
const val TAG_NAME_PRIVATE_KEY_BASE: String = "crypto-prvkey-base"

const val TAG_PUBLIC_KEYS: ULong = 40017uL
const val TAG_NAME_PUBLIC_KEYS: String = "crypto-pubkeys"

const val TAG_SALT: ULong = 40018uL
const val TAG_NAME_SALT: String = "salt"

const val TAG_SEALED_MESSAGE: ULong = 40019uL
const val TAG_NAME_SEALED_MESSAGE: String = "crypto-sealed"

const val TAG_SIGNATURE: ULong = 40020uL
const val TAG_NAME_SIGNATURE: String = "signature"

const val TAG_SIGNING_PRIVATE_KEY: ULong = 40021uL
const val TAG_NAME_SIGNING_PRIVATE_KEY: String = "signing-private-key"

const val TAG_SIGNING_PUBLIC_KEY: ULong = 40022uL
const val TAG_NAME_SIGNING_PUBLIC_KEY: String = "signing-public-key"

const val TAG_SYMMETRIC_KEY: ULong = 40023uL
const val TAG_NAME_SYMMETRIC_KEY: String = "crypto-key"

const val TAG_XID: ULong = 40024uL
const val TAG_NAME_XID: String = "xid"

const val TAG_REFERENCE: ULong = 40025uL
const val TAG_NAME_REFERENCE: String = "reference"

const val TAG_EVENT: ULong = 40026uL
const val TAG_NAME_EVENT: String = "event"

const val TAG_ENCRYPTED_KEY: ULong = 40027uL
const val TAG_NAME_ENCRYPTED_KEY: String = "encrypted-key"

const val TAG_MLKEM_PRIVATE_KEY: ULong = 40100uL
const val TAG_NAME_MLKEM_PRIVATE_KEY: String = "mlkem-private-key"

const val TAG_MLKEM_PUBLIC_KEY: ULong = 40101uL
const val TAG_NAME_MLKEM_PUBLIC_KEY: String = "mlkem-public-key"

const val TAG_MLKEM_CIPHERTEXT: ULong = 40102uL
const val TAG_NAME_MLKEM_CIPHERTEXT: String = "mlkem-ciphertext"

const val TAG_MLDSA_PRIVATE_KEY: ULong = 40103uL
const val TAG_NAME_MLDSA_PRIVATE_KEY: String = "mldsa-private-key"

const val TAG_MLDSA_PUBLIC_KEY: ULong = 40104uL
const val TAG_NAME_MLDSA_PUBLIC_KEY: String = "mldsa-public-key"

const val TAG_MLDSA_SIGNATURE: ULong = 40105uL
const val TAG_NAME_MLDSA_SIGNATURE: String = "mldsa-signature"

const val TAG_SEED: ULong = 40300uL
const val TAG_NAME_SEED: String = "seed"

const val TAG_HDKEY: ULong = 40303uL
const val TAG_NAME_HDKEY: String = "hdkey"

const val TAG_DERIVATION_PATH: ULong = 40304uL
const val TAG_NAME_DERIVATION_PATH: String = "keypath"

const val TAG_USE_INFO: ULong = 40305uL
const val TAG_NAME_USE_INFO: String = "coin-info"

const val TAG_EC_KEY: ULong = 40306uL
const val TAG_NAME_EC_KEY: String = "eckey"

const val TAG_ADDRESS: ULong = 40307uL
const val TAG_NAME_ADDRESS: String = "address"

const val TAG_OUTPUT_DESCRIPTOR: ULong = 40308uL
const val TAG_NAME_OUTPUT_DESCRIPTOR: String = "output-descriptor"

const val TAG_SSKR_SHARE: ULong = 40309uL
const val TAG_NAME_SSKR_SHARE: String = "sskr"

const val TAG_PSBT: ULong = 40310uL
const val TAG_NAME_PSBT: String = "psbt"

const val TAG_ACCOUNT_DESCRIPTOR: ULong = 40311uL
const val TAG_NAME_ACCOUNT_DESCRIPTOR: String = "account-descriptor"

const val TAG_SSH_TEXT_PRIVATE_KEY: ULong = 40800uL
const val TAG_NAME_SSH_TEXT_PRIVATE_KEY: String = "ssh-private"

const val TAG_SSH_TEXT_PUBLIC_KEY: ULong = 40801uL
const val TAG_NAME_SSH_TEXT_PUBLIC_KEY: String = "ssh-public"

const val TAG_SSH_TEXT_SIGNATURE: ULong = 40802uL
const val TAG_NAME_SSH_TEXT_SIGNATURE: String = "ssh-signature"

const val TAG_SSH_TEXT_CERTIFICATE: ULong = 40803uL
const val TAG_NAME_SSH_TEXT_CERTIFICATE: String = "ssh-certificate"

const val TAG_PROVENANCE_MARK: ULong = 1347571542uL
const val TAG_NAME_PROVENANCE_MARK: String = "provenance"

const val TAG_SEED_V1: ULong = 300uL
const val TAG_NAME_SEED_V1: String = "crypto-seed"

const val TAG_EC_KEY_V1: ULong = 306uL
const val TAG_NAME_EC_KEY_V1: String = "crypto-eckey"

const val TAG_SSKR_SHARE_V1: ULong = 309uL
const val TAG_NAME_SSKR_SHARE_V1: String = "crypto-sskr"

const val TAG_HDKEY_V1: ULong = 303uL
const val TAG_NAME_HDKEY_V1: String = "crypto-hdkey"

const val TAG_DERIVATION_PATH_V1: ULong = 304uL
const val TAG_NAME_DERIVATION_PATH_V1: String = "crypto-keypath"

const val TAG_USE_INFO_V1: ULong = 305uL
const val TAG_NAME_USE_INFO_V1: String = "crypto-coin-info"

const val TAG_OUTPUT_DESCRIPTOR_V1: ULong = 307uL
const val TAG_NAME_OUTPUT_DESCRIPTOR_V1: String = "crypto-output"

const val TAG_PSBT_V1: ULong = 310uL
const val TAG_NAME_PSBT_V1: String = "crypto-psbt"

const val TAG_ACCOUNT_V1: ULong = 311uL
const val TAG_NAME_ACCOUNT_V1: String = "crypto-account"

const val TAG_OUTPUT_SCRIPT_HASH: ULong = 400uL
const val TAG_NAME_OUTPUT_SCRIPT_HASH: String = "output-script-hash"

const val TAG_OUTPUT_WITNESS_SCRIPT_HASH: ULong = 401uL
const val TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH: String = "output-witness-script-hash"

const val TAG_OUTPUT_PUBLIC_KEY: ULong = 402uL
const val TAG_NAME_OUTPUT_PUBLIC_KEY: String = "output-public-key"

const val TAG_OUTPUT_PUBLIC_KEY_HASH: ULong = 403uL
const val TAG_NAME_OUTPUT_PUBLIC_KEY_HASH: String = "output-public-key-hash"

const val TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH: ULong = 404uL
const val TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH: String = "output-witness-public-key-hash"

const val TAG_OUTPUT_COMBO: ULong = 405uL
const val TAG_NAME_OUTPUT_COMBO: String = "output-combo"

const val TAG_OUTPUT_MULTISIG: ULong = 406uL
const val TAG_NAME_OUTPUT_MULTISIG: String = "output-multisig"

const val TAG_OUTPUT_SORTED_MULTISIG: ULong = 407uL
const val TAG_NAME_OUTPUT_SORTED_MULTISIG: String = "output-sorted-multisig"

const val TAG_OUTPUT_RAW_SCRIPT: ULong = 408uL
const val TAG_NAME_OUTPUT_RAW_SCRIPT: String = "output-raw-script"

const val TAG_OUTPUT_TAPROOT: ULong = 409uL
const val TAG_NAME_OUTPUT_TAPROOT: String = "output-taproot"

const val TAG_OUTPUT_COSIGNER: ULong = 410uL
const val TAG_NAME_OUTPUT_COSIGNER: String = "output-cosigner"

/** Internal ordered tag list used by [registerTagsIn]. */
internal val BC_TAGS: List<Tag> = listOf(
    Tag(TAG_URI, TAG_NAME_URI),
    Tag(TAG_UUID, TAG_NAME_UUID),
    Tag(TAG_ENCODED_CBOR, TAG_NAME_ENCODED_CBOR),
    Tag(TAG_ENVELOPE, TAG_NAME_ENVELOPE),
    Tag(TAG_LEAF, TAG_NAME_LEAF),
    Tag(TAG_JSON, TAG_NAME_JSON),
    Tag(TAG_KNOWN_VALUE, TAG_NAME_KNOWN_VALUE),
    Tag(TAG_DIGEST, TAG_NAME_DIGEST),
    Tag(TAG_ENCRYPTED, TAG_NAME_ENCRYPTED),
    Tag(TAG_COMPRESSED, TAG_NAME_COMPRESSED),
    Tag(TAG_REQUEST, TAG_NAME_REQUEST),
    Tag(TAG_RESPONSE, TAG_NAME_RESPONSE),
    Tag(TAG_FUNCTION, TAG_NAME_FUNCTION),
    Tag(TAG_PARAMETER, TAG_NAME_PARAMETER),
    Tag(TAG_PLACEHOLDER, TAG_NAME_PLACEHOLDER),
    Tag(TAG_REPLACEMENT, TAG_NAME_REPLACEMENT),
    Tag(TAG_EVENT, TAG_NAME_EVENT),
    Tag(TAG_SEED_V1, TAG_NAME_SEED_V1),
    Tag(TAG_EC_KEY_V1, TAG_NAME_EC_KEY_V1),
    Tag(TAG_SSKR_SHARE_V1, TAG_NAME_SSKR_SHARE_V1),
    Tag(TAG_SEED, TAG_NAME_SEED),
    Tag(TAG_EC_KEY, TAG_NAME_EC_KEY),
    Tag(TAG_SSKR_SHARE, TAG_NAME_SSKR_SHARE),
    Tag(TAG_X25519_PRIVATE_KEY, TAG_NAME_X25519_PRIVATE_KEY),
    Tag(TAG_X25519_PUBLIC_KEY, TAG_NAME_X25519_PUBLIC_KEY),
    Tag(TAG_ARID, TAG_NAME_ARID),
    Tag(TAG_PRIVATE_KEYS, TAG_NAME_PRIVATE_KEYS),
    Tag(TAG_NONCE, TAG_NAME_NONCE),
    Tag(TAG_PASSWORD, TAG_NAME_PASSWORD),
    Tag(TAG_PRIVATE_KEY_BASE, TAG_NAME_PRIVATE_KEY_BASE),
    Tag(TAG_PUBLIC_KEYS, TAG_NAME_PUBLIC_KEYS),
    Tag(TAG_SALT, TAG_NAME_SALT),
    Tag(TAG_SEALED_MESSAGE, TAG_NAME_SEALED_MESSAGE),
    Tag(TAG_SIGNATURE, TAG_NAME_SIGNATURE),
    Tag(TAG_SIGNING_PRIVATE_KEY, TAG_NAME_SIGNING_PRIVATE_KEY),
    Tag(TAG_SIGNING_PUBLIC_KEY, TAG_NAME_SIGNING_PUBLIC_KEY),
    Tag(TAG_SYMMETRIC_KEY, TAG_NAME_SYMMETRIC_KEY),
    Tag(TAG_XID, TAG_NAME_XID),
    Tag(TAG_REFERENCE, TAG_NAME_REFERENCE),
    Tag(TAG_ENCRYPTED_KEY, TAG_NAME_ENCRYPTED_KEY),
    Tag(TAG_MLKEM_PRIVATE_KEY, TAG_NAME_MLKEM_PRIVATE_KEY),
    Tag(TAG_MLKEM_PUBLIC_KEY, TAG_NAME_MLKEM_PUBLIC_KEY),
    Tag(TAG_MLKEM_CIPHERTEXT, TAG_NAME_MLKEM_CIPHERTEXT),
    Tag(TAG_MLDSA_PRIVATE_KEY, TAG_NAME_MLDSA_PRIVATE_KEY),
    Tag(TAG_MLDSA_PUBLIC_KEY, TAG_NAME_MLDSA_PUBLIC_KEY),
    Tag(TAG_MLDSA_SIGNATURE, TAG_NAME_MLDSA_SIGNATURE),
    Tag(TAG_HDKEY_V1, TAG_NAME_HDKEY_V1),
    Tag(TAG_DERIVATION_PATH_V1, TAG_NAME_DERIVATION_PATH_V1),
    Tag(TAG_USE_INFO_V1, TAG_NAME_USE_INFO_V1),
    Tag(TAG_OUTPUT_DESCRIPTOR_V1, TAG_NAME_OUTPUT_DESCRIPTOR_V1),
    Tag(TAG_PSBT_V1, TAG_NAME_PSBT_V1),
    Tag(TAG_ACCOUNT_V1, TAG_NAME_ACCOUNT_V1),
    Tag(TAG_HDKEY, TAG_NAME_HDKEY),
    Tag(TAG_DERIVATION_PATH, TAG_NAME_DERIVATION_PATH),
    Tag(TAG_USE_INFO, TAG_NAME_USE_INFO),
    Tag(TAG_ADDRESS, TAG_NAME_ADDRESS),
    Tag(TAG_OUTPUT_DESCRIPTOR, TAG_NAME_OUTPUT_DESCRIPTOR),
    Tag(TAG_PSBT, TAG_NAME_PSBT),
    Tag(TAG_ACCOUNT_DESCRIPTOR, TAG_NAME_ACCOUNT_DESCRIPTOR),
    Tag(TAG_SSH_TEXT_PRIVATE_KEY, TAG_NAME_SSH_TEXT_PRIVATE_KEY),
    Tag(TAG_SSH_TEXT_PUBLIC_KEY, TAG_NAME_SSH_TEXT_PUBLIC_KEY),
    Tag(TAG_SSH_TEXT_SIGNATURE, TAG_NAME_SSH_TEXT_SIGNATURE),
    Tag(TAG_SSH_TEXT_CERTIFICATE, TAG_NAME_SSH_TEXT_CERTIFICATE),
    Tag(TAG_OUTPUT_SCRIPT_HASH, TAG_NAME_OUTPUT_SCRIPT_HASH),
    Tag(TAG_OUTPUT_WITNESS_SCRIPT_HASH, TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH),
    Tag(TAG_OUTPUT_PUBLIC_KEY, TAG_NAME_OUTPUT_PUBLIC_KEY),
    Tag(TAG_OUTPUT_PUBLIC_KEY_HASH, TAG_NAME_OUTPUT_PUBLIC_KEY_HASH),
    Tag(TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH, TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH),
    Tag(TAG_OUTPUT_COMBO, TAG_NAME_OUTPUT_COMBO),
    Tag(TAG_OUTPUT_MULTISIG, TAG_NAME_OUTPUT_MULTISIG),
    Tag(TAG_OUTPUT_SORTED_MULTISIG, TAG_NAME_OUTPUT_SORTED_MULTISIG),
    Tag(TAG_OUTPUT_RAW_SCRIPT, TAG_NAME_OUTPUT_RAW_SCRIPT),
    Tag(TAG_OUTPUT_TAPROOT, TAG_NAME_OUTPUT_TAPROOT),
    Tag(TAG_OUTPUT_COSIGNER, TAG_NAME_OUTPUT_COSIGNER),
    Tag(TAG_PROVENANCE_MARK, TAG_NAME_PROVENANCE_MARK))

/** Registers dCBOR base tags and Blockchain Commons tags in [tagsStore]. */
fun registerTagsIn(tagsStore: TagsStore) {
    com.blockchaincommons.dcbor.registerTagsIn(tagsStore)
    tagsStore.insertAll(BC_TAGS)
}

/** Registers Blockchain Commons tags in dCBOR's global tag store. */
fun registerTags() {
    GlobalTags.withTagsMut { registerTagsIn(it) }
}
