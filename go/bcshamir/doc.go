// Package bcshamir implements Shamir's Secret Sharing (SSS), a cryptographic
// technique in which a secret is divided into parts, called shares, in such a
// way that a threshold of several shares are needed to reconstruct the secret.
// The shares are distributed in a way that makes it impossible for an attacker
// to know anything about the secret without having a threshold of shares. If
// the number of shares is less than the threshold, then no information about
// the secret is revealed.
package bcshamir
