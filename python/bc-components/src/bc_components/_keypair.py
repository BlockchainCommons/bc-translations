"""Factory functions for generating key pairs."""

from __future__ import annotations

from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator

from ._private_keys import PrivateKeys
from ._public_keys import PublicKeys
from .encapsulation._encapsulation_scheme import EncapsulationScheme
from .signing._signature_scheme import SignatureScheme


def keypair() -> tuple[PrivateKeys, PublicKeys]:
    """Generate a key pair using the default signature and encapsulation schemes.

    Returns a tuple of (PrivateKeys, PublicKeys) using Schnorr + X25519.
    """
    return keypair_opt(SignatureScheme.default(), EncapsulationScheme.default())


def keypair_using(
    rng: RandomNumberGenerator,
) -> tuple[PrivateKeys, PublicKeys]:
    """Generate a key pair using the default schemes and a custom RNG."""
    return keypair_opt_using(
        SignatureScheme.default(),
        EncapsulationScheme.default(),
        rng,
    )


def keypair_opt(
    signature_scheme: SignatureScheme,
    encapsulation_scheme: EncapsulationScheme,
) -> tuple[PrivateKeys, PublicKeys]:
    """Generate a key pair with specified signature and encapsulation schemes."""
    signing_private_key, signing_public_key = signature_scheme.keypair()
    encapsulation_private_key, encapsulation_public_key = (
        encapsulation_scheme.keypair()
    )
    private_keys = PrivateKeys.with_keys(
        signing_private_key, encapsulation_private_key,
    )
    public_keys = PublicKeys.new(
        signing_public_key, encapsulation_public_key,
    )
    return private_keys, public_keys


def keypair_opt_using(
    signature_scheme: SignatureScheme,
    encapsulation_scheme: EncapsulationScheme,
    rng: RandomNumberGenerator,
) -> tuple[PrivateKeys, PublicKeys]:
    """Generate a key pair with specified schemes using a custom RNG."""
    signing_private_key, signing_public_key = signature_scheme.keypair_using(rng)
    encapsulation_private_key, encapsulation_public_key = (
        encapsulation_scheme.keypair_using(rng)
    )
    private_keys = PrivateKeys.with_keys(
        signing_private_key, encapsulation_private_key,
    )
    public_keys = PublicKeys.new(
        signing_public_key, encapsulation_public_key,
    )
    return private_keys, public_keys
