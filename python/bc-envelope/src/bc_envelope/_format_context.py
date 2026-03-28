"""Format context for annotated envelope formatting.

Provides a FormatContext containing CBOR tag, known-value, function, and
parameter registries used when formatting envelopes as human-readable text.
"""

from __future__ import annotations

import enum
import threading
from collections.abc import Callable
from typing import TypeVar

from bc_components import register_tags as _bc_components_register_tags
from bc_tags import (
    TAG_EVENT,
    TAG_FUNCTION,
    TAG_KNOWN_VALUE,
    TAG_PARAMETER,
    TAG_REQUEST,
    TAG_RESPONSE,
)
from dcbor import CBOR, TagsStore, with_tags
from known_values import KNOWN_VALUES, KnownValue, KnownValuesStore

T = TypeVar("T")


# ---------------------------------------------------------------------------
# FormatContextOpt -- discriminated union for context selection
# ---------------------------------------------------------------------------

class _FormatContextMode(enum.Enum):
    NONE = "none"
    GLOBAL = "global"
    CUSTOM = "custom"


class FormatContextOpt:
    """Discriminated union selecting which format context to use.

    Use the class-level factory methods ``none()``, ``global_()``, or
    ``custom(context)`` to construct instances.
    """

    __slots__ = ("_mode", "_context")

    def __init__(
        self, mode: _FormatContextMode, context: FormatContext | None = None
    ) -> None:
        self._mode = mode
        self._context = context

    @staticmethod
    def none() -> FormatContextOpt:
        return FormatContextOpt(_FormatContextMode.NONE)

    @staticmethod
    def global_() -> FormatContextOpt:
        return FormatContextOpt(_FormatContextMode.GLOBAL)

    @staticmethod
    def custom(context: FormatContext) -> FormatContextOpt:
        return FormatContextOpt(_FormatContextMode.CUSTOM, context)

    @property
    def is_none(self) -> bool:
        return self._mode == _FormatContextMode.NONE

    @property
    def is_global(self) -> bool:
        return self._mode == _FormatContextMode.GLOBAL

    @property
    def is_custom(self) -> bool:
        return self._mode == _FormatContextMode.CUSTOM

    @property
    def context(self) -> FormatContext:
        """Return the custom context.  Raises if mode is not CUSTOM."""
        if self._context is None:
            raise ValueError("FormatContextOpt is not CUSTOM")
        return self._context

    def tags_store(self) -> TagsStore | None:
        """Return the underlying TagsStore, or ``None`` for mode NONE."""
        if self._mode == _FormatContextMode.NONE:
            return None
        if self._mode == _FormatContextMode.GLOBAL:
            return with_format_context(lambda ctx: ctx.tags)
        return self._context.tags if self._context is not None else None


# ---------------------------------------------------------------------------
# FormatContext
# ---------------------------------------------------------------------------

class FormatContext:
    """Context object for formatting Gordian Envelopes with annotations.

    Contains registries for CBOR tags, known values, functions, and parameters
    that enable human-readable output when formatting envelopes.
    """

    __slots__ = ("_tags", "_known_values", "_functions", "_parameters")

    def __init__(
        self,
        tags: TagsStore | None = None,
        known_values: KnownValuesStore | None = None,
        functions: object | None = None,
        parameters: object | None = None,
    ) -> None:
        self._tags = tags if tags is not None else TagsStore()
        self._known_values = (
            known_values if known_values is not None else KnownValuesStore()
        )
        # FunctionsStore / ParametersStore (lazy import to avoid circular)
        self._functions = functions
        self._parameters = parameters

    @property
    def tags(self) -> TagsStore:
        return self._tags

    @property
    def known_values(self) -> KnownValuesStore:
        return self._known_values

    @property
    def functions(self) -> object | None:
        """The FunctionsStore, or ``None``."""
        return self._functions

    @property
    def parameters(self) -> object | None:
        """The ParametersStore, or ``None``."""
        return self._parameters


# ---------------------------------------------------------------------------
# Global singleton
# ---------------------------------------------------------------------------

_global_lock = threading.RLock()
_global_format_context: FormatContext | None = None
_initialized = False


