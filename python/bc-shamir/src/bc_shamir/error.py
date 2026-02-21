"""Error types for bc-shamir."""


class Error(Exception):
    """Base error for bc-shamir operations."""


class SecretTooLong(Error):
    """Raised when a secret exceeds the maximum supported length."""

    def __init__(self) -> None:
        super().__init__("secret is too long")


class TooManyShares(Error):
    """Raised when the requested share count exceeds the maximum."""

    def __init__(self) -> None:
        super().__init__("too many shares")


class InterpolationFailure(Error):
    """Raised when interpolation fails unexpectedly."""

    def __init__(self) -> None:
        super().__init__("interpolation failed")


class ChecksumFailure(Error):
    """Raised when share digest verification fails during recovery."""

    def __init__(self) -> None:
        super().__init__("checksum failure")


class SecretTooShort(Error):
    """Raised when a secret is shorter than the minimum supported length."""

    def __init__(self) -> None:
        super().__init__("secret is too short")


class SecretNotEvenLen(Error):
    """Raised when a secret length is odd."""

    def __init__(self) -> None:
        super().__init__("secret is not of even length")


class InvalidThreshold(Error):
    """Raised when threshold/share parameters are invalid."""

    def __init__(self) -> None:
        super().__init__("invalid threshold")


class SharesUnequalLength(Error):
    """Raised when provided shares do not have equal length."""

    def __init__(self) -> None:
        super().__init__("shares have unequal length")


__all__ = [
    "ChecksumFailure",
    "Error",
    "InterpolationFailure",
    "InvalidThreshold",
    "SecretNotEvenLen",
    "SecretTooLong",
    "SecretTooShort",
    "SharesUnequalLength",
    "TooManyShares",
]
