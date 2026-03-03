"""Private key for creating digital signatures."""

from __future__ import annotations

from dataclasses import dataclass
from enum import Enum, unique
from typing import TYPE_CHECKING, Any

from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
from bc_tags import (
    CBOR,
    CBORCase,
    Tag,
    TAG_MLDSA_PRIVATE_KEY,
    TAG_SIGNING_PRIVATE_KEY,
    TAG_SSH_TEXT_PRIVATE_KEY,
    tags_for_values,
)

if TYPE_CHECKING:
    from .._digest import Digest
    from .._reference import Reference
    from ..ed25519._ed25519_private_key import Ed25519PrivateKey
    from ..mldsa._mldsa_private_key import MLDSAPrivateKey

from ..ec_key import ECPrivateKey
from ._signature import Signature
from ._signing_public_key import SigningPublicKey


@dataclass
class SchnorrSigningOptions:
    """Options for Schnorr signatures with a custom RNG."""

    rng: RandomNumberGenerator


@dataclass
class SshSigningOptions:
    """Options for SSH signatures with namespace and hash algorithm."""

    namespace: str
    hash_alg: str  # e.g., "sha256", "sha512"


SigningOptions = SchnorrSigningOptions | SshSigningOptions


@unique
class _SigningPrivateKeyType(Enum):
    SCHNORR = "schnorr"
    ECDSA = "ecdsa"
    ED25519 = "ed25519"
    SSH = "ssh"
    MLDSA = "mldsa"


