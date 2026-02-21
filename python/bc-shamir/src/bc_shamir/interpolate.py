"""Lagrange interpolation over GF(2^8) for Shamir secret sharing."""

from __future__ import annotations

from collections.abc import Sequence

from bc_crypto import memzero, memzero_vec_vec_u8

from .constants import MAX_SECRET_LEN
from .hazmat import (
    bitslice,
    bitslice_setall,
    gf256_add,
    gf256_inv,
    gf256_mul,
    unbitslice,
)


def _hazmat_lagrange_basis(
    values: bytearray,
    n: int,
    xc: Sequence[int],
    x: int,
) -> None:
    xx = bytearray(32 + 16)
    x_slice = [0] * 8
    lxi = [[0] * 8 for _ in range(n)]
    numerator = [0] * 8
    denominator = [0] * 8
    temp = [0] * 8

    xx[:n] = bytes(xc[:n])

    for i in range(n):
        bitslice(lxi[i], xx[i:])
        xx[i + n] = xx[i]

    bitslice_setall(x_slice, x)
    bitslice_setall(numerator, 1)
    bitslice_setall(denominator, 1)

    for i in range(1, n):
        temp[:] = x_slice
        gf256_add(temp, lxi[i])
        numerator2 = numerator.copy()
        gf256_mul(numerator, numerator2, temp)

        temp[:] = lxi[0]
        gf256_add(temp, lxi[i])
        denominator2 = denominator.copy()
        gf256_mul(denominator, denominator2, temp)

    gf256_inv(temp, denominator)
    numerator2 = numerator.copy()
    gf256_mul(numerator, numerator2, temp)

    unbitslice(xx, numerator)
    values[:n] = xx[:n]


def interpolate(
    n: int,
    xi: Sequence[int],
    yl: int,
    yij: Sequence[bytes | bytearray | memoryview],
    x: int,
) -> bytes:
    y = [bytearray(MAX_SECRET_LEN) for _ in range(n)]
    values = bytearray(MAX_SECRET_LEN)

    for i in range(n):
        y[i][:yl] = bytes(yij[i][:yl])

    lagrange = bytearray(n)
    y_slice = [0] * 8
    result_slice = [0] * 8
    temp = [0] * 8

    _hazmat_lagrange_basis(lagrange, n, xi, x)

    bitslice_setall(result_slice, 0)

    for i in range(n):
        bitslice(y_slice, y[i])
        bitslice_setall(temp, lagrange[i])
        temp2 = temp.copy()
        gf256_mul(temp, temp2, y_slice)
        gf256_add(result_slice, temp)

    unbitslice(values, result_slice)
    result = bytes(values[:yl])

    memzero(lagrange)
    memzero(y_slice)
    memzero(result_slice)
    memzero(temp)
    memzero_vec_vec_u8(y)
    memzero(values)

    return result


__all__ = ["interpolate"]
