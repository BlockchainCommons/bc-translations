package sskr

import (
	"encoding/hex"
	"slices"
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

type fakeRandomNumberGenerator struct{}

func (r *fakeRandomNumberGenerator) NextU64() uint64 {
	panic("not implemented")
}

func (r *fakeRandomNumberGenerator) NextU32() uint32 {
	panic("not implemented")
}

func (r *fakeRandomNumberGenerator) RandomData(size int) []byte {
	data := make([]byte, size)
	r.FillRandomData(data)
	return data
}

func (r *fakeRandomNumberGenerator) FillRandomData(data []byte) {
	value := byte(0)
	for i := range data {
		data[i] = value
		value = value + 17
	}
}

func decodeHex(t *testing.T, source string) []byte {
	t.Helper()
	decoded, err := hex.DecodeString(source)
	if err != nil {
		t.Fatalf("DecodeString(%q) failed: %v", source, err)
	}
	return decoded
}

func TestSplit35(t *testing.T) {
	rng := &fakeRandomNumberGenerator{}
	secret, err := NewSecret(decodeHex(t, "0ff784df000c4380a5ed683f7e6e3dcf"))
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}
	group, err := NewGroupSpec(3, 5)
	if err != nil {
		t.Fatalf("NewGroupSpec failed: %v", err)
	}
	spec, err := NewSpec(1, []GroupSpec{group})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	shares, err := SSKRGenerateUsing(&spec, &secret, rng)
	if err != nil {
		t.Fatalf("SSKRGenerateUsing failed: %v", err)
	}
	flattenedShares := slices.Concat(shares...)
	if len(flattenedShares) != 5 {
		t.Fatalf("flattened share count = %d, want 5", len(flattenedShares))
	}
	for i, share := range flattenedShares {
		if len(share) != MetadataSizeBytes+secret.Len() {
			t.Fatalf("share %d length = %d, want %d", i, len(share), MetadataSizeBytes+secret.Len())
		}
	}

	recoveredShares := [][]byte{
		flattenedShares[1],
		flattenedShares[2],
		flattenedShares[4],
	}
	recoveredSecret, err := SSKRCombine(recoveredShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !recoveredSecret.Equal(secret) {
		t.Fatal("recovered secret does not match original")
	}
}

func TestSplit27(t *testing.T) {
	rng := &fakeRandomNumberGenerator{}
	secret, err := NewSecret(decodeHex(t, "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"))
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}
	group, err := NewGroupSpec(2, 7)
	if err != nil {
		t.Fatalf("NewGroupSpec failed: %v", err)
	}
	spec, err := NewSpec(1, []GroupSpec{group})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	shares, err := SSKRGenerateUsing(&spec, &secret, rng)
	if err != nil {
		t.Fatalf("SSKRGenerateUsing failed: %v", err)
	}
	if len(shares) != 1 {
		t.Fatalf("group count = %d, want 1", len(shares))
	}
	if len(shares[0]) != 7 {
		t.Fatalf("shares[0] count = %d, want 7", len(shares[0]))
	}
	flattenedShares := slices.Concat(shares...)
	if len(flattenedShares) != 7 {
		t.Fatalf("flattened share count = %d, want 7", len(flattenedShares))
	}
	for i, share := range flattenedShares {
		if len(share) != MetadataSizeBytes+secret.Len() {
			t.Fatalf("share %d length = %d, want %d", i, len(share), MetadataSizeBytes+secret.Len())
		}
	}

	recoveredShares := [][]byte{
		flattenedShares[3],
		flattenedShares[4],
	}
	recoveredSecret, err := SSKRCombine(recoveredShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !recoveredSecret.Equal(secret) {
		t.Fatal("recovered secret does not match original")
	}
}

func TestSplit2323(t *testing.T) {
	rng := &fakeRandomNumberGenerator{}
	secret, err := NewSecret(decodeHex(t, "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"))
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}
	group1, err := NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec group1 failed: %v", err)
	}
	group2, err := NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec group2 failed: %v", err)
	}
	spec, err := NewSpec(2, []GroupSpec{group1, group2})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	shares, err := SSKRGenerateUsing(&spec, &secret, rng)
	if err != nil {
		t.Fatalf("SSKRGenerateUsing failed: %v", err)
	}
	if len(shares) != 2 {
		t.Fatalf("group count = %d, want 2", len(shares))
	}
	if len(shares[0]) != 3 {
		t.Fatalf("shares[0] count = %d, want 3", len(shares[0]))
	}
	if len(shares[1]) != 3 {
		t.Fatalf("shares[1] count = %d, want 3", len(shares[1]))
	}

	flattenedShares := slices.Concat(shares...)
	if len(flattenedShares) != 6 {
		t.Fatalf("flattened share count = %d, want 6", len(flattenedShares))
	}
	for i, share := range flattenedShares {
		if len(share) != MetadataSizeBytes+secret.Len() {
			t.Fatalf("share %d length = %d, want %d", i, len(share), MetadataSizeBytes+secret.Len())
		}
	}

	recoveredShares := [][]byte{
		flattenedShares[0],
		flattenedShares[1],
		flattenedShares[3],
		flattenedShares[5],
	}
	recoveredSecret, err := SSKRCombine(recoveredShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !recoveredSecret.Equal(secret) {
		t.Fatal("recovered secret does not match original")
	}
}

