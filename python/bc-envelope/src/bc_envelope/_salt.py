"""Salt extension for Gordian Envelope.

Adds random salt assertions to decorrelate envelopes, preventing
third parties from correlating elided envelopes by comparing digests.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Salt
from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
import known_values

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Public API
# ---------------------------------------------------------------------------

def add_salt(self: Envelope) -> Envelope:
    """Add proportionally-sized random salt to decorrelate the envelope."""
    rng = SecureRandomNumberGenerator()
    return add_salt_using(self, rng)


def add_salt_instance(self: Envelope, salt: Salt) -> Envelope:
    """Add a specific ``Salt`` as an assertion."""
    return self.add_assertion(known_values.SALT, salt)


def add_salt_with_length(self: Envelope, count: int) -> Envelope:
    """Add salt of exactly *count* bytes (minimum 8)."""
    rng = SecureRandomNumberGenerator()
    return add_salt_with_length_using(self, count, rng)


def add_salt_in_range(self: Envelope, start: int, end: int) -> Envelope:
    """Add salt with a length randomly chosen from ``[start, end]``."""
    rng = SecureRandomNumberGenerator()
    return add_salt_in_range_using(self, start, end, rng)


# ---------------------------------------------------------------------------
# Internal ``_using`` variants for deterministic testing
# ---------------------------------------------------------------------------

def add_salt_using(self: Envelope, rng: RandomNumberGenerator) -> Envelope:
    """Add proportional salt using the given RNG."""
    size = len(self.tagged_cbor().to_cbor_data())
    salt = Salt.generate_for_size_using(size, rng)
    return add_salt_instance(self, salt)


def add_salt_with_length_using(
    self: Envelope,
    count: int,
    rng: RandomNumberGenerator,
) -> Envelope:
    """Add salt of *count* bytes using the given RNG."""
    salt = Salt.generate_with_len_using(count, rng)
    return add_salt_instance(self, salt)


def add_salt_in_range_using(
    self: Envelope,
    start: int,
    end: int,
    rng: RandomNumberGenerator,
) -> Envelope:
    """Add salt with random length in ``[start, end]`` using the given RNG."""
    salt = Salt.generate_in_range_using(start, end, rng)
    return add_salt_instance(self, salt)
