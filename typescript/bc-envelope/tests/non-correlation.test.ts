import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import { fakeContentKey, fakeNonce } from './test-data.js';

describe('non-correlation tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('salt changes envelope digest', () => {
        const base = Envelope.from('Alice');
        const salted1 = base.addSaltWithLength(16);
        const salted2 = base.addSaltWithLength(16);

        expect(salted1.digest().equals(salted2.digest())).toBe(false);
    });

    test('encryption can vary ciphertext with same semantic digest', () => {
        const base = Envelope.from('message').addAssertion('n', 1);
        const e1 = base.encryptSubject(fakeContentKey(), fakeNonce());
        const e2 = base.encryptSubject(fakeContentKey());

        expect(e1.digest().equals(e2.digest())).toBe(true);
        expect(Buffer.from(e1.taggedCborData()).equals(Buffer.from(e2.taggedCborData()))).toBe(false);
    });

    test('predicate correlation', () => {
        const e1 = Envelope.from('Foo')
            .addAssertion('note', 'Bar');
        const e2 = Envelope.from('Baz')
            .addAssertion('note', 'Quux');

        // expected-text-output-rubric:
        expectActualExpected(
            e1.format(),
            `
            "Foo" [
                "note": "Bar"
            ]
            `,
        );

        // e1 and e2 have the same predicate
        const e1Pred = e1.assertions()[0]!.asPredicate()!;
        const e2Pred = e2.assertions()[0]!.asPredicate()!;
        expect(e1Pred.isEquivalentTo(e2Pred)).toBe(true);

        // Elide the entire contents of e1 without eliding the envelope itself
        const e1Elided = e1.elideRevealingTarget(e1);

        // expected-text-output-rubric:
        expectActualExpected(
            e1Elided.format(),
            `
            ELIDED [
                ELIDED
            ]
            `,
        );
    });
});
