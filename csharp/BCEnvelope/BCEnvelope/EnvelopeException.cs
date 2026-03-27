using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Base exception type for Gordian Envelope operations.
/// </summary>
public class EnvelopeException : Exception
{
    public EnvelopeException(string message) : base(message) { }
    public EnvelopeException(string message, Exception innerException) : base(message, innerException) { }

    // --- Base Specification ---

    /// <summary>
    /// Creates an exception for an already-elided envelope that cannot be compressed or encrypted.
    /// </summary>
    public static EnvelopeException AlreadyElided() =>
        new("envelope was elided, so it cannot be compressed or encrypted");

    /// <summary>
    /// Creates an exception when multiple assertions match a predicate.
    /// </summary>
    public static EnvelopeException AmbiguousPredicate() =>
        new("more than one assertion matches the predicate");

    /// <summary>
    /// Creates an exception for a digest validation failure.
    /// </summary>
    public static EnvelopeException InvalidDigest() =>
        new("digest did not match");

    /// <summary>
    /// Creates an exception for an invalid envelope format.
    /// </summary>
    public static EnvelopeException InvalidFormat() =>
        new("invalid format");

    /// <summary>
    /// Creates an exception when a digest is expected but not found.
    /// </summary>
    public static EnvelopeException MissingDigest() =>
        new("a digest was expected but not found");

    /// <summary>
    /// Creates an exception when no assertion matches a predicate.
    /// </summary>
    public static EnvelopeException NonexistentPredicate() =>
        new("no assertion matches the predicate");

    /// <summary>
    /// Creates an exception when unwrapping a non-wrapped envelope.
    /// </summary>
    public static EnvelopeException NotWrapped() =>
        new("cannot unwrap an envelope that was not wrapped");

    /// <summary>
    /// Creates an exception when the envelope subject is not a leaf.
    /// </summary>
    public static EnvelopeException NotLeaf() =>
        new("the envelope's subject is not a leaf");

    /// <summary>
    /// Creates an exception when the envelope subject is not an assertion.
    /// </summary>
    public static EnvelopeException NotAssertion() =>
        new("the envelope's subject is not an assertion");

    /// <summary>
    /// Creates an exception for an invalid assertion format.
    /// </summary>
    public static EnvelopeException InvalidAssertion() =>
        new("assertion must be a map with exactly one element");

    // --- Attachments Extension ---

    /// <summary>
    /// Creates an exception for an invalid attachment format.
    /// </summary>
    public static EnvelopeException InvalidAttachment() =>
        new("invalid attachment");

    /// <summary>
    /// Creates an exception for a nonexistent attachment.
    /// </summary>
    public static EnvelopeException NonexistentAttachment() =>
        new("nonexistent attachment");

    /// <summary>
    /// Creates an exception for an ambiguous attachment.
    /// </summary>
    public static EnvelopeException AmbiguousAttachment() =>
        new("ambiguous attachment");

    // --- Edges Extension ---

    /// <summary>
    /// Creates an exception when an edge is missing the 'isA' assertion.
    /// </summary>
    public static EnvelopeException EdgeMissingIsA() =>
        new("edge missing 'isA' assertion");

    /// <summary>
    /// Creates an exception when an edge is missing the 'source' assertion.
    /// </summary>
    public static EnvelopeException EdgeMissingSource() =>
        new("edge missing 'source' assertion");

    /// <summary>
    /// Creates an exception when an edge is missing the 'target' assertion.
    /// </summary>
    public static EnvelopeException EdgeMissingTarget() =>
        new("edge missing 'target' assertion");

    /// <summary>
    /// Creates an exception when an edge has duplicate 'isA' assertions.
    /// </summary>
    public static EnvelopeException EdgeDuplicateIsA() =>
        new("edge has duplicate 'isA' assertions");

    /// <summary>
    /// Creates an exception when an edge has duplicate 'source' assertions.
    /// </summary>
    public static EnvelopeException EdgeDuplicateSource() =>
        new("edge has duplicate 'source' assertions");

    /// <summary>
    /// Creates an exception when an edge has duplicate 'target' assertions.
    /// </summary>
    public static EnvelopeException EdgeDuplicateTarget() =>
        new("edge has duplicate 'target' assertions");

    /// <summary>
    /// Creates an exception when an edge has an unexpected assertion.
    /// </summary>
    public static EnvelopeException EdgeUnexpectedAssertion() =>
        new("edge has unexpected assertion");

    /// <summary>
    /// Creates an exception when a requested edge does not exist.
    /// </summary>
    public static EnvelopeException NonexistentEdge() =>
        new("nonexistent edge");

