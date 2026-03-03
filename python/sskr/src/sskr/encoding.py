"""SSKR generation, serialization, and combination logic."""

from __future__ import annotations

from collections.abc import Sequence

from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
from bc_shamir import ShamirError as BCShamirError
from bc_shamir import recover_secret, split_secret

from .constants import METADATA_SIZE_BYTES
from .error import (
    DuplicateMemberIndexError,
    GroupThresholdInvalidError,
    MemberThresholdInvalidError,
    NotEnoughGroupsError,
    ShareLengthInvalidError,
    ShareReservedBitsInvalidError,
    ShareSetInvalidError,
    SharesEmptyError,
    ShamirError,
)
from .secret import Secret
from .share import SSKRShare
from .spec import Spec


class _Group:
    __slots__ = ("group_index", "member_threshold", "member_indexes", "member_shares")

    def __init__(self, group_index: int, member_threshold: int) -> None:
        self.group_index = group_index
        self.member_threshold = member_threshold
        self.member_indexes: list[int] = []
        self.member_shares: list[Secret] = []


def sskr_generate(spec: Spec, master_secret: Secret) -> list[list[bytes]]:
    """Generate SSKR shares for a split specification and master secret."""
    rng = SecureRandomNumberGenerator()
    return sskr_generate_using(spec, master_secret, rng)


def sskr_generate_using(
    spec: Spec,
    master_secret: Secret,
    random_generator: RandomNumberGenerator,
) -> list[list[bytes]]:
    """Generate SSKR shares using a provided random number generator."""
    groups_shares = _generate_shares(spec, master_secret, random_generator)
    return [[_serialize_share(share) for share in group] for group in groups_shares]


def sskr_combine(shares: Sequence[bytes | bytearray | memoryview]) -> Secret:
    """Combine SSKR shares into a recovered secret."""
    sskr_shares = [_deserialize_share(share) for share in shares]
    return _combine_shares(sskr_shares)


def _serialize_share(share: SSKRShare) -> bytes:
    result = bytearray(share.value().len() + METADATA_SIZE_BYTES)
    identifier = share.identifier()
    group_threshold = (share.group_threshold() - 1) & 0xF
    group_count = (share.group_count() - 1) & 0xF
    group_index = share.group_index() & 0xF
    member_threshold = (share.member_threshold() - 1) & 0xF
    member_index = share.member_index() & 0xF

    result[0] = (identifier >> 8) & 0xFF
    result[1] = identifier & 0xFF
    result[2] = ((group_threshold << 4) | group_count) & 0xFF
    result[3] = ((group_index << 4) | member_threshold) & 0xFF
    result[4] = member_index & 0xFF
    result[METADATA_SIZE_BYTES:] = share.value().data()
    return bytes(result)


def _deserialize_share(source: bytes | bytearray | memoryview) -> SSKRShare:
    source_bytes = bytes(source)
    if len(source_bytes) < METADATA_SIZE_BYTES:
        raise ShareLengthInvalidError()

    group_threshold = (source_bytes[2] >> 4) + 1
    group_count = (source_bytes[2] & 0xF) + 1
    if group_threshold > group_count:
        raise GroupThresholdInvalidError()

    identifier = (source_bytes[0] << 8) | source_bytes[1]
    group_index = source_bytes[3] >> 4
    member_threshold = (source_bytes[3] & 0xF) + 1

    reserved = source_bytes[4] >> 4
    if reserved != 0:
        raise ShareReservedBitsInvalidError()

    member_index = source_bytes[4] & 0xF
    value = Secret.new(source_bytes[METADATA_SIZE_BYTES:])

    return SSKRShare(
        identifier=identifier,
        group_index=group_index,
        group_threshold=group_threshold,
        group_count=group_count,
        member_index=member_index,
        member_threshold=member_threshold,
        value=value,
    )


def _generate_shares(
    spec: Spec,
    master_secret: Secret,
    random_generator: RandomNumberGenerator,
) -> list[list[SSKRShare]]:
    identifier_bytes = bytearray(2)
    random_generator.fill_random_data(identifier_bytes)
    identifier = (identifier_bytes[0] << 8) | identifier_bytes[1]

    try:
        group_secrets = split_secret(
            spec.group_threshold(),
            spec.group_count(),
            master_secret.data(),
            random_generator,
        )
    except BCShamirError as exc:
        raise ShamirError(exc) from exc

    groups_shares: list[list[SSKRShare]] = []
    for group_index, group in enumerate(spec.groups()):
        group_secret = group_secrets[group_index]
        try:
            member_secrets = split_secret(
                group.member_threshold(),
                group.member_count(),
                group_secret,
                random_generator,
            )
        except BCShamirError as exc:
            raise ShamirError(exc) from exc

        shares_for_group: list[SSKRShare] = []
        for member_index, member_secret in enumerate(member_secrets):
            secret = Secret.new(member_secret)
            shares_for_group.append(
                SSKRShare(
                    identifier=identifier,
                    group_index=group_index,
                    group_threshold=spec.group_threshold(),
                    group_count=spec.group_count(),
                    member_index=member_index,
                    member_threshold=group.member_threshold(),
                    value=secret,
                )
            )
        groups_shares.append(shares_for_group)

    return groups_shares


def _combine_shares(shares: Sequence[SSKRShare]) -> Secret:
    if not shares:
        raise SharesEmptyError()

    identifier = 0
    group_threshold = 0
    group_count = 0
    secret_len = 0

    next_group = 0
    groups: list[_Group] = []

    for index, share in enumerate(shares):
        if index == 0:
            identifier = share.identifier()
            group_threshold = share.group_threshold()
            group_count = share.group_count()
            secret_len = share.value().len()
        else:
            if (
                share.identifier() != identifier
                or share.group_threshold() != group_threshold
                or share.group_count() != group_count
                or share.value().len() != secret_len
            ):
                raise ShareSetInvalidError()

        group_found = False
        for group in groups:
            if share.group_index() == group.group_index:
                group_found = True
                if share.member_threshold() != group.member_threshold:
                    raise MemberThresholdInvalidError()
                if share.member_index() in group.member_indexes:
                    raise DuplicateMemberIndexError()
                if len(group.member_indexes) < group.member_threshold:
                    group.member_indexes.append(share.member_index())
                    group.member_shares.append(share.value())

        if not group_found:
            group = _Group(share.group_index(), share.member_threshold())
            group.member_indexes.append(share.member_index())
            group.member_shares.append(share.value())
            groups.append(group)
            next_group += 1

    if next_group < group_threshold:
        raise NotEnoughGroupsError()

    master_indexes: list[int] = []
    master_shares: list[bytes] = []

    for group in groups:
        if len(group.member_indexes) < group.member_threshold:
            continue

        try:
            group_secret = recover_secret(
                group.member_indexes,
                [member_share.data() for member_share in group.member_shares],
            )
        except BCShamirError:
            continue

        master_indexes.append(group.group_index)
        master_shares.append(group_secret)

        if len(master_indexes) == group_threshold:
            break

    if len(master_indexes) < group_threshold:
        raise NotEnoughGroupsError()

    try:
        master_secret = recover_secret(master_indexes, master_shares)
    except BCShamirError as exc:
        raise ShamirError(exc) from exc

    return Secret.new(master_secret)


__all__ = ["sskr_combine", "sskr_generate", "sskr_generate_using"]
