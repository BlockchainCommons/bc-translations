using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCCrypto.Tests;

public class EcdsaKeysTests
{
    [Fact]
    public void TestEcdsaKeys()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        byte[] privateKey = EcdsaKeys.EcdsaNewPrivateKeyUsing(rng);
        Assert.Equal(
            Convert.FromHexString("7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"),
            privateKey);

        byte[] publicKey = EcdsaKeys.EcdsaPublicKeyFromPrivateKey(privateKey);
        Assert.Equal(
            Convert.FromHexString("0271b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"),
            publicKey);

        byte[] decompressed = EcdsaKeys.EcdsaDecompressPublicKey(publicKey);
        Assert.Equal(
            Convert.FromHexString("0471b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b72325f1f3bb69a44d3f1cb6d1fd488220dd502f49c0b1a46cb91ce3718d8334a"),
            decompressed);

        byte[] compressed = EcdsaKeys.EcdsaCompressPublicKey(decompressed);
        Assert.Equal(publicKey, compressed);

        byte[] xOnlyPublicKey = EcdsaKeys.SchnorrPublicKeyFromPrivateKey(privateKey);
        Assert.Equal(
            Convert.FromHexString("71b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"),
            xOnlyPublicKey);

        byte[] derivedPrivateKey = EcdsaKeys.EcdsaDerivePrivateKey("password"u8);
        Assert.Equal(
            Convert.FromHexString("05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b"),
            derivedPrivateKey);
    }
}
