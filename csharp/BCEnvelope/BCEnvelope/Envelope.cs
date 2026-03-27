using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A flexible container for structured data with built-in integrity verification.
/// </summary>
/// <remarks>
/// <para>
/// Gordian Envelope is the primary data structure of this library. It provides a
/// way to encapsulate and organize data with cryptographic integrity, privacy
/// features, and selective disclosure capabilities.
/// </para>
/// <para>
/// Key characteristics:
/// <list type="bullet">
/// <item><b>Immutability</b>: Envelopes are immutable. Operations that appear to
/// modify an envelope actually create a new envelope.</item>
/// <item><b>Semantic Structure</b>: Envelopes can represent subject-predicate-object
/// relationships (similar to RDF triples).</item>
/// <item><b>Digest Tree</b>: Each envelope maintains a Merkle-like digest tree for
/// content integrity verification.</item>
/// <item><b>Privacy Features</b>: Envelopes support elision, encryption, and
/// compression while maintaining structural integrity.</item>
/// <item><b>Deterministic Representation</b>: Uses deterministic CBOR encoding for
/// consistent serialization across platforms.</item>
/// </list>
/// </para>
/// </remarks>
public sealed partial class Envelope : IDigestProvider
{
    private readonly EnvelopeCase _case;

    private Envelope(EnvelopeCase envelopeCase)
    {
        _case = envelopeCase;
    }

    /// <summary>
    /// Returns the underlying envelope case variant.
    /// </summary>
    public EnvelopeCase Case => _case;

    // ===== Public Constructors =====

    /// <summary>
    /// Creates an envelope from any value that can be converted to an envelope.
    /// </summary>
    /// <param name="subject">The subject value.</param>
    /// <returns>A new envelope.</returns>
    public static Envelope Create(object subject)
    {
        return EnvelopeExtensions.ToEnvelope(subject);
    }

    /// <summary>
    /// Creates an assertion envelope with a predicate and object.
    /// </summary>
    /// <param name="predicate">The predicate value.</param>
    /// <param name="object">The object value.</param>
    /// <returns>A new assertion envelope.</returns>
    public static Envelope CreateAssertion(object predicate, object @object)
    {
        var pred = EnvelopeExtensions.ToEnvelope(predicate);
        var obj = EnvelopeExtensions.ToEnvelope(@object);
        return CreateWithAssertion(new Assertion(pred, obj));
    }

    /// <summary>
    /// Creates an envelope from an optional value. Returns null envelope if the value is null.
    /// </summary>
    public static Envelope CreateOrNull(object? subject)
    {
        return subject is null ? Null() : Create(subject);
    }

    /// <summary>
    /// Creates an envelope from an optional value. Returns null if the value is null.
    /// </summary>
    public static Envelope? CreateOrNone(object? subject)
    {
        return subject is null ? null : Create(subject);
    }

    // ===== Internal Constructors =====

    internal static Envelope CreateFromCase(EnvelopeCase envelopeCase)
    {
        return new Envelope(envelopeCase);
    }

    internal static Envelope CreateWithUncheckedAssertions(Envelope subject, List<Envelope> uncheckedAssertions)
    {
        if (uncheckedAssertions.Count == 0)
            throw new ArgumentException("assertions must not be empty", nameof(uncheckedAssertions));

        var sortedAssertions = new List<Envelope>(uncheckedAssertions);
        sortedAssertions.Sort((a, b) => a.GetDigest().CompareTo(b.GetDigest()));

        var digests = new List<Digest> { subject.GetDigest() };
        foreach (var a in sortedAssertions)
            digests.Add(a.GetDigest());

        var digest = Digest.FromDigests(digests.ToArray());
        return new Envelope(new EnvelopeCase.NodeCase(subject, sortedAssertions, digest));
    }

    internal static Envelope CreateWithAssertions(Envelope subject, List<Envelope> assertions)
    {
        foreach (var a in assertions)
        {
            if (!a.IsSubjectAssertion && !a.IsSubjectObscured)
                throw EnvelopeException.InvalidFormat();
        }
        return CreateWithUncheckedAssertions(subject, assertions);
    }

