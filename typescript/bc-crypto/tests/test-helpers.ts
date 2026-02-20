import { expect } from 'vitest';

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

export function expectBytes(actual: Uint8Array, expectedHex: string): void {
    expect(actual).toEqual(hexToBytes(expectedHex));
}
