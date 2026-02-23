package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bctags.TAG_KNOWN_VALUE
import com.blockchaincommons.bctags.TAG_FUNCTION
import com.blockchaincommons.bctags.TAG_PARAMETER
import com.blockchaincommons.bctags.TAG_REQUEST
import com.blockchaincommons.bctags.TAG_RESPONSE
import com.blockchaincommons.bctags.TAG_EVENT
import com.blockchaincommons.dcbor.*
import com.blockchaincommons.knownvalues.KNOWN_VALUES
import com.blockchaincommons.knownvalues.KnownValue
import com.blockchaincommons.knownvalues.KnownValuesStore

/**
 * Option type for selecting which format context to use.
 */
sealed class FormatContextOpt {
    /** No format context. */
    data object None : FormatContextOpt()

    /** Use the global format context. */
    data object Global : FormatContextOpt()

    /** Use a custom format context. */
    data class Custom(val context: FormatContext) : FormatContextOpt()
}

/**
 * Context for formatting Gordian Envelopes with annotations.
 *
 * Provides information about CBOR tags, known values, functions, and parameters
 * that is used to annotate the output of envelope formatting functions.
 */
class FormatContext(
    private val tags: TagsStore = TagsStore(),
    private val knownValues: KnownValuesStore = KnownValuesStore(),
    private val functions: FunctionsStore = FunctionsStore(),
    private val parameters: ParametersStore = ParametersStore(),
) : TagsStoreTrait {

    /** Returns the CBOR tags registry. */
    fun tags(): TagsStore = tags

    /** Returns the known values registry. */
    fun knownValues(): KnownValuesStore = knownValues

    /** Returns the functions registry. */
    fun functions(): FunctionsStore = functions

    /** Returns the parameters registry. */
    fun parameters(): ParametersStore = parameters

    // -- TagsStoreTrait delegation --

    override fun assignedNameForTag(tag: Tag): String? = tags.assignedNameForTag(tag)

    override fun nameForTag(tag: Tag): String = tags.nameForTag(tag)

    override fun tagForName(name: String): Tag? = tags.tagForName(name)

    override fun tagForValue(value: ULong): Tag? = tags.tagForValue(value)

    override fun summarizer(tag: ULong): CborSummarizer? = tags.summarizer(tag)

    override fun nameForValue(value: ULong): String = tags.nameForValue(value)
}

/**
 * Thread-safe global format context singleton.
 *
 * Lazily initialized with standard tags, known values, functions, and parameters.
 */
object GlobalFormatContext {
    private val context: FormatContext by lazy(LazyThreadSafetyMode.SYNCHRONIZED) {
        // Ensure component tags are registered in the global dcbor tag store
        com.blockchaincommons.bccomponents.registerTags()

        // Build our own TagsStore for the FormatContext
        val tags = TagsStore()

        val ctx = FormatContext(
            tags = tags,
            knownValues = KNOWN_VALUES,
            functions = GLOBAL_FUNCTIONS,
            parameters = GLOBAL_PARAMETERS,
        )
        registerTagsIn(ctx)
        ctx
    }

    /** Returns the global format context. */
    fun get(): FormatContext = context
}

/**
 * Accesses the global format context for read-only operations.
 */
fun <T> withFormatContext(action: (FormatContext) -> T): T {
    return action(GlobalFormatContext.get())
}

/**
 * Registers standard tags and summarizers in a format context.
 */
fun registerTagsIn(context: FormatContext) {
    // Register standard component tags into the context's tag store
    com.blockchaincommons.bccomponents.registerTagsIn(context.tags())

    // Known value summarizer
    val knownValues = context.knownValues()
    context.tags().setSummarizer(TAG_KNOWN_VALUE) { untaggedCbor, _ ->
        val kv = KnownValue.fromUntaggedCbor(untaggedCbor)
        knownValues.name(kv).flankedBy("'", "'")
    }

    // Function summarizer
    val functions = context.functions()
    context.tags().setSummarizer(TAG_FUNCTION) { untaggedCbor, _ ->
        val f = Function.fromUntaggedCbor(untaggedCbor)
        FunctionsStore.nameForFunction(f, functions).flankedBy("\u00AB", "\u00BB")
    }

    // Parameter summarizer
    val parameters = context.parameters()
    context.tags().setSummarizer(TAG_PARAMETER) { untaggedCbor, _ ->
        val p = Parameter.fromUntaggedCbor(untaggedCbor)
        ParametersStore.nameForParameter(p, parameters).flankedBy("\u2770", "\u2771")
    }

    // Request summarizer
    context.tags().setSummarizer(TAG_REQUEST) { untaggedCbor, flat ->
        val e = Envelope.newLeaf(untaggedCbor)
        val formatted = e.formatOpt(flat = flat, context = FormatContextOpt.Custom(context))
        formatted.flankedBy("request(", ")")
    }

    // Response summarizer
    context.tags().setSummarizer(TAG_RESPONSE) { untaggedCbor, flat ->
        val e = Envelope.newLeaf(untaggedCbor)
        val formatted = e.formatOpt(flat = flat, context = FormatContextOpt.Custom(context))
        formatted.flankedBy("response(", ")")
    }

    // Event summarizer
    context.tags().setSummarizer(TAG_EVENT) { untaggedCbor, flat ->
        val e = Envelope.newLeaf(untaggedCbor)
        val formatted = e.formatOpt(flat = flat, context = FormatContextOpt.Custom(context))
        formatted.flankedBy("event(", ")")
    }
}

/**
 * Registers standard tags in the global format context.
 */
fun registerTags() {
    // Accessing the global format context triggers lazy initialization
    // which calls registerTagsIn
    GlobalFormatContext.get()
}
