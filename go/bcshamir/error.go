package bcshamir

import "errors"

var (
	ErrSecretTooLong        = errors.New("secret is too long")
	ErrTooManyShares        = errors.New("too many shares")
	ErrInterpolationFailure = errors.New("interpolation failed")
	ErrChecksumFailure      = errors.New("checksum failure")
	ErrSecretTooShort       = errors.New("secret is too short")
	ErrSecretOddLength      = errors.New("secret length is odd")
	ErrInvalidThreshold     = errors.New("invalid threshold")
	ErrSharesUnequalLength  = errors.New("shares have unequal length")
)
