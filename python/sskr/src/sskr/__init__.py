"""Sharded Secret Key Reconstruction (SSKR) for Python.

SSKR splits a secret into multiple groups and shares, where a threshold of
member shares from a threshold of groups is needed to reconstruct the secret.
"""

from .constants import (
    MAX_GROUPS_COUNT,
    MAX_SECRET_LEN,
    MAX_SHARE_COUNT,
    METADATA_SIZE_BYTES,
    MIN_SECRET_LEN,
    MIN_SERIALIZE_SIZE_BYTES,
)
from .encoding import sskr_combine, sskr_generate, sskr_generate_using
from .error import (
    DuplicateMemberIndexError,
    Error,
    GroupCountInvalidError,
    GroupSpecInvalidError,
    GroupThresholdInvalidError,
    MemberCountInvalidError,
    MemberThresholdInvalidError,
    NotEnoughGroupsError,
    SecretLengthNotEvenError,
    SecretTooLongError,
    SecretTooShortError,
    ShareLengthInvalidError,
    ShareReservedBitsInvalidError,
    ShareSetInvalidError,
    SharesEmptyError,
    ShamirError,
)
from .secret import Secret
from .spec import GroupSpec, Spec

__all__ = [
    "DuplicateMemberIndexError",
    "Error",
    "GroupCountInvalidError",
    "GroupSpec",
    "GroupSpecInvalidError",
    "GroupThresholdInvalidError",
    "MAX_GROUPS_COUNT",
    "MAX_SECRET_LEN",
    "MAX_SHARE_COUNT",
    "METADATA_SIZE_BYTES",
    "MIN_SECRET_LEN",
    "MIN_SERIALIZE_SIZE_BYTES",
    "MemberCountInvalidError",
    "MemberThresholdInvalidError",
    "NotEnoughGroupsError",
    "Secret",
    "SecretLengthNotEvenError",
    "SecretTooLongError",
    "SecretTooShortError",
    "ShareLengthInvalidError",
    "ShareReservedBitsInvalidError",
    "ShareSetInvalidError",
    "SharesEmptyError",
    "ShamirError",
    "Spec",
    "sskr_combine",
    "sskr_generate",
    "sskr_generate_using",
]