    internal static Envelope CreateWithAssertion(Assertion assertion)
    {
        return new Envelope(new EnvelopeCase.AssertionCase(assertion));
    }

    internal static Envelope CreateWithKnownValue(KnownValue value)
    {
        var digest = value.GetDigest();
        return new Envelope(new EnvelopeCase.KnownValueCase(value, digest));
    }

    internal static Envelope CreateWithEncrypted(EncryptedMessage encryptedMessage)
    {
        if (!encryptedMessage.HasDigest)
            throw EnvelopeException.MissingDigest();
        return new Envelope(new EnvelopeCase.EncryptedCase(encryptedMessage));
    }

    internal static Envelope CreateWithCompressed(Compressed compressed)
    {
        if (!compressed.HasDigest)
            throw EnvelopeException.MissingDigest();
        return new Envelope(new EnvelopeCase.CompressedCase(compressed));
    }

    internal static Envelope CreateElided(Digest digest)
    {
        return new Envelope(new EnvelopeCase.ElidedCase(digest));
    }

    internal static Envelope CreateLeaf(Cbor cbor)
    {
        var digest = Digest.FromImage(cbor.ToCborData());
        return new Envelope(new EnvelopeCase.LeafCase(cbor, digest));
    }

    internal static Envelope CreateWrapped(Envelope envelope)
    {
        var digest = Digest.FromDigests([envelope.GetDigest()]);
        return new Envelope(new EnvelopeCase.WrappedCase(envelope, digest));
    }

    // ===== Well-Known Envelopes =====

    /// <summary>Creates a null envelope.</summary>
    public static Envelope Null() => CreateLeaf(Cbor.Null());

    /// <summary>Creates a true envelope.</summary>
    public static Envelope True() => CreateLeaf(Cbor.True());

    /// <summary>Creates a false envelope.</summary>
    public static Envelope False() => CreateLeaf(Cbor.False());

    /// <summary>Creates a unit envelope (known value '').</summary>
    public static Envelope Unit() => Create(KnownValuesRegistry.Unit);

    // ===== Subject, Assertions, Structural Queries =====

    /// <summary>
    /// Gets the envelope's subject. For an envelope with no assertions, returns
    /// the same envelope.
    /// </summary>
    public Envelope Subject => _case switch
    {
        EnvelopeCase.NodeCase n => n.Subject,
        _ => this,
    };

    /// <summary>
    /// Gets the envelope's assertions.
    /// </summary>
    public IReadOnlyList<Envelope> Assertions => _case switch
    {
        EnvelopeCase.NodeCase n => n.Assertions,
        _ => Array.Empty<Envelope>(),
    };

    /// <summary>
    /// Returns true if the envelope has at least one assertion.
    /// </summary>
    public bool HasAssertions => _case is EnvelopeCase.NodeCase n && n.Assertions.Count > 0;

    /// <summary>
    /// If the envelope is an assertion, returns this envelope; otherwise null.
    /// </summary>
    public Envelope? AsAssertion() => _case is EnvelopeCase.AssertionCase ? this : null;

    /// <summary>
    /// If the envelope is an assertion, returns this envelope; otherwise throws.
    /// </summary>
    public Envelope TryAssertion() =>
        AsAssertion() ?? throw EnvelopeException.NotAssertion();

    /// <summary>
    /// Gets the envelope's predicate, or null if not an assertion.
    /// </summary>
    public Envelope? AsPredicate()
    {
        return Subject.Case switch
        {
            EnvelopeCase.AssertionCase a => a.Assertion.Predicate,
            _ => null,
        };
    }

    /// <summary>
    /// Gets the envelope's predicate, or throws if not an assertion.
    /// </summary>
    public Envelope TryPredicate() =>
        AsPredicate() ?? throw EnvelopeException.NotAssertion();

    /// <summary>
    /// Gets the envelope's object, or null if not an assertion.
    /// </summary>
    public Envelope? AsObject()
    {
        return Subject.Case switch
        {
            EnvelopeCase.AssertionCase a => a.Assertion.Object,
            _ => null,
        };
    }

