"""Exception hierarchy for bc-ur."""


class URError(Exception):
    """Base exception for all UR errors."""


class URDecoderError(URError):
    """Error from the UR fountain decoder."""

    def __init__(self, message: str) -> None:
        super().__init__(f"UR decoder error ({message})")


class BytewordsError(URError):
    """Error from bytewords encoding/decoding."""

    def __init__(self, message: str) -> None:
        super().__init__(f"Bytewords error ({message})")


class URCborError(URError):
    """Error wrapping a dcbor error."""

    def __init__(self, cause: Exception) -> None:
        super().__init__(f"CBOR error ({cause})")
        self.cause = cause


class InvalidSchemeError(URError):
    """Invalid UR scheme (not 'ur:')."""

    def __init__(self) -> None:
        super().__init__("invalid UR scheme")


class TypeUnspecifiedError(URError):
    """No UR type specified."""

    def __init__(self) -> None:
        super().__init__("no UR type specified")


class InvalidTypeError(URError):
    """Invalid UR type string."""

    def __init__(self) -> None:
        super().__init__("invalid UR type")


class NotSinglePartError(URError):
    """UR is not a single-part UR."""

    def __init__(self) -> None:
        super().__init__("UR is not a single-part")


class UnexpectedTypeError(URError):
    """UR type does not match expected type."""

    def __init__(self, expected: str, found: str) -> None:
        super().__init__(f"expected UR type {expected}, but found {found}")
        self.expected = expected
        self.found = found


class FountainError(URError):
    """Error from the fountain encoder/decoder."""

    def __init__(self, message: str) -> None:
        super().__init__(message)
