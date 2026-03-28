"""Elision, encryption, compression (obscuration) of envelope elements.

Supports selective disclosure by obscuring parts of an envelope while
maintaining the integrity of the digest tree.
"""

from __future__ import annotations

from enum import Enum, auto
from typing import TYPE_CHECKING

from bc_components import Digest, SymmetricKey

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# ObscureType — identifies how an element has been obscured
# ---------------------------------------------------------------------------


class ObscureType(Enum):
    """Identifies the kind of obscuration applied to an envelope element."""

    ELIDED = auto()
    ENCRYPTED = auto()
    COMPRESSED = auto()


# ---------------------------------------------------------------------------
# ObscureAction — what to do when obscuring an element
# ---------------------------------------------------------------------------


class _ObscureKind(Enum):
    """Internal discriminant for ObscureAction."""
    ELIDE = auto()
    ENCRYPT = auto()
    COMPRESS = auto()


class ObscureAction:
    """Action to perform when obscuring an envelope element.

    Use the class-level factory helpers:

    * ``ObscureAction.elide()``
    * ``ObscureAction.encrypt(key)``
    * ``ObscureAction.compress()``
    """

    __slots__ = ("_kind", "_key")

    def __init__(self, kind: _ObscureKind, key: SymmetricKey | None = None) -> None:
        self._kind = kind
        self._key = key

    # --- Factories ---------------------------------------------------------

    @staticmethod
    def elide() -> ObscureAction:
        return ObscureAction(_ObscureKind.ELIDE)

    @staticmethod
    def encrypt(key: SymmetricKey) -> ObscureAction:
        return ObscureAction(_ObscureKind.ENCRYPT, key)

    @staticmethod
    def compress() -> ObscureAction:
        return ObscureAction(_ObscureKind.COMPRESS)

    # --- Properties --------------------------------------------------------

    @property
    def kind(self) -> _ObscureKind:
        return self._kind

    @property
    def key(self) -> SymmetricKey | None:
        return self._key


# ---------------------------------------------------------------------------
# Envelope elision methods — attached to Envelope via __init__.py
# ---------------------------------------------------------------------------


def elide(self: Envelope) -> Envelope:
    """Return the elided variant of this envelope (digest only).

    Returns the same envelope if already elided.
    """
    from ._envelope import Envelope as Env

    if self.case_type == CaseType.ELIDED:
        return self
    return Env.new_elided(self.digest())


def elide_removing_set_with_action(
    self: Envelope,
    target: set[Digest],
    action: ObscureAction,
) -> Envelope:
    """Obscure elements whose digests are in *target*."""
    return _elide_set_with_action(self, target, False, action)


def elide_removing_set(self: Envelope, target: set[Digest]) -> Envelope:
    """Elide elements whose digests are in *target*."""
    return _elide_set(self, target, False)


def elide_removing_array_with_action(
    self: Envelope,
    target: list,
    action: ObscureAction,
) -> Envelope:
    """Obscure elements matching digest providers in *target*."""
    return _elide_array_with_action(self, target, False, action)


def elide_removing_array(self: Envelope, target: list) -> Envelope:
    """Elide elements matching digest providers in *target*."""
    return _elide_array(self, target, False)


def elide_removing_target_with_action(
    self: Envelope,
    target,
    action: ObscureAction,
) -> Envelope:
    """Obscure a single element identified by a digest provider."""
    return _elide_target_with_action(self, target, False, action)


def elide_removing_target(self: Envelope, target) -> Envelope:
    """Elide a single element identified by a digest provider."""
    return _elide_target(self, target, False)


def elide_revealing_set_with_action(
    self: Envelope,
    target: set[Digest],
    action: ObscureAction,
) -> Envelope:
    """Obscure all elements *except* those whose digests are in *target*."""
    return _elide_set_with_action(self, target, True, action)


def elide_revealing_set(self: Envelope, target: set[Digest]) -> Envelope:
    """Elide all elements *except* those whose digests are in *target*."""
    return _elide_set(self, target, True)


def elide_revealing_array_with_action(
    self: Envelope,
    target: list,
    action: ObscureAction,
) -> Envelope:
    """Obscure all elements *except* those matching digest providers."""
    return _elide_array_with_action(self, target, True, action)


def elide_revealing_array(self: Envelope, target: list) -> Envelope:
    """Elide all elements *except* those matching digest providers."""
    return _elide_array(self, target, True)


