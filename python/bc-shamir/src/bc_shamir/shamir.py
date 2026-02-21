"""Shamir secret sharing split and recover operations."""

from __future__ import annotations

from collections.abc import Sequence

from bc_crypto import hmac_sha256, memzero, memzero_vec_vec_u8
from bc_rand import RandomNumberGenerator

from .constants import (
    _DIGEST_INDEX,
    _SECRET_INDEX,
    MAX_SECRET_LEN,
    MAX_SHARE_COUNT,
    MIN_SECRET_LEN,
)
from .error import (
    ChecksumError,
    InterpolationError,
    InvalidThresholdError,
    SecretNotEvenLengthError,
    SecretTooLongError,
    SecretTooShortError,
    SharesUnequalLengthError,
    TooManySharesError,
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
        raise TooManySharesError()
    if threshold < 1 or threshold > share_count:
        raise InvalidThresholdError()
    if secret_length > MAX_SECRET_LEN:
        raise SecretTooLongError()
    if secret_length < MIN_SECRET_LEN:
        raise SecretTooShortError()
    if secret_length & 1 != 0:
        raise SecretNotEvenLengthError()


def split_secret(
    threshold: int,
    share_count: int,
    secret: bytes | bytearray | memoryview,
    random_generator: RandomNumberGenerator,
) -> list[bytes]:
    """Split a secret into shares using Shamir's Secret Sharing.

    At least ``threshold`` shares are needed to reconstruct the original
    secret.  The ``random_generator`` provides the randomness used to
    generate share polynomials.

    Args:
        threshold: Minimum number of shares required to reconstruct the
            secret.  Must be at least 1 and at most ``share_count``.
        share_count: Total number of shares to generate.  Must not exceed
            ``MAX_SHARE_COUNT``.
        secret: The secret to split.  Must be an even number of bytes,
            between ``MIN_SECRET_LEN`` and ``MAX_SECRET_LEN`` inclusive.
        random_generator: Source of randomness for polynomial generation.

    Returns:
        A list of ``share_count`` shares, each the same length as
        ``secret``.

    Raises:
        TooManySharesError: If ``share_count`` exceeds ``MAX_SHARE_COUNT``.
        InvalidThresholdError: If ``threshold`` is out of range.
        SecretTooLongError: If the secret exceeds ``MAX_SECRET_LEN``.
        SecretTooShortError: If the secret is shorter than ``MIN_SECRET_LEN``.
        SecretNotEvenLengthError: If the secret has an odd byte length.
        InterpolationError: If polynomial interpolation fails internally.
    """
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

    x[n] = _DIGEST_INDEX
    y[n][:] = digest
    n += 1

    x[n] = _SECRET_INDEX
    y[n][:] = secret_bytes
    n += 1

    try:
        for index in range(threshold - 2, share_count):
            value = interpolate(n, x, secret_len, y, index)
            result[index][:] = value
    except Exception as exc:  # pragma: no cover - defensive normalization
        raise InterpolationError() from exc
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
    """Recover a secret from share indexes and corresponding share data.

    The number of shares provided must equal the threshold used during
    splitting, and each share must have the same byte length.

    Args:
        indexes: The original share indexes (0-based) corresponding to
            each share.
        shares: The share data produced by :func:`split_secret`.

    Returns:
        The reconstructed secret as ``bytes``.

    Raises:
        InvalidThresholdError: If no shares are provided or the number
            of indexes does not match the number of shares.
        SharesUnequalLengthError: If shares have differing byte lengths.
        InterpolationError: If polynomial interpolation fails internally.
        ChecksumError: If the reconstructed digest does not match,
            indicating corrupted or incorrect shares.
    """
    threshold = len(shares)
    if threshold == 0 or len(indexes) != threshold:
        raise InvalidThresholdError()

    share_length = len(shares[0])
    _validate_parameters(threshold, threshold, share_length)

    if not all(len(share) == share_length for share in shares):
        raise SharesUnequalLengthError()

    if threshold == 1:
        return bytes(shares[0])

    indexes_u8 = bytearray(int(index) & 0xFF for index in indexes)

    try:
        digest = bytearray(
            interpolate(threshold, indexes_u8, share_length, shares, _DIGEST_INDEX)
        )
        secret = interpolate(threshold, indexes_u8, share_length, shares, _SECRET_INDEX)
    except Exception as exc:  # pragma: no cover - defensive normalization
        raise InterpolationError() from exc

    verify = bytearray(_create_digest(digest[4:], secret))

    valid = True
    for i in range(4):
        valid &= digest[i] == verify[i]

    memzero(digest)
    memzero(verify)
    memzero(indexes_u8)

    if not valid:
        raise ChecksumError()

    return secret


__all__ = ["recover_secret", "split_secret"]
