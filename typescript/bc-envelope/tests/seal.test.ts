import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import {
    alicePrivateKey,
    alicePublicKey,
    bobPrivateKey,
    bobPublicKey,
} from './test-data.js';

describe('seal tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('seal and unseal', () => {
        const message = 'Top secret message';
        const originalEnvelope = Envelope.from(message);

        const sealedEnvelope = originalEnvelope.seal(alicePrivateKey(), bobPublicKey());
        expect(sealedEnvelope.isSubjectEncrypted()).toBe(true);

        const unsealedEnvelope = sealedEnvelope.unseal(alicePublicKey(), bobPrivateKey());
        expect(unsealedEnvelope.extractSubject<string>()).toBe(message);
    });

    test('sealOpt and unseal', () => {
        const message = 'Confidential data';
        const originalEnvelope = Envelope.from(message);

        const sealedEnvelope = originalEnvelope.sealOpt(alicePrivateKey(), bobPublicKey());
        expect(sealedEnvelope.isSubjectEncrypted()).toBe(true);

        const unsealedEnvelope = sealedEnvelope.unseal(alicePublicKey(), bobPrivateKey());
        expect(unsealedEnvelope.extractSubject<string>()).toBe(message);
    });
});
