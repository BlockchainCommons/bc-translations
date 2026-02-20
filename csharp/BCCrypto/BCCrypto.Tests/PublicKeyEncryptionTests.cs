using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCCrypto.Tests;

public class PublicKeyEncryptionTests
{
    [Fact]
    public void TestX25519Keys()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        byte[] privateKey = PublicKeyEncryption.X25519NewPrivateKeyUsing(rng);
        Assert.Equal(
            Convert.FromHexString("7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"),
            privateKey);

        byte[] publicKey = PublicKeyEncryption.X25519PublicKeyFromPrivateKey(privateKey);
        Assert.Equal(
            Convert.FromHexString("f1bd7a7e118ea461eba95126a3efef543ebb78439d1574bedcbe7d89174cf025"),
            publicKey);

        byte[] derivedAgreementKey = PublicKeyEncryption.DeriveAgreementPrivateKey("password"u8);
        Assert.Equal(
            Convert.FromHexString("7b19769132648ff43ae60cbaa696d5be3f6d53e6645db72e2d37516f0729619f"),
            derivedAgreementKey);

        byte[] derivedSigningKey = PublicKeyEncryption.DeriveSigningPrivateKey("password"u8);
        Assert.Equal(
            Convert.FromHexString("05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b"),
            derivedSigningKey);
    }

    [Fact]
    public void TestKeyAgreement()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();

        byte[] alicePrivateKey = PublicKeyEncryption.X25519NewPrivateKeyUsing(rng);
        byte[] alicePublicKey = PublicKeyEncryption.X25519PublicKeyFromPrivateKey(alicePrivateKey);

        byte[] bobPrivateKey = PublicKeyEncryption.X25519NewPrivateKeyUsing(rng);
        byte[] bobPublicKey = PublicKeyEncryption.X25519PublicKeyFromPrivateKey(bobPrivateKey);

        byte[] aliceSharedKey = PublicKeyEncryption.X25519SharedKey(alicePrivateKey, bobPublicKey);
        byte[] bobSharedKey = PublicKeyEncryption.X25519SharedKey(bobPrivateKey, alicePublicKey);

        Assert.Equal(aliceSharedKey, bobSharedKey);
        Assert.Equal(
            Convert.FromHexString("1e9040d1ff45df4bfca7ef2b4dd2b11101b40d91bf5bf83f8c83d53f0fbb6c23"),
            aliceSharedKey);
    }
}
