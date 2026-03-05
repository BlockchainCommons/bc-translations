import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import {
    alicePrivateKey,
    alicePublicKey,
    bobPrivateKey,
    bobPublicKey,
    carolPrivateKey,
    carolPublicKey,
} from './test-data.js';

describe('multi permit tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('multi-signer threshold and verification', () => {
        const envelope = Envelope
            .from('permit')
            .addSignatures([alicePrivateKey(), bobPrivateKey(), carolPrivateKey()]);

        const verifiers = [alicePublicKey(), bobPublicKey(), carolPublicKey()];
        expect(envelope.hasSignaturesFromThreshold(verifiers, 2)).toBe(true);
        expect(envelope.hasSignaturesFromThreshold(verifiers, 3)).toBe(true);
        expect(envelope.verifySignaturesFromThreshold(verifiers, 2).isIdenticalTo(envelope)).toBe(true);
    });
});
