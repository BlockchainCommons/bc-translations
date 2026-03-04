package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const DigestSize = 32

// Digest is a cryptographically secure SHA-256 digest (32 bytes).
type Digest struct {
	data [DigestSize]byte
}

// DigestFromData creates a Digest from a 32-byte array.
func DigestFromData(data [DigestSize]byte) Digest {
	return Digest{data: data}
}

// DigestFromDataRef creates a Digest from a byte slice, validating its length.
func DigestFromDataRef(data []byte) (Digest, error) {
	if len(data) != DigestSize {
		return Digest{}, errInvalidSize("digest", DigestSize, len(data))
	}
	var d Digest
	copy(d.data[:], data)
	return d, nil
}

// DigestFromImage creates a Digest by computing the SHA-256 hash of the data.
func DigestFromImage(image []byte) Digest {
	return DigestFromData(bccrypto.SHA256(image))
}

// DigestFromImageParts creates a Digest by concatenating parts and hashing.
func DigestFromImageParts(parts [][]byte) Digest {
	var buf []byte
	for _, part := range parts {
		buf = append(buf, part...)
	}
	return DigestFromImage(buf)
}

// DigestFromDigests creates a Digest by concatenating digests and hashing.
func DigestFromDigests(digests []Digest) Digest {
	var buf []byte
	for _, d := range digests {
		buf = append(buf, d.data[:]...)
	}
	return DigestFromImage(buf)
}

// DigestFromHex creates a Digest from a hex string. Panics if invalid.
func DigestFromHex(h string) Digest {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid digest hex: %v", err))
	}
	d, err := DigestFromDataRef(data)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: %v", err))
	}
	return d
}

// Data returns the 32-byte digest data.
func (d Digest) Data() [DigestSize]byte { return d.data }

// Bytes returns the digest as a byte slice.
func (d Digest) Bytes() []byte { return d.data[:] }

// Validate checks whether this digest matches the SHA-256 hash of the image.
func (d Digest) Validate(image []byte) bool {
	return d == DigestFromImage(image)
}

// ValidateOpt validates data against an optional digest. Returns true if the
// digest is nil or if it matches the image.
func ValidateOpt(image []byte, digest *Digest) bool {
	if digest == nil {
		return true
	}
	return digest.Validate(image)
}

// Hex returns the digest as a 64-character hex string.
func (d Digest) Hex() string { return hex.EncodeToString(d.data[:]) }

// ShortDescription returns the first 4 bytes as an 8-character hex string.
func (d Digest) ShortDescription() string { return hex.EncodeToString(d.data[:4]) }

// String returns a human-readable representation.
func (d Digest) String() string { return fmt.Sprintf("Digest(%s)", d.Hex()) }

// Equal reports whether two digests are equal.
func (d Digest) Equal(other Digest) bool { return d.data == other.data }

// Digest implements DigestProvider by returning itself.
func (d Digest) Digest() Digest { return d }

// --- CBOR support ---

// DigestCBORTags returns the CBOR tags used for Digest.
func DigestCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagDigest})
}

// CBORTags implements dcbor.CBORTagged.
func (d Digest) CBORTags() []dcbor.Tag { return DigestCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (d Digest) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(d.data[:]) }

// TaggedCBOR returns the tagged CBOR encoding of the Digest.
func (d Digest) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(d)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (d Digest) ToCBOR() dcbor.CBOR { return d.TaggedCBOR() }

// DecodeDigest decodes a Digest from untagged CBOR.
func DecodeDigest(cbor dcbor.CBOR) (Digest, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return Digest{}, err
	}
	return DigestFromDataRef(data)
}

// DecodeTaggedDigest decodes a Digest from tagged CBOR.
func DecodeTaggedDigest(cbor dcbor.CBOR) (Digest, error) {
	return dcbor.DecodeTagged(cbor, DigestCBORTags(), DecodeDigest)
}

// DigestFromTaggedCBOR decodes a Digest from tagged CBOR bytes.
func DigestFromTaggedCBOR(data []byte) (Digest, error) {
	return dcbor.DecodeTaggedData(data, DigestCBORTags(), DecodeDigest)
}

// --- UR support ---

// DigestToURString encodes a Digest as a UR string.
func DigestToURString(d Digest) string { return bcur.ToURString(d) }

// DigestFromURString decodes a Digest from a UR string.
func DigestFromURString(urString string) (Digest, error) {
	return bcur.DecodeURString(urString, DigestCBORTags(), DecodeDigest)
}
