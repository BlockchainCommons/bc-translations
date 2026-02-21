"""Blockchain Commons CBOR Tags.

This package re-exports the `dcbor` public API and adds Blockchain Commons
tag constants plus registration helpers from `bc-tags`.
"""

from dcbor import *  # noqa: F401,F403
from dcbor import __all__ as _DCBOR_ALL

from .tags_registry import *  # noqa: F401,F403
from .tags_registry import __all__ as _BC_TAGS_ALL

__all__ = list(dict.fromkeys([*_DCBOR_ALL, *_BC_TAGS_ALL]))
