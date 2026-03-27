"""Known values registry constants and lazy global store."""

from __future__ import annotations

from threading import Lock

from ._directory_loader import _get_and_lock_config, load_from_config
from ._known_value import KnownValue
from ._known_value_store import KnownValuesStore

_REGISTRY_ENTRIES: tuple[tuple[int, str, str], ...] = (
    (0, "UNIT", ""),
    (1, "IS_A", "isA"),
    (2, "ID", "id"),
    (3, "SIGNED", "signed"),
    (4, "NOTE", "note"),
    (5, "HAS_RECIPIENT", "hasRecipient"),
    (6, "SSKR_SHARE", "sskrShare"),
    (7, "CONTROLLER", "controller"),
    (8, "KEY", "key"),
    (9, "DEREFERENCE_VIA", "dereferenceVia"),
    (10, "ENTITY", "entity"),
    (11, "NAME", "name"),
    (12, "LANGUAGE", "language"),
    (13, "ISSUER", "issuer"),
    (14, "HOLDER", "holder"),
    (15, "SALT", "salt"),
    (16, "DATE", "date"),
    (17, "UNKNOWN_VALUE", "Unknown"),
    (18, "VERSION_VALUE", "version"),
    (19, "HAS_SECRET", "hasSecret"),
    (20, "DIFF_EDITS", "edits"),
    (21, "VALID_FROM", "validFrom"),
    (22, "VALID_UNTIL", "validUntil"),
    (23, "POSITION", "position"),
    (24, "NICKNAME", "nickname"),
    (25, "VALUE", "value"),
    (26, "ATTESTATION", "attestation"),
    (27, "VERIFIABLE_AT", "verifiableAt"),
    (50, "ATTACHMENT", "attachment"),
    (51, "VENDOR", "vendor"),
    (52, "CONFORMS_TO", "conformsTo"),
    (60, "ALLOW", "allow"),
    (61, "DENY", "deny"),
    (62, "ENDPOINT", "endpoint"),
    (63, "DELEGATE", "delegate"),
    (64, "PROVENANCE", "provenance"),
    (65, "PRIVATE_KEY", "privateKey"),
    (66, "SERVICE", "service"),
    (67, "CAPABILITY", "capability"),
    (68, "PROVENANCE_GENERATOR", "provenanceGenerator"),
    (70, "PRIVILEGE_ALL", "All"),
    (71, "PRIVILEGE_AUTH", "Authorize"),
    (72, "PRIVILEGE_SIGN", "Sign"),
    (73, "PRIVILEGE_ENCRYPT", "Encrypt"),
    (74, "PRIVILEGE_ELIDE", "Elide"),
    (75, "PRIVILEGE_ISSUE", "Issue"),
    (76, "PRIVILEGE_ACCESS", "Access"),
    (80, "PRIVILEGE_DELEGATE", "Delegate"),
    (81, "PRIVILEGE_VERIFY", "Verify"),
    (82, "PRIVILEGE_UPDATE", "Update"),
    (83, "PRIVILEGE_TRANSFER", "Transfer"),
    (84, "PRIVILEGE_ELECT", "Elect"),
    (85, "PRIVILEGE_BURN", "Burn"),
    (86, "PRIVILEGE_REVOKE", "Revoke"),
    (100, "BODY", "body"),
    (101, "RESULT", "result"),
    (102, "ERROR", "error"),
    (103, "OK_VALUE", "OK"),
    (104, "PROCESSING_VALUE", "Processing"),
    (105, "SENDER", "sender"),
    (106, "SENDER_CONTINUATION", "senderContinuation"),
    (107, "RECIPIENT_CONTINUATION", "recipientContinuation"),
    (108, "CONTENT", "content"),
    (200, "SEED_TYPE", "Seed"),
    (201, "PRIVATE_KEY_TYPE", "PrivateKey"),
    (202, "PUBLIC_KEY_TYPE", "PublicKey"),
    (203, "MASTER_KEY_TYPE", "MasterKey"),
    (300, "ASSET", "asset"),
    (301, "BITCOIN_VALUE", "Bitcoin"),
    (302, "ETHEREUM_VALUE", "Ethereum"),
    (303, "TEZOS_VALUE", "Tezos"),
    (400, "NETWORK", "network"),
    (401, "MAIN_NET_VALUE", "MainNet"),
    (402, "TEST_NET_VALUE", "TestNet"),
    (500, "BIP32_KEY_TYPE", "BIP32Key"),
    (501, "CHAIN_CODE", "chainCode"),
    (502, "DERIVATION_PATH_TYPE", "DerivationPath"),
    (503, "PARENT_PATH", "parentPath"),
    (504, "CHILDREN_PATH", "childrenPath"),
    (505, "PARENT_FINGERPRINT", "parentFingerprint"),
    (506, "PSBT_TYPE", "PSBT"),
    (507, "OUTPUT_DESCRIPTOR_TYPE", "OutputDescriptor"),
    (508, "OUTPUT_DESCRIPTOR", "outputDescriptor"),
    (600, "GRAPH", "Graph"),
    (601, "SOURCE_TARGET_GRAPH", "SourceTargetGraph"),
    (602, "PARENT_CHILD_GRAPH", "ParentChildGraph"),
    (603, "DIGRAPH", "Digraph"),
    (604, "ACYCLIC_GRAPH", "AcyclicGraph"),
    (605, "MULTIGRAPH", "Multigraph"),
    (606, "PSEUDOGRAPH", "Pseudograph"),
    (607, "GRAPH_FRAGMENT", "GraphFragment"),
    (608, "DAG", "DAG"),
    (609, "TREE", "Tree"),
    (610, "FOREST", "Forest"),
    (611, "COMPOUND_GRAPH", "CompoundGraph"),
    (612, "HYPERGRAPH", "Hypergraph"),
    (613, "DIHYPERGRAPH", "Dihypergraph"),
    (700, "NODE", "node"),
    (701, "EDGE", "edge"),
    (702, "SOURCE", "source"),
    (703, "TARGET", "target"),
    (704, "PARENT", "parent"),
    (705, "CHILD", "child"),
    (706, "SELF", "Self"),
)

