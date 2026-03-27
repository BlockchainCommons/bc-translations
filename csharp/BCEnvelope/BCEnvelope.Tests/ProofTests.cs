using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class ProofTests
{
    [Fact]
    public void TestFriendsList()
    {
        // This document contains a list of people Alice knows. Each "knows"
        // assertion has been salted.
        var aliceFriends = Envelope.Create("Alice")
            .AddAssertionSalted("knows", "Bob", true)
            .AddAssertionSalted("knows", "Carol", true)
            .AddAssertionSalted("knows", "Dan", true);

        var expectedFormat =
            "\"Alice\" [\n" +
            "    {\n" +
            "        \"knows\": \"Bob\"\n" +
            "    } [\n" +
            "        'salt': Salt\n" +
            "    ]\n" +
            "    {\n" +
            "        \"knows\": \"Carol\"\n" +
            "    } [\n" +
            "        'salt': Salt\n" +
            "    ]\n" +
            "    {\n" +
            "        \"knows\": \"Dan\"\n" +
            "    } [\n" +
            "        'salt': Salt\n" +
            "    ]\n" +
            "]";
        Assert.Equal(expectedFormat, aliceFriends.Format());

        // Alice provides just the root digest of her document to a third party.
        var aliceFriendsRoot = aliceFriends.ElideRevealingSet(new HashSet<Digest>());
        Assert.Equal("ELIDED", aliceFriendsRoot.Format());

        // Alice wants to prove she knows Bob.
        var knowsBobAssertion = Envelope.CreateAssertion("knows", "Bob");
        var aliceKnowsBobProof = aliceFriends
            .ProofContainsTarget(knowsBobAssertion)!
            .CheckEncoding();

        var expectedProofFormat =
            "ELIDED [\n" +
            "    ELIDED [\n" +
            "        ELIDED\n" +
            "    ]\n" +
            "    ELIDED (2)\n" +
            "]";
        Assert.Equal(expectedProofFormat, aliceKnowsBobProof.Format());

        // The third party confirms the proof.
        Assert.True(
            aliceFriendsRoot.ConfirmContainsTarget(
                knowsBobAssertion, aliceKnowsBobProof));
    }

    [Fact]
    public void TestMultiPosition()
    {
        var aliceFriends = Envelope.Create("Alice")
            .AddAssertionSalted("knows", "Bob", true)
            .AddAssertionSalted("knows", "Carol", true)
            .AddAssertionSalted("knows", "Dan", true);

        // The target "knows" exists at three positions.
        var knowsProof = aliceFriends
            .ProofContainsTarget(Envelope.Create("knows"))!
            .CheckEncoding();

        var expectedFormat =
            "ELIDED [\n" +
            "    {\n" +
            "        ELIDED: ELIDED\n" +
            "    } [\n" +
            "        ELIDED\n" +
            "    ]\n" +
            "    {\n" +
            "        ELIDED: ELIDED\n" +
            "    } [\n" +
            "        ELIDED\n" +
            "    ]\n" +
            "    {\n" +
            "        ELIDED: ELIDED\n" +
            "    } [\n" +
            "        ELIDED\n" +
            "    ]\n" +
            "]";
        Assert.Equal(expectedFormat, knowsProof.Format());
    }

    [Fact]
    public void TestVerifiableCredential()
    {
        var aliceSeed = new Seed(
            Convert.FromHexString("82f32c855d3d542256180810797e0073"));
        var alicePrivateKey = PrivateKeyBase.FromData(aliceSeed.Data);
        var arid = Envelope.Create(
            ARID.FromData(Convert.FromHexString(
                "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d")));
        var credential = arid
            .AddAssertionSalted("firstName", "John", true)
            .AddAssertionSalted("lastName", "Smith", true)
            .AddAssertionSalted("address", "123 Main St.", true)
            .AddAssertionSalted("birthDate",
                CborDate.FromString("1970-01-01"), true)
            .AddAssertionSalted("photo", "This is John Smith's photo.", true)
            .AddAssertionSalted("dlNumber", "123-456-789", true)
            .AddAssertionSalted("nonCommercialVehicleEndorsement", true, true)
            .AddAssertionSalted("motorocycleEndorsement", true, true)
            .AddAssertion(KnownValuesRegistry.Issuer, "State of Example")
            .AddAssertion(KnownValuesRegistry.Controller, "State of Example")
            .Wrap()
            .AddSignature(alicePrivateKey)
            .AddAssertion(KnownValuesRegistry.Note, "Signed by the State of Example");

        var credentialRoot = credential.ElideRevealingSet(new HashSet<Digest>());

        // Prove a single assertion: the address.
        var addressAssertion = Envelope.CreateAssertion("address", "123 Main St.");
        var addressProof = credential
            .ProofContainsTarget(addressAssertion)!
            .CheckEncoding();

        var expectedProofFormat =
            "{\n" +
            "    ELIDED [\n" +
            "        ELIDED [\n" +
            "            ELIDED\n" +
            "        ]\n" +
            "        ELIDED (9)\n" +
            "    ]\n" +
            "} [\n" +
            "    ELIDED (2)\n" +
            "]";
        Assert.Equal(expectedProofFormat, addressProof.Format());

        // The proof confirms the address, as intended.
        Assert.True(
            credentialRoot.ConfirmContainsTarget(addressAssertion, addressProof));

        // Assertions without salt can also be confirmed.
        var issuerAssertion = Envelope.CreateAssertion(
            KnownValuesRegistry.Issuer, "State of Example");
        Assert.True(
            credentialRoot.ConfirmContainsTarget(issuerAssertion, addressProof));

        // The proof cannot be used to confirm salted assertions.
        var firstNameAssertion = Envelope.CreateAssertion("firstName", "John");
        Assert.False(
            credentialRoot.ConfirmContainsTarget(firstNameAssertion, addressProof));
    }
}
