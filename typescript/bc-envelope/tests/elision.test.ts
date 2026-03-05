import { beforeAll, describe, expect, test } from 'vitest';

import { Digest } from '@bc/components';
import { Envelope, EnvelopeError, registerTags } from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import { checkEncoding } from './test-data.js';

function basicEnvelope(): Envelope {
    return Envelope.from('Hello.');
}

function assertionEnvelope(): Envelope {
    return Envelope.newAssertion('knows', 'Bob');
}

function singleAssertionEnvelope(): Envelope {
    return Envelope.from('Alice').addAssertion('knows', 'Bob');
}

function doubleAssertionEnvelope(): Envelope {
    return Envelope.from('Alice')
        .addAssertion('knows', 'Bob')
        .addAssertion('knows', 'Carol');
}

describe('elision tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('elide removing target', () => {
        const envelope = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const elided = envelope.elideRemovingTarget(Envelope.from('Bob'));

        // expected-text-output-rubric:
        expectActualExpected(
            elided.format(),
            `
            "Alice" [
    "knows": ELIDED
            ]
            `,
        );
    });

    test('unelide restores from original', () => {
        const original = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const elided = original.elideRemovingTarget(Envelope.from('Bob'));
        const restored = elided.unelide(original);
        expect(restored.isIdenticalTo(original)).toBe(true);
    });

    test('test envelope elision', () => {
        const e1 = basicEnvelope();

        const e2 = e1.elide();
        expect(e1.isEquivalentTo(e2)).toBe(true);
        expect(e1.isIdenticalTo(e2)).toBe(false);

        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            ELIDED
            `,
        );

        // expected-text-output-rubric:
        expectActualExpected(
            e2.diagnosticAnnotated(),
            `
            200(h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59')   / envelope /
            `,
        );

        const e3 = e2.unelide(e1);
        expect(e3.isEquivalentTo(e1)).toBe(true);

        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            "Hello."
            `,
        );
    });

    test('test single assertion remove elision', () => {
        // The original Envelope
        const e1 = singleAssertionEnvelope();
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
            ]
            `,
        );

        // Elide the entire envelope
        const e2 = checkEncoding(e1.elideRemovingTarget(e1));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            ELIDED
            `,
        );

        // Elide just the envelope's subject
        const e3 = checkEncoding(e1.elideRemovingTarget(Envelope.from('Alice')));
        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            ELIDED [
                "knows": "Bob"
            ]
            `,
        );

        // Elide just the assertion's predicate
        const e4 = checkEncoding(e1.elideRemovingTarget(Envelope.from('knows')));
        // expected-text-output-rubric:
        expectActualExpected(
            e4.format(),
            `
            "Alice" [
                ELIDED: "Bob"
            ]
            `,
        );

        // Elide just the assertion's object
        const e5 = checkEncoding(e1.elideRemovingTarget(Envelope.from('Bob')));
        // expected-text-output-rubric:
        expectActualExpected(
            e5.format(),
            `
            "Alice" [
                "knows": ELIDED
            ]
            `,
        );

        // Elide the entire assertion
        const e6 = checkEncoding(e1.elideRemovingTarget(assertionEnvelope()));
        // expected-text-output-rubric:
        expectActualExpected(
            e6.format(),
            `
            "Alice" [
                ELIDED
            ]
            `,
        );
    });

    test('test double assertion remove elision', () => {
        // The original Envelope
        const e1 = doubleAssertionEnvelope();
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            `,
        );

        // Elide the entire envelope
        const e2 = checkEncoding(e1.elideRemovingTarget(e1));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            ELIDED
            `,
        );

        // Elide just the envelope's subject
        const e3 = checkEncoding(e1.elideRemovingTarget(Envelope.from('Alice')));
        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            ELIDED [
                "knows": "Bob"
                "knows": "Carol"
            ]
            `,
        );

        // Elide just the assertion's predicate
        const e4 = checkEncoding(e1.elideRemovingTarget(Envelope.from('knows')));
        // expected-text-output-rubric:
        expectActualExpected(
            e4.format(),
            `
            "Alice" [
                ELIDED: "Bob"
                ELIDED: "Carol"
            ]
            `,
        );

        // Elide just the assertion's object
        const e5 = checkEncoding(e1.elideRemovingTarget(Envelope.from('Bob')));
        // expected-text-output-rubric:
        expectActualExpected(
            e5.format(),
            `
            "Alice" [
                "knows": "Carol"
                "knows": ELIDED
            ]
            `,
        );

        // Elide the entire assertion
        const e6 = checkEncoding(e1.elideRemovingTarget(assertionEnvelope()));
        // expected-text-output-rubric:
        expectActualExpected(
            e6.format(),
            `
            "Alice" [
                "knows": "Carol"
                ELIDED
            ]
            `,
        );
    });

    test('test single assertion reveal elision', () => {
        // The original Envelope
        const e1 = singleAssertionEnvelope();
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
            ]
            `,
        );

        // Elide revealing nothing
        const e2 = checkEncoding(e1.elideRevealingArray([]));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            ELIDED
            `,
        );

        // Reveal just the envelope's structure
        const e3 = checkEncoding(e1.elideRevealingArray([e1]));
        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            ELIDED [
                ELIDED
            ]
            `,
        );

        // Reveal just the envelope's subject
        const e4 = checkEncoding(e1.elideRevealingArray([e1, Envelope.from('Alice')]));
        // expected-text-output-rubric:
        expectActualExpected(
            e4.format(),
            `
            "Alice" [
                ELIDED
            ]
            `,
        );

        // Reveal just the assertion's structure
        const e5 = checkEncoding(e1.elideRevealingArray([e1, assertionEnvelope()]));
        // expected-text-output-rubric:
        expectActualExpected(
            e5.format(),
            `
            ELIDED [
                ELIDED: ELIDED
            ]
            `,
        );

        // Reveal just the assertion's predicate
        const e6 = checkEncoding(
            e1.elideRevealingArray([e1, assertionEnvelope(), Envelope.from('knows')]),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            e6.format(),
            `
            ELIDED [
                "knows": ELIDED
            ]
            `,
        );

        // Reveal just the assertion's object
        const e7 = checkEncoding(
            e1.elideRevealingArray([e1, assertionEnvelope(), Envelope.from('Bob')]),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            e7.format(),
            `
            ELIDED [
                ELIDED: "Bob"
            ]
            `,
        );
    });

    test('test double assertion reveal elision', () => {
        // The original Envelope
        const e1 = doubleAssertionEnvelope();
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            `,
        );

        // Elide revealing nothing
        const e2 = checkEncoding(e1.elideRevealingArray([]));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            ELIDED
            `,
        );

        // Reveal just the envelope's structure
        const e3 = checkEncoding(e1.elideRevealingArray([e1]));
        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            ELIDED [
                ELIDED (2)
            ]
            `,
        );

        // Reveal just the envelope's subject
        const e4 = checkEncoding(e1.elideRevealingArray([e1, Envelope.from('Alice')]));
        // expected-text-output-rubric:
        expectActualExpected(
            e4.format(),
            `
            "Alice" [
                ELIDED (2)
            ]
            `,
        );

        // Reveal just the assertion's structure
        const e5 = checkEncoding(e1.elideRevealingArray([e1, assertionEnvelope()]));
        // expected-text-output-rubric:
        expectActualExpected(
            e5.format(),
            `
            ELIDED [
                ELIDED: ELIDED
                ELIDED
            ]
            `,
        );

        // Reveal just the assertion's predicate
        const e6 = checkEncoding(
            e1.elideRevealingArray([e1, assertionEnvelope(), Envelope.from('knows')]),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            e6.format(),
            `
            ELIDED [
                "knows": ELIDED
                ELIDED
            ]
            `,
        );

        // Reveal just the assertion's object
        const e7 = checkEncoding(
            e1.elideRevealingArray([e1, assertionEnvelope(), Envelope.from('Bob')]),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            e7.format(),
            `
            ELIDED [
                ELIDED: "Bob"
                ELIDED
            ]
            `,
        );
    });

    test('test digests', () => {
        const e1 = doubleAssertionEnvelope();
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            `,
        );

        const e2 = checkEncoding(e1.elideRevealingSet(e1.digests(0)));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            ELIDED
            `,
        );

        const e3 = checkEncoding(e1.elideRevealingSet(e1.digests(1)));
        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            "Alice" [
                ELIDED (2)
            ]
            `,
        );

        const e4 = checkEncoding(e1.elideRevealingSet(e1.digests(2)));
        // expected-text-output-rubric:
        expectActualExpected(
            e4.format(),
            `
            "Alice" [
                ELIDED: ELIDED
                ELIDED: ELIDED
            ]
            `,
        );

        const e5 = checkEncoding(e1.elideRevealingSet(e1.digests(3)));
        // expected-text-output-rubric:
        expectActualExpected(
            e5.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            `,
        );
    });

    test('test target reveal', () => {
        const e1 = doubleAssertionEnvelope().addAssertion('livesAt', '123 Main St.');
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
                "livesAt": "123 Main St."
            ]
            `,
        );

        const target = new Set<Digest>();
        // Reveal the Envelope structure
        for (const d of e1.digests(1)) {
            target.add(d);
        }
        // Reveal everything about the subject
        for (const d of e1.subject().deepDigests()) {
            target.add(d);
        }
        // Reveal everything about one of the assertions
        for (const d of assertionEnvelope().deepDigests()) {
            target.add(d);
        }
        // Reveal the specific `livesAt` assertion
        for (const d of e1.assertionWithPredicate('livesAt').deepDigests()) {
            target.add(d);
        }
        const e2 = checkEncoding(e1.elideRevealingSet(target));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            "Alice" [
                "knows": "Bob"
                "livesAt": "123 Main St."
                ELIDED
            ]
            `,
        );
    });

    test('test targeted remove', () => {
        const e1 = doubleAssertionEnvelope().addAssertion('livesAt', '123 Main St.');
        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
                "livesAt": "123 Main St."
            ]
            `,
        );

        // Hide one of the assertions
        const target2 = new Set<Digest>();
        for (const d of assertionEnvelope().digests(1)) {
            target2.add(d);
        }
        const e2 = checkEncoding(e1.elideRemovingSet(target2));
        // expected-text-output-rubric:
        expectActualExpected(
            e2.format(),
            `
            "Alice" [
                "knows": "Carol"
                "livesAt": "123 Main St."
                ELIDED
            ]
            `,
        );

        // Hide one of the assertions by finding its predicate
        const target3 = new Set<Digest>();
        for (const d of e1.assertionWithPredicate('livesAt').deepDigests()) {
            target3.add(d);
        }
        const e3 = checkEncoding(e1.elideRemovingSet(target3));
        // expected-text-output-rubric:
        expectActualExpected(
            e3.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
                ELIDED
            ]
            `,
        );

        // Semantically equivalent
        expect(e1.isEquivalentTo(e3)).toBe(true);

        // Structurally different
        expect(e1.isIdenticalTo(e3)).toBe(false);
    });

    test('test walk replace basic', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const charlie = Envelope.from('Charlie');

        const envelope = alice
            .addAssertion('knows', bob)
            .addAssertion('likes', bob);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob"
                "likes": "Bob"
            ]
            `,
        );

        // Replace all instances of Bob with Charlie
        const target = new Set<Digest>();
        target.add(bob.digest());

        const modified = envelope.walkReplace(target, charlie);

        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Alice" [
                "knows": "Charlie"
                "likes": "Charlie"
            ]
            `,
        );

        // The structure is different (different content)
        expect(modified.isEquivalentTo(envelope)).toBe(false);
    });

    test('test walk replace subject', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const carol = Envelope.from('Carol');

        const envelope = alice.addAssertion('knows', bob);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob"
            ]
            `,
        );

        // Replace the subject (Alice) with Carol
        const target = new Set<Digest>();
        target.add(alice.digest());

        const modified = envelope.walkReplace(target, carol);

        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Carol" [
                "knows": "Bob"
            ]
            `,
        );
    });

    test('test walk replace nested', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const charlie = Envelope.from('Charlie');

        // Create a nested structure with Bob appearing at multiple levels
        const inner = bob.addAssertion('friend', bob);
        const envelope = alice.addAssertion('knows', inner);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob" [
                    "friend": "Bob"
                ]
            ]
            `,
        );

        // Replace all instances of Bob with Charlie
        const target = new Set<Digest>();
        target.add(bob.digest());

        const modified = envelope.walkReplace(target, charlie);

        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Alice" [
                "knows": "Charlie" [
                    "friend": "Charlie"
                ]
            ]
            `,
        );
    });

    test('test walk replace wrapped', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const charlie = Envelope.from('Charlie');

        // Create a wrapped envelope containing Bob
        const wrapped = bob.wrap();
        const envelope = alice.addAssertion('data', wrapped);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "data": {
                    "Bob"
                }
            ]
            `,
        );

        // Replace Bob with Charlie
        const target = new Set<Digest>();
        target.add(bob.digest());

        const modified = envelope.walkReplace(target, charlie);

        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Alice" [
                "data": {
                    "Charlie"
                }
            ]
            `,
        );
    });

    test('test walk replace no match', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const charlie = Envelope.from('Charlie');
        const dave = Envelope.from('Dave');

        const envelope = alice.addAssertion('knows', bob);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob"
            ]
            `,
        );

        // Try to replace Dave (who doesn't exist in the envelope)
        const target = new Set<Digest>();
        target.add(dave.digest());

        const modified = envelope.walkReplace(target, charlie);

        // Should be identical since nothing matched
        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Alice" [
                "knows": "Bob"
            ]
            `,
        );

        expect(modified.isIdenticalTo(envelope)).toBe(true);
    });

    test('test walk replace multiple targets', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const carol = Envelope.from('Carol');
        const replacement = Envelope.from('REDACTED');

        const envelope = alice
            .addAssertion('knows', bob)
            .addAssertion('likes', carol);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob"
                "likes": "Carol"
            ]
            `,
        );

        // Replace both Bob and Carol with REDACTED
        const target = new Set<Digest>();
        target.add(bob.digest());
        target.add(carol.digest());

        const modified = envelope.walkReplace(target, replacement);

        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Alice" [
                "knows": "REDACTED"
                "likes": "REDACTED"
            ]
            `,
        );
    });

    test('test walk replace elided', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const charlie = Envelope.from('Charlie');

        // Create an envelope with Bob, then elide Bob
        const envelope = alice
            .addAssertion('knows', bob)
            .addAssertion('likes', bob);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob"
                "likes": "Bob"
            ]
            `,
        );

        // Elide Bob
        const elided = envelope.elideRemovingTarget(bob);

        // expected-text-output-rubric:
        expectActualExpected(
            elided.format(),
            `
            "Alice" [
                "knows": ELIDED
                "likes": ELIDED
            ]
            `,
        );

        // Replace the elided Bob with Charlie
        // This works because the elided node has Bob's digest
        const target = new Set<Digest>();
        target.add(bob.digest());

        const modified = elided.walkReplace(target, charlie);

        // expected-text-output-rubric:
        expectActualExpected(
            modified.format(),
            `
            "Alice" [
                "knows": "Charlie"
                "likes": "Charlie"
            ]
            `,
        );

        // Verify that the elided nodes were replaced
        expect(modified.isEquivalentTo(envelope)).toBe(false);
        expect(modified.isEquivalentTo(elided)).toBe(false);
    });

    test('test walk replace assertion with non-assertion fails', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const charlie = Envelope.from('Charlie');

        const envelope = alice.addAssertion('knows', bob);

        // Get the assertion's digest
        const knowsAssertion = envelope.assertionWithPredicate('knows');
        const assertionDigest = knowsAssertion.digest();

        // Try to replace the entire assertion with Charlie (a non-assertion)
        const target = new Set<Digest>();
        target.add(assertionDigest);

        expect(() => envelope.walkReplace(target, charlie)).toThrow('invalid format');
    });
});
