import type { Cbor } from '@bc/dcbor';

import type { EncryptedMessage } from '../symmetric/encrypted-message.js';
import type { SymmetricKey } from '../symmetric/symmetric-key.js';

export interface KeyDerivation {
    index(): number;
    lock(contentKey: SymmetricKey, secret: Uint8Array): EncryptedMessage;
    unlock(encryptedMessage: EncryptedMessage, secret: Uint8Array): SymmetricKey;
    toCbor(): Cbor;
}
