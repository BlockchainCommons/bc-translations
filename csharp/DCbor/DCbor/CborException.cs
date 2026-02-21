namespace BlockchainCommons.DCbor;

/// <summary>
/// Base exception for all dCBOR encoding and decoding errors.
/// </summary>
public class CborException : Exception
{
    public CborException(string message) : base(message) { }
    public CborException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// The CBOR data ended prematurely during decoding.
/// </summary>
public class CborUnderrunException : CborException
{
    public CborUnderrunException() : base("early end of CBOR data") { }
}

/// <summary>
/// An unsupported or invalid value was encountered in a CBOR header byte.
/// </summary>
public class CborUnsupportedHeaderValueException : CborException
{
    public byte HeaderValue { get; }
    public CborUnsupportedHeaderValueException(byte headerValue)
        : base("unsupported value in CBOR header")
    {
        HeaderValue = headerValue;
    }
}

/// <summary>
/// A CBOR numeric value was encoded in non-canonical form.
/// </summary>
public class CborNonCanonicalNumericException : CborException
{
    public CborNonCanonicalNumericException()
        : base("a CBOR numeric value was encoded in non-canonical form") { }
}

/// <summary>
/// An invalid CBOR simple value was encountered.
/// </summary>
public class CborInvalidSimpleValueException : CborException
{
    public CborInvalidSimpleValueException()
        : base("an invalid CBOR simple value was encountered") { }
}

/// <summary>
/// A CBOR text string was not valid UTF-8.
/// </summary>
public class CborInvalidStringException : CborException
{
    public CborInvalidStringException(string detail)
        : base($"an invalidly-encoded UTF-8 string was encountered in the CBOR ({detail})") { }
}

/// <summary>
/// A CBOR text string was not in Unicode NFC form.
/// </summary>
public class CborNonCanonicalStringException : CborException
{
    public CborNonCanonicalStringException()
        : base("a CBOR string was not encoded in Unicode Canonical Normalization Form C") { }
}

/// <summary>
/// The decoded CBOR item had trailing bytes.
/// </summary>
public class CborUnusedDataException : CborException
{
    public int ExtraBytes { get; }
    public CborUnusedDataException(int extraBytes)
        : base($"the decoded CBOR had {extraBytes} extra bytes at the end")
    {
        ExtraBytes = extraBytes;
    }
}

/// <summary>
/// Map keys are not in canonical order.
/// </summary>
public class CborMisorderedMapKeyException : CborException
{
    public CborMisorderedMapKeyException()
        : base("the decoded CBOR map has keys that are not in canonical order") { }
}

/// <summary>
/// Map contains duplicate keys.
/// </summary>
public class CborDuplicateMapKeyException : CborException
{
    public CborDuplicateMapKeyException()
        : base("the decoded CBOR map has a duplicate key") { }
}

/// <summary>
/// A requested key was not found in a CBOR map.
/// </summary>
public class CborMissingMapKeyException : CborException
{
    public CborMissingMapKeyException()
        : base("missing CBOR map key") { }
}

/// <summary>
/// A CBOR numeric value could not be represented in the target type.
/// </summary>
public class CborOutOfRangeException : CborException
{
    public CborOutOfRangeException()
        : base("the CBOR numeric value could not be represented in the specified numeric type") { }
}

/// <summary>
/// The CBOR value is not of the expected type.
/// </summary>
public class CborWrongTypeException : CborException
{
    public CborWrongTypeException()
        : base("the decoded CBOR value was not the expected type") { }
}

/// <summary>
/// The CBOR tagged value had a different tag than expected.
/// </summary>
public class CborWrongTagException : CborException
{
    public Tag ExpectedTag { get; }
    public Tag ActualTag { get; }
    public CborWrongTagException(Tag expected, Tag actual)
        : base($"expected CBOR tag {expected}, but got {actual}")
    {
        ExpectedTag = expected;
        ActualTag = actual;
    }
}

/// <summary>
/// Invalid ISO 8601 date format.
/// </summary>
public class CborInvalidDateException : CborException
{
    public CborInvalidDateException(string detail)
        : base($"invalid ISO 8601 date string: {detail}") { }
}
