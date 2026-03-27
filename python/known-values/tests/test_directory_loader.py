"""Tests translated from `src/directory_loader.rs` and `tests/directory_loading.rs`."""

from __future__ import annotations

import tempfile
from pathlib import Path

import pytest
import known_values._directory_loader as directory_loader
import known_values._known_values_registry as known_values_registry

from known_values import (
    DirectoryConfig,
    IS_A,
    KNOWN_VALUES,
    KnownValuesStore,
    LoadError,
    LoadResult,
    NOTE,
    load_from_config,
    load_from_directory,
)


def _temp_dir() -> Path:
    return Path(tempfile.mkdtemp())


def test_parse_registry_json() -> None:
    registry = load_from_config(
        DirectoryConfig.with_paths(
            [
                _write_registry_dir(
                    {
                        "test.json": """{
                            "ontology": {"name": "test"},
                            "entries": [
                                {"codepoint": 9999, "name": "testValue", "type": "property"}
                            ],
                            "statistics": {}
                        }""",
                    }
                )
            ]
        )
    )

    assert len(registry) == 1
    assert registry.values[9999].name == "testValue"


def test_parse_minimal_registry() -> None:
    values = load_from_directory(
        _write_registry_dir(
            {"minimal.json": """{"entries": [{"codepoint": 1, "name": "minimal"}]}"""}
        )
    )
    assert len(values) == 1
    assert values[0].value == 1


def test_parse_full_entry() -> None:
    result = load_from_config(
        DirectoryConfig.with_paths(
            [
                _write_registry_dir(
                    {
                        "full.json": """{
                            "entries": [{
                                "codepoint": 100,
                                "name": "fullEntry",
                                "type": "class",
                                "uri": "https://example.com/vocab#fullEntry",
                                "description": "A complete entry with all fields"
                            }]
                        }""",
                    }
                )
            ]
        )
    )
    assert result.values[100].name == "fullEntry"


def test_directory_config_default() -> None:
    config = DirectoryConfig.default_only()
    assert len(config.paths) == 1
    assert config.paths[0].name == ".known-values"


def test_directory_config_new_and_add_path(monkeypatch: pytest.MonkeyPatch) -> None:
    config = directory_loader.DirectoryConfig()
    assert config.paths == []

    config.add_path(Path("/tmp/known-values"))
    assert config.paths == [Path("/tmp/known-values")]

    monkeypatch.setattr(
        Path,
        "home",
        staticmethod(
            lambda: (_ for _ in ()).throw(RuntimeError("no home")),
        ),
    )
    assert (
        directory_loader.DirectoryConfig.default_directory()
        == Path(".") / ".known-values"
    )


def test_directory_config_custom_paths_unit() -> None:
    config = DirectoryConfig.with_paths([Path("/a"), Path("/b")])
    assert len(config.paths) == 2
    assert config.paths[0] == Path("/a")
    assert config.paths[1] == Path("/b")


def test_directory_config_with_default() -> None:
    config = DirectoryConfig.with_paths_and_default([Path("/custom")])
    assert len(config.paths) == 2
    assert config.paths[0] == Path("/custom")
    assert config.paths[1].name == ".known-values"


def test_load_from_nonexistent_directory() -> None:
    result = load_from_directory(Path("/nonexistent/path/12345"))
    assert result == []


def test_load_result_methods_unit() -> None:
    result = LoadResult()
    assert len(result) == 0
    assert not result.has_errors

    result.values[1] = KnownValuesStore.known_value_for_raw_value(1, None)
    assert len(result) == 1


def test_load_error_helper_messages() -> None:
    io_error = directory_loader.LoadError.io(OSError("boom"))
    json_error = directory_loader.LoadError.json(
        Path("bad.json"),
        ValueError("bad json"),
    )

    assert str(io_error) == "IO error: boom"
    assert str(json_error) == "JSON parse error in bad.json: bad json"
    assert directory_loader.ConfigError() == directory_loader.ConfigError()
    assert hash(directory_loader.ConfigError()) == hash(
        directory_loader.ConfigError()
    )


def test_directory_config_repr() -> None:
    config = DirectoryConfig.with_paths([Path("/a"), Path("/b")])
    r = repr(config)
    assert "DirectoryConfig" in r
    assert "/a" in r


def test_global_registry_still_works() -> None:
    store = KNOWN_VALUES.get()

    is_a = store.known_value_named("isA")
    assert is_a is not None
    assert is_a.value == 1
    assert KNOWN_VALUES.get() is store


