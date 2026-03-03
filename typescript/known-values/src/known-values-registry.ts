/**
 * Known Values Registry — predefined constants and lazy global store.
 *
 * Defines all 104 standard Known Value constants and the lazily initialized
 * global KNOWN_VALUES store.
 */

import { KnownValue } from './known-value.js';
import { KnownValuesStore } from './known-values-store.js';
import { lockConfig } from './config-state.js';

// ---------------------------------------------------------------------------
// Helper to define a known value constant pair
// ---------------------------------------------------------------------------

function kv(value: number, name: string): [bigint, KnownValue] {
    const raw = BigInt(value);
    return [raw, new KnownValue(raw, name)];
}

// ---------------------------------------------------------------------------
// General (0–27)
// ---------------------------------------------------------------------------

export const UNIT_RAW = 0n;
export const UNIT = new KnownValue(UNIT_RAW, '');

const [IS_A_RAW_V, IS_A_V] = kv(1, 'isA');
export const IS_A_RAW = IS_A_RAW_V;
export const IS_A = IS_A_V;

const [ID_RAW_V, ID_V] = kv(2, 'id');
export const ID_RAW = ID_RAW_V;
export const ID = ID_V;

const [SIGNED_RAW_V, SIGNED_V] = kv(3, 'signed');
export const SIGNED_RAW = SIGNED_RAW_V;
export const SIGNED = SIGNED_V;

const [NOTE_RAW_V, NOTE_V] = kv(4, 'note');
export const NOTE_RAW = NOTE_RAW_V;
export const NOTE = NOTE_V;

const [HAS_RECIPIENT_RAW_V, HAS_RECIPIENT_V] = kv(5, 'hasRecipient');
export const HAS_RECIPIENT_RAW = HAS_RECIPIENT_RAW_V;
export const HAS_RECIPIENT = HAS_RECIPIENT_V;

const [SSKR_SHARE_RAW_V, SSKR_SHARE_V] = kv(6, 'sskrShare');
export const SSKR_SHARE_RAW = SSKR_SHARE_RAW_V;
export const SSKR_SHARE = SSKR_SHARE_V;

const [CONTROLLER_RAW_V, CONTROLLER_V] = kv(7, 'controller');
export const CONTROLLER_RAW = CONTROLLER_RAW_V;
export const CONTROLLER = CONTROLLER_V;

const [KEY_RAW_V, KEY_V] = kv(8, 'key');
export const KEY_RAW = KEY_RAW_V;
export const KEY = KEY_V;

const [DEREFERENCE_VIA_RAW_V, DEREFERENCE_VIA_V] = kv(9, 'dereferenceVia');
export const DEREFERENCE_VIA_RAW = DEREFERENCE_VIA_RAW_V;
export const DEREFERENCE_VIA = DEREFERENCE_VIA_V;

const [ENTITY_RAW_V, ENTITY_V] = kv(10, 'entity');
export const ENTITY_RAW = ENTITY_RAW_V;
export const ENTITY = ENTITY_V;

const [NAME_RAW_V, NAME_V] = kv(11, 'name');
export const NAME_RAW = NAME_RAW_V;
export const NAME = NAME_V;

const [LANGUAGE_RAW_V, LANGUAGE_V] = kv(12, 'language');
export const LANGUAGE_RAW = LANGUAGE_RAW_V;
export const LANGUAGE = LANGUAGE_V;

const [ISSUER_RAW_V, ISSUER_V] = kv(13, 'issuer');
export const ISSUER_RAW = ISSUER_RAW_V;
export const ISSUER = ISSUER_V;

const [HOLDER_RAW_V, HOLDER_V] = kv(14, 'holder');
export const HOLDER_RAW = HOLDER_RAW_V;
export const HOLDER = HOLDER_V;

const [SALT_RAW_V, SALT_V] = kv(15, 'salt');
export const SALT_RAW = SALT_RAW_V;
export const SALT = SALT_V;

const [DATE_RAW_V, DATE_V] = kv(16, 'date');
export const DATE_RAW = DATE_RAW_V;
export const DATE = DATE_V;

