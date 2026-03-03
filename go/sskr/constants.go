package sskr

import bcshamir "github.com/nickel-blockchaincommons/bcshamir-go"

// MinSecretLen is the minimum length of a secret.
const MinSecretLen = bcshamir.MinSecretLen

// MaxSecretLen is the maximum length of a secret.
const MaxSecretLen = bcshamir.MaxSecretLen

// MaxShareCount is the maximum number of shares that can be generated from a secret.
const MaxShareCount = bcshamir.MaxShareCount

// MaxGroupsCount is the maximum number of groups in a split.
const MaxGroupsCount = MaxShareCount

// MetadataSizeBytes is the number of bytes used to encode share metadata.
const MetadataSizeBytes = 5

// MinSerializeSizeBytes is the minimum number of bytes required to encode a share.
const MinSerializeSizeBytes = MetadataSizeBytes + MinSecretLen