def test_global_registry_loads_custom_config_before_first_access() -> None:
    temp_dir = _write_registry_dir(
        {
            "custom.json": """{"entries": [{"codepoint": 80001, "name": "customLoaded"}]}""",
        }
    )

    directory_loader.set_directory_config(DirectoryConfig.with_paths([temp_dir]))
    store = known_values_registry.KNOWN_VALUES.get()

    assert store.known_value_named("customLoaded") is not None
    assert store.known_value_named("customLoaded").value == 80001


def test_load_from_temp_directory() -> None:
    temp_dir = _write_registry_dir(
        {
            "test_registry.json": """{
                "entries": [
                    {"codepoint": 99999, "name": "integrationTestValue"}
                ]
            }""",
        }
    )

    store = KnownValuesStore([IS_A, NOTE])
    count = store.load_from_directory(temp_dir)

    assert count == 1
    loaded = store.known_value_named("integrationTestValue")
    assert loaded is not None
    assert loaded.value == 99999
    assert store.known_value_named("isA") is not None
    assert store.known_value_named("note") is not None


def test_override_hardcoded_value() -> None:
    temp_dir = _write_registry_dir(
        {
            "override.json": """{
                "entries": [
                    {"codepoint": 1, "name": "overriddenIsA"}
                ]
            }""",
        }
    )

    store = KnownValuesStore([IS_A])
    store.load_from_directory(temp_dir)

    assert store.known_value_named("isA") is None
    overridden = store.known_value_named("overriddenIsA")
    assert overridden is not None
    assert overridden.value == 1


def test_multiple_files_in_directory() -> None:
    temp_dir = _write_registry_dir(
        {
            "registry1.json": """{"entries": [{"codepoint": 10001, "name": "valueOne"}]}""",
            "registry2.json": """{"entries": [{"codepoint": 10002, "name": "valueTwo"}]}""",
        }
    )

    store = KnownValuesStore()
    count = store.load_from_directory(temp_dir)

    assert count == 2
    assert store.known_value_named("valueOne") is not None
    assert store.known_value_named("valueTwo") is not None


def test_directory_config_custom_paths_integration() -> None:
    temp_dir1 = _write_registry_dir(
        {"a.json": """{"entries": [{"codepoint": 20001, "name": "fromDirOne"}]}"""}
    )
    temp_dir2 = _write_registry_dir(
        {"b.json": """{"entries": [{"codepoint": 20002, "name": "fromDirTwo"}]}"""}
    )

    config = DirectoryConfig.with_paths([temp_dir1, temp_dir2])
    store = KnownValuesStore()
    result = store.load_from_config(config)

    assert len(result) == 2
    assert store.known_value_named("fromDirOne") is not None
    assert store.known_value_named("fromDirTwo") is not None


def test_later_directory_overrides_earlier() -> None:
    temp_dir1 = _write_registry_dir(
        {"first.json": """{"entries": [{"codepoint": 30000, "name": "firstVersion"}]}"""}
    )
    temp_dir2 = _write_registry_dir(
        {"second.json": """{"entries": [{"codepoint": 30000, "name": "secondVersion"}]}"""}
    )

    config = DirectoryConfig.with_paths([temp_dir1, temp_dir2])
    store = KnownValuesStore()
    store.load_from_config(config)

    value = store.known_value_named("secondVersion")
    assert value is not None
    assert value.value == 30000
    assert store.known_value_named("firstVersion") is None


def test_nonexistent_directory_is_ok() -> None:
    store = KnownValuesStore()
    result = store.load_from_directory(Path("/nonexistent/path/12345"))
    assert result == 0


def test_invalid_json_is_error() -> None:
    temp_dir = _write_registry_dir({"invalid.json": "{ this is not valid json }"})

    store = KnownValuesStore()
    with pytest.raises(LoadError):
        store.load_from_directory(temp_dir)


def test_tolerant_loading_continues_on_error() -> None:
    temp_dir = _write_registry_dir(
        {
            "valid.json": """{"entries": [{"codepoint": 40001, "name": "validValue"}]}""",
            "invalid.json": "{ invalid json }",
        }
    )

    config = DirectoryConfig.with_paths([temp_dir])
    result = load_from_config(config)

    assert 40001 in result.values
    assert result.has_errors


def test_full_registry_format() -> None:
    temp_dir = _write_registry_dir(
        {
            "full_format.json": """{
                "ontology": {
                    "name": "test_registry",
                    "source_url": "https://example.com",
                    "start_code_point": 50000,
                    "processing_strategy": "test"
                },
                "generated": {
                    "tool": "test"
                },
                "entries": [
                    {
                        "codepoint": 50001,
                        "name": "fullFormatValue",
                        "type": "property",
                        "uri": "https://example.com/vocab#fullFormatValue",
                        "description": "A value in full format"
                    },
                    {
                        "codepoint": 50002,
                        "name": "anotherValue",
                        "type": "class"
                    }
                ],
                "statistics": {
                    "total_entries": 2
                }
            }""",
        }
    )

    store = KnownValuesStore()
    count = store.load_from_directory(temp_dir)

    assert count == 2
    assert store.known_value_named("fullFormatValue") is not None
    assert store.known_value_named("anotherValue") is not None


