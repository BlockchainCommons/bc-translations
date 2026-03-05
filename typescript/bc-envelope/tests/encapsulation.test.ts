import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import {
    alicePrivateKey,
    alicePublicKey,
} from './test-data.js';

describe('encapsulation tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('encrypt/decrypt to recipient', () => {
        const envelope = Envelope.from('recipient payload').addAssertion('v', 1);
        const encrypted = envelope.encryptToRecipient(alicePublicKey());
        const decrypted = encrypted.decryptToRecipient(alicePrivateKey());

        expect(encrypted.subject().isEncrypted()).toBe(true);
        expect(decrypted.isIdenticalTo(envelope)).toBe(true);
    });
});
