/** Accepted input types for functions that operate on binary data. */
export type BytesLike = Uint8Array | ArrayLike<number> | string;

const encoder = new TextEncoder();

/** Converts a {@link BytesLike} value to a `Uint8Array`. */
export function toBytes(value: BytesLike): Uint8Array {
    if (value instanceof Uint8Array) {
        return value;
    }
    if (typeof value === 'string') {
        return encoder.encode(value);
    }
    return Uint8Array.from(value);
}

/**
 * Converts a {@link BytesLike} value to a `Uint8Array` and asserts that its
 * length matches the expected size.
 *
 * @throws {RangeError} If the converted byte length does not equal `length`.
 */
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

/** Encodes a `Uint8Array` as a lowercase hexadecimal string. */
export function bytesToHex(bytes: Uint8Array): string {
    let out = '';
    for (const byte of bytes) {
        out += byte.toString(16).padStart(2, '0');
    }
    return out;
}
