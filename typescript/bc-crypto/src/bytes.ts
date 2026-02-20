export type BytesLike = Uint8Array | ArrayLike<number> | string;

const encoder = new TextEncoder();

export function toBytes(value: BytesLike): Uint8Array {
    if (value instanceof Uint8Array) {
        return value;
    }
    if (typeof value === 'string') {
        return encoder.encode(value);
    }
    return Uint8Array.from(value);
}

export function requireLength(
    value: BytesLike,
    length: number,
    message: string,
): Uint8Array {
    const bytes = toBytes(value);
    if (bytes.length !== length) {
        throw new RangeError(message);
    }
    return bytes;
}

export function bytesToHex(bytes: Uint8Array): string {
    let out = '';
    for (const byte of bytes) {
        out += byte.toString(16).padStart(2, '0');
    }
    return out;
}