const [UNKNOWN_VALUE_RAW_V, UNKNOWN_VALUE_V] = kv(17, 'Unknown');
export const UNKNOWN_VALUE_RAW = UNKNOWN_VALUE_RAW_V;
export const UNKNOWN_VALUE = UNKNOWN_VALUE_V;

const [VERSION_VALUE_RAW_V, VERSION_VALUE_V] = kv(18, 'version');
export const VERSION_VALUE_RAW = VERSION_VALUE_RAW_V;
export const VERSION_VALUE = VERSION_VALUE_V;

const [HAS_SECRET_RAW_V, HAS_SECRET_V] = kv(19, 'hasSecret');
export const HAS_SECRET_RAW = HAS_SECRET_RAW_V;
export const HAS_SECRET = HAS_SECRET_V;

const [DIFF_EDITS_RAW_V, DIFF_EDITS_V] = kv(20, 'edits');
export const DIFF_EDITS_RAW = DIFF_EDITS_RAW_V;
export const DIFF_EDITS = DIFF_EDITS_V;

const [VALID_FROM_RAW_V, VALID_FROM_V] = kv(21, 'validFrom');
export const VALID_FROM_RAW = VALID_FROM_RAW_V;
export const VALID_FROM = VALID_FROM_V;

const [VALID_UNTIL_RAW_V, VALID_UNTIL_V] = kv(22, 'validUntil');
export const VALID_UNTIL_RAW = VALID_UNTIL_RAW_V;
export const VALID_UNTIL = VALID_UNTIL_V;

const [POSITION_RAW_V, POSITION_V] = kv(23, 'position');
export const POSITION_RAW = POSITION_RAW_V;
export const POSITION = POSITION_V;

const [NICKNAME_RAW_V, NICKNAME_V] = kv(24, 'nickname');
export const NICKNAME_RAW = NICKNAME_RAW_V;
export const NICKNAME = NICKNAME_V;

const [VALUE_RAW_V, VALUE_V] = kv(25, 'value');
export const VALUE_RAW = VALUE_RAW_V;
export const VALUE = VALUE_V;

const [ATTESTATION_RAW_V, ATTESTATION_V] = kv(26, 'attestation');
export const ATTESTATION_RAW = ATTESTATION_RAW_V;
export const ATTESTATION = ATTESTATION_V;

const [VERIFIABLE_AT_RAW_V, VERIFIABLE_AT_V] = kv(27, 'verifiableAt');
export const VERIFIABLE_AT_RAW = VERIFIABLE_AT_RAW_V;
export const VERIFIABLE_AT = VERIFIABLE_AT_V;

// ---------------------------------------------------------------------------
// Attachments (50–52)
// ---------------------------------------------------------------------------

const [ATTACHMENT_RAW_V, ATTACHMENT_V] = kv(50, 'attachment');
export const ATTACHMENT_RAW = ATTACHMENT_RAW_V;
export const ATTACHMENT = ATTACHMENT_V;

const [VENDOR_RAW_V, VENDOR_V] = kv(51, 'vendor');
export const VENDOR_RAW = VENDOR_RAW_V;
export const VENDOR = VENDOR_V;

const [CONFORMS_TO_RAW_V, CONFORMS_TO_V] = kv(52, 'conformsTo');
export const CONFORMS_TO_RAW = CONFORMS_TO_RAW_V;
export const CONFORMS_TO = CONFORMS_TO_V;

// ---------------------------------------------------------------------------
// XID Documents (60–68)
// ---------------------------------------------------------------------------

const [ALLOW_RAW_V, ALLOW_V] = kv(60, 'allow');
export const ALLOW_RAW = ALLOW_RAW_V;
export const ALLOW = ALLOW_V;

const [DENY_RAW_V, DENY_V] = kv(61, 'deny');
export const DENY_RAW = DENY_RAW_V;
export const DENY = DENY_V;

const [ENDPOINT_RAW_V, ENDPOINT_V] = kv(62, 'endpoint');
export const ENDPOINT_RAW = ENDPOINT_RAW_V;
export const ENDPOINT = ENDPOINT_V;

const [DELEGATE_RAW_V, DELEGATE_V] = kv(63, 'delegate');
export const DELEGATE_RAW = DELEGATE_RAW_V;
export const DELEGATE = DELEGATE_V;

