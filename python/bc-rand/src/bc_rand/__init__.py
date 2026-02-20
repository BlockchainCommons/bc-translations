"""Blockchain Commons Random Number Utilities.

``bc_rand`` exposes a uniform API for the random number primitives used in
higher-level Blockchain Commons projects, including a cryptographically strong
random number generator (``SecureRandomNumberGenerator``) and a deterministic
random number generator (``SeededRandomNumberGenerator``).

These primitive random number generators implement the
``RandomNumberGenerator`` protocol to produce random numbers, which is
important when using the deterministic random number generator for
cross-platform testing.

The package also includes several convenience functions for generating secure
and deterministic random numbers.
"""

from .random_number_generator import (
    BitWidth,
    RandomNumberGenerator,
    rng_fill_random_data,
    rng_next_in_closed_range,
    rng_next_in_range,
    rng_next_with_upper_bound,
    rng_random_array,
    rng_random_bool,
    rng_random_data,
    rng_random_u32,
)
from .secure_random import (
    SecureRandomNumberGenerator,
    fill_random_data,
    random_data,
)
from .seeded_random import (
    SeededRandomNumberGenerator,
    fake_random_data,
    make_fake_random_number_generator,
)

def thread_rng() -> SecureRandomNumberGenerator:
    """Return a thread-safe cryptographically secure RNG."""
    return SecureRandomNumberGenerator()


__all__ = [
    "BitWidth",
    "RandomNumberGenerator",
    "SecureRandomNumberGenerator",
    "SeededRandomNumberGenerator",
    "fake_random_data",
    "fill_random_data",
    "make_fake_random_number_generator",
    "random_data",
    "rng_fill_random_data",
    "rng_next_in_closed_range",
    "rng_next_in_range",
    "rng_next_with_upper_bound",
    "rng_random_array",
    "rng_random_bool",
    "rng_random_data",
    "rng_random_u32",
    "thread_rng",
]
