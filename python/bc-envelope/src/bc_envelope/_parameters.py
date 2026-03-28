"""Well-known parameter constants and ParametersStore for Gordian Envelope expressions."""

from __future__ import annotations

from ._parameter import Parameter


# ---------------------------------------------------------------------------
# Well-known parameter constants
# ---------------------------------------------------------------------------

BLANK = Parameter.new_known(1, "_")
LHS = Parameter.new_known(2, "lhs")
RHS = Parameter.new_known(3, "rhs")


# ===========================================================================
# ParametersStore
# ===========================================================================

class ParametersStore:
    """A registry mapping known parameters to their display names."""

    __slots__ = ("_dict",)

    def __init__(self, parameters: list[Parameter] | None = None) -> None:
        self._dict: dict[Parameter, str] = {}
        if parameters:
            for p in parameters:
                self._insert(p)

    def insert(self, parameter: Parameter) -> None:
        self._insert(parameter)

    def assigned_name(self, parameter: Parameter) -> str | None:
        return self._dict.get(parameter)

    def name(self, parameter: Parameter) -> str:
        n = self.assigned_name(parameter)
        return n if n is not None else parameter.name

    @staticmethod
    def name_for_parameter(
        parameter: Parameter,
        store: ParametersStore | None = None,
    ) -> str:
        if store is not None:
            n = store.assigned_name(parameter)
            if n is not None:
                return n
        return parameter.name

    def _insert(self, parameter: Parameter) -> None:
        if not parameter.is_known:
            raise ValueError("Only known parameters can be inserted into a ParametersStore")
        self._dict[parameter] = parameter.name


# ---------------------------------------------------------------------------
# Global store (lazily populated)
# ---------------------------------------------------------------------------

_GLOBAL_PARAMETERS: ParametersStore | None = None


def global_parameters() -> ParametersStore:
    global _GLOBAL_PARAMETERS
    if _GLOBAL_PARAMETERS is None:
        _GLOBAL_PARAMETERS = ParametersStore([BLANK, LHS, RHS])
    return _GLOBAL_PARAMETERS
