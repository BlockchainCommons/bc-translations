package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_ARID
import com.blockchaincommons.bctags.TAG_DIGEST
import com.blockchaincommons.bctags.TAG_EC_KEY
import com.blockchaincommons.bctags.TAG_ENCRYPTED_KEY
import com.blockchaincommons.bctags.TAG_JSON
import com.blockchaincommons.bctags.TAG_MLDSA_PRIVATE_KEY
import com.blockchaincommons.bctags.TAG_MLDSA_PUBLIC_KEY
import com.blockchaincommons.bctags.TAG_MLDSA_SIGNATURE
import com.blockchaincommons.bctags.TAG_MLKEM_CIPHERTEXT
import com.blockchaincommons.bctags.TAG_MLKEM_PRIVATE_KEY
import com.blockchaincommons.bctags.TAG_MLKEM_PUBLIC_KEY
import com.blockchaincommons.bctags.TAG_NONCE
import com.blockchaincommons.bctags.TAG_PRIVATE_KEYS
import com.blockchaincommons.bctags.TAG_PRIVATE_KEY_BASE
import com.blockchaincommons.bctags.TAG_PUBLIC_KEYS
import com.blockchaincommons.bctags.TAG_REFERENCE
import com.blockchaincommons.bctags.TAG_SALT
import com.blockchaincommons.bctags.TAG_SEALED_MESSAGE
import com.blockchaincommons.bctags.TAG_SEED
import com.blockchaincommons.bctags.TAG_SIGNATURE
import com.blockchaincommons.bctags.TAG_SIGNING_PRIVATE_KEY
import com.blockchaincommons.bctags.TAG_SIGNING_PUBLIC_KEY
import com.blockchaincommons.bctags.TAG_SSKR_SHARE
import com.blockchaincommons.bctags.TAG_SYMMETRIC_KEY
import com.blockchaincommons.bctags.TAG_URI
import com.blockchaincommons.bctags.TAG_UUID
import com.blockchaincommons.bctags.TAG_X25519_PRIVATE_KEY
import com.blockchaincommons.bctags.TAG_X25519_PUBLIC_KEY
import com.blockchaincommons.bctags.TAG_XID
import com.blockchaincommons.dcbor.GlobalTags
import com.blockchaincommons.dcbor.TagsStore

/**
 * Registers all bc-tags and bc-components summarizers in the given [tagsStore].
 *
 * This first delegates to [com.blockchaincommons.bctags.registerTagsIn] for
 * BC tag constants, then installs component-specific summarizers for
 * diagnostic output.
 */
