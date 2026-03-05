import {
    IS_A,
    type KnownValue,
} from '@bc/known-values';

import { Envelope } from './envelope.js';
import { EnvelopeError } from './error.js';

export function addType(envelope: Envelope, objectValue: unknown): Envelope {
    return envelope.addAssertion(IS_A, objectValue);
}

export function types(envelope: Envelope): Envelope[] {
    return envelope.objectsForPredicate(IS_A);
}

export function getType(envelope: Envelope): Envelope {
    const values = types(envelope);
    if (values.length === 1) {
        return values[0]!;
    }
    throw EnvelopeError.ambiguousType();
}

export function hasType(envelope: Envelope, typeValue: unknown): boolean {
    const expected = Envelope.from(typeValue);
    return types(envelope).some((item) => item.digest().equals(expected.digest()));
}

export function hasTypeValue(envelope: Envelope, typeValue: KnownValue): boolean {
    const expected = Envelope.from(typeValue);
    return types(envelope).some((item) => item.digest().equals(expected.digest()));
}

export function checkTypeValue(envelope: Envelope, typeValue: KnownValue): void {
    if (!hasTypeValue(envelope, typeValue)) {
        throw EnvelopeError.invalidType();
    }
}

export function checkType(envelope: Envelope, typeValue: unknown): void {
    if (!hasType(envelope, typeValue)) {
        throw EnvelopeError.invalidType();
    }
}

declare module './envelope.js' {
    interface Envelope {
        addType(objectValue: unknown): Envelope;
        types(): Envelope[];
        getType(): Envelope;
        hasType(typeValue: unknown): boolean;
        hasTypeValue(typeValue: KnownValue): boolean;
        checkTypeValue(typeValue: KnownValue): void;
        checkType(typeValue: unknown): void;
    }
}

Envelope.prototype.addType = function addTypeProto(this: Envelope, objectValue: unknown): Envelope {
    return addType(this, objectValue);
};
Envelope.prototype.types = function typesProto(this: Envelope): Envelope[] {
    return types(this);
};
Envelope.prototype.getType = function getTypeProto(this: Envelope): Envelope {
    return getType(this);
};
Envelope.prototype.hasType = function hasTypeProto(this: Envelope, typeValue: unknown): boolean {
    return hasType(this, typeValue);
};
Envelope.prototype.hasTypeValue = function hasTypeValueProto(this: Envelope, typeValue: KnownValue): boolean {
    return hasTypeValue(this, typeValue);
};
Envelope.prototype.checkTypeValue = function checkTypeValueProto(this: Envelope, typeValue: KnownValue): void {
    checkTypeValue(this, typeValue);
};
Envelope.prototype.checkType = function checkTypeProto(this: Envelope, typeValue: unknown): void {
    checkType(this, typeValue);
};
