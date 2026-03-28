"""Edge tests for bc-envelope.

Translated from rust/bc-envelope/tests/edge_tests.rs
All 44 tests covering edge construction, validation, accessors,
container operations, matching, signed edges, and coexistence.
"""

from textwrap import dedent

from bc_components import URI
from bc_envelope import (
    Edges,
    Envelope,
    EdgeDuplicateIsA,
    EdgeDuplicateSource,
    EdgeDuplicateTarget,
    EdgeMissingIsA,
    EdgeMissingSource,
    EdgeMissingTarget,
    EdgeUnexpectedAssertion,
)
import known_values

from tests.common.test_data import alice_private_keys


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def make_edge(subject: str, is_a: str, source: Envelope, target: Envelope) -> Envelope:
    """Create a basic edge envelope with the three required assertions."""
    return (
        Envelope(subject)
        .add_assertion(known_values.IS_A, is_a)
        .add_assertion(known_values.SOURCE, source)
        .add_assertion(known_values.TARGET, target)
    )


def xid_like(name: str) -> Envelope:
    """Create an XID-like identifier envelope."""
    return Envelope(name)


# -------------------------------------------------------------------
# Edge construction and format
# -------------------------------------------------------------------

def test_edge_basic_format():
    alice = xid_like("Alice")
    edge = make_edge("credential-1", "foaf:Person", alice, alice)

    expected = dedent("""\
        "credential-1" [
            'isA': "foaf:Person"
            'source': "Alice"
            'target': "Alice"
        ]""")
    assert edge.format() == expected


def test_edge_relationship_format():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge = make_edge("knows-bob", "schema:colleague", alice, bob)

    expected = dedent("""\
        "knows-bob" [
            'isA': "schema:colleague"
            'source': "Alice"
            'target': "Bob"
        ]""")
    assert edge.format() == expected


# -------------------------------------------------------------------
# Edge validation
# -------------------------------------------------------------------

def test_validate_edge_valid():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    edge.validate_edge()  # should not raise


def test_validate_edge_missing_is_a():
    alice = xid_like("Alice")
    edge = (
        Envelope("cred-1")
        .add_assertion(known_values.SOURCE, alice)
        .add_assertion(known_values.TARGET, alice)
    )
    try:
        edge.validate_edge()
        assert False, "Expected EdgeMissingIsA"
    except EdgeMissingIsA:
        pass


def test_validate_edge_missing_source():
    alice = xid_like("Alice")
    edge = (
        Envelope("cred-1")
        .add_assertion(known_values.IS_A, "foaf:Person")
        .add_assertion(known_values.TARGET, alice)
    )
    try:
        edge.validate_edge()
        assert False, "Expected EdgeMissingSource"
    except EdgeMissingSource:
        pass


def test_validate_edge_missing_target():
    alice = xid_like("Alice")
    edge = (
        Envelope("cred-1")
        .add_assertion(known_values.IS_A, "foaf:Person")
        .add_assertion(known_values.SOURCE, alice)
    )
    try:
        edge.validate_edge()
        assert False, "Expected EdgeMissingTarget"
    except EdgeMissingTarget:
        pass


def test_validate_edge_no_assertions():
    edge = Envelope("cred-1")
    try:
        edge.validate_edge()
        assert False, "Expected EdgeMissingIsA"
    except EdgeMissingIsA:
        pass


def test_validate_edge_duplicate_is_a():
    alice = xid_like("Alice")
    edge = (
        Envelope("cred-1")
        .add_assertion(known_values.IS_A, "foaf:Person")
        .add_assertion(known_values.IS_A, "schema:Thing")
        .add_assertion(known_values.SOURCE, alice)
        .add_assertion(known_values.TARGET, alice)
    )
    try:
        edge.validate_edge()
        assert False, "Expected EdgeDuplicateIsA"
    except EdgeDuplicateIsA:
        pass


def test_validate_edge_duplicate_source():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge = (
        Envelope("cred-1")
        .add_assertion(known_values.IS_A, "foaf:Person")
        .add_assertion(known_values.SOURCE, alice)
        .add_assertion(known_values.SOURCE, bob)
        .add_assertion(known_values.TARGET, alice)
    )
    try:
        edge.validate_edge()
        assert False, "Expected EdgeDuplicateSource"
    except EdgeDuplicateSource:
        pass


def test_validate_edge_duplicate_target():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge = (
        Envelope("cred-1")
        .add_assertion(known_values.IS_A, "foaf:Person")
        .add_assertion(known_values.SOURCE, alice)
        .add_assertion(known_values.TARGET, alice)
        .add_assertion(known_values.TARGET, bob)
    )
    try:
        edge.validate_edge()
        assert False, "Expected EdgeDuplicateTarget"
    except EdgeDuplicateTarget:
        pass