fun registerTagsIn(tagsStore: TagsStore) {
    com.blockchaincommons.bctags.registerTagsIn(tagsStore)

    tagsStore.setSummarizer(TAG_DIGEST) { untaggedCbor, _ ->
        val digest = Digest.fromUntaggedCbor(untaggedCbor)
        "Digest(${digest.shortDescription()})"
    }

    tagsStore.setSummarizer(TAG_ARID) { untaggedCbor, _ ->
        val arid = ARID.fromUntaggedCbor(untaggedCbor)
        "ARID(${arid.shortDescription()})"
    }

    tagsStore.setSummarizer(TAG_XID) { untaggedCbor, _ ->
        val xid = XID.fromUntaggedCbor(untaggedCbor)
        "XID(${xid.shortDescription()})"
    }

    tagsStore.setSummarizer(TAG_URI) { untaggedCbor, _ ->
        val uri = URI.fromUntaggedCbor(untaggedCbor)
        "URI($uri)"
    }

    tagsStore.setSummarizer(TAG_UUID) { untaggedCbor, _ ->
        val uuid = UUID.fromUntaggedCbor(untaggedCbor)
        "UUID($uuid)"
    }

    tagsStore.setSummarizer(TAG_NONCE) { untaggedCbor, _ ->
        Nonce.fromUntaggedCbor(untaggedCbor)
        "Nonce"
    }

    tagsStore.setSummarizer(TAG_SALT) { untaggedCbor, _ ->
        Salt.fromUntaggedCbor(untaggedCbor)
        "Salt"
    }

    tagsStore.setSummarizer(TAG_JSON) { untaggedCbor, _ ->
        val json = CborJson.fromUntaggedCbor(untaggedCbor)
        "JSON(${json.asString()})"
    }

    tagsStore.setSummarizer(TAG_SEED) { untaggedCbor, _ ->
        Seed.fromUntaggedCbor(untaggedCbor)
        "Seed"
    }

    tagsStore.setSummarizer(TAG_SYMMETRIC_KEY) { untaggedCbor, _ ->
        SymmetricKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_X25519_PRIVATE_KEY) { untaggedCbor, _ ->
        X25519PrivateKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_X25519_PUBLIC_KEY) { untaggedCbor, _ ->
        X25519PublicKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_EC_KEY) { untaggedCbor, _ ->
        // EC key maps use key 2 to distinguish private from public.
        val map = untaggedCbor.tryMap()
        val isPrivate: Boolean? = map.get<Int, Boolean>(2)
        if (isPrivate != null) {
            ECPrivateKey.fromUntaggedCbor(untaggedCbor).toString()
        } else {
            ECPublicKey.fromUntaggedCbor(untaggedCbor).toString()
        }
    }

    tagsStore.setSummarizer(TAG_PRIVATE_KEYS) { untaggedCbor, _ ->
        PrivateKeys.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_PUBLIC_KEYS) { untaggedCbor, _ ->
        PublicKeys.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_REFERENCE) { untaggedCbor, _ ->
        Reference.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_ENCRYPTED_KEY) { untaggedCbor, _ ->
        EncryptedKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_PRIVATE_KEY_BASE) { untaggedCbor, _ ->
        PrivateKeyBase.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_SIGNING_PRIVATE_KEY) { untaggedCbor, _ ->
        SigningPrivateKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_SIGNING_PUBLIC_KEY) { untaggedCbor, _ ->
        SigningPublicKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_SIGNATURE) { untaggedCbor, _ ->
        val signature = Signature.fromUntaggedCbor(untaggedCbor)
        val scheme = signature.scheme
        if (scheme == SignatureScheme.Schnorr) {
            "Signature"
        } else {
            "Signature(${scheme.name})"
        }
    }

    tagsStore.setSummarizer(TAG_SEALED_MESSAGE) { untaggedCbor, _ ->
        val sealedMessage = SealedMessage.fromUntaggedCbor(untaggedCbor)
        val encapsulationScheme = sealedMessage.encapsulationScheme()
        if (encapsulationScheme == EncapsulationScheme.X25519) {
            "SealedMessage"
        } else {
            "SealedMessage(${encapsulationScheme.name})"
        }
    }

    tagsStore.setSummarizer(TAG_SSKR_SHARE) { untaggedCbor, _ ->
        SSKRShare.fromUntaggedCbor(untaggedCbor)
        "SSKRShare"
    }

    tagsStore.setSummarizer(TAG_MLDSA_PRIVATE_KEY) { untaggedCbor, _ ->
        MLDSAPrivateKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_MLDSA_PUBLIC_KEY) { untaggedCbor, _ ->
        MLDSAPublicKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_MLDSA_SIGNATURE) { untaggedCbor, _ ->
        MLDSASignature.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_MLKEM_PRIVATE_KEY) { untaggedCbor, _ ->
        MLKEMPrivateKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_MLKEM_PUBLIC_KEY) { untaggedCbor, _ ->
        MLKEMPublicKey.fromUntaggedCbor(untaggedCbor).toString()
    }

    tagsStore.setSummarizer(TAG_MLKEM_CIPHERTEXT) { untaggedCbor, _ ->
        MLKEMCiphertext.fromUntaggedCbor(untaggedCbor).toString()
    }
}

/**
 * Registers all Blockchain Commons tags and component summarizers in dCBOR's
 * global tag store.
 *
 * Call this once at application startup to enable tag name resolution and
 * summarization in diagnostic output formatting.
 */
fun registerTags() {
    GlobalTags.withTagsMut { registerTagsIn(it) }
}
