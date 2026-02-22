import BCTags
import DCBOR
import Foundation

private func withUntaggedCBOR<T>(
    _ payload: Any,
    _ body: (CBOR) throws -> T
) throws -> T {
    guard let untaggedCBOR = payload as? CBOR else {
        throw BCComponentsError.invalidData(
            dataType: "tag summary payload",
            reason: "expected CBOR payload"
        )
    }
    return try body(untaggedCBOR)
}

@MainActor
public func registerTagsIn(_ tagsStore: TagsStore) {
    BCTags.registerTagsIn(tagsStore)

    tagsStore.setSummarizer(.digest) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let digest = try Digest(untaggedCBOR: cbor)
            return "Digest(\(digest.shortDescription()))"
        }
    }

    tagsStore.setSummarizer(.arid) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let arid = try ARID(untaggedCBOR: cbor)
            return "ARID(\(arid.shortDescription()))"
        }
    }

    tagsStore.setSummarizer(.xid) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let xid = try XID(untaggedCBOR: cbor)
            return "XID(\(xid.shortDescription()))"
        }
    }

    tagsStore.setSummarizer(.uri) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let uri = try URI(untaggedCBOR: cbor)
            return "URI(\(uri))"
        }
    }

    tagsStore.setSummarizer(.uuid) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let uuid = try UUID(untaggedCBOR: cbor)
            return "UUID(\(uuid))"
        }
    }

    tagsStore.setSummarizer(.nonce) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            _ = try Nonce(untaggedCBOR: cbor)
            return "Nonce"
        }
    }

    tagsStore.setSummarizer(.salt) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            _ = try Salt(untaggedCBOR: cbor)
            return "Salt"
        }
    }

    tagsStore.setSummarizer(.json) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let json = try JSON(untaggedCBOR: cbor)
            return "JSON(\(json.asString()))"
        }
    }

    tagsStore.setSummarizer(.seed) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            _ = try Seed(untaggedCBOR: cbor)
            return "Seed"
        }
    }

    tagsStore.setSummarizer(.privateKeys) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let privateKeys = try PrivateKeys(untaggedCBOR: cbor)
            return privateKeys.description
        }
    }

    tagsStore.setSummarizer(.publicKeys) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let publicKeys = try PublicKeys(untaggedCBOR: cbor)
            return publicKeys.description
        }
    }

    tagsStore.setSummarizer(.reference) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let reference = try Reference(untaggedCBOR: cbor)
            return reference.description
        }
    }

    tagsStore.setSummarizer(.encryptedKey) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let encryptedKey = try EncryptedKey(untaggedCBOR: cbor)
            return encryptedKey.description
        }
    }

    tagsStore.setSummarizer(.privateKeyBase) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let privateKeyBase = try PrivateKeyBase(untaggedCBOR: cbor)
            return privateKeyBase.description
        }
    }

    tagsStore.setSummarizer(.signingPrivateKey) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let signingPrivateKey = try SigningPrivateKey(untaggedCBOR: cbor)
            return signingPrivateKey.description
        }
    }

    tagsStore.setSummarizer(.signingPublicKey) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let signingPublicKey = try SigningPublicKey(untaggedCBOR: cbor)
            return signingPublicKey.description
        }
    }

    tagsStore.setSummarizer(.signature) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let signature = try Signature(untaggedCBOR: cbor)
            let scheme = signature.scheme()
            if scheme == .default {
                return "Signature"
            }
            return "Signature(\(String(describing: scheme)))"
        }
    }

    tagsStore.setSummarizer(.sealedMessage) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let sealedMessage = try SealedMessage(untaggedCBOR: cbor)
            let scheme = sealedMessage.encapsulationScheme()
            if scheme == .default {
                return "SealedMessage"
            }
            return "SealedMessage(\(String(describing: scheme)))"
        }
    }

    tagsStore.setSummarizer(.sskrShare) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            _ = try SSKRShare(untaggedCBOR: cbor)
            return "SSKRShare"
        }
    }

    tagsStore.setSummarizer(.sshTextPrivateKey) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let key = try SSHPrivateKey(openssh: try textString(cbor))
            return "SSHPrivateKey(\(key.refHexShort()))"
        }
    }

    tagsStore.setSummarizer(.sshTextPublicKey) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            let key = try SSHPublicKey(openssh: try textString(cbor))
            return "SSHPublicKey(\(key.refHexShort()))"
        }
    }

    tagsStore.setSummarizer(.sshTextSignature) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            _ = try textString(cbor)
            return "SSHSignature"
        }
    }

    tagsStore.setSummarizer(.sshTextCertificate) { payload, _ in
        try withUntaggedCBOR(payload) { cbor in
            _ = try textString(cbor)
            return "SSHCertificate"
        }
    }
}

@MainActor
public func registerTags() {
    registerTagsIn(globalTags)
}