const [PROVENANCE_RAW_V, PROVENANCE_V] = kv(64, 'provenance');
export const PROVENANCE_RAW = PROVENANCE_RAW_V;
export const PROVENANCE = PROVENANCE_V;

const [PRIVATE_KEY_RAW_V, PRIVATE_KEY_V] = kv(65, 'privateKey');
export const PRIVATE_KEY_RAW = PRIVATE_KEY_RAW_V;
export const PRIVATE_KEY = PRIVATE_KEY_V;

const [SERVICE_RAW_V, SERVICE_V] = kv(66, 'service');
export const SERVICE_RAW = SERVICE_RAW_V;
export const SERVICE = SERVICE_V;

const [CAPABILITY_RAW_V, CAPABILITY_V] = kv(67, 'capability');
export const CAPABILITY_RAW = CAPABILITY_RAW_V;
export const CAPABILITY = CAPABILITY_V;

const [PROVENANCE_GENERATOR_RAW_V, PROVENANCE_GENERATOR_V] = kv(68, 'provenanceGenerator');
export const PROVENANCE_GENERATOR_RAW = PROVENANCE_GENERATOR_RAW_V;
export const PROVENANCE_GENERATOR = PROVENANCE_GENERATOR_V;

// ---------------------------------------------------------------------------
// XID Privileges (70–86)
// ---------------------------------------------------------------------------

const [PRIVILEGE_ALL_RAW_V, PRIVILEGE_ALL_V] = kv(70, 'All');
export const PRIVILEGE_ALL_RAW = PRIVILEGE_ALL_RAW_V;
export const PRIVILEGE_ALL = PRIVILEGE_ALL_V;

const [PRIVILEGE_AUTH_RAW_V, PRIVILEGE_AUTH_V] = kv(71, 'Authorize');
export const PRIVILEGE_AUTH_RAW = PRIVILEGE_AUTH_RAW_V;
export const PRIVILEGE_AUTH = PRIVILEGE_AUTH_V;

const [PRIVILEGE_SIGN_RAW_V, PRIVILEGE_SIGN_V] = kv(72, 'Sign');
export const PRIVILEGE_SIGN_RAW = PRIVILEGE_SIGN_RAW_V;
export const PRIVILEGE_SIGN = PRIVILEGE_SIGN_V;

const [PRIVILEGE_ENCRYPT_RAW_V, PRIVILEGE_ENCRYPT_V] = kv(73, 'Encrypt');
export const PRIVILEGE_ENCRYPT_RAW = PRIVILEGE_ENCRYPT_RAW_V;
export const PRIVILEGE_ENCRYPT = PRIVILEGE_ENCRYPT_V;

const [PRIVILEGE_ELIDE_RAW_V, PRIVILEGE_ELIDE_V] = kv(74, 'Elide');
export const PRIVILEGE_ELIDE_RAW = PRIVILEGE_ELIDE_RAW_V;
export const PRIVILEGE_ELIDE = PRIVILEGE_ELIDE_V;

const [PRIVILEGE_ISSUE_RAW_V, PRIVILEGE_ISSUE_V] = kv(75, 'Issue');
export const PRIVILEGE_ISSUE_RAW = PRIVILEGE_ISSUE_RAW_V;
export const PRIVILEGE_ISSUE = PRIVILEGE_ISSUE_V;

const [PRIVILEGE_ACCESS_RAW_V, PRIVILEGE_ACCESS_V] = kv(76, 'Access');
export const PRIVILEGE_ACCESS_RAW = PRIVILEGE_ACCESS_RAW_V;
export const PRIVILEGE_ACCESS = PRIVILEGE_ACCESS_V;

const [PRIVILEGE_DELEGATE_RAW_V, PRIVILEGE_DELEGATE_V] = kv(80, 'Delegate');
export const PRIVILEGE_DELEGATE_RAW = PRIVILEGE_DELEGATE_RAW_V;
export const PRIVILEGE_DELEGATE = PRIVILEGE_DELEGATE_V;

const [PRIVILEGE_VERIFY_RAW_V, PRIVILEGE_VERIFY_V] = kv(81, 'Verify');
export const PRIVILEGE_VERIFY_RAW = PRIVILEGE_VERIFY_RAW_V;
export const PRIVILEGE_VERIFY = PRIVILEGE_VERIFY_V;