func fisherYatesShuffle[T any](items []T, rng bcrand.RandomNumberGenerator) {
	i := len(items)
	for i > 1 {
		i--
		j := int(bcrand.NextInClosedRange(rng, 0, int64(i), 64))
		items[i], items[j] = items[j], items[i]
	}
}

func TestShuffle(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
	values := make([]int, 100)
	for i := range values {
		values[i] = i
	}
	fisherYatesShuffle(values, rng)
	if len(values) != 100 {
		t.Fatalf("values length = %d, want 100", len(values))
	}
	want := []int{
		79, 70, 40, 53, 25, 30, 31, 88, 10, 1, 45, 54, 81, 58, 55, 59,
		69, 78, 65, 47, 75, 61, 0, 72, 20, 9, 80, 13, 73, 11, 60, 56,
		19, 42, 33, 12, 36, 38, 6, 35, 68, 77, 50, 18, 97, 49, 98, 85,
		89, 91, 15, 71, 99, 67, 84, 23, 64, 14, 57, 48, 62, 29, 28, 94,
		44, 8, 66, 34, 43, 21, 63, 16, 92, 95, 27, 51, 26, 86, 22, 41,
		93, 82, 7, 87, 74, 37, 46, 3, 96, 24, 90, 39, 32, 17, 76, 4,
		83, 2, 52, 5,
	}
	if !slices.Equal(values, want) {
		t.Fatalf("shuffle mismatch\n got: %v\nwant: %v", values, want)
	}
}

type recoverSpec struct {
	secret                 Secret
	spec                   Spec
	shares                 [][][]byte
	recoveredGroupIndexes  []int
	recoveredMemberIndexes [][]int
	recoveredShares        [][]byte
}

func newRecoverSpec(
	secret Secret,
	spec Spec,
	shares [][][]byte,
	rng bcrand.RandomNumberGenerator,
) recoverSpec {
	groupIndexes := make([]int, spec.GroupCount())
	for i := range groupIndexes {
		groupIndexes[i] = i
	}
	fisherYatesShuffle(groupIndexes, rng)
	recoveredGroupIndexes := append([]int(nil), groupIndexes[:spec.GroupThreshold()]...)

	recoveredMemberIndexes := make([][]int, 0, len(recoveredGroupIndexes))
	groups := spec.Groups()
	for _, groupIndex := range recoveredGroupIndexes {
		group := groups[groupIndex]
		memberIndexes := make([]int, group.MemberCount())
		for i := range memberIndexes {
			memberIndexes[i] = i
		}
		fisherYatesShuffle(memberIndexes, rng)
		recoveredMemberIndexes = append(
			recoveredMemberIndexes,
			append([]int(nil), memberIndexes[:group.MemberThreshold()]...),
		)
	}

	recoveredShares := make([][]byte, 0)
	for i, recoveredGroupIndex := range recoveredGroupIndexes {
		groupShares := shares[recoveredGroupIndex]
		for _, recoveredMemberIndex := range recoveredMemberIndexes[i] {
			recoveredShares = append(recoveredShares, groupShares[recoveredMemberIndex])
		}
	}
	fisherYatesShuffle(recoveredShares, rng)

	return recoverSpec{
		secret:                 secret,
		spec:                   spec,
		shares:                 shares,
		recoveredGroupIndexes:  recoveredGroupIndexes,
		recoveredMemberIndexes: recoveredMemberIndexes,
		recoveredShares:        recoveredShares,
	}
}

func (s recoverSpec) recover(t *testing.T) {
	t.Helper()
	recoveredSecret, err := SSKRCombine(s.recoveredShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !recoveredSecret.Equal(s.secret) {
		t.Fatalf("recovered secret mismatch\n got: %x\nwant: %x", recoveredSecret.Data(), s.secret.Data())
	}
}

func oneFuzzTest(t *testing.T, rng bcrand.RandomNumberGenerator) {
	t.Helper()
	secretLen := int(bcrand.NextInClosedRange(rng, int64(MinSecretLen), int64(MaxSecretLen), 64)) &^ 1
	secret, err := NewSecret(rng.RandomData(secretLen))
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}

	groupCount := int(bcrand.NextInClosedRange(rng, 1, int64(MaxGroupsCount), 64))
	groupSpecs := make([]GroupSpec, 0, groupCount)
	for i := 0; i < groupCount; i++ {
		memberCount := int(bcrand.NextInClosedRange(rng, 1, int64(MaxShareCount), 64))
		memberThreshold := int(bcrand.NextInClosedRange(rng, 1, int64(memberCount), 64))
		group, err := NewGroupSpec(memberThreshold, memberCount)
		if err != nil {
			t.Fatalf("NewGroupSpec failed: %v", err)
		}
		groupSpecs = append(groupSpecs, group)
	}
	groupThreshold := int(bcrand.NextInClosedRange(rng, 1, int64(groupCount), 64))
	spec, err := NewSpec(groupThreshold, groupSpecs)
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	shares, err := SSKRGenerateUsing(&spec, &secret, rng)
	if err != nil {
		t.Fatalf("SSKRGenerateUsing failed: %v", err)
	}

	recovery := newRecoverSpec(secret, spec, shares, rng)
	recovery.recover(t)
}

