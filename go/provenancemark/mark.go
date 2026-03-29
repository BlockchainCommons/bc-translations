package provenancemark

import (
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"net/url"
	"strings"

	bcenvelope "github.com/nickel-blockchaincommons/bcenvelope-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// ProvenanceMarkPrefix is the visual prefix used for mark identifiers.
const ProvenanceMarkPrefix = "\U0001F15F"

// ProvenanceMark is an individual mark in a provenance chain.
type ProvenanceMark struct {
	res       ProvenanceMarkResolution
	key       []byte
	hash      []byte
	chainID   []byte
	infoBytes []byte
	seqBytes  []byte
	dateBytes []byte
	seq       uint32
	date      dcbor.Date
}

// NewProvenanceMark constructs a mark from its revealed key and successor key.
func NewProvenanceMark(
	res ProvenanceMarkResolution,
	key []byte,
	nextKey []byte,
	chainID []byte,
	seq uint32,
	date dcbor.Date,
	info any,
) (ProvenanceMark, error) {
	linkLength := res.LinkLength()
	if len(key) != linkLength {
		return ProvenanceMark{}, newInvalidKeyLength(linkLength, len(key))
	}
	if len(nextKey) != linkLength {
		return ProvenanceMark{}, newInvalidNextKeyLength(linkLength, len(nextKey))
	}
	if len(chainID) != linkLength {
		return ProvenanceMark{}, newInvalidChainIDLength(linkLength, len(chainID))
	}

	dateBytes, err := res.SerializeDate(date)
	if err != nil {
		return ProvenanceMark{}, err
	}
	seqBytes, err := res.SerializeSeq(seq)
	if err != nil {
		return ProvenanceMark{}, err
	}
	roundTrippedDate, err := res.DeserializeDate(dateBytes)
	if err != nil {
		return ProvenanceMark{}, err
	}

	infoBytes := []byte{}
	if info != nil {
		cbor, err := dcbor.FromAny(info)
		if err != nil {
			return ProvenanceMark{}, wrapCBORError(err)
		}
		infoBytes = cbor.ToCBORData()
	}

	hash := makeProvenanceMarkHash(res, key, nextKey, chainID, seqBytes, dateBytes, infoBytes)

	return ProvenanceMark{
		res:       res,
		key:       cloneBytes(key),
		hash:      hash,
		chainID:   cloneBytes(chainID),
		infoBytes: cloneBytes(infoBytes),
		seqBytes:  cloneBytes(seqBytes),
		dateBytes: cloneBytes(dateBytes),
		seq:       seq,
		date:      roundTrippedDate,
	}, nil
}

// ProvenanceMarkFromMessage decodes a mark from its compact message bytes.
func ProvenanceMarkFromMessage(res ProvenanceMarkResolution, message []byte) (ProvenanceMark, error) {
	if len(message) < res.FixedLength() {
		return ProvenanceMark{}, newInvalidMessageLength(res.FixedLength(), len(message))
	}

	key := sliceByteRange(message, res.KeyRange())
	payload := Obfuscate(key, message[res.LinkLength():])
	hash := sliceByteRange(payload, res.HashRange())
	chainID := sliceByteRange(payload, res.ChainIDRange())
	seqBytes := sliceByteRange(payload, res.SeqBytesRange())
	seq, err := res.DeserializeSeq(seqBytes)
	if err != nil {
		return ProvenanceMark{}, err
	}
	dateBytes := sliceByteRange(payload, res.DateBytesRange())
	date, err := res.DeserializeDate(dateBytes)
	if err != nil {
		return ProvenanceMark{}, err
	}
	infoBytes := sliceByteRange(payload, res.InfoRange())
	if len(infoBytes) > 0 {
		if _, err := dcbor.TryFromData(infoBytes); err != nil {
			return ProvenanceMark{}, newInvalidInfoCBOR()
		}
	}

	return ProvenanceMark{
		res:       res,
		key:       key,
		hash:      hash,
		chainID:   chainID,
		infoBytes: infoBytes,
		seqBytes:  seqBytes,
		dateBytes: dateBytes,
		seq:       seq,
		date:      date,
	}, nil
}

func makeProvenanceMarkHash(
	res ProvenanceMarkResolution,
	key []byte,
	nextKey []byte,
	chainID []byte,
	seqBytes []byte,
	dateBytes []byte,
	infoBytes []byte,
) []byte {
	buffer := make([]byte, 0, len(key)+len(nextKey)+len(chainID)+len(seqBytes)+len(dateBytes)+len(infoBytes))
	buffer = append(buffer, key...)
	buffer = append(buffer, nextKey...)
	buffer = append(buffer, chainID...)
	buffer = append(buffer, seqBytes...)
	buffer = append(buffer, dateBytes...)
	buffer = append(buffer, infoBytes...)
	return SHA256Prefix(buffer, res.LinkLength())
}

// Res returns the mark resolution.
func (m ProvenanceMark) Res() ProvenanceMarkResolution {
	return m.res
}

// Key returns the revealed key bytes.
func (m ProvenanceMark) Key() []byte {
	return cloneBytes(m.key)
}

// Hash returns the stored hash bytes.
func (m ProvenanceMark) Hash() []byte {
	return cloneBytes(m.hash)
}

// ChainID returns the chain identifier bytes.
func (m ProvenanceMark) ChainID() []byte {
	return cloneBytes(m.chainID)
}

// SeqBytes returns the encoded sequence number bytes.
func (m ProvenanceMark) SeqBytes() []byte {
	return cloneBytes(m.seqBytes)
}

// DateBytes returns the encoded date bytes.
func (m ProvenanceMark) DateBytes() []byte {
	return cloneBytes(m.dateBytes)
}

// Seq returns the decoded sequence number.
func (m ProvenanceMark) Seq() uint32 {
	return m.seq
}

// Date returns the decoded date.
func (m ProvenanceMark) Date() dcbor.Date {
	return m.date
}

// Message returns the compact provenance message bytes.
func (m ProvenanceMark) Message() []byte {
	payload := make([]byte, 0, len(m.chainID)+len(m.hash)+len(m.seqBytes)+len(m.dateBytes)+len(m.infoBytes))
	payload = append(payload, m.chainID...)
	payload = append(payload, m.hash...)
	payload = append(payload, m.seqBytes...)
	payload = append(payload, m.dateBytes...)
	payload = append(payload, m.infoBytes...)

	message := make([]byte, 0, len(m.key)+len(payload))
	message = append(message, m.key...)
	message = append(message, Obfuscate(m.key, payload)...)
	return message
}

// Info returns the optional info CBOR payload.
func (m ProvenanceMark) Info() *dcbor.CBOR {
	if len(m.infoBytes) == 0 {
		return nil
	}
	cbor, err := dcbor.TryFromData(m.infoBytes)
	if err != nil {
		return nil
	}
	return &cbor
}

// ID returns the 32-byte mark ID.
func (m ProvenanceMark) ID() [SHA256Size]byte {
	var result [SHA256Size]byte
	n := len(m.hash)
	copy(result[:n], m.hash)
	if n < SHA256Size {
		fingerprint := m.Fingerprint()
		copy(result[n:], fingerprint[:SHA256Size-n])
	}
	return result
}

// IDHex returns the full 32-byte mark ID as hex.
func (m ProvenanceMark) IDHex() string {
	id := m.ID()
	return hex.EncodeToString(id[:])
}

// IDBytewords returns the first wordCount bytes of the mark ID as upper-case Bytewords.
func (m ProvenanceMark) IDBytewords(wordCount int, prefix bool) string {
	if wordCount < 4 || wordCount > SHA256Size {
		panic(fmt.Sprintf("word_count must be 4..=32, got %d", wordCount))
	}
	id := m.ID()
	text := strings.ToUpper(bcur.EncodeToWords(id[:wordCount]))
	if prefix {
		return ProvenanceMarkPrefix + " " + text
	}
	return text
}

// IDBytemoji returns the first wordCount bytes of the mark ID as Bytemoji.
func (m ProvenanceMark) IDBytemoji(wordCount int, prefix bool) string {
	if wordCount < 4 || wordCount > SHA256Size {
		panic(fmt.Sprintf("word_count must be 4..=32, got %d", wordCount))
	}
	id := m.ID()
	text := strings.ToUpper(bcur.EncodeToBytemojis(id[:wordCount]))
	if prefix {
		return ProvenanceMarkPrefix + " " + text
	}
	return text
}

// IDBytewordsMinimal returns the first wordCount bytes of the mark ID as minimal Bytewords.
func (m ProvenanceMark) IDBytewordsMinimal(wordCount int, prefix bool) string {
	if wordCount < 4 || wordCount > SHA256Size {
		panic(fmt.Sprintf("word_count must be 4..=32, got %d", wordCount))
	}
	id := m.ID()
	text := strings.ToUpper(bcur.EncodeToMinimalBytewords(id[:wordCount]))
	if prefix {
		return ProvenanceMarkPrefix + " " + text
	}
	return text
}

func minimalNoncollidingPrefixLengths(ids [][SHA256Size]byte) []int {
	lengths := make([]int, len(ids))
	for i := range lengths {
		lengths[i] = 4
	}

	groups := make(map[[4]byte][]int)
	for i, id := range ids {
		var key [4]byte
		copy(key[:], id[:4])
		groups[key] = append(groups[key], i)
	}

	for _, indices := range groups {
		if len(indices) <= 1 {
			continue
		}
		resolveCollisionGroup(ids, indices, lengths)
	}

	return lengths
}

func resolveCollisionGroup(ids [][SHA256Size]byte, initialIndices []int, lengths []int) {
	unresolved := append([]int(nil), initialIndices...)
	for prefixLen := 5; prefixLen <= SHA256Size; prefixLen++ {
		subGroups := make(map[string][]int)
		for _, i := range unresolved {
			subGroups[hex.EncodeToString(ids[i][:prefixLen])] = append(subGroups[hex.EncodeToString(ids[i][:prefixLen])], i)
		}

		nextUnresolved := make([]int, 0)
		for _, subIndices := range subGroups {
			if len(subIndices) == 1 {
				lengths[subIndices[0]] = prefixLen
			} else {
				nextUnresolved = append(nextUnresolved, subIndices...)
			}
		}
		if len(nextUnresolved) == 0 {
			return
		}
		unresolved = nextUnresolved
	}

	for _, i := range unresolved {
		lengths[i] = SHA256Size
	}
}

// DisambiguatedIDBytewords returns minimally disambiguated upper-case Bytewords IDs.
func DisambiguatedIDBytewords(marks []ProvenanceMark, prefix bool) []string {
	if len(marks) == 0 {
		return []string{}
	}
	ids := make([][SHA256Size]byte, len(marks))
	for i, mark := range marks {
		ids[i] = mark.ID()
	}
	lengths := minimalNoncollidingPrefixLengths(ids)
	result := make([]string, len(ids))
	for i, id := range ids {
		text := strings.ToUpper(bcur.EncodeToWords(id[:lengths[i]]))
		if prefix {
			result[i] = ProvenanceMarkPrefix + " " + text
		} else {
			result[i] = text
		}
	}
	return result
}

// DisambiguatedIDBytemoji returns minimally disambiguated Bytemoji IDs.
func DisambiguatedIDBytemoji(marks []ProvenanceMark, prefix bool) []string {
	if len(marks) == 0 {
		return []string{}
	}
	ids := make([][SHA256Size]byte, len(marks))
	for i, mark := range marks {
		ids[i] = mark.ID()
	}
	lengths := minimalNoncollidingPrefixLengths(ids)
	result := make([]string, len(ids))
	for i, id := range ids {
		text := strings.ToUpper(bcur.EncodeToBytemojis(id[:lengths[i]]))
		if prefix {
			result[i] = ProvenanceMarkPrefix + " " + text
		} else {
			result[i] = text
		}
	}
	return result
}

// Precedes reports whether this mark validly precedes the next mark.
func (m ProvenanceMark) Precedes(next ProvenanceMark) bool {
	return m.PrecedesOpt(next) == nil
}

// PrecedesOpt validates that this mark precedes the next mark.
func (m ProvenanceMark) PrecedesOpt(next ProvenanceMark) error {
	if next.seq == 0 {
		return wrapValidationIssue(NewNonGenesisAtZeroIssue())
	}
	if compareBytes(next.key, next.chainID) == 0 {
		return wrapValidationIssue(NewInvalidGenesisKeyIssue())
	}
	if m.seq != next.seq-1 {
		return wrapValidationIssue(NewSequenceGapIssue(m.seq+1, next.seq))
	}
	if m.date.Datetime().After(next.date.Datetime()) {
		return wrapValidationIssue(NewDateOrderingIssue(m.date, next.date))
	}

	expectedHash := makeProvenanceMarkHash(
		m.res,
		m.key,
		next.key,
		m.chainID,
		m.seqBytes,
		m.dateBytes,
		m.infoBytes,
	)
	if compareBytes(m.hash, expectedHash) != 0 {
		return wrapValidationIssue(NewHashMismatchIssue(expectedHash, m.hash))
	}

	return nil
}

// IsGenesis reports whether this is the genesis mark in its chain.
func (m ProvenanceMark) IsGenesis() bool {
	return m.seq == 0 && compareBytes(m.key, m.chainID) == 0
}

// IsSequenceValid reports whether the provided marks form a valid sequence.
func IsSequenceValid(marks []ProvenanceMark) bool {
	if len(marks) < 2 {
		return false
	}
	if marks[0].Seq() == 0 && !marks[0].IsGenesis() {
		return false
	}
	for i := 0; i < len(marks)-1; i++ {
		if !marks[i].Precedes(marks[i+1]) {
			return false
		}
	}
	return true
}

// ToBytewordsWithStyle encodes the message using bytewords with CRC.
func (m ProvenanceMark) ToBytewordsWithStyle(style bcur.BytewordsStyle) string {
	return bcur.BytewordsEncode(m.Message(), style)
}

// ToBytewords encodes the message using standard bytewords.
func (m ProvenanceMark) ToBytewords() string {
	return m.ToBytewordsWithStyle(bcur.BytewordsStandard)
}

// ProvenanceMarkFromBytewords decodes a mark from standard bytewords.
func ProvenanceMarkFromBytewords(res ProvenanceMarkResolution, bytewords string) (ProvenanceMark, error) {
	message, err := bcur.BytewordsDecode(bytewords, bcur.BytewordsStandard)
	if err != nil {
		return ProvenanceMark{}, wrapBytewordsError(err)
	}
	return ProvenanceMarkFromMessage(res, message)
}

// ToURLEncoding returns the minimal-bytewords encoding of the tagged CBOR payload.
func (m ProvenanceMark) ToURLEncoding() string {
	return bcur.BytewordsEncode(m.TaggedCBOR().ToCBORData(), bcur.BytewordsMinimal)
}

// ProvenanceMarkFromURLEncoding decodes a mark from its URL-safe encoding.
func ProvenanceMarkFromURLEncoding(urlEncoding string) (ProvenanceMark, error) {
	cborData, err := bcur.BytewordsDecode(urlEncoding, bcur.BytewordsMinimal)
	if err != nil {
		return ProvenanceMark{}, wrapBytewordsError(err)
	}
	return ProvenanceMarkFromTaggedCBORData(cborData)
}

// ToURL appends the provenance query parameter to the given base URL.
func (m ProvenanceMark) ToURL(base string) *url.URL {
	parsed, err := url.Parse(base)
	if err != nil {
		panic(err)
	}
	query := parsed.Query()
	query.Set("provenance", m.ToURLEncoding())
	parsed.RawQuery = query.Encode()
	return parsed
}

// ProvenanceMarkFromURL decodes a mark from a URL query parameter.
func ProvenanceMarkFromURL(value *url.URL) (ProvenanceMark, error) {
	if value == nil {
		return ProvenanceMark{}, newMissingURLParameter("provenance")
	}
	encoding := value.Query().Get("provenance")
	if encoding == "" {
		return ProvenanceMark{}, newMissingURLParameter("provenance")
	}
	return ProvenanceMarkFromURLEncoding(encoding)
}

// DebugString returns the Rust-like debug representation.
func (m ProvenanceMark) DebugString() string {
	components := []string{
		fmt.Sprintf("key: %s", hex.EncodeToString(m.key)),
		fmt.Sprintf("hash: %s", hex.EncodeToString(m.hash)),
		fmt.Sprintf("chainID: %s", hex.EncodeToString(m.chainID)),
		fmt.Sprintf("seq: %d", m.seq),
		fmt.Sprintf("date: %s", m.date.String()),
	}
	if info := m.Info(); info != nil {
		components = append(components, fmt.Sprintf("info: %s", info.Diagnostic()))
	}
	return fmt.Sprintf("ProvenanceMark(%s)", strings.Join(components, ", "))
}

// String returns the display representation.
func (m ProvenanceMark) String() string {
	return fmt.Sprintf("ProvenanceMark(%s)", m.IDHex())
}

// GoString returns the debug representation for %#v formatting.
func (m ProvenanceMark) GoString() string {
	return m.DebugString()
}

// Equal reports semantic equality, matching the Rust implementation.
func (m ProvenanceMark) Equal(other ProvenanceMark) bool {
	return m.res == other.res && compareBytes(m.Message(), other.Message()) == 0
}

// ProvenanceMarkCBORTags returns the accepted CBOR tags for provenance marks.
func ProvenanceMarkCBORTags() []dcbor.Tag {
	return []dcbor.Tag{dcbor.NewTag(bctags.TagProvenanceMark, bctags.TagNameProvenanceMark)}
}

// CBORTags implements dcbor.CBORTagged.
func (m ProvenanceMark) CBORTags() []dcbor.Tag {
	return ProvenanceMarkCBORTags()
}

// UntaggedCBOR encodes the untagged provenance-mark payload.
func (m ProvenanceMark) UntaggedCBOR() dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		m.res.ToCBOR(),
		dcbor.ToByteString(m.Message()),
	})
}

