package bcenvelope

// Tests corresponding to Rust inline tests in source files:
// - envelope.rs: test_any_*
// - expression.rs: test_expression_*
// - request.rs: test_basic_request, test_request_with_metadata, test_parameter_format
// - response.rs: test_success_ok, test_success_result, test_early_failure, test_failure
// - event.rs: test_event
// - seal.rs: test_seal_and_unseal, test_seal_opt_with_options

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// --- envelope.rs inline tests ---

func TestAnyEnvelope(t *testing.T) {
	e1 := newLeaf(dcbor.NewCBORText("Hello"))
	e2 := NewEnvelope("Hello")
	if e1.Format() != e2.Format() {
		t.Errorf("format mismatch: %q vs %q", e1.Format(), e2.Format())
	}
	if !e1.Digest().Equal(e2.Digest()) {
		t.Error("digest mismatch")
	}
}

func TestAnyKnownValue(t *testing.T) {
	kv := knownvalues.NewKnownValue(100)
	e1 := newWithKnownValue(kv)
	e2 := NewEnvelope(kv)
	if e1.Format() != e2.Format() {
		t.Errorf("format mismatch: %q vs %q", e1.Format(), e2.Format())
	}
	if !e1.Digest().Equal(e2.Digest()) {
		t.Error("digest mismatch")
	}
}

func TestAnyAssertion(t *testing.T) {
	assertion := NewAssertion(EnvelopeEncodableString{"knows"}, EnvelopeEncodableString{"Bob"})
	e1 := newWithAssertion(assertion)
	e2 := NewEnvelope(assertion)
	if e1.Format() != e2.Format() {
		t.Errorf("format mismatch: %q vs %q", e1.Format(), e2.Format())
	}
	if !e1.Digest().Equal(e2.Digest()) {
		t.Error("digest mismatch")
	}
}

func TestAnyCompressed(t *testing.T) {
	data := []byte("Hello")
	digest := bccomponents.DigestFromImage(data)
	compressed := bccomponents.CompressedFromData(data, &digest)
	e1, err := newWithCompressed(&compressed)
	if err != nil {
		t.Fatalf("newWithCompressed: %v", err)
	}
	e2 := NewCompressedEnvelope(&compressed)
	if e1.Format() != e2.Format() {
		t.Errorf("format mismatch: %q vs %q", e1.Format(), e2.Format())
	}
	if !e1.Digest().Equal(e2.Digest()) {
		t.Error("digest mismatch")
	}
}

func TestAnyCBOREncodable(t *testing.T) {
	e1 := newLeaf(dcbor.MustFromAny(1))
	e2 := NewEnvelope(1)
	if e1.Format() != e2.Format() {
		t.Errorf("format mismatch: %q vs %q", e1.Format(), e2.Format())
	}
	if !e1.Digest().Equal(e2.Digest()) {
		t.Error("digest mismatch")
	}
}

// --- expression.rs inline tests ---

func TestExpression1(t *testing.T) {
	RegisterTags()

	expression := NewExpression(FunctionAdd).
		WithParameter(ParameterLHS, NewEnvelope(2)).
		WithParameter(ParameterRHS, NewEnvelope(3))

	envelope := expression.ToEnvelope()

	expected := "«add» [\n    ❰lhs❱: 2\n    ❰rhs❱: 3\n]"
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := ExpressionFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ExpressionFromEnvelope: %v", err)
	}

	lhsVal, err := ExtractObjectForParameter(parsed, ParameterLHS, func(c dcbor.CBOR) (int, error) {
		v, ok := c.AsInt64()
		if !ok {
			return 0, ErrInvalidFormat
		}
		return int(v), nil
	})
	if err != nil {
		t.Fatalf("extract LHS: %v", err)
	}
	if lhsVal != 2 {
		t.Errorf("expected LHS=2, got %d", lhsVal)
	}

	rhsVal, err := ExtractObjectForParameter(parsed, ParameterRHS, func(c dcbor.CBOR) (int, error) {
		v, ok := c.AsInt64()
		if !ok {
			return 0, ErrInvalidFormat
		}
		return int(v), nil
	})
	if err != nil {
		t.Fatalf("extract RHS: %v", err)
	}
	if rhsVal != 3 {
		t.Errorf("expected RHS=3, got %d", rhsVal)
	}

	if !parsed.Function().Equal(expression.Function()) {
		t.Error("function mismatch")
	}
	if !parsed.ExpressionEnvelope().IsIdenticalTo(expression.ExpressionEnvelope()) {
		t.Error("expression envelope mismatch")
	}
	if !parsed.Equal(expression) {
		t.Error("expression equality mismatch")
	}
}

