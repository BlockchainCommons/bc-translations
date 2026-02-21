import { memzero } from '@bc/crypto';

/**
 * Pack up to 32 bytes from `x` into bitsliced form in `r` (Uint32Array of length 8).
 */
export function bitslice(r: Uint32Array, x: Uint8Array): void {
    memzero(r);
    for (let arrIdx = 0; arrIdx < 32; arrIdx++) {
        const cur = x[arrIdx]!;
        for (let bitIdx = 0; bitIdx < 8; bitIdx++) {
            r[bitIdx] = (r[bitIdx]! |
                (((cur & (1 << bitIdx)) >>> bitIdx) << arrIdx)) >>> 0;
        }
    }
}

/**
 * Unpack bitsliced form `x` (Uint32Array of length 8) into 32 bytes in `r`.
 */
export function unbitslice(r: Uint8Array, x: Uint32Array): void {
    memzero(r);
    for (let bitIdx = 0; bitIdx < 8; bitIdx++) {
        const cur = x[bitIdx]!;
        for (let arrIdx = 0; arrIdx < 32; arrIdx++) {
            r[arrIdx] = (r[arrIdx]! |
                ((((cur & (1 << arrIdx)) >>> arrIdx) << bitIdx) & 0xFF)) & 0xFF;
        }
    }
}

/**
 * Set all 32 parallel elements in the bitsliced representation to `x`.
 */
export function bitsliceSetall(r: Uint32Array, x: number): void {
    for (let idx = 0; idx < 8; idx++) {
        r[idx] = ((((x & (1 << idx)) << (31 - idx)) | 0) >> 31) >>> 0;
    }
}

/**
 * Add (XOR) `r` with `x` and store the result in `r`.
 */
export function gf256Add(r: Uint32Array, x: Uint32Array): void {
    for (let i = 0; i < 8; i++) {
        r[i] = (r[i]! ^ x[i]!) >>> 0;
    }
}

/**
 * Safely multiply two bitsliced polynomials in GF(2^8) reduced by
 * x^8 + x^4 + x^3 + x + 1. `r` and `a` may overlap, but overlapping of `r`
 * and `b` will produce an incorrect result! If you need to square a polynomial
 * use `gf256Square` instead.
 */
export function gf256Mul(r: Uint32Array, a: Uint32Array, b: Uint32Array): void {
    const a2 = Uint32Array.from(a);

    r[0] = (a2[0]! & b[0]!) >>> 0;
    r[1] = (a2[1]! & b[0]!) >>> 0;
    r[2] = (a2[2]! & b[0]!) >>> 0;
    r[3] = (a2[3]! & b[0]!) >>> 0;
    r[4] = (a2[4]! & b[0]!) >>> 0;
    r[5] = (a2[5]! & b[0]!) >>> 0;
    r[6] = (a2[6]! & b[0]!) >>> 0;
    r[7] = (a2[7]! & b[0]!) >>> 0;
    a2[0] = (a2[0]! ^ a2[7]!) >>> 0; // reduce
    a2[2] = (a2[2]! ^ a2[7]!) >>> 0;
    a2[3] = (a2[3]! ^ a2[7]!) >>> 0;

    r[0] = (r[0]! ^ (a2[7]! & b[1]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[0]! & b[1]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[1]! & b[1]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[2]! & b[1]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[3]! & b[1]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[4]! & b[1]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[5]! & b[1]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[6]! & b[1]!)) >>> 0;
    a2[7] = (a2[7]! ^ a2[6]!) >>> 0; // reduce
    a2[1] = (a2[1]! ^ a2[6]!) >>> 0;
    a2[2] = (a2[2]! ^ a2[6]!) >>> 0;

    r[0] = (r[0]! ^ (a2[6]! & b[2]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[7]! & b[2]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[0]! & b[2]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[1]! & b[2]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[2]! & b[2]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[3]! & b[2]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[4]! & b[2]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[5]! & b[2]!)) >>> 0;
    a2[6] = (a2[6]! ^ a2[5]!) >>> 0; // reduce
    a2[0] = (a2[0]! ^ a2[5]!) >>> 0;
    a2[1] = (a2[1]! ^ a2[5]!) >>> 0;

    r[0] = (r[0]! ^ (a2[5]! & b[3]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[6]! & b[3]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[7]! & b[3]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[0]! & b[3]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[1]! & b[3]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[2]! & b[3]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[3]! & b[3]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[4]! & b[3]!)) >>> 0;
    a2[5] = (a2[5]! ^ a2[4]!) >>> 0; // reduce
    a2[7] = (a2[7]! ^ a2[4]!) >>> 0;
    a2[0] = (a2[0]! ^ a2[4]!) >>> 0;

    r[0] = (r[0]! ^ (a2[4]! & b[4]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[5]! & b[4]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[6]! & b[4]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[7]! & b[4]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[0]! & b[4]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[1]! & b[4]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[2]! & b[4]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[3]! & b[4]!)) >>> 0;
    a2[4] = (a2[4]! ^ a2[3]!) >>> 0; // reduce
    a2[6] = (a2[6]! ^ a2[3]!) >>> 0;
    a2[7] = (a2[7]! ^ a2[3]!) >>> 0;

    r[0] = (r[0]! ^ (a2[3]! & b[5]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[4]! & b[5]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[5]! & b[5]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[6]! & b[5]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[7]! & b[5]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[0]! & b[5]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[1]! & b[5]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[2]! & b[5]!)) >>> 0;
    a2[3] = (a2[3]! ^ a2[2]!) >>> 0; // reduce
    a2[5] = (a2[5]! ^ a2[2]!) >>> 0;
    a2[6] = (a2[6]! ^ a2[2]!) >>> 0;

    r[0] = (r[0]! ^ (a2[2]! & b[6]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[3]! & b[6]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[4]! & b[6]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[5]! & b[6]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[6]! & b[6]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[7]! & b[6]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[0]! & b[6]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[1]! & b[6]!)) >>> 0;
    a2[2] = (a2[2]! ^ a2[1]!) >>> 0; // reduce
    a2[4] = (a2[4]! ^ a2[1]!) >>> 0;
    a2[5] = (a2[5]! ^ a2[1]!) >>> 0;

    r[0] = (r[0]! ^ (a2[1]! & b[7]!)) >>> 0; // add
    r[1] = (r[1]! ^ (a2[2]! & b[7]!)) >>> 0;
    r[2] = (r[2]! ^ (a2[3]! & b[7]!)) >>> 0;
    r[3] = (r[3]! ^ (a2[4]! & b[7]!)) >>> 0;
    r[4] = (r[4]! ^ (a2[5]! & b[7]!)) >>> 0;
    r[5] = (r[5]! ^ (a2[6]! & b[7]!)) >>> 0;
    r[6] = (r[6]! ^ (a2[7]! & b[7]!)) >>> 0;
    r[7] = (r[7]! ^ (a2[0]! & b[7]!)) >>> 0;
}

