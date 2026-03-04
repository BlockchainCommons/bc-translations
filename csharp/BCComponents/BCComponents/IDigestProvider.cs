namespace BlockchainCommons.BCComponents;

/// <summary>
/// A type that can provide a single unique digest that characterizes its contents.
/// </summary>
/// <remarks>
/// Implementations should return a <see cref="Digest"/> derived from the
/// object's content, typically by hashing with SHA-256. This is useful for
/// content-addressable storage, data integrity verification, and comparing
/// objects by their content rather than identity.
/// </remarks>
public interface IDigestProvider
{
    /// <summary>
    /// Returns a digest that uniquely characterizes the content of this object.
    /// </summary>
    /// <returns>A <see cref="Digest"/> computed from this object's content.</returns>
    Digest GetDigest();
}
