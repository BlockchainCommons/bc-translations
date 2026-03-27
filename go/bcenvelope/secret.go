package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// LockSubject encrypts the envelope's subject with a randomly generated
// content key, then locks the content key using the specified key derivation
// method and secret. The locked key is attached as a 'hasSecret' assertion.
func (e *Envelope) LockSubject(
	method bccomponents.KeyDerivationMethod,
	secret []byte,
) (*Envelope, error) {
	contentKey := bccomponents.NewSymmetricKey()
	encryptedKey, err := bccomponents.LockEncryptedKey(method, secret, contentKey)
	if err != nil {
		return nil, err
	}
	encrypted, err := e.EncryptSubject(contentKey)
	if err != nil {
		return nil, err
	}
	return encrypted.AddAssertion(knownvalues.HasSecret, encryptedKey), nil
}

// UnlockSubject attempts to decrypt the envelope's subject by finding a
// 'hasSecret' assertion whose encrypted key can be unlocked with the
// provided secret.
func (e *Envelope) UnlockSubject(secret []byte) (*Envelope, error) {
	assertions := e.AssertionsWithPredicate(knownvalues.HasSecret)
	for _, assertion := range assertions {
		obj := assertion.AsObject()
		if obj == nil || obj.IsObscured() {
			continue
		}
		encryptedKey, err := ExtractSubject[*bccomponents.EncryptedKey](obj)
		if err != nil {
			continue
		}
		contentKey, err := encryptedKey.Unlock(secret)
		if err != nil {
			continue
		}
		return e.DecryptSubject(contentKey)
	}
	return nil, ErrUnknownSecret
}

// IsLockedWithPassword returns whether the envelope has a 'hasSecret' assertion
// with a password-based key derivation method.
func (e *Envelope) IsLockedWithPassword() bool {
	assertions := e.AssertionsWithPredicate(knownvalues.HasSecret)
	for _, assertion := range assertions {
		obj := assertion.AsObject()
		if obj == nil {
			continue
		}
		encryptedKey, err := ExtractSubject[*bccomponents.EncryptedKey](obj)
		if err != nil {
			continue
		}
		if encryptedKey.IsPasswordBased() {
			return true
		}
	}
	return false
}

// IsLockedWithSSHAgent returns whether the envelope has a 'hasSecret' assertion
// with an SSH agent key derivation method.
func (e *Envelope) IsLockedWithSSHAgent() bool {
	assertions := e.AssertionsWithPredicate(knownvalues.HasSecret)
	for _, assertion := range assertions {
		obj := assertion.AsObject()
		if obj == nil {
			continue
		}
		encryptedKey, err := ExtractSubject[*bccomponents.EncryptedKey](obj)
		if err != nil {
			continue
		}
		if encryptedKey.IsSSHAgent() {
			return true
		}
	}
	return false
}

// AddSecret adds an additional 'hasSecret' assertion using the given key
// derivation method, secret, and content key. This allows multiple secrets
// (passwords, SSH keys, etc.) to unlock the same content.
func (e *Envelope) AddSecret(
	method bccomponents.KeyDerivationMethod,
	secret []byte,
	contentKey bccomponents.SymmetricKey,
) (*Envelope, error) {
	encryptedKey, err := bccomponents.LockEncryptedKey(method, secret, contentKey)
	if err != nil {
		return nil, err
	}
	return e.AddAssertion(knownvalues.HasSecret, encryptedKey), nil
}

// Lock is a convenience method that wraps the envelope and then locks its
// subject, effectively locking the entire envelope including assertions.
func (e *Envelope) Lock(
	method bccomponents.KeyDerivationMethod,
	secret []byte,
) (*Envelope, error) {
	return e.Wrap().LockSubject(method, secret)
}

// Unlock is a convenience method that unlocks the subject and unwraps the
// resulting envelope, reversing the Lock operation.
func (e *Envelope) Unlock(secret []byte) (*Envelope, error) {
	unlocked, err := e.UnlockSubject(secret)
	if err != nil {
		return nil, err
	}
	return unlocked.Unwrap()
}