class SigningPrivateKey:
    """A private key used for creating digital signatures.

    Supports Schnorr (secp256k1), ECDSA (secp256k1), Ed25519, SSH,
    and ML-DSA key types.
    """

    __slots__ = ("_type", "_key")

    def __init__(self, key_type: _SigningPrivateKeyType, key: Any) -> None:
        self._type = key_type
        self._key = key

    # --- Factory methods ---

    @staticmethod
    def new_schnorr(key: ECPrivateKey) -> SigningPrivateKey:
        """Create a Schnorr signing private key from an ECPrivateKey."""
        return SigningPrivateKey(_SigningPrivateKeyType.SCHNORR, key)

    @staticmethod
    def new_ecdsa(key: ECPrivateKey) -> SigningPrivateKey:
        """Create an ECDSA signing private key from an ECPrivateKey."""
        return SigningPrivateKey(_SigningPrivateKeyType.ECDSA, key)

    @staticmethod
    def new_ed25519(key: Ed25519PrivateKey) -> SigningPrivateKey:
        """Create an Ed25519 signing private key from an Ed25519PrivateKey."""
        return SigningPrivateKey(_SigningPrivateKeyType.ED25519, key)

    @staticmethod
    def new_ssh(key: Any) -> SigningPrivateKey:
        """Create an SSH signing private key from an SSH private key object."""
        return SigningPrivateKey(_SigningPrivateKeyType.SSH, key)

    @staticmethod
    def new_mldsa(key: MLDSAPrivateKey) -> SigningPrivateKey:
        """Create an ML-DSA signing private key from an MLDSAPrivateKey."""
        return SigningPrivateKey(_SigningPrivateKeyType.MLDSA, key)

    # --- Accessors ---

    def to_schnorr(self) -> ECPrivateKey | None:
        """Return the underlying ECPrivateKey if this is a Schnorr key."""
        if self._type == _SigningPrivateKeyType.SCHNORR:
            return self._key
        return None

    def is_schnorr(self) -> bool:
        return self._type == _SigningPrivateKeyType.SCHNORR

    def to_ecdsa(self) -> ECPrivateKey | None:
        """Return the underlying ECPrivateKey if this is an ECDSA key."""
        if self._type == _SigningPrivateKeyType.ECDSA:
            return self._key
        return None

    def is_ecdsa(self) -> bool:
        return self._type == _SigningPrivateKeyType.ECDSA

    def to_ed25519(self) -> Any | None:
        """Return the underlying Ed25519PrivateKey if this is an Ed25519 key."""
        if self._type == _SigningPrivateKeyType.ED25519:
            return self._key
        return None

    def to_ssh(self) -> Any | None:
        """Return the underlying SSH private key if this is an SSH key."""
        if self._type == _SigningPrivateKeyType.SSH:
            return self._key
        return None

    def is_ssh(self) -> bool:
        return self._type == _SigningPrivateKeyType.SSH

    def to_mldsa(self) -> Any | None:
        """Return the underlying MLDSAPrivateKey if this is an ML-DSA key."""
        if self._type == _SigningPrivateKeyType.MLDSA:
            return self._key
        return None

    # --- Public key derivation ---

    def public_key(self) -> SigningPublicKey:
        """Derive the corresponding public key for this private key."""
        if self._type == _SigningPrivateKeyType.SCHNORR:
            return SigningPublicKey.from_schnorr(self._key.schnorr_public_key())
        elif self._type == _SigningPrivateKeyType.ECDSA:
            return SigningPublicKey.from_ecdsa(self._key.public_key())
        elif self._type == _SigningPrivateKeyType.ED25519:
            return SigningPublicKey.from_ed25519(self._key.public_key())
        elif self._type == _SigningPrivateKeyType.SSH:
            # SSH key: extract public key from the cryptography private key
            ssh_priv = self._key
            from cryptography.hazmat.primitives import serialization

            pub_key = ssh_priv.public_key()
            return SigningPublicKey.from_ssh(pub_key)
        elif self._type == _SigningPrivateKeyType.MLDSA:
            raise ValueError("Deriving ML-DSA public key not supported")
        raise ValueError(f"Unknown key type: {self._type}")

    # --- Signing ---

    def _ecdsa_sign(self, message: bytes | bytearray) -> Signature:
        """Sign using ECDSA."""
        if self._type != _SigningPrivateKeyType.ECDSA:
            raise ValueError("Invalid key type for ECDSA signing")
        sig = self._key.ecdsa_sign(message)
        return Signature.ecdsa_from_data(sig)

    def schnorr_sign(
        self,
        message: bytes | bytearray,
        rng: RandomNumberGenerator,
    ) -> Signature:
        """Sign using Schnorr with a custom RNG."""
        if self._type != _SigningPrivateKeyType.SCHNORR:
            raise ValueError("Invalid key type for Schnorr signing")
        sig = self._key.schnorr_sign_using(message, rng)
        return Signature.schnorr_from_data(sig)

    def _ed25519_sign(self, message: bytes | bytearray) -> Signature:
        """Sign using Ed25519."""
        if self._type != _SigningPrivateKeyType.ED25519:
            raise ValueError("Invalid key type for Ed25519 signing")
        sig = self._key.sign(bytes(message))
        return Signature.ed25519_from_data(sig)

    def _ssh_sign(
        self,
        message: bytes | bytearray,
        namespace: str,
        hash_alg: str,
    ) -> Signature:
        """Sign using SSH."""
        if self._type != _SigningPrivateKeyType.SSH:
            raise ValueError("Invalid key type for SSH signing")

        from cryptography.hazmat.primitives import hashes, serialization
        from cryptography.hazmat.primitives.asymmetric import ec, ed25519, dsa, utils

        ssh_priv = self._key
        msg = bytes(message)

        # Build the sshsig envelope
        pem = _ssh_sign_message(ssh_priv, msg, namespace, hash_alg)
        return Signature.from_ssh(pem)

    def _mldsa_sign(self, message: bytes | bytearray) -> Signature:
        """Sign using ML-DSA."""
        if self._type != _SigningPrivateKeyType.MLDSA:
            raise ValueError("Invalid key type for MLDSA signing")
        sig = self._key.sign(bytes(message))
        return Signature.from_mldsa(sig)

    def sign_with_options(
        self,
        message: bytes | bytearray,
        options: SigningOptions | None = None,
    ) -> Signature:
        """Sign a message with optional algorithm-specific parameters."""
        if self._type == _SigningPrivateKeyType.SCHNORR:
            if isinstance(options, SchnorrSigningOptions):
                rng = options.rng
            else:
                rng = SecureRandomNumberGenerator()
            return self.schnorr_sign(message, rng)
        elif self._type == _SigningPrivateKeyType.ECDSA:
            return self._ecdsa_sign(message)
        elif self._type == _SigningPrivateKeyType.ED25519:
            return self._ed25519_sign(message)
        elif self._type == _SigningPrivateKeyType.SSH:
            if isinstance(options, SshSigningOptions):
                return self._ssh_sign(message, options.namespace, options.hash_alg)
            raise ValueError(
                "Missing namespace and hash algorithm for SSH signing"
            )
        elif self._type == _SigningPrivateKeyType.MLDSA:
            return self._mldsa_sign(message)
        raise ValueError(f"Unknown key type: {self._type}")

    def sign(self, message: bytes | bytearray) -> Signature:
        """Sign a message using default options."""
        return self.sign_with_options(message)

    # --- Verifier ---

    def verify(
        self,
        signature: Signature,
        message: bytes | bytearray,
    ) -> bool:
        """Verify a signature (only supported for Schnorr keys)."""
        if self._type == _SigningPrivateKeyType.SCHNORR:
            schnorr_sig = signature.to_schnorr()
            if schnorr_sig is None:
                return False
            return self._key.schnorr_public_key().schnorr_verify(
                schnorr_sig, message
            )
        return False

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SIGNING_PRIVATE_KEY])

    def untagged_cbor(self) -> CBOR:
        """Encode to untagged CBOR.

        - Schnorr: byte_string(key_data)
        - ECDSA: [1, byte_string(key_data)]
        - Ed25519: [2, byte_string(key_data)]
        - SSH: tagged text string (OpenSSH format)
        - MLDSA: delegates to MLDSAPrivateKey
        """
        if self._type == _SigningPrivateKeyType.SCHNORR:
            return CBOR.from_bytes(self._key.data)
        elif self._type == _SigningPrivateKeyType.ECDSA:
            return CBOR.from_array([
                CBOR.from_int(1),
                CBOR.from_bytes(self._key.data),
            ])
        elif self._type == _SigningPrivateKeyType.ED25519:
            return CBOR.from_array([
                CBOR.from_int(2),
                CBOR.from_bytes(self._key.data),
            ])
        elif self._type == _SigningPrivateKeyType.SSH:
            from cryptography.hazmat.primitives import serialization

            ssh_priv = self._key
            pem_bytes = ssh_priv.private_bytes(
                encoding=serialization.Encoding.PEM,
                format=serialization.PrivateFormat.OpenSSH,
                encryption_algorithm=serialization.NoEncryption(),
            )
            pem_str = pem_bytes.decode("utf-8")
            return CBOR.from_tagged_value(
                TAG_SSH_TEXT_PRIVATE_KEY,
                CBOR.from_text(pem_str),
            )
        elif self._type == _SigningPrivateKeyType.MLDSA:
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
    def from_untagged_cbor(cbor: CBOR) -> SigningPrivateKey:
        """Decode a SigningPrivateKey from untagged CBOR."""
        case = cbor.case

        # Byte string -> Schnorr
        if case == CBORCase.BYTE_STRING:
            data = cbor.try_byte_string()
            return SigningPrivateKey.new_schnorr(ECPrivateKey.from_data(data))

        # Array -> discriminated type
        if case == CBORCase.ARRAY:
            elements = cbor.try_array()
            discriminator = elements[0].try_int()
            data = elements[1].try_byte_string()
            if discriminator == 1:
                return SigningPrivateKey.new_ecdsa(
                    ECPrivateKey.from_data(data)
                )
            elif discriminator == 2:
                from ..ed25519._ed25519_private_key import Ed25519PrivateKey

                return SigningPrivateKey.new_ed25519(
                    Ed25519PrivateKey.from_data(data)
                )
            raise ValueError(
                f"Invalid discriminator for SigningPrivateKey: {discriminator}"
            )

        # Tagged value -> SSH or MLDSA
        if case == CBORCase.TAGGED:
            tag, item = cbor.try_tagged_value()
            tag_val = tag.value
            if tag_val == TAG_SSH_TEXT_PRIVATE_KEY:
                pem_str = item.try_text()
                from cryptography.hazmat.primitives.serialization import (
                    load_ssh_private_key,
                )

                key = load_ssh_private_key(pem_str.encode("utf-8"), password=None)
                return SigningPrivateKey.new_ssh(key)
            elif tag_val == TAG_MLDSA_PRIVATE_KEY:
                from ..mldsa._mldsa_private_key import MLDSAPrivateKey

                key = MLDSAPrivateKey.from_untagged_cbor(item)
                return SigningPrivateKey.new_mldsa(key)
            raise ValueError(
                f"Invalid CBOR tag for SigningPrivateKey: {tag_val}"
            )

        raise ValueError("Invalid CBOR case for SigningPrivateKey")

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> SigningPrivateKey:
        tags = SigningPrivateKey.cbor_tags()
        inner = cbor
        for tag in tags:
            inner = inner.try_expected_tagged_value(tag.value)
        return SigningPrivateKey.from_untagged_cbor(inner)

    @staticmethod
    def from_tagged_cbor_data(data: bytes | bytearray) -> SigningPrivateKey:
        return SigningPrivateKey.from_tagged_cbor(CBOR.from_data(bytes(data)))

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
        if not isinstance(other, SigningPrivateKey):
            return NotImplemented
        if self._type != other._type:
            return False
        if self._type in (
            _SigningPrivateKeyType.SCHNORR,
            _SigningPrivateKeyType.ECDSA,
        ):
            return self._key == other._key
        if self._type == _SigningPrivateKeyType.ED25519:
            return self._key == other._key
        if self._type == _SigningPrivateKeyType.SSH:
            # Compare by serialized form
            from cryptography.hazmat.primitives import serialization

            self_bytes = self._key.private_bytes(
                encoding=serialization.Encoding.PEM,
                format=serialization.PrivateFormat.OpenSSH,
                encryption_algorithm=serialization.NoEncryption(),
            )
            other_bytes = other._key.private_bytes(
                encoding=serialization.Encoding.PEM,
                format=serialization.PrivateFormat.OpenSSH,
                encryption_algorithm=serialization.NoEncryption(),
            )
            return self_bytes == other_bytes
        if self._type == _SigningPrivateKeyType.MLDSA:
            return self._key.data == other._key.data
        return False

    def __hash__(self) -> int:
        if self._type in (
            _SigningPrivateKeyType.SCHNORR,
            _SigningPrivateKeyType.ECDSA,
        ):
            return hash((self._type, self._key.data))
        if self._type == _SigningPrivateKeyType.ED25519:
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
    def from_ur(ur) -> SigningPrivateKey:
        """Decode from a UR."""
        from bc_ur import from_ur
        return from_ur(SigningPrivateKey, ur)

    @staticmethod
    def from_ur_string(ur_string: str) -> SigningPrivateKey:
        """Decode from a UR string."""
        from bc_ur import from_ur_string
        return from_ur_string(SigningPrivateKey, ur_string)

    def __repr__(self) -> str:
        return "SigningPrivateKey"

    def __str__(self) -> str:
        if self._type == _SigningPrivateKeyType.SCHNORR:
            return (
                f"SigningPrivateKey({self.ref_hex_short()}, "
                f"SchnorrPrivateKey({self._key.ref_hex_short()}))"
            )
        elif self._type == _SigningPrivateKeyType.ECDSA:
            return (
                f"SigningPrivateKey({self.ref_hex_short()}, "
                f"ECDSAPrivateKey({self._key.ref_hex_short()}))"
            )
        elif self._type == _SigningPrivateKeyType.ED25519:
            return (
                f"SigningPrivateKey({self.ref_hex_short()}, "
                f"{self._key})"
            )
        elif self._type == _SigningPrivateKeyType.SSH:
            return f"SigningPrivateKey({self.ref_hex_short()}, SSH)"
        elif self._type == _SigningPrivateKeyType.MLDSA:
            return (
                f"SigningPrivateKey({self.ref_hex_short()}, "
                f"{self._key})"
            )
        return f"SigningPrivateKey({self._type})"


