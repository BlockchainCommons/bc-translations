import { describe, expect, test } from 'vitest';
import { SecureRandomNumberGenerator } from '@bc/rand';

import { splitSecret, recoverSecret } from '../src/index.js';

describe('shamir examples', () => {
    test('should split with secure rng', () => {
        const threshold = 2;
        const shareCount = 3;
        const secret = new TextEncoder().encode('my secret belongs to me.');
        const randomGenerator = new SecureRandomNumberGenerator();
        const shares = splitSecret(threshold, shareCount, secret, randomGenerator);
        expect(shares.length).toBe(shareCount);
    });

    test('should recover from known shares', () => {
        const indexes = [0, 2];
        const shares = [
            new Uint8Array([
                47, 165, 102, 232, 218, 99, 6, 94, 39, 6, 253, 215, 12, 88, 64,
                32, 105, 40, 222, 146, 93, 197, 48, 129,
            ]),
            new Uint8Array([
                221, 174, 116, 201, 90, 99, 136, 33, 64, 215, 60, 84, 207, 28,
                74, 10, 111, 243, 43, 224, 48, 64, 199, 172,
            ]),
        ];

        const secret = recoverSecret(indexes, shares);
        expect(secret).toEqual(new TextEncoder().encode('my secret belongs to me.'));
    });
});
