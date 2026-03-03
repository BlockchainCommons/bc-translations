"""Key derivation method enum."""

from __future__ import annotations

from enum import IntEnum

from bc_tags import CBOR


class KeyDerivationMethod(IntEnum):
    """Supported key derivation methods.

    CDDL::

        KeyDerivationMethod = HKDF / PBKDF2 / Scrypt / Argon2id
        HKDF = 0
        PBKDF2 = 1
        Scrypt = 2
        Argon2id = 3
    """

    HKDF = 0
    PBKDF2 = 1
    SCRYPT = 2
    ARGON2ID = 3

    def index(self) -> int:
        """Return the zero-based index of the key derivation method."""
        return self.value

    @staticmethod
    def from_index(index: int) -> KeyDerivationMethod | None:
        """Attempt to create a KeyDerivationMethod from a zero-based index."""
        try:
            return KeyDerivationMethod(index)
        except ValueError:
            return None

    def __str__(self) -> str:
        _NAMES = {
            KeyDerivationMethod.HKDF: "HKDF",
            KeyDerivationMethod.PBKDF2: "PBKDF2",
            KeyDerivationMethod.SCRYPT: "Scrypt",
            KeyDerivationMethod.ARGON2ID: "Argon2id",
        }
        return _NAMES[self]

    @staticmethod
    def from_cbor(cbor: CBOR) -> KeyDerivationMethod:
        """Decode a key derivation method from a CBOR unsigned integer."""
        i = cbor.try_int()
        result = KeyDerivationMethod.from_index(i)
        if result is None:
            raise ValueError(f"Invalid KeyDerivationMethod: {i}")
        return result
