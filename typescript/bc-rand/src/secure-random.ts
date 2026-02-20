import { randomFillSync } from 'node:crypto';
import type { RandomNumberGenerator } from './random-number-generator.js';

/**
 * A random number generator that can be used as a source of
 * cryptographically-strong randomness.
 */
export class SecureRandomNumberGenerator implements RandomNumberGenerator {
    nextU32(): number {
        const buf = new Uint8Array(4);
        randomFillSync(buf);
        return new DataView(buf.buffer).getUint32(0, true);
    }

    nextU64(): bigint {
        const buf = new Uint8Array(8);
        randomFillSync(buf);
        return new DataView(buf.buffer).getBigUint64(0, true);
    }

    randomData(size: number): Uint8Array {
        const data = new Uint8Array(size);
        randomFillSync(data);
        return data;
    }

    fillRandomData(data: Uint8Array): void {
        randomFillSync(data);
    }
}

/**
 * Generate random bytes of the given size using cryptographically strong
 * randomness.
 *
 * @param size - The number of random bytes to generate.
 * @returns A new Uint8Array filled with cryptographically strong random bytes.
 */
export function secureRandomData(size: number): Uint8Array {
    const data = new Uint8Array(size);
    randomFillSync(data);
    return data;
}

/**
 * Fill the given Uint8Array with cryptographically strong random bytes.
 *
 * @param data - The Uint8Array to fill with random bytes.
 */
export function secureFillRandomData(data: Uint8Array): void {
    randomFillSync(data);
}
