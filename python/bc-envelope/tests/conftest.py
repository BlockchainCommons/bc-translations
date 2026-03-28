"""Shared test fixtures for bc-envelope tests."""
import pytest

from bc_envelope import with_format_context, register_tags as _register_tags


@pytest.fixture(autouse=True)
def _setup_tags():
    """Register known tags before each test.

    We trigger global context initialization first (via with_format_context)
    to avoid a deadlock in register_tags caused by a non-reentrant lock.
    """
    # Ensure the global format context is initialized before calling
    # register_tags, which acquires the same lock inside with_format_context_mut.
    with_format_context(lambda ctx: None)
    _register_tags()
