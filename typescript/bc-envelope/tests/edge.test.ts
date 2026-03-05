import { beforeAll, describe, expect, test } from 'vitest';
import { URI } from '@bc/components';
import { DEREFERENCE_VIA, IS_A, SOURCE, TARGET } from '@bc/known-values';

import { Edges, Envelope, EnvelopeError, registerTags } from '../src/index.js';
import { alicePrivateKey } from './test-data.js';
import { expectActualExpected } from './test-helpers.js';

/** Assert that fn throws an EnvelopeError with the given code. */
function expectEdgeError(fn: () => void, code: string): void {
    try {
        fn();
        expect.unreachable('expected an error to be thrown');
    } catch (e) {
        expect(e).toBeInstanceOf(EnvelopeError);
        expect((e as EnvelopeError).code).toBe(code);
    }
}

// Helper to create a basic edge envelope with the three required assertions.
function makeEdge(subject: string, isA: string, source: Envelope, target: Envelope): Envelope {
    return Envelope.from(subject)
        .addAssertion(IS_A, isA)
        .addAssertion(SOURCE, source)
        .addAssertion(TARGET, target);
}

// Helper to create an XID-like identifier envelope.
function xidLike(name: string): Envelope {
    return Envelope.from(name);
}

describe('edge tests', () => {
    beforeAll(() => {
        registerTags();
    });

    // -------------------------------------------------------------------
    // Edge construction and format
    // -------------------------------------------------------------------

    test('edge basic format', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('credential-1', 'foaf:Person', alice, alice);

        // expected-text-output-rubric:
        expectActualExpected(edge.format(), `
            "credential-1" [
                'isA': "foaf:Person"
                'source': "Alice"
                'target': "Alice"
            ]
        `);
    });

    test('edge relationship format', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge = makeEdge('knows-bob', 'schema:colleague', alice, bob);

        // expected-text-output-rubric:
        expectActualExpected(edge.format(), `
            "knows-bob" [
                'isA': "schema:colleague"
                'source': "Alice"
                'target': "Bob"
            ]
        `);
    });

    // -------------------------------------------------------------------
    // Edge validation
    // -------------------------------------------------------------------

    test('validate edge valid', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);
        expect(() => edge.validateEdge()).not.toThrow();
    });

    test('validate edge missing isA', () => {
        const alice = xidLike('Alice');
        const edge = Envelope.from('cred-1')
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, alice);
        expectEdgeError(() => edge.validateEdge(), 'edge-missing-isa');
    });

    test('validate edge missing source', () => {
        const alice = xidLike('Alice');
        const edge = Envelope.from('cred-1')
            .addAssertion(IS_A, 'foaf:Person')
            .addAssertion(TARGET, alice);
        expectEdgeError(() => edge.validateEdge(), 'edge-missing-source');
    });

    test('validate edge missing target', () => {
        const alice = xidLike('Alice');
        const edge = Envelope.from('cred-1')
            .addAssertion(IS_A, 'foaf:Person')
            .addAssertion(SOURCE, alice);
        expectEdgeError(() => edge.validateEdge(), 'edge-missing-target');
    });

    test('validate edge no assertions', () => {
        const edge = Envelope.from('cred-1');
        expectEdgeError(() => edge.validateEdge(), 'edge-missing-isa');
    });

    test('validate edge duplicate isA', () => {
        const alice = xidLike('Alice');
        const edge = Envelope.from('cred-1')
            .addAssertion(IS_A, 'foaf:Person')
            .addAssertion(IS_A, 'schema:Thing')
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, alice);
        expectEdgeError(() => edge.validateEdge(), 'edge-duplicate-isa');
    });

    test('validate edge duplicate source', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge = Envelope.from('cred-1')
            .addAssertion(IS_A, 'foaf:Person')
            .addAssertion(SOURCE, alice)
            .addAssertion(SOURCE, bob)
            .addAssertion(TARGET, alice);
        expectEdgeError(() => edge.validateEdge(), 'edge-duplicate-source');
    });

    test('validate edge duplicate target', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge = Envelope.from('cred-1')
            .addAssertion(IS_A, 'foaf:Person')
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, alice)
            .addAssertion(TARGET, bob);
        expectEdgeError(() => edge.validateEdge(), 'edge-duplicate-target');
    });

    test('validate edge wrapped signed', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        // Wrap and sign the edge
        const signedEdge = edge.wrap().addSignature(alicePrivateKey());

        // Signed (wrapped) edge should still validate
        expect(() => signedEdge.validateEdge()).not.toThrow();
    });

    // -------------------------------------------------------------------
    // Edge accessor methods
    // -------------------------------------------------------------------

    test('edge isA', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const isA = edge.edgeIsA();
        expectActualExpected(isA.format(), '"foaf:Person"');
    });

    test('edge source', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const source = edge.edgeSource();
        expectActualExpected(source.format(), '"Alice"');
    });

    test('edge target', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge = makeEdge('knows-bob', 'schema:colleague', alice, bob);

        const target = edge.edgeTarget();
        expectActualExpected(target.format(), '"Bob"');
    });

    test('edge subject', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('my-credential', 'foaf:Person', alice, alice);

        const subject = edge.edgeSubject();
        expectActualExpected(subject.format(), '"my-credential"');
    });

    test('edge accessors on signed edge', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, bob);

        const signedEdge = edge.wrap().addSignature(alicePrivateKey());

        // Accessors should work through the wrapped/signed layer
        const isA = signedEdge.edgeIsA();
        expectActualExpected(isA.format(), '"foaf:Person"');

        const source = signedEdge.edgeSource();
        expectActualExpected(source.format(), '"Alice"');

        const target = signedEdge.edgeTarget();
        expectActualExpected(target.format(), '"Bob"');

        const subject = signedEdge.edgeSubject();
        expectActualExpected(subject.format(), '"cred-1"');
    });

    // -------------------------------------------------------------------
    // Adding edges to envelopes
    // -------------------------------------------------------------------

    test('add edge envelope', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const doc = Envelope.from('Alice').addEdgeEnvelope(edge);

        // expected-text-output-rubric:
        expectActualExpected(doc.format(), `
            "Alice" [
                'edge': "cred-1" [
                    'isA': "foaf:Person"
                    'source': "Alice"
                    'target': "Alice"
                ]
            ]
        `);
    });

    test('add multiple edges', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('knows-bob', 'schema:colleague', alice, bob);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2);

        const edgeList = doc.edges();
        expect(edgeList.length).toBe(2);

        const formatted = doc.format();
        expect(formatted).toContain("'edge'");
        expect(formatted).toContain('"self-desc"');
        expect(formatted).toContain('"knows-bob"');
    });

    // -------------------------------------------------------------------
    // Edges retrieval via envelope
    // -------------------------------------------------------------------

    test('edges empty', () => {
        const doc = Envelope.from('Alice');
        const edgeList = doc.edges();
        expect(edgeList.length).toBe(0);
    });

    test('edges retrieval', () => {
        const alice = xidLike('Alice');
        const edge1 = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('cred-2', 'schema:Thing', alice, alice);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2);

        const edgeList = doc.edges();
        expect(edgeList.length).toBe(2);

        // Each retrieved edge should be a valid edge
        for (const edge of edgeList) {
            expect(() => edge.validateEdge()).not.toThrow();
        }
    });

    // -------------------------------------------------------------------
    // Edges container (add / get / remove / clear / size)
    // -------------------------------------------------------------------

    test('edges container new is empty', () => {
        const edges = new Edges();
        expect(edges.isEmpty()).toBe(true);
        expect(edges.size).toBe(0);
    });

    test('edges container add and get', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const digest = edge.digest();

        const edges = new Edges();
        edges.add(edge);

        expect(edges.isEmpty()).toBe(false);
        expect(edges.size).toBe(1);
        expect(edges.get(digest)).toBeDefined();
        expect(edges.get(digest)!.isEquivalentTo(edge)).toBe(true);
    });

    test('edges container remove', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const digest = edge.digest();

        const edges = new Edges();
        edges.add(edge);

        const removed = edges.remove(digest);
        expect(removed).toBeDefined();
        expect(edges.isEmpty()).toBe(true);
    });

    test('edges container remove nonexistent', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const edges = new Edges();
        const removed = edges.remove(edge.digest());
        expect(removed).toBeUndefined();
    });

    test('edges container clear', () => {
        const alice = xidLike('Alice');
        const edge1 = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('cred-2', 'schema:Thing', alice, alice);

        const edges = new Edges();
        edges.add(edge1);
        edges.add(edge2);
        expect(edges.size).toBe(2);

        edges.clear();
        expect(edges.isEmpty()).toBe(true);
        expect(edges.size).toBe(0);
    });

    test('edges container iter', () => {
        const alice = xidLike('Alice');
        const edge1 = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('cred-2', 'schema:Thing', alice, alice);

        const edges = new Edges();
        edges.add(edge1);
        edges.add(edge2);

        const count = edges.entries.length;
        expect(count).toBe(2);
    });

    // -------------------------------------------------------------------
    // Edges container round-trip: addToEnvelope / fromEnvelope
    // -------------------------------------------------------------------

    test('edges container roundtrip', () => {
        const alice = xidLike('Alice');
        const edge1 = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('cred-2', 'schema:Thing', alice, alice);

        const edges = new Edges();
        edges.add(edge1);
        edges.add(edge2);

        // Serialize to envelope
        const doc = Envelope.from('Alice');
        const docWithEdges = edges.addToEnvelope(doc);

        // Deserialize back
        const recovered = Edges.fromEnvelope(docWithEdges);
        expect(recovered.size).toBe(2);
        expect(recovered.get(edge1.digest())).toBeDefined();
        expect(recovered.get(edge2.digest())).toBeDefined();
    });

    test('edges container roundtrip empty', () => {
        const edges = new Edges();
        const doc = Envelope.from('Alice');
        const docWithEdges = edges.addToEnvelope(doc);

        const recovered = Edges.fromEnvelope(docWithEdges);
        expect(recovered.isEmpty()).toBe(true);
    });

    test('edges container roundtrip preserves format', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge = makeEdge('knows-bob', 'schema:colleague', alice, bob);

        const edges = new Edges();
        edges.add(edge);

        const doc = edges.addToEnvelope(Envelope.from('Alice'));

        // expected-text-output-rubric:
        expectActualExpected(doc.format(), `
            "Alice" [
                'edge': "knows-bob" [
                    'isA': "schema:colleague"
                    'source': "Alice"
                    'target': "Bob"
                ]
            ]
        `);

        const recovered = Edges.fromEnvelope(doc);
        expect(recovered.size).toBe(1);
    });

    // -------------------------------------------------------------------
    // Edgeable trait
    // -------------------------------------------------------------------

    test('edgeable default methods', () => {
        // Test the Edges container methods directly to verify behavior
        // (mirrors Rust's Edgeable trait test).
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const digest = edge.digest();

        const edges = new Edges();
        edges.add(edge);

        expect(edges.isEmpty()).toBe(false);
        expect(edges.size).toBe(1);
        expect(edges.get(digest)).toBeDefined();

        const removed = edges.remove(digest);
        expect(removed).toBeDefined();
        expect(edges.isEmpty()).toBe(true);
    });

    // -------------------------------------------------------------------
    // edgesMatching — filtering by criteria
    // -------------------------------------------------------------------

    test('edges matching no filters', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('knows-bob', 'schema:colleague', alice, bob);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2);

        // No filters => all edges
        const matching = doc.edgesMatching();
        expect(matching.length).toBe(2);
    });

    test('edges matching by isA', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('knows-bob', 'schema:colleague', alice, bob);
        const edge3 = makeEdge('self-thing', 'foaf:Person', alice, alice);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3);

        const isAPerson = Envelope.from('foaf:Person');
        let matching = doc.edgesMatching(isAPerson);
        expect(matching.length).toBe(2);

        const isAColleague = Envelope.from('schema:colleague');
        matching = doc.edgesMatching(isAColleague);
        expect(matching.length).toBe(1);

        const isANone = Envelope.from('nonexistent');
        matching = doc.edgesMatching(isANone);
        expect(matching.length).toBe(0);
    });

    test('edges matching by source', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('alice-claim', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('bob-claim', 'foaf:Person', bob, alice);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2);

        let matching = doc.edgesMatching(undefined, alice);
        expect(matching.length).toBe(1);

        matching = doc.edgesMatching(undefined, bob);
        expect(matching.length).toBe(1);

        const carol = xidLike('Carol');
        matching = doc.edgesMatching(undefined, carol);
        expect(matching.length).toBe(0);
    });

    test('edges matching by target', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('knows-bob', 'schema:colleague', alice, bob);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2);

        let matching = doc.edgesMatching(undefined, undefined, alice);
        expect(matching.length).toBe(1);

        matching = doc.edgesMatching(undefined, undefined, bob);
        expect(matching.length).toBe(1);
    });

    test('edges matching by subject', () => {
        const alice = xidLike('Alice');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('cred-2', 'schema:Thing', alice, alice);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2);

        const subjectFilter = Envelope.from('self-desc');
        let matching = doc.edgesMatching(undefined, undefined, undefined, subjectFilter);
        expect(matching.length).toBe(1);

        const noMatch = Envelope.from('nonexistent');
        matching = doc.edgesMatching(undefined, undefined, undefined, noMatch);
        expect(matching.length).toBe(0);
    });

    test('edges matching combined filters', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('self-thing', 'foaf:Person', alice, alice);
        const edge3 = makeEdge('knows-bob', 'foaf:Person', alice, bob);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3);

        // All three are foaf:Person
        const isA = Envelope.from('foaf:Person');
        let matching = doc.edgesMatching(isA);
        expect(matching.length).toBe(3);

        // foaf:Person + target Alice => 2 (self-desc, self-thing)
        matching = doc.edgesMatching(isA, undefined, alice);
        expect(matching.length).toBe(2);

        // foaf:Person + target Bob => 1 (knows-bob)
        matching = doc.edgesMatching(isA, undefined, bob);
        expect(matching.length).toBe(1);

        // foaf:Person + target Alice + subject "self-desc" => 1
        let subj = Envelope.from('self-desc');
        matching = doc.edgesMatching(isA, undefined, alice, subj);
        expect(matching.length).toBe(1);

        // foaf:Person + source Alice + target Bob + subject "knows-bob" => 1
        subj = Envelope.from('knows-bob');
        matching = doc.edgesMatching(isA, alice, bob, subj);
        expect(matching.length).toBe(1);

        // All filters that match nothing
        subj = Envelope.from('nonexistent');
        matching = doc.edgesMatching(isA, alice, alice, subj);
        expect(matching.length).toBe(0);
    });

    // -------------------------------------------------------------------
    // Signed edges with format verification
    // -------------------------------------------------------------------

    test('signed edge format', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const signedEdge = edge.wrap().addSignature(alicePrivateKey());

        // expected-text-output-rubric:
        expectActualExpected(signedEdge.format(), `
            {
                "cred-1" [
                    'isA': "foaf:Person"
                    'source': "Alice"
                    'target': "Alice"
                ]
            } [
                'signed': Signature
            ]
        `);
    });

    test('signed edge on document format', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);
        const signedEdge = edge.wrap().addSignature(alicePrivateKey());

        const doc = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addEdgeEnvelope(signedEdge);

        const formatted = doc.format();
        expect(formatted).toContain("'edge': {");
        expect(formatted).toContain("'signed': Signature");
        expect(formatted).toContain("'isA': \"foaf:Person\"");
    });

    // -------------------------------------------------------------------
    // Edge coexistence with attachments
    // -------------------------------------------------------------------

    test('edges coexist with attachments', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const doc = Envelope.from('Alice')
            .addAttachment('Metadata', 'com.example', 'https://example.com/v1')
            .addEdgeEnvelope(edge);

        // Both should be present
        expect(doc.edges().length).toBe(1);
        expect(doc.attachments().length).toBe(1);

        const formatted = doc.format();
        expect(formatted).toContain("'edge'");
        expect(formatted).toContain("'attachment'");
    });

    // -------------------------------------------------------------------
    // Edge UR round-trip
    // -------------------------------------------------------------------

    test('edge UR roundtrip', () => {
        const alice = xidLike('Alice');
        const edge = makeEdge('cred-1', 'foaf:Person', alice, alice);

        const doc = Envelope.from('Alice').addEdgeEnvelope(edge);

        // Round-trip through UR string
        const ur = doc.urString();
        const recovered = Envelope.fromUrString(ur);
        expect(recovered.isEquivalentTo(doc)).toBe(true);

        const recoveredEdges = recovered.edges();
        expect(recoveredEdges.length).toBe(1);
        expect(recoveredEdges[0]!.isEquivalentTo(edge)).toBe(true);
    });

    test('multiple edges UR roundtrip', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');
        const edge1 = makeEdge('self-desc', 'foaf:Person', alice, alice);
        const edge2 = makeEdge('knows-bob', 'schema:colleague', alice, bob);
        const edge3 = makeEdge('project', 'schema:CreativeWork', alice, bob);

        const doc = Envelope.from('Alice')
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3);

        const ur = doc.urString();
        const recovered = Envelope.fromUrString(ur);
        expect(recovered.isEquivalentTo(doc)).toBe(true);

        const recoveredEdges = recovered.edges();
        expect(recoveredEdges.length).toBe(3);
    });

    // -------------------------------------------------------------------
    // Edge with extra assertions beyond the required three
    // -------------------------------------------------------------------

    test('edge with additional assertions', () => {
        const alice = xidLike('Alice');
        const bob = xidLike('Bob');

        // An edge with extra detail assertions beyond isA/source/target
        // should fail validation: only the three required assertions are
        // permitted on the edge subject.
        const edge = Envelope.from('knows-bob')
            .addAssertion(IS_A, 'schema:colleague')
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, bob)
            .addAssertion('department', 'Engineering')
            .addAssertion('since', '2024-01-15');

        expectEdgeError(() => edge.validateEdge(), 'edge-unexpected-assertion');
    });

    test('edge with claim detail on target', () => {
        // Claim detail goes as assertions on the *target* object,
        // not on the edge subject itself.
        const alice = xidLike('Alice');
        const target = xidLike('Bob')
            .addAssertion('department', 'Engineering')
            .addAssertion('since', '2024-01-15');
        const edge = makeEdge('knows-bob', 'schema:colleague', alice, target);
        expect(() => edge.validateEdge()).not.toThrow();
    });

    test('edge with claim detail on source', () => {
        // The source XID may also carry assertions such as 'dereferenceVia'.
        const source = xidLike('Alice').addAssertion(
            DEREFERENCE_VIA,
            new URI('https://example.com/xid/'),
        );
        const target = xidLike('Bob');
        const edge = makeEdge('knows-bob', 'schema:colleague', source, target);
        expect(() => edge.validateEdge()).not.toThrow();
    });
});