// TaggedCBOR returns the tagged CBOR representation.
func (m ProvenanceMark) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(m)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (m ProvenanceMark) ToCBOR() dcbor.CBOR {
	return m.TaggedCBOR()
}

// DecodeProvenanceMark decodes an untagged provenance-mark payload.
func DecodeProvenanceMark(cbor dcbor.CBOR) (ProvenanceMark, error) {
	items, err := cbor.TryIntoArray()
	if err != nil {
		return ProvenanceMark{}, wrapCBORError(err)
	}
	if len(items) != 2 {
		return ProvenanceMark{}, wrapCBORError(dcbor.NewErrorf("Invalid provenance mark length"))
	}
	res, err := DecodeProvenanceMarkResolution(items[0])
	if err != nil {
		return ProvenanceMark{}, err
	}
	message, err := items[1].TryIntoByteString()
	if err != nil {
		return ProvenanceMark{}, wrapCBORError(err)
	}
	return ProvenanceMarkFromMessage(res, message)
}

// DecodeTaggedProvenanceMark decodes a tagged provenance mark.
func DecodeTaggedProvenanceMark(cbor dcbor.CBOR) (ProvenanceMark, error) {
	return dcbor.DecodeTagged(cbor, ProvenanceMarkCBORTags(), DecodeProvenanceMark)
}

