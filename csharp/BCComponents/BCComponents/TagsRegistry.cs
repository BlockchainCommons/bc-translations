using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Registers all bc-tags and bc-components summarizers in the dCBOR tag store.
/// </summary>
/// <remarks>
/// Call <see cref="RegisterTags"/> once at application startup to enable tag
/// name resolution and summarization in diagnostic output formatting.
/// </remarks>
public static class TagsRegistry
{
    /// <summary>
    /// Registers all bc-tags and bc-components summarizers in the given
    /// <paramref name="tagsStore"/>.
    /// </summary>
    /// <remarks>
    /// This first delegates to <see cref="BcTags.RegisterTagsIn"/> for
    /// BC tag constants, then installs component-specific summarizers for
    /// diagnostic output.
    /// </remarks>
    /// <param name="tagsStore">The tags store to register into.</param>
    public static void RegisterTagsIn(TagsStore tagsStore)
    {
        BcTags.RegisterTagsIn(tagsStore);

        tagsStore.SetSummarizer(BcTags.TagDigest, (untaggedCbor, _) =>
        {
            var digest = Digest.FromUntaggedCbor(untaggedCbor);
            return $"Digest({digest.ShortDescription()})";
        });

        tagsStore.SetSummarizer(BcTags.TagArid, (untaggedCbor, _) =>
        {
            var arid = ARID.FromUntaggedCbor(untaggedCbor);
            return $"ARID({arid.ShortDescription()})";
        });

        tagsStore.SetSummarizer(BcTags.TagXid, (untaggedCbor, _) =>
        {
            var xid = XID.FromUntaggedCbor(untaggedCbor);
            return $"XID({xid.ShortDescription()})";
        });

        tagsStore.SetSummarizer(BcTags.TagUri, (untaggedCbor, _) =>
        {
            var uri = URI.FromUntaggedCbor(untaggedCbor);
            return $"URI({uri})";
        });

        tagsStore.SetSummarizer(BcTags.TagUuid, (untaggedCbor, _) =>
        {
            var uuid = UUID.FromUntaggedCbor(untaggedCbor);
            return $"UUID({uuid})";
        });

        tagsStore.SetSummarizer(BcTags.TagNonce, (untaggedCbor, _) =>
        {
            Nonce.FromUntaggedCbor(untaggedCbor);
            return "Nonce";
        });

        tagsStore.SetSummarizer(BcTags.TagSalt, (untaggedCbor, _) =>
        {
            Salt.FromUntaggedCbor(untaggedCbor);
            return "Salt";
        });

        tagsStore.SetSummarizer(BcTags.TagSeed, (untaggedCbor, _) =>
        {
            Seed.FromUntaggedCbor(untaggedCbor);
            return "Seed";
        });

        tagsStore.SetSummarizer(BcTags.TagSymmetricKey, (untaggedCbor, _) =>
            SymmetricKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagX25519PrivateKey, (untaggedCbor, _) =>
            X25519PrivateKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagX25519PublicKey, (untaggedCbor, _) =>
            X25519PublicKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagEcKey, (untaggedCbor, _) =>
        {
            // EC key maps use key 2 to distinguish private from public.
            var map = untaggedCbor.TryIntoMap();
            var isPrivateCbor = map.GetValue(Cbor.FromInt(2));
            if (isPrivateCbor is not null)
                return ECPrivateKey.FromUntaggedCbor(untaggedCbor).ToString();
            return ECPublicKey.FromUntaggedCbor(untaggedCbor).ToString();
        });

        tagsStore.SetSummarizer(BcTags.TagPrivateKeys, (untaggedCbor, _) =>
            PrivateKeys.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagPublicKeys, (untaggedCbor, _) =>
            PublicKeys.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagReference, (untaggedCbor, _) =>
            Reference.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagEncryptedKey, (untaggedCbor, _) =>
            EncryptedKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagPrivateKeyBase, (untaggedCbor, _) =>
            PrivateKeyBase.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagSigningPrivateKey, (untaggedCbor, _) =>
            SigningPrivateKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagSigningPublicKey, (untaggedCbor, _) =>
            SigningPublicKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagSignature, (untaggedCbor, _) =>
        {
            var signature = Signature.FromUntaggedCbor(untaggedCbor);
            var scheme = signature.Scheme();
            return scheme == SignatureScheme.Schnorr
                ? "Signature"
                : $"Signature({scheme})";
        });

        tagsStore.SetSummarizer(BcTags.TagSealedMessage, (untaggedCbor, _) =>
        {
            var sealedMessage = SealedMessage.FromUntaggedCbor(untaggedCbor);
            var scheme = sealedMessage.Scheme;
            return scheme == EncapsulationScheme.X25519
                ? "SealedMessage"
                : $"SealedMessage({scheme})";
        });

        tagsStore.SetSummarizer(BcTags.TagSskrShare, (untaggedCbor, _) =>
        {
            SSKRShare.FromUntaggedCbor(untaggedCbor);
            return "SSKRShare";
        });

        tagsStore.SetSummarizer(BcTags.TagMldsaPrivateKey, (untaggedCbor, _) =>
            MLDSAPrivateKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagMldsaPublicKey, (untaggedCbor, _) =>
            MLDSAPublicKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagMldsaSignature, (untaggedCbor, _) =>
            MLDSASignature.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagMlkemPrivateKey, (untaggedCbor, _) =>
            MLKEMPrivateKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagMlkemPublicKey, (untaggedCbor, _) =>
            MLKEMPublicKey.FromUntaggedCbor(untaggedCbor).ToString());

        tagsStore.SetSummarizer(BcTags.TagMlkemCiphertext, (untaggedCbor, _) =>
            MLKEMCiphertext.FromUntaggedCbor(untaggedCbor).ToString());
    }

    /// <summary>
    /// Registers all Blockchain Commons tags and component summarizers in
    /// dCBOR's global tag store.
    /// </summary>
    /// <remarks>
    /// Call this once at application startup to enable tag name resolution and
    /// summarization in diagnostic output formatting.
    /// </remarks>
    public static void RegisterTags()
    {
        GlobalTags.WithTagsMut(RegisterTagsIn);
    }
}
