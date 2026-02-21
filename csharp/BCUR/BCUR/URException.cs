namespace BlockchainCommons.BCUR;

/// <summary>
/// Base exception for all UR-related errors.
/// </summary>
public class URException : Exception
{
    public URException(string message) : base(message) { }
    public URException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Error from the UR decoder.
/// </summary>
public class URDecoderException : URException
{
    public URDecoderException(string message) : base($"UR decoder error ({message})") { }
}

/// <summary>
/// Error from bytewords encoding/decoding.
/// </summary>
public class BytewordsException : URException
{
    public BytewordsException(string message) : base($"Bytewords error ({message})") { }
}

/// <summary>
/// Error wrapping a CBOR exception.
/// </summary>
public class URCborException : URException
{
    public URCborException(Exception innerException)
        : base($"CBOR error ({innerException.Message})", innerException) { }
}

/// <summary>
/// The UR string does not start with "ur:".
/// </summary>
public class InvalidSchemeException : URException
{
    public InvalidSchemeException() : base("invalid UR scheme") { }
}

/// <summary>
/// No UR type was specified in the UR string.
/// </summary>
public class TypeUnspecifiedException : URException
{
    public TypeUnspecifiedException() : base("no UR type specified") { }
}

/// <summary>
/// The UR type string contains invalid characters.
/// </summary>
public class InvalidTypeException : URException
{
    public InvalidTypeException() : base("invalid UR type") { }
}

/// <summary>
/// Expected a single-part UR but found a multi-part UR.
/// </summary>
public class NotSinglePartException : URException
{
    public NotSinglePartException() : base("UR is not a single-part") { }
}

/// <summary>
/// The UR type did not match the expected type.
/// </summary>
public class UnexpectedTypeException : URException
{
    public string Expected { get; }
    public string Found { get; }

    public UnexpectedTypeException(string expected, string found)
        : base($"expected UR type {expected}, but found {found}")
    {
        Expected = expected;
        Found = found;
    }
}

/// <summary>
/// Error from the fountain encoder/decoder.
/// </summary>
public class FountainException : URException
{
    public FountainException(string message) : base($"Fountain error ({message})") { }
}
