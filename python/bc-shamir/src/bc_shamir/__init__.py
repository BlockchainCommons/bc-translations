"""Shamir's Secret Sharing (SSS).

Shamir's Secret Sharing splits a secret into shares so that a threshold number
of shares is required to reconstruct the secret.
"""

from .constants import MAX_SECRET_LEN, MAX_SHARE_COUNT, MIN_SECRET_LEN
from .error import (
    ChecksumError,
    InterpolationError,
    InvalidThresholdError,
    SecretNotEvenLengthError,
    SecretTooLongError,
    SecretTooShortError,
    ShamirError,
    SharesUnequalLengthError,
    TooManySharesError,
)
from .shamir import recover_secret, split_secret

__all__ = [
    "ChecksumError",
    "InterpolationError",
    "InvalidThresholdError",
    "MAX_SECRET_LEN",
    "MAX_SHARE_COUNT",
    "MIN_SECRET_LEN",
    "SecretNotEvenLengthError",
    "SecretTooLongError",
    "SecretTooShortError",
    "ShamirError",
    "SharesUnequalLengthError",
    "TooManySharesError",
    "recover_secret",
    "split_secret",
]
