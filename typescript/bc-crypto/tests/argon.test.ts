import { describe, expect, test } from 'vitest';

import { argon2id } from '../src/index.js';

describe('argon', () => {
    test('testArgon2idBasic', () => {
        const pass = 'password';
        const salt = 'example salt';
        const output = argon2id(pass, salt, 32);
        expect(output.length).toBe(32);

        const output2 = argon2id(pass, salt, 32);
        expect(output2).toEqual(output);
    });

    test('testArgon2idDifferentSalt', () => {
        const pass = 'password';
        const out1 = argon2id(pass, 'example salt', 32);
        const out2 = argon2id(pass, 'example salt2', 32);
        expect(out1).not.toEqual(out2);
    });
});