def elide_revealing_target_with_action(
    self: Envelope,
    target,
    action: ObscureAction,
) -> Envelope:
    """Obscure all elements *except* a single digest provider."""
    return _elide_target_with_action(self, target, True, action)


def elide_revealing_target(self: Envelope, target) -> Envelope:
    """Elide all elements *except* a single digest provider."""
    return _elide_target(self, target, True)


# ---------------------------------------------------------------------------
# Core dispatch
# ---------------------------------------------------------------------------

# Truth table:
#   target_matches   is_revealing   -> obscure?
#   False            False          -> False
#   False            True           -> True
#   True             False          -> True
#   True             True           -> False
# i.e. obscure = (target_matches != is_revealing)


def _elide_set_with_action(
    envelope: Envelope,
    target: set[Digest],
    is_revealing: bool,
    action: ObscureAction,
) -> Envelope:
    """Core recursive elision engine."""
    from ._assertion import Assertion
    from ._envelope import Envelope as Env

    self_digest = envelope.digest()

    if (self_digest in target) != is_revealing:
        # This element should be obscured
        if action.kind == _ObscureKind.ELIDE:
            return elide(envelope)
        elif action.kind == _ObscureKind.ENCRYPT:
            assert action.key is not None
            message = action.key.encrypt_with_digest(
                envelope.tagged_cbor().to_cbor_data(),
                self_digest,
            )
            return Env.new_with_encrypted(message)
        elif action.kind == _ObscureKind.COMPRESS:
            return envelope.compress()
        else:
            raise ValueError(f"Unknown obscure action: {action.kind}")

    case = envelope.case
    ct = case.case_type

    if ct == CaseType.ASSERTION:
        predicate = _elide_set_with_action(
            case.assertion.predicate, target, is_revealing, action
        )
        obj = _elide_set_with_action(
            case.assertion.object, target, is_revealing, action
        )
        elided_assertion = Assertion(predicate, obj)
        assert elided_assertion == case.assertion
        return Env.new_with_assertion(elided_assertion)

    if ct == CaseType.NODE:
        elided_subject = _elide_set_with_action(
            case.subject, target, is_revealing, action
        )
        assert elided_subject.digest() == case.subject.digest()
        elided_assertions = []
        for a in case.assertions:
            ea = _elide_set_with_action(a, target, is_revealing, action)
            assert ea.digest() == a.digest()
            elided_assertions.append(ea)
        return Env.new_with_unchecked_assertions(
            elided_subject, elided_assertions
        )

    if ct == CaseType.WRAPPED:
        elided_envelope = _elide_set_with_action(
            case.envelope, target, is_revealing, action
        )
        assert elided_envelope.digest() == case.envelope.digest()
        return Env.new_wrapped(elided_envelope)

    return envelope


def _elide_set(
    envelope: Envelope,
    target: set[Digest],
    is_revealing: bool,
) -> Envelope:
    return _elide_set_with_action(
        envelope, target, is_revealing, ObscureAction.elide()
    )


def _elide_array_with_action(
    envelope: Envelope,
    target: list,
    is_revealing: bool,
    action: ObscureAction,
) -> Envelope:
    digest_set = {item.digest() for item in target}
    return _elide_set_with_action(envelope, digest_set, is_revealing, action)


def _elide_array(
    envelope: Envelope,
    target: list,
    is_revealing: bool,
) -> Envelope:
    return _elide_array_with_action(
        envelope, target, is_revealing, ObscureAction.elide()
    )


def _elide_target_with_action(
    envelope: Envelope,
    target,
    is_revealing: bool,
    action: ObscureAction,
) -> Envelope:
    return _elide_array_with_action(envelope, [target], is_revealing, action)


def _elide_target(
    envelope: Envelope,
    target,
    is_revealing: bool,
) -> Envelope:
    return _elide_target_with_action(
        envelope, target, is_revealing, ObscureAction.elide()
    )


# ---------------------------------------------------------------------------
# Unelide
# ---------------------------------------------------------------------------


def unelide(self: Envelope, original: Envelope) -> Envelope:
    """Restore from *original* if digests match.

    Raises
    ------
    InvalidDigest
        If the digests do not match.
    """
    from ._error import InvalidDigest

    if self.digest() == original.digest():
        return original
    raise InvalidDigest()


# ---------------------------------------------------------------------------
# nodes_matching
# ---------------------------------------------------------------------------