def _ensure_global_context() -> FormatContext:
    global _global_format_context, _initialized
    if _initialized:
        assert _global_format_context is not None
        return _global_format_context

    with _global_lock:
        if not _initialized:
            _bc_components_register_tags()
            tags_store = with_tags(lambda store: store)
            known_values_store = KNOWN_VALUES.get()

            # Import expression stores (may be None if expressions not yet loaded)
            try:
                from ._functions import global_functions
                from ._parameters import global_parameters
                funcs = global_functions()
                params = global_parameters()
            except ImportError:
                funcs = None
                params = None

            ctx = FormatContext(
                tags=tags_store,
                known_values=known_values_store,
                functions=funcs,
                parameters=params,
            )
            _global_format_context = ctx
            _initialized = True
    return _global_format_context  # type: ignore[return-value]


def with_format_context(action: Callable[[FormatContext], T]) -> T:
    """Execute *action* with a read reference to the global FormatContext."""
    ctx = _ensure_global_context()
    return action(ctx)


def with_format_context_mut(action: Callable[[FormatContext], T]) -> T:
    """Execute *action* with a mutable reference to the global FormatContext."""
    with _global_lock:
        ctx = _ensure_global_context()
        return action(ctx)


# ---------------------------------------------------------------------------
# Utility (re-exported from _string_utils)
# ---------------------------------------------------------------------------

from ._string_utils import flanked_by  # noqa: F401


# ---------------------------------------------------------------------------
# Tag registration
# ---------------------------------------------------------------------------

def register_tags_in(context: FormatContext) -> None:
    """Register standard envelope tags and summarizers in a FormatContext."""
    from bc_components import register_tags_in as _bc_comp_register_tags_in

    _bc_comp_register_tags_in(context.tags)

    # Known value summarizer
    kv_store = context.known_values

    def _known_value_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        kv = KnownValue.from_untagged_cbor(untagged_cbor)
        name = kv_store.name_for(kv)
        return flanked_by(name, "'", "'")

    context.tags.set_summarizer(TAG_KNOWN_VALUE, _known_value_summarizer)

    # Function summarizer
    funcs_store = context.functions

    def _function_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._function import Function
        from ._functions import FunctionsStore

        f = Function.from_untagged_cbor(untagged_cbor)
        name = FunctionsStore.name_for_function(f, funcs_store)
        return flanked_by(name, "\u00ab", "\u00bb")  # << >>

    context.tags.set_summarizer(TAG_FUNCTION, _function_summarizer)

    # Parameter summarizer
    params_store = context.parameters

    def _parameter_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._parameter import Parameter
        from ._parameters import ParametersStore

        p = Parameter.from_untagged_cbor(untagged_cbor)
        name = ParametersStore.name_for_parameter(p, params_store)
        return flanked_by(name, "\u2770", "\u2771")  # heavy angle brackets

    context.tags.set_summarizer(TAG_PARAMETER, _parameter_summarizer)

    # Request / Response / Event summarizers
    #
    # The inner CBOR of these tags is an ARID (or known-value), not an
    # envelope-encoded value.  Render it via CBOR summary with the
    # context's tags store so that ARID etc. get their own summarizers.
    def _request_summarizer(untagged_cbor: CBOR, flat: bool) -> str:
        text = untagged_cbor.summary(tags_store=context.tags)
        return flanked_by(text, "request(", ")")

    context.tags.set_summarizer(TAG_REQUEST, _request_summarizer)

    def _response_summarizer(untagged_cbor: CBOR, flat: bool) -> str:
        text = untagged_cbor.summary(tags_store=context.tags)
        return flanked_by(text, "response(", ")")

    context.tags.set_summarizer(TAG_RESPONSE, _response_summarizer)

    def _event_summarizer(untagged_cbor: CBOR, flat: bool) -> str:
        text = untagged_cbor.summary(tags_store=context.tags)
        return flanked_by(text, "event(", ")")

    context.tags.set_summarizer(TAG_EVENT, _event_summarizer)


def register_tags() -> None:
    """Register standard envelope tags in the global format context."""
    with_format_context_mut(register_tags_in)
