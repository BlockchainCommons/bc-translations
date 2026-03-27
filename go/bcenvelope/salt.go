package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// AddSalt adds a proportionally-sized salt assertion to decorrelate the
// envelope. The size of the salt is proportional to the envelope's serialized
// size. This changes the digest while preserving semantic content, preventing
// correlation between envelopes containing the same information.
func (e *Envelope) AddSalt() *Envelope {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return e.AddSaltUsing(rng)
}

// AddSaltUsing adds a proportionally-sized salt assertion using the provided
// random number generator. Primarily used for deterministic testing.
func (e *Envelope) AddSaltUsing(rng bcrand.RandomNumberGenerator) *Envelope {
	salt := bccomponents.NewSaltForSizeUsing(
		len(e.TaggedCBOR().ToCBORData()),
		rng,
	)
	return e.AddSaltInstance(salt)
}

// AddSaltInstance adds the given Salt as an assertion to the envelope, using
// the known value 'salt' as the predicate.
func (e *Envelope) AddSaltInstance(salt bccomponents.Salt) *Envelope {
	return e.AddAssertion(knownvalues.Salt, salt)
}

// AddSaltWithLen adds salt of a specific byte length to the envelope. The
// count must be at least 8 bytes (64 bits) to ensure sufficient entropy.
func (e *Envelope) AddSaltWithLen(count int) (*Envelope, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return e.AddSaltWithLenUsing(count, rng)
}

// AddSaltWithLenUsing adds salt of a specific byte length using the provided
// random number generator. The count must be at least 8 bytes.
func (e *Envelope) AddSaltWithLenUsing(count int, rng bcrand.RandomNumberGenerator) (*Envelope, error) {
	salt, err := bccomponents.NewSaltWithLenUsing(count, rng)
	if err != nil {
		return nil, err
	}
	return e.AddSaltInstance(salt), nil
}

// AddSaltInRange adds salt with a byte length randomly chosen from the given
// inclusive range [min, max]. The minimum must be at least 8.
func (e *Envelope) AddSaltInRange(min, max int) (*Envelope, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return e.AddSaltInRangeUsing(min, max, rng)
}

// AddSaltInRangeUsing adds salt with a byte length randomly chosen from the
// given inclusive range using the provided random number generator.
func (e *Envelope) AddSaltInRangeUsing(min, max int, rng bcrand.RandomNumberGenerator) (*Envelope, error) {
	salt, err := bccomponents.NewSaltInRangeUsing(min, max, rng)
	if err != nil {
		return nil, err
	}
	return e.AddSaltInstance(salt), nil
}