def test_validate_edge_wrapped_signed():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    signed_edge = edge.wrap().add_signature(alice_private_keys())
    signed_edge.validate_edge()  # should not raise


# -------------------------------------------------------------------
# Edge accessor methods
# -------------------------------------------------------------------

def test_edge_is_a():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    is_a = edge.edge_is_a()
    assert is_a.format() == '"foaf:Person"'


def test_edge_source():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    source = edge.edge_source()
    assert source.format() == '"Alice"'


def test_edge_target():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge = make_edge("knows-bob", "schema:colleague", alice, bob)
    target = edge.edge_target()
    assert target.format() == '"Bob"'


def test_edge_subject():
    alice = xid_like("Alice")
    edge = make_edge("my-credential", "foaf:Person", alice, alice)
    subject = edge.edge_subject()
    assert subject.format() == '"my-credential"'


def test_edge_accessors_on_signed_edge():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge = make_edge("cred-1", "foaf:Person", alice, bob)
    signed_edge = edge.wrap().add_signature(alice_private_keys())

    assert signed_edge.edge_is_a().format() == '"foaf:Person"'
    assert signed_edge.edge_source().format() == '"Alice"'
    assert signed_edge.edge_target().format() == '"Bob"'
    assert signed_edge.edge_subject().format() == '"cred-1"'


# -------------------------------------------------------------------
# Adding edges to envelopes
# -------------------------------------------------------------------

def test_add_edge_envelope():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    doc = Envelope("Alice").add_edge_envelope(edge)

    expected = dedent("""\
        "Alice" [
            'edge': "cred-1" [
                'isA': "foaf:Person"
                'source': "Alice"
                'target': "Alice"
            ]
        ]""")
    assert doc.format() == expected


def test_add_multiple_edges():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("knows-bob", "schema:colleague", alice, bob)

    doc = Envelope("Alice").add_edge_envelope(edge1).add_edge_envelope(edge2)

    edges = doc.edges()
    assert len(edges) == 2

    formatted = doc.format()
    assert "'edge'" in formatted
    assert '"self-desc"' in formatted
    assert '"knows-bob"' in formatted


# -------------------------------------------------------------------
# Edges retrieval via envelope
# -------------------------------------------------------------------

def test_edges_empty():
    doc = Envelope("Alice")
    edges = doc.edges()
    assert len(edges) == 0


def test_edges_retrieval():
    alice = xid_like("Alice")
    edge1 = make_edge("cred-1", "foaf:Person", alice, alice)
    edge2 = make_edge("cred-2", "schema:Thing", alice, alice)

    doc = Envelope("Alice").add_edge_envelope(edge1).add_edge_envelope(edge2)

    edges = doc.edges()
    assert len(edges) == 2

    for edge in edges:
        edge.validate_edge()


# -------------------------------------------------------------------
# Edges container (add / get / remove / clear / len)
# -------------------------------------------------------------------

def test_edges_container_new_is_empty():
    edges = Edges()
    assert edges.is_empty
    assert len(edges) == 0


def test_edges_container_add_and_get():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    digest = edge.digest()

    edges = Edges()
    edges.add(edge)

    assert not edges.is_empty
    assert len(edges) == 1
    assert edges.get(digest) is not None
    assert edges.get(digest).is_equivalent_to(edge)


def test_edges_container_remove():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    digest = edge.digest()

    edges = Edges()
    edges.add(edge)

    removed = edges.remove(digest)
    assert removed is not None
    assert edges.is_empty


def test_edges_container_remove_nonexistent():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)

    edges = Edges()
    removed = edges.remove(edge.digest())
    assert removed is None


def test_edges_container_clear():
    alice = xid_like("Alice")
    edge1 = make_edge("cred-1", "foaf:Person", alice, alice)
    edge2 = make_edge("cred-2", "schema:Thing", alice, alice)

    edges = Edges()
    edges.add(edge1)
    edges.add(edge2)
    assert len(edges) == 2

    edges.clear()
    assert edges.is_empty
    assert len(edges) == 0


def test_edges_container_iter():
    alice = xid_like("Alice")
    edge1 = make_edge("cred-1", "foaf:Person", alice, alice)
    edge2 = make_edge("cred-2", "schema:Thing", alice, alice)

    edges = Edges()
    edges.add(edge1)
    edges.add(edge2)

    count = sum(1 for _ in edges)
    assert count == 2


# -------------------------------------------------------------------
# Edges container round-trip: add_to_envelope / from_envelope
# -------------------------------------------------------------------

def test_edges_container_roundtrip():
    alice = xid_like("Alice")
    edge1 = make_edge("cred-1", "foaf:Person", alice, alice)
    edge2 = make_edge("cred-2", "schema:Thing", alice, alice)

    edges = Edges()
    edges.add(edge1)
    edges.add(edge2)

    doc = Envelope("Alice")
    doc_with_edges = edges.add_to_envelope(doc)

    recovered = Edges.from_envelope(doc_with_edges)
    assert len(recovered) == 2
    assert recovered.get(edge1.digest()) is not None
    assert recovered.get(edge2.digest()) is not None