const [PRIVILEGE_UPDATE_RAW_V, PRIVILEGE_UPDATE_V] = kv(82, 'Update');
export const PRIVILEGE_UPDATE_RAW = PRIVILEGE_UPDATE_RAW_V;
export const PRIVILEGE_UPDATE = PRIVILEGE_UPDATE_V;

const [PRIVILEGE_TRANSFER_RAW_V, PRIVILEGE_TRANSFER_V] = kv(83, 'Transfer');
export const PRIVILEGE_TRANSFER_RAW = PRIVILEGE_TRANSFER_RAW_V;
export const PRIVILEGE_TRANSFER = PRIVILEGE_TRANSFER_V;

const [PRIVILEGE_ELECT_RAW_V, PRIVILEGE_ELECT_V] = kv(84, 'Elect');
export const PRIVILEGE_ELECT_RAW = PRIVILEGE_ELECT_RAW_V;
export const PRIVILEGE_ELECT = PRIVILEGE_ELECT_V;

const [PRIVILEGE_BURN_RAW_V, PRIVILEGE_BURN_V] = kv(85, 'Burn');
export const PRIVILEGE_BURN_RAW = PRIVILEGE_BURN_RAW_V;
export const PRIVILEGE_BURN = PRIVILEGE_BURN_V;

const [PRIVILEGE_REVOKE_RAW_V, PRIVILEGE_REVOKE_V] = kv(86, 'Revoke');
export const PRIVILEGE_REVOKE_RAW = PRIVILEGE_REVOKE_RAW_V;
export const PRIVILEGE_REVOKE = PRIVILEGE_REVOKE_V;

// ---------------------------------------------------------------------------
// Expression and Function Calls (100–108)
// ---------------------------------------------------------------------------

const [BODY_RAW_V, BODY_V] = kv(100, 'body');
export const BODY_RAW = BODY_RAW_V;
export const BODY = BODY_V;

const [RESULT_RAW_V, RESULT_V] = kv(101, 'result');
export const RESULT_RAW = RESULT_RAW_V;
export const RESULT = RESULT_V;

const [ERROR_RAW_V, ERROR_V] = kv(102, 'error');
export const ERROR_RAW = ERROR_RAW_V;
export const ERROR = ERROR_V;

const [OK_VALUE_RAW_V, OK_VALUE_V] = kv(103, 'OK');
export const OK_VALUE_RAW = OK_VALUE_RAW_V;
export const OK_VALUE = OK_VALUE_V;

const [PROCESSING_VALUE_RAW_V, PROCESSING_VALUE_V] = kv(104, 'Processing');
export const PROCESSING_VALUE_RAW = PROCESSING_VALUE_RAW_V;
export const PROCESSING_VALUE = PROCESSING_VALUE_V;

const [SENDER_RAW_V, SENDER_V] = kv(105, 'sender');
export const SENDER_RAW = SENDER_RAW_V;
export const SENDER = SENDER_V;

const [SENDER_CONTINUATION_RAW_V, SENDER_CONTINUATION_V] = kv(106, 'senderContinuation');
export const SENDER_CONTINUATION_RAW = SENDER_CONTINUATION_RAW_V;
export const SENDER_CONTINUATION = SENDER_CONTINUATION_V;

const [RECIPIENT_CONTINUATION_RAW_V, RECIPIENT_CONTINUATION_V] = kv(107, 'recipientContinuation');
export const RECIPIENT_CONTINUATION_RAW = RECIPIENT_CONTINUATION_RAW_V;
export const RECIPIENT_CONTINUATION = RECIPIENT_CONTINUATION_V;

const [CONTENT_RAW_V, CONTENT_V] = kv(108, 'content');
export const CONTENT_RAW = CONTENT_RAW_V;
export const CONTENT = CONTENT_V;

// ---------------------------------------------------------------------------
// Cryptography (200–203)
// ---------------------------------------------------------------------------

const [SEED_TYPE_RAW_V, SEED_TYPE_V] = kv(200, 'Seed');
export const SEED_TYPE_RAW = SEED_TYPE_RAW_V;
export const SEED_TYPE = SEED_TYPE_V;