    /// <summary>
    /// Gets the envelope's object, or throws if not an assertion.
    /// </summary>
    public Envelope TryObject() =>
        AsObject() ?? throw EnvelopeException.NotAssertion();

    /// <summary>
    /// Gets the envelope's leaf CBOR value, or null if not a leaf.
    /// </summary>
    public Cbor? AsLeaf() => _case is EnvelopeCase.LeafCase l ? l.Cbor : null;

    /// <summary>
    /// Gets the envelope's leaf CBOR value, or throws if not a leaf.
    /// </summary>
    public Cbor TryLeaf() =>
        AsLeaf() ?? throw EnvelopeException.NotLeaf();

    // ===== Type Queries =====

    /// <summary>True if the envelope is an assertion.</summary>
    public bool IsAssertion => _case is EnvelopeCase.AssertionCase;

    /// <summary>True if the envelope is encrypted.</summary>
    public bool IsEncrypted => _case is EnvelopeCase.EncryptedCase;

    /// <summary>True if the envelope is compressed.</summary>
    public bool IsCompressed => _case is EnvelopeCase.CompressedCase;

    /// <summary>True if the envelope is elided.</summary>
    public bool IsElided => _case is EnvelopeCase.ElidedCase;

    /// <summary>True if the envelope is a leaf.</summary>
    public bool IsLeaf => _case is EnvelopeCase.LeafCase;

    /// <summary>True if the envelope is a node (subject with assertions).</summary>
    public bool IsNode => _case is EnvelopeCase.NodeCase;

    /// <summary>True if the envelope is wrapped.</summary>
    public bool IsWrapped => _case is EnvelopeCase.WrappedCase;

    /// <summary>True if the envelope is a known value.</summary>
    public bool IsKnownValue => _case is EnvelopeCase.KnownValueCase;

    // ===== Subject Type Queries =====

    /// <summary>True if the subject of the envelope is an assertion.</summary>
    public bool IsSubjectAssertion => _case switch
    {
        EnvelopeCase.AssertionCase => true,
        EnvelopeCase.NodeCase n => n.Subject.IsSubjectAssertion,
        _ => false,
    };

    /// <summary>True if the subject of the envelope has been encrypted.</summary>
    public bool IsSubjectEncrypted => _case switch
    {
        EnvelopeCase.EncryptedCase => true,
        EnvelopeCase.NodeCase n => n.Subject.IsSubjectEncrypted,
        _ => false,
    };

    /// <summary>True if the subject of the envelope has been compressed.</summary>
    public bool IsSubjectCompressed => _case switch
    {
        EnvelopeCase.CompressedCase => true,
        EnvelopeCase.NodeCase n => n.Subject.IsSubjectCompressed,
        _ => false,
    };

    /// <summary>True if the subject of the envelope has been elided.</summary>
    public bool IsSubjectElided => _case switch
    {
        EnvelopeCase.ElidedCase => true,
        EnvelopeCase.NodeCase n => n.Subject.IsSubjectElided,
        _ => false,
    };

    /// <summary>
    /// True if the subject is obscured (encrypted, elided, or compressed).
    /// </summary>
    public bool IsSubjectObscured =>
        IsSubjectElided || IsSubjectEncrypted || IsSubjectCompressed;

    /// <summary>
    /// True if the envelope is internal (has child elements: node, wrapped, or assertion).
    /// </summary>
    public bool IsInternal => _case is EnvelopeCase.NodeCase
        or EnvelopeCase.WrappedCase
        or EnvelopeCase.AssertionCase;

    /// <summary>
    /// True if the envelope is obscured (encrypted, elided, or compressed).
    /// </summary>
    public bool IsObscured => IsElided || IsEncrypted || IsCompressed;

    // ===== Leaf Helpers =====

    /// <summary>True if the envelope is a null leaf.</summary>
    public bool IsNull
    {
        get
        {
            var cbor = AsLeaf();
            return cbor is not null && cbor.IsNull;
        }
    }

