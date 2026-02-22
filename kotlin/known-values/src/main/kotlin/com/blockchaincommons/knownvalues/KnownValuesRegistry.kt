package com.blockchaincommons.knownvalues

// For definitions see:
// https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2023-002-known-value.md#appendix-a-registry

const val UNIT_RAW: ULong = 0uL
val UNIT: KnownValue = KnownValue.withName(UNIT_RAW, "")

const val IS_A_RAW: ULong = 1uL
val IS_A: KnownValue = KnownValue.withName(IS_A_RAW, "isA")

const val ID_RAW: ULong = 2uL
val ID: KnownValue = KnownValue.withName(ID_RAW, "id")

const val SIGNED_RAW: ULong = 3uL
val SIGNED: KnownValue = KnownValue.withName(SIGNED_RAW, "signed")

const val NOTE_RAW: ULong = 4uL
val NOTE: KnownValue = KnownValue.withName(NOTE_RAW, "note")

const val HAS_RECIPIENT_RAW: ULong = 5uL
val HAS_RECIPIENT: KnownValue = KnownValue.withName(HAS_RECIPIENT_RAW, "hasRecipient")

const val SSKR_SHARE_RAW: ULong = 6uL
val SSKR_SHARE: KnownValue = KnownValue.withName(SSKR_SHARE_RAW, "sskrShare")

const val CONTROLLER_RAW: ULong = 7uL
val CONTROLLER: KnownValue = KnownValue.withName(CONTROLLER_RAW, "controller")

const val KEY_RAW: ULong = 8uL
val KEY: KnownValue = KnownValue.withName(KEY_RAW, "key")

const val DEREFERENCE_VIA_RAW: ULong = 9uL
val DEREFERENCE_VIA: KnownValue = KnownValue.withName(DEREFERENCE_VIA_RAW, "dereferenceVia")

const val ENTITY_RAW: ULong = 10uL
val ENTITY: KnownValue = KnownValue.withName(ENTITY_RAW, "entity")

const val NAME_RAW: ULong = 11uL
val NAME: KnownValue = KnownValue.withName(NAME_RAW, "name")

const val LANGUAGE_RAW: ULong = 12uL
val LANGUAGE: KnownValue = KnownValue.withName(LANGUAGE_RAW, "language")

const val ISSUER_RAW: ULong = 13uL
val ISSUER: KnownValue = KnownValue.withName(ISSUER_RAW, "issuer")

const val HOLDER_RAW: ULong = 14uL
val HOLDER: KnownValue = KnownValue.withName(HOLDER_RAW, "holder")

const val SALT_RAW: ULong = 15uL
val SALT: KnownValue = KnownValue.withName(SALT_RAW, "salt")

const val DATE_RAW: ULong = 16uL
val DATE: KnownValue = KnownValue.withName(DATE_RAW, "date")

const val UNKNOWN_VALUE_RAW: ULong = 17uL
val UNKNOWN_VALUE: KnownValue = KnownValue.withName(UNKNOWN_VALUE_RAW, "Unknown")

const val VERSION_VALUE_RAW: ULong = 18uL
val VERSION_VALUE: KnownValue = KnownValue.withName(VERSION_VALUE_RAW, "version")

const val HAS_SECRET_RAW: ULong = 19uL
val HAS_SECRET: KnownValue = KnownValue.withName(HAS_SECRET_RAW, "hasSecret")

const val DIFF_EDITS_RAW: ULong = 20uL
val DIFF_EDITS: KnownValue = KnownValue.withName(DIFF_EDITS_RAW, "edits")

const val VALID_FROM_RAW: ULong = 21uL
val VALID_FROM: KnownValue = KnownValue.withName(VALID_FROM_RAW, "validFrom")

const val VALID_UNTIL_RAW: ULong = 22uL
val VALID_UNTIL: KnownValue = KnownValue.withName(VALID_UNTIL_RAW, "validUntil")

const val POSITION_RAW: ULong = 23uL
val POSITION: KnownValue = KnownValue.withName(POSITION_RAW, "position")

const val NICKNAME_RAW: ULong = 24uL
val NICKNAME: KnownValue = KnownValue.withName(NICKNAME_RAW, "nickname")

const val VALUE_RAW: ULong = 25uL
val VALUE: KnownValue = KnownValue.withName(VALUE_RAW, "value")

const val ATTESTATION_RAW: ULong = 26uL
val ATTESTATION: KnownValue = KnownValue.withName(ATTESTATION_RAW, "attestation")

