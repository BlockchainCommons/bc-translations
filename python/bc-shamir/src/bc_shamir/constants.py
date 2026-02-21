"""Package constants for bc-shamir."""

# The minimum length of a secret.
MIN_SECRET_LEN = 16
# The maximum length of a secret.
MAX_SECRET_LEN = 32
# The maximum number of shares that can be generated from a secret.
MAX_SHARE_COUNT = 16

SECRET_INDEX = 255
DIGEST_INDEX = 254

__all__ = [
    "DIGEST_INDEX",
    "MAX_SECRET_LEN",
    "MAX_SHARE_COUNT",
    "MIN_SECRET_LEN",
    "SECRET_INDEX",
]
