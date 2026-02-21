namespace BlockchainCommons.DCbor.Tests;

public class EncodeTests
{
    // --- Helpers ---

    private static void TestCbor(Cbor cbor, string expectedDebug, string expectedDisplay, string expectedData)
    {
        Assert.Equal(expectedDebug, cbor.DebugDescription);
        Assert.Equal(expectedDisplay, cbor.ToString());
        string data = cbor.Hex();
        Assert.Equal(expectedData, data);
        var decoded = Cbor.TryFromData(cbor.ToCborData());
        Assert.Equal(cbor, decoded);
    }

    private static void TestCborDecode(string hex, string expectedDebug, string expectedDisplay)
    {
        var cbor = Cbor.TryFromHex(hex);
        Assert.Equal(expectedDebug, cbor.DebugDescription);
        Assert.Equal(expectedDisplay, cbor.ToString());
    }

    private static void TestUnsigned(ulong value, string debug, string display, string hex)
    {
        var cbor = Cbor.FromUInt(value);
        TestCbor(cbor, debug, display, hex);
        // Round-trip through decode
        var decoded = Cbor.TryFromData(cbor.ToCborData());
        Assert.Equal(cbor, decoded);
        ulong recovered = decoded.TryIntoUInt64();
        Assert.Equal(value, recovered);
    }

    private static void TestSigned(long value, string debug, string display, string hex)
    {
        var cbor = Cbor.FromInt(value);
        TestCbor(cbor, debug, display, hex);
        var decoded = Cbor.TryFromData(cbor.ToCborData());
        Assert.Equal(cbor, decoded);
        long recovered = decoded.TryIntoInt64();
        Assert.Equal(value, recovered);
    }

    // --- Unsigned ---

    [Fact]
    public void EncodeUnsigned()
    {
        TestUnsigned(0, "unsigned(0)", "0", "00");
        TestUnsigned(1, "unsigned(1)", "1", "01");
        TestUnsigned(23, "unsigned(23)", "23", "17");
        TestUnsigned(24, "unsigned(24)", "24", "1818");
        TestUnsigned(255, "unsigned(255)", "255", "18ff");
        TestUnsigned(65535, "unsigned(65535)", "65535", "19ffff");
        TestUnsigned(65536, "unsigned(65536)", "65536", "1a00010000");
        TestUnsigned(4294967295, "unsigned(4294967295)", "4294967295", "1affffffff");
        TestUnsigned(4294967296, "unsigned(4294967296)", "4294967296", "1b0000000100000000");
        TestUnsigned(ulong.MaxValue, "unsigned(18446744073709551615)", "18446744073709551615", "1bffffffffffffffff");
    }

    // --- Signed ---

    [Fact]
    public void EncodeSigned()
    {
        TestSigned(-1, "negative(-1)", "-1", "20");
        TestSigned(-2, "negative(-2)", "-2", "21");
        TestSigned(-127, "negative(-127)", "-127", "387e");
        TestSigned(-128, "negative(-128)", "-128", "387f");
        TestSigned(127, "unsigned(127)", "127", "187f");
        TestSigned(-32768, "negative(-32768)", "-32768", "397fff");
        TestSigned(32767, "unsigned(32767)", "32767", "197fff");
        TestSigned(-2147483648, "negative(-2147483648)", "-2147483648", "3a7fffffff");
        TestSigned(2147483647, "unsigned(2147483647)", "2147483647", "1a7fffffff");
        TestSigned(long.MinValue, "negative(-9223372036854775808)", "-9223372036854775808", "3b7fffffffffffffff");
        TestSigned(long.MaxValue, "unsigned(9223372036854775807)", "9223372036854775807", "1b7fffffffffffffff");
    }

    // --- Bytes ---

    [Fact]
    public void EncodeBytes()
    {
        var bs = new ByteString(new byte[] { 0x00, 0x11, 0x22, 0x33 });
        var cbor = Cbor.FromByteString(bs);
        TestCbor(cbor, "bytes(00112233)", "h'00112233'", "4400112233");

        // Round-trip
        var decoded = Cbor.TryFromData(cbor.ToCborData());
        Assert.Equal(cbor, decoded);
    }

