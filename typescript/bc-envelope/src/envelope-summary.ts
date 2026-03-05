import {
    diagnosticOpt,
    type Cbor,
} from '@bc/dcbor';
import { KnownValuesStore } from '@bc/known-values';

import type { Envelope } from './envelope.js';
import { flankedBy } from './string-utils.js';
import type { FormatContext, FormatContextOpt } from './format-context.js';
import { FormatContextOpts, withFormatContext } from './format-context.js';

function cborEnvelopeSummary(cbor: Cbor, maxLength: number, context: FormatContextOpt): string {
    if (cbor.isUnsigned()) {
        return `${cbor.toInteger()}`;
    }
    if (cbor.isNegative()) {
        return `${cbor.toInteger()}`;
    }
    if (cbor.isByteString()) {
        return `Bytes(${cbor.toByteString().length})`;
    }
    if (cbor.isText()) {
        const value = cbor.toText();
        const display = value.length > maxLength ? `${value.slice(0, maxLength)}\u2026` : value;
        return flankedBy(display.replaceAll('\n', '\\n'), '"', '"');
    }
    if (cbor.isSimple()) {
        return cbor.toString();
    }

    if (context.type === 'none') {
        return diagnosticOpt(cbor, { summarize: true, flat: true });
    }
    if (context.type === 'global') {
        return withFormatContext((ctx) => diagnosticOpt(cbor, {
            summarize: true,
            flat: true,
            tags: ctx.tags(),
        }));
    }
    return diagnosticOpt(cbor, {
        summarize: true,
        flat: true,
        tags: context.context.tags(),
    });
}

export function envelopeSummary(envelope: Envelope, maxLength: number, context: FormatContext): string {
    const c = envelope.case();
    switch (c.kind) {
        case 'node':
            return 'NODE';
        case 'leaf':
            return cborEnvelopeSummary(c.cbor, maxLength, FormatContextOpts.custom(context));
        case 'wrapped':
            return 'WRAPPED';
        case 'assertion':
            return 'ASSERTION';
        case 'elided':
            return 'ELIDED';
        case 'known-value': {
            const knownValue = KnownValuesStore.knownValueForRawValue(c.value.value, context.knownValues());
            return flankedBy(knownValue.toString(), "'", "'");
        }
        case 'encrypted':
            return 'ENCRYPTED';
        case 'compressed':
            return 'COMPRESSED';
    }
}
