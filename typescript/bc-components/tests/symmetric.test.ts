import { describe, expect, test } from 'vitest';

import { Digest, SymmetricKey } from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('Symmetric', () => {
    test('encrypt/decrypt with aad', () => {
        const key = SymmetricKey.new();
        const plaintext = utf8('symmetric test');
        const aad = utf8('context');

        const encrypted = key.encrypt(plaintext, aad);
        expect(key.decrypt(encrypted)).toEqual(plaintext);
    });

    test('encrypt/decrypt with digest aad', () => {
        const key = SymmetricKey.new();
        const plaintext = utf8('digest aad');
        const digest = Digest.fromImage(plaintext);

        const encrypted = key.encryptWithDigest(plaintext, digest);
        expect(key.decrypt(encrypted)).toEqual(plaintext);
        expect(encrypted.hasDigest()).toBe(true);
    });
});
