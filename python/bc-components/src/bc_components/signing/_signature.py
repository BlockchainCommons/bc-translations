"""Digital signature types for various cryptographic schemes."""

from __future__ import annotations

from enum import Enum, unique
from typing import TYPE_CHECKING, Any

import bc_crypto
from bc_tags import (
    CBOR,
    CBORCase,
    Tag,
    TAG_MLDSA_SIGNATURE,
    TAG_SIGNATURE,
    TAG_SSH_TEXT_SIGNATURE,
    tags_for_values,
)

if TYPE_CHECKING:
    from ..mldsa._mldsa_signature import MLDSASignature
    from ._signature_scheme import SignatureScheme


@unique
class _SignatureType(Enum):
    SCHNORR = "schnorr"
    ECDSA = "ecdsa"
    ED25519 = "ed25519"
    SSH = "ssh"
    MLDSA = "mldsa"


class Signature:
    """A digital signature created with various signature algorithms.

    Supports Schnorr (BIP-340), ECDSA (secp256k1), Ed25519, SSH, and ML-DSA
    signature types. Uses a tagged union internally.
    """

    __slots__ = ("_type", "_data")

    def __init__(self, sig_type: _SignatureType, data: Any) -> None:
        self._type = sig_type
        self._data = data

    # --- Factory methods ---

    @staticmethod
    def schnorr_from_data(data: bytes | bytearray) -> Signature:
        """Create a Schnorr signature from a 64-byte array."""
        data = bytes(data)
        if len(data) != bc_crypto.SCHNORR_SIGNATURE_SIZE:
            raise ValueError(
                f"Schnorr signature must be {bc_crypto.SCHNORR_SIGNATURE_SIZE} bytes, "
                f"got {len(data)}"
            )
        return Signature(_SignatureType.SCHNORR, data)

    @staticmethod
    def ecdsa_from_data(data: bytes | bytearray) -> Signature:
        """Create an ECDSA signature from a 64-byte array."""
        data = bytes(data)
        if len(data) != bc_crypto.ECDSA_SIGNATURE_SIZE:
            raise ValueError(
                f"ECDSA signature must be {bc_crypto.ECDSA_SIGNATURE_SIZE} bytes, "
                f"got {len(data)}"
            )
        return Signature(_SignatureType.ECDSA, data)

    @staticmethod
    def ed25519_from_data(data: bytes | bytearray) -> Signature:
        """Create an Ed25519 signature from a 64-byte array."""
        data = bytes(data)
        if len(data) != bc_crypto.ED25519_SIGNATURE_SIZE:
            raise ValueError(
                f"Ed25519 signature must be {bc_crypto.ED25519_SIGNATURE_SIZE} bytes, "
                f"got {len(data)}"
            )
        return Signature(_SignatureType.ED25519, data)

    @staticmethod
    def from_ssh(sig: Any) -> Signature:
        """Create an SSH signature from an ssh SshSig-like object.

        The sig parameter is the PEM string representation of the SSH signature.
        """
        return Signature(_SignatureType.SSH, sig)

    @staticmethod
    def from_mldsa(sig: MLDSASignature) -> Signature:
        """Create an ML-DSA signature from an MLDSASignature object."""
        return Signature(_SignatureType.MLDSA, sig)

    # --- Accessors ---

    def to_schnorr(self) -> bytes | None:
        """Return the Schnorr signature data, or None if not Schnorr."""
        if self._type == _SignatureType.SCHNORR:
            return self._data
        return None

    def to_ecdsa(self) -> bytes | None:
        """Return the ECDSA signature data, or None if not ECDSA."""
        if self._type == _SignatureType.ECDSA:
            return self._data
        return None

    def to_ed25519(self) -> bytes | None:
        """Return the Ed25519 signature data, or None if not Ed25519."""
        if self._type == _SignatureType.ED25519:
            return self._data
        return None

    def to_ssh(self) -> Any | None:
        """Return the SSH signature, or None if not SSH."""
        if self._type == _SignatureType.SSH:
            return self._data
        return None

    def to_mldsa(self) -> Any | None:
        """Return the ML-DSA signature, or None if not ML-DSA."""
        if self._type == _SignatureType.MLDSA:
            return self._data
        return None

    def scheme(self) -> SignatureScheme:
        """Determine the signature scheme used to create this signature."""
        from ._signature_scheme import SignatureScheme

        if self._type == _SignatureType.SCHNORR:
            return SignatureScheme.SCHNORR
        elif self._type == _SignatureType.ECDSA:
            return SignatureScheme.ECDSA
        elif self._type == _SignatureType.ED25519:
            return SignatureScheme.ED25519
        elif self._type == _SignatureType.SSH:
            # Determine SSH algorithm from stored signature info
            sig_data = self._data
            if isinstance(sig_data, str):
                # PEM-encoded SSH signature: parse algorithm from content
                if "ed25519" in sig_data.lower():
                    return SignatureScheme.SSH_ED25519
                elif "ecdsa-sha2-nistp256" in sig_data:
                    return SignatureScheme.SSH_ECDSA_P256
                elif "ecdsa-sha2-nistp384" in sig_data:
                    return SignatureScheme.SSH_ECDSA_P384
                elif "ssh-dss" in sig_data:
                    return SignatureScheme.SSH_DSA
            raise ValueError("Unsupported SSH signature algorithm")
        elif self._type == _SignatureType.MLDSA:
            sig = self._data
            level = sig.level
            from ..mldsa._mldsa_level import MLDSALevel

            if level == MLDSALevel.MLDSA44:
                return SignatureScheme.MLDSA44
            elif level == MLDSALevel.MLDSA65:
                return SignatureScheme.MLDSA65
            elif level == MLDSALevel.MLDSA87:
                return SignatureScheme.MLDSA87
            raise ValueError("Unknown MLDSA level")
        raise ValueError("Unknown signature type")

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SIGNATURE])

    def untagged_cbor(self) -> CBOR:
        """Encode the signature to untagged CBOR.

        - Schnorr: byte string
        - ECDSA: [1, byte_string]
        - Ed25519: [2, byte_string]
        - SSH: tagged text string (PEM)
        - MLDSA: delegates to MLDSASignature
        """
        if self._type == _SignatureType.SCHNORR:
            return CBOR.from_bytes(self._data)
        elif self._type == _SignatureType.ECDSA:
            return CBOR.from_array([
                CBOR.from_int(1),
                CBOR.from_bytes(self._data),
            ])
        elif self._type == _SignatureType.ED25519:
            return CBOR.from_array([
                CBOR.from_int(2),
                CBOR.from_bytes(self._data),
            ])
        elif self._type == _SignatureType.SSH:
            pem = self._data
            return CBOR.from_tagged_value(
                TAG_SSH_TEXT_SIGNATURE,
                CBOR.from_text(pem),
            )
        elif self._type == _SignatureType.MLDSA:
            return self._data.tagged_cbor()
        raise ValueError("Unknown signature type")

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        cbor = self.untagged_cbor()
        for tag in reversed(tags):
            cbor = CBOR.from_tagged_value(tag, cbor)
        return cbor

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> Signature:
        """Decode a Signature from untagged CBOR."""
        case = cbor.case

        # Byte string -> Schnorr signature
        if case == CBORCase.BYTE_STRING:
            data = cbor.try_byte_string()
            return Signature.schnorr_from_data(data)

        # Array -> discriminated type
        if case == CBORCase.ARRAY:
            elements = cbor.try_array()
            if len(elements) == 2:
                first = elements[0]
                second = elements[1]
                # Check if first element is a byte string (legacy Schnorr encoding)
                if first.case == CBORCase.BYTE_STRING:
                    return Signature.schnorr_from_data(first.try_byte_string())
                discriminator = first.try_int()
                if discriminator == 1:
                    return Signature.ecdsa_from_data(second.try_byte_string())
                elif discriminator == 2:
                    return Signature.ed25519_from_data(second.try_byte_string())
            raise ValueError("Invalid signature format")

        # Tagged value -> MLDSA or SSH
        if case == CBORCase.TAGGED:
            tag, item = cbor.try_tagged_value()
            tag_val = tag.value
            if tag_val == TAG_MLDSA_SIGNATURE:
                from ..mldsa._mldsa_signature import MLDSASignature

                sig = MLDSASignature.from_tagged_cbor(cbor)
                return Signature.from_mldsa(sig)
            elif tag_val == TAG_SSH_TEXT_SIGNATURE:
                pem = item.try_text()
                return Signature.from_ssh(pem)
            raise ValueError("Invalid signature format")

        raise ValueError("Invalid signature format")

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> Signature:
        tags = Signature.cbor_tags()
        inner = cbor
        for tag in tags:
            inner = inner.try_expected_tagged_value(tag.value)
        return Signature.from_untagged_cbor(inner)

    @staticmethod
    def from_tagged_cbor_data(data: bytes | bytearray) -> Signature:
        return Signature.from_tagged_cbor(CBOR.from_data(bytes(data)))

    # --- Comparison ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Signature):
            return NotImplemented
        if self._type != other._type:
            return False
        if self._type in (_SignatureType.SCHNORR, _SignatureType.ECDSA, _SignatureType.ED25519):
            return self._data == other._data
        if self._type == _SignatureType.SSH:
            return self._data == other._data
        if self._type == _SignatureType.MLDSA:
            return self._data.data == other._data.data
        return False

    def __hash__(self) -> int:
        if self._type in (_SignatureType.SCHNORR, _SignatureType.ECDSA, _SignatureType.ED25519):
            return hash((self._type, self._data))
        if self._type == _SignatureType.SSH:
            return hash((self._type, self._data))
        return hash(self._type)

    def __repr__(self) -> str:
        if self._type in (_SignatureType.SCHNORR, _SignatureType.ECDSA, _SignatureType.ED25519):
            return f"{self._type.value.upper()}({self._data.hex()})"
        if self._type == _SignatureType.SSH:
            return f"SSH({self._data!r})"
        if self._type == _SignatureType.MLDSA:
            return f"MLDSA({self._data!r})"
        return f"Signature({self._type})"

