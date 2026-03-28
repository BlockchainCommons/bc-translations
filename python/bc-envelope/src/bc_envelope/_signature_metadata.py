"""Signature metadata for Gordian Envelope signatures.

Provides ``SignatureMetadata``, a collection of assertions that can be
attached to a signature and then co-signed with the same key.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from ._assertion import Assertion
    from ._envelope import Envelope


@dataclass
class SignatureMetadata:
    """Metadata associated with a signature.

    Contains a list of ``Assertion`` objects that will be attached to
    the signature envelope and then co-signed with the same key.
    """

    _assertions: list[Assertion] = field(default_factory=list)

    # --- Construction ---

    def add_assertion(self, assertion: Assertion) -> SignatureMetadata:
        """Return a new metadata instance with *assertion* appended."""
        new = SignatureMetadata(list(self._assertions))
        new._assertions.append(assertion)
        return new

    def with_assertion(
        self,
        predicate: object,
        obj: object,
    ) -> SignatureMetadata:
        """Convenience: create an ``Assertion`` from *predicate* and *obj* and append it."""
        from ._assertion import Assertion as A
        from ._envelope import Envelope as Env

        pred_env = predicate if isinstance(predicate, Env) else Env(predicate)
        obj_env = obj if isinstance(obj, Env) else Env(obj)
        return self.add_assertion(A(pred_env, obj_env))

    # --- Accessors ---

    @property
    def assertions(self) -> list[Assertion]:
        return list(self._assertions)

    def has_assertions(self) -> bool:
        return len(self._assertions) > 0
