"""Public key for verifying digital signatures."""

from __future__ import annotations

from enum import Enum, unique
from typing import TYPE_CHECKING, Any

from bc_tags import (
    CBOR,
    CBORCase,
    Tag,
    TAG_MLDSA_PUBLIC_KEY,
    TAG_SIGNING_PUBLIC_KEY,
    TAG_SSH_TEXT_PUBLIC_KEY,
    tags_for_values,
)

if TYPE_CHECKING:
    from .._digest import Digest
    from .._reference import Reference
    from ..ed25519._ed25519_public_key import Ed25519PublicKey
    from ..mldsa._mldsa_public_key import MLDSAPublicKey

from ..ec_key import ECPublicKey, SchnorrPublicKey
from ._signature import Signature


@unique
class _SigningPublicKeyType(Enum):
    SCHNORR = "schnorr"
    ECDSA = "ecdsa"
    ED25519 = "ed25519"
    SSH = "ssh"
    MLDSA = "mldsa"


class SigningPublicKey:
    """A public key used for verifying digital signatures.

    Supports Schnorr (BIP-340), ECDSA (secp256k1), Ed25519, SSH,
    and ML-DSA key types.
    """

    __slots__ = ("_type", "_key")

    def __init__(self, key_type: _SigningPublicKeyType, key: Any) -> None:
        self._type = key_type
        self._key = key

    # --- Factory methods ---

    @staticmethod
    def from_schnorr(key: SchnorrPublicKey) -> SigningPublicKey:
        """Create from a Schnorr (x-only) public key."""
        return SigningPublicKey(_SigningPublicKeyType.SCHNORR, key)

    @staticmethod
    def from_ecdsa(key: ECPublicKey) -> SigningPublicKey:
        """Create from a compressed ECDSA public key."""
        return SigningPublicKey(_SigningPublicKeyType.ECDSA, key)

    @staticmethod
    def from_ed25519(key: Ed25519PublicKey) -> SigningPublicKey:
        """Create from an Ed25519 public key."""
        return SigningPublicKey(_SigningPublicKeyType.ED25519, key)

    @staticmethod
    def from_ssh(key: Any) -> SigningPublicKey:
        """Create from an SSH public key (cryptography library object)."""
        return SigningPublicKey(_SigningPublicKeyType.SSH, key)

    @staticmethod
    def from_mldsa(key: MLDSAPublicKey) -> SigningPublicKey:
        """Create from an ML-DSA public key."""
        return SigningPublicKey(_SigningPublicKeyType.MLDSA, key)

    # --- Accessors ---

    def to_schnorr(self) -> SchnorrPublicKey | None:
        """Return the underlying SchnorrPublicKey, or None."""
        if self._type == _SigningPublicKeyType.SCHNORR:
            return self._key
        return None

    def to_ecdsa(self) -> ECPublicKey | None:
        """Return the underlying ECPublicKey, or None."""
        if self._type == _SigningPublicKeyType.ECDSA:
            return self._key
        return None

    def to_ed25519(self) -> Any | None:
        """Return the underlying Ed25519PublicKey, or None."""
        if self._type == _SigningPublicKeyType.ED25519:
            return self._key
        return None

    def to_ssh(self) -> Any | None:
        """Return the underlying SSH public key, or None."""
        if self._type == _SigningPublicKeyType.SSH:
            return self._key
        return None

    def to_mldsa(self) -> Any | None:
        """Return the underlying MLDSAPublicKey, or None."""
        if self._type == _SigningPublicKeyType.MLDSA:
            return self._key
        return None

    # --- Verification ---

    def verify(
        self,
        signature: Signature,
        message: bytes | bytearray,
    ) -> bool:
        """Verify a signature against a message.

        The signature type must match this key type. Returns True if valid.
        """
        if self._type == _SigningPublicKeyType.SCHNORR:
            schnorr_sig = signature.to_schnorr()
            if schnorr_sig is None:
                return False
            return self._key.schnorr_verify(schnorr_sig, message)
        elif self._type == _SigningPublicKeyType.ECDSA:
            ecdsa_sig = signature.to_ecdsa()
            if ecdsa_sig is None:
                return False
            return self._key.verify(ecdsa_sig, message)
        elif self._type == _SigningPublicKeyType.ED25519:
            ed_sig = signature.to_ed25519()
            if ed_sig is None:
                return False
            return self._key.verify(ed_sig, message)
        elif self._type == _SigningPublicKeyType.SSH:
            ssh_sig = signature.to_ssh()
            if ssh_sig is None:
                return False
            return _ssh_verify(self._key, ssh_sig, message)
        elif self._type == _SigningPublicKeyType.MLDSA:
            mldsa_sig = signature.to_mldsa()
            if mldsa_sig is None:
                return False
            try:
                return self._key.verify(mldsa_sig, message)
            except Exception:
                return False
        return False

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SIGNING_PUBLIC_KEY])

    def untagged_cbor(self) -> CBOR:
        """Encode to untagged CBOR.

        - Schnorr: byte_string(32-byte key)
        - ECDSA: [1, byte_string(33-byte key)]
        - Ed25519: [2, byte_string(32-byte key)]
        - SSH: tagged text string (OpenSSH format)
        - MLDSA: delegates to MLDSAPublicKey
        """
        if self._type == _SigningPublicKeyType.SCHNORR:
            return CBOR.from_bytes(self._key.data)
        elif self._type == _SigningPublicKeyType.ECDSA:
            return CBOR.from_array([
                CBOR.from_int(1),
                CBOR.from_bytes(self._key.data),
            ])
        elif self._type == _SigningPublicKeyType.ED25519:
            return CBOR.from_array([
                CBOR.from_int(2),
                CBOR.from_bytes(self._key.data),
            ])
        elif self._type == _SigningPublicKeyType.SSH:
            from cryptography.hazmat.primitives import serialization

            ssh_pub = self._key
            ssh_bytes = ssh_pub.public_bytes(
                encoding=serialization.Encoding.OpenSSH,
                format=serialization.PublicFormat.OpenSSH,
            )
            ssh_str = ssh_bytes.decode("utf-8")
            return CBOR.from_tagged_value(
                TAG_SSH_TEXT_PUBLIC_KEY,
                CBOR.from_text(ssh_str),
            )
        elif self._type == _SigningPublicKeyType.MLDSA:
            return self._key.tagged_cbor()
        raise ValueError(f"Unknown key type: {self._type}")

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        cbor = self.untagged_cbor()
        for tag in reversed(tags):
            cbor = CBOR.from_tagged_value(tag, cbor)
        return cbor

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> SigningPublicKey:
        """Decode a SigningPublicKey from untagged CBOR."""
        case = cbor.case

        # Byte string -> Schnorr
        if case == CBORCase.BYTE_STRING:
            data = cbor.try_byte_string()
            return SigningPublicKey.from_schnorr(
                SchnorrPublicKey.from_data(data)
            )

        # Array -> discriminated type
        if case == CBORCase.ARRAY:
            elements = cbor.try_array()
            if len(elements) == 2:
                discriminator = elements[0].try_int()
                data = elements[1].try_byte_string()
                if discriminator == 1:
                    return SigningPublicKey.from_ecdsa(
                        ECPublicKey.from_data(data)
                    )
                elif discriminator == 2:
                    from ..ed25519._ed25519_public_key import Ed25519PublicKey

                    return SigningPublicKey.from_ed25519(
                        Ed25519PublicKey.from_data(data)
                    )
            raise ValueError("Invalid signing public key")

        # Tagged value -> SSH or MLDSA
        if case == CBORCase.TAGGED:
            tag, item = cbor.try_tagged_value()
            tag_val = tag.value
            if tag_val == TAG_SSH_TEXT_PUBLIC_KEY:
                ssh_str = item.try_text()
                from cryptography.hazmat.primitives.serialization import (
                    load_ssh_public_key,
                )

                key = load_ssh_public_key(ssh_str.encode("utf-8"))
                return SigningPublicKey.from_ssh(key)
            elif tag_val == TAG_MLDSA_PUBLIC_KEY:
                from ..mldsa._mldsa_public_key import MLDSAPublicKey

                key = MLDSAPublicKey.from_tagged_cbor(cbor)
                return SigningPublicKey.from_mldsa(key)
            raise ValueError("Invalid signing public key")

        raise ValueError("Invalid signing public key")

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> SigningPublicKey:
        tags = SigningPublicKey.cbor_tags()
        inner = cbor
        for tag in tags:
            inner = inner.try_expected_tagged_value(tag.value)
        return SigningPublicKey.from_untagged_cbor(inner)

    @staticmethod
    def from_tagged_cbor_data(data: bytes | bytearray) -> SigningPublicKey:
        return SigningPublicKey.from_tagged_cbor(CBOR.from_data(bytes(data)))

    # --- Reference ---

    def reference(self) -> Reference:
        from .._digest import Digest
        from .._reference import Reference

        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor_data())
        )

    def ref_hex_short(self) -> str:
        return self.reference().ref_hex_short()

    # --- Comparison ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, SigningPublicKey):
            return NotImplemented
        if self._type != other._type:
            return False
        if self._type in (
            _SigningPublicKeyType.SCHNORR,
            _SigningPublicKeyType.ECDSA,
            _SigningPublicKeyType.ED25519,
        ):
            return self._key == other._key
        if self._type == _SigningPublicKeyType.SSH:
            from cryptography.hazmat.primitives import serialization

            self_bytes = self._key.public_bytes(
                encoding=serialization.Encoding.OpenSSH,
                format=serialization.PublicFormat.OpenSSH,
            )
            other_bytes = other._key.public_bytes(
                encoding=serialization.Encoding.OpenSSH,
                format=serialization.PublicFormat.OpenSSH,
            )
            return self_bytes == other_bytes
        if self._type == _SigningPublicKeyType.MLDSA:
            return self._key == other._key
        return False

    def __hash__(self) -> int:
        if self._type in (
            _SigningPublicKeyType.SCHNORR,
            _SigningPublicKeyType.ECDSA,
        ):
            return hash((self._type, self._key.data))
        if self._type == _SigningPublicKeyType.ED25519:
            return hash((self._type, self._key.data))
        return hash(self._type)

    # --- UR ---

    def to_ur(self):
        """Encode to a UR."""
        from bc_ur import to_ur
        return to_ur(self)

    def ur_string(self) -> str:
        """Encode to a UR string."""
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @staticmethod
    def from_ur(ur) -> SigningPublicKey:
        """Decode from a UR."""
        from bc_ur import from_ur
        return from_ur(SigningPublicKey, ur)

    @staticmethod
    def from_ur_string(ur_string: str) -> SigningPublicKey:
        """Decode from a UR string."""
        from bc_ur import from_ur_string
        return from_ur_string(SigningPublicKey, ur_string)

    def __repr__(self) -> str:
        return f"SigningPublicKey({self._type.value})"

    def __str__(self) -> str:
        if self._type == _SigningPublicKeyType.SCHNORR:
            return (
                f"SigningPublicKey({self.ref_hex_short()}, "
                f"{self._key})"
            )
        elif self._type == _SigningPublicKeyType.ECDSA:
            return (
                f"SigningPublicKey({self.ref_hex_short()}, "
                f"{self._key})"
            )
        elif self._type == _SigningPublicKeyType.ED25519:
            return (
                f"SigningPublicKey({self.ref_hex_short()}, "
                f"{self._key})"
            )
        elif self._type == _SigningPublicKeyType.SSH:
            return f"SigningPublicKey({self.ref_hex_short()}, SSH)"
        elif self._type == _SigningPublicKeyType.MLDSA:
            return (
                f"SigningPublicKey({self.ref_hex_short()}, "
                f"{self._key})"
            )
        return f"SigningPublicKey({self._type})"