    /// <summary>True if the envelope is a true leaf.</summary>
    public bool IsTrue
    {
        get
        {
            var cbor = AsLeaf();
            return cbor is not null && cbor.IsTrue;
        }
    }

    /// <summary>True if the envelope is a false leaf.</summary>
    public bool IsFalse
    {
        get
        {
            var cbor = AsLeaf();
            return cbor is not null && cbor.IsFalse;
        }
    }

    /// <summary>True if the envelope is a boolean leaf.</summary>
    public bool IsBool
    {
        get
        {
            var cbor = AsLeaf();
            return cbor is not null && cbor.IsBool;
        }
    }

    /// <summary>True if the envelope is a leaf containing a number.</summary>
    public bool IsNumber
    {
        get
        {
            var cbor = AsLeaf();
            return cbor is not null && cbor.IsNumber;
        }
    }

    /// <summary>True if the subject of the envelope is a number.</summary>
    public bool IsSubjectNumber => Subject.IsNumber;

    /// <summary>True if the envelope is a leaf containing NaN.</summary>
    public bool IsNan
    {
        get
        {
            var cbor = AsLeaf();
            return cbor is not null && cbor.IsNan;
        }
    }

    /// <summary>True if the subject of the envelope is NaN.</summary>
    public bool IsSubjectNan => Subject.IsNan;

    /// <summary>
    /// True if the subject of the envelope is the unit known value.
    /// </summary>
    public bool IsSubjectUnit
    {
        get
        {
            var kv = Subject.AsKnownValue();
            return kv is not null && kv == KnownValuesRegistry.Unit;
        }
    }

    /// <summary>
    /// Checks that the subject is the unit value; throws otherwise.
    /// </summary>
    public Envelope CheckSubjectUnit()
    {
        if (!IsSubjectUnit)
            throw EnvelopeException.SubjectNotUnit();
        return this;
    }

    // ===== Known Value Helpers =====

    /// <summary>
    /// Gets the envelope's known value, or null if not a known value case.
    /// </summary>
    public KnownValue? AsKnownValue() =>
        _case is EnvelopeCase.KnownValueCase kv ? kv.Value : null;

    /// <summary>
    /// Gets the envelope's known value, or throws if not a known value case.
    /// </summary>
    public KnownValue TryKnownValue() =>
        AsKnownValue() ?? throw EnvelopeException.NotKnownValue();

    // ===== Byte/Array/Map/Text Access =====

    /// <summary>
    /// Gets the leaf content as a byte array, or throws.
    /// </summary>
    public byte[] TryByteString()
    {
        return TryLeaf().TryIntoByteString();
    }

    /// <summary>
    /// Gets the leaf content as a byte array, or null.
    /// </summary>
    public byte[]? AsByteString()
    {
        return AsLeaf()?.AsByteStringData();
    }

    /// <summary>
    /// Gets the leaf content as a CBOR array, or null.
    /// </summary>
    public IReadOnlyList<Cbor>? AsArray()
    {
        return AsLeaf()?.AsArray();
    }

    /// <summary>
    /// Gets the leaf content as a CBOR map, or null.
    /// </summary>
    public CborMap? AsMap()
    {
        return AsLeaf()?.AsMap();
    }

    /// <summary>
    /// Gets the leaf content as text, or null.
    /// </summary>
    public string? AsText()
    {
        return AsLeaf()?.AsText();
    }

    // ===== Content Extraction =====

    /// <summary>
    /// Extracts the subject value as the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>The extracted value.</returns>
    /// <exception cref="EnvelopeException">Thrown if the subject cannot be converted to the requested type.</exception>
    public T ExtractSubject<T>()
    {
        return _case switch
        {
            EnvelopeCase.WrappedCase w => ExtractType<T, Envelope>(w.Envelope),
            EnvelopeCase.NodeCase n => n.Subject.ExtractSubject<T>(),
            EnvelopeCase.LeafCase l => ExtractFromCbor<T>(l.Cbor),
            EnvelopeCase.AssertionCase a => ExtractType<T, Assertion>(a.Assertion),
            EnvelopeCase.ElidedCase e => ExtractType<T, Digest>(e.Digest),
            EnvelopeCase.KnownValueCase kv => ExtractType<T, KnownValue>(kv.Value),
            EnvelopeCase.EncryptedCase enc => ExtractType<T, EncryptedMessage>(enc.EncryptedMessage),
            EnvelopeCase.CompressedCase comp => ExtractType<T, Compressed>(comp.Compressed),
            _ => throw EnvelopeException.InvalidFormat(),
        };
    }

