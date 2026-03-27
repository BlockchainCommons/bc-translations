"""Directory-based loading of known values from JSON registry files."""

from __future__ import annotations

import json
from collections.abc import Iterator
from dataclasses import dataclass, field
from pathlib import Path
from threading import Lock
from typing import Any

from ._known_value import KnownValue


@dataclass(slots=True)
class RegistryEntry:
    """A single entry in a known values JSON registry file."""

    codepoint: int
    name: str
    entry_type: str | None = None
    uri: str | None = None
    description: str | None = None


@dataclass(slots=True)
class OntologyInfo:
    """Metadata about the ontology or registry source."""

    name: str | None = None
    source_url: str | None = None
    start_code_point: int | None = None
    processing_strategy: str | None = None


@dataclass(slots=True)
class GeneratedInfo:
    """Information about how a registry file was generated."""

    tool: str | None = None


@dataclass(slots=True)
class RegistryFile:
    """Root structure of a known values JSON registry file."""

    ontology: OntologyInfo | None = None
    generated: GeneratedInfo | None = None
    entries: list[RegistryEntry] = field(default_factory=list)
    statistics: Any | None = None


class LoadError(Exception):
    """Errors that can occur when loading known values from directories."""

    __slots__ = ("error", "file", "kind")

    def __init__(
        self,
        kind: str,
        error: Exception,
        file: Path | None = None,
    ) -> None:
        self.kind = kind
        self.file = file
        self.error = error
        if kind == "io":
            message = f"IO error: {error}"
        else:
            assert file is not None
            message = f"JSON parse error in {file}: {error}"
        super().__init__(message)
        self.__cause__ = error

    @classmethod
    def io(cls, error: OSError) -> LoadError:
        return cls("io", error)

    @classmethod
    def json(cls, file: Path, error: Exception) -> LoadError:
        return cls("json", error, file)


@dataclass(slots=True)
class LoadResult:
    """Result of a tolerant directory loading operation."""

    values: dict[int, KnownValue] = field(default_factory=dict)
    files_processed: list[Path] = field(default_factory=list)
    errors: list[tuple[Path, LoadError]] = field(default_factory=list)

    def __len__(self) -> int:
        """Return the number of unique values loaded."""
        return len(self.values)

    def __iter__(self) -> Iterator[KnownValue]:
        """Iterate over loaded KnownValue instances."""
        return iter(self.values.values())

    @property
    def has_errors(self) -> bool:
        """Whether any errors were recorded during loading."""
        return bool(self.errors)


class DirectoryConfig:
    """Configuration for loading known values from directories."""

    __slots__ = ("_paths",)

    def __init__(self, paths: list[Path] | None = None) -> None:
        self._paths = list(paths or [])

    @classmethod
    def default_only(cls) -> DirectoryConfig:
        """Create configuration containing only the default directory."""
        return cls([cls.default_directory()])

    @classmethod
    def with_paths(cls, paths: list[Path]) -> DirectoryConfig:
        """Create configuration with the provided paths."""
        return cls([Path(path) for path in paths])

    @classmethod
    def with_paths_and_default(cls, paths: list[Path]) -> DirectoryConfig:
        """Create configuration with custom paths followed by the default directory."""
        values = [Path(path) for path in paths]
        values.append(cls.default_directory())
        return cls(values)

    @staticmethod
    def default_directory() -> Path:
        """Return the default ``~/.known-values`` directory with a local fallback."""
        try:
            home = Path.home()
        except Exception:
            home = Path(".")
        return home / ".known-values"

    @property
    def paths(self) -> list[Path]:
        """The configured search paths (copy)."""
        return list(self._paths)

    def add_path(self, path: Path) -> None:
        """Append a search path so it takes precedence over earlier paths."""
        self._paths.append(Path(path))

    def __repr__(self) -> str:
        return f"DirectoryConfig(paths={self._paths!r})"


class ConfigError(Exception):
    """Raised when directory configuration is modified after initialization."""

    def __init__(self) -> None:
        super().__init__(
            "Cannot modify directory configuration after KNOWN_VALUES has been accessed"
        )

    def __eq__(self, other: object) -> bool:
        return isinstance(other, ConfigError)

    def __hash__(self) -> int:
        return hash(ConfigError)


_CUSTOM_CONFIG: DirectoryConfig | None = None
_CONFIG_LOCKED = False
_CONFIG_MUTEX = Lock()


def load_from_directory(path: Path) -> list[KnownValue]:
    """Load all JSON registry files from a single directory."""
    directory = Path(path)
    values: list[KnownValue] = []

    if not directory.exists() or not directory.is_dir():
        return values

    try:
        entries = list(directory.iterdir())
    except OSError as error:
        raise LoadError.io(error) from error

    for entry in entries:
        if entry.suffix != ".json":
            continue
        values.extend(_load_single_file(entry))

    return values


def load_from_config(config: DirectoryConfig) -> LoadResult:
    """Load known values from all directories in the given configuration."""
    result = LoadResult()

    for dir_path in config.paths:
        try:
            values, errors = _load_from_directory_tolerant(dir_path)
            for value in values:
                result.values[value.value] = value
            if errors:
                result.errors.extend(errors)
            result.files_processed.append(dir_path)
        except LoadError as error:
            result.errors.append((dir_path, error))

    return result