def _ssh_verify(
    public_key: Any,
    pem_signature: str,
    message: bytes | bytearray,
) -> bool:
    """Verify an SSH signature in sshsig PEM format.

    Parses the SSHSIG envelope, extracts the signed data, and verifies
    the inner signature using the appropriate algorithm.
    """
    import base64
    import hashlib
    import struct

    from cryptography.exceptions import InvalidSignature
    from cryptography.hazmat.primitives import hashes
    from cryptography.hazmat.primitives.asymmetric import (
        dsa,
        ec,
        ed25519,
        utils,
    )

    MAGIC_PREAMBLE = b"SSHSIG"

    # Parse PEM
    lines = pem_signature.strip().splitlines()
    if lines[0].strip() != "-----BEGIN SSH SIGNATURE-----":
        return False
    if lines[-1].strip() != "-----END SSH SIGNATURE-----":
        return False
    b64_data = "".join(line.strip() for line in lines[1:-1])
    envelope = base64.b64decode(b64_data)

    def read_string(data: bytes, offset: int) -> tuple[bytes, int]:
        length = struct.unpack(">I", data[offset : offset + 4])[0]
        end = offset + 4 + length
        return data[offset + 4 : end], end

    # Parse envelope
    if not envelope.startswith(MAGIC_PREAMBLE):
        return False
    pos = len(MAGIC_PREAMBLE)
    version = struct.unpack(">I", envelope[pos : pos + 4])[0]
    pos += 4
    if version != 1:
        return False

    _public_key_blob, pos = read_string(envelope, pos)
    namespace_bytes, pos = read_string(envelope, pos)
    _reserved, pos = read_string(envelope, pos)
    hash_name_bytes, pos = read_string(envelope, pos)
    sig_blob, pos = read_string(envelope, pos)

    namespace = namespace_bytes.decode("utf-8")
    hash_name = hash_name_bytes.decode("utf-8")

    # Compute message hash
    msg = bytes(message)
    if hash_name == "sha256":
        msg_hash = hashlib.sha256(msg).digest()
    elif hash_name == "sha512":
        msg_hash = hashlib.sha512(msg).digest()
    else:
        return False

    # Build the signed data (same format as signing)
    def encode_string(data: bytes) -> bytes:
        return struct.pack(">I", len(data)) + data

    signed_data = (
        MAGIC_PREAMBLE
        + struct.pack(">I", 1)
        + encode_string(namespace.encode("utf-8"))
        + encode_string(b"")
        + encode_string(hash_name.encode("utf-8"))
        + encode_string(msg_hash)
    )

    # Parse the inner signature blob
    algo_name, blob_pos = read_string(sig_blob, 0)
    raw_sig, _ = read_string(sig_blob, blob_pos)

    # Verify with the appropriate algorithm
    try:
        if isinstance(public_key, ed25519.Ed25519PublicKey):
            public_key.verify(raw_sig, signed_data)
            return True
        elif isinstance(public_key, ec.EllipticCurvePublicKey):
            curve = public_key.curve
            if isinstance(curve, ec.SECP256R1):
                hash_obj = hashes.SHA256()
            elif isinstance(curve, ec.SECP384R1):
                hash_obj = hashes.SHA384()
            else:
                return False
            public_key.verify(raw_sig, signed_data, ec.ECDSA(hash_obj))
            return True
        elif isinstance(public_key, dsa.DSAPublicKey):
            # Convert raw signature to DER
            r = int.from_bytes(raw_sig[:20], "big")
            s = int.from_bytes(raw_sig[20:40], "big")
            der_sig = utils.encode_dss_signature(r, s)
            public_key.verify(der_sig, signed_data, hashes.SHA1())
            return True
    except InvalidSignature:
        return False
    except Exception:
        return False

    return False