    [Fact]
    public void EncodeLongBytes()
    {
        var data = Convert.FromHexString("c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7");
        var cbor = Cbor.FromByteString(data);
        TestCbor(cbor,
            "bytes(c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7)",
            "h'c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7'",
            "5820c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7");

        // Additional bytes sub-test from Rust
        var bytes2 = Cbor.FromByteString(new byte[] { 0x11, 0x22, 0x33 });
        TestCbor(bytes2, "bytes(112233)", "h'112233'", "43112233");
    }

    // --- String ---

    [Fact]
    public void EncodeString()
    {
        var cbor = Cbor.FromString("Hello");
        TestCbor(cbor, "text(\"Hello\")", "\"Hello\"", "6548656c6c6f");

        var longStr = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        var longCbor = Cbor.FromString(longStr);
        TestCbor(longCbor,
            $"text(\"{longStr}\")",
            $"\"{longStr}\"",
            "7901bd4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e20557420656e696d206164206d696e696d2076656e69616d2c2071756973206e6f737472756420657865726369746174696f6e20756c6c616d636f206c61626f726973206e69736920757420616c697175697020657820656120636f6d6d6f646f20636f6e7365717561742e2044756973206175746520697275726520646f6c6f7220696e20726570726568656e646572697420696e20766f6c7570746174652076656c697420657373652063696c6c756d20646f6c6f726520657520667567696174206e756c6c612070617269617475722e204578636570746575722073696e74206f6363616563617420637570696461746174206e6f6e2070726f6964656e742c2073756e7420696e2063756c706120717569206f666669636961206465736572756e74206d6f6c6c697420616e696d20696420657374206c61626f72756d2e");
    }

    // --- Normalized string ---

    [Fact]
    public void TestNormalizedString()
    {
        string composedEAcute = "\u00E9";
        string decomposedEAcute = "\u0065\u0301";

        // They're different strings
        Assert.NotEqual(composedEAcute, decomposedEAcute);

        // But serializing as dCBOR yields the same data
        var cbor1 = Cbor.FromString(composedEAcute).ToCborData();
        var cbor2 = Cbor.FromString(decomposedEAcute).ToCborData();
        Assert.Equal(cbor1, cbor2);

        // Non-NFC string should fail to decode
        byte[] nonNfcCborData = { 0x63, 0x65, 0xcc, 0x81 };
        var ex = Assert.Throws<CborNonCanonicalStringException>(() => Cbor.TryFromData(nonNfcCborData));
        Assert.Equal("a CBOR string was not encoded in Unicode Canonical Normalization Form C", ex.Message);
    }

    // --- Array ---