def test_load_result_methods_integration() -> None:
    temp_dir = _write_registry_dir(
        {
            "test.json": """{"entries": [
                {"codepoint": 60001, "name": "resultTest1"},
                {"codepoint": 60002, "name": "resultTest2"}
            ]}""",
        }
    )

    config = DirectoryConfig.with_paths([temp_dir])
    result = load_from_config(config)

    assert len(result) == 2
    assert not result.has_errors
    assert len(result.files_processed) == 1

    values = list(result)
    assert len(values) == 2


def test_empty_entries_array() -> None:
    temp_dir = _write_registry_dir({"empty.json": """{"entries": []}"""})

    store = KnownValuesStore()
    count = store.load_from_directory(temp_dir)

    assert count == 0


def test_non_json_files_ignored() -> None:
    temp_dir = _write_registry_dir(
        {
            "valid.json": """{"entries": [{"codepoint": 70001, "name": "jsonValue"}]}""",
            "readme.txt": "Some text",
            "data.xml": "<xml/>",
        }
    )

    store = KnownValuesStore()
    count = store.load_from_directory(temp_dir)

    assert count == 1
    assert store.known_value_named("jsonValue") is not None


def test_add_search_paths_creates_default_config() -> None:
    directory_loader._CUSTOM_CONFIG = None
    directory_loader.add_search_paths([Path("/tmp/extra-known-values")])
    assert directory_loader._CUSTOM_CONFIG is not None
    assert Path("/tmp/extra-known-values") in directory_loader._CUSTOM_CONFIG.paths


def test_directory_iteration_errors_are_reported(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
) -> None:
    original_iterdir = Path.iterdir

    def failing_iterdir(self: Path):
        if self == tmp_path:
            raise OSError("iterdir failed")
        return original_iterdir(self)

    monkeypatch.setattr(Path, "iterdir", failing_iterdir)

    with pytest.raises(LoadError):
        load_from_directory(tmp_path)

    result = load_from_config(DirectoryConfig.with_paths([tmp_path]))
    assert result.has_errors
    assert result.files_processed == []


def test_file_read_errors_are_reported(monkeypatch: pytest.MonkeyPatch) -> None:
    temp_dir = _write_registry_dir(
        {
            "valid.json": """{"entries": [{"codepoint": 90001, "name": "willFail"}]}""",
        }
    )
    target_file = temp_dir / "valid.json"
    original_read_text = Path.read_text

    def failing_read_text(self: Path, *args, **kwargs):
        if self == target_file:
            raise OSError("read failed")
        return original_read_text(self, *args, **kwargs)

    monkeypatch.setattr(Path, "read_text", failing_read_text)

    with pytest.raises(LoadError):
        load_from_directory(temp_dir)


def test_parser_validation_errors() -> None:
    with pytest.raises(ValueError):
        directory_loader._parse_registry_file([])

    with pytest.raises(ValueError):
        directory_loader._parse_registry_file({"entries": {}})

    with pytest.raises(ValueError):
        directory_loader._parse_registry_file({"ontology": [], "entries": []})

    with pytest.raises(ValueError):
        directory_loader._parse_registry_file({"generated": [], "entries": []})

    with pytest.raises(ValueError):
        directory_loader._parse_registry_entry([])

    with pytest.raises(ValueError):
        directory_loader._required_int(True, "codepoint")

    with pytest.raises(ValueError):
        directory_loader._required_str(1, "name")


def test_get_and_lock_config_defaults_when_unset() -> None:
    directory_loader._CUSTOM_CONFIG = None
    directory_loader._CONFIG_LOCKED = False
    config = directory_loader._get_and_lock_config()
    assert config.paths[0].name == ".known-values"


def test_tolerant_loader_handles_missing_and_non_json_paths() -> None:
    missing_values, missing_errors = directory_loader._load_from_directory_tolerant(
        Path("/nonexistent/path/67890")
    )
    assert missing_values == []
    assert missing_errors == []

    temp_dir = _write_registry_dir({"readme.txt": "ignored"})
    values, errors = directory_loader._load_from_directory_tolerant(temp_dir)
    assert values == []
    assert errors == []


def _write_registry_dir(files: dict[str, str]) -> Path:
    directory = _temp_dir()
    for name, content in files.items():
        (directory / name).write_text(content, encoding="utf-8")
    return directory
