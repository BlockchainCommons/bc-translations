import { beforeAll, describe, expect, test } from 'vitest';
import { SymmetricKey } from '@bc/components';

import {
    DigestDisplayFormat,
    Envelope,
    registerTags,
} from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import { alicePrivateKey } from './test-data.js';

describe('format tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('plaintext formatting vectors', () => {
        const envelope = Envelope.from('Hello.');
        expect(envelope.format()).toBe('"Hello."');
        expect(envelope.formatFlat()).toBe('"Hello."');

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.treeFormat(),
            `
            8cc96cdb "Hello."
            `,
        );

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.treeFormatOpt({ digestDisplay: DigestDisplayFormat.Full }),
            `
            8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59 "Hello."
            `,
        );
    });

    test('encrypted subject formatting vectors', () => {
        const envelope = Envelope
            .from('Alice')
            .addAssertion('knows', 'Bob')
            .encryptSubject(SymmetricKey.new());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            ENCRYPTED [
    "knows": "Bob"
            ]
            `,
        );

        expect(envelope.formatFlat()).toBe('ENCRYPTED [ "knows": "Bob" ]');
    });

    test('mermaid format produces graph text', () => {
        const envelope = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const mermaid = envelope.mermaidFormat();
        expect(/graph|flowchart|digraph/i.test(mermaid)).toBe(true);
        expect(mermaid.length).toBeGreaterThan(0);
    });

    test('signed plaintext format', () => {
        const envelope = Envelope.from('Hello.').addSignature(alicePrivateKey());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Hello." [
                'signed': Signature
            ]
            `,
        );

        expect(envelope.formatFlat()).toBe('"Hello." [ \'signed\': Signature ]');

        // Tree format: check structure (digests vary with non-deterministic signing)
        const tree = envelope.treeFormat();
        expect(tree).toContain('NODE');
        expect(tree).toContain('subj "Hello."');
        expect(tree).toContain('ASSERTION');
        expect(tree).toContain("pred 'signed'");
        expect(tree).toContain('obj Signature');
    });

    test('wrap then signed format', () => {
        const envelope = Envelope.from('Hello.').wrap().addSignature(alicePrivateKey());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            {
                "Hello."
            } [
                'signed': Signature
            ]
            `,
        );
    });

    test('top level assertion format', () => {
        const envelope = Envelope.newAssertion('knows', 'Bob');

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "knows": "Bob"
            `,
        );

        expect(envelope.formatFlat()).toBe('"knows": "Bob"');

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.treeFormat(),
            `
            78d666eb ASSERTION
                db7dd21c pred "knows"
                13b74194 obj "Bob"
            `,
        );
    });

    test('signed subject format', () => {
        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('knows', 'Carol')
            .addSignature(alicePrivateKey());

        // Assertions are sorted by digest, so ordering may vary.
        // Check that all expected parts are present.
        const fmt = envelope.format();
        expect(fmt).toContain('"Alice"');
        expect(fmt).toContain('"knows": "Bob"');
        expect(fmt).toContain('"knows": "Carol"');
        expect(fmt).toContain("'signed': Signature");

        const flat = envelope.formatFlat();
        expect(flat).toContain('"Alice"');
        expect(flat).toContain('"knows": "Bob"');
        expect(flat).toContain('"knows": "Carol"');
        expect(flat).toContain("'signed': Signature");

        const tree = envelope.treeFormat();
        expect(tree).toContain('subj "Alice"');
        expect(tree).toContain("pred 'signed'");
        expect(tree).toContain('obj Signature');
        expect(tree).toContain('pred "knows"');
        expect(tree).toContain('obj "Carol"');
        expect(tree).toContain('obj "Bob"');
    });

    test('assertion positions in tree format', () => {
        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('knows', 'Carol');

        const tree = envelope.treeFormat();

        // Verify structure contains subject, assertions, predicates, objects
        expect(tree).toContain('subj "Alice"');
        expect(tree).toContain('pred "knows"');
        expect(tree).toContain('obj "Bob"');
        expect(tree).toContain('obj "Carol"');
        expect(tree).toContain('NODE');
        expect(tree).toContain('ASSERTION');
    });
});