def test_edges_container_roundtrip_empty():
    edges = Edges()
    doc = Envelope("Alice")
    doc_with_edges = edges.add_to_envelope(doc)

    recovered = Edges.from_envelope(doc_with_edges)
    assert recovered.is_empty


def test_edges_container_roundtrip_preserves_format():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge = make_edge("knows-bob", "schema:colleague", alice, bob)

    edges = Edges()
    edges.add(edge)

    doc = edges.add_to_envelope(Envelope("Alice"))

    expected = dedent("""\
        "Alice" [
            'edge': "knows-bob" [
                'isA': "schema:colleague"
                'source': "Alice"
                'target': "Bob"
            ]
        ]""")
    assert doc.format() == expected

    recovered = Edges.from_envelope(doc)
    assert len(recovered) == 1


# -------------------------------------------------------------------
# Edgeable trait (tested via Edges container directly)
# -------------------------------------------------------------------

def test_edgeable_default_methods():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    digest = edge.digest()

    edges = Edges()
    edges.add(edge)

    assert not edges.is_empty
    assert len(edges) == 1
    assert edges.get(digest) is not None

    removed = edges.remove(digest)
    assert removed is not None
    assert edges.is_empty


# -------------------------------------------------------------------
# edges_matching — filtering by criteria
# -------------------------------------------------------------------

def test_edges_matching_no_filters():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("knows-bob", "schema:colleague", alice, bob)

    doc = Envelope("Alice").add_edge_envelope(edge1).add_edge_envelope(edge2)

    matching = doc.edges_matching(None, None, None, None)
    assert len(matching) == 2


def test_edges_matching_by_is_a():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("knows-bob", "schema:colleague", alice, bob)
    edge3 = make_edge("self-thing", "foaf:Person", alice, alice)

    doc = (
        Envelope("Alice")
        .add_edge_envelope(edge1)
        .add_edge_envelope(edge2)
        .add_edge_envelope(edge3)
    )

    is_a_person = Envelope("foaf:Person")
    matching = doc.edges_matching(is_a_person, None, None, None)
    assert len(matching) == 2

    is_a_colleague = Envelope("schema:colleague")
    matching = doc.edges_matching(is_a_colleague, None, None, None)
    assert len(matching) == 1

    is_a_none = Envelope("nonexistent")
    matching = doc.edges_matching(is_a_none, None, None, None)
    assert len(matching) == 0


def test_edges_matching_by_source():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("alice-claim", "foaf:Person", alice, alice)
    edge2 = make_edge("bob-claim", "foaf:Person", bob, alice)

    doc = Envelope("Alice").add_edge_envelope(edge1).add_edge_envelope(edge2)

    matching = doc.edges_matching(None, alice, None, None)
    assert len(matching) == 1

    matching = doc.edges_matching(None, bob, None, None)
    assert len(matching) == 1

    carol = xid_like("Carol")
    matching = doc.edges_matching(None, carol, None, None)
    assert len(matching) == 0


def test_edges_matching_by_target():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("knows-bob", "schema:colleague", alice, bob)

    doc = Envelope("Alice").add_edge_envelope(edge1).add_edge_envelope(edge2)

    matching = doc.edges_matching(None, None, alice, None)
    assert len(matching) == 1

    matching = doc.edges_matching(None, None, bob, None)
    assert len(matching) == 1


def test_edges_matching_by_subject():
    alice = xid_like("Alice")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("cred-2", "schema:Thing", alice, alice)

    doc = Envelope("Alice").add_edge_envelope(edge1).add_edge_envelope(edge2)

    subject_filter = Envelope("self-desc")
    matching = doc.edges_matching(None, None, None, subject_filter)
    assert len(matching) == 1

    subject_filter = Envelope("nonexistent")
    matching = doc.edges_matching(None, None, None, subject_filter)
    assert len(matching) == 0


