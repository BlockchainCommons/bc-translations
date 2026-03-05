import {
    type Decrypter,
    type Encrypter,
    type Signer,
    type SigningOptions,
    type Verifier,
} from '@bc/components';

import { Envelope } from './envelope.js';

export function seal(envelope: Envelope, sender: Signer, recipient: Encrypter): Envelope {
    return envelope.sign(sender).encryptToRecipient(recipient);
}

export function sealOpt(
    envelope: Envelope,
    sender: Signer,
    recipient: Encrypter,
    options?: SigningOptions,
): Envelope {
    return envelope.signOpt(sender, options).encryptToRecipient(recipient);
}

export function unseal(envelope: Envelope, sender: Verifier, recipient: Decrypter): Envelope {
    return envelope.decryptToRecipient(recipient).verify(sender);
}

declare module './envelope.js' {
    interface Envelope {
        seal(sender: Signer, recipient: Encrypter): Envelope;
        sealOpt(sender: Signer, recipient: Encrypter, options?: SigningOptions): Envelope;
        unseal(sender: Verifier, recipient: Decrypter): Envelope;
    }
}

Envelope.prototype.seal = function sealProto(this: Envelope, sender: Signer, recipient: Encrypter): Envelope {
    return seal(this, sender, recipient);
};
Envelope.prototype.sealOpt = function sealOptProto(
    this: Envelope,
    sender: Signer,
    recipient: Encrypter,
    options?: SigningOptions,
): Envelope {
    return sealOpt(this, sender, recipient, options);
};
Envelope.prototype.unseal = function unsealProto(this: Envelope, sender: Verifier, recipient: Decrypter): Envelope {
    return unseal(this, sender, recipient);
};
