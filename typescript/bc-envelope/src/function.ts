import {
    cbor as toCbor,
    type Cbor,
    tagsForValues,
    toTaggedValue,
} from '@bc/dcbor';
import { TAG_FUNCTION } from '@bc/tags';

import { EnvelopeError } from './error.js';

export type EnvelopeFunction =
    | { kind: 'known'; value: bigint; assignedName?: string }
    | { kind: 'named'; name: string };

export function knownFunction(value: bigint | number, assignedName?: string): EnvelopeFunction {
    return { kind: 'known', value: BigInt(value), assignedName };
}

export function namedFunction(name: string): EnvelopeFunction {
    return { kind: 'named', name };
}

export function functionName(value: EnvelopeFunction): string {
    if (value.kind === 'known') {
        return value.assignedName ?? value.value.toString();
    }
    return `"${value.name}"`;
}

export function functionNamedName(value: EnvelopeFunction): string | undefined {
    return value.kind === 'named' ? value.name : undefined;
}

export function functionUntaggedCbor(value: EnvelopeFunction): Cbor {
    if (value.kind === 'known') {
        return toCbor(value.value);
    }
    return toCbor(value.name);
}

export function functionTaggedCbor(value: EnvelopeFunction): Cbor {
    return toTaggedValue(TAG_FUNCTION, functionUntaggedCbor(value));
}

export function functionCborTags() {
    return tagsForValues([TAG_FUNCTION]);
}

export function functionFromUntaggedCbor(cbor: Cbor): EnvelopeFunction {
    if (cbor.isUnsigned()) {
        return knownFunction(cbor.toInteger() as bigint);
    }
    if (cbor.isText()) {
        return namedFunction(cbor.toText());
    }
    throw EnvelopeError.cbor('invalid function');
}

export function functionsEqual(a: EnvelopeFunction, b: EnvelopeFunction): boolean {
    if (a.kind !== b.kind) return false;
    if (a.kind === 'known' && b.kind === 'known') return a.value === b.value;
    if (a.kind === 'named' && b.kind === 'named') return a.name === b.name;
    return false;
}

export function functionFromTaggedCbor(cbor: Cbor): EnvelopeFunction {
    const [tag, item] = cbor.toTagged();
    if (BigInt(tag.value) !== BigInt(TAG_FUNCTION)) {
        throw EnvelopeError.cbor(`expected function tag ${TAG_FUNCTION}`);
    }
    return functionFromUntaggedCbor(item);
}