func TestExpression2(t *testing.T) {
	RegisterTags()

	expression := NewExpressionFromString("foo").
		WithParameter(NewNamedParameter("bar"), NewEnvelope("baz")).
		WithOptionalParameter(NewNamedParameter("qux"), nil)

	envelope := expression.ToEnvelope()

	expected := "«\"foo\"» [\n    ❰\"bar\"❱: \"baz\"\n]"
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := ExpressionFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ExpressionFromEnvelope: %v", err)
	}

	barVal, err := ExtractObjectForParameter(parsed, NewNamedParameter("bar"), func(c dcbor.CBOR) (string, error) {
		return c.TryIntoText()
	})
	if err != nil {
		t.Fatalf("extract bar: %v", err)
	}
	if barVal != "baz" {
		t.Errorf("expected bar=baz, got %s", barVal)
	}

	quxVal, err := ExtractOptionalObjectForParameter(parsed, NewNamedParameter("qux"), func(c dcbor.CBOR) (int, error) {
		v, ok := c.AsInt64()
		if !ok {
			return 0, ErrInvalidFormat
		}
		return int(v), nil
	})
	if err != nil {
		t.Fatalf("extract qux: %v", err)
	}
	if quxVal != nil {
		t.Errorf("expected qux=nil, got %v", *quxVal)
	}

	if !parsed.Function().Equal(expression.Function()) {
		t.Error("function mismatch")
	}
	if !parsed.ExpressionEnvelope().IsIdenticalTo(expression.ExpressionEnvelope()) {
		t.Error("expression envelope mismatch")
	}
	if !parsed.Equal(expression) {
		t.Error("expression equality mismatch")
	}
}

// --- request.rs inline tests ---

func testRequestID() bccomponents.ARID {
	return bccomponents.ARIDFromHex("c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc")
}

func TestBasicRequest(t *testing.T) {
	RegisterTags()

	request := NewRequestFromString("test", testRequestID()).
		WithParameter(NewNamedParameter("param1"), NewEnvelope(42)).
		WithParameter(NewNamedParameter("param2"), NewEnvelope("hello"))

	envelope := request.ToEnvelope()
	expected := `request(ARID(c66be27d)) [
    'body': «"test"» [
        ❰"param1"❱: 42
        ❰"param2"❱: "hello"
    ]
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := RequestFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("RequestFromEnvelope: %v", err)
	}

	param1Obj, err := parsed.ObjectForParameter(NewNamedParameter("param1"))
	if err != nil {
		t.Fatalf("param1: %v", err)
	}
	param1Val, err := ExtractSubjectInt(param1Obj)
	if err != nil {
		t.Fatalf("extract param1: %v", err)
	}
	if param1Val != 42 {
		t.Errorf("expected param1=42, got %d", param1Val)
	}

	param2Obj, err := parsed.ObjectForParameter(NewNamedParameter("param2"))
	if err != nil {
		t.Fatalf("param2: %v", err)
	}
	param2Val, err := ExtractSubjectString(param2Obj)
	if err != nil {
		t.Fatalf("extract param2: %v", err)
	}
	if param2Val != "hello" {
		t.Errorf("expected param2=hello, got %s", param2Val)
	}

	if parsed.Note() != "" {
		t.Errorf("expected empty note, got %q", parsed.Note())
	}
	if parsed.Date() != nil {
		t.Error("expected nil date")
	}

	if !request.Equal(parsed) {
		t.Error("request equality mismatch")
	}
}

func TestRequestWithMetadata(t *testing.T) {
	RegisterTags()

	requestDate, err := dcbor.DateFromString("2024-07-04T11:11:11Z")
	if err != nil {
		t.Fatalf("DateFromString: %v", err)
	}

	request := NewRequestFromString("test", testRequestID()).
		WithParameter(NewNamedParameter("param1"), NewEnvelope(42)).
		WithParameter(NewNamedParameter("param2"), NewEnvelope("hello")).
		WithNote("This is a test").
		WithDate(requestDate)

	envelope := request.ToEnvelope()
	expected := `request(ARID(c66be27d)) [
    'body': «"test"» [
        ❰"param1"❱: 42
        ❰"param2"❱: "hello"
    ]
    'date': 2024-07-04T11:11:11Z
    'note': "This is a test"
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := RequestFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("RequestFromEnvelope: %v", err)
	}

	param1Obj, err := parsed.ObjectForParameter(NewNamedParameter("param1"))
	if err != nil {
		t.Fatalf("param1: %v", err)
	}
	param1Val, err := ExtractSubjectInt(param1Obj)
	if err != nil {
		t.Fatalf("extract param1: %v", err)
	}
	if param1Val != 42 {
		t.Errorf("expected param1=42, got %d", param1Val)
	}

	if parsed.Note() != "This is a test" {
		t.Errorf("expected note='This is a test', got %q", parsed.Note())
	}
	if parsed.Date() == nil || !parsed.Date().Equal(requestDate) {
		t.Error("date mismatch")
	}

	if !request.Equal(parsed) {
		t.Error("request equality mismatch")
	}
}

