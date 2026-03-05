import { Digest } from '@bc/components';
import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import { checkEncoding } from './test-data.js';

describe('proof tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('proof contains target', () => {
        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('knows', 'Carol');
        const target = Envelope.from('Bob');

        const proof = envelope.proofContainsTarget(target);
        expect(proof).toBeDefined();
        expect(envelope.confirmContainsTarget(target, proof!)).toBe(true);
    });

    test('proof fails for absent target', () => {
        const envelope = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const target = Envelope.from('Mallory');
        const proof = envelope.proofContainsTarget(target);
        expect(proof).toBeUndefined();
    });

    test('friends list with salted assertions', () => {
        const aliceFriends = Envelope.from('Alice')
            .addAssertionSalted('knows', 'Bob', true)
            .addAssertionSalted('knows', 'Carol', true)
            .addAssertionSalted('knows', 'Dan', true);

        // expected-text-output-rubric:
        expectActualExpected(
            aliceFriends.format(),
            `
            "Alice" [
                {
                    "knows": "Bob"
                } [
                    'salt': Salt
                ]
                {
                    "knows": "Carol"
                } [
                    'salt': Salt
                ]
                {
                    "knows": "Dan"
                } [
                    'salt': Salt
                ]
            ]
            `,
        );

        // Completely elided root
        const aliceFriendsRoot = aliceFriends.elideRevealingSet(new Set<Digest>());
        expect(aliceFriendsRoot.format()).toBe('ELIDED');

        // Prove that the document contains "knows Bob"
        const knowsBobAssertion = Envelope.newAssertion('knows', 'Bob');
        const aliceKnowsBobProof = checkEncoding(
            aliceFriends.proofContainsTarget(knowsBobAssertion)!,
        );

        // expected-text-output-rubric:
        expectActualExpected(
            aliceKnowsBobProof.format(),
            `
            ELIDED [
                ELIDED [
                    ELIDED
                ]
                ELIDED (2)
            ]
            `,
        );

        // Confirm the proof
        expect(
            aliceFriendsRoot.confirmContainsTarget(knowsBobAssertion, aliceKnowsBobProof),
        ).toBe(true);
    });

    test('multi position proof', () => {
        const aliceFriends = Envelope.from('Alice')
            .addAssertionSalted('knows', 'Bob', true)
            .addAssertionSalted('knows', 'Carol', true)
            .addAssertionSalted('knows', 'Dan', true);

        // The "knows" predicate exists at three positions
        const knowsProof = checkEncoding(
            aliceFriends.proofContainsTarget(Envelope.from('knows'))!,
        );

        // expected-text-output-rubric:
        expectActualExpected(
            knowsProof.format(),
            `
            ELIDED [
                {
                    ELIDED: ELIDED
                } [
                    ELIDED
                ]
                {
                    ELIDED: ELIDED
                } [
                    ELIDED
                ]
                {
                    ELIDED: ELIDED
                } [
                    ELIDED
                ]
            ]
            `,
        );
    });
});