    [Fact]
    public void EncodeArray()
    {
        // Empty array
        var empty = new Cbor(CborCase.Array(new List<Cbor>()));
        TestCbor(empty, "array([])", "[]", "80");

        // [1, 2, 3]
        var arr = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3)
        }));
        TestCbor(arr,
            "array([unsigned(1), unsigned(2), unsigned(3)])",
            "[1, 2, 3]",
            "83010203");

        // [1, -2, 3]
        var mixed = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1), Cbor.FromInt(-2), Cbor.FromInt(3)
        }));
        TestCbor(mixed,
            "array([unsigned(1), negative(-2), unsigned(3)])",
            "[1, -2, 3]",
            "83012103");
    }

    // --- Heterogeneous array ---

    [Fact]
    public void EncodeHeterogeneousArray()
    {
        var arr = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1),
            Cbor.FromString("Hello"),
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3)
            }))
        }));
        TestCbor(arr,
            "array([unsigned(1), text(\"Hello\"), array([unsigned(1), unsigned(2), unsigned(3)])])",
            "[1, \"Hello\", [1, 2, 3]]",
            "83016548656c6c6f83010203");
    }

    // --- Map ---

    [Fact]
    public void EncodeMap()
    {
        var m = new CborMap();
        TestCbor(new Cbor(CborCase.Map(m)), "map({})", "{}", "a0");

        m = new CborMap();
        m.Insert(Cbor.FromInt(-1), Cbor.FromInt(3));
        m.Insert(new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(-1) })), Cbor.FromInt(7));
        m.Insert(Cbor.FromString("z"), Cbor.FromInt(4));
        m.Insert(Cbor.FromInt(10), Cbor.FromInt(1));
        m.Insert(Cbor.FromBool(false), Cbor.FromInt(8));
        m.Insert(Cbor.FromInt(100), Cbor.FromInt(2));
        m.Insert(Cbor.FromString("aa"), Cbor.FromInt(5));
        m.Insert(new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(100) })), Cbor.FromInt(6));

        var mapCbor = new Cbor(CborCase.Map(m));
        Assert.Equal("a80a011864022003617a046261610581186406812007f408", mapCbor.Hex());

        // Map lookups
        Assert.Equal(Cbor.FromInt(8), m.GetValue(Cbor.FromBool(false)));
        Assert.Null(m.GetValue(Cbor.FromBool(true)));
    }

    // --- Map misordered ---

    [Fact]
    public void EncodeMapMisordered()
    {
        var ex = Assert.Throws<CborMisorderedMapKeyException>(() =>
            Cbor.TryFromHex("a2026141016142"));
        Assert.Equal("the decoded CBOR map has keys that are not in canonical order", ex.Message);
    }

    // --- Tagged ---

    [Fact]
    public void EncodeTagged()
    {
        var cbor = Cbor.ToTaggedValue(1, Cbor.FromString("Hello"));
        TestCbor(cbor,
            "tagged(1, text(\"Hello\"))",
            "1(\"Hello\")",
            "c16548656c6c6f");
    }

    // --- Bool ---

    [Fact]
    public void EncodeValue()
    {
        TestCbor(Cbor.FromBool(false), "simple(false)", "false", "f4");
        TestCbor(Cbor.FromBool(true), "simple(true)", "true", "f5");
    }

    // --- Envelope ---

    [Fact]
    public void EncodeEnvelope()
    {
        var alice = Cbor.ToTaggedValue(200, Cbor.ToTaggedValue(201, Cbor.FromString("Alice")));
        var knows = Cbor.ToTaggedValue(200, Cbor.ToTaggedValue(201, Cbor.FromString("knows")));
        var bob = Cbor.ToTaggedValue(200, Cbor.ToTaggedValue(201, Cbor.FromString("Bob")));
        var knowsBob = Cbor.ToTaggedValue(200, Cbor.ToTaggedValue(221,
            new Cbor(CborCase.Array(new List<Cbor> { knows, bob }))));
        var envelope = Cbor.ToTaggedValue(200,
            new Cbor(CborCase.Array(new List<Cbor> { alice, knowsBob })));

        Assert.Equal(
            "200([200(201(\"Alice\")), 200(221([200(201(\"knows\")), 200(201(\"Bob\"))]))])",
            envelope.ToString());
        Assert.Equal(
            "d8c882d8c8d8c965416c696365d8c8d8dd82d8c8d8c9656b6e6f7773d8c8d8c963426f62",
            envelope.Hex());

        var decoded = Cbor.TryFromData(envelope.ToCborData());
        Assert.Equal(envelope, decoded);
    }

    // --- Float ---

    [Fact]
    public void EncodeFloat()
    {
        TestCbor(Cbor.FromDouble(1.5), "simple(1.5)", "1.5", "f93e00");
        TestCbor(Cbor.FromDouble(2345678.25), "simple(2345678.25)", "2345678.25", "fa4a0f2b39");
        TestCbor(Cbor.FromDouble(1.2), "simple(1.2)", "1.2", "fb3ff3333333333333");
        TestCbor(Cbor.FromDouble(double.PositiveInfinity), "simple(inf)", "Infinity", "f97c00");

        // Float-to-integer reduction
        TestCbor(Cbor.FromFloat(42.0f), "unsigned(42)", "42", "182a");
        TestCbor(Cbor.FromDouble(2345678.0), "unsigned(2345678)", "2345678", "1a0023cace");
        TestCbor(Cbor.FromDouble(-2345678.0), "negative(-2345678)", "-2345678", "3a0023cacd");

        // Negative zero -> 0
        TestCbor(Cbor.FromDouble(-0.0), "unsigned(0)", "0", "00");

        // Smallest half-precision subnormal
        TestCbor(Cbor.FromDouble(5.960464477539063e-8), "simple(5.960464477539063e-8)", "5.960464477539063e-8", "f90001");

        // Smallest single subnormal
        TestCbor(Cbor.FromDouble(1.401298464324817e-45), "simple(1.401298464324817e-45)", "1.401298464324817e-45", "fa00000001");

        // Smallest double subnormal
        TestCbor(Cbor.FromDouble(5e-324), "simple(5e-324)", "5e-324", "fb0000000000000001");

        // Smallest double normal
        TestCbor(Cbor.FromDouble(2.2250738585072014e-308), "simple(2.2250738585072014e-308)", "2.2250738585072014e-308", "fb0010000000000000");

        // Smallest half-precision normal
        TestCbor(Cbor.FromDouble(6.103515625e-5), "simple(6.103515625e-5)", "6.103515625e-5", "f90400");

        // Largest possible half-precision -> integer
        TestCbor(Cbor.FromDouble(65504.0), "unsigned(65504)", "65504", "19ffe0");

        // Most negative double that converts to int64
        TestCbor(Cbor.FromDouble(-9223372036854774784.0),
            "negative(-9223372036854774784)", "-9223372036854774784", "3b7ffffffffffffbff");

        // Most negative encoded as 65-bit neg (decode only)
        TestCborDecode("3b8000000000000000",
            "negative(-9223372036854775809)", "-9223372036854775809");

        // Largest double that can convert to uint64
        TestCbor(Cbor.FromDouble(18446744073709550000.0),
            "unsigned(18446744073709549568)", "18446744073709549568", "1bfffffffffffff800");

        // Just too large for uint64
        TestCbor(Cbor.FromDouble(18446744073709552000.0),
            "simple(1.8446744073709552e19)", "1.8446744073709552e19", "fa5f800000");

        // Most negative 65-bit neg float
        TestCbor(Cbor.FromDouble(-18446744073709551616.0),
            "negative(-18446744073709551616)", "-18446744073709551616", "3bffffffffffffffff");

        // Exponent 24 single boundary
        TestCbor(Cbor.FromDouble(33554430.0),
            "unsigned(33554430)", "33554430", "1a01fffffe");

        // Int64 with too much precision to be a float
        TestCbor(Cbor.FromInt(-9223372036854775807L),
            "negative(-9223372036854775807)", "-9223372036854775807", "3b7ffffffffffffffe");

        // Least negative float not representable as Int64
        TestCbor(Cbor.FromDouble(-9223372036854777856.0),
            "negative(-9223372036854777856)", "-9223372036854777856", "3b80000000000007ff");

        // Next to most negative float encodable as 65-bit neg
        TestCbor(Cbor.FromDouble(-18446744073709549568.0),
            "negative(-18446744073709549568)", "-18446744073709549568", "3bfffffffffffff7ff");

        // Most negative encodable as 65-bit neg
        TestCbor(Cbor.FromDouble(-18446744073709551616.0),
            "negative(-18446744073709551616)", "-18446744073709551616", "3bffffffffffffffff");

        // Least negative whole integer that must be encoded as float in dCBOR
        TestCbor(Cbor.FromDouble(-18446744073709555712.0),
            "simple(-1.8446744073709556e19)", "-1.8446744073709556e19", "fbc3f0000000000001");

        // Large negative that converts to negative int
        TestCbor(Cbor.FromDouble(-18446742974197924000.0),
            "negative(-18446742974197923840)", "-18446742974197923840", "3bfffffeffffffffff");

        // Largest possible single
        TestCbor(Cbor.FromDouble(3.4028234663852886e38),
            "simple(3.4028234663852886e38)", "3.4028234663852886e38", "fa7f7fffff");

        // Slightly larger than largest possible single
        TestCbor(Cbor.FromDouble(3.402823466385289e38),
            "simple(3.402823466385289e38)", "3.402823466385289e38", "fb47efffffe0000001");

        // Largest double
        TestCbor(Cbor.FromDouble(1.7976931348623157e308),
            "simple(1.7976931348623157e308)", "1.7976931348623157e308", "fb7fefffffffffffff");

        // 65-bit negative decode-only tests
        TestCborDecode("3bfffffffffffffffe",
            "negative(-18446744073709551615)", "-18446744073709551615");
    }

    // --- NaN ---

    [Fact]
    public void EncodeNan()
    {
        var canonicalNanData = new byte[] { 0xf9, 0x7e, 0x00 };
        Assert.Equal(canonicalNanData, Cbor.FromDouble(double.NaN).ToCborData());
    }

    [Fact]
    public void DecodeNan()
    {
        // Canonical NaN decodes
        var cbor = Cbor.TryFromData(new byte[] { 0xf9, 0x7e, 0x00 });
        Assert.True(cbor.IsNan);

        // Non-canonical NaNs fail
        Assert.ThrowsAny<CborException>(() => Cbor.TryFromData(new byte[] { 0xf9, 0x7e, 0x01 }));
        Assert.ThrowsAny<CborException>(() => Cbor.TryFromData(new byte[] { 0xfa, 0xff, 0xc0, 0x00, 0x01 }));
        Assert.ThrowsAny<CborException>(() => Cbor.TryFromData(new byte[] { 0xfb, 0x7f, 0xf9, 0x10, 0x00, 0x00, 0x00, 0x00, 0x01 }));
    }

    // --- Infinity ---

    [Fact]
    public void EncodeInfinity()
    {
        var posInf = new byte[] { 0xf9, 0x7c, 0x00 };
        var negInf = new byte[] { 0xf9, 0xfc, 0x00 };

        Assert.Equal(posInf, Cbor.FromDouble(double.PositiveInfinity).ToCborData());
        Assert.Equal(negInf, Cbor.FromDouble(double.NegativeInfinity).ToCborData());
    }

    [Fact]
    public void DecodeInfinity()
    {
        var posInf = new byte[] { 0xf9, 0x7c, 0x00 };
        var negInf = new byte[] { 0xf9, 0xfc, 0x00 };

        var a = Cbor.TryFromData(posInf).TryIntoDouble();
        Assert.Equal(double.PositiveInfinity, a);

        var b = Cbor.TryFromData(negInf).TryIntoDouble();
        Assert.Equal(double.NegativeInfinity, b);

        // Non-canonical infinities fail
        Assert.ThrowsAny<CborException>(() => Cbor.TryFromData(new byte[] { 0xfa, 0x7f, 0x80, 0x00, 0x00 }));
        Assert.ThrowsAny<CborException>(() => Cbor.TryFromData(new byte[] { 0xfb, 0x7f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
    }

    // --- Non-canonical float ---

    [Fact]
    public void NonCanonicalFloat1()
    {
        var ex = Assert.Throws<CborNonCanonicalNumericException>(() =>
            Cbor.TryFromHex("FB3FF8000000000000"));
        Assert.Equal("a CBOR numeric value was encoded in non-canonical form", ex.Message);
    }

    [Fact]
    public void NonCanonicalFloat2()
    {
        var ex = Assert.Throws<CborNonCanonicalNumericException>(() =>
            Cbor.TryFromHex("F94A00"));
        Assert.Equal("a CBOR numeric value was encoded in non-canonical form", ex.Message);
    }

    // --- Unused data ---

    [Fact]
    public void UnusedData()
    {
        var ex = Assert.Throws<CborUnusedDataException>(() =>
            Cbor.TryFromHex("0001"));
        Assert.Equal("the decoded CBOR had 1 extra bytes at the end", ex.Message);
    }

    // --- Int coerced to float ---

    [Fact]
    public void IntCoercedToFloat()
    {
        var c = Cbor.FromInt(42);
        double f = c.TryIntoDouble();
        Assert.Equal(42.0, f);
        var c2 = Cbor.FromDouble(f);
        Assert.Equal(c2, c);
        int i = c.TryIntoInt32();
        Assert.Equal(42, i);
    }

    // --- Float cannot be coerced to int ---

    [Fact]
    public void FailFloatCoercedToInt()
    {
        var c = Cbor.FromDouble(42.5);
        double f = c.TryIntoDouble();
        Assert.Equal(42.5, f);
        Assert.Throws<CborWrongTypeException>(() => c.TryIntoInt32());
    }

    // --- Date ---

    [Fact]
    public void EncodeDate()
    {
        GlobalTags.RegisterTags();
        var date = CborDate.FromTimestamp(1675854714.0);
        Cbor cbor = date.TaggedCbor();
        TestCbor(cbor,
            "tagged(date, unsigned(1675854714))",
            "date(1675854714)",
            "c11a63e3837a");
    }

    // --- Usage tests ---

    [Fact]
    public void UsageTest1()
    {
        var array = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1000), Cbor.FromInt(2000), Cbor.FromInt(3000)
        }));
        Assert.Equal("831903e81907d0190bb8", array.Hex());
    }

    [Fact]
    public void UsageTest2()
    {
        byte[] data = Convert.FromHexString("831903e81907d0190bb8");
        var cbor = Cbor.TryFromData(data);
        Assert.Equal("[1000, 2000, 3000]", cbor.Diagnostic());
    }

    // --- Map with map keys ---

    [Fact]
    public void EncodeMapWithMapKeys()
    {
        var k1 = new CborMap();
        k1.Insert(Cbor.FromInt(1), Cbor.FromInt(2));

        var k2 = new CborMap();
        k2.Insert(Cbor.FromInt(3), Cbor.FromInt(4));

        var m = new CborMap();
        m.Insert(new Cbor(CborCase.Map(k1)), Cbor.FromInt(5));
        m.Insert(new Cbor(CborCase.Map(k2)), Cbor.FromInt(6));

        TestCbor(new Cbor(CborCase.Map(m)),
            "map({0xa10102: (map({0x01: (unsigned(1), unsigned(2))}), unsigned(5)), 0xa10304: (map({0x03: (unsigned(3), unsigned(4))}), unsigned(6))})",
            "{{1: 2}: 5, {3: 4}: 6}",
            "a2a1010205a1030406");
    }

    // --- Anders map ---

    [Fact]
    public void EncodeAndersMap()
    {
        var m = new CborMap();
        m.Insert(Cbor.FromInt(1), Cbor.FromDouble(45.7));
        m.Insert(Cbor.FromInt(2), Cbor.FromString("Hi there!"));
        Assert.Equal("a201fb4046d9999999999a0269486920746865726521",
            new Cbor(CborCase.Map(m)).Hex());
    }

    // --- Tag display ---

    [Fact]
    public void TagDisplay()
    {
        var tag1 = new Tag(1, "A");
        Assert.Equal("A", tag1.ToString());

        var tag2 = new Tag(2);
        Assert.Equal("2", tag2.ToString());
    }

    // --- Convert values (round-trip) ---

    [Fact]
    public void ConvertValues()
    {
        // Integer round-trip
        var c1 = Cbor.FromInt(10);
        Assert.Equal(10, c1.TryIntoInt32());
        Assert.Equal(c1, Cbor.FromInt(c1.TryIntoInt32()));

        // Negative integer round-trip
        var c2 = Cbor.FromInt(-10);
        Assert.Equal(-10L, c2.TryIntoInt64());
        Assert.Equal(c2, Cbor.FromInt(c2.TryIntoInt64()));

        // Boolean round-trip
        var c3 = Cbor.FromBool(false);
        Assert.False(c3.TryIntoBool());
        Assert.Equal(c3, Cbor.FromBool(c3.TryIntoBool()));

        // String round-trip
        var c4 = Cbor.FromString("Hello");
        Assert.Equal("Hello", c4.TryIntoText());
        Assert.Equal(c4, Cbor.FromString(c4.TryIntoText()));

        // Float round-trip
        var c5 = Cbor.FromDouble(10.0);
        Assert.Equal(10.0, c5.TryIntoDouble());
        Assert.Equal(c5, Cbor.FromDouble(c5.TryIntoDouble()));

        // ByteString round-trip
        var bs = new ByteString(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 });
        var c6 = Cbor.FromByteString(bs);
        var recovered = c6.TryIntoByteString();
        Assert.Equal(bs.ToArray(), recovered);
    }

    // --- Collection round-trip tests ---

    [Fact]
    public void ConvertDictionary()
    {
        var h = new Dictionary<int, string>
        {
            { 1, "A" },
            { 50, "B" },
            { 25, "C" },
        };
        var cbor = Cbor.FromDictionary(h);
        Assert.Equal("{1: \"A\", 25: \"C\", 50: \"B\"}", cbor.Diagnostic());
        var h2 = cbor.TryIntoDictionaryIntString();
        Assert.Equal(h, h2);
    }

    [Fact]
    public void ConvertSortedDictionary()
    {
        var h = new SortedDictionary<int, string>
        {
            { 1, "A" },
            { 50, "B" },
            { 25, "C" },
        };
        var cbor = Cbor.FromDictionary(h);
        Assert.Equal("{1: \"A\", 25: \"C\", 50: \"B\"}", cbor.Diagnostic());
        var h2 = cbor.TryIntoSortedDictionaryIntString();
        Assert.Equal(h, h2);
    }

    [Fact]
    public void ConvertList()
    {
        var v = new List<int> { 1, 50, 25 };
        var cbor = Cbor.FromIntList(v);
        Assert.Equal("[1, 50, 25]", cbor.Diagnostic());
        var v2 = cbor.TryIntoIntList();
        Assert.Equal(v, v2);
    }

    [Fact]
    public void ConvertHashSet()
    {
        var v = new HashSet<int> { 1, 50, 25 };
        // Convert to CBOR (via sorted list for determinism)
        var sorted = CborSortable.SortByCborEncoding(v.Select(i => Cbor.FromInt(i)));
        var cbor = Cbor.FromList(sorted);
        // Round-trip back to HashSet
        var v2 = cbor.TryIntoHashSetInt();
        Assert.Equal(v, v2);
    }

    // --- Nonstandard NaN encoding ---

    [Fact]
    public void EncodeNonstandardNan()
    {
        // A nonstandard NaN that should be canonicalized to f97e00
        var canonicalNanData = new byte[] { 0xf9, 0x7e, 0x00 };
        var nonstandardF64Nan = BitConverter.Int64BitsToDouble(unchecked((long)0x7ff9100000000001));
        Assert.True(double.IsNaN(nonstandardF64Nan));
        Assert.Equal(canonicalNanData, Cbor.FromDouble(nonstandardF64Nan).ToCborData());

        // Standard double NaN
        Assert.Equal(canonicalNanData, Cbor.FromDouble(double.NaN).ToCborData());

        // Standard float NaN
        Assert.Equal(canonicalNanData, Cbor.FromFloat(float.NaN).ToCborData());
    }

    // --- Encode infinity (encode side) ---

    [Fact]
    public void EncodeInfinityValues()
    {
        var posInf = new byte[] { 0xf9, 0x7c, 0x00 };
        var negInf = new byte[] { 0xf9, 0xfc, 0x00 };

        // f64 infinity
        Assert.Equal(posInf, Cbor.FromDouble(double.PositiveInfinity).ToCborData());
        Assert.Equal(negInf, Cbor.FromDouble(double.NegativeInfinity).ToCborData());

        // f32 infinity
        Assert.Equal(posInf, Cbor.FromFloat(float.PositiveInfinity).ToCborData());
        Assert.Equal(negInf, Cbor.FromFloat(float.NegativeInfinity).ToCborData());
    }
}
