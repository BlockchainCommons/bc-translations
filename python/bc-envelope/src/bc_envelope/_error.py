"""Error types for Gordian Envelope operations.

Provides a hierarchy of exceptions for various conditions that can occur
when working with envelopes, including structure validation, operation
constraints, and extension-specific errors.
"""

from __future__ import annotations


class EnvelopeError(Exception):
    """Base exception for all Gordian Envelope errors."""


# ---------------------------------------------------------------------------
# Base Specification
# ---------------------------------------------------------------------------

class AlreadyElided(EnvelopeError):
    """Envelope was elided, so it cannot be compressed or encrypted."""

    def __init__(self) -> None:
        super().__init__("envelope was elided, so it cannot be compressed or encrypted")


class AmbiguousPredicate(EnvelopeError):
    """More than one assertion matches the predicate."""

    def __init__(self) -> None:
        super().__init__("more than one assertion matches the predicate")


class InvalidDigest(EnvelopeError):
    """Digest did not match."""

    def __init__(self) -> None:
        super().__init__("digest did not match")


class InvalidFormat(EnvelopeError):
    """Invalid envelope format."""

    def __init__(self, msg: str = "invalid format") -> None:
        super().__init__(msg)


class MissingDigest(EnvelopeError):
    """A digest was expected but not found."""

    def __init__(self) -> None:
        super().__init__("a digest was expected but not found")


class NonexistentPredicate(EnvelopeError):
    """No assertion matches the predicate."""

    def __init__(self) -> None:
        super().__init__("no assertion matches the predicate")


class NotWrapped(EnvelopeError):
    """Cannot unwrap an envelope that was not wrapped."""

    def __init__(self) -> None:
        super().__init__("cannot unwrap an envelope that was not wrapped")


class NotLeaf(EnvelopeError):
    """The envelope's subject is not a leaf."""

    def __init__(self) -> None:
        super().__init__("the envelope's subject is not a leaf")


class NotAssertion(EnvelopeError):
    """The envelope's subject is not an assertion."""

    def __init__(self) -> None:
        super().__init__("the envelope's subject is not an assertion")


class InvalidAssertion(EnvelopeError):
    """Assertion must be a map with exactly one element."""

    def __init__(self) -> None:
        super().__init__("assertion must be a map with exactly one element")


# ---------------------------------------------------------------------------
# Attachments Extension
# ---------------------------------------------------------------------------

class InvalidAttachment(EnvelopeError):
    """Invalid attachment."""

    def __init__(self) -> None:
        super().__init__("invalid attachment")


class NonexistentAttachment(EnvelopeError):
    """Nonexistent attachment."""

    def __init__(self) -> None:
        super().__init__("nonexistent attachment")


class AmbiguousAttachment(EnvelopeError):
    """Ambiguous attachment."""

    def __init__(self) -> None:
        super().__init__("ambiguous attachment")


# ---------------------------------------------------------------------------
# Edges Extension
# ---------------------------------------------------------------------------

class EdgeMissingIsA(EnvelopeError):
    """Edge missing 'isA' assertion."""

    def __init__(self) -> None:
        super().__init__("edge missing 'isA' assertion")


class EdgeMissingSource(EnvelopeError):
    """Edge missing 'source' assertion."""

    def __init__(self) -> None:
        super().__init__("edge missing 'source' assertion")


class EdgeMissingTarget(EnvelopeError):
    """Edge missing 'target' assertion."""

    def __init__(self) -> None:
        super().__init__("edge missing 'target' assertion")


class EdgeDuplicateIsA(EnvelopeError):
    """Edge has duplicate 'isA' assertions."""

    def __init__(self) -> None:
        super().__init__("edge has duplicate 'isA' assertions")


class EdgeDuplicateSource(EnvelopeError):
    """Edge has duplicate 'source' assertions."""

    def __init__(self) -> None:
        super().__init__("edge has duplicate 'source' assertions")


class EdgeDuplicateTarget(EnvelopeError):
    """Edge has duplicate 'target' assertions."""

    def __init__(self) -> None:
        super().__init__("edge has duplicate 'target' assertions")


class EdgeUnexpectedAssertion(EnvelopeError):
    """Edge has unexpected assertion."""

    def __init__(self) -> None:
        super().__init__("edge has unexpected assertion")


class NonexistentEdge(EnvelopeError):
    """Nonexistent edge."""

    def __init__(self) -> None:
        super().__init__("nonexistent edge")


