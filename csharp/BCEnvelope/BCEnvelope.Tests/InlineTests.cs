using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;
using Xunit;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Inline unit tests for individual source module functionality.
/// </summary>
public sealed class InlineTests
{
    // ===================================================================
    // Section 1: base/envelope.rs — Constructor equivalence tests
    // ===================================================================

    [Fact]
    public void TestAnyEnvelope()
    {
        var e1 = Envelope.CreateLeaf(Cbor.FromString("Hello"));
        var e2 = Envelope.Create("Hello");
        Assert.Equal(e1.Format(), e2.Format());
        Assert.Equal(e1.GetDigest(), e2.GetDigest());
    }

    [Fact]
    public void TestAnyKnownValue()
    {
        var knownValue = new KnownValue(100);
        var e1 = Envelope.CreateWithKnownValue(knownValue);
        var e2 = Envelope.Create(knownValue);
        Assert.Equal(e1.Format(), e2.Format());
        Assert.Equal(e1.GetDigest(), e2.GetDigest());
    }

    [Fact]
    public void TestAnyAssertion()
    {
        var predEnv = Envelope.Create("knows");
        var objEnv = Envelope.Create("Bob");
        var assertion = new Assertion(predEnv, objEnv);
        var e1 = Envelope.CreateWithAssertion(assertion);
        var e2 = Envelope.Create(assertion);
        Assert.Equal(e1.Format(), e2.Format());
        Assert.Equal(e1.GetDigest(), e2.GetDigest());
    }

    [Fact]
    public void TestAnyEncrypted()
    {
        // The reference test is empty (todo), so we verify the constructor
        // path exists but leave the test as a no-op.
    }

