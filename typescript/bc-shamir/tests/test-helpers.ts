import type { RandomNumberGenerator } from '@bc/rand';

export function hexToBytes(hex: string): Uint8Array {
    const clean = hex.replace(/\s+/g, '').toLowerCase();
    if (clean.length % 2 !== 0) {
        throw new Error('hex string must have even length');
    }
    const bytes = new Uint8Array(clean.length / 2);
    for (let i = 0; i < clean.length; i += 2) {
        bytes[i / 2] = parseInt(clean.slice(i, i + 2), 16);
    }
    return bytes;
}

/**
 * A deterministic RNG for testing that produces sequential bytes
 * starting at 0 with step 17 (wrapping at 256).
 *
 * Sequence: 0, 17, 34, 51, 68, 85, 102, 119, 136, 153, 170, 187, ...
 */
export class FakeRandomNumberGenerator implements RandomNumberGenerator {
    nextU32(): number {
        throw new Error('not implemented');
    }

    nextU64(): bigint {
        throw new Error('not implemented');
    }

    randomData(size: number): Uint8Array {
        const b = new Uint8Array(size);
        this.fillRandomData(b);
        return b;
    }

    fillRandomData(data: Uint8Array): void {
        let b = 0;
        for (let i = 0; i < data.length; i++) {
            data[i] = b;
            b = (b + 17) & 0xFF;
        }
    }
}