const [PRIVATE_KEY_TYPE_RAW_V, PRIVATE_KEY_TYPE_V] = kv(201, 'PrivateKey');
export const PRIVATE_KEY_TYPE_RAW = PRIVATE_KEY_TYPE_RAW_V;
export const PRIVATE_KEY_TYPE = PRIVATE_KEY_TYPE_V;

const [PUBLIC_KEY_TYPE_RAW_V, PUBLIC_KEY_TYPE_V] = kv(202, 'PublicKey');
export const PUBLIC_KEY_TYPE_RAW = PUBLIC_KEY_TYPE_RAW_V;
export const PUBLIC_KEY_TYPE = PUBLIC_KEY_TYPE_V;

const [MASTER_KEY_TYPE_RAW_V, MASTER_KEY_TYPE_V] = kv(203, 'MasterKey');
export const MASTER_KEY_TYPE_RAW = MASTER_KEY_TYPE_RAW_V;
export const MASTER_KEY_TYPE = MASTER_KEY_TYPE_V;

// ---------------------------------------------------------------------------
// Cryptocurrency Assets (300–303)
// ---------------------------------------------------------------------------

const [ASSET_RAW_V, ASSET_V] = kv(300, 'asset');
export const ASSET_RAW = ASSET_RAW_V;
export const ASSET = ASSET_V;

const [BITCOIN_VALUE_RAW_V, BITCOIN_VALUE_V] = kv(301, 'Bitcoin');
export const BITCOIN_VALUE_RAW = BITCOIN_VALUE_RAW_V;
export const BITCOIN_VALUE = BITCOIN_VALUE_V;

const [ETHEREUM_VALUE_RAW_V, ETHEREUM_VALUE_V] = kv(302, 'Ethereum');
export const ETHEREUM_VALUE_RAW = ETHEREUM_VALUE_RAW_V;
export const ETHEREUM_VALUE = ETHEREUM_VALUE_V;

const [TEZOS_VALUE_RAW_V, TEZOS_VALUE_V] = kv(303, 'Tezos');
export const TEZOS_VALUE_RAW = TEZOS_VALUE_RAW_V;
export const TEZOS_VALUE = TEZOS_VALUE_V;

// ---------------------------------------------------------------------------
// Cryptocurrency Networks (400–402)
// ---------------------------------------------------------------------------

const [NETWORK_RAW_V, NETWORK_V] = kv(400, 'network');
export const NETWORK_RAW = NETWORK_RAW_V;
export const NETWORK = NETWORK_V;

const [MAIN_NET_VALUE_RAW_V, MAIN_NET_VALUE_V] = kv(401, 'MainNet');
export const MAIN_NET_VALUE_RAW = MAIN_NET_VALUE_RAW_V;
export const MAIN_NET_VALUE = MAIN_NET_VALUE_V;

const [TEST_NET_VALUE_RAW_V, TEST_NET_VALUE_V] = kv(402, 'TestNet');
export const TEST_NET_VALUE_RAW = TEST_NET_VALUE_RAW_V;
export const TEST_NET_VALUE = TEST_NET_VALUE_V;

// ---------------------------------------------------------------------------
// Bitcoin (500–508)
// ---------------------------------------------------------------------------

const [BIP32_KEY_TYPE_RAW_V, BIP32_KEY_TYPE_V] = kv(500, 'BIP32Key');
export const BIP32_KEY_TYPE_RAW = BIP32_KEY_TYPE_RAW_V;
export const BIP32_KEY_TYPE = BIP32_KEY_TYPE_V;

const [CHAIN_CODE_RAW_V, CHAIN_CODE_V] = kv(501, 'chainCode');
export const CHAIN_CODE_RAW = CHAIN_CODE_RAW_V;
export const CHAIN_CODE = CHAIN_CODE_V;

const [DERIVATION_PATH_TYPE_RAW_V, DERIVATION_PATH_TYPE_V] = kv(502, 'DerivationPath');
export const DERIVATION_PATH_TYPE_RAW = DERIVATION_PATH_TYPE_RAW_V;
export const DERIVATION_PATH_TYPE = DERIVATION_PATH_TYPE_V;

