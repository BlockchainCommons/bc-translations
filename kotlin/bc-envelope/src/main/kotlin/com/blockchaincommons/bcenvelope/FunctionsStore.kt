package com.blockchaincommons.bcenvelope

/**
 * A store that maps functions to their assigned names.
 */
class FunctionsStore(
    functions: Iterable<Function> = emptyList(),
) {
    private val dict = mutableMapOf<Function, String>()

    init {
        for (function in functions) insert(function)
    }

    /** Inserts a function into the store. */
    fun insert(function: Function) {
        require(function is Function.Known) { "Only Known functions can be inserted into FunctionsStore" }
        dict[function] = function.name()
    }

    /** Returns the assigned name for a function, if present. */
    fun assignedName(function: Function): String? = dict[function]

    /** Returns the name for a function from this store or the function itself. */
    fun name(function: Function): String =
        assignedName(function) ?: function.name()

    companion object {
        /** Returns the name for a function using an optional store. */
        fun nameForFunction(function: Function, store: FunctionsStore?): String =
            store?.assignedName(function) ?: function.name()
    }
}

// -- Well-known function constants --

val ADD = Function.Known(1uL, "add")
val SUB = Function.Known(2uL, "sub")
val MUL = Function.Known(3uL, "mul")
val DIV = Function.Known(4uL, "div")
val NEG = Function.Known(5uL, "neg")
val LT = Function.Known(6uL, "lt")
val LE = Function.Known(7uL, "le")
val GT = Function.Known(8uL, "gt")
val GE = Function.Known(9uL, "ge")
val EQ = Function.Known(10uL, "eq")
val NE = Function.Known(11uL, "ne")
val AND = Function.Known(12uL, "and")
val OR = Function.Known(13uL, "or")
val XOR = Function.Known(14uL, "xor")
val NOT = Function.Known(15uL, "not")

/** Global functions store, lazily initialized with arithmetic functions. */
val GLOBAL_FUNCTIONS: FunctionsStore by lazy(LazyThreadSafetyMode.SYNCHRONIZED) {
    FunctionsStore(listOf(ADD, SUB, MUL, DIV))
}
