import { beforeAll, describe, expect, test } from 'vitest';
import { hexToBytes } from '@bc/dcbor';
import { Digest } from '@bc/components';
import { NOTE, UNIT, POSITION } from '@bc/known-values';

import { Envelope, registerTags } from '../src/index.js';
import {
    assertionEnvelope,
    checkEncoding,
    doubleAssertionEnvelope,
    doubleWrappedEnvelope,
    helloEnvelope,
    knownValueEnvelope,
    singleAssertionEnvelope,
    wrappedEnvelope,
} from './test-data.js';
import { expectActualExpected } from './test-helpers.js';

describe('core tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('reads legacy leaf encoding', () => {
        const legacyEnvelope = Envelope.fromTaggedCborData(hexToBytes('d8c8d818182a'));
        const envelope = Envelope.from(42);
        expect(legacyEnvelope.isIdenticalTo(envelope)).toBe(true);
        expect(legacyEnvelope.isEquivalentTo(envelope)).toBe(true);
    });

    test('integer subject vectors', () => {
        const envelope = checkEncoding(Envelope.from(42));

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.diagnosticAnnotated(),
            '200(201(42)   / leaf /)   / envelope /',
        );

        expect(envelope.digest().toString()).toBe('Digest(7f83f7bda2d63959d34767689f06d47576683d378d9eb8d09386c9a020395c53)');
        expect(envelope.format()).toBe('42');
        expect(envelope.extractSubject<number>()).toBe(42);
    });

    test('known value subject vectors', () => {
        const envelope = checkEncoding(knownValueEnvelope());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.diagnosticAnnotated(),
            `
            200(4)   / envelope /
            `,
        );

        expect(envelope.format()).toBe("'note'");
        expect(envelope.extractSubject()).toEqual(NOTE);
    });

    test('assertion subject vectors', () => {
        const envelope = checkEncoding(assertionEnvelope());

        expect(envelope.format()).toBe('"knows": "Bob"');
        expect(envelope.digest().equals(Envelope.newAssertion('knows', 'Bob').digest())).toBe(true);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.diagnosticAnnotated(),
            `
            200({
                201("knows")   / leaf /:
                201("Bob")   / leaf /
            })   / envelope /
            `,
        );
    });

    test('subject with assertion vectors', () => {
        const envelope = checkEncoding(singleAssertionEnvelope());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
    "knows": "Bob"
            ]
            `,
        );

        expect(envelope.extractSubject<string>()).toBe('Alice');
    });

    test('wrapped vectors', () => {
        const envelope = checkEncoding(wrappedEnvelope());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            {
    "Hello."
            }
            `,
        );
    });

    test('negative int subject', () => {
        const envelope = checkEncoding(Envelope.from(-42));

        expect(envelope.diagnosticAnnotated()).toBe(
            '200(201(-42)   / leaf /)   / envelope /',
        );

        expect(envelope.digest().toString()).toBe(
            'Digest(9e0ad272780de7aa1dbdfbc99058bb81152f623d3b95b5dfb0a036badfcc9055)',
        );
        expect(envelope.format()).toBe('-42');
        expect(envelope.extractSubject<number>()).toBe(-42);
    });

    test('cbor encodable subject', () => {
        const envelope = checkEncoding(helloEnvelope());

        expect(envelope.diagnosticAnnotated()).toBe(
            '200(201("Hello.")   / leaf /)   / envelope /',
        );

        expect(envelope.digest().toString()).toBe(
            'Digest(8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59)',
        );
        expect(envelope.format()).toBe('"Hello."');
        expect(envelope.extractSubject<string>()).toBe('Hello.');
    });

    test('subject with two assertions', () => {
        const envelope = checkEncoding(doubleAssertionEnvelope());

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.diagnosticAnnotated(),
            `
            200([
                201("Alice")   / leaf /,
                {
                    201("knows")   / leaf /:
                    201("Carol")   / leaf /
                },
                {
                    201("knows")   / leaf /:
                    201("Bob")   / leaf /
                }
            ])   / envelope /
            `,
        );

        expect(envelope.digest().toString()).toBe(
            'Digest(b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471)',
        );

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            `,
        );

        expect(envelope.extractSubject<string>()).toBe('Alice');
    });

    test('double wrapped', () => {
        const envelope = checkEncoding(doubleWrappedEnvelope());

        expect(envelope.diagnosticAnnotated()).toBe(
            '200(200(200(201("Hello.")   / leaf /)   / envelope /)   / envelope /)   / envelope /',
        );

        expect(envelope.digest().toString()).toBe(
            'Digest(8b14f3bcd7c05aac8f2162e7047d7ef5d5eab7d82ee3f9dc4846c70bae4d200b)',
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
    });

    test('assertion with assertions', () => {
        const a = Envelope.newAssertion(1, 2)
            .addAssertion(3, 4)
            .addAssertion(5, 6);
        const e = Envelope.from(7).addAssertionEnvelope(a);

        // expected-text-output-rubric:
        expectActualExpected(
            e.format(),
            `
            7 [
                {
                    1: 2
                } [
                    3: 4
                    5: 6
                ]
            ]
            `,
        );
    });

    test('digest leaf', () => {
        const digest = helloEnvelope().digest();
        const e = checkEncoding(Envelope.from(digest));

        // expected-text-output-rubric:
        expectActualExpected(
            e.format(),
            'Digest(8cc96cdb)',
        );

        expect(e.digest().toString()).toBe(
            'Digest(07b518af92a6196bc153752aabefedb34ff8e1a7d820c01ef978dfc3e7e52e05)',
        );

        expect(e.diagnosticAnnotated()).toBe(
            "200(201(40001(h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59')   / digest /)   / leaf /)   / envelope /",
        );
    });

    test('unknown leaf', () => {
        const unknownUr = 'ur:envelope/tpsotaaodnoyadgdjlssmkcklgoskseodnyteofwwfylkiftaydpdsjz';
        const e = Envelope.fromUrString(unknownUr);
        expect(e.format()).toBe("555({1: h'6fc4981e8da778332bf93342f3f77d3a'})");
    });

    test('true', () => {
        const e = checkEncoding(Envelope.from(true));
        expect(e.isBool()).toBe(true);
        expect(e.isTrue()).toBe(true);
        expect(e.isFalse()).toBe(false);
        expect(e.format()).toBe('true');
    });

    test('false', () => {
        const e = checkEncoding(Envelope.from(false));
        expect(e.isBool()).toBe(true);
        expect(e.isTrue()).toBe(false);
        expect(e.isFalse()).toBe(true);
        expect(e.format()).toBe('false');
    });

    test('unit', () => {
        let e = checkEncoding(Envelope.unit());
        expect(e.isSubjectUnit()).toBe(true);
        expect(e.format()).toBe("''");

        e = e.addAssertion('foo', 'bar');
        expect(e.isSubjectUnit()).toBe(true);

        // expected-text-output-rubric:
        expectActualExpected(
            e.format(),
            `
            '' [
                "foo": "bar"
            ]
            `,
        );

        expect(e.extractSubject()).toEqual(UNIT);
    });

    test('position', () => {
        let e = Envelope.from('Hello');
        expect(() => e.position()).toThrow();

        e = e.setPosition(42);
        expect(e.position()).toBe(42);

        // expected-text-output-rubric:
        expectActualExpected(
            e.format(),
            `
            "Hello" [
                'position': 42
            ]
            `,
        );

        e = e.setPosition(0);
        expect(e.position()).toBe(0);

        // expected-text-output-rubric:
        expectActualExpected(
            e.format(),
            `
            "Hello" [
                'position': 0
            ]
            `,
        );

        e = e.removePosition();
        expect(() => e.position()).toThrow();
        expect(e.format()).toBe('"Hello"');
    });
});