const [PARENT_PATH_RAW_V, PARENT_PATH_V] = kv(503, 'parentPath');
export const PARENT_PATH_RAW = PARENT_PATH_RAW_V;
export const PARENT_PATH = PARENT_PATH_V;

const [CHILDREN_PATH_RAW_V, CHILDREN_PATH_V] = kv(504, 'childrenPath');
export const CHILDREN_PATH_RAW = CHILDREN_PATH_RAW_V;
export const CHILDREN_PATH = CHILDREN_PATH_V;

const [PARENT_FINGERPRINT_RAW_V, PARENT_FINGERPRINT_V] = kv(505, 'parentFingerprint');
export const PARENT_FINGERPRINT_RAW = PARENT_FINGERPRINT_RAW_V;
export const PARENT_FINGERPRINT = PARENT_FINGERPRINT_V;

const [PSBT_TYPE_RAW_V, PSBT_TYPE_V] = kv(506, 'PSBT');
export const PSBT_TYPE_RAW = PSBT_TYPE_RAW_V;
export const PSBT_TYPE = PSBT_TYPE_V;

const [OUTPUT_DESCRIPTOR_TYPE_RAW_V, OUTPUT_DESCRIPTOR_TYPE_V] = kv(507, 'OutputDescriptor');
export const OUTPUT_DESCRIPTOR_TYPE_RAW = OUTPUT_DESCRIPTOR_TYPE_RAW_V;
export const OUTPUT_DESCRIPTOR_TYPE = OUTPUT_DESCRIPTOR_TYPE_V;

const [OUTPUT_DESCRIPTOR_RAW_V, OUTPUT_DESCRIPTOR_V] = kv(508, 'outputDescriptor');
export const OUTPUT_DESCRIPTOR_RAW = OUTPUT_DESCRIPTOR_RAW_V;
export const OUTPUT_DESCRIPTOR = OUTPUT_DESCRIPTOR_V;

// ---------------------------------------------------------------------------
// Graphs (600–613, 700–706)
// ---------------------------------------------------------------------------

const [GRAPH_RAW_V, GRAPH_V] = kv(600, 'Graph');
export const GRAPH_RAW = GRAPH_RAW_V;
export const GRAPH = GRAPH_V;

const [SOURCE_TARGET_GRAPH_RAW_V, SOURCE_TARGET_GRAPH_V] = kv(601, 'SourceTargetGraph');
export const SOURCE_TARGET_GRAPH_RAW = SOURCE_TARGET_GRAPH_RAW_V;
export const SOURCE_TARGET_GRAPH = SOURCE_TARGET_GRAPH_V;

const [PARENT_CHILD_GRAPH_RAW_V, PARENT_CHILD_GRAPH_V] = kv(602, 'ParentChildGraph');
export const PARENT_CHILD_GRAPH_RAW = PARENT_CHILD_GRAPH_RAW_V;
export const PARENT_CHILD_GRAPH = PARENT_CHILD_GRAPH_V;

const [DIGRAPH_RAW_V, DIGRAPH_V] = kv(603, 'Digraph');
export const DIGRAPH_RAW = DIGRAPH_RAW_V;
export const DIGRAPH = DIGRAPH_V;

const [ACYCLIC_GRAPH_RAW_V, ACYCLIC_GRAPH_V] = kv(604, 'AcyclicGraph');
export const ACYCLIC_GRAPH_RAW = ACYCLIC_GRAPH_RAW_V;
export const ACYCLIC_GRAPH = ACYCLIC_GRAPH_V;

const [MULTIGRAPH_RAW_V, MULTIGRAPH_V] = kv(605, 'Multigraph');
export const MULTIGRAPH_RAW = MULTIGRAPH_RAW_V;
export const MULTIGRAPH = MULTIGRAPH_V;

const [PSEUDOGRAPH_RAW_V, PSEUDOGRAPH_V] = kv(606, 'Pseudograph');
export const PSEUDOGRAPH_RAW = PSEUDOGRAPH_RAW_V;
export const PSEUDOGRAPH = PSEUDOGRAPH_V;

const [GRAPH_FRAGMENT_RAW_V, GRAPH_FRAGMENT_V] = kv(607, 'GraphFragment');
export const GRAPH_FRAGMENT_RAW = GRAPH_FRAGMENT_RAW_V;
export const GRAPH_FRAGMENT = GRAPH_FRAGMENT_V;

