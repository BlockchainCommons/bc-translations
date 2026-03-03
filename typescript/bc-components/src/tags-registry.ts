import {
    Err,
    Ok,
    type Cbor,
    type SummarizerResult,
    type TagsStore,
    errorMsg,
    withTagsMut,
} from '@bc/dcbor';
import {
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
    registerTagsIn as bcTagsRegisterTagsIn,
} from '@bc/tags';

import { ARID } from './id/arid.js';
import { Digest } from './digest.js';
import { EncryptedKey } from './encrypted-key/encrypted-key.js';
import { JSON } from './json.js';
import { Nonce } from './nonce.js';
import { PrivateKeyBase } from './private-key-base.js';
import { PrivateKeys } from './private-keys.js';
import { PublicKeys } from './public-keys.js';
import { Reference } from './reference.js';
import { Salt } from './salt.js';
import { SealedMessage } from './encapsulation/sealed-message.js';
import { Seed } from './seed.js';
import { Signature } from './signing/signature.js';
import { SigningPrivateKey } from './signing/signing-private-key.js';
import { SigningPublicKey } from './signing/signing-public-key.js';
import { SSKRShare } from './sskr-mod.js';
import { URI } from './id/uri.js';
import { UUID } from './id/uuid.js';
import { XID } from './id/xid.js';
import { EncapsulationScheme } from './encapsulation/encapsulation-scheme.js';
import { SignatureScheme } from './signing/signature-scheme.js';

function summarize(fn: () => string): SummarizerResult {
    try {
        return Ok(fn());
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        return Err(errorMsg(message));
    }
}

export function registerTagsIn(tagsStore: TagsStore): void {
    bcTagsRegisterTagsIn(tagsStore);

    tagsStore.setSummarizer(TAG_DIGEST, (untaggedCbor: Cbor) => summarize(() => {
        const digest = Digest.fromUntaggedCbor(untaggedCbor);
        return `Digest(${digest.shortDescription})`;
    }));

    tagsStore.setSummarizer(TAG_ARID, (untaggedCbor: Cbor) => summarize(() => {
        const arid = ARID.fromUntaggedCbor(untaggedCbor);
        return `ARID(${arid.shortDescription()})`;
    }));

    tagsStore.setSummarizer(TAG_XID, (untaggedCbor: Cbor) => summarize(() => {
        const xid = XID.fromUntaggedCbor(untaggedCbor);
        return `XID(${xid.shortDescription})`;
    }));

    tagsStore.setSummarizer(TAG_URI, (untaggedCbor: Cbor) => summarize(() => {
        return `URI(${URI.fromUntaggedCbor(untaggedCbor).toString()})`;
    }));

    tagsStore.setSummarizer(TAG_UUID, (untaggedCbor: Cbor) => summarize(() => {
        return `UUID(${UUID.fromUntaggedCbor(untaggedCbor).toString()})`;
    }));

    tagsStore.setSummarizer(TAG_NONCE, (untaggedCbor: Cbor) => summarize(() => {
        Nonce.fromUntaggedCbor(untaggedCbor);
        return 'Nonce';
    }));

    tagsStore.setSummarizer(TAG_SALT, (untaggedCbor: Cbor) => summarize(() => {
        Salt.fromUntaggedCbor(untaggedCbor);
        return 'Salt';
    }));

    tagsStore.setSummarizer(TAG_JSON, (untaggedCbor: Cbor) => summarize(() => {
        return `JSON(${JSON.fromUntaggedCbor(untaggedCbor).stringValue})`;
    }));

    tagsStore.setSummarizer(TAG_SEED, (untaggedCbor: Cbor) => summarize(() => {
        Seed.fromUntaggedCbor(untaggedCbor);
        return 'Seed';
    }));

    tagsStore.setSummarizer(TAG_PRIVATE_KEYS, (untaggedCbor: Cbor) => summarize(() => {
        return PrivateKeys.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_PUBLIC_KEYS, (untaggedCbor: Cbor) => summarize(() => {
        return PublicKeys.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_REFERENCE, (untaggedCbor: Cbor) => summarize(() => {
        return Reference.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_ENCRYPTED_KEY, (untaggedCbor: Cbor) => summarize(() => {
        return EncryptedKey.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_PRIVATE_KEY_BASE, (untaggedCbor: Cbor) => summarize(() => {
        return PrivateKeyBase.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_SIGNING_PRIVATE_KEY, (untaggedCbor: Cbor) => summarize(() => {
        return SigningPrivateKey.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_SIGNING_PUBLIC_KEY, (untaggedCbor: Cbor) => summarize(() => {
        return SigningPublicKey.fromUntaggedCbor(untaggedCbor).toString();
    }));

    tagsStore.setSummarizer(TAG_SIGNATURE, (untaggedCbor: Cbor) => summarize(() => {
        const signature = Signature.fromUntaggedCbor(untaggedCbor);
        const scheme = signature.scheme();
        if (scheme.equals(SignatureScheme.default())) {
            return 'Signature';
        }
        return `Signature(${scheme.toString()})`;
    }));

    tagsStore.setSummarizer(TAG_SEALED_MESSAGE, (untaggedCbor: Cbor) => summarize(() => {
        const sealedMessage = SealedMessage.fromUntaggedCbor(untaggedCbor);
        const encapsulationScheme = sealedMessage.encapsulationScheme();
        if (encapsulationScheme.equals(EncapsulationScheme.default())) {
            return 'SealedMessage';
        }
        return `SealedMessage(${encapsulationScheme.toString()})`;
    }));

    tagsStore.setSummarizer(TAG_SSKR_SHARE, (untaggedCbor: Cbor) => summarize(() => {
        SSKRShare.fromUntaggedCbor(untaggedCbor);
        return 'SSKRShare';
    }));
}

export function registerTags(): void {
    withTagsMut((tagsStore: TagsStore) => {
        registerTagsIn(tagsStore);
    });
}
