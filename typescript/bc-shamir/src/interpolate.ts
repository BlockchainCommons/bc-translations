import { memzero, memzeroVecVecU8 } from '@bc/crypto';

import { MAX_SECRET_LEN } from './index.js';
import {
    bitslice,
    bitsliceSetall,
    gf256Add,
    gf256Inv,
    gf256Mul,
    unbitslice,
} from './hazmat.js';

/**
 * Calculate the Lagrange basis coefficients for the Lagrange polynomial
 * defined by the x coordinates `xc` at the value `x`.
 */
function hazmatLagrangeBasis(
    values: Uint8Array,
    n: number,
    xc: Uint8Array,
    x: number,
): void {
    const xx = new Uint8Array(32 + 16);
    const xSlice = new Uint32Array(8);
    const lxi: Uint32Array[] = [];
    for (let i = 0; i < n; i++) {
        lxi.push(new Uint32Array(8));
    }
    const numerator = new Uint32Array(8);
    const denominator = new Uint32Array(8);
    const temp = new Uint32Array(8);
    xx.set(xc.subarray(0, n));

    for (let i = 0; i < n; i++) {
        bitslice(lxi[i]!, xx.subarray(i));
        xx[i + n] = xx[i]!;
    }

    bitsliceSetall(xSlice, x);
    bitsliceSetall(numerator, 1);
    bitsliceSetall(denominator, 1);

    for (let i = 1; i < n; i++) {
        temp.set(xSlice);
        gf256Add(temp, lxi[i]!);
        const numerator2 = Uint32Array.from(numerator);
        gf256Mul(numerator, numerator2, temp);

        temp.set(lxi[0]!);
        gf256Add(temp, lxi[i]!);
        const denominator2 = Uint32Array.from(denominator);
        gf256Mul(denominator, denominator2, temp);
    }

    gf256Inv(temp, denominator);

    const numerator2 = Uint32Array.from(numerator);
    gf256Mul(numerator, numerator2, temp);

    unbitslice(xx, numerator);

    values.set(xx.subarray(0, n));
}

/**
 * Safely interpolate the polynomial going through
 * the points (x0 [y0_0 y0_1 ...]), (x1 [y1_0 ...]), ...
 */
export function interpolate(
    n: number,
    xi: Uint8Array,
    yLength: number,
    yij: Uint8Array[],
    x: number,
): Uint8Array {
    const y: Uint8Array[] = [];
    for (let i = 0; i < n; i++) {
        y.push(new Uint8Array(MAX_SECRET_LEN));
    }
    const values = new Uint8Array(MAX_SECRET_LEN);

    for (let i = 0; i < n; i++) {
        y[i]!.set(yij[i]!.subarray(0, yLength));
    }

    const lagrange = new Uint8Array(n);
    const ySlice = new Uint32Array(8);
    const resultSlice = new Uint32Array(8);
    const temp = new Uint32Array(8);

    hazmatLagrangeBasis(lagrange, n, xi, x);

    bitsliceSetall(resultSlice, 0);

    for (let i = 0; i < n; i++) {
        bitslice(ySlice, y[i]!);
        bitsliceSetall(temp, lagrange[i]!);
        const temp2 = Uint32Array.from(temp);
        gf256Mul(temp, temp2, ySlice);
        gf256Add(resultSlice, temp);
    }

    unbitslice(values, resultSlice);
    const result = new Uint8Array(yLength);
    result.set(values.subarray(0, yLength));

    // clean up stack
    memzero(lagrange);
    memzero(ySlice);
    memzero(resultSlice);
    memzero(temp);
    memzeroVecVecU8(y);
    memzero(values);

    return result;
}
