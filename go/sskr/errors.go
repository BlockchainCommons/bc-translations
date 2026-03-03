package sskr

import (
	"errors"
)

var (
	// ErrDuplicateMemberIndex indicates duplicate member index within a group.
	ErrDuplicateMemberIndex = errors.New("when combining shares, the provided shares contained a duplicate member index")

	// ErrGroupSpecInvalid indicates an invalid group spec string.
	ErrGroupSpecInvalid = errors.New("invalid group specification")

	// ErrGroupCountInvalid indicates an invalid number of groups in a split spec.
	ErrGroupCountInvalid = errors.New("when creating a split spec, the group count is invalid")

	// ErrGroupThresholdInvalid indicates an invalid group threshold.
	ErrGroupThresholdInvalid = errors.New("SSKR group threshold is invalid")

	// ErrMemberCountInvalid indicates an invalid member count.
	ErrMemberCountInvalid = errors.New("SSKR member count is invalid")

	// ErrMemberThresholdInvalid indicates an invalid member threshold.
	ErrMemberThresholdInvalid = errors.New("SSKR member threshold is invalid")

	// ErrNotEnoughGroups indicates that shares did not contain enough groups to recover.
	ErrNotEnoughGroups = errors.New("SSKR shares did not contain enough groups")

	// ErrSecretLengthNotEven indicates odd-length secret data.
	ErrSecretLengthNotEven = errors.New("SSKR secret is not of even length")

	// ErrSecretTooLong indicates a secret longer than MaxSecretLen.
	ErrSecretTooLong = errors.New("SSKR secret is too long")

	// ErrSecretTooShort indicates a secret shorter than MinSecretLen.
	ErrSecretTooShort = errors.New("SSKR secret is too short")

	// ErrShareLengthInvalid indicates share bytes shorter than metadata size.
	ErrShareLengthInvalid = errors.New("SSKR shares did not contain enough serialized bytes")

	// ErrShareReservedBitsInvalid indicates reserved metadata bits were non-zero.
	ErrShareReservedBitsInvalid = errors.New("SSKR shares contained invalid reserved bits")

	// ErrSharesEmpty indicates no shares were provided for combine.
	ErrSharesEmpty = errors.New("SSKR shares were empty")

	// ErrShareSetInvalid indicates incompatible share metadata in a share set.
	ErrShareSetInvalid = errors.New("SSKR shares were invalid")
)

// ShamirError wraps an error returned by bc-shamir operations.
type ShamirError struct {
	cause error
}

// Error returns the formatted SSKR shim error message.
func (e *ShamirError) Error() string {
	if e == nil || e.cause == nil {
		return "SSKR Shamir error: <nil>"
	}
	return "SSKR Shamir error: " + e.cause.Error()
}

// Unwrap returns the underlying bc-shamir error.
func (e *ShamirError) Unwrap() error {
	if e == nil {
		return nil
	}
	return e.cause
}

func wrapShamirError(err error) error {
	if err == nil {
		return nil
	}
	return &ShamirError{cause: err}
}