    /// <summary>
    /// Creates an exception when multiple edges match a query.
    /// </summary>
    public static EnvelopeException AmbiguousEdge() =>
        new("ambiguous edge");

    // --- Compression Extension ---

    /// <summary>
    /// Creates an exception for an already-compressed envelope.
    /// </summary>
    public static EnvelopeException AlreadyCompressed() =>
        new("envelope was already compressed");

    /// <summary>
    /// Creates an exception when decompressing a non-compressed envelope.
    /// </summary>
    public static EnvelopeException NotCompressed() =>
        new("cannot decompress an envelope that was not compressed");

    // --- Symmetric Encryption Extension ---

    /// <summary>
    /// Creates an exception for an already-encrypted or compressed envelope.
    /// </summary>
    public static EnvelopeException AlreadyEncrypted() =>
        new("envelope was already encrypted or compressed, so it cannot be encrypted");

    /// <summary>
    /// Creates an exception when decrypting a non-encrypted envelope.
    /// </summary>
    public static EnvelopeException NotEncrypted() =>
        new("cannot decrypt an envelope that was not encrypted");

    // --- Known Values Extension ---

    /// <summary>
    /// Creates an exception when the envelope subject is not a known value.
    /// </summary>
    public static EnvelopeException NotKnownValue() =>
        new("the envelope's subject is not a known value");

    /// <summary>
    /// Creates an exception when the subject of the envelope is not the unit value.
    /// </summary>
    public static EnvelopeException SubjectNotUnit() =>
        new("the subject of the envelope is not the unit value");

    // --- Public Key Encryption Extension ---

    /// <summary>
    /// Creates an exception for an unknown recipient.
    /// </summary>
    public static EnvelopeException UnknownRecipient() =>
        new("unknown recipient");

    // --- Encrypted Key Extension ---

    /// <summary>
    /// Creates an exception for an unknown secret.
    /// </summary>
    public static EnvelopeException UnknownSecret() =>
        new("secret not found");

    // --- Public Key Signing Extension ---

    /// <summary>
    /// Creates an exception for a signature verification failure.
    /// </summary>
    public static EnvelopeException UnverifiedSignature() =>
        new("could not verify a signature");

    /// <summary>
    /// Creates an exception for an invalid outer signature object type.
    /// </summary>
    public static EnvelopeException InvalidOuterSignatureType() =>
        new("unexpected outer signature object type");

    /// <summary>
    /// Creates an exception for an invalid inner signature object type.
    /// </summary>
    public static EnvelopeException InvalidInnerSignatureType() =>
        new("unexpected inner signature object type");

    /// <summary>
    /// Creates an exception when the inner signature is not made with the same key.
    /// </summary>
    public static EnvelopeException UnverifiedInnerSignature() =>
        new("inner signature not made with same key as outer signature");

    /// <summary>
    /// Creates an exception for an unexpected signature object type.
    /// </summary>
    public static EnvelopeException InvalidSignatureType() =>
        new("unexpected signature object type");

    // --- SSKR Extension ---

    /// <summary>
    /// Creates an exception for invalid SSKR shares.
    /// </summary>
    public static EnvelopeException InvalidShares() =>
        new("invalid SSKR shares");

    // --- Types Extension ---

    /// <summary>
    /// Creates an exception for an invalid type.
    /// </summary>
    public static EnvelopeException InvalidType() =>
        new("invalid type");

    /// <summary>
    /// Creates an exception for ambiguous type information.
    /// </summary>
    public static EnvelopeException AmbiguousType() =>
        new("ambiguous type");

    // --- Expressions Extension ---

    /// <summary>
    /// Creates an exception for an unexpected response ID.
    /// </summary>
    public static EnvelopeException UnexpectedResponseID() =>
        new("unexpected response ID");

    /// <summary>
    /// Creates an exception for an invalid response.
    /// </summary>
    public static EnvelopeException InvalidResponse() =>
        new("invalid response");

    // --- Wrapped Errors ---

    /// <summary>
    /// Creates an exception wrapping a CBOR exception.
    /// </summary>
    public static EnvelopeException FromCborException(CborException inner) =>
        new($"dcbor error: {inner.Message}", inner);

    /// <summary>
    /// Creates an exception wrapping a BCComponents exception.
    /// </summary>
    public static EnvelopeException FromComponentsException(BCComponentsException inner) =>
        new($"components error: {inner.Message}", inner);

    /// <summary>
    /// Creates an exception wrapping an SSKR exception.
    /// </summary>
    public static EnvelopeException FromSskrException(Exception inner) =>
        new($"sskr error: {inner.Message}", inner);

    /// <summary>
    /// Creates a general envelope exception with a custom message.
    /// </summary>
    public static EnvelopeException General(string message) =>
        new($"general error: {message}");
}
