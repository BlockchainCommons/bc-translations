using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCCrypto.Tests;

public class EcdsaSigningTests
{
    private static readonly byte[] Message = "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it."u8.ToArray();

    [Fact]
    public void TestEcdsaSigning()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        byte[] privateKey = EcdsaKeys.EcdsaNewPrivateKeyUsing(rng);
        byte[] publicKey = EcdsaKeys.EcdsaPublicKeyFromPrivateKey(privateKey);
        byte[] signature = EcdsaSigning.EcdsaSign(privateKey, Message);
        Assert.Equal(
            Convert.FromHexString(
                "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"),
            signature);
        Assert.True(EcdsaSigning.EcdsaVerify(publicKey, signature, Message));
    }
}
