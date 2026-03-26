from __future__ import annotations

import pytest

import known_values._directory_loader as directory_loader
import known_values._known_values_registry as known_values_registry


@pytest.fixture(autouse=True)
def reset_known_values_state() -> None:
    with directory_loader._CONFIG_MUTEX:
        directory_loader._CUSTOM_CONFIG = directory_loader.DirectoryConfig.with_paths([])
        directory_loader._CONFIG_LOCKED = False
    with known_values_registry.KNOWN_VALUES._lock:
        known_values_registry.KNOWN_VALUES._data = None