def nodes_matching(
    self: Envelope,
    target_digests: set[Digest] | None,
    obscure_types: list[ObscureType],
) -> set[Digest]:
    """Return digests of nodes matching the given criteria."""
    from ._walk import EdgeType

    result: set[Digest] = set()

    def visitor(
        env: Envelope, _level: int, _edge: EdgeType, state: None
    ) -> tuple[None, bool]:
        digest_matches = (
            target_digests is None or env.digest() in target_digests
        )
        if not digest_matches:
            return (None, False)

        if not obscure_types:
            result.add(env.digest())
            return (None, False)

        ct = env.case_type
        for ot in obscure_types:
            if ot == ObscureType.ELIDED and ct == CaseType.ELIDED:
                result.add(env.digest())
                break
            if ot == ObscureType.ENCRYPTED and ct == CaseType.ENCRYPTED:
                result.add(env.digest())
                break
            if ot == ObscureType.COMPRESSED and ct == CaseType.COMPRESSED:
                result.add(env.digest())
                break

        return (None, False)

    self.walk(False, None, visitor)
    return result


# ---------------------------------------------------------------------------
# walk_unelide
# ---------------------------------------------------------------------------


def walk_unelide(self: Envelope, envelopes: list[Envelope]) -> Envelope:
    """Restore elided nodes using the provided list of envelopes."""
    envelope_map: dict[Digest, Envelope] = {}
    for env in envelopes:
        envelope_map[env.digest()] = env
    return _walk_unelide_with_map(self, envelope_map)


def _walk_unelide_with_map(
    envelope: Envelope,
    envelope_map: dict[Digest, Envelope],
) -> Envelope:
    from ._assertion import Assertion
    from ._envelope import Envelope as Env

    case = envelope.case
    ct = case.case_type

    if ct == CaseType.ELIDED:
        replacement = envelope_map.get(envelope.digest())
        if replacement is not None:
            return replacement
        return envelope

    if ct == CaseType.NODE:
        new_subject = _walk_unelide_with_map(case.subject, envelope_map)
        new_assertions = [
            _walk_unelide_with_map(a, envelope_map) for a in case.assertions
        ]
        if new_subject.is_identical_to(case.subject) and all(
            na.is_identical_to(oa)
            for na, oa in zip(new_assertions, case.assertions)
        ):
            return envelope
        return Env.new_with_unchecked_assertions(new_subject, new_assertions)

    if ct == CaseType.WRAPPED:
        new_inner = _walk_unelide_with_map(case.envelope, envelope_map)
        if new_inner.is_identical_to(case.envelope):
            return envelope
        return new_inner.wrap()

    if ct == CaseType.ASSERTION:
        new_pred = _walk_unelide_with_map(
            case.assertion.predicate, envelope_map
        )
        new_obj = _walk_unelide_with_map(case.assertion.object, envelope_map)
        if new_pred.is_identical_to(
            case.assertion.predicate
        ) and new_obj.is_identical_to(case.assertion.object):
            return envelope
        return Env.new_assertion(new_pred, new_obj)

    return envelope


# ---------------------------------------------------------------------------
# walk_replace
# ---------------------------------------------------------------------------


def walk_replace(
    self: Envelope,
    target: set[Digest],
    replacement: Envelope,
) -> Envelope:
    """Replace nodes whose digests are in *target* with *replacement*.

    Raises
    ------
    InvalidFormat
        If an assertion would be replaced by a non-assertion,
        non-obscured envelope.
    """
    return _walk_replace_impl(self, target, replacement)


def _walk_replace_impl(
    envelope: Envelope,
    target: set[Digest],
    replacement: Envelope,
) -> Envelope:
    from ._envelope import Envelope as Env

    if envelope.digest() in target:
        return replacement

    case = envelope.case
    ct = case.case_type

    if ct == CaseType.NODE:
        new_subject = _walk_replace_impl(case.subject, target, replacement)
        new_assertions = [
            _walk_replace_impl(a, target, replacement) for a in case.assertions
        ]
        if new_subject.is_identical_to(case.subject) and all(
            na.is_identical_to(oa)
            for na, oa in zip(new_assertions, case.assertions)
        ):
            return envelope
        # Use validated constructor to check assertion validity
        return Env.new_with_assertions(new_subject, new_assertions)

    if ct == CaseType.WRAPPED:
        new_inner = _walk_replace_impl(case.envelope, target, replacement)
        if new_inner.is_identical_to(case.envelope):
            return envelope
        return new_inner.wrap()

    if ct == CaseType.ASSERTION:
        new_pred = _walk_replace_impl(
            case.assertion.predicate, target, replacement
        )
        new_obj = _walk_replace_impl(
            case.assertion.object, target, replacement
        )
        if new_pred.is_identical_to(
            case.assertion.predicate
        ) and new_obj.is_identical_to(case.assertion.object):
            return envelope
        return Env.new_assertion(new_pred, new_obj)

    return envelope


