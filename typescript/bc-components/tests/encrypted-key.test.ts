import { describe, expect, test } from 'vitest';

import {
    EncryptedKey,
    KeyDerivationMethod,
    SymmetricKey,
    registerTags,
} from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('EncryptedKey', () => {
    test('lock/unlock roundtrips across derivation methods', () => {
        registerTags();
        const secret = utf8('correct horse battery staple');

        const methods = [
            KeyDerivationMethod.HKDF,
            KeyDerivationMethod.PBKDF2,
            KeyDerivationMethod.Scrypt,
            KeyDerivationMethod.Argon2id,
        ];

        for (const method of methods) {
            const contentKey = SymmetricKey.new();
            try {
                const encrypted = EncryptedKey.lock(method, secret, contentKey);
                const roundtrip = EncryptedKey.fromCbor(encrypted.taggedCbor());
                const decrypted = roundtrip.unlock(secret);
                expect(decrypted.equals(contentKey)).toBe(true);
            } catch (error) {
                if (
                    method === KeyDerivationMethod.Argon2id
                    && error instanceof Error
                    && error.message.includes('argon2 unavailable')
                ) {
                    continue;
                }
                throw error;
            }
        }
    });

    test('wrong secret fails', () => {
        const secret = utf8('correct horse battery staple');
        const wrongSecret = utf8('wrong secret');
        const contentKey = SymmetricKey.new();

        const encrypted = EncryptedKey.lock(KeyDerivationMethod.HKDF, secret, contentKey);
        expect(() => encrypted.unlock(wrongSecret)).toThrow();
    });
});