func TestParameterFormat(t *testing.T) {
	RegisterTags()

	param := NewNamedParameter("testParam")
	envelope := EnvelopeFromCBOR(param.TaggedCBOR())
	expected := `❰"testParam"❱`
	assertActualExpected(t, envelope.Format(), expected)
}

// --- response.rs inline tests ---

func TestSuccessOk(t *testing.T) {
	RegisterTags()

	response := NewSuccessResponse(testRequestID())
	envelope := response.ToEnvelope()

	expected := `response(ARID(c66be27d)) [
    'result': 'OK'
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := ResponseFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ResponseFromEnvelope: %v", err)
	}
	if !parsed.IsSuccess() {
		t.Error("expected success")
	}
	if parsed.ExpectID() != testRequestID() {
		t.Error("ID mismatch")
	}

	resultEnv, err := parsed.Result()
	if err != nil {
		t.Fatalf("Result: %v", err)
	}
	resultKV, err := ExtractSubjectKnownValue(resultEnv)
	if err != nil {
		t.Fatalf("extract result: %v", err)
	}
	if !resultKV.Equal(knownvalues.OKValue) {
		t.Error("expected OK known value")
	}

	if !response.Equal(parsed) {
		t.Error("response equality mismatch")
	}
}

func TestSuccessResult(t *testing.T) {
	RegisterTags()

	response := NewSuccessResponse(testRequestID()).
		WithResult(NewEnvelope("It works!"))
	envelope := response.ToEnvelope()

	expected := `response(ARID(c66be27d)) [
    'result': "It works!"
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := ResponseFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ResponseFromEnvelope: %v", err)
	}
	if !parsed.IsSuccess() {
		t.Error("expected success")
	}
	if parsed.ExpectID() != testRequestID() {
		t.Error("ID mismatch")
	}

	resultEnv, err := parsed.Result()
	if err != nil {
		t.Fatalf("Result: %v", err)
	}
	resultStr, err := ExtractSubjectString(resultEnv)
	if err != nil {
		t.Fatalf("extract result: %v", err)
	}
	if resultStr != "It works!" {
		t.Errorf("expected 'It works!', got %q", resultStr)
	}

	if !response.Equal(parsed) {
		t.Error("response equality mismatch")
	}
}

func TestEarlyFailure(t *testing.T) {
	RegisterTags()

	response := NewEarlyFailureResponse()
	envelope := response.ToEnvelope()

	expected := `response('Unknown') [
    'error': 'Unknown'
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := ResponseFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ResponseFromEnvelope: %v", err)
	}
	if !parsed.IsError() {
		t.Error("expected error")
	}
	if parsed.ID() != nil {
		t.Error("expected nil ID")
	}

	errorEnv, err := parsed.Error()
	if err != nil {
		t.Fatalf("Error: %v", err)
	}
	errorKV, err := ExtractSubjectKnownValue(errorEnv)
	if err != nil {
		t.Fatalf("extract error: %v", err)
	}
	if !errorKV.Equal(knownvalues.UnknownValue) {
		t.Error("expected Unknown known value")
	}

	if !response.Equal(parsed) {
		t.Error("response equality mismatch")
	}
}

func TestFailure(t *testing.T) {
	RegisterTags()

	response := NewFailureResponse(testRequestID()).
		WithError(NewEnvelope("It doesn't work!"))
	envelope := response.ToEnvelope()

	expected := `response(ARID(c66be27d)) [
    'error': "It doesn't work!"
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := ResponseFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ResponseFromEnvelope: %v", err)
	}
	if !parsed.IsError() {
		t.Error("expected error")
	}
	if parsed.ID() == nil || *parsed.ID() != testRequestID() {
		t.Error("ID mismatch")
	}

	errorEnv, err := parsed.Error()
	if err != nil {
		t.Fatalf("Error: %v", err)
	}
	errorStr, err := ExtractSubjectString(errorEnv)
	if err != nil {
		t.Fatalf("extract error: %v", err)
	}
	if errorStr != "It doesn't work!" {
		t.Errorf("expected 'It doesn't work!', got %q", errorStr)
	}

	if !response.Equal(parsed) {
		t.Error("response equality mismatch")
	}
}

