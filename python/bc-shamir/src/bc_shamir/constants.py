"""Package constants for Shamir secret sharing."""

# The minimum length of a secret in bytes.
MIN_SECRET_LEN = 16
# The maximum length of a secret in bytes.
MAX_SECRET_LEN = 32
# The maximum number of shares that can be generated from a secret.
MAX_SHARE_COUNT = 16

# Internal share indexes used during split/recover.
_SECRET_INDEX = 255
_DIGEST_INDEX = 254

__all__ = [
    "MAX_SECRET_LEN",
    "MAX_SHARE_COUNT",
    "MIN_SECRET_LEN",
]
