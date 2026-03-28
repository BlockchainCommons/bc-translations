"""Signature extension for Gordian Envelope.

Provides methods for signing envelopes and verifying signatures, including
multi-signature and metadata-aware variants.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Signature, SigningOptions
from bc_components.signing import Signer, Verifier
import known_values

from ._signature_metadata import SignatureMetadata

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Creating signatures
# ---------------------------------------------------------------------------

def add_signature(self: Envelope, signer: Signer) -> Envelope:
    """Add a ``'signed': Signature`` assertion to the envelope."""
    return add_signature_opt(self, signer, None, None)


def add_signature_opt(
    self: Envelope,
    signer: Signer,
    options: SigningOptions | None = None,
    metadata: SignatureMetadata | None = None,
) -> Envelope:
    """Add a signature assertion with optional options and metadata."""
    from ._envelope import Envelope as Env

    digest_bytes = bytes(self.subject().digest().data)
    signature_obj = signer.sign_with_options(digest_bytes, options)
    sig_envelope = Env(signature_obj)

    if metadata is not None and metadata.has_assertions():
        sig_with_meta = sig_envelope
        for assertion in metadata.assertions:
            sig_with_meta = sig_with_meta.add_assertion_envelope(assertion.to_envelope())
        sig_with_meta = sig_with_meta.wrap()

        outer_sig = Env(
            signer.sign_with_options(bytes(sig_with_meta.digest().data), options)
        )
        sig_envelope = sig_with_meta.add_assertion(known_values.SIGNED, outer_sig)

    return self.add_assertion(known_values.SIGNED, sig_envelope)


def add_signatures(self: Envelope, signers: list[Signer]) -> Envelope:
    """Add signature assertions from multiple signers."""
    result = self
    for signer in signers:
        result = add_signature(result, signer)
    return result


def add_signatures_opt(
    self: Envelope,
    signers: list[tuple[Signer, SigningOptions | None, SignatureMetadata | None]],
) -> Envelope:
    """Add signature assertions from multiple signers with individual options."""
    result = self
    for signer, options, metadata in signers:
        result = add_signature_opt(result, signer, options, metadata)
    return result


# ---------------------------------------------------------------------------
# Verifying signatures
# ---------------------------------------------------------------------------

def _is_signature_from_key(
    self: Envelope,
    signature: Signature,
    key: Verifier,
) -> bool:
    """Check whether *signature* was made by *key* over this envelope's subject."""
    return key.verify(signature, bytes(self.subject().digest().data))


def is_verified_signature(
    self: Envelope,
    signature: Signature,
    public_key: Verifier,
) -> bool:
    """Return whether *signature* is valid for this envelope's subject."""
    return _is_signature_from_key(self, signature, public_key)


def verify_signature(
    self: Envelope,
    signature: Signature,
    public_key: Verifier,
) -> Envelope:
    """Raise ``UnverifiedSignature`` if *signature* is not valid; return self otherwise."""
    from ._error import UnverifiedSignature

    if not _is_signature_from_key(self, signature, public_key):
        raise UnverifiedSignature()
    return self


def _has_some_signature_from_key_returning_metadata(
    self: Envelope,
    key: Verifier,
) -> Envelope | None:
    """Return the signature metadata envelope if *key* signed this envelope, else None.

    Raises on structural errors in signature assertions.
    """
    from ._error import (
        InvalidInnerSignatureType,
        InvalidOuterSignatureType,
        InvalidSignatureType,
        UnverifiedInnerSignature,
    )

    signature_objects = self.objects_for_predicate(known_values.SIGNED)
    for sig_obj in signature_objects:
        sig_obj_subject = sig_obj.subject()
        if sig_obj_subject.is_wrapped():
            # Metadata-aware signature path
            try:
                outer_sig_obj = sig_obj.object_for_predicate(known_values.SIGNED)
            except Exception:
                continue
            try:
                outer_sig: Signature = outer_sig_obj.extract_subject()
            except Exception:
                raise InvalidOuterSignatureType()

            if not _is_signature_from_key(sig_obj_subject, outer_sig, key):
                continue

            sig_meta_envelope = sig_obj_subject.try_unwrap()
            try:
                inner_sig: Signature = sig_meta_envelope.extract_subject()
            except Exception:
                raise InvalidInnerSignatureType()

            if not _is_signature_from_key(self, inner_sig, key):
                raise UnverifiedInnerSignature()

            return sig_meta_envelope
        else:
            # Simple signature path
            try:
                sig: Signature = sig_obj.extract_subject()
            except Exception:
                raise InvalidSignatureType()

            if not _is_signature_from_key(self, sig, key):
                continue
            return sig_obj

    return None


