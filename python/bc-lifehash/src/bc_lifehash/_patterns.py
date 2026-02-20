from __future__ import annotations

from enum import Enum

from ._bit_enumerator import BitEnumerator


class Pattern(Enum):
    SNOWFLAKE = "snowflake"
    PINWHEEL = "pinwheel"
    FIDUCIAL = "fiducial"


def select_pattern(entropy: BitEnumerator, version: Version) -> Pattern:
    from ._lifehash import Version

    if version in (Version.FIDUCIAL, Version.GRAYSCALE_FIDUCIAL):
        return Pattern.FIDUCIAL
    if entropy.next():
        return Pattern.SNOWFLAKE
    return Pattern.PINWHEEL