const val VERIFIABLE_AT_RAW: ULong = 27uL
val VERIFIABLE_AT: KnownValue = KnownValue.withName(VERIFIABLE_AT_RAW, "verifiableAt")

const val ATTACHMENT_RAW: ULong = 50uL
val ATTACHMENT: KnownValue = KnownValue.withName(ATTACHMENT_RAW, "attachment")

const val VENDOR_RAW: ULong = 51uL
val VENDOR: KnownValue = KnownValue.withName(VENDOR_RAW, "vendor")

const val CONFORMS_TO_RAW: ULong = 52uL
val CONFORMS_TO: KnownValue = KnownValue.withName(CONFORMS_TO_RAW, "conformsTo")

const val ALLOW_RAW: ULong = 60uL
val ALLOW: KnownValue = KnownValue.withName(ALLOW_RAW, "allow")

const val DENY_RAW: ULong = 61uL
val DENY: KnownValue = KnownValue.withName(DENY_RAW, "deny")

const val ENDPOINT_RAW: ULong = 62uL
val ENDPOINT: KnownValue = KnownValue.withName(ENDPOINT_RAW, "endpoint")

const val DELEGATE_RAW: ULong = 63uL
val DELEGATE: KnownValue = KnownValue.withName(DELEGATE_RAW, "delegate")

const val PROVENANCE_RAW: ULong = 64uL
val PROVENANCE: KnownValue = KnownValue.withName(PROVENANCE_RAW, "provenance")

const val PRIVATE_KEY_RAW: ULong = 65uL
val PRIVATE_KEY: KnownValue = KnownValue.withName(PRIVATE_KEY_RAW, "privateKey")

const val SERVICE_RAW: ULong = 66uL
val SERVICE: KnownValue = KnownValue.withName(SERVICE_RAW, "service")

const val CAPABILITY_RAW: ULong = 67uL
val CAPABILITY: KnownValue = KnownValue.withName(CAPABILITY_RAW, "capability")

const val PROVENANCE_GENERATOR_RAW: ULong = 68uL
val PROVENANCE_GENERATOR: KnownValue = KnownValue.withName(PROVENANCE_GENERATOR_RAW, "provenanceGenerator")

const val PRIVILEGE_ALL_RAW: ULong = 70uL
val PRIVILEGE_ALL: KnownValue = KnownValue.withName(PRIVILEGE_ALL_RAW, "All")

const val PRIVILEGE_AUTH_RAW: ULong = 71uL
val PRIVILEGE_AUTH: KnownValue = KnownValue.withName(PRIVILEGE_AUTH_RAW, "Authorize")

const val PRIVILEGE_SIGN_RAW: ULong = 72uL
val PRIVILEGE_SIGN: KnownValue = KnownValue.withName(PRIVILEGE_SIGN_RAW, "Sign")

const val PRIVILEGE_ENCRYPT_RAW: ULong = 73uL
val PRIVILEGE_ENCRYPT: KnownValue = KnownValue.withName(PRIVILEGE_ENCRYPT_RAW, "Encrypt")

const val PRIVILEGE_ELIDE_RAW: ULong = 74uL
val PRIVILEGE_ELIDE: KnownValue = KnownValue.withName(PRIVILEGE_ELIDE_RAW, "Elide")

const val PRIVILEGE_ISSUE_RAW: ULong = 75uL
val PRIVILEGE_ISSUE: KnownValue = KnownValue.withName(PRIVILEGE_ISSUE_RAW, "Issue")

const val PRIVILEGE_ACCESS_RAW: ULong = 76uL
val PRIVILEGE_ACCESS: KnownValue = KnownValue.withName(PRIVILEGE_ACCESS_RAW, "Access")

const val PRIVILEGE_DELEGATE_RAW: ULong = 80uL
val PRIVILEGE_DELEGATE: KnownValue = KnownValue.withName(PRIVILEGE_DELEGATE_RAW, "Delegate")

const val PRIVILEGE_VERIFY_RAW: ULong = 81uL
val PRIVILEGE_VERIFY: KnownValue = KnownValue.withName(PRIVILEGE_VERIFY_RAW, "Verify")

const val PRIVILEGE_UPDATE_RAW: ULong = 82uL
val PRIVILEGE_UPDATE: KnownValue = KnownValue.withName(PRIVILEGE_UPDATE_RAW, "Update")

const val PRIVILEGE_TRANSFER_RAW: ULong = 83uL
val PRIVILEGE_TRANSFER: KnownValue = KnownValue.withName(PRIVILEGE_TRANSFER_RAW, "Transfer")