/**
 * Square `x` in GF(2^8) and write the result to `r`. `r` and `x` may overlap.
 */
export function gf256Square(r: Uint32Array, x: Uint32Array): void {
    let r8: number;
    let r10: number;
    // Use the Freshman's Dream rule to square the polynomial
    // Assignments are done from 7 downto 0, because this allows the user
    // to execute this function in-place (e.g. `gf256Square(r, r);`).
    const r14 = x[7]!;
    const r12 = x[6]!;
    r10 = x[5]!;
    r8 = x[4]!;
    r[6] = x[3]!;
    r[4] = x[2]!;
    r[2] = x[1]!;
    r[0] = x[0]!;

    // Reduce with x^8 + x^4 + x^3 + x + 1 until order is less than 8
    r[7] = r14; // r[7] was 0
    r[6] = (r[6]! ^ r14) >>> 0;
    r10 = (r10 ^ r14) >>> 0;
    // Skip, because r13 is always 0
    r[4] = (r[4]! ^ r12) >>> 0;
    r[5] = r12; // r[5] was 0
    r[7] = (r[7]! ^ r12) >>> 0;
    r8 = (r8 ^ r12) >>> 0;
    // Skip, because r11 is always 0
    r[2] = (r[2]! ^ r10) >>> 0;
    r[3] = r10; // r[3] was 0
    r[5] = (r[5]! ^ r10) >>> 0;
    r[6] = (r[6]! ^ r10) >>> 0;
    r[1] = r14; // r[1] was 0
    r[2] = (r[2]! ^ r14) >>> 0; // Substitute r9 by r14 because they will always be equal
    r[4] = (r[4]! ^ r14) >>> 0;
    r[5] = (r[5]! ^ r14) >>> 0;
    r[0] = (r[0]! ^ r8) >>> 0;
    r[1] = (r[1]! ^ r8) >>> 0;
    r[3] = (r[3]! ^ r8) >>> 0;
    r[4] = (r[4]! ^ r8) >>> 0;
}

/**
 * Invert `x` in GF(2^8) and write the result to `r`.
 */
export function gf256Inv(r: Uint32Array, x: Uint32Array): void {
    const y = new Uint32Array(8);
    const z = new Uint32Array(8);

    gf256Square(y, x);              // y = x^2
    const y2 = Uint32Array.from(y);
    gf256Square(y, y2);             // y = x^4
    gf256Square(r, y);              // r = x^8
    gf256Mul(z, r, x);              // z = x^9
    const r2a = Uint32Array.from(r);
    gf256Square(r, r2a);            // r = x^16
    const r2b = Uint32Array.from(r);
    gf256Mul(r, r2b, z);            // r = x^25
    const r2c = Uint32Array.from(r);
    gf256Square(r, r2c);            // r = x^50
    gf256Square(z, r);              // z = x^100
    const z2 = Uint32Array.from(z);
    gf256Square(z, z2);             // z = x^200
    const r2d = Uint32Array.from(r);
    gf256Mul(r, r2d, z);            // r = x^250
    const r2e = Uint32Array.from(r);
    gf256Mul(r, r2e, y);            // r = x^254
}
