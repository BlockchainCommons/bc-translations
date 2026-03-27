package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
	sskr "github.com/nickel-blockchaincommons/sskr-go"
)

// addSSKRShare returns a new envelope with an added 'sskrShare': SSKRShare
// assertion.
func (e *Envelope) addSSKRShare(share bccomponents.SSKRShare) *Envelope {
	return e.AddAssertion(knownvalues.SSKRShare, share)
}

// SSKRSplit splits the envelope into a set of SSKR shares. The returned
// structure is a nested slice preserving the group structure: each outer
// slice represents a group and each inner slice contains the shares for
// that group. Each share is an envelope containing the original encrypted
// envelope with a unique SSKR share assertion.
func (e *Envelope) SSKRSplit(
	spec *bccomponents.SSKRSpec,
	contentKey bccomponents.SymmetricKey,
) ([][]*Envelope, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return e.SSKRSplitUsing(spec, contentKey, rng)
}

// SSKRSplitFlattened splits the envelope into a flattened set of SSKR shares,
// discarding the group structure.
func (e *Envelope) SSKRSplitFlattened(
	spec *bccomponents.SSKRSpec,
	contentKey bccomponents.SymmetricKey,
) ([]*Envelope, error) {
	groups, err := e.SSKRSplit(spec, contentKey)
	if err != nil {
		return nil, err
	}
	var result []*Envelope
	for _, group := range groups {
		result = append(result, group...)
	}
	return result, nil
}

// SSKRSplitUsing splits the envelope using the provided random number
// generator for deterministic testing.
func (e *Envelope) SSKRSplitUsing(
	spec *bccomponents.SSKRSpec,
	contentKey bccomponents.SymmetricKey,
	rng bcrand.RandomNumberGenerator,
) ([][]*Envelope, error) {
	masterSecret, err := sskr.NewSecret(contentKey.Bytes())
	if err != nil {
		return nil, err
	}
	shares, err := bccomponents.SSKRGenerateUsing(spec, &masterSecret, rng)
	if err != nil {
		return nil, err
	}
	var result [][]*Envelope
	for _, group := range shares {
		var groupResult []*Envelope
		for _, share := range group {
			shareResult := e.addSSKRShare(share)
			groupResult = append(groupResult, shareResult)
		}
		result = append(result, groupResult)
	}
	return result, nil
}

// SSKRJoin reconstructs the original envelope from a set of envelopes
// containing SSKR share assertions. It tries all combinations of shares
// with matching identifiers to find a valid reconstruction.
func SSKRJoin(envelopes []*Envelope) (*Envelope, error) {
	if len(envelopes) == 0 {
		return nil, ErrInvalidShares
	}

	groupedShares, err := sskrSharesIn(envelopes)
	if err != nil {
		return nil, err
	}

	for _, shares := range groupedShares {
		secret, err := bccomponents.SSKRCombine(shares)
		if err != nil {
			continue
		}
		contentKey, err := bccomponents.SymmetricKeyFromDataRef(secret.Data())
		if err != nil {
			continue
		}
		envelope, err := envelopes[0].DecryptSubject(contentKey)
		if err != nil {
			continue
		}
		return envelope.Subject(), nil
	}

	return nil, ErrInvalidShares
}

// sskrSharesIn extracts SSKR shares from a set of envelopes, grouped by
// their identifier.
func sskrSharesIn(envelopes []*Envelope) (map[uint16][]bccomponents.SSKRShare, error) {
	result := make(map[uint16][]bccomponents.SSKRShare)
	for _, envelope := range envelopes {
		assertions := envelope.AssertionsWithPredicate(knownvalues.SSKRShare)
		for _, assertion := range assertions {
			obj := assertion.AsObject()
			if obj == nil {
				continue
			}
			share, err := ExtractSubject[bccomponents.SSKRShare](obj)
			if err != nil {
				return nil, err
			}
			id := share.Identifier()
			result[id] = append(result[id], share)
		}
	}
	return result, nil
}
