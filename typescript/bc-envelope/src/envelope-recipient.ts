import {
    type Decrypter,
    type Encrypter,
    Nonce,
    SealedMessage,
    SymmetricKey,
} from '@bc/components';
import { decodeCbor } from '@bc/dcbor';
import { HAS_RECIPIENT } from '@bc/known-values';

import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

function makeHasRecipient(recipient: Encrypter, contentKey: SymmetricKey, testNonce?: Nonce): Envelope {
    const sealedMessage = SealedMessage.newOpt(
        contentKey.taggedCborData(),
        recipient,
        undefined,
        testNonce,
    );
    return Envelope.newAssertion(HAS_RECIPIENT, sealedMessage);
}

function firstPlaintextInSealedMessages(sealedMessages: SealedMessage[], privateKey: Decrypter): Uint8Array {
    for (const sealedMessage of sealedMessages) {
        try {
            return sealedMessage.decrypt(privateKey);
        } catch {
            // try next
        }
    }
    throw EnvelopeError.unknownRecipient();
}

export function addRecipient(
    envelope: Envelope,
    recipient: Encrypter,
    contentKey: SymmetricKey,
): Envelope {
    return addRecipientOpt(envelope, recipient, contentKey);
}

export function addRecipientOpt(
    envelope: Envelope,
    recipient: Encrypter,
    contentKey: SymmetricKey,
    testNonce?: Nonce,
): Envelope {
    const assertion = makeHasRecipient(recipient, contentKey, testNonce);
    return envelope.addAssertionEnvelope(assertion);
}

export function recipients(envelope: Envelope): SealedMessage[] {
    return envelope
        .assertionsWithPredicate(HAS_RECIPIENT)
        .filter((assertion) => !assertion.asObject()!.isObscured())
        .map((assertion) => assertion.asObject()!.extractSubject<SealedMessage>());
}

export function encryptSubjectToRecipients(
    envelope: Envelope,
    recipientList: Encrypter[],
): Envelope {
    return encryptSubjectToRecipientsOpt(envelope, recipientList);
}

export function encryptSubjectToRecipientsOpt(
    envelope: Envelope,
    recipientList: Encrypter[],
    testNonce?: Nonce,
): Envelope {
    const contentKey = SymmetricKey.new();
    let result = envelope.encryptSubject(contentKey);
    for (const recipient of recipientList) {
        result = result.addRecipientOpt(recipient, contentKey, testNonce);
    }
    return result;
}

export function encryptSubjectToRecipient(
    envelope: Envelope,
    recipient: Encrypter,
): Envelope {
    return encryptSubjectToRecipientOpt(envelope, recipient);
}

export function encryptSubjectToRecipientOpt(
    envelope: Envelope,
    recipient: Encrypter,
    testNonce?: Nonce,
): Envelope {
    return encryptSubjectToRecipientsOpt(envelope, [recipient], testNonce);
}

export function decryptSubjectToRecipient(
    envelope: Envelope,
    recipient: Decrypter,
): Envelope {
    const sealedMessages = envelope.recipients();
    const contentKeyData = firstPlaintextInSealedMessages(sealedMessages, recipient);
    const contentKey = SymmetricKey.fromCbor(decodeCbor(contentKeyData));
    return envelope.decryptSubject(contentKey);
}

export function encryptToRecipient(envelope: Envelope, recipient: Encrypter): Envelope {
    return envelope.wrap().encryptSubjectToRecipient(recipient);
}

export function decryptToRecipient(envelope: Envelope, recipient: Decrypter): Envelope {
    return envelope.decryptSubjectToRecipient(recipient).unwrap();
}

declare module './envelope.js' {
    interface Envelope {
        addRecipient(recipient: Encrypter, contentKey: SymmetricKey): Envelope;
        addRecipientOpt(recipient: Encrypter, contentKey: SymmetricKey, testNonce?: Nonce): Envelope;
        recipients(): SealedMessage[];
        encryptSubjectToRecipients(recipientList: Encrypter[]): Envelope;
        encryptSubjectToRecipientsOpt(recipientList: Encrypter[], testNonce?: Nonce): Envelope;
        encryptSubjectToRecipient(recipient: Encrypter): Envelope;
        encryptSubjectToRecipientOpt(recipient: Encrypter, testNonce?: Nonce): Envelope;
        decryptSubjectToRecipient(recipient: Decrypter): Envelope;
        encryptToRecipient(recipient: Encrypter): Envelope;
        decryptToRecipient(recipient: Decrypter): Envelope;
    }
}

Envelope.prototype.addRecipient = function addRecipientProto(
    this: Envelope,
    recipient: Encrypter,
    contentKey: SymmetricKey,
): Envelope {
    return addRecipient(this, recipient, contentKey);
};
Envelope.prototype.addRecipientOpt = function addRecipientOptProto(
    this: Envelope,
    recipient: Encrypter,
    contentKey: SymmetricKey,
    testNonce?: Nonce,
): Envelope {
    return addRecipientOpt(this, recipient, contentKey, testNonce);
};
Envelope.prototype.recipients = function recipientsProto(this: Envelope): SealedMessage[] {
    return recipients(this);
};
Envelope.prototype.encryptSubjectToRecipients = function encryptSubjectToRecipientsProto(
    this: Envelope,
    recipientList: Encrypter[],
): Envelope {
    return encryptSubjectToRecipients(this, recipientList);
};
Envelope.prototype.encryptSubjectToRecipientsOpt = function encryptSubjectToRecipientsOptProto(
    this: Envelope,
    recipientList: Encrypter[],
    testNonce?: Nonce,
): Envelope {
    return encryptSubjectToRecipientsOpt(this, recipientList, testNonce);
};
Envelope.prototype.encryptSubjectToRecipient = function encryptSubjectToRecipientProto(
    this: Envelope,
    recipient: Encrypter,
): Envelope {
    return encryptSubjectToRecipient(this, recipient);
};
Envelope.prototype.encryptSubjectToRecipientOpt = function encryptSubjectToRecipientOptProto(
    this: Envelope,
    recipient: Encrypter,
    testNonce?: Nonce,
): Envelope {
    return encryptSubjectToRecipientOpt(this, recipient, testNonce);
};
Envelope.prototype.decryptSubjectToRecipient = function decryptSubjectToRecipientProto(
    this: Envelope,
    recipient: Decrypter,
): Envelope {
    return decryptSubjectToRecipient(this, recipient);
};
Envelope.prototype.encryptToRecipient = function encryptToRecipientProto(
    this: Envelope,
    recipient: Encrypter,
): Envelope {
    return encryptToRecipient(this, recipient);
};
Envelope.prototype.decryptToRecipient = function decryptToRecipientProto(
    this: Envelope,
    recipient: Decrypter,
): Envelope {
    return decryptToRecipient(this, recipient);
};
