using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR;

/// <summary>
/// A Uniform Resource (UR) is a URI-encoded CBOR object.
/// </summary>
public sealed class UR : IEquatable<UR>
{
    private readonly URType _urType;
    private readonly Cbor _cbor;

    private UR(URType urType, Cbor cbor)
    {
        _urType = urType;
        _cbor = cbor;
    }

    /// <summary>
    /// Creates a new UR from the provided type string and CBOR value.
    /// </summary>
    /// <exception cref="InvalidTypeException">If the type string is invalid.</exception>
    public static UR Create(string urType, Cbor cbor)
    {
        return new UR(new URType(urType), cbor);
    }

    /// <summary>
    /// Creates a new UR from the provided URType and CBOR value.
    /// </summary>
    public static UR Create(URType urType, Cbor cbor)
    {
        return new UR(urType, cbor);
    }

    /// <summary>
    /// Creates a new UR from a UR-encoded string.
    /// </summary>
    /// <exception cref="InvalidSchemeException">If the string doesn't start with "ur:".</exception>
    /// <exception cref="TypeUnspecifiedException">If no type is specified.</exception>
    /// <exception cref="NotSinglePartException">If the UR is not single-part.</exception>
    public static UR FromUrString(string urString)
    {
        var lower = urString.ToLowerInvariant();

        if (!lower.StartsWith("ur:", StringComparison.Ordinal))
            throw new InvalidSchemeException();

        var withoutScheme = lower[3..];
        var slashIndex = withoutScheme.IndexOf('/');
        if (slashIndex < 0)
            throw new TypeUnspecifiedException();

        var urTypeStr = withoutScheme[..slashIndex];
        var urType = new URType(urTypeStr);

        var (kind, data) = UREncoding.Decode(lower);
        if (kind != URKind.SinglePart)
            throw new NotSinglePartException();

        try
        {
            var cbor = Cbor.TryFromData(data);
            return new UR(urType, cbor);
        }
        catch (Exception ex) when (ex is not URException)
        {
            throw new URCborException(ex);
        }
    }

    /// <summary>
    /// Returns the UR type.
    /// </summary>
    public URType UrType => _urType;

    /// <summary>
    /// Returns the UR type as a string.
    /// </summary>
    public string UrTypeStr => _urType.Value;

    /// <summary>
    /// Returns the CBOR value.
    /// </summary>
    public Cbor Cbor => _cbor;

    /// <summary>
    /// Returns the UR-encoded string representation.
    /// </summary>
    public string ToUrString()
    {
        var data = _cbor.ToCborData();
        return UREncoding.Encode(data, _urType.Value);
    }

    /// <summary>
    /// Returns the UR string in uppercase, most efficient for QR codes.
    /// </summary>
    public string QrString() => ToUrString().ToUpperInvariant();

    /// <summary>
    /// Returns the UR data in uppercase bytes, most efficient for QR codes.
    /// </summary>
    public byte[] QrData() => System.Text.Encoding.ASCII.GetBytes(QrString());

    /// <summary>
    /// Checks the UR type against an expected type.
    /// </summary>
    /// <exception cref="UnexpectedTypeException">If the types don't match.</exception>
    public void CheckType(string expectedType)
    {
        var expected = new URType(expectedType);
        if (_urType != expected)
        {
            throw new UnexpectedTypeException(expected.Value, _urType.Value);
        }
    }

    /// <summary>
    /// Checks the UR type against an expected URType.
    /// </summary>
    public void CheckType(URType expectedType)
    {
        if (_urType != expectedType)
        {
            throw new UnexpectedTypeException(expectedType.Value, _urType.Value);
        }
    }

    public bool Equals(UR? other)
    {
        if (other is null) return false;
        return _urType == other._urType && _cbor.Equals(other._cbor);
    }

    public override bool Equals(object? obj) => obj is UR other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_urType, _cbor);
    public override string ToString() => ToUrString();

    public static bool operator ==(UR? left, UR? right) =>
        left is null ? right is null : left.Equals(right);
    public static bool operator !=(UR? left, UR? right) => !(left == right);

    /// <summary>
    /// Converts a UR to its CBOR value.
    /// </summary>
    public static implicit operator Cbor(UR ur) => ur._cbor;
}