class AmbiguousEdge(EnvelopeError):
    """Ambiguous edge."""

    def __init__(self) -> None:
        super().__init__("ambiguous edge")


# ---------------------------------------------------------------------------
# Compression Extension
# ---------------------------------------------------------------------------

class AlreadyCompressed(EnvelopeError):
    """Envelope was already compressed."""

    def __init__(self) -> None:
        super().__init__("envelope was already compressed")


class NotCompressed(EnvelopeError):
    """Cannot decompress an envelope that was not compressed."""

    def __init__(self) -> None:
        super().__init__("cannot decompress an envelope that was not compressed")


# ---------------------------------------------------------------------------
# Symmetric Encryption Extension
# ---------------------------------------------------------------------------

class AlreadyEncrypted(EnvelopeError):
    """Envelope was already encrypted or compressed, so it cannot be encrypted."""

    def __init__(self) -> None:
        super().__init__(
            "envelope was already encrypted or compressed, so it cannot be encrypted"
        )


class NotEncrypted(EnvelopeError):
    """Cannot decrypt an envelope that was not encrypted."""

    def __init__(self) -> None:
        super().__init__("cannot decrypt an envelope that was not encrypted")


# ---------------------------------------------------------------------------
# Known Values Extension
# ---------------------------------------------------------------------------

class NotKnownValue(EnvelopeError):
    """The envelope's subject is not a known value."""

    def __init__(self) -> None:
        super().__init__("the envelope's subject is not a known value")


class SubjectNotUnit(EnvelopeError):
    """The subject of the envelope is not the unit value."""

    def __init__(self) -> None:
        super().__init__("the subject of the envelope is not the unit value")


# ---------------------------------------------------------------------------
# Public Key Encryption Extension
# ---------------------------------------------------------------------------

class UnknownRecipient(EnvelopeError):
    """Unknown recipient."""

    def __init__(self) -> None:
        super().__init__("unknown recipient")


# ---------------------------------------------------------------------------
# Encrypted Key Extension
# ---------------------------------------------------------------------------

class UnknownSecret(EnvelopeError):
    """Secret not found."""

    def __init__(self) -> None:
        super().__init__("secret not found")


# ---------------------------------------------------------------------------
# Public Key Signing Extension
# ---------------------------------------------------------------------------

class UnverifiedSignature(EnvelopeError):
    """Could not verify a signature."""

    def __init__(self) -> None:
        super().__init__("could not verify a signature")


class InvalidOuterSignatureType(EnvelopeError):
    """Unexpected outer signature object type."""

    def __init__(self) -> None:
        super().__init__("unexpected outer signature object type")


class InvalidInnerSignatureType(EnvelopeError):
    """Unexpected inner signature object type."""

    def __init__(self) -> None:
        super().__init__("unexpected inner signature object type")


class UnverifiedInnerSignature(EnvelopeError):
    """Inner signature not made with same key as outer signature."""

    def __init__(self) -> None:
        super().__init__("inner signature not made with same key as outer signature")


class InvalidSignatureType(EnvelopeError):
    """Unexpected signature object type."""

    def __init__(self) -> None:
        super().__init__("unexpected signature object type")


# ---------------------------------------------------------------------------
# SSKR Extension
# ---------------------------------------------------------------------------

class InvalidShares(EnvelopeError):
    """Invalid SSKR shares."""

    def __init__(self) -> None:
        super().__init__("invalid SSKR shares")


# ---------------------------------------------------------------------------
# Types Extension
# ---------------------------------------------------------------------------

class InvalidType(EnvelopeError):
    """Invalid type."""

    def __init__(self) -> None:
        super().__init__("invalid type")


class AmbiguousType(EnvelopeError):
    """Ambiguous type."""

    def __init__(self) -> None:
        super().__init__("ambiguous type")


# ---------------------------------------------------------------------------
# Expressions Extension
# ---------------------------------------------------------------------------

class UnexpectedResponseID(EnvelopeError):
    """Unexpected response ID."""

    def __init__(self) -> None:
        super().__init__("unexpected response ID")


class InvalidResponse(EnvelopeError):
    """Invalid response."""

    def __init__(self) -> None:
        super().__init__("invalid response")


# ---------------------------------------------------------------------------
# General error
# ---------------------------------------------------------------------------

class GeneralError(EnvelopeError):
    """A general envelope error with a custom message."""

    def __init__(self, msg: str) -> None:
        super().__init__(msg)
