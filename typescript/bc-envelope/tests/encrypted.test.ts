import { beforeAll, describe, expect, test } from 'vitest';
import { KeyDerivationMethod } from '@bc/components';

import { Envelope, registerTags } from '../src/index.js';

describe('encrypted tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('lock and unlock with password', () => {
        const envelope = Envelope.from('secret payload').addAssertion('v', 1);
        const locked = envelope.lockWithPassword(KeyDerivationMethod.Argon2id, 'correct horse');

        expect(locked.isLockedWithPassword()).toBe(true);

        const unlocked = locked.unlockWithPassword('correct horse');
        expect(unlocked.isIdenticalTo(envelope)).toBe(true);
    });

    test('unlock with wrong password fails', () => {
        const locked = Envelope.from('secret').lockWithPassword(KeyDerivationMethod.Argon2id, 'pw1');
        expect(() => locked.unlockWithPassword('pw2')).toThrow();
    });
});
