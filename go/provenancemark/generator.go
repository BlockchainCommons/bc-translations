package provenancemark

import (
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"

	bcenvelope "github.com/nickel-blockchaincommons/bcenvelope-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// ProvenanceMarkGenerator produces sequential provenance marks.
type ProvenanceMarkGenerator struct {
	res      ProvenanceMarkResolution
	seed     ProvenanceSeed
	chainID  []byte
	nextSeq  uint32
	rngState RngState
}

// NewProvenanceMarkGenerator constructs a generator from explicit state.
func NewProvenanceMarkGenerator(
	res ProvenanceMarkResolution,
	seed ProvenanceSeed,
	chainID []byte,
	nextSeq uint32,
	rngState RngState,
) (ProvenanceMarkGenerator, error) {
	if len(chainID) != res.LinkLength() {
		return ProvenanceMarkGenerator{}, newInvalidChainIDLength(res.LinkLength(), len(chainID))
	}
	return ProvenanceMarkGenerator{
		res:      res,
		seed:     seed,
		chainID:  cloneBytes(chainID),
		nextSeq:  nextSeq,
		rngState: rngState,
	}, nil
}

// NewProvenanceMarkGeneratorWithSeed derives a generator from the given seed.
func NewProvenanceMarkGeneratorWithSeed(res ProvenanceMarkResolution, seed ProvenanceSeed) ProvenanceMarkGenerator {
	seedBytes := seed.Bytes()
	digest1 := SHA256(seedBytes[:])
	chainID := cloneBytes(digest1[:res.LinkLength()])
	digest2 := SHA256(digest1[:])
	generator, err := NewProvenanceMarkGenerator(res, seed, chainID, 0, RngStateFromBytes(digest2))
	if err != nil {
		panic(err)
	}
	return generator
}

// NewProvenanceMarkGeneratorWithPassphrase derives a generator from a passphrase.
func NewProvenanceMarkGeneratorWithPassphrase(res ProvenanceMarkResolution, passphrase string) ProvenanceMarkGenerator {
	return NewProvenanceMarkGeneratorWithSeed(res, NewProvenanceSeedWithPassphrase(passphrase))
}

// NewProvenanceMarkGeneratorUsing derives a generator from randomness.
func NewProvenanceMarkGeneratorUsing(res ProvenanceMarkResolution, rng bcrand.RandomNumberGenerator) ProvenanceMarkGenerator {
	return NewProvenanceMarkGeneratorWithSeed(res, NewProvenanceSeedUsing(rng))
}

// NewRandomProvenanceMarkGenerator creates a generator using secure randomness.
func NewRandomProvenanceMarkGenerator(res ProvenanceMarkResolution) ProvenanceMarkGenerator {
	return NewProvenanceMarkGeneratorWithSeed(res, NewProvenanceSeed())
}

// Res returns the generator resolution.
func (g ProvenanceMarkGenerator) Res() ProvenanceMarkResolution {
	return g.res
}

// Seed returns the generator seed.
func (g ProvenanceMarkGenerator) Seed() ProvenanceSeed {
	return g.seed
}

// ChainID returns the chain identifier.
func (g ProvenanceMarkGenerator) ChainID() []byte {
	return cloneBytes(g.chainID)
}

// NextSeq returns the next sequence number that will be generated.
func (g ProvenanceMarkGenerator) NextSeq() uint32 {
	return g.nextSeq
}

// RngState returns the current deterministic RNG state.
func (g ProvenanceMarkGenerator) RngState() RngState {
	return g.rngState
}

// Next emits the next mark and mutates the generator state.
func (g *ProvenanceMarkGenerator) Next(date dcbor.Date, info any) ProvenanceMark {
	if g == nil {
		panic("nil ProvenanceMarkGenerator")
	}
	rng := Xoshiro256StarStarFromData(g.rngState.Bytes())

	seq := g.nextSeq
	g.nextSeq++

	var key []byte
	if seq == 0 {
		key = cloneBytes(g.chainID)
	} else {
		key = rng.NextBytes(g.res.LinkLength())
		g.rngState = RngStateFromBytes(rng.Data())
	}

	nextRng := rng.Clone()
	nextKey := nextRng.NextBytes(g.res.LinkLength())

	mark, err := NewProvenanceMark(g.res, key, nextKey, g.chainID, seq, date, info)
	if err != nil {
		panic(err)
	}
	return mark
}

// String returns the display form.
func (g ProvenanceMarkGenerator) String() string {
	return fmt.Sprintf(
		"ProvenanceMarkGenerator(chainID: %s, res: %s, seed: %s, nextSeq: %d, rngState: %s)",
		hex.EncodeToString(g.chainID),
		g.res,
		g.seed.Hex(),
		g.nextSeq,
		g.rngState.Hex(),
	)
}

// Equal reports whether two generators have identical state.
func (g ProvenanceMarkGenerator) Equal(other ProvenanceMarkGenerator) bool {
	return g.res == other.res &&
		g.seed == other.seed &&
		compareBytes(g.chainID, other.chainID) == 0 &&
		g.nextSeq == other.nextSeq &&
		g.rngState == other.rngState
}

// ToEnvelope converts the generator to an envelope.
func (g ProvenanceMarkGenerator) ToEnvelope() *bcenvelope.Envelope {
	return bcenvelope.NewEnvelope(dcbor.ToByteString(g.chainID)).
		AddType("provenance-generator").
		AddAssertion("res", g.res.ToCBOR()).
		AddAssertion("seed", g.seed.ToCBOR()).
		AddAssertion("next-seq", uint64(g.nextSeq)).
		AddAssertion("rng-state", g.rngState.ToCBOR())
}

// ProvenanceMarkGeneratorFromEnvelope decodes a generator from an envelope.
func ProvenanceMarkGeneratorFromEnvelope(envelope *bcenvelope.Envelope) (ProvenanceMarkGenerator, error) {
	if envelope == nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(errors.New("nil envelope"))
	}
	if err := envelope.CheckType("provenance-generator"); err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	chainID, err := envelope.Subject().TryByteString()
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	const expectedKeyCount = 5
	if len(envelope.Assertions()) != expectedKeyCount {
		return ProvenanceMarkGenerator{}, newExtraKeys(expectedKeyCount, len(envelope.Assertions()))
	}

	resLeaf, err := envelope.ObjectForPredicate("res")
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	resCBOR, err := resLeaf.TryLeaf()
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	res, err := DecodeProvenanceMarkResolution(resCBOR)
	if err != nil {
		return ProvenanceMarkGenerator{}, err
	}

	seedLeaf, err := envelope.ObjectForPredicate("seed")
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	seedCBOR, err := seedLeaf.TryLeaf()
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	seed, err := DecodeProvenanceSeed(seedCBOR)
	if err != nil {
		return ProvenanceMarkGenerator{}, err
	}

	nextSeqLeaf, err := envelope.ObjectForPredicate("next-seq")
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	nextSeqCBOR, err := nextSeqLeaf.TryLeaf()
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	nextSeq, err := dcbor.DecodeUInt32(nextSeqCBOR)
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapCBORError(err)
	}

	rngStateLeaf, err := envelope.ObjectForPredicate("rng-state")
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	rngStateCBOR, err := rngStateLeaf.TryLeaf()
	if err != nil {
		return ProvenanceMarkGenerator{}, wrapEnvelopeError(err)
	}
	rngState, err := DecodeRngState(rngStateCBOR)
	if err != nil {
		return ProvenanceMarkGenerator{}, err
	}

	return NewProvenanceMarkGenerator(res, seed, chainID, nextSeq, rngState)
}

