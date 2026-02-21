package bccrypto

import (
	"bytes"
	"testing"
)

func TestCRC32(t *testing.T) {
	input := []byte("Hello, world!")
	if got := CRC32(input); got != 0xebe6c6e6 {
		t.Fatalf("CRC32() = 0x%x, want 0xebe6c6e6", got)
	}
	if got := CRC32Data(input); got != must4("ebe6c6e6") {
		t.Fatalf("CRC32Data() = %x, want ebe6c6e6", got)
	}
	if got := CRC32DataWithEndian(input, true); got != must4("e6c6e6eb") {
		t.Fatalf("CRC32DataWithEndian(true) = %x, want e6c6e6eb", got)
	}
}

func TestSHA256(t *testing.T) {
	input := []byte("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq")
	expected := must32("248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1")
	if got := SHA256(input); got != expected {
		t.Fatalf("SHA256() = %x, want %x", got, expected)
	}
}

func TestSHA512(t *testing.T) {
	input := []byte("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq")
	expected := must64("204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c33596fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445")
	if got := SHA512(input); got != expected {
		t.Fatalf("SHA512() = %x, want %x", got, expected)
	}
}

func TestHMACSHA(t *testing.T) {
	key := mustLen("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", 20)
	message := []byte("Hi There")

	expected256 := must32("b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7")
	if got := HMACSHA256(key, message); got != expected256 {
		t.Fatalf("HMACSHA256() = %x, want %x", got, expected256)
	}

	expected512 := must64("87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854")
	if got := HMACSHA512(key, message); got != expected512 {
		t.Fatalf("HMACSHA512() = %x, want %x", got, expected512)
	}
}

func TestPBKDF2HMACSHA256(t *testing.T) {
	expected := must32("120fb6cffcf8b32c43e7225256c4f837a86548c92ccc35480805987cb70be17b")
	got := PBKDF2HMACSHA256([]byte("password"), []byte("salt"), 1, 32)
	if !bytes.Equal(got, expected[:]) {
		t.Fatalf("PBKDF2HMACSHA256() = %x, want %x", got, expected)
	}
}

func TestHKDFHMACSHA256(t *testing.T) {
	keyMaterial := []byte("hello")
	salt := mustLen("8e94ef805b93e683ff18", 10)
	expected := must32("13485067e21af17c0900f70d885f02593c0e61e46f86450e4a0201a54c14db76")
	got := HKDFHMACSHA256(keyMaterial, salt, 32)
	if !bytes.Equal(got, expected[:]) {
		t.Fatalf("HKDFHMACSHA256() = %x, want %x", got, expected)
	}
}