    private static T ExtractType<T, U>(U value)
    {
        if (value is T result)
            return result;
        throw EnvelopeException.InvalidFormat();
    }

    private static T ExtractFromCbor<T>(Cbor cbor)
    {
        var targetType = typeof(T);

        if (targetType == typeof(string))
            return (T)(object)cbor.TryIntoText();
        if (targetType == typeof(bool))
            return (T)(object)cbor.TryIntoBool();
        if (targetType == typeof(byte))
            return (T)(object)(byte)cbor.TryIntoUInt64();
        if (targetType == typeof(ushort))
            return (T)(object)(ushort)cbor.TryIntoUInt64();
        if (targetType == typeof(uint))
            return (T)(object)(uint)cbor.TryIntoUInt64();
        if (targetType == typeof(ulong))
            return (T)(object)cbor.TryIntoUInt64();
        if (targetType == typeof(sbyte))
            return (T)(object)(sbyte)cbor.TryIntoInt64();
        if (targetType == typeof(short))
            return (T)(object)(short)cbor.TryIntoInt64();
        if (targetType == typeof(int))
            return (T)(object)cbor.TryIntoInt32();
        if (targetType == typeof(long))
            return (T)(object)cbor.TryIntoInt64();
        if (targetType == typeof(float))
            return (T)(object)(float)cbor.TryIntoDouble();
        if (targetType == typeof(double))
            return (T)(object)cbor.TryIntoDouble();
        if (targetType == typeof(ByteString))
        {
            var data = cbor.TryIntoByteString();
            return (T)(object)new ByteString(data);
        }
        if (targetType == typeof(CborDate))
            return (T)(object)CborDate.FromTaggedCbor(cbor);
        if (targetType == typeof(Cbor))
            return (T)(object)cbor;
        if (targetType == typeof(Digest))
            return (T)(object)Digest.FromTaggedCbor(cbor);
        if (targetType == typeof(Salt))
            return (T)(object)Salt.FromTaggedCbor(cbor);
        if (targetType == typeof(ARID))
            return (T)(object)ARID.FromTaggedCbor(cbor);
        if (targetType == typeof(BCComponents.URI))
            return (T)(object)BCComponents.URI.FromTaggedCbor(cbor);
        if (targetType == typeof(BCComponents.UUID))
            return (T)(object)BCComponents.UUID.FromTaggedCbor(cbor);
        if (targetType == typeof(XID))
            return (T)(object)XID.FromTaggedCbor(cbor);
        if (targetType == typeof(Reference))
            return (T)(object)Reference.FromTaggedCbor(cbor);
        if (targetType == typeof(PublicKeys))
            return (T)(object)PublicKeys.FromTaggedCbor(cbor);
        if (targetType == typeof(PrivateKeys))
            return (T)(object)PrivateKeys.FromTaggedCbor(cbor);
        if (targetType == typeof(PrivateKeyBase))
            return (T)(object)PrivateKeyBase.FromTaggedCbor(cbor);
        if (targetType == typeof(SealedMessage))
            return (T)(object)SealedMessage.FromTaggedCbor(cbor);
        if (targetType == typeof(EncryptedMessage))
            return (T)(object)EncryptedMessage.FromTaggedCbor(cbor);
        if (targetType == typeof(EncryptedKey))
            return (T)(object)EncryptedKey.FromTaggedCbor(cbor);
        if (targetType == typeof(Signature))
            return (T)(object)Signature.FromTaggedCbor(cbor);
        if (targetType == typeof(SSKRShare))
            return (T)(object)SSKRShare.FromTaggedCbor(cbor);
        if (targetType == typeof(Compressed))
            return (T)(object)Compressed.FromTaggedCbor(cbor);
        if (targetType == typeof(KnownValue))
            return (T)(object)KnownValue.FromTaggedCbor(cbor);
        if (targetType == typeof(Function))
            return (T)(object)Function.FromTaggedCbor(cbor);
        if (targetType == typeof(Parameter))
            return (T)(object)Parameter.FromTaggedCbor(cbor);
        if (targetType == typeof(Simple))
        {
            if (cbor.Case is CborCase.SimpleCase s)
                return (T)(object)s.Value;
            throw new CborWrongTypeException();
        }

        throw EnvelopeException.InvalidFormat();
    }