const [DAG_RAW_V, DAG_V] = kv(608, 'DAG');
export const DAG_RAW = DAG_RAW_V;
export const DAG = DAG_V;

const [TREE_RAW_V, TREE_V] = kv(609, 'Tree');
export const TREE_RAW = TREE_RAW_V;
export const TREE = TREE_V;

const [FOREST_RAW_V, FOREST_V] = kv(610, 'Forest');
export const FOREST_RAW = FOREST_RAW_V;
export const FOREST = FOREST_V;

const [COMPOUND_GRAPH_RAW_V, COMPOUND_GRAPH_V] = kv(611, 'CompoundGraph');
export const COMPOUND_GRAPH_RAW = COMPOUND_GRAPH_RAW_V;
export const COMPOUND_GRAPH = COMPOUND_GRAPH_V;

const [HYPERGRAPH_RAW_V, HYPERGRAPH_V] = kv(612, 'Hypergraph');
export const HYPERGRAPH_RAW = HYPERGRAPH_RAW_V;
export const HYPERGRAPH = HYPERGRAPH_V;

const [DIHYPERGRAPH_RAW_V, DIHYPERGRAPH_V] = kv(613, 'Dihypergraph');
export const DIHYPERGRAPH_RAW = DIHYPERGRAPH_RAW_V;
export const DIHYPERGRAPH = DIHYPERGRAPH_V;

const [NODE_RAW_V, NODE_V] = kv(700, 'node');
export const NODE_RAW = NODE_RAW_V;
export const NODE = NODE_V;

const [EDGE_RAW_V, EDGE_V] = kv(701, 'edge');
export const EDGE_RAW = EDGE_RAW_V;
export const EDGE = EDGE_V;

const [SOURCE_RAW_V, SOURCE_V] = kv(702, 'source');
export const SOURCE_RAW = SOURCE_RAW_V;
export const SOURCE = SOURCE_V;

const [TARGET_RAW_V, TARGET_V] = kv(703, 'target');
export const TARGET_RAW = TARGET_RAW_V;
export const TARGET = TARGET_V;

const [PARENT_RAW_V, PARENT_V] = kv(704, 'parent');
export const PARENT_RAW = PARENT_RAW_V;
export const PARENT = PARENT_V;

const [CHILD_RAW_V, CHILD_V] = kv(705, 'child');
export const CHILD_RAW = CHILD_RAW_V;
export const CHILD = CHILD_V;

const [SELF_RAW_V, SELF_V] = kv(706, 'Self');
export const SELF_RAW = SELF_RAW_V;
export const SELF = SELF_V;

// ---------------------------------------------------------------------------
// Lazy Global Store
// ---------------------------------------------------------------------------

/**
 * The 102 known values included in the global store initialization.
 *
 * This intentionally omits VALUE and SELF, matching the Rust source behavior.
 */
const INITIAL_KNOWN_VALUES: KnownValue[] = [
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
];

let _knownValues: KnownValuesStore | undefined;

/**
 * Gets the global KnownValuesStore, initializing it if necessary.
 *
 * On first access, the store is initialized with 102 hardcoded known values
 * and the configuration is locked.
 */
function initKnownValues(): KnownValuesStore {
    if (_knownValues === undefined) {
        _knownValues = new KnownValuesStore(INITIAL_KNOWN_VALUES);
        lockConfig();
    }
    return _knownValues;
}

/**
 * The global registry of Known Values.
 *
 * Lazily initialized on first property access. Contains all standard Known
 * Values defined in the registry specification.
 *
 * Usage: `KNOWN_VALUES.name(kv)` — the underlying store is created
 * automatically on first access, after which the directory configuration
 * is locked.
 */
// eslint-disable-next-line @typescript-eslint/naming-convention
export const KNOWN_VALUES: KnownValuesStore = new Proxy(
    {} as KnownValuesStore,
    {
        get(_target, prop, receiver) {
            const store = initKnownValues();
            const value = Reflect.get(store, prop, receiver);
            return typeof value === 'function' ? value.bind(store) : value;
        },
    },
);
