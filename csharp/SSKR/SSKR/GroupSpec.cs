using System.Globalization;

namespace BlockchainCommons.SSKR;

/// <summary>
/// A specification for a group of shares within an SSKR split.
/// </summary>
public sealed class GroupSpec : IEquatable<GroupSpec>
{
    private GroupSpec(int memberThreshold, int memberCount)
    {
        MemberThreshold = memberThreshold;
        MemberCount = memberCount;
    }

    /// <summary>
    /// Creates a new <see cref="GroupSpec"/> with the given member threshold
    /// and count.
    /// </summary>
    /// <exception cref="SSKRException">
    /// Thrown if the member count is zero, greater than
    /// <see cref="Sskr.MaxShareCount"/>, or if member threshold is greater than
    /// member count.
    /// </exception>
    public static GroupSpec Create(int memberThreshold, int memberCount)
    {
        if (memberCount <= 0)
            throw new SSKRException(SskrError.MemberCountInvalid);
        if (memberCount > Sskr.MaxShareCount)
            throw new SSKRException(SskrError.MemberCountInvalid);
        if (memberThreshold < 0)
            throw new SSKRException(SskrError.MemberThresholdInvalid);
        if (memberThreshold > memberCount)
            throw new SSKRException(SskrError.MemberThresholdInvalid);

        return new GroupSpec(memberThreshold, memberCount);
    }

    /// <summary>Returns the default group specification (1-of-1).</summary>
    public static GroupSpec Default { get; } = Create(1, 1);

    /// <summary>Returns the member share threshold for this group.</summary>
    public int MemberThreshold { get; }

    /// <summary>Returns the number of member shares in this group.</summary>
    public int MemberCount { get; }

    /// <summary>Parses a group specification string in the form <c>M-of-N</c>.</summary>
    /// <exception cref="SSKRException">Thrown on invalid format.</exception>
    public static GroupSpec Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        var parts = s.Split('-');
        if (parts.Length != 3)
            throw new SSKRException(SskrError.GroupSpecInvalid);
        if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var memberThreshold))
            throw new SSKRException(SskrError.GroupSpecInvalid);
        if (parts[1] != "of")
            throw new SSKRException(SskrError.GroupSpecInvalid);
        if (!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var memberCount))
            throw new SSKRException(SskrError.GroupSpecInvalid);

        return Create(memberThreshold, memberCount);
    }

    public override string ToString()
    {
        return $"{MemberThreshold}-of-{MemberCount}";
    }

    public bool Equals(GroupSpec? other)
    {
        if (other is null)
            return false;
        return MemberThreshold == other.MemberThreshold
            && MemberCount == other.MemberCount;
    }

    public override bool Equals(object? obj) => Equals(obj as GroupSpec);

    public override int GetHashCode() => HashCode.Combine(MemberThreshold, MemberCount);
}
