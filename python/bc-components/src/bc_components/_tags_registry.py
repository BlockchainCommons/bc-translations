"""CBOR tag registration for bc-components.

Extends the bc-tags registration with bc-components-specific summarizers.
"""

from __future__ import annotations

from bc_tags import (
    TAG_ARID,
    TAG_DIGEST,
    TAG_ENCRYPTED_KEY,
    TAG_JSON,
    TAG_NONCE,
    TAG_PRIVATE_KEY_BASE,
    TAG_PRIVATE_KEYS,
    TAG_PUBLIC_KEYS,
    TAG_REFERENCE,
    TAG_SALT,
    TAG_SEALED_MESSAGE,
    TAG_SEED,
    TAG_SIGNATURE,
    TAG_SIGNING_PRIVATE_KEY,
    TAG_SIGNING_PUBLIC_KEY,
    TAG_SSKR_SHARE,
    TAG_URI,
    TAG_UUID,
    TAG_XID,
    register_tags_in as _bc_tags_register_tags_in,
)
from dcbor import CBOR, TagsStore, with_tags, with_tags_mut


def register_tags_in(tags_store: TagsStore) -> None:
    """Register all bc-tags and bc-components summarizers into *tags_store*."""
    _bc_tags_register_tags_in(tags_store)

    # --- Digest ---
    def _digest_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._digest import Digest

        d = Digest.from_untagged_cbor(untagged_cbor)
        return f"Digest({d.short_description()})"

    tags_store.set_summarizer(TAG_DIGEST, _digest_summarizer)

    # --- ARID ---
    def _arid_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .id._arid import ARID

        arid = ARID.from_untagged_cbor(untagged_cbor)
        return f"ARID({arid.short_description()})"

    tags_store.set_summarizer(TAG_ARID, _arid_summarizer)

    # --- XID ---
    def _xid_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .id._xid import XID

        xid = XID.from_untagged_cbor(untagged_cbor)
        return f"XID({xid.short_description()})"

    tags_store.set_summarizer(TAG_XID, _xid_summarizer)

    # --- URI ---
    def _uri_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .id._uri import URI

        uri = URI.from_untagged_cbor(untagged_cbor)
        return f"URI({uri})"

    tags_store.set_summarizer(TAG_URI, _uri_summarizer)

    # --- UUID ---
    def _uuid_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .id._uuid import UUID

        uuid = UUID.from_untagged_cbor(untagged_cbor)
        return f"UUID({uuid})"

    tags_store.set_summarizer(TAG_UUID, _uuid_summarizer)

    # --- Nonce ---
    def _nonce_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._nonce import Nonce

        Nonce.from_untagged_cbor(untagged_cbor)
        return "Nonce"

    tags_store.set_summarizer(TAG_NONCE, _nonce_summarizer)

    # --- Salt ---
    def _salt_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._salt import Salt

        Salt.from_untagged_cbor(untagged_cbor)
        return "Salt"

    tags_store.set_summarizer(TAG_SALT, _salt_summarizer)

    # --- JSON ---
    def _json_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._json import JSON

        j = JSON.from_untagged_cbor(untagged_cbor)
        return f"JSON({j.as_str()})"

    tags_store.set_summarizer(TAG_JSON, _json_summarizer)

    # --- Seed ---
    def _seed_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._seed import Seed

        Seed.from_untagged_cbor(untagged_cbor)
        return "Seed"

    tags_store.set_summarizer(TAG_SEED, _seed_summarizer)

    # --- PrivateKeys ---
    def _private_keys_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._private_keys import PrivateKeys

        pk = PrivateKeys.from_untagged_cbor(untagged_cbor)
        return str(pk)

    tags_store.set_summarizer(TAG_PRIVATE_KEYS, _private_keys_summarizer)

    # --- PublicKeys ---
    def _public_keys_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._public_keys import PublicKeys

        pk = PublicKeys.from_untagged_cbor(untagged_cbor)
        return str(pk)

    tags_store.set_summarizer(TAG_PUBLIC_KEYS, _public_keys_summarizer)

    # --- Reference ---
    def _reference_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._reference import Reference

        ref = Reference.from_untagged_cbor(untagged_cbor)
        return str(ref)

    tags_store.set_summarizer(TAG_REFERENCE, _reference_summarizer)

    # --- EncryptedKey ---
    def _encrypted_key_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .encrypted_key._encrypted_key import EncryptedKey

        ek = EncryptedKey.from_untagged_cbor(untagged_cbor)
        return str(ek)

    tags_store.set_summarizer(TAG_ENCRYPTED_KEY, _encrypted_key_summarizer)

    # --- PrivateKeyBase ---
    def _private_key_base_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._private_key_base import PrivateKeyBase

        pkb = PrivateKeyBase.from_untagged_cbor(untagged_cbor)
        return str(pkb)

    tags_store.set_summarizer(
        TAG_PRIVATE_KEY_BASE, _private_key_base_summarizer
    )

    # --- SigningPrivateKey ---
    def _signing_private_key_summarizer(
        untagged_cbor: CBOR, _flat: bool
    ) -> str:
        from .signing._signing_private_key import SigningPrivateKey

        spk = SigningPrivateKey.from_untagged_cbor(untagged_cbor)
        return str(spk)

    tags_store.set_summarizer(
        TAG_SIGNING_PRIVATE_KEY, _signing_private_key_summarizer
    )

    # --- SigningPublicKey ---
    def _signing_public_key_summarizer(
        untagged_cbor: CBOR, _flat: bool
    ) -> str:
        from .signing._signing_public_key import SigningPublicKey

        spk = SigningPublicKey.from_untagged_cbor(untagged_cbor)
        return str(spk)

    tags_store.set_summarizer(
        TAG_SIGNING_PUBLIC_KEY, _signing_public_key_summarizer
    )

    # --- Signature ---
    def _signature_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .signing._signature import Signature
        from .signing._signature_scheme import SignatureScheme

        sig = Signature.from_untagged_cbor(untagged_cbor)
        try:
            scheme = sig.scheme()
            if scheme == SignatureScheme.SCHNORR:
                return "Signature"
            return f"Signature({scheme.name})"
        except Exception:
            return "Signature(Unknown)"

    tags_store.set_summarizer(TAG_SIGNATURE, _signature_summarizer)

    # --- SealedMessage ---
    def _sealed_message_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from .encapsulation._encapsulation_scheme import EncapsulationScheme
        from .encapsulation._sealed_message import SealedMessage

        sm = SealedMessage.from_untagged_cbor(untagged_cbor)
        enc_scheme = sm.encapsulation_scheme()
        if enc_scheme == EncapsulationScheme.default():
            return "SealedMessage"
        return f"SealedMessage({enc_scheme.name})"

    tags_store.set_summarizer(TAG_SEALED_MESSAGE, _sealed_message_summarizer)

    # --- SSKRShare ---
    def _sskr_share_summarizer(untagged_cbor: CBOR, _flat: bool) -> str:
        from ._sskr_mod import SSKRShare

        SSKRShare.from_untagged_cbor(untagged_cbor)
        return "SSKRShare"

    tags_store.set_summarizer(TAG_SSKR_SHARE, _sskr_share_summarizer)


def register_tags() -> None:
    """Register all tags in the global store."""
    # Force lazy initialization before acquiring the mutable lock.
    with_tags(lambda _: None)
    with_tags_mut(lambda store: register_tags_in(store))
