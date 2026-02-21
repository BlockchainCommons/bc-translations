"""Bitsliced GF(2^8) primitives used by Shamir interpolation.

These are internal implementation details and should not be used directly.
"""

from __future__ import annotations

from collections.abc import MutableSequence, Sequence

from bc_crypto import memzero

_MASK32 = 0xFFFFFFFF


def _u32(v: int) -> int:
    return v & _MASK32


def bitslice(r: MutableSequence[int], x: bytes | bytearray | Sequence[int]) -> None:
    if len(x) < 32:
        raise ValueError("bitslice input must be at least 32 bytes")
    memzero(r)
    for arr_idx, cur in enumerate(x[:32]):
        cur_u32 = int(cur)
        for bit_idx in range(8):
            r[bit_idx] = _u32(
                r[bit_idx]
                | (((cur_u32 & (1 << bit_idx)) >> bit_idx) << arr_idx)
            )


def unbitslice(r: MutableSequence[int], x: Sequence[int]) -> None:
    if len(r) < 32:
        raise ValueError("unbitslice output must be at least 32 elements")
    memzero(r)
    for bit_idx, cur in enumerate(x):
        for arr_idx in range(32):
            r[arr_idx] = int(r[arr_idx]) | (
                ((int(cur) & (1 << arr_idx)) >> arr_idx) << bit_idx
            )


def bitslice_setall(r: MutableSequence[int], x: int) -> None:
    x_u8 = x & 0xFF
    for idx in range(8):
        r[idx] = _MASK32 if ((x_u8 >> idx) & 1) else 0


def gf256_add(r: MutableSequence[int], x: Sequence[int]) -> None:
    """Add (XOR) ``r`` with ``x`` and store the result in ``r``."""
    for idx in range(8):
        r[idx] = _u32(int(r[idx]) ^ int(x[idx]))


def gf256_mul(r: MutableSequence[int], a: Sequence[int], b: Sequence[int]) -> None:
    """Multiply two bitsliced polynomials in GF(2^8)."""
    a2 = [int(v) & _MASK32 for v in a]

    r[0] = _u32(a2[0] & b[0])
    r[1] = _u32(a2[1] & b[0])
    r[2] = _u32(a2[2] & b[0])
    r[3] = _u32(a2[3] & b[0])
    r[4] = _u32(a2[4] & b[0])
    r[5] = _u32(a2[5] & b[0])
    r[6] = _u32(a2[6] & b[0])
    r[7] = _u32(a2[7] & b[0])
    a2[0] = _u32(a2[0] ^ a2[7])
    a2[2] = _u32(a2[2] ^ a2[7])
    a2[3] = _u32(a2[3] ^ a2[7])

    r[0] = _u32(r[0] ^ (a2[7] & b[1]))
    r[1] = _u32(r[1] ^ (a2[0] & b[1]))
    r[2] = _u32(r[2] ^ (a2[1] & b[1]))
    r[3] = _u32(r[3] ^ (a2[2] & b[1]))
    r[4] = _u32(r[4] ^ (a2[3] & b[1]))
    r[5] = _u32(r[5] ^ (a2[4] & b[1]))
    r[6] = _u32(r[6] ^ (a2[5] & b[1]))
    r[7] = _u32(r[7] ^ (a2[6] & b[1]))
    a2[7] = _u32(a2[7] ^ a2[6])
    a2[1] = _u32(a2[1] ^ a2[6])
    a2[2] = _u32(a2[2] ^ a2[6])

    r[0] = _u32(r[0] ^ (a2[6] & b[2]))
    r[1] = _u32(r[1] ^ (a2[7] & b[2]))
    r[2] = _u32(r[2] ^ (a2[0] & b[2]))
    r[3] = _u32(r[3] ^ (a2[1] & b[2]))
    r[4] = _u32(r[4] ^ (a2[2] & b[2]))
    r[5] = _u32(r[5] ^ (a2[3] & b[2]))
    r[6] = _u32(r[6] ^ (a2[4] & b[2]))
    r[7] = _u32(r[7] ^ (a2[5] & b[2]))
    a2[6] = _u32(a2[6] ^ a2[5])
    a2[0] = _u32(a2[0] ^ a2[5])
    a2[1] = _u32(a2[1] ^ a2[5])

    r[0] = _u32(r[0] ^ (a2[5] & b[3]))
    r[1] = _u32(r[1] ^ (a2[6] & b[3]))
    r[2] = _u32(r[2] ^ (a2[7] & b[3]))
    r[3] = _u32(r[3] ^ (a2[0] & b[3]))
    r[4] = _u32(r[4] ^ (a2[1] & b[3]))
    r[5] = _u32(r[5] ^ (a2[2] & b[3]))
    r[6] = _u32(r[6] ^ (a2[3] & b[3]))
    r[7] = _u32(r[7] ^ (a2[4] & b[3]))
    a2[5] = _u32(a2[5] ^ a2[4])
    a2[7] = _u32(a2[7] ^ a2[4])
    a2[0] = _u32(a2[0] ^ a2[4])

    r[0] = _u32(r[0] ^ (a2[4] & b[4]))
    r[1] = _u32(r[1] ^ (a2[5] & b[4]))
    r[2] = _u32(r[2] ^ (a2[6] & b[4]))
    r[3] = _u32(r[3] ^ (a2[7] & b[4]))
    r[4] = _u32(r[4] ^ (a2[0] & b[4]))
    r[5] = _u32(r[5] ^ (a2[1] & b[4]))
    r[6] = _u32(r[6] ^ (a2[2] & b[4]))
    r[7] = _u32(r[7] ^ (a2[3] & b[4]))
    a2[4] = _u32(a2[4] ^ a2[3])
    a2[6] = _u32(a2[6] ^ a2[3])
    a2[7] = _u32(a2[7] ^ a2[3])

    r[0] = _u32(r[0] ^ (a2[3] & b[5]))
    r[1] = _u32(r[1] ^ (a2[4] & b[5]))
    r[2] = _u32(r[2] ^ (a2[5] & b[5]))
    r[3] = _u32(r[3] ^ (a2[6] & b[5]))
    r[4] = _u32(r[4] ^ (a2[7] & b[5]))
    r[5] = _u32(r[5] ^ (a2[0] & b[5]))
    r[6] = _u32(r[6] ^ (a2[1] & b[5]))
    r[7] = _u32(r[7] ^ (a2[2] & b[5]))
    a2[3] = _u32(a2[3] ^ a2[2])
    a2[5] = _u32(a2[5] ^ a2[2])
    a2[6] = _u32(a2[6] ^ a2[2])

    r[0] = _u32(r[0] ^ (a2[2] & b[6]))
    r[1] = _u32(r[1] ^ (a2[3] & b[6]))
    r[2] = _u32(r[2] ^ (a2[4] & b[6]))
    r[3] = _u32(r[3] ^ (a2[5] & b[6]))
    r[4] = _u32(r[4] ^ (a2[6] & b[6]))
    r[5] = _u32(r[5] ^ (a2[7] & b[6]))
    r[6] = _u32(r[6] ^ (a2[0] & b[6]))
    r[7] = _u32(r[7] ^ (a2[1] & b[6]))
    a2[2] = _u32(a2[2] ^ a2[1])
    a2[4] = _u32(a2[4] ^ a2[1])
    a2[5] = _u32(a2[5] ^ a2[1])

    r[0] = _u32(r[0] ^ (a2[1] & b[7]))
    r[1] = _u32(r[1] ^ (a2[2] & b[7]))
    r[2] = _u32(r[2] ^ (a2[3] & b[7]))
    r[3] = _u32(r[3] ^ (a2[4] & b[7]))
    r[4] = _u32(r[4] ^ (a2[5] & b[7]))
    r[5] = _u32(r[5] ^ (a2[6] & b[7]))
    r[6] = _u32(r[6] ^ (a2[7] & b[7]))
    r[7] = _u32(r[7] ^ (a2[0] & b[7]))


