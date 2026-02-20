namespace BlockchainCommons.BCLifeHash;

internal enum Pattern
{
    Snowflake,
    Pinwheel,
    Fiducial,
}

internal static class Patterns
{
    public static Pattern SelectPattern(BitEnumerator entropy, LifeHashVersion version)
    {
        return version switch
        {
            LifeHashVersion.Fiducial or LifeHashVersion.GrayscaleFiducial => Pattern.Fiducial,
            _ => entropy.Next() ? Pattern.Snowflake : Pattern.Pinwheel,
        };
    }
}