const val PRIVILEGE_ELECT_RAW: ULong = 84uL
val PRIVILEGE_ELECT: KnownValue = KnownValue.withName(PRIVILEGE_ELECT_RAW, "Elect")

const val PRIVILEGE_BURN_RAW: ULong = 85uL
val PRIVILEGE_BURN: KnownValue = KnownValue.withName(PRIVILEGE_BURN_RAW, "Burn")

const val PRIVILEGE_REVOKE_RAW: ULong = 86uL
val PRIVILEGE_REVOKE: KnownValue = KnownValue.withName(PRIVILEGE_REVOKE_RAW, "Revoke")

const val BODY_RAW: ULong = 100uL
val BODY: KnownValue = KnownValue.withName(BODY_RAW, "body")

const val RESULT_RAW: ULong = 101uL
val RESULT: KnownValue = KnownValue.withName(RESULT_RAW, "result")

const val ERROR_RAW: ULong = 102uL
val ERROR: KnownValue = KnownValue.withName(ERROR_RAW, "error")

const val OK_VALUE_RAW: ULong = 103uL
val OK_VALUE: KnownValue = KnownValue.withName(OK_VALUE_RAW, "OK")

const val PROCESSING_VALUE_RAW: ULong = 104uL
val PROCESSING_VALUE: KnownValue = KnownValue.withName(PROCESSING_VALUE_RAW, "Processing")

const val SENDER_RAW: ULong = 105uL
val SENDER: KnownValue = KnownValue.withName(SENDER_RAW, "sender")

const val SENDER_CONTINUATION_RAW: ULong = 106uL
val SENDER_CONTINUATION: KnownValue = KnownValue.withName(SENDER_CONTINUATION_RAW, "senderContinuation")

const val RECIPIENT_CONTINUATION_RAW: ULong = 107uL
val RECIPIENT_CONTINUATION: KnownValue = KnownValue.withName(RECIPIENT_CONTINUATION_RAW, "recipientContinuation")

const val CONTENT_RAW: ULong = 108uL
val CONTENT: KnownValue = KnownValue.withName(CONTENT_RAW, "content")

const val SEED_TYPE_RAW: ULong = 200uL
val SEED_TYPE: KnownValue = KnownValue.withName(SEED_TYPE_RAW, "Seed")

const val PRIVATE_KEY_TYPE_RAW: ULong = 201uL
val PRIVATE_KEY_TYPE: KnownValue = KnownValue.withName(PRIVATE_KEY_TYPE_RAW, "PrivateKey")

const val PUBLIC_KEY_TYPE_RAW: ULong = 202uL
val PUBLIC_KEY_TYPE: KnownValue = KnownValue.withName(PUBLIC_KEY_TYPE_RAW, "PublicKey")

const val MASTER_KEY_TYPE_RAW: ULong = 203uL
val MASTER_KEY_TYPE: KnownValue = KnownValue.withName(MASTER_KEY_TYPE_RAW, "MasterKey")

const val ASSET_RAW: ULong = 300uL
val ASSET: KnownValue = KnownValue.withName(ASSET_RAW, "asset")

const val BITCOIN_VALUE_RAW: ULong = 301uL
val BITCOIN_VALUE: KnownValue = KnownValue.withName(BITCOIN_VALUE_RAW, "Bitcoin")

const val ETHEREUM_VALUE_RAW: ULong = 302uL
val ETHEREUM_VALUE: KnownValue = KnownValue.withName(ETHEREUM_VALUE_RAW, "Ethereum")

const val TEZOS_VALUE_RAW: ULong = 303uL
val TEZOS_VALUE: KnownValue = KnownValue.withName(TEZOS_VALUE_RAW, "Tezos")

const val NETWORK_RAW: ULong = 400uL
val NETWORK: KnownValue = KnownValue.withName(NETWORK_RAW, "network")

const val MAIN_NET_VALUE_RAW: ULong = 401uL
val MAIN_NET_VALUE: KnownValue = KnownValue.withName(MAIN_NET_VALUE_RAW, "MainNet")

const val TEST_NET_VALUE_RAW: ULong = 402uL
val TEST_NET_VALUE: KnownValue = KnownValue.withName(TEST_NET_VALUE_RAW, "TestNet")

const val BIP32_KEY_TYPE_RAW: ULong = 500uL
val BIP32_KEY_TYPE: KnownValue = KnownValue.withName(BIP32_KEY_TYPE_RAW, "BIP32Key")

