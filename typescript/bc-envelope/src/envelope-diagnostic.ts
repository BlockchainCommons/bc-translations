import { diagnosticOpt } from '@bc/dcbor';

import type { Envelope } from './envelope.js';
import { withFormatContext } from './format-context.js';

export function diagnosticAnnotated(envelope: Envelope): string {
    return withFormatContext((context) => diagnosticOpt(envelope.taggedCbor(), {
        annotate: true,
        tags: context.tags(),
    }));
}

export function diagnostic(envelope: Envelope): string {
    return envelope.taggedCbor().toDiagnostic();
}
