"""Error types for bc-crypto."""

from typing import Any


class Error(Exception):
    """Base error for bc-crypto operations."""


class AeadError(Error):
    """Raised when AEAD decryption/authentication fails."""


Result = Any


__all__ = ["AeadError", "Error", "Result"]