type provenanceMarkGeneratorJSON struct {
	Res      ProvenanceMarkResolution `json:"res"`
	Seed     ProvenanceSeed           `json:"seed"`
	ChainID  string                   `json:"chainID"`
	NextSeq  uint32                   `json:"nextSeq"`
	RngState RngState                 `json:"rngState"`
}

// MarshalJSON encodes the generator in its public JSON form.
func (g ProvenanceMarkGenerator) MarshalJSON() ([]byte, error) {
	return json.Marshal(provenanceMarkGeneratorJSON{
		Res:      g.res,
		Seed:     g.seed,
		ChainID:  SerializeBase64(g.chainID),
		NextSeq:  g.nextSeq,
		RngState: g.rngState,
	})
}

// UnmarshalJSON decodes the generator from its public JSON form.
func (g *ProvenanceMarkGenerator) UnmarshalJSON(data []byte) error {
	var payload provenanceMarkGeneratorJSON
	if err := json.Unmarshal(data, &payload); err != nil {
		return err
	}
	chainID, err := DeserializeBase64(payload.ChainID)
	if err != nil {
		return err
	}
	decoded, err := NewProvenanceMarkGenerator(payload.Res, payload.Seed, chainID, payload.NextSeq, payload.RngState)
	if err != nil {
		return err
	}
	*g = decoded
	return nil
}

var _ json.Marshaler = ProvenanceMarkGenerator{}
var _ json.Unmarshaler = (*ProvenanceMarkGenerator)(nil)
