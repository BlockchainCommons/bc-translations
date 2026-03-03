import { expect } from 'vitest';

export function hexToBytes(hex: string): Uint8Array {
    return Uint8Array.from(Buffer.from(hex, 'hex'));
}

export function utf8(value: string): Uint8Array {
    return new TextEncoder().encode(value);
}

export function expectBytes(actual: Uint8Array, expectedHex: string): void {
    expect(Buffer.from(actual).toString('hex')).toBe(expectedHex.toLowerCase());
}