// --- event.rs inline tests ---

func TestEvent(t *testing.T) {
	RegisterTags()

	eventDate, err := dcbor.DateFromString("2024-07-04T11:11:11Z")
	if err != nil {
		t.Fatalf("DateFromString: %v", err)
	}

	event := NewEventFromString("test", testRequestID()).
		WithNote("This is a test").
		WithDate(eventDate)

	envelope := event.ToEnvelope()
	expected := `event(ARID(c66be27d)) [
    'content': "test"
    'date': 2024-07-04T11:11:11Z
    'note': "This is a test"
]`
	assertActualExpected(t, envelope.Format(), expected)

	parsed, err := EventFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("EventFromEnvelope: %v", err)
	}

	contentStr, err := ExtractSubjectString(parsed.Content())
	if err != nil {
		t.Fatalf("extract content: %v", err)
	}
	if contentStr != "test" {
		t.Errorf("expected content='test', got %q", contentStr)
	}
	if parsed.Note() != "This is a test" {
		t.Errorf("expected note='This is a test', got %q", parsed.Note())
	}
	if parsed.Date() == nil || !parsed.Date().Equal(eventDate) {
		t.Error("date mismatch")
	}
}

// --- seal.rs inline tests ---

func TestSealAndUnseal(t *testing.T) {
	message := "Top secret message"
	originalEnvelope := NewEnvelope(message)

	senderSigner := alicePrivateKey().Ed25519SigningPrivateKey()
	senderVerifier := senderSigner.PublicKey()

	// Get encapsulation keys from bob
	recipientEncrypter := bobPublicKey().EncapsulationPublicKey()
	recipientDecrypter := bobPrivateKey().SchnorrPrivateKeys().EncapsulationPrivateKey()

	sealedEnvelope := originalEnvelope.Seal(senderSigner, recipientEncrypter)

	if !sealedEnvelope.IsSubjectEncrypted() {
		t.Error("expected sealed envelope to have encrypted subject")
	}

	unsealedEnvelope, err := sealedEnvelope.Unseal(senderVerifier, recipientDecrypter)
	if err != nil {
		t.Fatalf("Unseal: %v", err)
	}

	extractedMessage, err := ExtractSubjectString(unsealedEnvelope)
	if err != nil {
		t.Fatalf("extract message: %v", err)
	}
	if extractedMessage != message {
		t.Errorf("expected %q, got %q", message, extractedMessage)
	}
}

func TestSealOptWithOptions(t *testing.T) {
	message := "Confidential data"
	originalEnvelope := NewEnvelope(message)

	senderSigner := alicePrivateKey().Ed25519SigningPrivateKey()
	senderVerifier := senderSigner.PublicKey()

	recipientEncrypter := bobPublicKey().EncapsulationPublicKey()
	recipientDecrypter := bobPrivateKey().SchnorrPrivateKeys().EncapsulationPrivateKey()

	options := &bccomponents.SigningOptions{
		SSHNamespace: "test",
	}

	sealedEnvelope := originalEnvelope.SealOpt(senderSigner, recipientEncrypter, options)

	if !sealedEnvelope.IsSubjectEncrypted() {
		t.Error("expected sealed envelope to have encrypted subject")
	}

	unsealedEnvelope, err := sealedEnvelope.Unseal(senderVerifier, recipientDecrypter)
	if err != nil {
		t.Fatalf("Unseal: %v", err)
	}

	extractedMessage, err := ExtractSubjectString(unsealedEnvelope)
	if err != nil {
		t.Fatalf("extract message: %v", err)
	}
	if extractedMessage != message {
		t.Errorf("expected %q, got %q", message, extractedMessage)
	}
}