def set_directory_config(config: DirectoryConfig) -> None:
    """Set custom directory configuration before the global registry is accessed."""
    global _CUSTOM_CONFIG

    with _CONFIG_MUTEX:
        if _CONFIG_LOCKED:
            raise ConfigError()
        _CUSTOM_CONFIG = config


def add_search_paths(paths: list[Path]) -> None:
    """Add search paths before the global registry is accessed."""
    global _CUSTOM_CONFIG

    with _CONFIG_MUTEX:
        if _CONFIG_LOCKED:
            raise ConfigError()
        config = _CUSTOM_CONFIG
        if config is None:
            config = DirectoryConfig.default_only()
            _CUSTOM_CONFIG = config
        for path in paths:
            config.add_path(Path(path))


def _load_from_directory_tolerant(
    path: Path,
) -> tuple[list[KnownValue], list[tuple[Path, LoadError]]]:
    values: list[KnownValue] = []
    errors: list[tuple[Path, LoadError]] = []
    directory = Path(path)

    if not directory.exists() or not directory.is_dir():
        return values, errors

    try:
        entries = list(directory.iterdir())
    except OSError as error:
        raise LoadError.io(error) from error

    for entry in entries:
        if entry.suffix != ".json":
            continue
        try:
            values.extend(_load_single_file(entry))
        except LoadError as error:
            errors.append((entry, error))

    return values, errors


def _load_single_file(path: Path) -> list[KnownValue]:
    try:
        content = path.read_text(encoding="utf-8")
    except OSError as error:
        raise LoadError.io(error) from error

    try:
        raw = json.loads(content)
        registry = _parse_registry_file(raw)
    except (json.JSONDecodeError, TypeError, ValueError, KeyError) as error:
        raise LoadError.json(path, error) from error

    return [
        KnownValue(entry.codepoint, entry.name)
        for entry in registry.entries
    ]


def _parse_registry_file(raw: Any) -> RegistryFile:
    if not isinstance(raw, dict):
        raise ValueError("registry file must be a JSON object")

    ontology_raw = raw.get("ontology")
    generated_raw = raw.get("generated")
    entries_raw = raw.get("entries")

    if not isinstance(entries_raw, list):
        raise ValueError("entries must be a JSON array")

    ontology = None
    if ontology_raw is not None:
        if not isinstance(ontology_raw, dict):
            raise ValueError("ontology must be a JSON object")
        ontology = OntologyInfo(
            name=_optional_str(ontology_raw.get("name"), "ontology.name"),
            source_url=_optional_str(
                ontology_raw.get("source_url"),
                "ontology.source_url",
            ),
            start_code_point=_optional_int(
                ontology_raw.get("start_code_point"),
                "ontology.start_code_point",
            ),
            processing_strategy=_optional_str(
                ontology_raw.get("processing_strategy"),
                "ontology.processing_strategy",
            ),
        )

    generated = None
    if generated_raw is not None:
        if not isinstance(generated_raw, dict):
            raise ValueError("generated must be a JSON object")
        generated = GeneratedInfo(
            tool=_optional_str(generated_raw.get("tool"), "generated.tool")
        )

    entries = [_parse_registry_entry(entry) for entry in entries_raw]
    return RegistryFile(
        ontology=ontology,
        generated=generated,
        entries=entries,
        statistics=raw.get("statistics"),
    )


def _parse_registry_entry(raw: Any) -> RegistryEntry:
    if not isinstance(raw, dict):
        raise ValueError("registry entry must be a JSON object")
    return RegistryEntry(
        codepoint=_required_int(raw.get("codepoint"), "codepoint"),
        name=_required_str(raw.get("name"), "name"),
        entry_type=_optional_str(raw.get("type"), "type"),
        uri=_optional_str(raw.get("uri"), "uri"),
        description=_optional_str(raw.get("description"), "description"),
    )


def _required_int(value: Any, field_name: str) -> int:
    if isinstance(value, bool) or not isinstance(value, int):
        raise ValueError(f"{field_name} must be an integer")
    return value


def _optional_int(value: Any, field_name: str) -> int | None:
    if value is None:
        return None
    return _required_int(value, field_name)


def _required_str(value: Any, field_name: str) -> str:
    if not isinstance(value, str):
        raise ValueError(f"{field_name} must be a string")
    return value


def _optional_str(value: Any, field_name: str) -> str | None:
    if value is None:
        return None
    return _required_str(value, field_name)


def _get_and_lock_config() -> DirectoryConfig:
    global _CUSTOM_CONFIG, _CONFIG_LOCKED

    with _CONFIG_MUTEX:
        _CONFIG_LOCKED = True
        config = _CUSTOM_CONFIG
        _CUSTOM_CONFIG = None
    if config is None:
        return DirectoryConfig.default_only()
    return config


__all__ = [
    "ConfigError",
    "DirectoryConfig",
    "GeneratedInfo",
    "LoadError",
    "LoadResult",
    "OntologyInfo",
    "RegistryEntry",
    "RegistryFile",
    "add_search_paths",
    "load_from_config",
    "load_from_directory",
    "set_directory_config",
]
