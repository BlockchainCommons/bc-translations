"""DigestProvider protocol."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from ._digest import Digest


@runtime_checkable
class DigestProvider(Protocol):
    """A type that can provide a single unique digest characterizing its contents."""

    def digest(self) -> Digest: ...