    // ===== Assertion Query Methods =====

    /// <summary>
    /// Returns all assertions with the given predicate, matched by digest.
    /// </summary>
    public List<Envelope> AssertionsWithPredicate(object predicate)
    {
        var predicateEnvelope = EnvelopeExtensions.ToEnvelope(predicate);
        var predicateDigest = predicateEnvelope.GetDigest();
        var result = new List<Envelope>();
        foreach (var assertion in Assertions)
        {
            var pred = assertion.Subject.AsPredicate();
            if (pred is not null && pred.GetDigest() == predicateDigest)
                result.Add(assertion);
        }
        return result;
    }

    /// <summary>
    /// Returns the single assertion with the given predicate.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if zero or more than one assertion matches.</exception>
    public Envelope AssertionWithPredicate(object predicate)
    {
        var a = AssertionsWithPredicate(predicate);
        if (a.Count == 0)
            throw EnvelopeException.NonexistentPredicate();
        if (a.Count == 1)
            return a[0];
        throw EnvelopeException.AmbiguousPredicate();
    }

    /// <summary>
    /// Returns the single assertion with the given predicate, or null if none exists.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if more than one assertion matches.</exception>
    public Envelope? OptionalAssertionWithPredicate(object predicate)
    {
        var a = AssertionsWithPredicate(predicate);
        if (a.Count == 0)
            return null;
        if (a.Count == 1)
            return a[0];
        throw EnvelopeException.AmbiguousPredicate();
    }

    /// <summary>
    /// Returns the object of the assertion with the given predicate.
    /// </summary>
    public Envelope ObjectForPredicate(object predicate)
    {
        return AssertionWithPredicate(predicate).AsObject()!;
    }

    /// <summary>
    /// Returns the object of the assertion with the given predicate, or null if none exists.
    /// </summary>
    public Envelope? OptionalObjectForPredicate(object predicate)
    {
        var a = AssertionsWithPredicate(predicate);
        if (a.Count == 0)
            return null;
        if (a.Count == 1)
            return a[0].Subject.AsObject()!;
        throw EnvelopeException.AmbiguousPredicate();
    }

    /// <summary>
    /// Returns all objects for assertions with the matching predicate.
    /// </summary>
    public List<Envelope> ObjectsForPredicate(object predicate)
    {
        return AssertionsWithPredicate(predicate)
            .Select(a => a.AsObject()!)
            .ToList();
    }

    /// <summary>
    /// Returns the object of the assertion with the given predicate, decoded as the given type.
    /// </summary>
    public T ExtractObjectForPredicate<T>(object predicate)
    {
        return AssertionWithPredicate(predicate).ExtractObject<T>();
    }

    /// <summary>
    /// Returns the object for the predicate decoded as T, or default if none exists.
    /// </summary>
    public T? ExtractOptionalObjectForPredicate<T>(object predicate)
    {
        var obj = OptionalObjectForPredicate(predicate);
        if (obj is null)
            return default;
        return obj.ExtractSubject<T>();
    }

    /// <summary>
    /// Returns the object for the predicate decoded as T, or the provided default value.
    /// </summary>
    public T ExtractObjectForPredicateWithDefault<T>(object predicate, T defaultValue)
    {
        var result = ExtractOptionalObjectForPredicate<T>(predicate);
        return result ?? defaultValue;
    }

