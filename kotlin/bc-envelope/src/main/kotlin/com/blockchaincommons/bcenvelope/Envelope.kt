package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bctags.TAG_COMPRESSED
import com.blockchaincommons.bctags.TAG_ENCODED_CBOR
import com.blockchaincommons.bctags.TAG_ENCRYPTED
import com.blockchaincommons.bctags.TAG_ENVELOPE
import com.blockchaincommons.bctags.TAG_LEAF
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.UREncodable
import com.blockchaincommons.bcur.URType
import com.blockchaincommons.dcbor.*
import com.blockchaincommons.knownvalues.KnownValue
import com.blockchaincommons.knownvalues.KNOWN_VALUES
import com.blockchaincommons.knownvalues.UNIT
import com.blockchaincommons.knownvalues.POSITION
import com.blockchaincommons.knownvalues.UNKNOWN_VALUE
import com.blockchaincommons.knownvalues.OK_VALUE

/**
 * A flexible container for structured data with built-in integrity verification.
 *
 * Gordian Envelope is a hierarchical binary data format built on deterministic
 * CBOR (dCBOR) with a Merkle-like digest tree. Envelopes are immutable --
 * operations that appear to modify an envelope actually create a new envelope.
 */
class Envelope private constructor(
    private val envelopeCase: EnvelopeCase,
) : EnvelopeEncodable, DigestProvider, CborTaggedEncodable, UREncodable {

    /** Returns the underlying envelope case. */
    fun case(): EnvelopeCase = envelopeCase

    // ---- EnvelopeEncodable ----

    override fun toEnvelope(): Envelope = this

    // ---- DigestProvider ----

    override fun digest(): Digest = when (val c = envelopeCase) {
        is EnvelopeCase.Node -> c.digest
        is EnvelopeCase.Leaf -> c.digest
        is EnvelopeCase.Wrapped -> c.digest
        is EnvelopeCase.AssertionCase -> c.assertion.digest()
        is EnvelopeCase.Elided -> c.digest
        is EnvelopeCase.KnownValueCase -> c.digest
        is EnvelopeCase.Encrypted -> c.encryptedMessage.digest()
        is EnvelopeCase.CompressedCase -> c.compressed.digest()
    }

    // ---- Construction ----

    companion object {
        /** Creates an envelope from any [EnvelopeEncodable] subject. */
        fun from(subject: Any): Envelope = subject.asEnvelopeEncodable().toEnvelope()

        /** Creates an envelope or null sentinel from an optional subject. */
        fun fromOrNull(subject: Any?): Envelope =
            if (subject != null) from(subject) else null_()

        /** Creates an envelope from an optional subject, or returns null. */
        fun fromOrNone(subject: Any?): Envelope? =
            if (subject != null) from(subject) else null

        /** Creates an assertion envelope. */
        fun newAssertion(predicate: Any, objectValue: Any): Envelope =
            newWithAssertion(
                Assertion(
                    predicate.asEnvelopeEncodable(),
                    objectValue.asEnvelopeEncodable(),
                )
            )

        /** Creates a leaf envelope from a CBOR value. */
        fun newLeaf(cbor: Cbor): Envelope {
            val digest = Digest.fromImage(cbor.toCborData())
            return Envelope(EnvelopeCase.Leaf(cbor, digest))
        }

        /** Creates a wrapped envelope. */
        internal fun newWrapped(envelope: Envelope): Envelope {
            val digest = Digest.fromDigests(listOf(envelope.digest()))
            return Envelope(EnvelopeCase.Wrapped(envelope, digest))
        }

        /** Creates an elided envelope from a digest. */
        internal fun newElided(digest: Digest): Envelope =
            Envelope(EnvelopeCase.Elided(digest))

        /** Creates an assertion case envelope. */
        internal fun newWithAssertion(assertion: Assertion): Envelope =
            Envelope(EnvelopeCase.AssertionCase(assertion))

        /** Creates a known-value envelope. */
        internal fun newWithKnownValue(value: KnownValue): Envelope {
            val digest = value.digest()
            return Envelope(EnvelopeCase.KnownValueCase(value, digest))
        }

        /** Creates an encrypted envelope. */
        internal fun newWithEncrypted(encryptedMessage: EncryptedMessage): Envelope {
            if (!encryptedMessage.hasDigest()) {
                throw EnvelopeException.MissingDigest()
            }
            return Envelope(EnvelopeCase.Encrypted(encryptedMessage))
        }

        /** Creates a compressed envelope. */
        internal fun newWithCompressed(compressed: Compressed): Envelope {
            if (!compressed.hasDigest) {
                throw EnvelopeException.MissingDigest()
            }
            return Envelope(EnvelopeCase.CompressedCase(compressed))
        }

        /** Creates a node envelope with assertions (unchecked). */
        internal fun newWithUncheckedAssertions(
            subject: Envelope,
            uncheckedAssertions: List<Envelope>,
        ): Envelope {
            require(uncheckedAssertions.isNotEmpty())
            val sortedAssertions = uncheckedAssertions.sortedBy { it.digest() }
            val digests = mutableListOf(subject.digest())
            digests.addAll(sortedAssertions.map { it.digest() })
            val digest = Digest.fromDigests(digests)
            return Envelope(EnvelopeCase.Node(subject, sortedAssertions, digest))
        }

        /** Creates a node envelope with validated assertions. */
        internal fun newWithAssertions(
            subject: Envelope,
            assertions: List<Envelope>,
        ): Envelope {
            if (!assertions.all { it.isSubjectAssertion() || it.isSubjectObscured() }) {
                throw EnvelopeException.InvalidFormat()
            }
            return newWithUncheckedAssertions(subject, assertions)
        }

        // -- Static factories --

        /** Creates a null envelope. */
        fun null_(): Envelope = newLeaf(Cbor.`null`())

        /** Creates a true envelope. */
        fun true_(): Envelope = newLeaf(Cbor.`true`())

        /** Creates a false envelope. */
        fun false_(): Envelope = newLeaf(Cbor.`false`())

        /** Creates a unit envelope (known value ''). */
        fun unit(): Envelope = UNIT.toEnvelope()

        /** Creates an 'Unknown' known-value envelope. */
        fun unknown(): Envelope = UNKNOWN_VALUE.toEnvelope()

        /** Creates an 'OK' known-value envelope. */
        fun ok(): Envelope = OK_VALUE.toEnvelope()

        // ---- CBOR Decoding ----

        /** Decodes an envelope from untagged CBOR. */
        fun fromUntaggedCbor(cbor: Cbor): Envelope {
            return when {
                cbor.isTagged() -> {
                    val (tag, item) = cbor.tryTagged()
                    when (tag.value) {
                        TAG_LEAF, TAG_ENCODED_CBOR -> newLeaf(item)
                        TAG_ENVELOPE -> {
                            val envelope = fromTaggedCbor(cbor)
                            newWrapped(envelope)
                        }
                        TAG_ENCRYPTED -> {
                            val encrypted = EncryptedMessage.fromUntaggedCbor(item)
                            newWithEncrypted(encrypted)
                        }
                        TAG_COMPRESSED -> {
                            val compressed = Compressed.fromUntaggedCbor(item)
                            newWithCompressed(compressed)
                        }
                        else -> throw CborException.Custom("unknown envelope tag: ${tag.value}")
                    }
                }
                cbor.isByteString() -> {
                    val bytes = cbor.tryByteStringData()
                    newElided(Digest.fromDataChecked(bytes))
                }
                cbor.isArray() -> {
                    val elements = cbor.tryArray()
                    if (elements.size < 2) {
                        throw CborException.Custom("node must have at least two elements")
                    }
                    val subject = fromUntaggedCbor(elements[0])
                    val assertions = elements.drop(1).map { fromUntaggedCbor(it) }
                    newWithAssertions(subject, assertions)
                }
                cbor.isMap() -> {
                    val assertion = Assertion.fromCbor(cbor)
                    newWithAssertion(assertion)
                }
                cbor.cborCase is CborCase.Unsigned -> {
                    val value = cbor.tryULong()
                    newWithKnownValue(KnownValue(value))
                }
                else -> throw CborException.Custom("invalid envelope")
            }
        }

        /** Decodes an envelope from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Envelope =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_ENVELOPE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an envelope from tagged CBOR data bytes. */
        fun fromTaggedCborData(data: ByteArray): Envelope =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_ENVELOPE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an envelope from a UR. */
        fun fromUr(ur: UR): Envelope {
            val expectedTags = tagsForValues(listOf(TAG_ENVELOPE))
            val expectedType = expectedTags.first().name
                ?: throw IllegalStateException("Envelope tag must have a name")
            require(ur.urType.value == expectedType) {
                "Expected UR type '$expectedType', got '${ur.urType.value}'"
            }
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an envelope from a UR string. */
        fun fromUrString(urString: String): Envelope =
            fromUr(UR.fromUrString(urString))
    }

    // ---- CborTagged ----

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_ENVELOPE))

    override fun untaggedCbor(): Cbor = when (val c = envelopeCase) {
        is EnvelopeCase.Node -> {
            val result = mutableListOf(c.subject.untaggedCbor())
            result.addAll(c.assertions.map { it.untaggedCbor() })
            Cbor.fromArray(result)
        }
        is EnvelopeCase.Leaf -> Cbor.tagged(Tag(TAG_LEAF), c.cbor)
        is EnvelopeCase.Wrapped -> c.envelope.taggedCbor()
        is EnvelopeCase.AssertionCase -> c.assertion.toCbor()
        is EnvelopeCase.Elided -> c.digest.untaggedCbor()
        is EnvelopeCase.KnownValueCase -> c.value.untaggedCbor()
        is EnvelopeCase.Encrypted -> c.encryptedMessage.taggedCbor()
        is EnvelopeCase.CompressedCase -> c.compressed.taggedCbor()
    }

    // ---- Subject / Assertions ----

    /** Returns the envelope's subject. For a non-node, returns itself. */
    fun subject(): Envelope = when (val c = envelopeCase) {
        is EnvelopeCase.Node -> c.subject
        else -> this
    }

    /** Returns the envelope's assertions. */
    fun assertions(): List<Envelope> = when (val c = envelopeCase) {
        is EnvelopeCase.Node -> c.assertions
        else -> emptyList()
    }

    /** True if the envelope has at least one assertion. */
    fun hasAssertions(): Boolean = when (val c = envelopeCase) {
        is EnvelopeCase.Node -> c.assertions.isNotEmpty()
        else -> false
    }

    // ---- Type Queries ----

    /** True if the envelope is an assertion. */
    fun isAssertion(): Boolean = envelopeCase is EnvelopeCase.AssertionCase

    /** True if the envelope is encrypted. */
    fun isEncrypted(): Boolean = envelopeCase is EnvelopeCase.Encrypted

    /** True if the envelope is compressed. */
    fun isCompressed(): Boolean = envelopeCase is EnvelopeCase.CompressedCase

    /** True if the envelope is elided. */
    fun isElided(): Boolean = envelopeCase is EnvelopeCase.Elided

    /** True if the envelope is a leaf. */
    fun isLeaf(): Boolean = envelopeCase is EnvelopeCase.Leaf

    /** True if the envelope is a node (has assertions). */
    fun isNode(): Boolean = envelopeCase is EnvelopeCase.Node

    /** True if the envelope is wrapped. */
    fun isWrapped(): Boolean = envelopeCase is EnvelopeCase.Wrapped

    /** True if the envelope is a known value. */
    fun isKnownValue(): Boolean = envelopeCase is EnvelopeCase.KnownValueCase

    // ---- Subject Type Queries ----

    /** True if the subject is an assertion. */
    fun isSubjectAssertion(): Boolean = when (val c = envelopeCase) {
        is EnvelopeCase.AssertionCase -> true
        is EnvelopeCase.Node -> c.subject.isSubjectAssertion()
        else -> false
    }

    /** True if the subject is encrypted. */
    fun isSubjectEncrypted(): Boolean = when (val c = envelopeCase) {
        is EnvelopeCase.Encrypted -> true
        is EnvelopeCase.Node -> c.subject.isSubjectEncrypted()
        else -> false
    }

    /** True if the subject is compressed. */
    fun isSubjectCompressed(): Boolean = when (val c = envelopeCase) {
        is EnvelopeCase.CompressedCase -> true
        is EnvelopeCase.Node -> c.subject.isSubjectCompressed()
        else -> false
    }

    /** True if the subject is elided. */
    fun isSubjectElided(): Boolean = when (val c = envelopeCase) {
        is EnvelopeCase.Elided -> true
        is EnvelopeCase.Node -> c.subject.isSubjectElided()
        else -> false
    }

    /** True if the subject is obscured (elided, encrypted, or compressed). */
    fun isSubjectObscured(): Boolean =
        isSubjectElided() || isSubjectEncrypted() || isSubjectCompressed()

    /** True if the envelope is internal (has child elements). */
    fun isInternal(): Boolean = when (envelopeCase) {
        is EnvelopeCase.Node, is EnvelopeCase.Wrapped, is EnvelopeCase.AssertionCase -> true
        else -> false
    }

    /** True if the envelope is obscured (elided, encrypted, or compressed). */
    fun isObscured(): Boolean = isElided() || isEncrypted() || isCompressed()

    // ---- Assertion Access ----

    /** Returns the envelope as assertion, or null if not an assertion. */
    fun asAssertion(): Envelope? =
        if (envelopeCase is EnvelopeCase.AssertionCase) this else null

    /** Returns the envelope as assertion, or throws. */
    fun tryAssertion(): Envelope =
        asAssertion() ?: throw EnvelopeException.NotAssertion()

    /** Returns the predicate if the subject is an assertion, or null. */
    fun asPredicate(): Envelope? {
        val sub = subject().case()
        return if (sub is EnvelopeCase.AssertionCase) sub.assertion.predicate() else null
    }

    /** Returns the predicate if the subject is an assertion, or throws. */
    fun tryPredicate(): Envelope =
        asPredicate() ?: throw EnvelopeException.NotAssertion()

    /** Returns the object if the subject is an assertion, or null. */
    fun asObject(): Envelope? {
        val sub = subject().case()
        return if (sub is EnvelopeCase.AssertionCase) sub.assertion.objectEnvelope() else null
    }

    /** Returns the object if the subject is an assertion, or throws. */
    fun tryObject(): Envelope =
        asObject() ?: throw EnvelopeException.NotAssertion()

    /** Returns the leaf CBOR, or null. */
    fun asLeaf(): Cbor? = when (val c = envelopeCase) {
        is EnvelopeCase.Leaf -> c.cbor
        else -> null
    }

    /** Returns the leaf CBOR, or throws. */
    fun tryLeaf(): Cbor = asLeaf() ?: throw EnvelopeException.NotLeaf()

    /** Returns the known value, or null. */
    fun asKnownValue(): KnownValue? = when (val c = envelopeCase) {
        is EnvelopeCase.KnownValueCase -> c.value
        else -> null
    }

    /** Returns the known value, or throws. */
    fun tryKnownValue(): KnownValue =
        asKnownValue() ?: throw EnvelopeException.NotKnownValue()

    // ---- Leaf Helpers ----

    /** True if the envelope is a null leaf. */
    fun isNull(): Boolean = asLeaf()?.isNull() == true

    /** True if the envelope is a true leaf. */
    fun isTrue(): Boolean = asLeaf()?.isTrue() == true

    /** True if the envelope is a false leaf. */
    fun isFalse(): Boolean = asLeaf()?.isFalse() == true

    /** True if the envelope is a boolean leaf. */
    fun isBool(): Boolean = isTrue() || isFalse()

    /** True if the envelope is a number leaf. */
    fun isNumber(): Boolean = asLeaf()?.isNumber() == true

    /** True if the subject is a number. */
    fun isSubjectNumber(): Boolean = subject().isNumber()

    /** True if the envelope is NaN. */
    fun isNaN(): Boolean = asLeaf()?.isNaN() == true

    /** True if the subject is NaN. */
    fun isSubjectNaN(): Boolean = subject().isNaN()

    /** Returns the leaf as a byte string, or throws. */
    fun tryByteString(): ByteArray = tryLeaf().tryByteStringData()

    /** Returns the leaf as a byte string, or null. */
    fun asByteString(): ByteArray? = asLeaf()?.let {
        try { it.tryByteStringData() } catch (_: Exception) { null }
    }

    /** Returns the leaf as a text string, or null. */
    fun asText(): String? = asLeaf()?.let {
        try { it.tryText() } catch (_: Exception) { null }
    }

    /** Returns the leaf as an array, or null. */
    fun asArray(): List<Cbor>? = asLeaf()?.let {
        try { it.tryArray() } catch (_: Exception) { null }
    }

    /** Returns the leaf as a map, or null. */
    fun asMap(): CborMap? = asLeaf()?.let {
        try { it.tryMap() } catch (_: Exception) { null }
    }

    /** True if the subject is the unit value. */
    fun isSubjectUnit(): Boolean {
        val kv = subject().asKnownValue()
        return kv != null && kv == UNIT
    }

    /** Checks that the subject is the unit value, throws if not. */
    fun checkSubjectUnit(): Envelope {
        if (!isSubjectUnit()) throw EnvelopeException.SubjectNotUnit()
        return this
    }

    // ---- Subject Extraction ----

    /**
     * Extracts the subject as a CBOR-decodable type.
     *
     * @throws EnvelopeException.InvalidFormat if the type does not match.
     */
    inline fun <reified T : Any> extractSubject(): T {
        // Traverse through Node wrappers to find the actual subject
        var current = this
        while (current.case() is EnvelopeCase.Node) {
            current = (current.case() as EnvelopeCase.Node).subject
        }
        return when (val c = current.case()) {
            is EnvelopeCase.Node -> throw EnvelopeException.InvalidFormat() // should not reach here
            is EnvelopeCase.Leaf -> {
                try {
                    Cbor.to<T>(c.cbor)
                } catch (_: Exception) {
                    // Try tagged CBOR decoding for bc-components types
                    @Suppress("UNCHECKED_CAST")
                    try {
                        when (T::class) {
                            Digest::class -> Digest.fromTaggedCbor(c.cbor) as T
                            Salt::class -> Salt.fromTaggedCbor(c.cbor) as T
                            Nonce::class -> Nonce.fromTaggedCbor(c.cbor) as T
                            ARID::class -> ARID.fromTaggedCbor(c.cbor) as T
                            URI::class -> URI.fromTaggedCbor(c.cbor) as T
                            UUID::class -> UUID.fromTaggedCbor(c.cbor) as T
                            XID::class -> XID.fromTaggedCbor(c.cbor) as T
                            Reference::class -> Reference.fromTaggedCbor(c.cbor) as T
                            PublicKeys::class -> PublicKeys.fromTaggedCbor(c.cbor) as T
                            PrivateKeys::class -> PrivateKeys.fromTaggedCbor(c.cbor) as T
                            PrivateKeyBase::class -> PrivateKeyBase.fromTaggedCbor(c.cbor) as T
                            SealedMessage::class -> SealedMessage.fromTaggedCbor(c.cbor) as T
                            EncryptedKey::class -> EncryptedKey.fromTaggedCbor(c.cbor) as T
                            Signature::class -> Signature.fromTaggedCbor(c.cbor) as T
                            SSKRShare::class -> SSKRShare.fromTaggedCbor(c.cbor) as T
                            SymmetricKey::class -> SymmetricKey.fromTaggedCbor(c.cbor) as T
                            CborDate::class -> CborDate.fromCbor(c.cbor) as T
                            else -> throw EnvelopeException.InvalidFormat()
                        }
                    } catch (_: Exception) {
                        throw EnvelopeException.InvalidFormat()
                    }
                }
            }
            is EnvelopeCase.Wrapped -> {
                if (T::class == Envelope::class) {
                    @Suppress("UNCHECKED_CAST")
                    c.envelope as T
                } else {
                    throw EnvelopeException.InvalidFormat()
                }
            }
            is EnvelopeCase.AssertionCase -> {
                if (T::class == Assertion::class) {
                    @Suppress("UNCHECKED_CAST")
                    c.assertion as T
                } else {
                    throw EnvelopeException.InvalidFormat()
                }
            }
            is EnvelopeCase.Elided -> {
                if (T::class == Digest::class) {
                    @Suppress("UNCHECKED_CAST")
                    c.digest as T
                } else {
                    throw EnvelopeException.InvalidFormat()
                }
            }
            is EnvelopeCase.KnownValueCase -> {
                if (T::class == KnownValue::class) {
                    @Suppress("UNCHECKED_CAST")
                    c.value as T
                } else {
                    throw EnvelopeException.InvalidFormat()
                }
            }
            is EnvelopeCase.Encrypted -> {
                if (T::class == EncryptedMessage::class) {
                    @Suppress("UNCHECKED_CAST")
                    c.encryptedMessage as T
                } else {
                    throw EnvelopeException.InvalidFormat()
                }
            }
            is EnvelopeCase.CompressedCase -> {
                if (T::class == Compressed::class) {
                    @Suppress("UNCHECKED_CAST")
                    c.compressed as T
                } else {
                    throw EnvelopeException.InvalidFormat()
                }
            }
        }
    }

    /** Extracts the object of this assertion as a typed value. */
    inline fun <reified T : Any> extractObject(): T = tryObject().extractSubject()

    /** Extracts the predicate of this assertion as a typed value. */
    inline fun <reified T : Any> extractPredicate(): T = tryPredicate().extractSubject()

    // ---- Assertion Queries ----

    /** Returns all assertions with the given predicate (match by digest). */
    fun assertionsWithPredicate(predicate: Any): List<Envelope> {
        val predicateEnvelope = Envelope.from(predicate)
        return assertions().filter { assertion ->
            assertion.subject().asPredicate()?.let {
                it.digest() == predicateEnvelope.digest()
            } ?: false
        }
    }

    /** Returns the single assertion with the given predicate. */
    fun assertionWithPredicate(predicate: Any): Envelope {
        val a = assertionsWithPredicate(predicate)
        return when {
            a.isEmpty() -> throw EnvelopeException.NonexistentPredicate()
            a.size == 1 -> a[0]
            else -> throw EnvelopeException.AmbiguousPredicate()
        }
    }

    /** Returns the optional assertion with the given predicate. */
    fun optionalAssertionWithPredicate(predicate: Any): Envelope? {
        val a = assertionsWithPredicate(predicate)
        return when {
            a.isEmpty() -> null
            a.size == 1 -> a[0]
            else -> throw EnvelopeException.AmbiguousPredicate()
        }
    }

    /** Returns the object of the assertion with the given predicate. */
    fun objectForPredicate(predicate: Any): Envelope =
        assertionWithPredicate(predicate).asObject()!!

    /** Returns optional object of assertion with the given predicate. */
    fun optionalObjectForPredicate(predicate: Any): Envelope? {
        val a = assertionsWithPredicate(predicate)
        return when {
            a.isEmpty() -> null
            a.size == 1 -> a[0].subject().asObject()!!
            else -> throw EnvelopeException.AmbiguousPredicate()
        }
    }

    /** Returns objects of all assertions with the matching predicate. */
    fun objectsForPredicate(predicate: Any): List<Envelope> =
        assertionsWithPredicate(predicate).map { it.asObject()!! }

    /** Extracts the object for the predicate as a typed value. */
    inline fun <reified T : Any> extractObjectForPredicate(predicate: Any): T =
        assertionWithPredicate(predicate).extractObject()

    /** Extracts optional object for predicate as typed value. */
    inline fun <reified T : Any> extractOptionalObjectForPredicate(predicate: Any): T? {
        val obj = optionalObjectForPredicate(predicate) ?: return null
        return obj.extractSubject()
    }

    /** Extracts object for predicate, or returns default. */
    inline fun <reified T : Any> extractObjectForPredicateWithDefault(predicate: Any, default: T): T =
        extractOptionalObjectForPredicate(predicate) ?: default

    /** Extracts all objects for predicate as typed values. */
    inline fun <reified T : Any> extractObjectsForPredicate(predicate: Any): List<T> =
        objectsForPredicate(predicate).map { it.extractSubject() }

    /** Returns the number of structural elements in the envelope. */
    fun elementsCount(): Int {
        var result = 1
        when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                result += c.subject.elementsCount()
                for (a in c.assertions) result += a.elementsCount()
            }
            is EnvelopeCase.AssertionCase -> {
                result += c.assertion.predicate().elementsCount()
                result += c.assertion.objectEnvelope().elementsCount()
            }
            is EnvelopeCase.Wrapped -> {
                result += c.envelope.elementsCount()
            }
            else -> {}
        }
        return result
    }

    // ---- Assertion Management ----

    /** Adds an assertion with the given predicate and object. */
    fun addAssertion(predicate: Any, objectValue: Any): Envelope {
        val assertion = newAssertion(predicate, objectValue)
        return addOptionalAssertionEnvelope(assertion)
    }

    /** Adds a pre-constructed assertion envelope. */
    fun addAssertionEnvelope(assertionEnvelope: Envelope): Envelope =
        addOptionalAssertionEnvelope(assertionEnvelope)

    /** Adds multiple assertion envelopes. */
    fun addAssertionEnvelopes(assertions: List<Envelope>): Envelope {
        var e = this
        for (a in assertions) {
            e = e.addAssertionEnvelope(a)
        }
        return e
    }

    /** Adds an optional assertion envelope. Deduplicates by digest. */
    fun addOptionalAssertionEnvelope(assertion: Envelope?): Envelope {
        if (assertion == null) return this
        if (!assertion.isSubjectAssertion() && !assertion.isSubjectObscured()) {
            throw EnvelopeException.InvalidFormat()
        }
        return when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                if (c.assertions.any { it.digest() == assertion.digest() }) {
                    this
                } else {
                    val newAssertions = c.assertions + assertion
                    newWithUncheckedAssertions(c.subject, newAssertions)
                }
            }
            else -> newWithUncheckedAssertions(subject(), listOf(assertion))
        }
    }

    /** Adds an assertion with an optional object. If null, returns unchanged. */
    fun addOptionalAssertion(predicate: Any, objectValue: Any?): Envelope {
        if (objectValue == null) return this
        return addAssertionEnvelope(newAssertion(predicate, objectValue))
    }

    /** Adds an assertion only if the string is non-empty. */
    fun addNonemptyStringAssertion(predicate: Any, str: String): Envelope {
        if (str.isEmpty()) return this
        return addAssertion(predicate, str)
    }

    /** Adds multiple assertion envelopes (ignoring errors). */
    fun addAssertions(envelopes: List<Envelope>): Envelope {
        var e = this
        for (envelope in envelopes) {
            e = e.addAssertionEnvelope(envelope)
        }
        return e
    }

    /** Adds assertion only if condition is true. */
    fun addAssertionIf(condition: Boolean, predicate: Any, objectValue: Any): Envelope =
        if (condition) addAssertion(predicate, objectValue) else this

    /** Adds assertion envelope only if condition is true. */
    fun addAssertionEnvelopeIf(condition: Boolean, assertionEnvelope: Envelope): Envelope =
        if (condition) addAssertionEnvelope(assertionEnvelope) else this

    /** Removes an assertion matching the target's digest. */
    fun removeAssertion(target: Envelope): Envelope {
        val currentAssertions = assertions()
        val targetDigest = target.digest()
        val index = currentAssertions.indexOfFirst { it.digest() == targetDigest }
        if (index < 0) return this
        val newAssertions = currentAssertions.toMutableList()
        newAssertions.removeAt(index)
        return if (newAssertions.isEmpty()) {
            subject()
        } else {
            newWithUncheckedAssertions(subject(), newAssertions)
        }
    }

    /** Replaces an assertion with a new one. */
    fun replaceAssertion(old: Envelope, new: Envelope): Envelope =
        removeAssertion(old).addAssertionEnvelope(new)

    /** Replaces the subject, keeping all assertions. */
    fun replaceSubject(newSubject: Envelope): Envelope =
        assertions().fold(newSubject) { e, a -> e.addAssertionEnvelope(a) }

    // ---- Salted Assertions ----

    /** Adds an optionally salted assertion. */
    fun addAssertionSalted(predicate: Any, objectValue: Any, salted: Boolean): Envelope {
        val assertion = newAssertion(predicate, objectValue)
        return addOptionalAssertionEnvelopeSalted(assertion, salted)
    }

    /** Adds an optionally salted assertion envelope. */
    fun addAssertionEnvelopeSalted(assertionEnvelope: Envelope, salted: Boolean): Envelope =
        addOptionalAssertionEnvelopeSalted(assertionEnvelope, salted)

    /** Adds optional assertion envelope with salting. */
    fun addOptionalAssertionEnvelopeSalted(assertion: Envelope?, salted: Boolean): Envelope {
        if (assertion == null) return this
        if (!assertion.isSubjectAssertion() && !assertion.isSubjectObscured()) {
            throw EnvelopeException.InvalidFormat()
        }
        val envelope2 = if (salted) assertion.addSalt() else assertion
        return when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                if (c.assertions.any { it.digest() == envelope2.digest() }) {
                    this
                } else {
                    newWithUncheckedAssertions(c.subject, c.assertions + envelope2)
                }
            }
            else -> newWithUncheckedAssertions(subject(), listOf(envelope2))
        }
    }

    /** Adds multiple optionally salted assertions. */
    fun addAssertionsSalted(assertions: List<Envelope>, salted: Boolean): Envelope {
        var e = this
        for (a in assertions) {
            e = e.addAssertionEnvelopeSalted(a, salted)
        }
        return e
    }

    // ---- Wrapping ----

    /** Returns a new envelope wrapping this one. */
    fun wrap(): Envelope = newWrapped(this)

    /** Unwraps and returns the inner envelope. */
    fun unwrap(): Envelope {
        val sub = subject().case()
        return if (sub is EnvelopeCase.Wrapped) sub.envelope
        else throw EnvelopeException.NotWrapped()
    }

    // ---- Elision ----

    /** Returns the elided variant of this envelope. */
    fun elide(): Envelope =
        if (envelopeCase is EnvelopeCase.Elided) this else newElided(digest())

    /** Elides elements in the target set. */
    fun elideRemovingSet(target: Set<Digest>): Envelope =
        elideSetWithAction(target, false, ObscureAction.Elide)

    /** Reveals only elements in the target set, eliding others. */
    fun elideRevealingSet(target: Set<Digest>): Envelope =
        elideSetWithAction(target, true, ObscureAction.Elide)

    /** Elides a single target. */
    fun elideRemovingTarget(target: DigestProvider): Envelope =
        elideRemovingSet(setOf(target.digest()))

    /** Reveals only a single target, eliding others. */
    fun elideRevealingTarget(target: DigestProvider): Envelope =
        elideRevealingSet(setOf(target.digest()))

    /** Elides elements with a specific action (elide, encrypt, compress). */
    fun elideRemovingSetWithAction(target: Set<Digest>, action: ObscureAction): Envelope =
        elideSetWithAction(target, false, action)

    /** Reveals with action. */
    fun elideRevealingSetWithAction(target: Set<Digest>, action: ObscureAction): Envelope =
        elideSetWithAction(target, true, action)

    /** Elides a single target with action. */
    fun elideRemovingTargetWithAction(target: DigestProvider, action: ObscureAction): Envelope =
        elideRemovingSetWithAction(setOf(target.digest()), action)

    /** Reveals a single target with action. */
    fun elideRevealingTargetWithAction(target: DigestProvider, action: ObscureAction): Envelope =
        elideRevealingSetWithAction(setOf(target.digest()), action)

    /** Elides elements from an array of DigestProviders. */
    fun elideRemovingArray(target: List<DigestProvider>): Envelope =
        elideRemovingSet(target.map { it.digest() }.toSet())

    /** Reveals only elements from an array of DigestProviders. */
    fun elideRevealingArray(target: List<DigestProvider>): Envelope =
        elideRevealingSet(target.map { it.digest() }.toSet())

    /** Elides elements from an array of DigestProviders with action. */
    fun elideRemovingArrayWithAction(target: List<DigestProvider>, action: ObscureAction): Envelope =
        elideRemovingSetWithAction(target.map { it.digest() }.toSet(), action)

    /** Reveals only elements from an array of DigestProviders with action. */
    fun elideRevealingArrayWithAction(target: List<DigestProvider>, action: ObscureAction): Envelope =
        elideRevealingSetWithAction(target.map { it.digest() }.toSet(), action)

    /** Core elision implementation. */
    fun elideSetWithAction(
        target: Set<Digest>,
        isRevealing: Boolean,
        action: ObscureAction,
    ): Envelope {
        val selfDigest = digest()
        if (target.contains(selfDigest) != isRevealing) {
            return when (action) {
                is ObscureAction.Elide -> elide()
                is ObscureAction.Encrypt -> {
                    val message = action.key.encryptWithDigest(
                        taggedCbor().toCborData(),
                        selfDigest,
                        null,
                    )
                    newWithEncrypted(message)
                }
                is ObscureAction.Compress -> compress()
            }
        }
        return when (val c = envelopeCase) {
            is EnvelopeCase.AssertionCase -> {
                val predicate = c.assertion.predicate().elideSetWithAction(target, isRevealing, action)
                val objectEnv = c.assertion.objectEnvelope().elideSetWithAction(target, isRevealing, action)
                newWithAssertion(Assertion(predicate, objectEnv))
            }
            is EnvelopeCase.Node -> {
                val elidedSubject = c.subject.elideSetWithAction(target, isRevealing, action)
                val elidedAssertions = c.assertions.map {
                    it.elideSetWithAction(target, isRevealing, action)
                }
                newWithUncheckedAssertions(elidedSubject, elidedAssertions)
            }
            is EnvelopeCase.Wrapped -> {
                val elidedEnvelope = c.envelope.elideSetWithAction(target, isRevealing, action)
                newWrapped(elidedEnvelope)
            }
            else -> this
        }
    }

    /** Unelides this envelope given the original. */
    fun unelide(original: Envelope): Envelope {
        if (digest() == original.digest()) return original
        throw EnvelopeException.InvalidDigest()
    }

    /** Restores elided nodes from the provided envelopes. */
    fun walkUnelide(envelopes: List<Envelope>): Envelope {
        val map = envelopes.associateBy { it.digest() }
        return walkUnelideWithMap(map)
    }

    private fun walkUnelideWithMap(map: Map<Digest, Envelope>): Envelope {
        return when (val c = envelopeCase) {
            is EnvelopeCase.Elided -> map[digest()] ?: this
            is EnvelopeCase.Node -> {
                val newSubject = c.subject.walkUnelideWithMap(map)
                val newAssertions = c.assertions.map { it.walkUnelideWithMap(map) }
                if (newSubject.isIdenticalTo(c.subject) &&
                    newAssertions.zip(c.assertions).all { (a, b) -> a.isIdenticalTo(b) }
                ) this
                else newWithUncheckedAssertions(newSubject, newAssertions)
            }
            is EnvelopeCase.Wrapped -> {
                val newEnv = c.envelope.walkUnelideWithMap(map)
                if (newEnv.isIdenticalTo(c.envelope)) this else newEnv.wrap()
            }
            is EnvelopeCase.AssertionCase -> {
                val newPred = c.assertion.predicate().walkUnelideWithMap(map)
                val newObj = c.assertion.objectEnvelope().walkUnelideWithMap(map)
                if (newPred.isIdenticalTo(c.assertion.predicate()) &&
                    newObj.isIdenticalTo(c.assertion.objectEnvelope())
                ) this
                else newAssertion(newPred, newObj)
            }
            else -> this
        }
    }

    /** Replaces nodes with matching digests. */
    fun walkReplace(target: Set<Digest>, replacement: Envelope): Envelope {
        if (target.contains(digest())) return replacement
        return when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                val newSubject = c.subject.walkReplace(target, replacement)
                val newAssertions = c.assertions.map { it.walkReplace(target, replacement) }
                if (newSubject.isIdenticalTo(c.subject) &&
                    newAssertions.zip(c.assertions).all { (a, b) -> a.isIdenticalTo(b) }
                ) this
                else newWithAssertions(newSubject, newAssertions)
            }
            is EnvelopeCase.Wrapped -> {
                val newEnv = c.envelope.walkReplace(target, replacement)
                if (newEnv.isIdenticalTo(c.envelope)) this else newEnv.wrap()
            }
            is EnvelopeCase.AssertionCase -> {
                val newPred = c.assertion.predicate().walkReplace(target, replacement)
                val newObj = c.assertion.objectEnvelope().walkReplace(target, replacement)
                if (newPred.isIdenticalTo(c.assertion.predicate()) &&
                    newObj.isIdenticalTo(c.assertion.objectEnvelope())
                ) this
                else newAssertion(newPred, newObj)
            }
            else -> this
        }
    }

    /** Recursively decrypts encrypted nodes using the provided keys. */
    fun walkDecrypt(keys: List<SymmetricKey>): Envelope = when (val c = envelopeCase) {
        is EnvelopeCase.Encrypted -> {
            var result: Envelope = this
            for (key in keys) {
                try {
                    result = decryptSubject(key).walkDecrypt(keys)
                    break
                } catch (_: Exception) { /* try next key */ }
            }
            result
        }
        is EnvelopeCase.Node -> {
            val newSubject = c.subject.walkDecrypt(keys)
            val newAssertions = c.assertions.map { it.walkDecrypt(keys) }
            if (newSubject.isIdenticalTo(c.subject) &&
                newAssertions.zip(c.assertions).all { (a, b) -> a.isIdenticalTo(b) }
            ) this
            else newWithUncheckedAssertions(newSubject, newAssertions)
        }
        is EnvelopeCase.Wrapped -> {
            val newEnv = c.envelope.walkDecrypt(keys)
            if (newEnv.isIdenticalTo(c.envelope)) this else newEnv.wrap()
        }
        is EnvelopeCase.AssertionCase -> {
            val newPred = c.assertion.predicate().walkDecrypt(keys)
            val newObj = c.assertion.objectEnvelope().walkDecrypt(keys)
            if (newPred.isIdenticalTo(c.assertion.predicate()) &&
                newObj.isIdenticalTo(c.assertion.objectEnvelope())
            ) this
            else newAssertion(newPred, newObj)
        }
        else -> this
    }

    /** Recursively decompresses compressed nodes. */
    fun walkDecompress(targetDigests: Set<Digest>? = null): Envelope = when (val c = envelopeCase) {
        is EnvelopeCase.CompressedCase -> {
            val matches = targetDigests?.contains(digest()) ?: true
            if (matches) {
                try { decompress().walkDecompress(targetDigests) }
                catch (_: Exception) { this }
            } else this
        }
        is EnvelopeCase.Node -> {
            val newSubject = c.subject.walkDecompress(targetDigests)
            val newAssertions = c.assertions.map { it.walkDecompress(targetDigests) }
            if (newSubject.isIdenticalTo(c.subject) &&
                newAssertions.zip(c.assertions).all { (a, b) -> a.isIdenticalTo(b) }
            ) this
            else newWithUncheckedAssertions(newSubject, newAssertions)
        }
        is EnvelopeCase.Wrapped -> {
            val newEnv = c.envelope.walkDecompress(targetDigests)
            if (newEnv.isIdenticalTo(c.envelope)) this else newEnv.wrap()
        }
        is EnvelopeCase.AssertionCase -> {
            val newPred = c.assertion.predicate().walkDecompress(targetDigests)
            val newObj = c.assertion.objectEnvelope().walkDecompress(targetDigests)
            if (newPred.isIdenticalTo(c.assertion.predicate()) &&
                newObj.isIdenticalTo(c.assertion.objectEnvelope())
            ) this
            else newAssertion(newPred, newObj)
        }
        else -> this
    }

    /** Returns matching node digests for obscured nodes. */
    fun nodesMatching(
        targetDigests: Set<Digest>? = null,
        obscureTypes: List<ObscureType> = emptyList(),
    ): Set<Digest> {
        val result = mutableSetOf<Digest>()
        walk(false, Unit) { envelope, _, _, state ->
            val digestMatches = targetDigests?.contains(envelope.digest()) ?: true
            if (digestMatches) {
                if (obscureTypes.isEmpty()) {
                    result.add(envelope.digest())
                } else {
                    val typeMatches = obscureTypes.any { type ->
                        when (type) {
                            ObscureType.Elided -> envelope.isElided()
                            ObscureType.Encrypted -> envelope.isEncrypted()
                            ObscureType.Compressed -> envelope.isCompressed()
                        }
                    }
                    if (typeMatches) result.add(envelope.digest())
                }
            }
            Pair(state, false)
        }
        return result
    }

    // ---- Digest Operations ----

    /** Returns digests down to the specified level limit. */
    fun digests(levelLimit: Int): Set<Digest> {
        val result = mutableSetOf<Digest>()
        walk(false, Unit) { envelope, level, _, state ->
            if (level < levelLimit) {
                result.add(envelope.digest())
                result.add(envelope.subject().digest())
            }
            Pair(state, false)
        }
        return result
    }

    /** Returns all digests at all levels. */
    fun deepDigests(): Set<Digest> = digests(Int.MAX_VALUE)

    /** Returns digests down to second level only. */
    fun shallowDigests(): Set<Digest> = digests(2)

    /** Returns a structural digest. */
    fun structuralDigest(): Digest {
        val image = mutableListOf<Byte>()
        walk(false, Unit) { envelope, _, _, state ->
            when (envelope.case()) {
                is EnvelopeCase.Elided -> image.add(1)
                is EnvelopeCase.Encrypted -> image.add(0)
                is EnvelopeCase.CompressedCase -> image.add(2)
                else -> {}
            }
            image.addAll(envelope.digest().data().toList())
            Pair(state, false)
        }
        return Digest.fromImage(image.toByteArray())
    }

    /** Tests if semantically equivalent (same digest). */
    fun isEquivalentTo(other: Envelope): Boolean = digest() == other.digest()

    /** Tests if structurally identical. */
    fun isIdenticalTo(other: Envelope): Boolean {
        if (!isEquivalentTo(other)) return false
        return structuralDigest() == other.structuralDigest()
    }

    // ---- Walk / Visitor ----

    /** Walks the envelope structure. */
    fun <S> walk(hideNodes: Boolean, state: S, visit: (Envelope, Int, EdgeType, S) -> Pair<S, Boolean>) {
        if (hideNodes) walkTree(0, EdgeType.None, state, visit)
        else walkStructure(0, EdgeType.None, state, visit)
    }

    private fun <S> walkStructure(
        level: Int,
        incomingEdge: EdgeType,
        state: S,
        visit: (Envelope, Int, EdgeType, S) -> Pair<S, Boolean>,
    ) {
        val (nextState, stop) = visit(this, level, incomingEdge, state)
        if (stop) return
        val nextLevel = level + 1
        when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                c.subject.walkStructure(nextLevel, EdgeType.Subject, nextState, visit)
                for (a in c.assertions) {
                    a.walkStructure(nextLevel, EdgeType.Assertion, nextState, visit)
                }
            }
            is EnvelopeCase.Wrapped -> {
                c.envelope.walkStructure(nextLevel, EdgeType.Content, nextState, visit)
            }
            is EnvelopeCase.AssertionCase -> {
                c.assertion.predicate().walkStructure(nextLevel, EdgeType.Predicate, nextState, visit)
                c.assertion.objectEnvelope().walkStructure(nextLevel, EdgeType.Object, nextState, visit)
            }
            else -> {}
        }
    }

    private fun <S> walkTree(
        level: Int,
        incomingEdge: EdgeType,
        state: S,
        visit: (Envelope, Int, EdgeType, S) -> Pair<S, Boolean>,
    ): S {
        var currentState = state
        var subjectLevel = level
        if (!isNode()) {
            val (nextState, stop) = visit(this, level, incomingEdge, currentState)
            if (stop) return nextState
            currentState = nextState
            subjectLevel = level + 1
        }
        when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                val assertionState = c.subject.walkTree(subjectLevel, EdgeType.Subject, currentState, visit)
                val assertionLevel = subjectLevel + 1
                for (a in c.assertions) {
                    a.walkTree(assertionLevel, EdgeType.Assertion, assertionState, visit)
                }
            }
            is EnvelopeCase.Wrapped -> {
                c.envelope.walkTree(subjectLevel, EdgeType.Content, currentState, visit)
            }
            is EnvelopeCase.AssertionCase -> {
                c.assertion.predicate().walkTree(subjectLevel, EdgeType.Predicate, currentState, visit)
                c.assertion.objectEnvelope().walkTree(subjectLevel, EdgeType.Object, currentState, visit)
            }
            else -> {}
        }
        return currentState
    }

    // ---- Encryption ----

    /** Encrypts the subject with the given key. */
    fun encryptSubject(key: SymmetricKey, nonce: Nonce? = null): Envelope {
        return when (val c = envelopeCase) {
            is EnvelopeCase.Node -> {
                if (c.subject.isEncrypted()) throw EnvelopeException.AlreadyEncrypted()
                val encodedCbor = c.subject.taggedCbor().toCborData()
                val digest = c.subject.digest()
                val message = key.encryptWithDigest(encodedCbor, digest, nonce)
                val encryptedSubject = newWithEncrypted(message)
                newWithUncheckedAssertions(encryptedSubject, c.assertions)
            }
            is EnvelopeCase.Leaf -> {
                val encodedCbor = taggedCbor().toCborData()
                val digest = c.digest
                val message = key.encryptWithDigest(encodedCbor, digest, nonce)
                newWithEncrypted(message)
            }
            is EnvelopeCase.Wrapped -> {
                val encodedCbor = taggedCbor().toCborData()
                val digest = c.digest
                val message = key.encryptWithDigest(encodedCbor, digest, nonce)
                newWithEncrypted(message)
            }
            is EnvelopeCase.KnownValueCase -> {
                val encodedCbor = taggedCbor().toCborData()
                val digest = c.digest
                val message = key.encryptWithDigest(encodedCbor, digest, nonce)
                newWithEncrypted(message)
            }
            is EnvelopeCase.AssertionCase -> {
                val digest = c.assertion.digest()
                val encodedCbor = taggedCbor().toCborData()
                val message = key.encryptWithDigest(encodedCbor, digest, nonce)
                newWithEncrypted(message)
            }
            is EnvelopeCase.Encrypted -> throw EnvelopeException.AlreadyEncrypted()
            is EnvelopeCase.CompressedCase -> {
                val digest = c.compressed.digest()
                val encodedCbor = taggedCbor().toCborData()
                val message = key.encryptWithDigest(encodedCbor, digest, nonce)
                newWithEncrypted(message)
            }
            is EnvelopeCase.Elided -> throw EnvelopeException.AlreadyElided()
        }
    }

    /** Decrypts the subject with the given key. */
    fun decryptSubject(key: SymmetricKey): Envelope {
        val sub = subject()
        if (sub.case() !is EnvelopeCase.Encrypted) throw EnvelopeException.NotEncrypted()
        val encrypted = (sub.case() as EnvelopeCase.Encrypted).encryptedMessage
        val decrypted = key.decrypt(encrypted)
        val envelope = fromTaggedCbor(Cbor.tryFromData(decrypted))
        if (envelope.digest() != sub.digest()) throw EnvelopeException.InvalidDigest()
        val a = assertions()
        return if (a.isEmpty()) envelope
        else newWithUncheckedAssertions(envelope, a)
    }

    /** Convenience: wrap + encrypt subject. */
    fun encrypt(key: SymmetricKey, nonce: Nonce? = null): Envelope =
        wrap().encryptSubject(key, nonce)

    /** Convenience: decrypt subject + unwrap. */
    fun decrypt(key: SymmetricKey): Envelope = decryptSubject(key).unwrap()

    // ---- Compression ----

    /** Returns the compressed variant of this envelope. */
    fun compress(): Envelope = when (envelopeCase) {
        is EnvelopeCase.CompressedCase -> this
        is EnvelopeCase.Encrypted -> throw EnvelopeException.AlreadyEncrypted()
        is EnvelopeCase.Elided -> throw EnvelopeException.AlreadyElided()
        else -> {
            val data = taggedCbor().toCborData()
            val compressed = Compressed.fromDecompressedData(data, digest())
            newWithCompressed(compressed)
        }
    }

    /** Returns the decompressed variant. */
    fun decompress(): Envelope {
        if (case() !is EnvelopeCase.CompressedCase) throw EnvelopeException.NotCompressed()
        val compressed = (case() as EnvelopeCase.CompressedCase).compressed
        val data = compressed.decompress()
        return fromTaggedCbor(Cbor.tryFromData(data))
    }

    /** Compresses just the subject. */
    fun compressSubject(): Envelope {
        val sub = subject()
        if (sub.isElided()) throw EnvelopeException.AlreadyElided()
        if (sub.isCompressed()) throw EnvelopeException.AlreadyCompressed()
        val data = sub.taggedCbor().toCborData()
        val compressed = Compressed.fromDecompressedData(data, sub.digest())
        val compressedEnv = newWithCompressed(compressed)
        val a = assertions()
        return if (a.isEmpty()) compressedEnv
        else newWithUncheckedAssertions(compressedEnv, a)
    }

    /** Decompresses just the subject. */
    fun decompressSubject(): Envelope {
        val sub = subject()
        if (sub.case() !is EnvelopeCase.CompressedCase) throw EnvelopeException.NotCompressed()
        val compressed = (sub.case() as EnvelopeCase.CompressedCase).compressed
        val data = compressed.decompress()
        val decompressedEnv = fromTaggedCbor(Cbor.tryFromData(data))
        if (decompressedEnv.digest() != sub.digest()) throw EnvelopeException.InvalidDigest()
        val a = assertions()
        return if (a.isEmpty()) decompressedEnv
        else newWithUncheckedAssertions(decompressedEnv, a)
    }

    // ---- Salt ----

    /** Adds salt to decorrelate digests (proportionate to envelope size). */
    fun addSalt(): Envelope {
        val size = taggedCbor().toCborData().size
        val salt = Salt.createForSize(size)
        return addAssertion(
            com.blockchaincommons.knownvalues.SALT,
            salt.toEnvelope(),
        )
    }

    /** Adds a specific salt instance. */
    fun addSaltInstance(salt: Salt): Envelope = addAssertion(
        com.blockchaincommons.knownvalues.SALT,
        salt.toEnvelope(),
    )

    /** Adds salt using a specific RNG for deterministic testing. */
    fun addSaltUsing(rng: RandomNumberGenerator): Envelope {
        val size = taggedCbor().toCborData().size
        val salt = Salt.createForSizeUsing(size, rng)
        return addAssertion(
            com.blockchaincommons.knownvalues.SALT,
            salt.toEnvelope(),
        )
    }

    /** Adds salt with a specific byte count. */
    fun addSaltWithLength(count: Int): Envelope {
        val salt = Salt.createWithLength(count)
        return addAssertion(
            com.blockchaincommons.knownvalues.SALT,
            salt.toEnvelope(),
        )
    }

    /** Adds salt with a random length in the given range. */
    fun addSaltInRange(range: IntRange): Envelope {
        val salt = Salt.createInRange(range)
        return addAssertion(
            com.blockchaincommons.knownvalues.SALT,
            salt.toEnvelope(),
        )
    }

    // ---- Position ----

    /** Sets the position assertion. */
    fun setPosition(position: Int): Envelope {
        val posAssertions = assertionsWithPredicate(POSITION)
        if (posAssertions.size > 1) throw EnvelopeException.InvalidFormat()
        val base = if (posAssertions.isNotEmpty()) {
            removeAssertion(posAssertions[0])
        } else this
        return base.addAssertion(POSITION, position)
    }

    /** Gets the position value. */
    fun position(): Int = extractObjectForPredicate(POSITION)

    /** Removes the position assertion. */
    fun removePosition(): Envelope {
        val posAssertions = assertionsWithPredicate(POSITION)
        if (posAssertions.size > 1) throw EnvelopeException.InvalidFormat()
        return if (posAssertions.isNotEmpty()) removeAssertion(posAssertions[0])
        else this
    }

    // ---- Equality ----

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Envelope) return false
        return isIdenticalTo(other)
    }

    override fun hashCode(): Int = digest().hashCode()

    override fun toString(): String = format()
}
