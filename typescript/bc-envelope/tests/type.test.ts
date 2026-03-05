import { beforeAll, describe, expect, test } from 'vitest';
import { NOTE, SEED_TYPE } from '@bc/known-values';
import { CborDate, bytesToHex } from '@bc/dcbor';
import { createFakeRandomNumberGenerator, fakeRandomData, rngNextInClosedRange } from '@bc/rand';

import { Envelope, registerTags } from '../src/index.js';
import { checkEncoding } from './test-data.js';

describe('type tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('add and check known-value type', () => {
        const envelope = Envelope.from('seed').addType(SEED_TYPE);

        expect(envelope.hasType(SEED_TYPE)).toBe(true);
        expect(envelope.hasTypeValue(SEED_TYPE)).toBe(true);

        envelope.checkType(SEED_TYPE);
        envelope.checkTypeValue(SEED_TYPE);
    });

    test('type mismatch fails', () => {
        const envelope = Envelope.from('seed').addType(SEED_TYPE);
        expect(() => envelope.checkTypeValue(NOTE)).toThrow();
    });

    test('date', () => {
        const date = CborDate.fromString('2018-01-07');
        const envelope = checkEncoding(Envelope.from(date));
        expect(envelope.format()).toBe('2018-01-07');
    });

    test('fake random data', () => {
        expect(bytesToHex(fakeRandomData(100))).toBe(
            '7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d3545532daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a564e59b4e2',
        );
    });

    test('fake numbers', () => {
        const rng = createFakeRandomNumberGenerator();
        const array: number[] = [];
        for (let i = 0; i < 100; i++) {
            array.push(Number(rngNextInClosedRange(rng, -50n, 50n, 32)));
        }
        expect(JSON.stringify(array)).toBe(
            JSON.stringify([
                -43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15, -46, 20,
                50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26, 31, -23, 24, 19,
                -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4, -44, -6, 17, -15, 22,
                15, 20, -25, -35, -33, -27, -17, -44, -27, 15, -14, -38, -29, -12, 8, 43,
                49, -42, -11, -1, -42, -26, -25, 22, -13, 14, 42, -29, -38, 17, 2, 5,
                5, -31, 27, -3, 39, -12, 42, 46, -17, -25, -46, -19, 16, 2, -45, 41,
                12, -22, 43, -11,
            ]),
        );
    });
});
