import { expect } from 'vitest';
import { bytesToHex, hexToBytes as dcborHexToBytes } from '@bc/dcbor';

export function hexToBytes(hex: string): Uint8Array {
    return dcborHexToBytes(hex);
}

export function utf8(value: string): Uint8Array {
    return new TextEncoder().encode(value);
}

export function expectBytes(actual: Uint8Array, expectedHex: string): void {
    expect(bytesToHex(actual)).toBe(expectedHex.toLowerCase());
}