func TestFuzz(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
	for i := 0; i < 100; i++ {
		oneFuzzTest(t, rng)
	}
}

func TestExampleEncode(t *testing.T) {
	secret, err := NewSecret([]byte("my secret belongs to me."))
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}

	group1, err := NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec group1 failed: %v", err)
	}
	group2, err := NewGroupSpec(3, 5)
	if err != nil {
		t.Fatalf("NewGroupSpec group2 failed: %v", err)
	}
	spec, err := NewSpec(2, []GroupSpec{group1, group2})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	shares, err := SSKRGenerate(&spec, &secret)
	if err != nil {
		t.Fatalf("SSKRGenerate failed: %v", err)
	}
	if len(shares) != 2 {
		t.Fatalf("group count = %d, want 2", len(shares))
	}
	if len(shares[0]) != 3 {
		t.Fatalf("shares[0] count = %d, want 3", len(shares[0]))
	}
	if len(shares[1]) != 5 {
		t.Fatalf("shares[1] count = %d, want 5", len(shares[1]))
	}

	recoveredShares := [][]byte{
		shares[0][0],
		shares[0][2],
		shares[1][0],
		shares[1][1],
		shares[1][4],
	}
	recoveredSecret, err := SSKRCombine(recoveredShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !recoveredSecret.Equal(secret) {
		t.Fatal("recovered secret does not match original")
	}
}

func TestExampleEncode3(t *testing.T) {
	const text = "my secret belongs to me."

	roundtrip := func(m, n int) (Secret, error) {
		secret, err := NewSecret([]byte(text))
		if err != nil {
			return Secret{}, err
		}
		spec, err := NewSpec(1, []GroupSpec{mustGroupSpec(t, m, n)})
		if err != nil {
			return Secret{}, err
		}
		shares, err := SSKRGenerate(&spec, &secret)
		if err != nil {
			return Secret{}, err
		}
		flattened := slices.Concat(shares...)
		return SSKRCombine(flattened)
	}

	// Good, uses a 2/3 group.
	{
		result, err := roundtrip(2, 3)
		if err != nil {
			t.Fatalf("roundtrip(2,3) failed: %v", err)
		}
		if string(result.Data()) != text {
			t.Fatalf("roundtrip(2,3) = %q, want %q", string(result.Data()), text)
		}
	}

	// Still ok, uses a 1/1 group.
	{
		result, err := roundtrip(1, 1)
		if err != nil {
			t.Fatalf("roundtrip(1,1) failed: %v", err)
		}
		if string(result.Data()) != text {
			t.Fatalf("roundtrip(1,1) = %q, want %q", string(result.Data()), text)
		}
	}

	// Fixed, uses a 1/3 group.
	{
		result, err := roundtrip(1, 3)
		if err != nil {
			t.Fatalf("roundtrip(1,3) failed: %v", err)
		}
		if string(result.Data()) != text {
			t.Fatalf("roundtrip(1,3) = %q, want %q", string(result.Data()), text)
		}
	}
}

func mustGroupSpec(t *testing.T, memberThreshold, memberCount int) GroupSpec {
	t.Helper()
	group, err := NewGroupSpec(memberThreshold, memberCount)
	if err != nil {
		t.Fatalf("NewGroupSpec(%d,%d) failed: %v", memberThreshold, memberCount, err)
	}
	return group
}

func TestExampleEncode4(t *testing.T) {
	const text = "my secret belongs to me."

	secret, err := NewSecret([]byte(text))
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}
	spec, err := NewSpec(1, []GroupSpec{mustGroupSpec(t, 2, 3), mustGroupSpec(t, 2, 3)})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}
	groupedShares, err := SSKRGenerate(&spec, &secret)
	if err != nil {
		t.Fatalf("SSKRGenerate failed: %v", err)
	}
	flattenedShares := slices.Concat(groupedShares...)

	// The group threshold is 1, but we're providing an additional share
	// from the second group. The correct behavior is to ignore any group
	// that cannot be decoded.
	recoveredShares := [][]byte{
		flattenedShares[0],
		flattenedShares[1],
		flattenedShares[3],
	}
	recoveredSecret, err := SSKRCombine(recoveredShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if string(recoveredSecret.Data()) != text {
		t.Fatalf("recovered text = %q, want %q", string(recoveredSecret.Data()), text)
	}
}
