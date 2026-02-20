"""Blockchain Commons Random Number Utilities.

Provides a uniform API for random number primitives used in higher-level
Blockchain Commons projects, including a cryptographically strong generator
(``SecureRandomNumberGenerator``) and a deterministic generator for testing
(``SeededRandomNumberGenerator``).
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
    """Return a thread-safe cryptographically secure RNG.

    Equivalent to Rust's ``rand::rng()`` / ``thread_rng()``.
    """
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
