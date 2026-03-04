package bccomponents

import (
	"bytes"
	"compress/flate"
	"encoding/hex"
	"fmt"
	"hash/crc32"
	"io"
	"math"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Compressed is a compressed binary object with CRC32 integrity verification
// and optional cryptographic digest.
type Compressed struct {
	checksum         uint32
	decompressedSize int
	compressedData   []byte
	digest           *Digest
}

// NewCompressed creates a Compressed from pre-compressed data (low-level).
func NewCompressed(checksum uint32, decompressedSize int, compressedData []byte, digest *Digest) (Compressed, error) {
	if len(compressedData) > decompressedSize {
		return Compressed{}, errCompression("compressed data is larger than decompressed size")
	}
	cp := make([]byte, len(compressedData))
	copy(cp, compressedData)
	return Compressed{
		checksum:         checksum,
		decompressedSize: decompressedSize,
		compressedData:   cp,
		digest:           digest,
	}, nil
}

// CompressedFromData creates a Compressed by compressing the provided data
// using raw DEFLATE at level 6.
func CompressedFromData(decompressedData []byte, digest *Digest) Compressed {
	checksum := crc32.ChecksumIEEE(decompressedData)
	decompressedSize := len(decompressedData)

	var buf bytes.Buffer
	w, _ := flate.NewWriter(&buf, 6)
	w.Write(decompressedData)
	w.Close()
	compressedData := buf.Bytes()

	if len(compressedData) != 0 && len(compressedData) < decompressedSize {
		return Compressed{
			checksum:         checksum,
			decompressedSize: decompressedSize,
			compressedData:   compressedData,
			digest:           digest,
		}
	}
	cp := make([]byte, decompressedSize)
	copy(cp, decompressedData)
	return Compressed{
		checksum:         checksum,
		decompressedSize: decompressedSize,
		compressedData:   cp,
		digest:           digest,
	}
}

// Decompress returns the original decompressed data, verifying the CRC32 checksum.
func (c Compressed) Decompress() ([]byte, error) {
	if len(c.compressedData) >= c.decompressedSize {
		cp := make([]byte, len(c.compressedData))
		copy(cp, c.compressedData)
		return cp, nil
	}
	r := flate.NewReader(bytes.NewReader(c.compressedData))
	defer r.Close()
	data, err := io.ReadAll(r)
	if err != nil {
		return nil, errCompression("corrupt compressed data")
	}
	if crc32.ChecksumIEEE(data) != c.checksum {
		return nil, errCompression("compressed data checksum mismatch")
	}
	return data, nil
}

// CompressedSize returns the size of the compressed data.
func (c Compressed) CompressedSize() int { return len(c.compressedData) }

// CompressionRatio returns compressed/decompressed size ratio.
func (c Compressed) CompressionRatio() float64 {
	if c.decompressedSize == 0 {
		return math.NaN()
	}
	return float64(len(c.compressedData)) / float64(c.decompressedSize)
}

// DigestOpt returns the optional digest, or nil.
func (c Compressed) DigestOpt() *Digest { return c.digest }

// HasDigest returns whether this compressed data has an associated digest.
func (c Compressed) HasDigest() bool { return c.digest != nil }

// Digest implements DigestProvider. Panics if no digest is set.
func (c Compressed) Digest() Digest {
	if c.digest == nil {
		panic("bccomponents: Compressed has no digest")
	}
	return *c.digest
}

// String returns a human-readable representation.
func (c Compressed) String() string {
	digestStr := "None"
	if c.digest != nil {
		digestStr = c.digest.ShortDescription()
	}
	return fmt.Sprintf("Compressed(checksum: %s, size: %d/%d, ratio: %.2f, digest: %s)",
		hex.EncodeToString([]byte{byte(c.checksum >> 24), byte(c.checksum >> 16), byte(c.checksum >> 8), byte(c.checksum)}),
		c.CompressedSize(), c.decompressedSize, c.CompressionRatio(), digestStr)
}

// Equal reports whether two Compressed values are equal.
func (c Compressed) Equal(other Compressed) bool {
	if c.checksum != other.checksum || c.decompressedSize != other.decompressedSize {
		return false
	}
	if !bytes.Equal(c.compressedData, other.compressedData) {
		return false
	}
	if c.digest == nil && other.digest == nil {
		return true
	}
	if c.digest == nil || other.digest == nil {
		return false
	}
	return c.digest.Equal(*other.digest)
}

// --- CBOR support ---

func CompressedCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagCompressed})
}

func (c Compressed) CBORTags() []dcbor.Tag { return CompressedCBORTags() }

func (c Compressed) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(uint64(c.checksum)),
		dcbor.MustFromAny(uint64(c.decompressedSize)),
		dcbor.ToByteString(c.compressedData),
	}
	if c.digest != nil {
		elements = append(elements, c.digest.TaggedCBOR())
	}
	return dcbor.NewCBORArray(elements)
}

func (c Compressed) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(c)
	return cbor
}

func (c Compressed) ToCBOR() dcbor.CBOR { return c.TaggedCBOR() }

func DecodeCompressed(cbor dcbor.CBOR) (Compressed, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return Compressed{}, err
	}
	if len(elements) < 3 || len(elements) > 4 {
		return Compressed{}, dcbor.NewErrorf("invalid number of elements in compressed")
	}

	checksumU64, err := elements[0].TryIntoUInt64()
	if err != nil {
		return Compressed{}, err
	}
	sizeU64, err := elements[1].TryIntoUInt64()
	if err != nil {
		return Compressed{}, err
	}
	compressedData, err := elements[2].TryIntoByteString()
	if err != nil {
		return Compressed{}, err
	}

	var digest *Digest
	if len(elements) == 4 {
		d, err := DecodeTaggedDigest(elements[3])
		if err != nil {
			return Compressed{}, err
		}
		digest = &d
	}

	return NewCompressed(uint32(checksumU64), int(sizeU64), compressedData, digest)
}

func DecodeTaggedCompressed(cbor dcbor.CBOR) (Compressed, error) {
	return dcbor.DecodeTagged(cbor, CompressedCBORTags(), DecodeCompressed)
}
