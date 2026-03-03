namespace BlockchainCommons.SSKR;

internal sealed class SSKRShare(
    ushort identifier,
    int groupIndex,
    int groupThreshold,
    int groupCount,
    int memberIndex,
    int memberThreshold,
    Secret value)
{
    public ushort Identifier { get; } = identifier;

    public int GroupIndex { get; } = groupIndex;

    public int GroupThreshold { get; } = groupThreshold;

    public int GroupCount { get; } = groupCount;

    public int MemberIndex { get; } = memberIndex;

    public int MemberThreshold { get; } = memberThreshold;

    public Secret Value { get; } = value;
}
