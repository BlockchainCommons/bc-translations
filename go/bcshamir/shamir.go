package bcshamir

import (
	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

// MinSecretLen is the minimum length of a secret in bytes.
const MinSecretLen = 16

// MaxSecretLen is the maximum length of a secret in bytes.
const MaxSecretLen = 32

// MaxShareCount is the maximum number of shares that can be generated.
const MaxShareCount = 16

const secretIndex byte = 255
const digestIndex byte = 254

func createDigest(randomData, sharedSecret []byte) [32]byte {
	return bccrypto.HMACSHA256(randomData, sharedSecret)
}

func validateParameters(threshold, shareCount, secretLength int) error {
	if shareCount > MaxShareCount {
		return ErrTooManyShares
	}
	if threshold < 1 || threshold > shareCount {
		return ErrInvalidThreshold
	}
	if secretLength > MaxSecretLen {
		return ErrSecretTooLong
	}
	if secretLength < MinSecretLen {
		return ErrSecretTooShort
	}
	if secretLength&1 != 0 {
		return ErrSecretOddLength
	}
	return nil
}

// SplitSecret splits a secret into shares using the Shamir secret sharing
// algorithm.
//
// Parameters:
//   - threshold: the minimum number of shares required to reconstruct the
//     secret. Must be >= 1 and <= shareCount.
//   - shareCount: the total number of shares to generate. Must be >= threshold
//     and <= MaxShareCount.
//   - secret: a byte slice containing the secret to be split. Must be at least
//     MinSecretLen bytes long and at most MaxSecretLen bytes long. The length
//     must be an even number.
//   - rng: a RandomNumberGenerator used to generate random data.
func SplitSecret(threshold, shareCount int, secret []byte, rng bcrand.RandomNumberGenerator) ([][]byte, error) {
	if err := validateParameters(threshold, shareCount, len(secret)); err != nil {
		return nil, err
	}

	if threshold == 1 {
		// just return shareCount copies of the secret
		result := make([][]byte, shareCount)
		for i := range result {
			result[i] = make([]byte, len(secret))
			copy(result[i], secret)
		}
		return result, nil
	}

	x := make([]byte, shareCount)
	y := make([][]byte, shareCount)
	for i := range y {
		y[i] = make([]byte, len(secret))
	}
	n := 0
	result := make([][]byte, shareCount)
	for i := range result {
		result[i] = make([]byte, len(secret))
	}

	for index := 0; index < threshold-2; index++ {
		rng.FillRandomData(result[index])
		x[n] = byte(index)
		copy(y[n], result[index])
		n++
	}

	// generate secretLength - 4 bytes worth of random data
	digest := make([]byte, len(secret))
	rng.FillRandomData(digest[4:])
	// put 4 bytes of digest at the top of the digest array
	d := createDigest(digest[4:], secret)
	copy(digest[:4], d[:4])
	x[n] = digestIndex
	copy(y[n], digest)
	n++

	x[n] = secretIndex
	copy(y[n], secret)
	n++

	for index := threshold - 2; index < shareCount; index++ {
		v, err := interpolate(n, x, len(secret), y, byte(index))
		if err != nil {
			return nil, err
		}
		copy(result[index], v)
	}

	// clean up
	bccrypto.Memzero(digest)
	bccrypto.Memzero(x)
	bccrypto.MemzeroVecVecU8(y)

	return result, nil
}

// RecoverSecret recovers the secret from the given shares using the Shamir
// secret sharing algorithm.
//
// Parameters:
//   - indexes: a slice of indexes of the shares to be used for recovery.
//     These are the indexes of the shares returned by SplitSecret.
//   - shares: a slice of shares matching the indexes.
func RecoverSecret(indexes []int, shares [][]byte) ([]byte, error) {
	threshold := len(shares)
	if threshold == 0 || len(indexes) != threshold {
		return nil, ErrInvalidThreshold
	}
	shareLength := len(shares[0])
	if err := validateParameters(threshold, threshold, shareLength); err != nil {
		return nil, err
	}

	for _, share := range shares {
		if len(share) != shareLength {
			return nil, ErrSharesUnequalLength
		}
	}

	if threshold == 1 {
		result := make([]byte, shareLength)
		copy(result, shares[0])
		return result, nil
	}

	idxBytes := make([]byte, threshold)
	for i, idx := range indexes {
		idxBytes[i] = byte(idx)
	}

	digest, err := interpolate(threshold, idxBytes, shareLength, shares, digestIndex)
	if err != nil {
		return nil, err
	}
	secret, err := interpolate(threshold, idxBytes, shareLength, shares, secretIndex)
	if err != nil {
		return nil, err
	}
	verify := createDigest(digest[4:], secret)

	valid := true
	for i := 0; i < 4; i++ {
		valid = valid && (digest[i] == verify[i])
	}
	bccrypto.Memzero(digest)
	bccrypto.Memzero(verify[:])

	if !valid {
		return nil, ErrChecksumFailure
	}

	return secret, nil
}