def gf256_square(r: MutableSequence[int], x: Sequence[int]) -> None:
    """Square x in GF(2^8) and write the result to r."""
    r14 = int(x[7]) & _MASK32
    r12 = int(x[6]) & _MASK32
    r10 = int(x[5]) & _MASK32
    r8 = int(x[4]) & _MASK32
    x3 = int(x[3]) & _MASK32
    x2 = int(x[2]) & _MASK32
    x1 = int(x[1]) & _MASK32
    x0 = int(x[0]) & _MASK32

    r[6] = x3
    r[4] = x2
    r[2] = x1
    r[0] = x0

    r[7] = r14
    r[6] = _u32(r[6] ^ r14)
    r10 = _u32(r10 ^ r14)

    r[4] = _u32(r[4] ^ r12)
    r[5] = r12
    r[7] = _u32(r[7] ^ r12)
    r8 = _u32(r8 ^ r12)

    r[2] = _u32(r[2] ^ r10)
    r[3] = r10
    r[5] = _u32(r[5] ^ r10)
    r[6] = _u32(r[6] ^ r10)

    r[1] = r14
    r[2] = _u32(r[2] ^ r14)
    r[4] = _u32(r[4] ^ r14)
    r[5] = _u32(r[5] ^ r14)

    r[0] = _u32(r[0] ^ r8)
    r[1] = _u32(r[1] ^ r8)
    r[3] = _u32(r[3] ^ r8)
    r[4] = _u32(r[4] ^ r8)


def gf256_inv(r: MutableSequence[int], x: MutableSequence[int]) -> None:
    """Invert x in GF(2^8) and write the result to r."""
    y = [0] * 8
    z = [0] * 8

    gf256_square(y, x)
    y2 = y.copy()
    gf256_square(y, y2)
    gf256_square(r, y)
    gf256_mul(z, r, x)
    r2 = [int(v) & _MASK32 for v in r]
    gf256_square(r, r2)
    r2 = [int(v) & _MASK32 for v in r]
    gf256_mul(r, r2, z)
    r2 = [int(v) & _MASK32 for v in r]
    gf256_square(r, r2)
    gf256_square(z, r)
    z2 = z.copy()
    gf256_square(z, z2)
    r2 = [int(v) & _MASK32 for v in r]
    gf256_mul(r, r2, z)
    r2 = [int(v) & _MASK32 for v in r]
    gf256_mul(r, r2, y)
