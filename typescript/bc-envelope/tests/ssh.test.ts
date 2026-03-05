import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import { alicePrivateKey } from './test-data.js';

describe('ssh tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('ssh signing key generation is unsupported in TS translation', () => {
        expect(() => alicePrivateKey().sshSigningPrivateKey('ssh-ed25519', 'demo')).toThrow();
    });

    test('isLockedWithSshAgent false for non-ssh secrets', () => {
        const envelope = Envelope.from('x');
        expect(envelope.isLockedWithSshAgent()).toBe(false);
    });
});
