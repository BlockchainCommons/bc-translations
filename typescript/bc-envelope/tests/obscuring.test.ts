import { beforeAll, describe, expect, test } from 'vitest';
import { Digest, SymmetricKey } from '@bc/components';

import { Envelope, ObscureActions, ObscureType, registerTags } from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import { fakeContentKey, PLAINTEXT_HELLO } from './test-data.js';

describe('obscuring tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('compress obscure action', () => {
        const envelope = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const obscured = envelope.elideRemovingTargetWithAction(
            Envelope.from('Bob'),
            ObscureActions.compress(),
        );

        expect(obscured.format().includes('COMPRESSED')).toBe(true);
    });

    test('encrypt obscure action', () => {
        const envelope = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const obscured = envelope.elideRemovingTargetWithAction(
            Envelope.from('Bob'),
            ObscureActions.encrypt(fakeContentKey()),
        );

        expect(obscured.format().includes('ENCRYPTED')).toBe(true);
    });

    test('obscuring state transitions', () => {
        const key = SymmetricKey.new();
        const envelope = Envelope.from(PLAINTEXT_HELLO);
        expect(envelope.isObscured()).toBe(false);

        // Encrypted is obscured
        const encrypted = envelope.encryptSubject(key);
        expect(encrypted.isObscured()).toBe(true);

        // Elided is obscured
        const elided = envelope.elide();
        expect(elided.isObscured()).toBe(true);

        // Compressed is obscured
        const compressed = envelope.compress();
        expect(compressed.isObscured()).toBe(true);

        // Cannot encrypt an encrypted envelope
        expect(() => encrypted.encryptSubject(key)).toThrow();

        // Cannot encrypt an elided envelope
        expect(() => elided.encryptSubject(key)).toThrow();

        // OK to encrypt a compressed envelope
        const encryptedCompressed = compressed.encryptSubject(key);
        expect(encryptedCompressed.isEncrypted()).toBe(true);

        // OK to elide an encrypted envelope
        const elidedEncrypted = encrypted.elide();
        expect(elidedEncrypted.isElided()).toBe(true);

        // Eliding an elided envelope is idempotent
        const elidedElided = elided.elide();
        expect(elidedElided.isElided()).toBe(true);

        // OK to elide a compressed envelope
        const elidedCompressed = compressed.elide();
        expect(elidedCompressed.isElided()).toBe(true);

        // Cannot compress an encrypted envelope
        expect(() => encrypted.compress()).toThrow();

        // Cannot compress an elided envelope
        expect(() => elided.compress()).toThrow();

        // Compressing a compressed envelope is idempotent
        const compressedCompressed = compressed.compress();
        expect(compressedCompressed.isCompressed()).toBe(true);
    });

    test('nodes matching', () => {
        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('age', 30)
            .addAssertion('city', 'Boston');

        // Get some digests for targeting
        const knowsAssertion = envelope.assertionWithPredicate('knows');
        const knowsDigest = knowsAssertion.digest();
        const ageAssertion = envelope.assertionWithPredicate('age');
        const ageDigest = ageAssertion.digest();

        // Elide one assertion, compress another
        const elideTarget = new Set<Digest>([knowsDigest]);
        const compressTarget = new Set<Digest>([ageDigest]);

        let obscured = envelope.elideRemovingSet(elideTarget);
        obscured = obscured.elideRemovingSetWithAction(
            compressTarget,
            ObscureActions.compress(),
        );

        // expected-text-output-rubric:
        expectActualExpected(
            obscured.format(),
            `
            "Alice" [
                "city": "Boston"
                COMPRESSED
                ELIDED
            ]
            `,
        );

        // Test finding elided nodes
        const elidedNodes = obscured.nodesMatching(undefined, [ObscureType.Elided]);
        expect(elidedNodes.size).toBeGreaterThan(0);

        // Test finding compressed nodes
        const compressedNodes = obscured.nodesMatching(undefined, [ObscureType.Compressed]);
        expect(compressedNodes.size).toBeGreaterThan(0);

        // Test finding with target filter
        const targetFilter = new Set<Digest>([knowsDigest]);
        const filtered = obscured.nodesMatching(targetFilter, [ObscureType.Elided]);
        expect(filtered.size).toBe(1);

        // Test with no matches
        const noMatchTarget = new Set<Digest>([Digest.fromImage(new TextEncoder().encode('nonexistent'))]);
        const noMatches = obscured.nodesMatching(noMatchTarget, [ObscureType.Elided]);
        expect(noMatches.size).toBe(0);
    });

    test('walk unelide', () => {
        const alice = Envelope.from('Alice');
        const bob = Envelope.from('Bob');
        const carol = Envelope.from('Carol');

        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('friend', 'Carol');

        // Elide multiple parts
        const elided = envelope
            .elideRemovingTarget(alice)
            .elideRemovingTarget(bob);

        // expected-text-output-rubric:
        expectActualExpected(
            elided.format(),
            `
            ELIDED [
                "friend": "Carol"
                "knows": ELIDED
            ]
            `,
        );

        // Restore with walkUnelide
        const restored = elided.walkUnelide([alice, bob, carol]);

        // expected-text-output-rubric:
        expectActualExpected(
            restored.format(),
            `
            "Alice" [
                "friend": "Carol"
                "knows": "Bob"
            ]
            `,
        );

        // Partial restoration
        const partial = elided.walkUnelide([alice]);
        // expected-text-output-rubric:
        expectActualExpected(
            partial.format(),
            `
            "Alice" [
                "friend": "Carol"
                "knows": ELIDED
            ]
            `,
        );

        // No matching envelopes
        const unchanged = elided.walkUnelide([]);
        expect(unchanged.isIdenticalTo(elided)).toBe(true);
    });

    test('walk decrypt', () => {
        const key1 = SymmetricKey.new();
        const key2 = SymmetricKey.new();
        const key3 = SymmetricKey.new();

        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('age', 30)
            .addAssertion('city', 'Boston');

        // Encrypt different parts with different keys
        const knowsAssertion = envelope.assertionWithPredicate('knows');
        const ageAssertion = envelope.assertionWithPredicate('age');

        const encrypt1Target = new Set<Digest>([knowsAssertion.digest()]);
        const encrypt2Target = new Set<Digest>([ageAssertion.digest()]);

        const encrypted = envelope
            .elideRemovingSetWithAction(encrypt1Target, ObscureActions.encrypt(key1))
            .elideRemovingSetWithAction(encrypt2Target, ObscureActions.encrypt(key2));

        // expected-text-output-rubric:
        expectActualExpected(
            encrypted.format(),
            `
            "Alice" [
                "city": "Boston"
                ENCRYPTED (2)
            ]
            `,
        );

        // Decrypt with all keys
        const decrypted = encrypted.walkDecrypt([key1, key2]);
        // expected-text-output-rubric:
        expectActualExpected(
            decrypted.format(),
            `
            "Alice" [
                "age": 30
                "city": "Boston"
                "knows": "Bob"
            ]
            `,
        );

        // Partial decryption with one key
        const partial = encrypted.walkDecrypt([key1]);
        expect(partial.isIdenticalTo(encrypted)).toBe(false);
        expect(partial.isEquivalentTo(envelope)).toBe(true);

        // expected-text-output-rubric:
        expectActualExpected(
            partial.format(),
            `
            "Alice" [
                "city": "Boston"
                "knows": "Bob"
                ENCRYPTED
            ]
            `,
        );

        // Wrong key leaves unchanged
        const unchanged = encrypted.walkDecrypt([key3]);
        expect(unchanged.isIdenticalTo(encrypted)).toBe(true);
    });

    test('walk decompress', () => {
        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('bio', 'A'.repeat(1000))
            .addAssertion('description', 'B'.repeat(1000));

        const bioAssertion = envelope.assertionWithPredicate('bio');
        const descAssertion = envelope.assertionWithPredicate('description');
        const bioDigest = bioAssertion.digest();
        const descDigest = descAssertion.digest();

        const compressTarget = new Set<Digest>([bioDigest, descDigest]);
        const compressed = envelope.elideRemovingSetWithAction(
            compressTarget,
            ObscureActions.compress(),
        );

        // expected-text-output-rubric:
        expectActualExpected(
            compressed.format(),
            `
            "Alice" [
                "knows": "Bob"
                COMPRESSED (2)
            ]
            `,
        );

        // Decompress all
        const decompressed = compressed.walkDecompress();
        expect(decompressed.isEquivalentTo(envelope)).toBe(true);

        // Partial decompress with target filter
        const target = new Set<Digest>([bioDigest]);
        const partial = compressed.walkDecompress(target);
        expect(partial.isIdenticalTo(compressed)).toBe(false);
        expect(partial.isEquivalentTo(envelope)).toBe(true);

        // Bio should be decompressed but description still compressed
        const stillCompressed = partial.nodesMatching(undefined, [ObscureType.Compressed]);
        expect(stillCompressed.size).toBeGreaterThan(0);

        // Non-matching target leaves unchanged
        const noMatch = new Set<Digest>([Digest.fromImage(new TextEncoder().encode('nonexistent'))]);
        const unchanged = compressed.walkDecompress(noMatch);
        expect(unchanged.isIdenticalTo(compressed)).toBe(true);
    });

    test('mixed obscuration operations', () => {
        const key = SymmetricKey.new();

        const envelope = Envelope.from('Alice')
            .addAssertion('knows', 'Bob')
            .addAssertion('age', 30)
            .addAssertion('bio', 'A'.repeat(1000));

        const knowsAssertion = envelope.assertionWithPredicate('knows');
        const ageAssertion = envelope.assertionWithPredicate('age');
        const bioAssertion = envelope.assertionWithPredicate('bio');

        const knowsDigest = knowsAssertion.digest();
        const ageDigest = ageAssertion.digest();
        const bioDigest = bioAssertion.digest();

        // Apply different obscuration types
        const elideTarget = new Set<Digest>([knowsDigest]);
        const encryptTarget = new Set<Digest>([ageDigest]);
        const compressTarget = new Set<Digest>([bioDigest]);

        const obscured = envelope
            .elideRemovingSet(elideTarget)
            .elideRemovingSetWithAction(encryptTarget, ObscureActions.encrypt(key))
            .elideRemovingSetWithAction(compressTarget, ObscureActions.compress());

        // Verify different obscuration types
        const elided = obscured.nodesMatching(undefined, [ObscureType.Elided]);
        const encrypted = obscured.nodesMatching(undefined, [ObscureType.Encrypted]);
        const compressed = obscured.nodesMatching(undefined, [ObscureType.Compressed]);

        expect(elided.size).toBeGreaterThan(0);
        expect(encrypted.size).toBeGreaterThan(0);
        expect(compressed.size).toBeGreaterThan(0);

        // Restore everything
        const restored = obscured
            .walkUnelide([knowsAssertion])
            .walkDecrypt([key])
            .walkDecompress();

        expect(restored.isEquivalentTo(envelope)).toBe(true);
    });
});