def test_edges_matching_combined_filters():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("self-thing", "foaf:Person", alice, alice)
    edge3 = make_edge("knows-bob", "foaf:Person", alice, bob)

    doc = (
        Envelope("Alice")
        .add_edge_envelope(edge1)
        .add_edge_envelope(edge2)
        .add_edge_envelope(edge3)
    )

    is_a = Envelope("foaf:Person")
    matching = doc.edges_matching(is_a, None, None, None)
    assert len(matching) == 3

    # foaf:Person + target Alice => 2 (self-desc, self-thing)
    matching = doc.edges_matching(is_a, None, alice, None)
    assert len(matching) == 2

    # foaf:Person + target Bob => 1 (knows-bob)
    matching = doc.edges_matching(is_a, None, bob, None)
    assert len(matching) == 1

    # foaf:Person + target Alice + subject "self-desc" => 1
    subj = Envelope("self-desc")
    matching = doc.edges_matching(is_a, None, alice, subj)
    assert len(matching) == 1

    # foaf:Person + source Alice + target Bob + subject "knows-bob" => 1
    subj = Envelope("knows-bob")
    matching = doc.edges_matching(is_a, alice, bob, subj)
    assert len(matching) == 1

    # All filters that match nothing
    subj = Envelope("nonexistent")
    matching = doc.edges_matching(is_a, alice, alice, subj)
    assert len(matching) == 0


# -------------------------------------------------------------------
# Signed edges with format verification
# -------------------------------------------------------------------

def test_signed_edge_format():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    signed_edge = edge.wrap().add_signature(alice_private_keys())

    expected = dedent("""\
        {
            "cred-1" [
                'isA': "foaf:Person"
                'source': "Alice"
                'target': "Alice"
            ]
        } [
            'signed': Signature
        ]""")
    assert signed_edge.format() == expected


def test_signed_edge_on_document_format():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    signed_edge = edge.wrap().add_signature(alice_private_keys())

    doc = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_edge_envelope(signed_edge)
    )

    formatted = doc.format()
    assert "'edge': {" in formatted
    assert "'signed': Signature" in formatted
    assert "'isA': \"foaf:Person\"" in formatted


# -------------------------------------------------------------------
# Edge coexistence with attachments
# -------------------------------------------------------------------

def test_edges_coexist_with_attachments():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)

    doc = (
        Envelope("Alice")
        .add_attachment(
            "Metadata",
            "com.example",
            "https://example.com/v1",
        )
        .add_edge_envelope(edge)
    )

    assert len(doc.edges()) == 1
    assert len(doc.attachments()) == 1

    formatted = doc.format()
    assert "'edge'" in formatted
    assert "'attachment'" in formatted


# -------------------------------------------------------------------
# Edge UR round-trip
# -------------------------------------------------------------------

def test_edge_ur_roundtrip():
    alice = xid_like("Alice")
    edge = make_edge("cred-1", "foaf:Person", alice, alice)
    doc = Envelope("Alice").add_edge_envelope(edge)

    # Round-trip through CBOR
    cbor = doc.tagged_cbor()
    recovered = Envelope.from_tagged_cbor(cbor)
    assert recovered.is_equivalent_to(doc)

    recovered_edges = recovered.edges()
    assert len(recovered_edges) == 1
    assert recovered_edges[0].is_equivalent_to(edge)


def test_multiple_edges_ur_roundtrip():
    alice = xid_like("Alice")
    bob = xid_like("Bob")
    edge1 = make_edge("self-desc", "foaf:Person", alice, alice)
    edge2 = make_edge("knows-bob", "schema:colleague", alice, bob)
    edge3 = make_edge("project", "schema:CreativeWork", alice, bob)

    doc = (
        Envelope("Alice")
        .add_edge_envelope(edge1)
        .add_edge_envelope(edge2)
        .add_edge_envelope(edge3)
    )

    cbor = doc.tagged_cbor()
    recovered = Envelope.from_tagged_cbor(cbor)
    assert recovered.is_equivalent_to(doc)

    recovered_edges = recovered.edges()
    assert len(recovered_edges) == 3


# -------------------------------------------------------------------
# Edge with extra assertions beyond the required three
# -------------------------------------------------------------------

def test_edge_with_additional_assertions():
    alice = xid_like("Alice")
    bob = xid_like("Bob")

    edge = (
        Envelope("knows-bob")
        .add_assertion(known_values.IS_A, "schema:colleague")
        .add_assertion(known_values.SOURCE, alice)
        .add_assertion(known_values.TARGET, bob)
        .add_assertion("department", "Engineering")
        .add_assertion("since", "2024-01-15")
    )

    try:
        edge.validate_edge()
        assert False, "Expected EdgeUnexpectedAssertion"
    except EdgeUnexpectedAssertion:
        pass


def test_edge_with_claim_detail_on_target():
    alice = xid_like("Alice")
    target = (
        xid_like("Bob")
        .add_assertion("department", "Engineering")
        .add_assertion("since", "2024-01-15")
    )
    edge = make_edge("knows-bob", "schema:colleague", alice, target)
    edge.validate_edge()  # should not raise


def test_edge_with_claim_detail_on_source():
    source = (
        xid_like("Alice")
        .add_assertion(
            known_values.DEREFERENCE_VIA,
            URI("https://example.com/xid/"),
        )
    )
    target = xid_like("Bob")
    edge = make_edge("knows-bob", "schema:colleague", source, target)
    edge.validate_edge()  # should not raise
