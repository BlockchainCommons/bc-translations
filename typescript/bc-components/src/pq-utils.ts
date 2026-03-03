import { SecureRandomNumberGenerator } from '@bc/rand';
import { sha256 } from '@bc/crypto';

import { concatBytes } from './utils.js';

export function randomBytes(length: number): Uint8Array {
    const rng = new SecureRandomNumberGenerator();
    return rng.randomData(length);
}

export function expandBytes(seed: Uint8Array, label: string, length: number): Uint8Array {
    const labelBytes = new TextEncoder().encode(label);
    const chunks: Uint8Array[] = [];
    let counter = 0;
    while (chunks.reduce((sum, chunk) => sum + chunk.length, 0) < length) {
        const counterBytes = new Uint8Array(4);
        new DataView(counterBytes.buffer).setUint32(0, counter++, false);
        chunks.push(sha256(concatBytes([seed, labelBytes, counterBytes])));
    }
    return concatBytes(chunks).slice(0, length);
}
