import type { Envelope } from './envelope.js';

export function envelopeHex(envelope: Envelope): string {
    return envelope.taggedCbor().toHex();
}