// ProvenanceMarkFromTaggedCBORData decodes a tagged provenance mark from bytes.
func ProvenanceMarkFromTaggedCBORData(data []byte) (ProvenanceMark, error) {
	return dcbor.DecodeTaggedData(data, ProvenanceMarkCBORTags(), DecodeProvenanceMark)
}

// ToUR converts the mark to a UR.
func (m ProvenanceMark) ToUR() *bcur.UR {
	return bcur.ToUR(m)
}

// URString returns the canonical UR string.
func (m ProvenanceMark) URString() string {
	return bcur.ToURString(m)
}

// ProvenanceMarkFromUR decodes a mark from a UR.
func ProvenanceMarkFromUR(ur *bcur.UR) (ProvenanceMark, error) {
	return bcur.DecodeUR(ur, ProvenanceMarkCBORTags(), DecodeProvenanceMark)
}

// ProvenanceMarkFromURString decodes a mark from a UR string.
func ProvenanceMarkFromURString(urString string) (ProvenanceMark, error) {
	return bcur.DecodeURString(urString, ProvenanceMarkCBORTags(), DecodeProvenanceMark)
}

// ToEnvelope converts the mark to an envelope.
func (m ProvenanceMark) ToEnvelope() *bcenvelope.Envelope {
	return bcenvelope.NewEnvelope(m.TaggedCBOR())
}