    [Fact]
    public void TestAnyCompressed()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("Hello");
        var digest = Digest.FromImage(data);
        var compressed = Compressed.FromDecompressedData(data, digest);
        var e1 = Envelope.CreateWithCompressed(compressed);
        var e2 = Envelope.Create(compressed);
        Assert.Equal(e1.Format(), e2.Format());
        Assert.Equal(e1.GetDigest(), e2.GetDigest());
    }

    [Fact]
    public void TestAnyCborEncodable()
    {
        var e1 = Envelope.CreateLeaf(Cbor.FromInt(1));
        var e2 = Envelope.Create(1);
        Assert.Equal(e1.Format(), e2.Format());
        Assert.Equal(e1.GetDigest(), e2.GetDigest());
    }

    // ===================================================================
    // Section 2: extension/expressions/expression.rs — Expression tests
    // ===================================================================

    [Fact]
    public void TestExpression1()
    {
        TagsRegistry.RegisterTags();

        var expression = new Expression(Functions.Add)
            .WithParameter(Parameters.Lhs, 2)
            .WithParameter(Parameters.Rhs, 3);

        var envelope = expression.ToEnvelope();

        var expected =
            "\u00ABadd\u00BB [\n" +
            "    \u2770lhs\u2771: 2\n" +
            "    \u2770rhs\u2771: 3\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedExpression = Expression.FromEnvelope(envelope);

        Assert.Equal(2, parsedExpression.ExtractObjectForParameter<int>(Parameters.Lhs));
        Assert.Equal(3, parsedExpression.ExtractObjectForParameter<int>(Parameters.Rhs));

        Assert.Equal(expression.Function, parsedExpression.Function);
        Assert.True(expression.ExpressionEnvelope.IsEquivalentTo(parsedExpression.ExpressionEnvelope));
        Assert.Equal(expression, parsedExpression);
    }

    [Fact]
    public void TestExpression2()
    {
        TagsRegistry.RegisterTags();

        var expression = new Expression("foo")
            .WithParameter(Parameter.NewNamed("bar"), "baz")
            .WithOptionalParameter(Parameter.NewNamed("qux"), null);

        var envelope = expression.ToEnvelope();

        var expected =
            "\u00AB\"foo\"\u00BB [\n" +
            "    \u2770\"bar\"\u2771: \"baz\"\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedExpression = Expression.FromEnvelope(envelope);

        Assert.Equal("baz", parsedExpression.ExtractObjectForParameter<string>(Parameter.NewNamed("bar")));
        // The "qux" parameter was not added (value was null), so it should not exist.
        // C# value types return default(T) from ExtractOptionalObjectForPredicate,
        // so we verify by checking there are no objects for the parameter.
        Assert.Empty(parsedExpression.ObjectsForParameter(Parameter.NewNamed("qux")));

        Assert.Equal(expression.Function, parsedExpression.Function);
        Assert.True(expression.ExpressionEnvelope.IsEquivalentTo(parsedExpression.ExpressionEnvelope));
        Assert.Equal(expression, parsedExpression);
    }

    [Fact]
    public void TestTryAs()
    {
        TagsRegistry.RegisterTags();

        var textEnvelope = Envelope.Create("Hello");
        Assert.Equal("Hello", textEnvelope.TryAs<string>());

        var expression = new Expression(Functions.Add)
            .WithParameter(Parameters.Lhs, 2)
            .WithParameter(Parameters.Rhs, 3);

        var parsedExpression = expression.ToEnvelope().TryAs<Expression>();

        Assert.Equal(expression.Function, parsedExpression.Function);
        Assert.Equal(2, parsedExpression.ExtractObjectForParameter<int>(Parameters.Lhs));
        Assert.Equal(3, parsedExpression.ExtractObjectForParameter<int>(Parameters.Rhs));
        Assert.Equal(expression, parsedExpression);
    }

    [Fact]
    public void TestTryObjectHelpers()
    {
        TagsRegistry.RegisterTags();

        var singleExpression = new Expression(Functions.Add)
            .WithParameter(Parameters.Lhs, 2)
            .WithParameter(Parameters.Rhs, 3);

        var singleEnvelope = Envelope.Create("container")
            .AddAssertion("expr", singleExpression.ToEnvelope());

        var parsedSingle = singleEnvelope.TryObjectForPredicate<Expression>("expr");
        Assert.Equal(singleExpression, parsedSingle);
        Assert.Null(singleEnvelope.TryOptionalObjectForPredicate<Expression>("missing"));

        var firstExpression = new Expression(Functions.Add)
            .WithParameter(Parameters.Lhs, 5)
            .WithParameter(Parameters.Rhs, 8);
        var secondExpression = new Expression(Functions.Sub)
            .WithParameter(Parameters.Lhs, 9)
            .WithParameter(Parameters.Rhs, 4);

        var multiEnvelope = Envelope.Create("container")
            .AddAssertion("expr", firstExpression.ToEnvelope())
            .AddAssertion("expr", secondExpression.ToEnvelope());

        var parsedExpressions = multiEnvelope.TryObjectsForPredicate<Expression>("expr");

        Assert.Equal(2, parsedExpressions.Count);
        Assert.Contains(firstExpression, parsedExpressions);
        Assert.Contains(secondExpression, parsedExpressions);
    }

    // ===================================================================
    // Section 3: extension/expressions/request.rs — Request tests
    // ===================================================================

    private static ARID RequestId()
    {
        return ARID.FromData(Convert.FromHexString(
            "c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc"));
    }

    [Fact]
    public void TestBasicRequest()
    {
        TagsRegistry.RegisterTags();

        var request = new Request("test", RequestId())
            .WithParameter(Parameter.NewNamed("param1"), 42)
            .WithParameter(Parameter.NewNamed("param2"), "hello");

        var envelope = request.ToEnvelope();

        var expected =
            "request(ARID(c66be27d)) [\n" +
            "    'body': \u00AB\"test\"\u00BB [\n" +
            "        \u2770\"param1\"\u2771: 42\n" +
            "        \u2770\"param2\"\u2771: \"hello\"\n" +
            "    ]\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedRequest = Request.FromEnvelope(envelope);
        Assert.Equal(42, parsedRequest.ExtractObjectForParameter<int>(Parameter.NewNamed("param1")));
        Assert.Equal("hello", parsedRequest.ExtractObjectForParameter<string>(Parameter.NewNamed("param2")));
        Assert.Equal("", parsedRequest.Note);
        // The reference checks date() == None. In C#, CborDate is a struct so
        // ExtractOptionalObjectForPredicate returns default(CborDate) rather
        // than null. Verify no date assertion exists on the envelope instead.
        Assert.Null(envelope.OptionalAssertionWithPredicate(KnownValuesRegistry.Date));
    }

    [Fact]
    public void TestRequestWithMetadata()
    {
        TagsRegistry.RegisterTags();

        var requestDate = CborDate.FromString("2024-07-04T11:11:11Z");
        var request = new Request("test", RequestId())
            .WithParameter(Parameter.NewNamed("param1"), 42)
            .WithParameter(Parameter.NewNamed("param2"), "hello")
            .WithNote("This is a test")
            .WithDate(requestDate);

        var envelope = request.ToEnvelope();

        var expected =
            "request(ARID(c66be27d)) [\n" +
            "    'body': \u00AB\"test\"\u00BB [\n" +
            "        \u2770\"param1\"\u2771: 42\n" +
            "        \u2770\"param2\"\u2771: \"hello\"\n" +
            "    ]\n" +
            "    'date': 2024-07-04T11:11:11Z\n" +
            "    'note': \"This is a test\"\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedRequest = Request.FromEnvelope(envelope);
        Assert.Equal(42, parsedRequest.ExtractObjectForParameter<int>(Parameter.NewNamed("param1")));
        Assert.Equal("hello", parsedRequest.ExtractObjectForParameter<string>(Parameter.NewNamed("param2")));
        Assert.Equal("This is a test", parsedRequest.Note);
        Assert.Equal(requestDate, parsedRequest.Date);

        Assert.Equal(request, parsedRequest);
    }

    [Fact]
    public void TestParameterFormat()
    {
        TagsRegistry.RegisterTags();

        var parameter = Parameter.NewNamed("testParam");
        var envelope = Envelope.Create(parameter.TaggedCbor());
        var expected = "\u2770\"testParam\"\u2771";
        Assert.Equal(expected, envelope.Format());
    }

    // ===================================================================
    // Section 4: extension/expressions/response.rs — Response tests
    // ===================================================================

    [Fact]
    public void TestSuccessOk()
    {
        TagsRegistry.RegisterTags();

        var response = Response.NewSuccess(RequestId());
        var envelope = response.ToEnvelope();

        var expected =
            "response(ARID(c66be27d)) [\n" +
            "    'result': 'OK'\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedResponse = Response.FromEnvelope(envelope);
        Assert.True(parsedResponse.IsSuccess);
        Assert.Equal(RequestId(), parsedResponse.ExpectId());
        Assert.Equal(KnownValuesRegistry.OkValue, parsedResponse.ExtractResult<KnownValue>());
        Assert.Equal(response, parsedResponse);
    }

    [Fact]
    public void TestSuccessResult()
    {
        TagsRegistry.RegisterTags();

        var response = Response.NewSuccess(RequestId()).WithResult("It works!");
        var envelope = response.ToEnvelope();

        var expected =
            "response(ARID(c66be27d)) [\n" +
            "    'result': \"It works!\"\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedResponse = Response.FromEnvelope(envelope);
        Assert.True(parsedResponse.IsSuccess);
        Assert.Equal(RequestId(), parsedResponse.ExpectId());
        Assert.Equal("It works!", parsedResponse.ExtractResult<string>());
        Assert.Equal(response, parsedResponse);
    }

    [Fact]
    public void TestEarlyFailure()
    {
        TagsRegistry.RegisterTags();

        var response = Response.NewEarlyFailure();
        var envelope = response.ToEnvelope();

        var expected =
            "response('Unknown') [\n" +
            "    'error': 'Unknown'\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedResponse = Response.FromEnvelope(envelope);
        Assert.True(parsedResponse.IsFailure);
        Assert.Null(parsedResponse.Id);
        Assert.Equal(KnownValuesRegistry.UnknownValue, parsedResponse.ExtractError<KnownValue>());
        Assert.Equal(response, parsedResponse);
    }

    [Fact]
    public void TestFailure()
    {
        TagsRegistry.RegisterTags();

        var response = Response.NewFailure(RequestId()).WithError("It doesn't work!");
        var envelope = response.ToEnvelope();

        var expected =
            "response(ARID(c66be27d)) [\n" +
            "    'error': \"It doesn't work!\"\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedResponse = Response.FromEnvelope(envelope);
        Assert.True(parsedResponse.IsFailure);
        Assert.Equal(RequestId(), parsedResponse.Id);
        Assert.Equal("It doesn't work!", parsedResponse.ExtractError<string>());
        Assert.Equal(response, parsedResponse);
    }

    // ===================================================================
    // Section 5: extension/expressions/event.rs — Event test
    // ===================================================================

    [Fact]
    public void TestEvent()
    {
        TagsRegistry.RegisterTags();

        var eventDate = CborDate.FromString("2024-07-04T11:11:11Z");
        var ev = Event.OfString("test", RequestId())
            .WithNote("This is a test")
            .WithDate(eventDate);

        var envelope = ev.ToEnvelope();

        var expected =
            "event(ARID(c66be27d)) [\n" +
            "    'content': \"test\"\n" +
            "    'date': 2024-07-04T11:11:11Z\n" +
            "    'note': \"This is a test\"\n" +
            "]";
        Assert.Equal(expected, envelope.Format());

        var parsedEvent = Event.StringFromEnvelope(envelope);
        Assert.Equal("test", parsedEvent.Content);
        Assert.Equal("This is a test", parsedEvent.Note);
        Assert.Equal(eventDate, parsedEvent.Date);
    }

    // ===================================================================
    // Section 6: extension/sskr.rs — SSKR split and join inline test
    // ===================================================================

    [Fact]
    public void TestSskrSplitAndJoin()
    {
        // Create the original envelope with an assertion
        var original = Envelope.Create("Secret message")
            .AddAssertion("metadata", "This is a test");

        // Create a content key
        var contentKey = SymmetricKey.New();

        // Wrap the envelope (so the whole envelope including its assertions
        // become the subject)
        var wrappedOriginal = original.Wrap();

        // Encrypt the wrapped envelope
        var encrypted = wrappedOriginal.EncryptSubject(contentKey);

        // Create a 2-of-3 SSKR split specification
        var group = BlockchainCommons.SSKR.GroupSpec.Create(2, 3);
        var spec = BlockchainCommons.SSKR.Spec.Create(1, new[] { group });

        // Split the encrypted envelope into shares
        var shares = encrypted.SskrSplit(spec, contentKey);
        Assert.Equal(3, shares[0].Count);

        // The shares would normally be distributed to different people/places
        // For recovery, we need at least the threshold number of shares (2 in
        // this case)
        var share1 = shares[0][0];
        var share2 = shares[0][1];

        // Combine the shares to recover the original decrypted envelope
        var recoveredWrapped = Envelope.SskrJoin(new[] { share1, share2 });

        // Unwrap the envelope to get the original envelope
        var recovered = recoveredWrapped.TryUnwrap();

        // Check that the recovered envelope matches the original
        Assert.True(recovered.IsIdenticalTo(original));
    }

    // ===================================================================
    // Section 7: seal.rs — Seal/unseal tests
    // ===================================================================

    [Fact]
    public void TestSealAndUnseal()
    {
        // Create a test envelope
        var message = "Top secret message";
        var originalEnvelope = Envelope.Create(message);

        // Generate keys for sender and recipient using established schemes
        var (senderPrivate, senderPublic) = SignatureScheme.Ed25519.Keypair();
        var (recipientPrivate, recipientPublic) = EncapsulationScheme.X25519.Keypair();

        // Step 1: Seal the envelope
        var sealedEnvelope = originalEnvelope.Seal(senderPrivate, recipientPublic);

        // Verify the envelope is encrypted
        Assert.True(sealedEnvelope.IsSubjectEncrypted);

        // Step 2: Unseal the envelope
        var unsealedEnvelope = sealedEnvelope.Unseal(senderPublic, recipientPrivate);

        // Verify we got back the original message
        var extractedMessage = unsealedEnvelope.ExtractSubject<string>();
        Assert.Equal(message, extractedMessage);
    }

    [Fact]
    public void TestSealOptWithOptions()
    {
        // Create a test envelope
        var message = "Confidential data";
        var originalEnvelope = Envelope.Create(message);

        // Generate keys for sender and recipient
        var (senderPrivate, senderPublic) = SignatureScheme.Ed25519.Keypair();
        var (recipientPrivate, recipientPublic) = EncapsulationScheme.X25519.Keypair();

        // Create signing options
        var options = new SigningOptions.SshOptions("test", "sha512");

        // Seal the envelope with options
        var sealedEnvelope = originalEnvelope.SealOpt(senderPrivate, recipientPublic, options);

        // Verify the envelope is encrypted
        Assert.True(sealedEnvelope.IsSubjectEncrypted);

        // Unseal the envelope
        var unsealedEnvelope = sealedEnvelope.Unseal(senderPublic, recipientPrivate);

        // Verify we got back the original message
        var extractedMessage = unsealedEnvelope.ExtractSubject<string>();
        Assert.Equal(message, extractedMessage);
    }
}
