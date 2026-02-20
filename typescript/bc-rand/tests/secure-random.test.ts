import { describe, test, expect } from 'vitest';
import { secureRandomData } from '../src/index.js';

describe('SecureRandomNumberGenerator', () => {
    test('randomData', () => {
        const data1 = secureRandomData(32);
        const data2 = secureRandomData(32);
        const data3 = secureRandomData(32);
        expect(data1.length).toBe(32);
        expect(data1).not.toEqual(data2);
        expect(data1).not.toEqual(data3);
    });
});
