/**
 * A typed array or buffer that can be zeroed.
 * Covers `Uint8Array`, `Uint32Array`, and all other TypedArray variants.
 */
type Zeroable = { readonly length: number; fill(value: number): unknown };

/** Zero out a typed array in place. */
export function memzero(buffer: Zeroable): void {
    buffer.fill(0);
}

/** Zero out each buffer in an array of `Uint8Array`. */
export function memzeroAll(buffers: Uint8Array[]): void {
    for (const buffer of buffers) {
        memzero(buffer);
    }
}
