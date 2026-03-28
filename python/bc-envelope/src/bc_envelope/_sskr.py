"""SSKR (Sharded Secret Key Reconstruction) extension for Gordian Envelope.

Splits an encrypted envelope's content key into SSKR shares and
reconstructs the original envelope from a sufficient threshold of shares.
"""

from __future__ import annotations

from collections import defaultdict
from typing import TYPE_CHECKING

from bc_components import (
    SSKRGroupSpec,
    SSKRSecret,
    SSKRShare,
    SSKRSpec,
    SymmetricKey,
    sskr_combine,
    sskr_generate_using,
)
from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
import known_values

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Internal helper
# ---------------------------------------------------------------------------

def _add_sskr_share(self: Envelope, share: SSKRShare) -> Envelope:
    """Add a ``'sskrShare': SSKRShare`` assertion."""
    return self.add_assertion(known_values.SSKR_SHARE, share)


# ---------------------------------------------------------------------------
# sskr_split
# ---------------------------------------------------------------------------

def sskr_split(
    self: Envelope,
    spec: SSKRSpec,
    content_key: SymmetricKey,
) -> list[list[Envelope]]:
    """Split the envelope into grouped SSKR shares."""
    rng = SecureRandomNumberGenerator()
    return sskr_split_using(self, spec, content_key, rng)


def sskr_split_flattened(
    self: Envelope,
    spec: SSKRSpec,
    content_key: SymmetricKey,
) -> list[Envelope]:
    """Split the envelope into a flat list of SSKR shares."""
    groups = sskr_split(self, spec, content_key)
    return [share for group in groups for share in group]


def sskr_split_using(
    self: Envelope,
    spec: SSKRSpec,
    content_key: SymmetricKey,
    rng: RandomNumberGenerator,
) -> list[list[Envelope]]:
    """Split using a provided RNG for deterministic testing."""
    master_secret = SSKRSecret(content_key.data)
    shares = sskr_generate_using(spec, master_secret, rng)
    result: list[list[Envelope]] = []
    for group in shares:
        group_result: list[Envelope] = []
        for share in group:
            group_result.append(_add_sskr_share(self, share))
        result.append(group_result)
    return result


# ---------------------------------------------------------------------------
# sskr_join
# ---------------------------------------------------------------------------

def _sskr_shares_in(
    envelopes: list[Envelope],
) -> dict[int, list[SSKRShare]]:
    """Group shares from envelopes by their identifier."""
    result: dict[int, list[SSKRShare]] = defaultdict(list)
    for envelope in envelopes:
        for assertion in envelope.assertions_with_predicate(known_values.SSKR_SHARE):
            share: SSKRShare = assertion.as_object().extract_subject()
            result[share.identifier()].append(share)
    return result


def sskr_join(envelopes: list[Envelope]) -> Envelope:
    """Reconstruct the original envelope from a list of SSKR share envelopes.

    This is a static method -- call as ``Envelope.sskr_join(share_list)``.
    """
    from ._encrypt import decrypt_subject
    from ._error import InvalidShares

    if not envelopes:
        raise InvalidShares()

    grouped = _sskr_shares_in(envelopes)
    for shares in grouped.values():
        try:
            secret = sskr_combine(shares)
            content_key = SymmetricKey.from_data(bytes(secret))
            envelope = decrypt_subject(envelopes[0], content_key)
            return envelope.subject()
        except Exception:
            continue

    raise InvalidShares()
