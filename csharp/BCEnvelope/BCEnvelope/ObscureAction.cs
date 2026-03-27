using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Actions that can be performed on parts of an envelope to obscure them.
/// </summary>
/// <remarks>
/// Gordian Envelope supports several ways to obscure parts of an envelope while
/// maintaining its semantic integrity and digest tree. This abstract sealed
/// hierarchy defines the possible actions: elision, encryption, and compression.
/// </remarks>
public abstract class ObscureAction
{
    private ObscureAction() { }

    /// <summary>
    /// Elide the target, leaving only its digest.
    /// </summary>
    public sealed class ElideAction : ObscureAction
    {
        /// <summary>Singleton instance.</summary>
        public static readonly ElideAction Instance = new();
        private ElideAction() { }
    }

    /// <summary>
    /// Encrypt the target using the specified symmetric key.
    /// </summary>
    public sealed class EncryptAction : ObscureAction
    {
        /// <summary>The symmetric key to use for encryption.</summary>
        public SymmetricKey Key { get; }

        /// <summary>
        /// Creates an encrypt action with the specified key.
        /// </summary>
        /// <param name="key">The symmetric key to use.</param>
        public EncryptAction(SymmetricKey key) { Key = key; }
    }

    /// <summary>
    /// Compress the target.
    /// </summary>
    public sealed class CompressAction : ObscureAction
    {
        /// <summary>Singleton instance.</summary>
        public static readonly CompressAction Instance = new();
        private CompressAction() { }
    }

    /// <summary>Returns an elide action.</summary>
    public static ObscureAction Elide => ElideAction.Instance;

    /// <summary>Returns an encrypt action with the given key.</summary>
    /// <param name="key">The symmetric key to use.</param>
    /// <returns>An encrypt action.</returns>
    public static ObscureAction Encrypt(SymmetricKey key) => new EncryptAction(key);

    /// <summary>Returns a compress action.</summary>
    public static ObscureAction Compress => CompressAction.Instance;
}
