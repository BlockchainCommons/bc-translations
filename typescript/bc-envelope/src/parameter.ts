import {
    cbor as toCbor,
    type Cbor,
    tagsForValues,
    toTaggedValue,
} from '@bc/dcbor';
import { TAG_PARAMETER } from '@bc/tags';

import { EnvelopeError } from './error.js';

export type EnvelopeParameter =
    | { kind: 'known'; value: bigint; assignedName?: string }
    | { kind: 'named'; name: string };

export function knownParameter(value: bigint | number, assignedName?: string): EnvelopeParameter {
    return { kind: 'known', value: BigInt(value), assignedName };
}

export function namedParameter(name: string): EnvelopeParameter {
    return { kind: 'named', name };
}

export function parameterName(value: EnvelopeParameter): string {
    if (value.kind === 'known') {
        return value.assignedName ?? value.value.toString();
    }
    return `"${value.name}"`;
}

export function parameterNamedName(value: EnvelopeParameter): string | undefined {
    return value.kind === 'named' ? value.name : undefined;
}

export function parameterUntaggedCbor(value: EnvelopeParameter): Cbor {
    if (value.kind === 'known') {
        return toCbor(value.value);
    }
    return toCbor(value.name);
}

export function parameterTaggedCbor(value: EnvelopeParameter): Cbor {
    return toTaggedValue(TAG_PARAMETER, parameterUntaggedCbor(value));
}

export function parameterCborTags() {
    return tagsForValues([TAG_PARAMETER]);
}

export function parameterFromUntaggedCbor(cbor: Cbor): EnvelopeParameter {
    if (cbor.isUnsigned()) {
        return knownParameter(cbor.toInteger() as bigint);
    }
    if (cbor.isText()) {
        return namedParameter(cbor.toText());
    }
    throw EnvelopeError.cbor('invalid parameter');
}

export function parameterFromTaggedCbor(cbor: Cbor): EnvelopeParameter {
    const [tag, item] = cbor.toTagged();
    if (BigInt(tag.value) !== BigInt(TAG_PARAMETER)) {
        throw EnvelopeError.cbor(`expected parameter tag ${TAG_PARAMETER}`);
    }
    return parameterFromUntaggedCbor(item);
}