    /// <summary>
    /// Returns all objects for the given predicate, decoded as the given type.
    /// </summary>
    public List<T> ExtractObjectsForPredicate<T>(object predicate)
    {
        return ObjectsForPredicate(predicate)
            .Select(o => o.ExtractSubject<T>())
            .ToList();
    }

    /// <summary>
    /// Returns the object of the assertion, decoded as the given type.
    /// </summary>
    public T ExtractObject<T>()
    {
        return TryObject().ExtractSubject<T>();
    }

    /// <summary>
    /// Returns the predicate of the assertion, decoded as the given type.
    /// </summary>
    public T ExtractPredicate<T>()
    {
        return TryPredicate().ExtractSubject<T>();
    }

    /// <summary>
    /// Returns the number of structural elements in the envelope, including itself.
    /// </summary>
    public int ElementsCount
    {
        get
        {
            int count = 1;
            switch (_case)
            {
                case EnvelopeCase.NodeCase n:
                    count += n.Subject.ElementsCount;
                    foreach (var assertion in n.Assertions)
                        count += assertion.ElementsCount;
                    break;
                case EnvelopeCase.AssertionCase a:
                    count += a.Assertion.Predicate.ElementsCount;
                    count += a.Assertion.Object.ElementsCount;
                    break;
                case EnvelopeCase.WrappedCase w:
                    count += w.Envelope.ElementsCount;
                    break;
            }
            return count;
        }
    }

    // ===== Wrap / Unwrap =====

    /// <summary>
    /// Returns a new envelope that wraps this envelope.
    /// </summary>
    public Envelope Wrap() => CreateWrapped(this);

    /// <summary>
    /// Unwraps and returns the inner envelope.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if this is not a wrapped envelope.</exception>
    public Envelope TryUnwrap()
    {
        return Subject.Case switch
        {
            EnvelopeCase.WrappedCase w => w.Envelope,
            _ => throw EnvelopeException.NotWrapped(),
        };
    }

    // ===== Digest =====

    /// <summary>
    /// Returns the envelope's digest.
    /// </summary>
    public Digest GetDigest() => _case switch
    {
        EnvelopeCase.NodeCase n => n.Digest,
        EnvelopeCase.LeafCase l => l.Digest,
        EnvelopeCase.WrappedCase w => w.Digest,
        EnvelopeCase.AssertionCase a => a.Assertion.GetDigest(),
        EnvelopeCase.ElidedCase e => e.Digest,
        EnvelopeCase.KnownValueCase kv => kv.Digest,
        EnvelopeCase.EncryptedCase enc => ((IDigestProvider)enc.EncryptedMessage).GetDigest(),
        EnvelopeCase.CompressedCase comp => ((IDigestProvider)comp.Compressed).GetDigest(),
        _ => throw new InvalidOperationException(),
    };

    /// <summary>
    /// Returns the set of digests contained in the envelope's elements, down to
    /// the specified level.
    /// </summary>
    public HashSet<Digest> Digests(int levelLimit)
    {
        var result = new HashSet<Digest>();
        WalkStructure(0, (env, level) =>
        {
            if (level < levelLimit)
            {
                result.Add(env.GetDigest());
                result.Add(env.Subject.GetDigest());
            }
        });
        return result;
    }

    /// <summary>
    /// Returns the set of all digests in the envelope, at all levels.
    /// </summary>
    public HashSet<Digest> DeepDigests() => Digests(int.MaxValue);

    /// <summary>
    /// Returns the set of digests in the envelope, down to its second level only.
    /// </summary>
    public HashSet<Digest> ShallowDigests() => Digests(2);

    /// <summary>
    /// Returns a digest that captures the structural form of the envelope.
    /// </summary>
    public Digest StructuralDigest()
    {
        var image = new List<byte>();
        WalkStructure(0, (env, _) =>
        {
            switch (env.Case)
            {
                case EnvelopeCase.ElidedCase:
                    image.Add(1);
                    break;
                case EnvelopeCase.EncryptedCase:
                    image.Add(0);
                    break;
                case EnvelopeCase.CompressedCase:
                    image.Add(2);
                    break;
            }
            image.AddRange(env.GetDigest().Data);
        });
        return Digest.FromImage(image.ToArray());
    }

