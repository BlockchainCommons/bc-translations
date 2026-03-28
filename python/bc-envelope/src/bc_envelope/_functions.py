"""Well-known function constants and FunctionsStore for Gordian Envelope expressions."""

from __future__ import annotations

from ._function import Function


# ---------------------------------------------------------------------------
# Well-known function constants
# ---------------------------------------------------------------------------

ADD = Function.new_known(1, "add")
SUB = Function.new_known(2, "sub")
MUL = Function.new_known(3, "mul")
DIV = Function.new_known(4, "div")
NEG = Function.new_known(5, "neg")
LT = Function.new_known(6, "lt")
LE = Function.new_known(7, "le")
GT = Function.new_known(8, "gt")
GE = Function.new_known(9, "ge")
EQ = Function.new_known(10, "eq")
NE = Function.new_known(11, "ne")
AND = Function.new_known(12, "and")
OR = Function.new_known(13, "or")
XOR = Function.new_known(14, "xor")
NOT = Function.new_known(15, "not")


# ===========================================================================
# FunctionsStore
# ===========================================================================

class FunctionsStore:
    """A registry mapping known functions to their display names."""

    __slots__ = ("_dict",)

    def __init__(self, functions: list[Function] | None = None) -> None:
        self._dict: dict[Function, str] = {}
        if functions:
            for f in functions:
                self._insert(f)

    def insert(self, function: Function) -> None:
        self._insert(function)

    def assigned_name(self, function: Function) -> str | None:
        return self._dict.get(function)

    def name(self, function: Function) -> str:
        n = self.assigned_name(function)
        return n if n is not None else function.name

    @staticmethod
    def name_for_function(
        function: Function,
        store: FunctionsStore | None = None,
    ) -> str:
        if store is not None:
            n = store.assigned_name(function)
            if n is not None:
                return n
        return function.name

    def _insert(self, function: Function) -> None:
        if not function.is_known:
            raise ValueError("Only known functions can be inserted into a FunctionsStore")
        self._dict[function] = function.name


# ---------------------------------------------------------------------------
# Global store (lazily populated)
# ---------------------------------------------------------------------------

_GLOBAL_FUNCTIONS: FunctionsStore | None = None


def global_functions() -> FunctionsStore:
    global _GLOBAL_FUNCTIONS
    if _GLOBAL_FUNCTIONS is None:
        _GLOBAL_FUNCTIONS = FunctionsStore([ADD, SUB, MUL, DIV])
    return _GLOBAL_FUNCTIONS
