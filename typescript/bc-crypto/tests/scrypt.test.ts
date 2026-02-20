import { describe, expect, test } from 'vitest';

import { scrypt, scryptOpt } from '../src/index.js';

describe('scrypt', () => {
    test('testScryptBasic', () => {
        const pass = 'password';
        const salt = 'salt';
        const output = scrypt(pass, salt, 32);
        expect(output.length).toBe(32);

        const output2 = scrypt(pass, salt, 32);
        expect(output2).toEqual(output);
    });

    test('testScryptDifferentSalt', () => {
        const pass = 'password';
        const out1 = scrypt(pass, 'salt1', 32);
        const out2 = scrypt(pass, 'salt2', 32);
        expect(out1).not.toEqual(out2);
    });

    test('testScryptOptBasic', () => {
        const output = scryptOpt('password', 'salt', 32, 15, 8, 1);
        expect(output.length).toBe(32);
    });

    test('testScryptOutputLength', () => {
        for (const len of [16, 24, 32, 64]) {
            const output = scrypt('password', 'salt', len);
            expect(output.length).toBe(len);
        }
    });
});
