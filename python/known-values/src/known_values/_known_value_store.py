"""Bidirectional store for known values."""

from __future__ import annotations

from collections.abc import Iterable
from pathlib import Path
from typing import TYPE_CHECKING

from ._known_value import KnownValue

if TYPE_CHECKING:
    from ._directory_loader import DirectoryConfig, LoadResult


class KnownValuesStore:
    """A store that maps between known values and their assigned names.

    Construct with an optional iterable of :class:`KnownValue` instances::

        store = KnownValuesStore([IS_A, NOTE])
    """

    __slots__ = ("_known_values_by_assigned_name", "_known_values_by_raw_value")

    def __init__(self, known_values: Iterable[KnownValue] | None = None) -> None:
        self._known_values_by_raw_value: dict[int, KnownValue] = {}
        self._known_values_by_assigned_name: dict[str, KnownValue] = {}
        if known_values is not None:
            for known_value in known_values:
                self._insert(
                    known_value,
                    self._known_values_by_raw_value,
                    self._known_values_by_assigned_name,
                )

    def insert(self, known_value: KnownValue) -> None:
        """Insert a KnownValue into the store."""
        self._insert(
            known_value,
            self._known_values_by_raw_value,
            self._known_values_by_assigned_name,
        )

    def assigned_name_for(self, known_value: KnownValue) -> str | None:
        """Return the store-assigned name for a KnownValue if present."""
        current = self._known_values_by_raw_value.get(known_value.value)
        if current is None:
            return None
        return current.assigned_name

    def name_for(self, known_value: KnownValue) -> str:
        """Return the store-assigned name or the KnownValue default name."""
        assigned = self.assigned_name_for(known_value)
        if assigned is not None:
            return assigned
        return known_value.name

    def known_value_named(self, assigned_name: str) -> KnownValue | None:
        """Look up a KnownValue by assigned name."""
        return self._known_values_by_assigned_name.get(assigned_name)

    @staticmethod
    def known_value_for_raw_value(
        raw_value: int,
        known_values: KnownValuesStore | None,
    ) -> KnownValue:
        """Return the stored KnownValue for a raw value or a new unnamed value."""
        if known_values is not None:
            match = known_values._known_values_by_raw_value.get(raw_value)
            if match is not None:
                return match
        return KnownValue(raw_value)

    @staticmethod
    def known_value_for_name(
        name: str,
        known_values: KnownValuesStore | None,
    ) -> KnownValue | None:
        """Return the stored KnownValue for a name or ``None``."""
        if known_values is None:
            return None
        return known_values.known_value_named(name)

    @staticmethod
    def name_for_known_value(
        known_value: KnownValue,
        known_values: KnownValuesStore | None,
    ) -> str:
        """Return a name for a KnownValue using a store if one is available."""
        if known_values is not None:
            assigned = known_values.assigned_name_for(known_value)
            if assigned is not None:
                return assigned
        return known_value.name

    def load_from_directory(self, path: Path) -> int:
        """Load known values from a directory of JSON registry files."""
        from ._directory_loader import load_from_directory

        values = load_from_directory(path)
        for value in values:
            self.insert(value)
        return len(values)

    def load_from_config(self, config: DirectoryConfig) -> LoadResult:
        """Load known values from all directories in a DirectoryConfig."""
        from ._directory_loader import load_from_config

        result = load_from_config(config)
        for value in result.values.values():
            self.insert(value)
        return result

    @staticmethod
    def _insert(
        known_value: KnownValue,
        known_values_by_raw_value: dict[int, KnownValue],
        known_values_by_assigned_name: dict[str, KnownValue],
    ) -> None:
        old_value = known_values_by_raw_value.get(known_value.value)
        if old_value is not None:
            old_name = old_value.assigned_name
            if old_name is not None:
                known_values_by_assigned_name.pop(old_name, None)

        known_values_by_raw_value[known_value.value] = known_value
        name = known_value.assigned_name
        if name is not None:
            known_values_by_assigned_name[name] = known_value

    def __len__(self) -> int:
        """Return the number of values in the store."""
        return len(self._known_values_by_raw_value)

    def __contains__(self, item: object) -> bool:
        """Check whether a KnownValue (by code point) is in the store."""
        if isinstance(item, KnownValue):
            return item.value in self._known_values_by_raw_value
        if isinstance(item, int):
            return item in self._known_values_by_raw_value
        return NotImplemented

    def __repr__(self) -> str:
        values = list(self._known_values_by_raw_value.values())
        return f"KnownValuesStore({values!r})"