const val CHAIN_CODE_RAW: ULong = 501uL
val CHAIN_CODE: KnownValue = KnownValue.withName(CHAIN_CODE_RAW, "chainCode")

const val DERIVATION_PATH_TYPE_RAW: ULong = 502uL
val DERIVATION_PATH_TYPE: KnownValue = KnownValue.withName(DERIVATION_PATH_TYPE_RAW, "DerivationPath")

const val PARENT_PATH_RAW: ULong = 503uL
val PARENT_PATH: KnownValue = KnownValue.withName(PARENT_PATH_RAW, "parentPath")

const val CHILDREN_PATH_RAW: ULong = 504uL
val CHILDREN_PATH: KnownValue = KnownValue.withName(CHILDREN_PATH_RAW, "childrenPath")

const val PARENT_FINGERPRINT_RAW: ULong = 505uL
val PARENT_FINGERPRINT: KnownValue = KnownValue.withName(PARENT_FINGERPRINT_RAW, "parentFingerprint")

const val PSBT_TYPE_RAW: ULong = 506uL
val PSBT_TYPE: KnownValue = KnownValue.withName(PSBT_TYPE_RAW, "PSBT")

const val OUTPUT_DESCRIPTOR_TYPE_RAW: ULong = 507uL
val OUTPUT_DESCRIPTOR_TYPE: KnownValue = KnownValue.withName(OUTPUT_DESCRIPTOR_TYPE_RAW, "OutputDescriptor")

const val OUTPUT_DESCRIPTOR_RAW: ULong = 508uL
val OUTPUT_DESCRIPTOR: KnownValue = KnownValue.withName(OUTPUT_DESCRIPTOR_RAW, "outputDescriptor")

const val GRAPH_RAW: ULong = 600uL
val GRAPH: KnownValue = KnownValue.withName(GRAPH_RAW, "Graph")

const val SOURCE_TARGET_GRAPH_RAW: ULong = 601uL
val SOURCE_TARGET_GRAPH: KnownValue = KnownValue.withName(SOURCE_TARGET_GRAPH_RAW, "SourceTargetGraph")

const val PARENT_CHILD_GRAPH_RAW: ULong = 602uL
val PARENT_CHILD_GRAPH: KnownValue = KnownValue.withName(PARENT_CHILD_GRAPH_RAW, "ParentChildGraph")

const val DIGRAPH_RAW: ULong = 603uL
val DIGRAPH: KnownValue = KnownValue.withName(DIGRAPH_RAW, "Digraph")

const val ACYCLIC_GRAPH_RAW: ULong = 604uL
val ACYCLIC_GRAPH: KnownValue = KnownValue.withName(ACYCLIC_GRAPH_RAW, "AcyclicGraph")

const val MULTIGRAPH_RAW: ULong = 605uL
val MULTIGRAPH: KnownValue = KnownValue.withName(MULTIGRAPH_RAW, "Multigraph")

const val PSEUDOGRAPH_RAW: ULong = 606uL
val PSEUDOGRAPH: KnownValue = KnownValue.withName(PSEUDOGRAPH_RAW, "Pseudograph")

const val GRAPH_FRAGMENT_RAW: ULong = 607uL
val GRAPH_FRAGMENT: KnownValue = KnownValue.withName(GRAPH_FRAGMENT_RAW, "GraphFragment")

const val DAG_RAW: ULong = 608uL
val DAG: KnownValue = KnownValue.withName(DAG_RAW, "DAG")

const val TREE_RAW: ULong = 609uL
val TREE: KnownValue = KnownValue.withName(TREE_RAW, "Tree")

const val FOREST_RAW: ULong = 610uL
val FOREST: KnownValue = KnownValue.withName(FOREST_RAW, "Forest")

const val COMPOUND_GRAPH_RAW: ULong = 611uL
val COMPOUND_GRAPH: KnownValue = KnownValue.withName(COMPOUND_GRAPH_RAW, "CompoundGraph")

const val HYPERGRAPH_RAW: ULong = 612uL
val HYPERGRAPH: KnownValue = KnownValue.withName(HYPERGRAPH_RAW, "Hypergraph")

const val DIHYPERGRAPH_RAW: ULong = 613uL
val DIHYPERGRAPH: KnownValue = KnownValue.withName(DIHYPERGRAPH_RAW, "Dihypergraph")

const val NODE_RAW: ULong = 700uL
val NODE: KnownValue = KnownValue.withName(NODE_RAW, "node")