def _has_some_signature_from_key(self: Envelope, key: Verifier) -> bool:
    return _has_some_signature_from_key_returning_metadata(self, key) is not None


def has_signature_from(self: Envelope, public_key: Verifier) -> bool:
    """Return whether the envelope has a valid signature from *public_key*."""
    return _has_some_signature_from_key(self, public_key)


def has_signature_from_returning_metadata(
    self: Envelope,
    public_key: Verifier,
) -> Envelope | None:
    """Return the metadata envelope if signed by *public_key*, else ``None``."""
    return _has_some_signature_from_key_returning_metadata(self, public_key)


def verify_signature_from(self: Envelope, public_key: Verifier) -> Envelope:
    """Raise ``UnverifiedSignature`` unless *public_key* signed this envelope."""
    from ._error import UnverifiedSignature

    if not _has_some_signature_from_key(self, public_key):
        raise UnverifiedSignature()
    return self


def verify_signature_from_returning_metadata(
    self: Envelope,
    public_key: Verifier,
) -> Envelope:
    """Verify and return the signature metadata envelope."""
    from ._error import UnverifiedSignature

    metadata = _has_some_signature_from_key_returning_metadata(self, public_key)
    if metadata is None:
        raise UnverifiedSignature()
    return metadata


def has_signatures_from(
    self: Envelope,
    public_keys: list[Verifier],
) -> bool:
    """Return whether *all* keys have signed."""
    return has_signatures_from_threshold(self, public_keys, None)


def has_signatures_from_threshold(
    self: Envelope,
    public_keys: list[Verifier],
    threshold: int | None = None,
) -> bool:
    """Return whether at least *threshold* of *public_keys* have signed.

    If *threshold* is ``None``, all keys must have signed.
    """
    threshold = threshold if threshold is not None else len(public_keys)
    count = 0
    for key in public_keys:
        if _has_some_signature_from_key(self, key):
            count += 1
            if count >= threshold:
                return True
    return False


def verify_signatures_from(
    self: Envelope,
    public_keys: list[Verifier],
) -> Envelope:
    """Raise ``UnverifiedSignature`` unless all *public_keys* have signed."""
    return verify_signatures_from_threshold(self, public_keys, None)


def verify_signatures_from_threshold(
    self: Envelope,
    public_keys: list[Verifier],
    threshold: int | None = None,
) -> Envelope:
    """Raise unless at least *threshold* of *public_keys* have signed."""
    from ._error import UnverifiedSignature

    if not has_signatures_from_threshold(self, public_keys, threshold):
        raise UnverifiedSignature()
    return self


# ---------------------------------------------------------------------------
# Convenience: sign / verify (wrap, sign, unwrap)
# ---------------------------------------------------------------------------

def sign(self: Envelope, signer: Signer) -> Envelope:
    """Wrap and sign the entire envelope."""
    return sign_opt(self, signer, None)


def sign_opt(
    self: Envelope,
    signer: Signer,
    options: SigningOptions | None = None,
) -> Envelope:
    """Wrap and sign the entire envelope with optional signing options."""
    return add_signature_opt(self.wrap(), signer, options, None)


def verify(self: Envelope, verifier: Verifier) -> Envelope:
    """Verify the wrapped envelope's signature and unwrap."""
    return verify_signature_from(self, verifier).try_unwrap()


def verify_returning_metadata(
    self: Envelope,
    verifier: Verifier,
) -> tuple[Envelope, Envelope]:
    """Verify signature and return ``(unwrapped_envelope, metadata_envelope)``."""
    metadata = verify_signature_from_returning_metadata(self, verifier)
    return (self.try_unwrap(), metadata)
