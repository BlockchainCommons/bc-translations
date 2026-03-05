import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import { checkEncoding } from './test-data.js';
import { expectActualExpected } from './test-helpers.js';

describe('core nesting tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('supports nested wrapping and assertions', () => {
        const envelope = Envelope
            .from('Alice')
            .addAssertion('knows', 'Bob')
            .wrap()
            .addAssertion('note', 'outer');

        expect(envelope.subject().isWrapped()).toBe(true);
        expect(envelope.assertionsWithPredicate('note').length).toBe(1);
        expect(envelope.unwrap().extractSubject<string>()).toBe('Alice');
    });

    test('walks nested structures deterministically', () => {
        const envelope = Envelope
            .from('A')
            .addAssertion('b', Envelope.from('B').addAssertion('c', 'C'));

        const visited: string[] = [];
        envelope.walk<string[]>(false, visited, (item, _level, _edge, state) => {
            state.push(item.formatFlat());
            return [state, false];
        });

        expect(visited.length).toBeGreaterThan(0);
        expect(envelope.elementsCount()).toBeGreaterThan(1);
    });

    test('stable whole-text rendering for nested case', () => {
        const envelope = Envelope
            .from('Alice')
            .addAssertion('knows', Envelope.from('Bob').addAssertion('knows', 'Carol'));

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
    "knows": "Bob" [
        "knows": "Carol"
    ]
            ]
            `,
        );
    });

    test('predicate enclosures', () => {
        const alice = Envelope.from('Alice');
        const knows = Envelope.from('knows');
        const bob = Envelope.from('Bob');

        const a = Envelope.from('A');
        const b = Envelope.from('B');

        const knowsBob = Envelope.newAssertion(knows, bob);
        expect(knowsBob.format()).toBe('"knows": "Bob"');

        const ab = Envelope.newAssertion(a, b);
        expect(ab.format()).toBe('"A": "B"');

        const knowsAbBob = checkEncoding(
            Envelope.newAssertion(knows.addAssertionEnvelope(ab), bob),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            knowsAbBob.format(),
            `
            "knows" [
                "A": "B"
            ]
            : "Bob"
            `,
        );

        const knowsBobAb = checkEncoding(
            Envelope.newAssertion(knows, bob.addAssertionEnvelope(ab)),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            knowsBobAb.format(),
            `
            "knows": "Bob" [
                "A": "B"
            ]
            `,
        );

        const knowsBobEncloseAb = checkEncoding(
            knowsBob.addAssertionEnvelope(ab),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            knowsBobEncloseAb.format(),
            `
            {
                "knows": "Bob"
            } [
                "A": "B"
            ]
            `,
        );

        const aliceKnowsBob = checkEncoding(
            alice.addAssertionEnvelope(knowsBob),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceKnowsBob.format(),
            `
            "Alice" [
                "knows": "Bob"
            ]
            `,
        );

        const aliceAbKnowsBob = checkEncoding(
            aliceKnowsBob.addAssertionEnvelope(ab),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceAbKnowsBob.format(),
            `
            "Alice" [
                "A": "B"
                "knows": "Bob"
            ]
            `,
        );

        const aliceKnowsAbBob = checkEncoding(
            alice.addAssertionEnvelope(
                Envelope.newAssertion(knows.addAssertionEnvelope(ab), bob),
            ),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceKnowsAbBob.format(),
            `
            "Alice" [
                "knows" [
                    "A": "B"
                ]
                : "Bob"
            ]
            `,
        );

        const aliceKnowsBobAb = checkEncoding(
            alice.addAssertionEnvelope(
                Envelope.newAssertion(knows, bob.addAssertionEnvelope(ab)),
            ),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceKnowsBobAb.format(),
            `
            "Alice" [
                "knows": "Bob" [
                    "A": "B"
                ]
            ]
            `,
        );

        const aliceKnowsAbBobAb = checkEncoding(
            alice.addAssertionEnvelope(
                Envelope.newAssertion(
                    knows.addAssertionEnvelope(ab),
                    bob.addAssertionEnvelope(ab),
                ),
            ),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceKnowsAbBobAb.format(),
            `
            "Alice" [
                "knows" [
                    "A": "B"
                ]
                : "Bob" [
                    "A": "B"
                ]
            ]
            `,
        );

        const aliceAbKnowsAbBobAb = checkEncoding(
            alice
                .addAssertionEnvelope(ab)
                .addAssertionEnvelope(
                    Envelope.newAssertion(
                        knows.addAssertionEnvelope(ab),
                        bob.addAssertionEnvelope(ab),
                    ),
                ),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceAbKnowsAbBobAb.format(),
            `
            "Alice" [
                "A": "B"
                "knows" [
                    "A": "B"
                ]
                : "Bob" [
                    "A": "B"
                ]
            ]
            `,
        );

        const aliceAbKnowsAbBobAbEncloseAb = checkEncoding(
            alice
                .addAssertionEnvelope(ab)
                .addAssertionEnvelope(
                    Envelope.newAssertion(
                        knows.addAssertionEnvelope(ab),
                        bob.addAssertionEnvelope(ab),
                    )
                    .addAssertionEnvelope(ab),
                ),
        );
        // expected-text-output-rubric:
        expectActualExpected(
            aliceAbKnowsAbBobAbEncloseAb.format(),
            `
            "Alice" [
                {
                    "knows" [
                        "A": "B"
                    ]
                    : "Bob" [
                        "A": "B"
                    ]
                } [
                    "A": "B"
                ]
                "A": "B"
            ]
            `,
        );
    });

    test('nesting plaintext', () => {
        const envelope = Envelope.from('Hello.');
        expect(envelope.format()).toBe('"Hello."');

        const elidedEnvelope = envelope.elide();
        expect(elidedEnvelope.isEquivalentTo(envelope)).toBe(true);
        expect(elidedEnvelope.format()).toBe('ELIDED');
    });

    test('nesting once', () => {
        const envelope = checkEncoding(Envelope.from('Hello.').wrap());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            {
                "Hello."
            }
            `,
        );

        const elidedEnvelope = checkEncoding(
            Envelope.from('Hello.').elide().wrap(),
        );
        expect(elidedEnvelope.isEquivalentTo(envelope)).toBe(true);

        // expected-text-output-rubric:
        expectActualExpected(
            elidedEnvelope.format(),
            `
            {
                ELIDED
            }
            `,
        );
    });

    test('nesting twice', () => {
        const envelope = checkEncoding(
            Envelope.from('Hello.').wrap().wrap(),
        );

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            {
                {
                    "Hello."
                }
            }
            `,
        );

        const target = envelope.unwrap().unwrap();
        const elidedEnvelope = envelope.elideRemovingTarget(target);

        // expected-text-output-rubric:
        expectActualExpected(
            elidedEnvelope.format(),
            `
            {
                {
                    ELIDED
                }
            }
            `,
        );

        expect(envelope.isEquivalentTo(elidedEnvelope)).toBe(true);
    });

    test('assertions on all parts of envelope', () => {
        const predicate = Envelope.from('predicate')
            .addAssertion('predicate-predicate', 'predicate-object');
        const object = Envelope.from('object')
            .addAssertion('object-predicate', 'object-object');
        const envelope = checkEncoding(
            Envelope.from('subject').addAssertion(predicate, object),
        );

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "subject" [
                "predicate" [
                    "predicate-predicate": "predicate-object"
                ]
                : "object" [
                    "object-predicate": "object-object"
                ]
            ]
            `,
        );
    });

    test('assertion on bare assertion', () => {
        const envelope = Envelope.newAssertion('predicate', 'object')
            .addAssertion('assertion-predicate', 'assertion-object');

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            {
                "predicate": "object"
            } [
                "assertion-predicate": "assertion-object"
            ]
            `,
        );
    });
});
