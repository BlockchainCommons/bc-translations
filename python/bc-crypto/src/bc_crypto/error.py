"""Error types for bc-crypto."""


class Error(Exception):
    """Base error for bc-crypto operations."""


class AeadError(Error):
    """Raised when AEAD decryption/authentication fails."""

__all__ = ["AeadError", "Error"]
