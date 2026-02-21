"""Shamir secret sharing split/recover API."""

from __future__ import annotations

from collections.abc import Sequence

from bc_crypto import hmac_sha256, memzero, memzero_vec_vec_u8
from bc_rand import RandomNumberGenerator

from .constants import (
    DIGEST_INDEX,
    MAX_SECRET_LEN,
    MAX_SHARE_COUNT,
    MIN_SECRET_LEN,
    SECRET_INDEX,
)
from .error import (
    ChecksumFailure,
    InterpolationFailure,
    InvalidThreshold,
    SecretNotEvenLen,
    SecretTooLong,
    SecretTooShort,
    SharesUnequalLength,
    TooManyShares,
)
from .interpolate import interpolate


def _create_digest(random_data: bytes | bytearray, shared_secret: bytes) -> bytes:
    return hmac_sha256(random_data, shared_secret)


def _validate_parameters(
    threshold: int,
    share_count: int,
    secret_length: int,
) -> None:
    if share_count > MAX_SHARE_COUNT:
        raise TooManyShares()
    if threshold < 1 or threshold > share_count:
        raise InvalidThreshold()
    if secret_length > MAX_SECRET_LEN:
        raise SecretTooLong()
    if secret_length < MIN_SECRET_LEN:
        raise SecretTooShort()
    if secret_length & 1 != 0:
        raise SecretNotEvenLen()


def split_secret(
    threshold: int,
    share_count: int,
    secret: bytes | bytearray | memoryview,
    random_generator: RandomNumberGenerator,
) -> list[bytes]:
    """Split a secret into shares using Shamir's Secret Sharing."""
    secret_bytes = bytes(secret)
    _validate_parameters(threshold, share_count, len(secret_bytes))

    if threshold == 1:
        return [bytes(secret_bytes) for _ in range(share_count)]

    secret_len = len(secret_bytes)
    x = bytearray(share_count)
    y = [bytearray(secret_len) for _ in range(share_count)]
    n = 0
    result = [bytearray(secret_len) for _ in range(share_count)]

    for index in range(threshold - 2):
        random_generator.fill_random_data(result[index])
        x[n] = index
        y[n][:] = result[index]
        n += 1

    digest = bytearray(secret_len)
    random_tail = bytearray(secret_len - 4)
    random_generator.fill_random_data(random_tail)
    digest[4:] = random_tail

    d = _create_digest(digest[4:], secret_bytes)
    digest[:4] = d[:4]

    x[n] = DIGEST_INDEX
    y[n][:] = digest
    n += 1

    x[n] = SECRET_INDEX
    y[n][:] = secret_bytes
    n += 1

    try:
        for index in range(threshold - 2, share_count):
            value = interpolate(n, x, secret_len, y, index)
            result[index][:] = value
    except Exception as exc:  # pragma: no cover - defensive parity with Rust Result
        raise InterpolationFailure() from exc
    finally:
        memzero(digest)
        memzero(random_tail)
        memzero(x)
        memzero_vec_vec_u8(y)

    return [bytes(share) for share in result]


def recover_secret(
    indexes: Sequence[int],
    shares: Sequence[bytes | bytearray | memoryview],
) -> bytes:
    """Recover a secret from share indexes and corresponding share data."""
    threshold = len(shares)
    if threshold == 0 or len(indexes) != threshold:
        raise InvalidThreshold()

    share_length = len(shares[0])
    _validate_parameters(threshold, threshold, share_length)

    if not all(len(share) == share_length for share in shares):
        raise SharesUnequalLength()

    if threshold == 1:
        return bytes(shares[0])

    indexes_u8 = bytearray(int(index) & 0xFF for index in indexes)

    try:
        digest = bytearray(
            interpolate(threshold, indexes_u8, share_length, shares, DIGEST_INDEX)
        )
        secret = interpolate(threshold, indexes_u8, share_length, shares, SECRET_INDEX)
    except Exception as exc:  # pragma: no cover - defensive parity with Rust Result
        raise InterpolationFailure() from exc

    verify = bytearray(_create_digest(digest[4:], secret))

    valid = True
    for i in range(4):
        valid &= digest[i] == verify[i]

    memzero(digest)
    memzero(verify)
    memzero(indexes_u8)

    if not valid:
        raise ChecksumFailure()

    return secret


__all__ = ["recover_secret", "split_secret"]
