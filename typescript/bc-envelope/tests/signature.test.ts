import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags, SignatureMetadata } from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';
import {
    alicePrivateKey,
    alicePublicKey,
    bobPrivateKey,
    bobPublicKey,
    carolPrivateKey,
    carolPublicKey,
    checkEncoding,
    helloEnvelope,
    PLAINTEXT_HELLO,
} from './test-data.js';

describe('signature tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('signature with metadata roundtrip', () => {
        const metadata = new SignatureMetadata()
            .withAssertion('purpose', 'demo')
            .withAssertion('issuer', 'alice');

        const envelope = Envelope
            .from('payload')
            .addSignatureOpt(alicePrivateKey(), undefined, metadata);

        expect(envelope.hasSignatureFrom(alicePublicKey())).toBe(true);

        const metadataEnvelope = envelope.verifySignatureFromReturningMetadata(alicePublicKey());
        expect(metadataEnvelope.extractObjectForPredicate('purpose')).toBe('demo');
    });

    test('threshold signature verification', () => {
        const envelope = Envelope
            .from('threshold')
            .addSignatures([alicePrivateKey(), bobPrivateKey(), carolPrivateKey()]);

        expect(envelope.hasSignaturesFromThreshold([alicePublicKey(), bobPublicKey(), carolPublicKey()], 2)).toBe(true);
        expect(envelope.verifySignaturesFromThreshold([alicePublicKey(), bobPublicKey(), carolPublicKey()], 2).isIdenticalTo(envelope)).toBe(true);
    });

    test('signed plaintext roundtrip', () => {
        const envelope = checkEncoding(
            helloEnvelope().addSignature(alicePrivateKey()),
        );
        const urStr = envelope.urString();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Hello." [
                'signed': Signature
            ]
            `,
        );

        // Bob receives the envelope
        const receivedEnvelope = Envelope.fromUrString(urStr);

        // Verify Alice's signature and read message
        const receivedPlaintext = receivedEnvelope
            .verifySignatureFrom(alicePublicKey())
            .extractSubject<string>();
        expect(receivedPlaintext).toBe('Hello.');

        // Confirm it wasn't signed by Carol
        expect(() => receivedEnvelope.verifySignatureFrom(carolPublicKey())).toThrow();

        // Confirm it was signed by Alice OR Carol (threshold 1)
        receivedEnvelope.verifySignaturesFromThreshold(
            [alicePublicKey(), carolPublicKey()],
            1,
        );

        // Confirm it was NOT signed by Alice AND Carol (threshold 2)
        expect(() =>
            receivedEnvelope.verifySignaturesFromThreshold(
                [alicePublicKey(), carolPublicKey()],
                2,
            ),
        ).toThrow();
    });

    test('multisigned plaintext', () => {
        const envelope = checkEncoding(
            helloEnvelope().addSignatures([alicePrivateKey(), carolPrivateKey()]),
        );

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            "Hello." [
                'signed': Signature
                'signed': Signature
            ]
            `,
        );

        const urStr = envelope.urString();

        // Verify both signatures and read message
        const receivedPlaintext = Envelope.fromUrString(urStr)
            .verifySignaturesFrom([alicePublicKey(), carolPublicKey()])
            .extractSubject<string>();
        expect(receivedPlaintext).toBe(PLAINTEXT_HELLO);
    });
});
