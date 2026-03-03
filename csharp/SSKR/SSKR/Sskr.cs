using BlockchainCommons.BCRand;
using BlockchainCommons.BCShamir;

namespace BlockchainCommons.SSKR;

/// <summary>
/// Sharded Secret Key Reconstruction (SSKR) operations.
/// </summary>
public static class Sskr
{
    /// <summary>The minimum length of a secret.</summary>
    public const int MinSecretLen = Shamir.MinSecretLen;

    /// <summary>The maximum length of a secret.</summary>
    public const int MaxSecretLen = Shamir.MaxSecretLen;

    /// <summary>The maximum number of shares that can be generated from a secret.</summary>
    public const int MaxShareCount = Shamir.MaxShareCount;

    /// <summary>The maximum number of groups in a split.</summary>
    public const int MaxGroupsCount = MaxShareCount;

    /// <summary>The number of bytes used to encode the metadata for a share.</summary>
    public const int MetadataSizeBytes = 5;

    /// <summary>The minimum number of bytes required to encode a share.</summary>
    public const int MinSerializeSizeBytes = MetadataSizeBytes + MinSecretLen;

    /// <summary>
    /// Generates SSKR shares for the given <paramref name="spec"/> and
    /// <paramref name="masterSecret"/>.
    /// </summary>
    public static byte[][][] Generate(Spec spec, Secret masterSecret)
    {
        return GenerateUsing(spec, masterSecret, SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates SSKR shares for the given <paramref name="spec"/> and
    /// <paramref name="masterSecret"/> using the provided random number generator.
    /// </summary>
    public static byte[][][] GenerateUsing(
        Spec spec,
        Secret masterSecret,
        IRandomNumberGenerator randomGenerator)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(masterSecret);
        ArgumentNullException.ThrowIfNull(randomGenerator);

        var groupsShares = GenerateShares(spec, masterSecret, randomGenerator);
        var result = new byte[groupsShares.Count][][];

        for (var groupIndex = 0; groupIndex < groupsShares.Count; groupIndex++)
        {
            var shares = groupsShares[groupIndex];
            var serializedGroup = new byte[shares.Count][];
            for (var memberIndex = 0; memberIndex < shares.Count; memberIndex++)
                serializedGroup[memberIndex] = SerializeShare(shares[memberIndex]);
            result[groupIndex] = serializedGroup;
        }

        return result;
    }

    /// <summary>
    /// Combines the given SSKR shares into a <see cref="Secret"/>.
    /// </summary>
    public static Secret Combine(IReadOnlyList<byte[]> shares)
    {
        ArgumentNullException.ThrowIfNull(shares);

        var sskrShares = new List<SSKRShare>(shares.Count);
        foreach (var share in shares)
        {
            ArgumentNullException.ThrowIfNull(share, nameof(shares));
            sskrShares.Add(DeserializeShare(share));
        }

        return CombineShares(sskrShares);
    }

    private static byte[] SerializeShare(SSKRShare share)
    {
        // Pack id, group, and member metadata into 5 bytes:
        // identifier: 16 bits
        // group-threshold: 4 bits
        // group-count: 4 bits
        // group-index: 4 bits
        // member-threshold: 4 bits
        // reserved: 4 bits (must be zero)
        // member-index: 4 bits
        var result = new byte[MetadataSizeBytes + share.Value.Length];

        var gt = (share.GroupThreshold - 1) & 0xF;
        var gc = (share.GroupCount - 1) & 0xF;
        var gi = share.GroupIndex & 0xF;
        var mt = (share.MemberThreshold - 1) & 0xF;
        var mi = share.MemberIndex & 0xF;

        result[0] = unchecked((byte)(share.Identifier >> 8));
        result[1] = unchecked((byte)share.Identifier);
        result[2] = unchecked((byte)((gt << 4) | gc));
        result[3] = unchecked((byte)((gi << 4) | mt));
        result[4] = unchecked((byte)mi);
        share.Value.Data.CopyTo(result.AsSpan(MetadataSizeBytes));

        return result;
    }

    private static SSKRShare DeserializeShare(ReadOnlySpan<byte> source)
    {
        if (source.Length < MetadataSizeBytes)
            throw new SSKRException(SskrError.ShareLengthInvalid);

        var groupThreshold = (source[2] >> 4) + 1;
        var groupCount = (source[2] & 0xF) + 1;
        if (groupThreshold > groupCount)
            throw new SSKRException(SskrError.GroupThresholdInvalid);

        var identifier = (ushort)((source[0] << 8) | source[1]);
        var groupIndex = source[3] >> 4;
        var memberThreshold = (source[3] & 0xF) + 1;
        var reserved = source[4] >> 4;
        if (reserved != 0)
            throw new SSKRException(SskrError.ShareReservedBitsInvalid);
        var memberIndex = source[4] & 0xF;
        var value = Secret.Create(source[MetadataSizeBytes..]);

        return new SSKRShare(
            identifier,
            groupIndex,
            groupThreshold,
            groupCount,
            memberIndex,
            memberThreshold,
            value);
    }

    private static List<List<SSKRShare>> GenerateShares(
        Spec spec,
        Secret masterSecret,
        IRandomNumberGenerator randomGenerator)
    {
        var identifierBytes = new byte[2];
        randomGenerator.FillRandomData(identifierBytes);
        var identifier = (ushort)((identifierBytes[0] << 8) | identifierBytes[1]);

        byte[][] groupSecrets;
        try
        {
            groupSecrets = Shamir.SplitSecret(
                spec.GroupThreshold,
                spec.GroupCount,
                masterSecret.Data,
                randomGenerator);
        }
        catch (BCShamirException ex)
        {
            throw new SSKRException(ex);
        }

        var groupsShares = new List<List<SSKRShare>>(spec.GroupCount);

        for (var groupIndex = 0; groupIndex < spec.GroupCount; groupIndex++)
        {
            var group = spec.Groups[groupIndex];
            var groupSecret = groupSecrets[groupIndex];

            byte[][] memberSecrets;
            try
            {
                memberSecrets = Shamir.SplitSecret(
                    group.MemberThreshold,
                    group.MemberCount,
                    groupSecret,
                    randomGenerator);
            }
            catch (BCShamirException ex)
            {
                throw new SSKRException(ex);
            }

            var memberSskrShares = new List<SSKRShare>(memberSecrets.Length);
            for (var memberIndex = 0; memberIndex < memberSecrets.Length; memberIndex++)
            {
                var memberSecret = Secret.Create(memberSecrets[memberIndex]);
                memberSskrShares.Add(
                    new SSKRShare(
                        identifier,
                        groupIndex,
                        spec.GroupThreshold,
                        spec.GroupCount,
                        memberIndex,
                        group.MemberThreshold,
                        memberSecret));
            }

            groupsShares.Add(memberSskrShares);
        }

        return groupsShares;
    }

    private static Secret CombineShares(IReadOnlyList<SSKRShare> shares)
    {
        if (shares.Count == 0)
            throw new SSKRException(SskrError.SharesEmpty);

        ushort identifier = 0;
        var groupThreshold = 0;
        var groupCount = 0;
        var secretLength = 0;

        var nextGroup = 0;
        var groups = new List<Group>(16);

        for (var i = 0; i < shares.Count; i++)
        {
            var share = shares[i];

            if (i == 0)
            {
                identifier = share.Identifier;
                groupCount = share.GroupCount;
                groupThreshold = share.GroupThreshold;
                secretLength = share.Value.Length;
            }
            else if (share.Identifier != identifier
                || share.GroupThreshold != groupThreshold
                || share.GroupCount != groupCount
                || share.Value.Length != secretLength)
            {
                throw new SSKRException(SskrError.ShareSetInvalid);
            }

            var groupFound = false;
            foreach (var group in groups)
            {
                if (share.GroupIndex != group.GroupIndex)
                    continue;

                groupFound = true;

                if (share.MemberThreshold != group.MemberThreshold)
                    throw new SSKRException(SskrError.MemberThresholdInvalid);

                foreach (var memberIndex in group.MemberIndexes)
                {
                    if (share.MemberIndex == memberIndex)
                        throw new SSKRException(SskrError.DuplicateMemberIndex);
                }

                if (group.MemberIndexes.Count < group.MemberThreshold)
                {
                    group.MemberIndexes.Add(share.MemberIndex);
                    group.MemberShares.Add(share.Value.Clone());
                }
            }

            if (!groupFound)
            {
                var group = new Group(share.GroupIndex, share.MemberThreshold);
                group.MemberIndexes.Add(share.MemberIndex);
                group.MemberShares.Add(share.Value.Clone());
                groups.Add(group);
                nextGroup += 1;
            }
        }

        if (nextGroup < groupThreshold)
            throw new SSKRException(SskrError.NotEnoughGroups);

        var masterIndexes = new List<byte>(16);
        var masterShares = new List<byte[]>(16);

        foreach (var group in groups)
        {
            if (group.MemberIndexes.Count < group.MemberThreshold)
                continue;

            try
            {
                var memberIndexes = new byte[group.MemberIndexes.Count];
                for (var i = 0; i < group.MemberIndexes.Count; i++)
                    memberIndexes[i] = unchecked((byte)group.MemberIndexes[i]);

                var memberShares = new byte[group.MemberShares.Count][];
                for (var i = 0; i < group.MemberShares.Count; i++)
                    memberShares[i] = group.MemberShares[i].DataRef;

                var groupSecret = Shamir.RecoverSecret(memberIndexes, memberShares);
                masterIndexes.Add(unchecked((byte)group.GroupIndex));
                masterShares.Add(groupSecret);
            }
            catch (BCShamirException)
            {
                // Ignore groups that cannot be recovered; continue until quorum.
            }

            if (masterIndexes.Count == groupThreshold)
                break;
        }

        if (masterIndexes.Count < groupThreshold)
            throw new SSKRException(SskrError.NotEnoughGroups);

        try
        {
            var masterSecret = Shamir.RecoverSecret(masterIndexes, masterShares);
            return Secret.Create(masterSecret);
        }
        catch (BCShamirException ex)
        {
            throw new SSKRException(ex);
        }
    }

    private sealed class Group
    {
        public Group(int groupIndex, int memberThreshold)
        {
            GroupIndex = groupIndex;
            MemberThreshold = memberThreshold;
            MemberIndexes = new List<int>(16);
            MemberShares = new List<Secret>(16);
        }

        public int GroupIndex { get; }

        public int MemberThreshold { get; }

        public List<int> MemberIndexes { get; }

        public List<Secret> MemberShares { get; }
    }
}
