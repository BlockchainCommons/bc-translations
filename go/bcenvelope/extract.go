package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// ExtractSubject is a generic helper that extracts a typed value from an
// envelope's subject leaf. It supports the following types:
//   - string
//   - int, uint64
//   - bool
//   - float64
//   - []byte
//   - bccomponents.Signature
//   - *bccomponents.SealedMessage
//   - *bccomponents.EncryptedKey
//   - bccomponents.SSKRShare
//   - knownvalues.KnownValue
//   - bccomponents.Digest
func ExtractSubject[T any](e *Envelope) (T, error) {
	var zero T
	switch any(zero).(type) {
	case string:
		v, err := ExtractSubjectString(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case int:
		v, err := ExtractSubjectInt(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case uint64:
		v, err := ExtractSubjectUint64(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case bool:
		v, err := ExtractSubjectBool(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case float64:
		v, err := ExtractSubjectFloat64(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case []byte:
		v, err := ExtractSubjectBytes(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case bccomponents.Signature:
		cbor, err := e.Subject().TryLeaf()
		if err != nil {
			return zero, err
		}
		v, err := bccomponents.DecodeTaggedSignature(cbor)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case *bccomponents.SealedMessage:
		cbor, err := e.Subject().TryLeaf()
		if err != nil {
			return zero, err
		}
		v, err := bccomponents.DecodeTaggedSealedMessage(cbor)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case *bccomponents.EncryptedKey:
		cbor, err := e.Subject().TryLeaf()
		if err != nil {
			return zero, err
		}
		v, err := bccomponents.DecodeTaggedEncryptedKey(cbor)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case bccomponents.SSKRShare:
		cbor, err := e.Subject().TryLeaf()
		if err != nil {
			return zero, err
		}
		v, err := bccomponents.DecodeTaggedSSKRShare(cbor)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case knownvalues.KnownValue:
		v, err := ExtractSubjectKnownValue(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	case bccomponents.Digest:
		v, err := ExtractSubjectDigest(e)
		if err != nil {
			return zero, err
		}
		return any(v).(T), nil
	default:
		return zero, Errorf("unsupported extract type")
	}
}
