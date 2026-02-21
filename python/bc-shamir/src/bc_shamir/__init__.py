"""Shamir's Secret Sharing (SSS).

Shamir's Secret Sharing splits a secret into shares so that a threshold number
of shares is required to reconstruct the secret.
"""

from .constants import MAX_SECRET_LEN, MAX_SHARE_COUNT, MIN_SECRET_LEN
from .error import (
    ChecksumFailure,
    Error,
    InterpolationFailure,
    InvalidThreshold,
    SecretNotEvenLen,
    SecretTooLong,
    SecretTooShort,
    SharesUnequalLength,
    TooManyShares,
)
from .shamir import recover_secret, split_secret

__all__ = [
    "ChecksumFailure",
    "Error",
    "InterpolationFailure",
    "InvalidThreshold",
    "MAX_SECRET_LEN",
    "MAX_SHARE_COUNT",
    "MIN_SECRET_LEN",
    "SecretNotEvenLen",
    "SecretTooLong",
    "SecretTooShort",
    "SharesUnequalLength",
    "TooManyShares",
    "recover_secret",
    "split_secret",
]
