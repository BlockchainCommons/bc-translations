namespace BlockchainCommons.SSKR;

internal sealed class SSKRShare
{
    public SSKRShare(
        ushort identifier,
        int groupIndex,
        int groupThreshold,
        int groupCount,
        int memberIndex,
        int memberThreshold,
        Secret value)
    {
        Identifier = identifier;
        GroupIndex = groupIndex;
        GroupThreshold = groupThreshold;
        GroupCount = groupCount;
        MemberIndex = memberIndex;
        MemberThreshold = memberThreshold;
        Value = value;
    }

    public ushort Identifier { get; }

    public int GroupIndex { get; }

    public int GroupThreshold { get; }

    public int GroupCount { get; }

    public int MemberIndex { get; }

    public int MemberThreshold { get; }

    public Secret Value { get; }
}
