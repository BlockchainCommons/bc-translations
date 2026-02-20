/** Zero out a mutable array-like object. */
export function memzero(
    s: { length: number; [index: number]: number },
): void {
    for (let i = 0; i < s.length; i++) {
        s[i] = 0;
    }
}

/** Zero out a vector of vectors of bytes. */
export function memzeroVecVecU8(
    s: Array<{ length: number; [index: number]: number }>,
): void {
    for (const inner of s) {
        memzero(inner);
    }
}
