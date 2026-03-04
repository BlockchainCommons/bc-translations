package bccomponents

import (
	"encoding/hex"
	"fmt"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	sskr "github.com/nickel-blockchaincommons/sskr-go"
)

// Re-export the SSKR package types for convenience.
type (
	SSKRSpec      = sskr.Spec
	SSKRGroupSpec = sskr.GroupSpec
	SSKRSecret    = sskr.Secret
	SSKRError     = error
)

// SSKRShare is a share of a secret split using Sharded Secret Key
// Reconstruction (SSKR).
//
// Each share contains a 5-byte metadata header (identifier, group structure,
// member position) followed by the share value data.
type SSKRShare struct {
	data []byte
}

// SSKRShareFromData creates a new SSKRShare from raw binary data.
func SSKRShareFromData(data []byte) SSKRShare {
	copied := make([]byte, len(data))
	copy(copied, data)
	return SSKRShare{data: copied}
}

// SSKRShareFromHex creates a new SSKRShare from a hexadecimal string.
func SSKRShareFromHex(h string) (SSKRShare, error) {
	data, err := hex.DecodeString(h)
	if err != nil {
		return SSKRShare{}, fmt.Errorf("bccomponents: invalid hex for SSKR share: %w", err)
	}
	return SSKRShareFromData(data), nil
}

// Bytes returns a copy of the raw binary data.
func (s SSKRShare) Bytes() []byte {
	copied := make([]byte, len(s.data))
	copy(copied, s.data)
	return copied
}

// Hex returns the data as a hexadecimal string.
func (s SSKRShare) Hex() string { return hex.EncodeToString(s.data) }

// Identifier returns the 16-bit split identifier (bytes 0..2).
func (s SSKRShare) Identifier() uint16 {
	return (uint16(s.data[0]) << 8) | uint16(s.data[1])
}

// IdentifierHex returns the split identifier as a 4-character hex string.
func (s SSKRShare) IdentifierHex() string {
	return hex.EncodeToString(s.data[:2])
}

// GroupThreshold returns the minimum number of groups required for
// reconstruction. Encoded as (value - 1) in the high nibble of byte 2.
func (s SSKRShare) GroupThreshold() int {
	return int(s.data[2]>>4) + 1
}

// GroupCount returns the total number of groups in the split.
// Encoded as (value - 1) in the low nibble of byte 2.
func (s SSKRShare) GroupCount() int {
	return int(s.data[2]&0x0f) + 1
}

// GroupIndex returns the zero-based index of the group this share belongs to.
// Encoded in the high nibble of byte 3.
func (s SSKRShare) GroupIndex() int {
	return int(s.data[3] >> 4)
}

// MemberThreshold returns the minimum number of shares within this group
// needed for reconstruction. Encoded as (value - 1) in the low nibble of byte 3.
func (s SSKRShare) MemberThreshold() int {
	return int(s.data[3]&0x0f) + 1
}

// MemberIndex returns the zero-based index of this share within its group.
// Encoded in the low nibble of byte 4.
func (s SSKRShare) MemberIndex() int {
	return int(s.data[4] & 0x0f)
}

// String returns a human-readable representation.
func (s SSKRShare) String() string {
	return fmt.Sprintf("SSKRShare(%s)", s.IdentifierHex())
}

// --- CBOR support ---

// SSKRShareCBORTags returns the CBOR tags for SSKRShare.
func SSKRShareCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSSKRShare, bctags.TagSSKRShareV1})
}

// CBORTags implements dcbor.CBORTaggedEncodable.
func (s SSKRShare) CBORTags() []dcbor.Tag { return SSKRShareCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (s SSKRShare) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(s.data) }

// TaggedCBOR returns the share as tagged CBOR.
func (s SSKRShare) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(s)
	return cbor
}

// ToCBOR encodes the share as tagged CBOR.
func (s SSKRShare) ToCBOR() dcbor.CBOR { return s.TaggedCBOR() }

// DecodeSSKRShare decodes an SSKRShare from untagged CBOR.
func DecodeSSKRShare(cbor dcbor.CBOR) (SSKRShare, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return SSKRShare{}, err
	}
	return SSKRShareFromData(data), nil
}

// DecodeTaggedSSKRShare decodes an SSKRShare from tagged CBOR.
func DecodeTaggedSSKRShare(cbor dcbor.CBOR) (SSKRShare, error) {
	return dcbor.DecodeTagged(cbor, SSKRShareCBORTags(), DecodeSSKRShare)
}

// --- UR support ---

// SSKRShareToURString encodes an SSKRShare as a UR string.
func SSKRShareToURString(s SSKRShare) string { return bcur.ToURString(s) }

// SSKRShareFromURString decodes an SSKRShare from a UR string.
func SSKRShareFromURString(urString string) (SSKRShare, error) {
	return bcur.DecodeURString(urString, SSKRShareCBORTags(), DecodeSSKRShare)
}

// --- SSKR generate / combine ---

// SSKRGenerate generates SSKR shares for the given spec and master secret
// using a secure random number generator.
func SSKRGenerate(spec *SSKRSpec, masterSecret *SSKRSecret) ([][]SSKRShare, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return SSKRGenerateUsing(spec, masterSecret, rng)
}

// SSKRGenerateUsing generates SSKR shares using a custom random number
// generator.
func SSKRGenerateUsing(spec *SSKRSpec, masterSecret *SSKRSecret, rng bcrand.RandomNumberGenerator) ([][]SSKRShare, error) {
	rawGroups, err := sskr.SSKRGenerateUsing(spec, masterSecret, rng)
	if err != nil {
		return nil, err
	}
	groups := make([][]SSKRShare, len(rawGroups))
	for i, rawGroup := range rawGroups {
		shares := make([]SSKRShare, len(rawGroup))
		for j, rawShare := range rawGroup {
			shares[j] = SSKRShareFromData(rawShare)
		}
		groups[i] = shares
	}
	return groups, nil
}

// SSKRCombine combines SSKR shares to reconstruct the original secret.
func SSKRCombine(shares []SSKRShare) (SSKRSecret, error) {
	rawShares := make([][]byte, len(shares))
	for i, share := range shares {
		rawShares[i] = share.data
	}
	return sskr.SSKRCombine(rawShares)
}
