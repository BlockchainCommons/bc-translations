import { describe, test, expect } from 'vitest';
import { createFakeRandomNumberGenerator, rngNextInClosedRange } from '../src/index.js';

describe('RandomNumberGenerator', () => {
    test('fakeNumbers', () => {
        const rng = createFakeRandomNumberGenerator();
        const values: bigint[] = [];
        for (let i = 0; i < 100; i++) {
            values.push(rngNextInClosedRange(rng, -50n, 50n, 32));
        }
        const expected: bigint[] = [
            -43n, -6n, 43n, -34n, -34n, 17n, -9n, 24n, 17n, -29n,
            -32n, -44n, 12n, -15n, -46n, 20n, 50n, -31n, -50n, 36n,
            -28n, -23n, 6n, -27n, -31n, -45n, -27n, 26n, 31n, -23n,
            24n, 19n, -32n, 43n, -18n, -17n, 6n, -13n, -1n, -27n,
            4n, -48n, -4n, -44n, -6n, 17n, -15n, 22n, 15n, 20n,
            -25n, -35n, -33n, -27n, -17n, -44n, -27n, 15n, -14n, -38n,
            -29n, -12n, 8n, 43n, 49n, -42n, -11n, -1n, -42n, -26n,
            -25n, 22n, -13n, 14n, 42n, -29n, -38n, 17n, 2n, 5n,
            5n, -31n, 27n, -3n, 39n, -12n, 42n, 46n, -17n, -25n,
            -46n, -19n, 16n, 2n, -45n, 41n, 12n, -22n, 43n, -11n,
        ];
        expect(values).toEqual(expected);
    });
});