// ProvenanceMarkFromEnvelope decodes a mark from an envelope.
func ProvenanceMarkFromEnvelope(envelope *bcenvelope.Envelope) (ProvenanceMark, error) {
	if envelope == nil {
		return ProvenanceMark{}, wrapEnvelopeError(errors.New("nil envelope"))
	}
	leaf, err := envelope.Subject().TryLeaf()
	if err != nil {
		return ProvenanceMark{}, wrapCBORError(fmt.Errorf("envelope error: %w", err))
	}
	return DecodeTaggedProvenanceMark(leaf)
}

// Fingerprint returns the SHA-256 hash of the tagged CBOR encoding.
func (m ProvenanceMark) Fingerprint() [SHA256Size]byte {
	return SHA256(m.TaggedCBOR().ToCBORData())
}

// RegisterTagsIn registers provenance-mark summarizers in the given format context.
func RegisterTagsIn(context *bcenvelope.FormatContext) {
	bcenvelope.RegisterTagsIn(context)
	context.Tags().SetSummarizer(bctags.TagProvenanceMark, func(untagged dcbor.CBOR, _ bool) (string, error) {
		mark, err := DecodeProvenanceMark(untagged)
		if err != nil {
			return "", err
		}
		return mark.String(), nil
	})
}

// RegisterTags registers provenance-mark summarizers in the global format context.
func RegisterTags() {
	bcenvelope.RegisterTags()
	dcbor.WithTags(func(tagsStore *dcbor.TagsStore) struct{} {
		tagsStore.SetSummarizer(bctags.TagProvenanceMark, func(untagged dcbor.CBOR, _ bool) (string, error) {
			mark, err := DecodeProvenanceMark(untagged)
			if err != nil {
				return "", err
			}
			return mark.String(), nil
		})
		return struct{}{}
	})
}