def _ssh_sign_message(
    private_key: Any,
    message: bytes,
    namespace: str,
    hash_alg: str,
) -> str:
    """Create an SSH signature in the sshsig format and return PEM string.

    Implements the SSHSIG protocol as described in ssh-keygen(1).
    The signature envelope format:
        MAGIC_PREAMBLE || uint32 version || string namespace || string reserved
        || string hash_algorithm || string H(message)

    The resulting signature is PEM-encoded.
    """
    import hashlib
    import struct

    from cryptography.hazmat.primitives import hashes, serialization
    from cryptography.hazmat.primitives.asymmetric import (
        dsa,
        ec,
        ed25519,
        padding,
        utils,
    )

    MAGIC_PREAMBLE = b"SSHSIG"
    SIG_VERSION = 1

    # Determine hash function
    hash_funcs = {
        "sha256": (hashlib.sha256, "sha256"),
        "sha512": (hashlib.sha512, "sha512"),
    }
    if hash_alg not in hash_funcs:
        raise ValueError(f"Unsupported hash algorithm: {hash_alg}")

    hash_func, hash_name = hash_funcs[hash_alg]
    msg_hash = hash_func(message).digest()

    # Build the data to sign
    def encode_string(data: bytes) -> bytes:
        return struct.pack(">I", len(data)) + data

    signed_data = (
        MAGIC_PREAMBLE
        + struct.pack(">I", SIG_VERSION)
        + encode_string(namespace.encode("utf-8"))
        + encode_string(b"")  # reserved
        + encode_string(hash_name.encode("utf-8"))
        + encode_string(msg_hash)
    )

    # Sign with the appropriate algorithm
    if isinstance(private_key, ed25519.Ed25519PrivateKey):
        raw_sig = private_key.sign(signed_data)
        algo_name = b"ssh-ed25519"
        pub_key = private_key.public_key()
        pub_bytes = pub_key.public_bytes(
            serialization.Encoding.Raw, serialization.PublicFormat.Raw
        )
        sig_blob = encode_string(algo_name) + encode_string(raw_sig)
        public_key_blob = encode_string(algo_name) + encode_string(pub_bytes)
    elif isinstance(private_key, ec.EllipticCurvePrivateKey):
        curve = private_key.curve
        if isinstance(curve, ec.SECP256R1):
            curve_name = b"nistp256"
            algo_name = b"ecdsa-sha2-nistp256"
            hash_obj = hashes.SHA256()
        elif isinstance(curve, ec.SECP384R1):
            curve_name = b"nistp384"
            algo_name = b"ecdsa-sha2-nistp384"
            hash_obj = hashes.SHA384()
        else:
            raise ValueError(f"Unsupported EC curve: {curve.name}")

        der_sig = private_key.sign(signed_data, ec.ECDSA(hash_obj))
        sig_blob = encode_string(algo_name) + encode_string(der_sig)

        pub_key = private_key.public_key()
        pub_bytes = pub_key.public_bytes(
            serialization.Encoding.X962,
            serialization.PublicFormat.UncompressedPoint,
        )
        public_key_blob = (
            encode_string(algo_name)
            + encode_string(curve_name)
            + encode_string(pub_bytes)
        )
    elif isinstance(private_key, dsa.DSAPrivateKey):
        algo_name = b"ssh-dss"
        hash_obj = hashes.SHA1()
        der_sig = private_key.sign(signed_data, hash_obj)
        r, s = utils.decode_dss_signature(der_sig)
        raw_sig = r.to_bytes(20, "big") + s.to_bytes(20, "big")
        sig_blob = encode_string(algo_name) + encode_string(raw_sig)

        pub_key = private_key.public_key()
        params = pub_key.parameters().parameter_numbers()
        y = pub_key.public_numbers().y
        public_key_blob = (
            encode_string(algo_name)
            + encode_string(params.p.to_bytes((params.p.bit_length() + 7) // 8, "big"))
            + encode_string(params.q.to_bytes((params.q.bit_length() + 7) // 8, "big"))
            + encode_string(params.g.to_bytes((params.g.bit_length() + 7) // 8, "big"))
            + encode_string(y.to_bytes((y.bit_length() + 7) // 8, "big"))
        )
    else:
        raise ValueError(f"Unsupported SSH private key type: {type(private_key)}")

    # Build the sshsig envelope
    sig_envelope = (
        MAGIC_PREAMBLE
        + struct.pack(">I", SIG_VERSION)
        + encode_string(public_key_blob)
        + encode_string(namespace.encode("utf-8"))
        + encode_string(b"")  # reserved
        + encode_string(hash_name.encode("utf-8"))
        + encode_string(sig_blob)
    )

    # PEM-encode
    import base64

    b64 = base64.b64encode(sig_envelope).decode("ascii")
    lines = [b64[i : i + 70] for i in range(0, len(b64), 70)]
    pem = "-----BEGIN SSH SIGNATURE-----\n"
    pem += "\n".join(lines)
    pem += "\n-----END SSH SIGNATURE-----"
    return pem
