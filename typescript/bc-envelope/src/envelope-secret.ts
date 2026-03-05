import {
    EncryptedKey,
    type KeyDerivationMethod,
    SymmetricKey,
} from '@bc/components';
import { HAS_SECRET } from '@bc/known-values';

import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

const UTF8 = new TextEncoder();

export function lockSubject(envelope: Envelope, method: KeyDerivationMethod, secret: Uint8Array): Envelope {
    const contentKey = SymmetricKey.new();
    const encryptedKey = EncryptedKey.lock(method, secret, contentKey);
    return envelope.encryptSubject(contentKey).addAssertion(HAS_SECRET, encryptedKey);
}

export function lockSubjectWithPassword(envelope: Envelope, method: KeyDerivationMethod, password: string): Envelope {
    return lockSubject(envelope, method, UTF8.encode(password));
}

export function unlockSubject(envelope: Envelope, secret: Uint8Array): Envelope {
    for (const assertion of envelope.assertionsWithPredicate(HAS_SECRET)) {
        const objectEnvelope = assertion.asObject();
        if (objectEnvelope == null || objectEnvelope.isObscured()) {
            continue;
        }
        try {
            const encryptedKey = objectEnvelope.extractSubject<EncryptedKey>();
            const contentKey = encryptedKey.unlock(secret);
            return envelope.decryptSubject(contentKey);
        } catch {
            // try next assertion
        }
    }
    throw EnvelopeError.unknownSecret();
}

export function unlockSubjectWithPassword(envelope: Envelope, password: string): Envelope {
    return unlockSubject(envelope, UTF8.encode(password));
}

export function isLockedWithPassword(envelope: Envelope): boolean {
    return envelope.assertionsWithPredicate(HAS_SECRET).some((assertion) => {
        try {
            const objectEnvelope = assertion.asObject();
            if (objectEnvelope == null) {
                return false;
            }
            const encryptedKey = objectEnvelope.extractSubject<EncryptedKey>();
            return encryptedKey.isPasswordBased();
        } catch {
            return false;
        }
    });
}

export function isLockedWithSshAgent(envelope: Envelope): boolean {
    return envelope.assertionsWithPredicate(HAS_SECRET).some((assertion) => {
        try {
            const objectEnvelope = assertion.asObject();
            if (objectEnvelope == null) {
                return false;
            }
            const encryptedKey = objectEnvelope.extractSubject<EncryptedKey>();
            return encryptedKey.isSshAgent();
        } catch {
            return false;
        }
    });
}

export function addSecret(
    envelope: Envelope,
    method: KeyDerivationMethod,
    secret: Uint8Array,
    contentKey: SymmetricKey,
): Envelope {
    const encryptedKey = EncryptedKey.lock(method, secret, contentKey);
    return envelope.addAssertion(HAS_SECRET, encryptedKey);
}

export function addSecretWithPassword(
    envelope: Envelope,
    method: KeyDerivationMethod,
    password: string,
    contentKey: SymmetricKey,
): Envelope {
    return addSecret(envelope, method, UTF8.encode(password), contentKey);
}

export function lock(envelope: Envelope, method: KeyDerivationMethod, secret: Uint8Array): Envelope {
    return envelope.wrap().lockSubject(method, secret);
}

export function lockWithPassword(envelope: Envelope, method: KeyDerivationMethod, password: string): Envelope {
    return lock(envelope, method, UTF8.encode(password));
}

export function unlock(envelope: Envelope, secret: Uint8Array): Envelope {
    return envelope.unlockSubject(secret).unwrap();
}

export function unlockWithPassword(envelope: Envelope, password: string): Envelope {
    return unlock(envelope, UTF8.encode(password));
}

declare module './envelope.js' {
    interface Envelope {
        lockSubject(method: KeyDerivationMethod, secret: Uint8Array): Envelope;
        lockSubjectWithPassword(method: KeyDerivationMethod, password: string): Envelope;
        unlockSubject(secret: Uint8Array): Envelope;
        unlockSubjectWithPassword(password: string): Envelope;
        isLockedWithPassword(): boolean;
        isLockedWithSshAgent(): boolean;
        addSecret(method: KeyDerivationMethod, secret: Uint8Array, contentKey: SymmetricKey): Envelope;
        addSecretWithPassword(method: KeyDerivationMethod, password: string, contentKey: SymmetricKey): Envelope;
        lock(method: KeyDerivationMethod, secret: Uint8Array): Envelope;
        lockWithPassword(method: KeyDerivationMethod, password: string): Envelope;
        unlock(secret: Uint8Array): Envelope;
        unlockWithPassword(password: string): Envelope;
    }
}

Envelope.prototype.lockSubject = function lockSubjectProto(
    this: Envelope,
    method: KeyDerivationMethod,
    secret: Uint8Array,
): Envelope {
    return lockSubject(this, method, secret);
};
Envelope.prototype.lockSubjectWithPassword = function lockSubjectWithPasswordProto(
    this: Envelope,
    method: KeyDerivationMethod,
    password: string,
): Envelope {
    return lockSubjectWithPassword(this, method, password);
};
Envelope.prototype.unlockSubject = function unlockSubjectProto(this: Envelope, secret: Uint8Array): Envelope {
    return unlockSubject(this, secret);
};
Envelope.prototype.unlockSubjectWithPassword = function unlockSubjectWithPasswordProto(this: Envelope, password: string): Envelope {
    return unlockSubjectWithPassword(this, password);
};
Envelope.prototype.isLockedWithPassword = function isLockedWithPasswordProto(this: Envelope): boolean {
    return isLockedWithPassword(this);
};
Envelope.prototype.isLockedWithSshAgent = function isLockedWithSshAgentProto(this: Envelope): boolean {
    return isLockedWithSshAgent(this);
};
Envelope.prototype.addSecret = function addSecretProto(
    this: Envelope,
    method: KeyDerivationMethod,
    secret: Uint8Array,
    contentKey: SymmetricKey,
): Envelope {
    return addSecret(this, method, secret, contentKey);
};
Envelope.prototype.addSecretWithPassword = function addSecretWithPasswordProto(
    this: Envelope,
    method: KeyDerivationMethod,
    password: string,
    contentKey: SymmetricKey,
): Envelope {
    return addSecretWithPassword(this, method, password, contentKey);
};
Envelope.prototype.lock = function lockProto(this: Envelope, method: KeyDerivationMethod, secret: Uint8Array): Envelope {
    return lock(this, method, secret);
};
Envelope.prototype.lockWithPassword = function lockWithPasswordProto(
    this: Envelope,
    method: KeyDerivationMethod,
    password: string,
): Envelope {
    return lockWithPassword(this, method, password);
};
Envelope.prototype.unlock = function unlockProto(this: Envelope, secret: Uint8Array): Envelope {
    return unlock(this, secret);
};
Envelope.prototype.unlockWithPassword = function unlockWithPasswordProto(this: Envelope, password: string): Envelope {
    return unlockWithPassword(this, password);
};