type provenanceMarkJSON struct {
	Seq       uint32                   `json:"seq"`
	Date      string                   `json:"date"`
	Res       ProvenanceMarkResolution `json:"res"`
	ChainID   string                   `json:"chain_id"`
	Key       string                   `json:"key"`
	Hash      string                   `json:"hash"`
	InfoBytes string                   `json:"info_bytes,omitempty"`
}

// MarshalJSON encodes the mark in its public JSON form.
func (m ProvenanceMark) MarshalJSON() ([]byte, error) {
	payload := provenanceMarkJSON{
		Seq:     m.seq,
		Date:    SerializeISO8601(m.date),
		Res:     m.res,
		ChainID: SerializeBase64(m.chainID),
		Key:     SerializeBase64(m.key),
		Hash:    SerializeBase64(m.hash),
	}
	if len(m.infoBytes) > 0 {
		info, err := SerializeCBOR(m.infoBytes)
		if err != nil {
			return nil, err
		}
		payload.InfoBytes = info
	}
	return json.Marshal(payload)
}

// UnmarshalJSON decodes the mark from its public JSON form.
func (m *ProvenanceMark) UnmarshalJSON(data []byte) error {
	var payload provenanceMarkJSON
	if err := json.Unmarshal(data, &payload); err != nil {
		return err
	}

	key, err := DeserializeBase64(payload.Key)
	if err != nil {
		return err
	}
	hash, err := DeserializeBase64(payload.Hash)
	if err != nil {
		return err
	}
	chainID, err := DeserializeBase64(payload.ChainID)
	if err != nil {
		return err
	}
	date, err := DeserializeISO8601(payload.Date)
	if err != nil {
		return err
	}
	infoBytes := []byte{}
	if payload.InfoBytes != "" {
		infoBytes, err = DeserializeCBOR(payload.InfoBytes)
		if err != nil {
			return err
		}
	}
	seqBytes, err := payload.Res.SerializeSeq(payload.Seq)
	if err != nil {
		return err
	}
	dateBytes, err := payload.Res.SerializeDate(date)
	if err != nil {
		return err
	}

	*m = ProvenanceMark{
		res:       payload.Res,
		key:       key,
		hash:      hash,
		chainID:   chainID,
		infoBytes: infoBytes,
		seqBytes:  seqBytes,
		dateBytes: dateBytes,
		seq:       payload.Seq,
		date:      date,
	}
	return nil
}

var _ json.Marshaler = ProvenanceMark{}
var _ json.Unmarshaler = (*ProvenanceMark)(nil)
var _ dcbor.CBORTaggedEncodable = ProvenanceMark{}
