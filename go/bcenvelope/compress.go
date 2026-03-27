package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

// Compress returns a compressed version of this envelope. The compressed
// envelope maintains the same digest as the original, ensuring compatibility
// with the envelope's digest tree structure.
func (e *Envelope) Compress() (*Envelope, error) {
	switch e.Case().(type) {
	case *CompressedCase:
		return e, nil
	case *EncryptedCase:
		return nil, ErrAlreadyEncrypted
	case *ElidedCase:
		return nil, ErrAlreadyElided
	default:
		digest := e.Digest()
		compressed := bccomponents.CompressedFromData(
			e.TaggedCBOR().ToCBORData(),
			&digest,
		)
		return NewCompressedEnvelope(&compressed), nil
	}
}

// Decompress returns the decompressed variant of this envelope. Returns an
// error if the envelope is not compressed, the digest is missing, or the
// decompressed data doesn't match the expected digest.
func (e *Envelope) Decompress() (*Envelope, error) {
	comp, ok := e.Case().(*CompressedCase)
	if !ok {
		return nil, ErrNotCompressed
	}

	digestOpt := comp.Compressed.DigestOpt()
	if digestOpt == nil {
		return nil, ErrMissingDigest
	}

	if !digestOpt.Equal(e.Digest()) {
		return nil, ErrInvalidDigest
	}

	decompressedData, err := comp.Compressed.Decompress()
	if err != nil {
		return nil, err
	}

	envelope, err := FromTaggedCBORData(decompressedData)
	if err != nil {
		return nil, err
	}

	if !envelope.Digest().Equal(*digestOpt) {
		return nil, ErrInvalidDigest
	}

	return envelope, nil
}

// CompressSubject returns this envelope with its subject compressed. Unlike
// Compress which compresses the entire envelope, this method only compresses
// the subject, leaving assertions uncompressed.
func (e *Envelope) CompressSubject() (*Envelope, error) {
	if e.Subject().IsCompressed() {
		return e, nil
	}
	subject, err := e.Subject().Compress()
	if err != nil {
		return nil, err
	}
	return e.ReplaceSubject(subject), nil
}

// DecompressSubject returns this envelope with its subject decompressed,
// reversing the effect of CompressSubject.
func (e *Envelope) DecompressSubject() (*Envelope, error) {
	if e.Subject().IsCompressed() {
		subject, err := e.Subject().Decompress()
		if err != nil {
			return nil, err
		}
		return e.ReplaceSubject(subject), nil
	}
	return e, nil
}
