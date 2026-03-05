import { beforeAll, describe, expect, test } from 'vitest';
import { KeyDerivationMethod, SymmetricKey } from '@bc/components';
import { IS_A } from '@bc/known-values';

import { Envelope, registerTags } from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import {
    alicePrivateKey,
    alicePublicKey,
    bobPrivateKey,
    bobPublicKey,
    carolPrivateKey,
    carolPublicKey,
    checkEncoding,
    fakeContentKey,
    fakeNonce,
    helloEnvelope,
    PLAINTEXT_HELLO,
} from './test-data.js';

describe('crypto tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('encrypt/decrypt subject with deterministic nonce', () => {
        const envelope = Envelope.from('Alice').addAssertion('knows', 'Bob');
        const encrypted = envelope.encryptSubject(fakeContentKey(), fakeNonce());
        const decrypted = encrypted.decryptSubject(fakeContentKey());

        expect(encrypted.subject().isEncrypted()).toBe(true);
        expect(decrypted.isIdenticalTo(envelope)).toBe(true);
    });

    test('signature verification', () => {
        const envelope = checkEncoding(Envelope.from('signed payload').addSignature(alicePrivateKey()));
        expect(envelope.hasSignatureFrom(alicePublicKey())).toBe(true);
        expect(envelope.hasSignatureFrom(bobPrivateKey().publicKeys())).toBe(false);

        const verified = envelope.verifySignatureFrom(alicePublicKey());
        expect(verified.isIdenticalTo(envelope)).toBe(true);
    });

    test('plaintext UR roundtrip', () => {
        const envelope = helloEnvelope();
        const urStr = envelope.urString();
        expect(envelope.format()).toBe('"Hello."');

        const received = Envelope.fromUrString(urStr);
        const receivedPlaintext = received.extractSubject<string>();
        expect(receivedPlaintext).toBe(PLAINTEXT_HELLO);
    });

    test('symmetric encryption', () => {
        const key = SymmetricKey.new();

        const envelope = checkEncoding(helloEnvelope().encryptSubject(key));
        const urStr = envelope.urString();

        expect(envelope.format()).toBe('ENCRYPTED');

        const receivedEnvelope = Envelope.fromUrString(urStr);
        const receivedPlaintext = receivedEnvelope
            .decryptSubject(key)
            .extractSubject<string>();
        expect(receivedPlaintext).toBe(PLAINTEXT_HELLO);

        // Subject is encrypted, not a string
        expect(receivedEnvelope.subject().isEncrypted()).toBe(true);

        // Can't decrypt with wrong key
        expect(() => receivedEnvelope.decryptSubject(SymmetricKey.new())).toThrow();
    });

    test('encrypt decrypt roundtrip', () => {
        function roundTripTest(envelope: Envelope): void {
            const key = SymmetricKey.new();
            const plaintextSubject = checkEncoding(envelope);
            const encryptedSubject = plaintextSubject.encryptSubject(key);
            expect(encryptedSubject.isEquivalentTo(plaintextSubject)).toBe(true);
            const plaintextSubject2 = checkEncoding(encryptedSubject.decryptSubject(key));
            expect(encryptedSubject.isEquivalentTo(plaintextSubject2)).toBe(true);
            expect(plaintextSubject.isIdenticalTo(plaintextSubject2)).toBe(true);
        }

        // leaf
        roundTripTest(Envelope.from(PLAINTEXT_HELLO));
        // node
        roundTripTest(Envelope.from('Alice').addAssertion('knows', 'Bob'));
        // wrapped
        roundTripTest(Envelope.from('Alice').wrap());
        // known value
        roundTripTest(Envelope.from(IS_A));
        // assertion
        roundTripTest(Envelope.newAssertion('knows', 'Bob'));
        // compressed
        roundTripTest(Envelope.from(PLAINTEXT_HELLO).compress());
    });

    test('sign then encrypt', () => {
        const key = SymmetricKey.new();

        const envelope = checkEncoding(
            checkEncoding(
                checkEncoding(helloEnvelope().addSignature(alicePrivateKey()))
                    .wrap(),
            ).encryptSubject(key),
        );
        const urStr = envelope.urString();

        expect(envelope.format()).toBe('ENCRYPTED');

        const receivedPlaintext = Envelope.fromUrString(urStr)
            .decryptSubject(key)
            .unwrap()
            .verifySignatureFrom(alicePublicKey())
            .extractSubject<string>();
        expect(receivedPlaintext).toBe(PLAINTEXT_HELLO);
    });

    test('encrypt then sign', () => {
        const key = SymmetricKey.new();

        const envelope = checkEncoding(
            helloEnvelope()
                .encryptSubject(key)
                .addSignature(alicePrivateKey()),
        );
        const urStr = envelope.urString();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            ENCRYPTED [
                'signed': Signature
            ]
            `,
        );

        const receivedPlaintext = Envelope.fromUrString(urStr)
            .verifySignatureFrom(alicePublicKey())
            .decryptSubject(key)
            .extractSubject<string>();
        expect(receivedPlaintext).toBe(PLAINTEXT_HELLO);
    });

    test('multi recipient', () => {
        const contentKey = SymmetricKey.new();
        const envelope = checkEncoding(
            helloEnvelope()
                .encryptSubject(contentKey)
                .addRecipient(bobPublicKey(), contentKey)
                .addRecipient(carolPublicKey(), contentKey),
        );
        const urStr = envelope.urString();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
            ]
            `,
        );

        const receivedEnvelope = Envelope.fromUrString(urStr);

        // Bob decrypts
        const bobPlaintext = receivedEnvelope
            .decryptSubjectToRecipient(bobPrivateKey())
            .extractSubject<string>();
        expect(bobPlaintext).toBe(PLAINTEXT_HELLO);

        // Carol decrypts
        const carolPlaintext = receivedEnvelope
            .decryptSubjectToRecipient(carolPrivateKey())
            .extractSubject<string>();
        expect(carolPlaintext).toBe(PLAINTEXT_HELLO);

        // Alice can't decrypt
        expect(() => receivedEnvelope.decryptSubjectToRecipient(alicePrivateKey())).toThrow();
    });

    test('visible signature multi recipient', () => {
        const contentKey = SymmetricKey.new();
        const envelope = checkEncoding(
            helloEnvelope()
                .addSignature(alicePrivateKey())
                .encryptSubject(contentKey)
                .addRecipient(bobPublicKey(), contentKey)
                .addRecipient(carolPublicKey(), contentKey),
        );
        const urStr = envelope.urString();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
                'signed': Signature
            ]
            `,
        );

        const receivedEnvelope = Envelope.fromUrString(urStr);

        // Bob validates then decrypts
        const bobPlaintext = receivedEnvelope
            .verifySignatureFrom(alicePublicKey())
            .decryptSubjectToRecipient(bobPrivateKey())
            .extractSubject<string>();
        expect(bobPlaintext).toBe(PLAINTEXT_HELLO);
    });

    test('hidden signature multi recipient', () => {
        const contentKey = SymmetricKey.new();
        const envelope = checkEncoding(
            helloEnvelope()
                .addSignature(alicePrivateKey())
                .wrap()
                .encryptSubject(contentKey)
                .addRecipient(bobPublicKey(), contentKey)
                .addRecipient(carolPublicKey(), contentKey),
        );
        const urStr = envelope.urString();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
            ]
            `,
        );

        const receivedEnvelope = Envelope.fromUrString(urStr);

        // Bob decrypts, unwraps, then validates
        const bobPlaintext = receivedEnvelope
            .decryptSubjectToRecipient(bobPrivateKey())
            .unwrap()
            .verifySignatureFrom(alicePublicKey())
            .extractSubject<string>();
        expect(bobPlaintext).toBe(PLAINTEXT_HELLO);
    });

    test('secret with HKDF password', () => {
        const bobPassword = 'correct horse battery staple';
        const envelope = helloEnvelope()
            .lockWithPassword(KeyDerivationMethod.HKDF, bobPassword);

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            ENCRYPTED [
                'hasSecret': EncryptedKey(HKDF(SHA256))
            ]
            `,
        );

        const urStr = envelope.urString();
        const receivedEnvelope = Envelope.fromUrString(urStr);

        const bobPlaintext = receivedEnvelope
            .unlockWithPassword(bobPassword)
            .extractSubject<string>();
        expect(bobPlaintext).toBe(PLAINTEXT_HELLO);

        // Wrong password fails
        expect(() => receivedEnvelope.unlockWithPassword('wrong password')).toThrow();
    });

    test('multiple secrets HKDF and Scrypt', () => {
        const bobPassword = 'correct horse battery staple';
        const carolPassword = 'Able was I ere I saw Elba';
        const contentKey = SymmetricKey.new();
        const envelope = checkEncoding(
            helloEnvelope()
                .encryptSubject(contentKey)
                .addSecretWithPassword(KeyDerivationMethod.HKDF, bobPassword, contentKey)
                .addSecretWithPassword(KeyDerivationMethod.Scrypt, carolPassword, contentKey),
        );

        const format = envelope.format();
        expect(format).toContain('ENCRYPTED');
        expect(format).toContain("'hasSecret': EncryptedKey(HKDF(SHA256))");
        expect(format).toContain("'hasSecret': EncryptedKey(Scrypt)");

        const urStr = envelope.urString();
        const receivedEnvelope = Envelope.fromUrString(urStr);

        // Bob unlocks with HKDF password
        expect(
            receivedEnvelope
                .unlockSubjectWithPassword(bobPassword)
                .extractSubject<string>(),
        ).toBe(PLAINTEXT_HELLO);

        // Carol unlocks with Scrypt password
        expect(
            receivedEnvelope
                .unlockSubjectWithPassword(carolPassword)
                .extractSubject<string>(),
        ).toBe(PLAINTEXT_HELLO);

        // Wrong password fails
        expect(() => receivedEnvelope.unlockSubjectWithPassword('wrong password')).toThrow();
    });
});