    /// <summary>
    /// Tests if this envelope is semantically equivalent to another (same digest).
    /// </summary>
    public bool IsEquivalentTo(Envelope other)
    {
        return GetDigest() == other.GetDigest();
    }

    /// <summary>
    /// Tests if two envelopes are structurally identical (same content and structure).
    /// </summary>
    public bool IsIdenticalTo(Envelope other)
    {
        if (!IsEquivalentTo(other))
            return false;
        return StructuralDigest() == other.StructuralDigest();
    }

    // ===== Walk =====

    /// <summary>
    /// Walks the envelope structure recursively, calling the visitor for each element.
    /// </summary>
    internal void WalkStructure(int level, Action<Envelope, int> visitor)
    {
        visitor(this, level);
        int nextLevel = level + 1;
        switch (_case)
        {
            case EnvelopeCase.NodeCase n:
                n.Subject.WalkStructure(nextLevel, visitor);
                foreach (var assertion in n.Assertions)
                    assertion.WalkStructure(nextLevel, visitor);
                break;
            case EnvelopeCase.WrappedCase w:
                w.Envelope.WalkStructure(nextLevel, visitor);
                break;
            case EnvelopeCase.AssertionCase a:
                a.Assertion.Predicate.WalkStructure(nextLevel, visitor);
                a.Assertion.Object.WalkStructure(nextLevel, visitor);
                break;
        }
    }

    // ===== Equality =====

    /// <summary>
    /// Structural identity comparison (same content and same structure).
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is Envelope other && IsIdenticalTo(other);

    /// <inheritdoc />
    public override int GetHashCode() => GetDigest().GetHashCode();

    /// <summary>Tests structural identity of two envelopes.</summary>
    public static bool operator ==(Envelope? left, Envelope? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>Tests structural non-identity of two envelopes.</summary>
    public static bool operator !=(Envelope? left, Envelope? right) => !(left == right);

    // ===== CBOR Data Helpers =====

    /// <summary>
    /// Creates an envelope from a CBOR value.
    /// </summary>
    public static Envelope FromCbor(Cbor cbor)
    {
        return FromTaggedCbor(cbor);
    }

    /// <summary>
    /// Creates an envelope from raw CBOR binary data.
    /// </summary>
    public static Envelope FromCborData(byte[] data)
    {
        var cbor = Cbor.TryFromData(data);
        return FromCbor(cbor);
    }

    // ===== Position Extension =====

    /// <summary>
    /// Sets the position (index) of the envelope by adding or replacing a 'position' assertion.
    /// </summary>
    public Envelope SetPosition(int position)
    {
        var positionAssertions = AssertionsWithPredicate(KnownValuesRegistry.Position);
        if (positionAssertions.Count > 1)
            throw EnvelopeException.InvalidFormat();

        Envelope e;
        if (positionAssertions.Count == 1)
            e = RemoveAssertion(positionAssertions[0]);
        else
            e = this;

        return e.AddAssertion(KnownValuesRegistry.Position, position);
    }

    /// <summary>
    /// Retrieves the position value from the envelope's 'position' assertion.
    /// </summary>
    public int GetPosition()
    {
        var positionEnvelope = ObjectForPredicate(KnownValuesRegistry.Position);
        return positionEnvelope.ExtractSubject<int>();
    }

    /// <summary>
    /// Removes the 'position' assertion from the envelope.
    /// </summary>
    public Envelope RemovePosition()
    {
        var positionAssertions = AssertionsWithPredicate(KnownValuesRegistry.Position);
        if (positionAssertions.Count > 1)
            throw EnvelopeException.InvalidFormat();
        if (positionAssertions.Count == 1)
            return RemoveAssertion(positionAssertions[0]);
        return this;
    }
}
