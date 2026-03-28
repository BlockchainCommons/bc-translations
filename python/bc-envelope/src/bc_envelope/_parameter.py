"""Parameter type for Gordian Envelope expressions.

A parameter identifier that can be either numeric (Known) or string (Named),
CBOR-tagged with #6.40007 (TAG_PARAMETER).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_tags import TAG_PARAMETER, tags_for_values
from dcbor import CBOR, Tag

if TYPE_CHECKING:
    from ._envelope import Envelope


class Parameter:
    """A parameter identifier used in Gordian Envelope expressions.

    Parameters appear as assertion predicates on expression envelopes.
    Can be identified by a numeric ID (Known) or by a string name (Named).
    """

    __slots__ = ("_value", "_name", "_is_known")

    def __init__(
        self,
        value: int | None = None,
        name: str | None = None,
        *,
        is_known: bool = True,
    ) -> None:
        self._value = value
        self._name = name
        self._is_known = is_known

    # --- Factories ---

    @staticmethod
    def new_known(value: int, name: str | None = None) -> Parameter:
        """Create a parameter identified by a numeric ID, with an optional name."""
        return Parameter(value=value, name=name, is_known=True)

    @staticmethod
    def new_named(name: str) -> Parameter:
        """Create a parameter identified by a string name."""
        return Parameter(value=None, name=name, is_known=False)

    # --- Properties ---

    @property
    def is_known(self) -> bool:
        return self._is_known

    @property
    def value(self) -> int | None:
        return self._value

    @property
    def name(self) -> str:
        """Return the display name of the parameter."""
        if self._is_known:
            if self._name is not None:
                return self._name
            return str(self._value)
        assert self._name is not None
        return f'"{self._name}"'

    # --- Equality / Hash ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Parameter):
            return NotImplemented
        if self._is_known and other._is_known:
            return self._value == other._value
        if not self._is_known and not other._is_known:
            return self._name == other._name
        return False

    def __hash__(self) -> int:
        if self._is_known:
            return hash(("known", self._value))
        return hash(("named", self._name))

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_PARAMETER])

    def untagged_cbor(self) -> CBOR:
        if self._is_known:
            return CBOR.from_int(self._value)
        return CBOR.from_text(self._name)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Parameter:
        if cbor.is_unsigned:
            return cls.new_known(cbor.try_int())
        if cbor.is_text:
            return cls.new_named(cbor.try_text())
        raise ValueError("invalid parameter CBOR")

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Parameter:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Parameter:
        return cls.from_tagged_cbor(CBOR.from_data(data))

    # --- Envelope integration ---

    def to_envelope(self) -> Envelope:
        """Convert this parameter to an Envelope."""
        from ._envelope import Envelope as Env
        return Env.new_leaf(self)

    # --- Display ---

    def __repr__(self) -> str:
        if self._is_known:
            return f"Parameter({self._value}, {self._name!r})"
        return f"Parameter({self._name!r})"

    def __str__(self) -> str:
        return self.name
