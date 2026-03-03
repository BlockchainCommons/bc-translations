import { bytesToHex, hexToBytes } from '@bc/dcbor';

import { BCComponentsError } from './error.js';

export function copyBytes(data: Uint8Array): Uint8Array {
    return new Uint8Array(data);
}

export function bytesEqual(a: Uint8Array, b: Uint8Array): boolean {
    if (a.length !== b.length) {
        return false;
    }
    for (let i = 0; i < a.length; i += 1) {
        if (a[i] !== b[i]) {
            return false;
        }
    }
    return true;
}

export function concatBytes(parts: readonly Uint8Array[]): Uint8Array {
    const total = parts.reduce((sum, part) => sum + part.length, 0);
    const out = new Uint8Array(total);
    let offset = 0;
    for (const part of parts) {
        out.set(part, offset);
        offset += part.length;
    }
    return out;
}

export function requireLength(
    data: Uint8Array,
    size: number,
    what: string,
): Uint8Array {
    if (data.length !== size) {
        throw BCComponentsError.invalidSize(what, size, data.length);
    }
    return data;
}

export function requireMinLength(
    data: Uint8Array,
    min: number,
    what: string,
): Uint8Array {
    if (data.length < min) {
        throw BCComponentsError.dataTooShort(what, min, data.length);
    }
    return data;
}

export function hexEncode(data: Uint8Array): string {
    return bytesToHex(data);
}

export function hexDecode(hex: string): Uint8Array {
    return hexToBytes(hex);
}

export function toUtf8(data: Uint8Array): string {
    return new TextDecoder().decode(data);
}

export function fromUtf8(text: string): Uint8Array {
    return new TextEncoder().encode(text);
}

export function toHexShort(data: Uint8Array): string {
    return hexEncode(data.slice(0, 4));
}

export function nanToString(value: number): string {
    return Number.isNaN(value) ? 'NaN' : value.toFixed(2);
}
