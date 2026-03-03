namespace BlockchainCommons.SSKR;

/// <summary>
/// A specification for an SSKR split.
/// </summary>
public sealed class Spec : IEquatable<Spec>
{
    private readonly GroupSpec[] _groups;
    private readonly IReadOnlyList<GroupSpec> _groupsView;

    private Spec(int groupThreshold, GroupSpec[] groups)
    {
        GroupThreshold = groupThreshold;
        _groups = groups;
        _groupsView = Array.AsReadOnly(_groups);
    }

    /// <summary>
    /// Creates a new <see cref="Spec"/> with the given group threshold and
    /// groups.
    /// </summary>
    /// <exception cref="SSKRException">
    /// Thrown if the group threshold is zero, greater than the number of
    /// groups, or if the number of groups exceeds <see cref="Sskr.MaxShareCount"/>.
    /// </exception>
    public static Spec Create(int groupThreshold, IReadOnlyList<GroupSpec> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        if (groupThreshold <= 0)
            throw new SSKRException(SskrError.GroupThresholdInvalid);
        if (groupThreshold > groups.Count)
            throw new SSKRException(SskrError.GroupThresholdInvalid);
        if (groups.Count > Sskr.MaxShareCount)
            throw new SSKRException(SskrError.GroupCountInvalid);

        var copiedGroups = new GroupSpec[groups.Count];
        for (var i = 0; i < groups.Count; i++)
        {
            copiedGroups[i] = groups[i]
                ?? throw new ArgumentNullException(nameof(groups), "Groups cannot contain null entries.");
        }

        return new Spec(groupThreshold, copiedGroups);
    }

    /// <summary>Returns the group threshold.</summary>
    public int GroupThreshold { get; }

    /// <summary>Returns the group specifications.</summary>
    public IReadOnlyList<GroupSpec> Groups => _groupsView;

    /// <summary>Returns the number of groups.</summary>
    public int GroupCount => _groups.Length;

    /// <summary>Returns the total number of shares across all groups.</summary>
    public int ShareCount => _groups.Sum(group => group.MemberCount);

    public bool Equals(Spec? other)
    {
        if (other is null)
            return false;
        return GroupThreshold == other.GroupThreshold
            && _groups.SequenceEqual(other._groups);
    }

    public override bool Equals(object? obj) => Equals(obj as Spec);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GroupThreshold);
        foreach (var group in _groups)
            hash.Add(group);
        return hash.ToHashCode();
    }
}
