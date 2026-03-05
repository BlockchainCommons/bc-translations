import { Digest } from '@bc/components';
import {
    ATTACHMENT,
    CONFORMS_TO,
    VENDOR,
} from '@bc/known-values';

import { Assertion } from './assertion.js';
import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

export function newAttachment(payload: unknown, vendor: string, conformsTo?: string): Assertion {
    const payloadEnvelope = Envelope.from(payload)
        .wrap()
        .addAssertion(VENDOR, vendor);

    const withConformsTo = conformsTo != null
        ? payloadEnvelope.addAssertion(CONFORMS_TO, conformsTo)
        : payloadEnvelope;

    return Assertion.create(ATTACHMENT, withConformsTo);
}

export function addAttachment(envelope: Envelope, payload: unknown, vendor: string, conformsTo?: string): Envelope {
    return envelope.addAssertionEnvelope(newAttachment(payload, vendor, conformsTo).toEnvelope());
}

export function attachmentPayload(envelope: Envelope): Envelope {
    const c = envelope.case();
    if (c.kind !== 'assertion') {
        throw EnvelopeError.invalidAttachment();
    }
    return c.assertion.objectEnvelope().unwrap();
}

export function attachmentVendor(envelope: Envelope): string {
    const c = envelope.case();
    if (c.kind !== 'assertion') {
        throw EnvelopeError.invalidAttachment();
    }
    return c.assertion.objectEnvelope().extractObjectForPredicate(VENDOR);
}

export function attachmentConformsTo(envelope: Envelope): string | undefined {
    const c = envelope.case();
    if (c.kind !== 'assertion') {
        throw EnvelopeError.invalidAttachment();
    }
    return c.assertion.objectEnvelope().extractOptionalObjectForPredicate(CONFORMS_TO);
}

export function attachments(envelope: Envelope): Envelope[] {
    return attachmentsWithVendorAndConformsTo(envelope);
}

export function attachmentsWithVendorAndConformsTo(
    envelope: Envelope,
    vendor?: string,
    conformsTo?: string,
): Envelope[] {
    return envelope.assertionsWithPredicate(ATTACHMENT).filter((assertion) => {
        if (vendor != null) {
            try {
                if (attachmentVendor(assertion) !== vendor) {
                    return false;
                }
            } catch {
                return false;
            }
        }

        if (conformsTo != null) {
            try {
                if (attachmentConformsTo(assertion) !== conformsTo) {
                    return false;
                }
            } catch {
                return false;
            }
        }

        return true;
    });
}

export function attachmentWithVendorAndConformsTo(
    envelope: Envelope,
    vendor?: string,
    conformsTo?: string,
): Envelope {
    const matching = attachmentsWithVendorAndConformsTo(envelope, vendor, conformsTo);
    if (matching.length === 0) {
        throw EnvelopeError.nonexistentAttachment();
    }
    if (matching.length > 1) {
        throw EnvelopeError.ambiguousAttachment();
    }
    return matching[0]!;
}

export function validateAttachment(envelope: Envelope): void {
    if (envelope.case().kind !== 'assertion') {
        throw EnvelopeError.invalidAttachment();
    }
    attachmentPayload(envelope);
    attachmentVendor(envelope);
    attachmentConformsTo(envelope);
}

export class Attachments {
    readonly #envelopes = new Map<string, Envelope>();

    add(payload: unknown, vendor: string, conformsTo?: string): void {
        const attachment = Envelope.newAttachment(payload, vendor, conformsTo);
        this.#envelopes.set(attachment.digest().hex(), attachment);
    }

    get(digest: Digest): Envelope | undefined {
        return this.#envelopes.get(digest.hex());
    }

    remove(digest: Digest): Envelope | undefined {
        const key = digest.hex();
        const existing = this.#envelopes.get(key);
        this.#envelopes.delete(key);
        return existing;
    }

    clear(): void {
        this.#envelopes.clear();
    }

    isEmpty(): boolean {
        return this.#envelopes.size === 0;
    }

    addToEnvelope(envelope: Envelope): Envelope {
        let result = envelope;
        for (const attachment of this.#envelopes.values()) {
            result = result.addAssertionEnvelope(attachment);
        }
        return result;
    }

    static fromEnvelope(envelope: Envelope): Attachments {
        const attachments = new Attachments();
        for (const attachment of envelope.attachments()) {
            attachments.#envelopes.set(attachment.digest().hex(), attachment);
        }
        return attachments;
    }
}

export interface Attachable {
    attachments(): Attachments;
    mutableAttachments(): Attachments;
}

declare module './envelope.js' {
    interface Envelope {
        addAttachment(payload: unknown, vendor: string, conformsTo?: string): Envelope;
        attachmentPayload(): Envelope;
        attachmentVendor(): string;
        attachmentConformsTo(): string | undefined;
        attachments(): Envelope[];
        attachmentsWithVendorAndConformsTo(vendor?: string, conformsTo?: string): Envelope[];
        attachmentWithVendorAndConformsTo(vendor?: string, conformsTo?: string): Envelope;
        validateAttachment(): void;
    }

    namespace Envelope {
        let newAttachment: (payload: unknown, vendor: string, conformsTo?: string) => Envelope;
    }
}

Envelope.prototype.addAttachment = function addAttachmentProto(
    this: Envelope,
    payload: unknown,
    vendor: string,
    conformsTo?: string,
): Envelope {
    return addAttachment(this, payload, vendor, conformsTo);
};
Envelope.prototype.attachmentPayload = function attachmentPayloadProto(this: Envelope): Envelope {
    return attachmentPayload(this);
};
Envelope.prototype.attachmentVendor = function attachmentVendorProto(this: Envelope): string {
    return attachmentVendor(this);
};
Envelope.prototype.attachmentConformsTo = function attachmentConformsToProto(this: Envelope): string | undefined {
    return attachmentConformsTo(this);
};
Envelope.prototype.attachments = function attachmentsProto(this: Envelope): Envelope[] {
    return attachments(this);
};
Envelope.prototype.attachmentsWithVendorAndConformsTo = function attachmentsWithVendorAndConformsToProto(
    this: Envelope,
    vendor?: string,
    conformsTo?: string,
): Envelope[] {
    return attachmentsWithVendorAndConformsTo(this, vendor, conformsTo);
};
Envelope.prototype.attachmentWithVendorAndConformsTo = function attachmentWithVendorAndConformsToProto(
    this: Envelope,
    vendor?: string,
    conformsTo?: string,
): Envelope {
    return attachmentWithVendorAndConformsTo(this, vendor, conformsTo);
};
Envelope.prototype.validateAttachment = function validateAttachmentProto(this: Envelope): void {
    validateAttachment(this);
};

(Envelope as typeof Envelope & { newAttachment: (payload: unknown, vendor: string, conformsTo?: string) => Envelope }).newAttachment = (
    payload: unknown,
    vendor: string,
    conformsTo?: string,
): Envelope => newAttachment(payload, vendor, conformsTo).toEnvelope();
