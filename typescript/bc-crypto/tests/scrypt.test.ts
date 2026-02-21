import { describe, expect, test } from 'vitest';

import { scrypt, scryptWithParams } from '../src/index.js';

describe('scrypt', () => {
    test('deterministic output', () => {
        const pass = 'password';
        const salt = 'salt';
        const output = scrypt(pass, salt, 32);
        expect(output.length).toBe(32);

        const output2 = scrypt(pass, salt, 32);
        expect(output2).toEqual(output);
    });

    test('different salt produces different output', () => {
        const pass = 'password';
        const out1 = scrypt(pass, 'salt1', 32);
        const out2 = scrypt(pass, 'salt2', 32);
        expect(out1).not.toEqual(out2);
    });

    test('custom parameters', () => {
        const output = scryptWithParams('password', 'salt', 32, 15, 8, 1);
        expect(output.length).toBe(32);
    });

    test('variable output lengths', () => {
        for (const len of [16, 24, 32, 64]) {
            const output = scrypt('password', 'salt', len);
            expect(output.length).toBe(len);
        }
    });
});
