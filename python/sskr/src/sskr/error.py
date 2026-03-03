"""Error types for SSKR operations."""


class Error(Exception):
    """Base error for SSKR operations."""


class DuplicateMemberIndexError(Error):
    def __init__(self) -> None:
        super().__init__(
            "When combining shares, the provided shares contained a duplicate member index"
        )


class GroupSpecInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("Invalid group specification.")


class GroupCountInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("When creating a split spec, the group count is invalid")


class GroupThresholdInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR group threshold is invalid")


class MemberCountInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR member count is invalid")


class MemberThresholdInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR member threshold is invalid")


class NotEnoughGroupsError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR shares did not contain enough groups")


class SecretLengthNotEvenError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR secret is not of even length")


class SecretTooLongError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR secret is too long")


class SecretTooShortError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR secret is too short")


class ShareLengthInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR shares did not contain enough serialized bytes")


class ShareReservedBitsInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR shares contained invalid reserved bits")


class SharesEmptyError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR shares were empty")


class ShareSetInvalidError(Error):
    def __init__(self) -> None:
        super().__init__("SSKR shares were invalid")


class ShamirError(Error):
    """Wrap an upstream bc_shamir error."""

    def __init__(self, cause: Exception) -> None:
        self.cause = cause
        super().__init__(f"SSKR Shamir error: {cause}")


__all__ = [
    "DuplicateMemberIndexError",
    "Error",
    "GroupCountInvalidError",
    "GroupSpecInvalidError",
    "GroupThresholdInvalidError",
    "MemberCountInvalidError",
    "MemberThresholdInvalidError",
    "NotEnoughGroupsError",
    "SecretLengthNotEvenError",
    "SecretTooLongError",
    "SecretTooShortError",
    "ShareLengthInvalidError",
    "ShareReservedBitsInvalidError",
    "ShareSetInvalidError",
    "SharesEmptyError",
    "ShamirError",
]
