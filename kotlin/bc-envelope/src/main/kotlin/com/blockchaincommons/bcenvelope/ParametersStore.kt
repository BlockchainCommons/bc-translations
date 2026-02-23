package com.blockchaincommons.bcenvelope

/**
 * A store that maps parameters to their assigned names.
 */
class ParametersStore(
    parameters: Iterable<Parameter> = emptyList(),
) {
    private val dict = mutableMapOf<Parameter, String>()

    init {
        for (parameter in parameters) insert(parameter)
    }

    /** Inserts a parameter into the store. */
    fun insert(parameter: Parameter) {
        require(parameter is Parameter.Known) { "Only Known parameters can be inserted into ParametersStore" }
        dict[parameter] = parameter.name()
    }

    /** Returns the assigned name for a parameter, if present. */
    fun assignedName(parameter: Parameter): String? = dict[parameter]

    /** Returns the name for a parameter from this store or the parameter itself. */
    fun name(parameter: Parameter): String =
        assignedName(parameter) ?: parameter.name()

    companion object {
        /** Returns the name for a parameter using an optional store. */
        fun nameForParameter(parameter: Parameter, store: ParametersStore?): String =
            store?.assignedName(parameter) ?: parameter.name()
    }
}

// -- Well-known parameter constants --

val BLANK = Parameter.Known(1uL, "_")
val LHS = Parameter.Known(2uL, "lhs")
val RHS = Parameter.Known(3uL, "rhs")

/** Global parameters store, lazily initialized with standard parameters. */
val GLOBAL_PARAMETERS: ParametersStore by lazy(LazyThreadSafetyMode.SYNCHRONIZED) {
    ParametersStore(listOf(BLANK, LHS, RHS))
}