# ---------------------------------------------------------------------------
# walk_decrypt
# ---------------------------------------------------------------------------


def walk_decrypt(self: Envelope, keys: list[SymmetricKey]) -> Envelope:
    """Recursively decrypt encrypted nodes using the provided keys."""
    return _walk_decrypt_impl(self, keys)


def _walk_decrypt_impl(
    envelope: Envelope,
    keys: list[SymmetricKey],
) -> Envelope:
    from ._envelope import Envelope as Env

    case = envelope.case
    ct = case.case_type

    if ct == CaseType.ENCRYPTED:
        for key in keys:
            try:
                decrypted = envelope.decrypt_subject(key)
                return _walk_decrypt_impl(decrypted, keys)
            except Exception:
                continue
        return envelope

    if ct == CaseType.NODE:
        new_subject = _walk_decrypt_impl(case.subject, keys)
        new_assertions = [
            _walk_decrypt_impl(a, keys) for a in case.assertions
        ]
        if new_subject.is_identical_to(case.subject) and all(
            na.is_identical_to(oa)
            for na, oa in zip(new_assertions, case.assertions)
        ):
            return envelope
        return Env.new_with_unchecked_assertions(new_subject, new_assertions)

    if ct == CaseType.WRAPPED:
        new_inner = _walk_decrypt_impl(case.envelope, keys)
        if new_inner.is_identical_to(case.envelope):
            return envelope
        return new_inner.wrap()

    if ct == CaseType.ASSERTION:
        new_pred = _walk_decrypt_impl(case.assertion.predicate, keys)
        new_obj = _walk_decrypt_impl(case.assertion.object, keys)
        if new_pred.is_identical_to(
            case.assertion.predicate
        ) and new_obj.is_identical_to(case.assertion.object):
            return envelope
        return Env.new_assertion(new_pred, new_obj)

    return envelope


# ---------------------------------------------------------------------------
# walk_decompress
# ---------------------------------------------------------------------------


def walk_decompress(
    self: Envelope,
    target_digests: set[Digest] | None = None,
) -> Envelope:
    """Recursively decompress compressed nodes.

    If *target_digests* is provided, only decompress nodes whose digests
    are in the set.
    """
    return _walk_decompress_impl(self, target_digests)


def _walk_decompress_impl(
    envelope: Envelope,
    target_digests: set[Digest] | None,
) -> Envelope:
    from ._envelope import Envelope as Env

    case = envelope.case
    ct = case.case_type

    if ct == CaseType.COMPRESSED:
        matches = (
            target_digests is None or envelope.digest() in target_digests
        )
        if matches:
            try:
                decompressed = envelope.decompress()
                return _walk_decompress_impl(decompressed, target_digests)
            except Exception:
                pass
        return envelope

    if ct == CaseType.NODE:
        new_subject = _walk_decompress_impl(case.subject, target_digests)
        new_assertions = [
            _walk_decompress_impl(a, target_digests) for a in case.assertions
        ]
        if new_subject.is_identical_to(case.subject) and all(
            na.is_identical_to(oa)
            for na, oa in zip(new_assertions, case.assertions)
        ):
            return envelope
        return Env.new_with_unchecked_assertions(new_subject, new_assertions)

    if ct == CaseType.WRAPPED:
        new_inner = _walk_decompress_impl(case.envelope, target_digests)
        if new_inner.is_identical_to(case.envelope):
            return envelope
        return new_inner.wrap()

    if ct == CaseType.ASSERTION:
        new_pred = _walk_decompress_impl(
            case.assertion.predicate, target_digests
        )
        new_obj = _walk_decompress_impl(
            case.assertion.object, target_digests
        )
        if new_pred.is_identical_to(
            case.assertion.predicate
        ) and new_obj.is_identical_to(case.assertion.object):
            return envelope
        return Env.new_assertion(new_pred, new_obj)

    return envelope
