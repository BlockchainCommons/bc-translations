"""Exception hierarchy for bc-components.

Provides a base ``BCComponentsError`` with subclasses for specific error
categories and factory class methods for convenient construction.
"""

from __future__ import annotations


class BCComponentsError(Exception):
    """Base exception for all bc-components errors."""

    @staticmethod
    def invalid_size(data_type: str, expected: int, actual: int) -> InvalidSizeError:
        return InvalidSizeError(data_type, expected, actual)

    @staticmethod
    def invalid_data(data_type: str, reason: str) -> InvalidDataError:
        return InvalidDataError(data_type, reason)

    @staticmethod
    def data_too_short(
        data_type: str, minimum: int, actual: int,
    ) -> DataTooShortError:
        return DataTooShortError(data_type, minimum, actual)

    @staticmethod
    def crypto(message: str) -> CryptoError:
        return CryptoError(message)

    @staticmethod
    def cbor_error(message: str) -> CborError:
        return CborError(message)

    @staticmethod
    def sskr(message: str) -> SskrError:
        return SskrError(message)

    @staticmethod
    def ssh(message: str) -> SshError:
        return SshError(message)

    @staticmethod
    def uri(message: str) -> UriError:
        return UriError(message)

    @staticmethod
    def compression(message: str) -> CompressionError:
        return CompressionError(message)

    @staticmethod
    def post_quantum(message: str) -> PostQuantumError:
        return PostQuantumError(message)

    @staticmethod
    def level_mismatch() -> LevelMismatchError:
        return LevelMismatchError()

    @staticmethod
    def general(message: str) -> GeneralError:
        return GeneralError(message)


class InvalidSizeError(BCComponentsError):
    """Raised when data does not match the expected size."""

    def __init__(self, data_type: str, expected: int, actual: int) -> None:
        self.data_type = data_type
        self.expected = expected
        self.actual = actual
        super().__init__(
            f"invalid {data_type} size: expected {expected}, got {actual}"
        )


class InvalidDataError(BCComponentsError):
    """Raised when data is invalid or malformed."""

    def __init__(self, data_type: str, reason: str) -> None:
        self.data_type = data_type
        self.reason = reason
        super().__init__(f"invalid {data_type}: {reason}")


class DataTooShortError(BCComponentsError):
    """Raised when data is shorter than required."""

    def __init__(self, data_type: str, minimum: int, actual: int) -> None:
        self.data_type = data_type
        self.minimum = minimum
        self.actual = actual
        super().__init__(
            f"data too short: {data_type} expected at least {minimum}, got {actual}"
        )


class CryptoError(BCComponentsError):
    """Raised when a cryptographic operation fails."""

    def __init__(self, message: str) -> None:
        super().__init__(f"cryptographic operation failed: {message}")


class CborError(BCComponentsError):
    """Raised for CBOR encoding or decoding errors."""

    def __init__(self, message: str) -> None:
        super().__init__(f"CBOR error: {message}")


class SskrError(BCComponentsError):
    """Raised for SSKR errors."""

    def __init__(self, message: str) -> None:
        super().__init__(f"SSKR error: {message}")


class SshError(BCComponentsError):
    """Raised for SSH key operation failures."""

    def __init__(self, message: str) -> None:
        super().__init__(f"SSH operation failed: {message}")


class UriError(BCComponentsError):
    """Raised for URI parsing failures."""

    def __init__(self, message: str) -> None:
        super().__init__(f"invalid URI: {message}")


class CompressionError(BCComponentsError):
    """Raised for data compression/decompression errors."""

    def __init__(self, message: str) -> None:
        super().__init__(f"compression error: {message}")


class PostQuantumError(BCComponentsError):
    """Raised for post-quantum cryptography errors."""

    def __init__(self, message: str) -> None:
        super().__init__(f"post-quantum cryptography error: {message}")


class LevelMismatchError(BCComponentsError):
    """Raised when a signature level does not match the key level."""

    def __init__(self) -> None:
        super().__init__("signature level does not match key level")


class GeneralError(BCComponentsError):
    """General error with custom message."""

    def __init__(self, message: str) -> None:
        super().__init__(message)
