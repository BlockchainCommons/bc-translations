import { randomFillSync } from 'node:crypto';
import type { RandomNumberGenerator } from './random-number-generator.js';

/**
 * A cryptographically-secure random number generator.
 *
 * Uses Node.js crypto.randomFillSync for cryptographic randomness.
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

/** Returns a new Uint8Array of the given size filled with cryptographically-secure random bytes. */
export function secureRandomData(size: number): Uint8Array {
    const data = new Uint8Array(size);
    randomFillSync(data);
    return data;
}

/** Fills the given Uint8Array with cryptographically-secure random bytes. */
export function secureFillRandomData(data: Uint8Array): void {
    randomFillSync(data);
}
