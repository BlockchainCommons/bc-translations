"""Error types for Shamir secret sharing."""


class ShamirError(Exception):
    """Base error for Shamir secret sharing operations."""


class SecretTooLongError(ShamirError):
    """Raised when a secret exceeds the maximum supported length."""

    def __init__(self) -> None:
        super().__init__("secret is too long")


class TooManySharesError(ShamirError):
    """Raised when the requested share count exceeds the maximum."""

    def __init__(self) -> None:
        super().__init__("too many shares")


class InterpolationError(ShamirError):
    """Raised when interpolation fails unexpectedly."""

    def __init__(self) -> None:
        super().__init__("interpolation failed")


class ChecksumError(ShamirError):
    """Raised when share digest verification fails during recovery."""

    def __init__(self) -> None:
        super().__init__("checksum failure")


class SecretTooShortError(ShamirError):
    """Raised when a secret is shorter than the minimum supported length."""

    def __init__(self) -> None:
        super().__init__("secret is too short")


class SecretNotEvenLengthError(ShamirError):
    """Raised when a secret length is odd."""

    def __init__(self) -> None:
        super().__init__("secret is not of even length")


class InvalidThresholdError(ShamirError):
    """Raised when threshold/share parameters are invalid."""

    def __init__(self) -> None:
        super().__init__("invalid threshold")


class SharesUnequalLengthError(ShamirError):
    """Raised when provided shares do not have equal length."""

    def __init__(self) -> None:
        super().__init__("shares have unequal length")


__all__ = [
    "ChecksumError",
    "InterpolationError",
    "InvalidThresholdError",
    "SecretNotEvenLengthError",
    "SecretTooLongError",
    "SecretTooShortError",
    "ShamirError",
    "SharesUnequalLengthError",
    "TooManySharesError",
]