_REGISTRY_BASE_NAMES = tuple(name for _, name, _ in _REGISTRY_ENTRIES)
_REGISTRY_RAW_NAMES = tuple(f"{name}_RAW" for name in _REGISTRY_BASE_NAMES)

for raw_value, const_name, display_name in _REGISTRY_ENTRIES:
    globals()[f"{const_name}_RAW"] = raw_value
    globals()[const_name] = KnownValue(raw_value, display_name)

_KNOWN_VALUES_INITIAL_NAMES: tuple[str, ...] = (
    "UNIT",
    "IS_A",
    "ID",
    "SIGNED",
    "NOTE",
    "HAS_RECIPIENT",
    "SSKR_SHARE",
    "CONTROLLER",
    "KEY",
    "DEREFERENCE_VIA",
    "ENTITY",
    "NAME",
    "LANGUAGE",
    "ISSUER",
    "HOLDER",
    "SALT",
    "DATE",
    "UNKNOWN_VALUE",
    "VERSION_VALUE",
    "HAS_SECRET",
    "DIFF_EDITS",
    "VALID_FROM",
    "VALID_UNTIL",
    "POSITION",
    "NICKNAME",
    "ATTESTATION",
    "VERIFIABLE_AT",
    "ATTACHMENT",
    "VENDOR",
    "CONFORMS_TO",
    "ALLOW",
    "DENY",
    "ENDPOINT",
    "DELEGATE",
    "PROVENANCE",
    "PRIVATE_KEY",
    "SERVICE",
    "CAPABILITY",
    "PROVENANCE_GENERATOR",
    "PRIVILEGE_ALL",
    "PRIVILEGE_AUTH",
    "PRIVILEGE_SIGN",
    "PRIVILEGE_ENCRYPT",
    "PRIVILEGE_ELIDE",
    "PRIVILEGE_ISSUE",
    "PRIVILEGE_ACCESS",
    "PRIVILEGE_DELEGATE",
    "PRIVILEGE_VERIFY",
    "PRIVILEGE_UPDATE",
    "PRIVILEGE_TRANSFER",
    "PRIVILEGE_ELECT",
    "PRIVILEGE_BURN",
    "PRIVILEGE_REVOKE",
    "BODY",
    "RESULT",
    "ERROR",
    "OK_VALUE",
    "PROCESSING_VALUE",
    "SENDER",
    "SENDER_CONTINUATION",
    "RECIPIENT_CONTINUATION",
    "CONTENT",
    "SEED_TYPE",
    "PRIVATE_KEY_TYPE",
    "PUBLIC_KEY_TYPE",
    "MASTER_KEY_TYPE",
    "ASSET",
    "BITCOIN_VALUE",
    "ETHEREUM_VALUE",
    "TEZOS_VALUE",
    "NETWORK",
    "MAIN_NET_VALUE",
    "TEST_NET_VALUE",
    "BIP32_KEY_TYPE",
    "CHAIN_CODE",
    "DERIVATION_PATH_TYPE",
    "PARENT_PATH",
    "CHILDREN_PATH",
    "PARENT_FINGERPRINT",
    "PSBT_TYPE",
    "OUTPUT_DESCRIPTOR_TYPE",
    "OUTPUT_DESCRIPTOR",
    "GRAPH",
    "SOURCE_TARGET_GRAPH",
    "PARENT_CHILD_GRAPH",
    "DIGRAPH",
    "ACYCLIC_GRAPH",
    "MULTIGRAPH",
    "PSEUDOGRAPH",
    "GRAPH_FRAGMENT",
    "DAG",
    "TREE",
    "FOREST",
    "COMPOUND_GRAPH",
    "HYPERGRAPH",
    "DIHYPERGRAPH",
    "NODE",
    "EDGE",
    "SOURCE",
    "TARGET",
    "PARENT",
    "CHILD",
)


class LazyKnownValues:
    """A lazily initialized singleton holding the global registry of known values."""

    __slots__ = ("_data", "_lock")

    def __init__(self) -> None:
        self._lock = Lock()
        self._data: KnownValuesStore | None = None

    def get(self) -> KnownValuesStore:
        """Get the global KnownValuesStore, initializing it on first access."""
        store = self._data
        if store is not None:
            return store

        with self._lock:
            if self._data is None:
                store = KnownValuesStore(
                    globals()[name] for name in _KNOWN_VALUES_INITIAL_NAMES
                )
                result = load_from_config(_get_and_lock_config())
                for value in result:
                    store.insert(value)
                self._data = store
            return self._data


KNOWN_VALUES = LazyKnownValues()

__all__ = [
    "KNOWN_VALUES",
    "LazyKnownValues",
    *_REGISTRY_RAW_NAMES,
    *_REGISTRY_BASE_NAMES,
]