const val EDGE_RAW: ULong = 701uL
val EDGE: KnownValue = KnownValue.withName(EDGE_RAW, "edge")

const val SOURCE_RAW: ULong = 702uL
val SOURCE: KnownValue = KnownValue.withName(SOURCE_RAW, "source")

const val TARGET_RAW: ULong = 703uL
val TARGET: KnownValue = KnownValue.withName(TARGET_RAW, "target")

const val PARENT_RAW: ULong = 704uL
val PARENT: KnownValue = KnownValue.withName(PARENT_RAW, "parent")

const val CHILD_RAW: ULong = 705uL
val CHILD: KnownValue = KnownValue.withName(CHILD_RAW, "child")

const val SELF_RAW: ULong = 706uL
val SELF: KnownValue = KnownValue.withName(SELF_RAW, "Self")

/**
 * Lazily initialized global known-values registry.
 *
 * Thread-safe: the store is created once on first access and then cached.
 * Directory configuration must be set before the first access.
 */
val KNOWN_VALUES: KnownValuesStore by lazy(LazyThreadSafetyMode.SYNCHRONIZED) {
    val store = KnownValuesStore(
        listOf(
            UNIT,
            IS_A,
            ID,
            SIGNED,
            NOTE,
            HAS_RECIPIENT,
            SSKR_SHARE,
            CONTROLLER,
            KEY,
            DEREFERENCE_VIA,
            ENTITY,
            NAME,
            LANGUAGE,
            ISSUER,
            HOLDER,
            SALT,
            DATE,
            UNKNOWN_VALUE,
            VERSION_VALUE,
            HAS_SECRET,
            DIFF_EDITS,
            VALID_FROM,
            VALID_UNTIL,
            POSITION,
            NICKNAME,
            ATTESTATION,
            VERIFIABLE_AT,
            ATTACHMENT,
            VENDOR,
            CONFORMS_TO,
            ALLOW,
            DENY,
            ENDPOINT,
            DELEGATE,
            PROVENANCE,
            PRIVATE_KEY,
            SERVICE,
            CAPABILITY,
            PROVENANCE_GENERATOR,
            PRIVILEGE_ALL,
            PRIVILEGE_AUTH,
            PRIVILEGE_SIGN,
            PRIVILEGE_ENCRYPT,
            PRIVILEGE_ELIDE,
            PRIVILEGE_ISSUE,
            PRIVILEGE_ACCESS,
            PRIVILEGE_DELEGATE,
            PRIVILEGE_VERIFY,
            PRIVILEGE_UPDATE,
            PRIVILEGE_TRANSFER,
            PRIVILEGE_ELECT,
            PRIVILEGE_BURN,
            PRIVILEGE_REVOKE,
            BODY,
            RESULT,
            ERROR,
            OK_VALUE,
            PROCESSING_VALUE,
            SENDER,
            SENDER_CONTINUATION,
            RECIPIENT_CONTINUATION,
            CONTENT,
            SEED_TYPE,
            PRIVATE_KEY_TYPE,
            PUBLIC_KEY_TYPE,
            MASTER_KEY_TYPE,
            ASSET,
            BITCOIN_VALUE,
            ETHEREUM_VALUE,
            TEZOS_VALUE,
            NETWORK,
            MAIN_NET_VALUE,
            TEST_NET_VALUE,
            BIP32_KEY_TYPE,
            CHAIN_CODE,
            DERIVATION_PATH_TYPE,
            PARENT_PATH,
            CHILDREN_PATH,
            PARENT_FINGERPRINT,
            PSBT_TYPE,
            OUTPUT_DESCRIPTOR_TYPE,
            OUTPUT_DESCRIPTOR,
            GRAPH,
            SOURCE_TARGET_GRAPH,
            PARENT_CHILD_GRAPH,
            DIGRAPH,
            ACYCLIC_GRAPH,
            MULTIGRAPH,
            PSEUDOGRAPH,
            GRAPH_FRAGMENT,
            DAG,
            TREE,
            FOREST,
            COMPOUND_GRAPH,
            HYPERGRAPH,
            DIHYPERGRAPH,
            NODE,
            EDGE,
            SOURCE,
            TARGET,
            PARENT,
            CHILD,
        ),
    )

    // Load configured directory entries, overriding hardcoded values.
    val config = getAndLockConfig()
    val result = loadFromConfig(config)
    for (value in result.intoValues()) {
        store.insert(value)
    }

    store
}
