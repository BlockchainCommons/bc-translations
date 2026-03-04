package bccomponents

import (
	"bytes"
	"fmt"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const MinSeedLength = 16

// Seed is a cryptographic seed for deterministic key generation, with optional
// metadata (name, note, creation date).
type Seed struct {
	data         []byte
	name         string
	note         string
	creationDate *dcbor.Date
}

// NewSeed creates a new random seed with the default length (16 bytes).
func NewSeed() *Seed {
	s, _ := NewSeedWithLen(MinSeedLength)
	return s
}

// NewSeedWithLen creates a new random seed with the specified length.
// The length must be at least 16.
func NewSeedWithLen(count int) (*Seed, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewSeedWithLenUsing(count, rng)
}

// NewSeedWithLenUsing creates a new random seed using the provided RNG.
func NewSeedWithLenUsing(count int, rng bcrand.RandomNumberGenerator) (*Seed, error) {
	data := bcrand.RandomDataFrom(rng, count)
	return NewSeedOpt(data, "", "", nil)
}

// NewSeedOpt creates a seed from data and optional metadata.
// The data must be at least 16 bytes.
func NewSeedOpt(data []byte, name, note string, creationDate *dcbor.Date) (*Seed, error) {
	if len(data) < MinSeedLength {
		return nil, errDataTooShort("seed", MinSeedLength, len(data))
	}
	cp := make([]byte, len(data))
	copy(cp, data)
	return &Seed{
		data:         cp,
		name:         name,
		note:         note,
		creationDate: creationDate,
	}, nil
}

// Bytes returns the seed data.
func (s *Seed) Bytes() []byte {
	cp := make([]byte, len(s.data))
	copy(cp, s.data)
	return cp
}

// Name returns the seed's name.
func (s *Seed) Name() string { return s.name }

// SetName sets the seed's name.
func (s *Seed) SetName(name string) { s.name = name }

// Note returns the seed's note.
func (s *Seed) Note() string { return s.note }

// SetNote sets the seed's note.
func (s *Seed) SetNote(note string) { s.note = note }

// CreationDate returns the seed's creation date, or nil.
func (s *Seed) CreationDate() *dcbor.Date { return s.creationDate }

// SetCreationDate sets the seed's creation date.
func (s *Seed) SetCreationDate(d *dcbor.Date) { s.creationDate = d }

// PrivateKeyData implements PrivateKeyDataProvider.
func (s *Seed) PrivateKeyData() []byte { return s.Bytes() }

// String returns a human-readable representation.
func (s *Seed) String() string { return fmt.Sprintf("Seed(%d bytes)", len(s.data)) }

// Equal reports whether two seeds are equal.
func (s *Seed) Equal(other *Seed) bool {
	if s == nil || other == nil {
		return s == other
	}
	if !bytes.Equal(s.data, other.data) {
		return false
	}
	if s.name != other.name || s.note != other.note {
		return false
	}
	if s.creationDate == nil && other.creationDate == nil {
		return true
	}
	if s.creationDate == nil || other.creationDate == nil {
		return false
	}
	return s.creationDate.Equal(*other.creationDate)
}

// --- CBOR support ---

// SeedCBORTags returns the CBOR tags used for Seed.
func SeedCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSeed, bctags.TagSeedV1})
}

// CBORTags implements dcbor.CBORTagged.
func (s *Seed) CBORTags() []dcbor.Tag { return SeedCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (s *Seed) UntaggedCBOR() dcbor.CBOR {
	m := dcbor.NewMap()
	m.InsertAny(1, dcbor.ToByteString(s.data))
	if s.creationDate != nil {
		m.InsertAny(2, s.creationDate.UntaggedCBOR())
	}
	if s.name != "" {
		m.InsertAny(3, dcbor.MustFromAny(s.name))
	}
	if s.note != "" {
		m.InsertAny(4, dcbor.MustFromAny(s.note))
	}
	return dcbor.NewCBORMap(m)
}

// TaggedCBOR returns the tagged CBOR encoding of the Seed.
func (s *Seed) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(s)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (s *Seed) ToCBOR() dcbor.CBOR { return s.TaggedCBOR() }

// DecodeSeed decodes a Seed from untagged CBOR.
func DecodeSeed(cbor dcbor.CBOR) (*Seed, error) {
	m, err := cbor.TryIntoMap()
	if err != nil {
		return nil, err
	}

	dataCBOR, err := m.Extract(dcbor.MustFromAny(int64(1)))
	if err != nil {
		return nil, err
	}
	data, err := dataCBOR.TryIntoByteString()
	if err != nil {
		return nil, err
	}
	if len(data) == 0 {
		return nil, dcbor.NewErrorf("seed data is empty")
	}

	var name, note string
	var creationDate *dcbor.Date

	if dateCBOR, ok := m.Get(dcbor.MustFromAny(int64(2))); ok {
		d, err := dcbor.DateFromUntaggedCBOR(dateCBOR)
		if err != nil {
			return nil, err
		}
		creationDate = &d
	}
	if nameCBOR, ok := m.Get(dcbor.MustFromAny(int64(3))); ok {
		if s, ok := nameCBOR.AsText(); ok {
			name = s
		}
	}
	if noteCBOR, ok := m.Get(dcbor.MustFromAny(int64(4))); ok {
		if s, ok := noteCBOR.AsText(); ok {
			note = s
		}
	}

	return NewSeedOpt(data, name, note, creationDate)
}

// DecodeTaggedSeed decodes a Seed from tagged CBOR.
func DecodeTaggedSeed(cbor dcbor.CBOR) (*Seed, error) {
	return dcbor.DecodeTagged(cbor, SeedCBORTags(), DecodeSeed)
}

// --- UR support ---

// SeedToURString encodes a Seed as a UR string.
func SeedToURString(s *Seed) string { return bcur.ToURString(s) }

// SeedFromURString decodes a Seed from a UR string.
func SeedFromURString(urString string) (*Seed, error) {
	return bcur.DecodeURString(urString, SeedCBORTags(), DecodeSeed)
}
