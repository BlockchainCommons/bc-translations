import { Digest } from '@bc/components';
import {
    EDGE,
    IS_A,
    IS_A_RAW,
    SOURCE,
    SOURCE_RAW,
    TARGET,
    TARGET_RAW,
} from '@bc/known-values';

import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

export function addEdgeEnvelope(envelope: Envelope, edge: Envelope): Envelope {
    return envelope.addAssertion(EDGE, edge);
}

export function edges(envelope: Envelope): Envelope[] {
    return envelope.objectsForPredicate(EDGE);
}

export function validateEdge(envelope: Envelope): void {
    const inner = envelope.subject().isWrapped() ? envelope.subject().unwrap() : envelope;

    let seenIsA = false;
    let seenSource = false;
    let seenTarget = false;

    for (const assertion of inner.assertions()) {
        let predicateValue: bigint;
        try {
            predicateValue = assertion.tryPredicate().tryKnownValue().value;
        } catch {
            throw EnvelopeError.edgeUnexpectedAssertion();
        }

        if (predicateValue === IS_A_RAW) {
            if (seenIsA) {
                throw EnvelopeError.edgeDuplicateIsA();
            }
            seenIsA = true;
        } else if (predicateValue === SOURCE_RAW) {
            if (seenSource) {
                throw EnvelopeError.edgeDuplicateSource();
            }
            seenSource = true;
        } else if (predicateValue === TARGET_RAW) {
            if (seenTarget) {
                throw EnvelopeError.edgeDuplicateTarget();
            }
            seenTarget = true;
        } else {
            throw EnvelopeError.edgeUnexpectedAssertion();
        }
    }

    if (!seenIsA) throw EnvelopeError.edgeMissingIsA();
    if (!seenSource) throw EnvelopeError.edgeMissingSource();
    if (!seenTarget) throw EnvelopeError.edgeMissingTarget();
}

export function edgeIsA(envelope: Envelope): Envelope {
    const inner = envelope.subject().isWrapped() ? envelope.subject().unwrap() : envelope;
    return inner.objectForPredicate(IS_A);
}

export function edgeSource(envelope: Envelope): Envelope {
    const inner = envelope.subject().isWrapped() ? envelope.subject().unwrap() : envelope;
    return inner.objectForPredicate(SOURCE);
}

export function edgeTarget(envelope: Envelope): Envelope {
    const inner = envelope.subject().isWrapped() ? envelope.subject().unwrap() : envelope;
    return inner.objectForPredicate(TARGET);
}

export function edgeSubject(envelope: Envelope): Envelope {
    const inner = envelope.subject().isWrapped() ? envelope.subject().unwrap() : envelope;
    return inner.subject();
}

export function edgesMatching(
    envelope: Envelope,
    isA?: Envelope,
    source?: Envelope,
    target?: Envelope,
    subject?: Envelope,
): Envelope[] {
    return envelope.edges().filter((edge) => {
        if (isA != null) {
            try {
                if (!edge.edgeIsA().isEquivalentTo(isA)) {
                    return false;
                }
            } catch {
                return false;
            }
        }

        if (source != null) {
            try {
                if (!edge.edgeSource().isEquivalentTo(source)) {
                    return false;
                }
            } catch {
                return false;
            }
        }

        if (target != null) {
            try {
                if (!edge.edgeTarget().isEquivalentTo(target)) {
                    return false;
                }
            } catch {
                return false;
            }
        }

        if (subject != null) {
            try {
                if (!edge.edgeSubject().isEquivalentTo(subject)) {
                    return false;
                }
            } catch {
                return false;
            }
        }

        return true;
    });
}

export class Edges {
    readonly #envelopes = new Map<string, Envelope>();

    add(edgeEnvelope: Envelope): void {
        this.#envelopes.set(edgeEnvelope.digest().hex(), edgeEnvelope);
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

    get size(): number {
        return this.#envelopes.size;
    }

    get entries(): ReadonlyArray<[string, Envelope]> {
        return [...this.#envelopes.entries()];
    }

    addToEnvelope(envelope: Envelope): Envelope {
        let result = envelope;
        for (const edgeEnvelope of this.#envelopes.values()) {
            result = result.addAssertion(EDGE, edgeEnvelope);
        }
        return result;
    }

    static fromEnvelope(envelope: Envelope): Edges {
        const edges = new Edges();
        for (const edgeEnvelope of envelope.edges()) {
            edges.#envelopes.set(edgeEnvelope.digest().hex(), edgeEnvelope);
        }
        return edges;
    }
}

export interface Edgeable {
    edgesContainer(): Edges;
    mutableEdgesContainer(): Edges;
}

declare module './envelope.js' {
    interface Envelope {
        addEdgeEnvelope(edge: Envelope): Envelope;
        edges(): Envelope[];
        validateEdge(): void;
        edgeIsA(): Envelope;
        edgeSource(): Envelope;
        edgeTarget(): Envelope;
        edgeSubject(): Envelope;
        edgesMatching(isA?: Envelope, source?: Envelope, target?: Envelope, subject?: Envelope): Envelope[];
    }
}

Envelope.prototype.addEdgeEnvelope = function addEdgeEnvelopeProto(this: Envelope, edge: Envelope): Envelope {
    return addEdgeEnvelope(this, edge);
};
Envelope.prototype.edges = function edgesProto(this: Envelope): Envelope[] {
    return edges(this);
};
Envelope.prototype.validateEdge = function validateEdgeProto(this: Envelope): void {
    validateEdge(this);
};
Envelope.prototype.edgeIsA = function edgeIsAProto(this: Envelope): Envelope {
    return edgeIsA(this);
};
Envelope.prototype.edgeSource = function edgeSourceProto(this: Envelope): Envelope {
    return edgeSource(this);
};
Envelope.prototype.edgeTarget = function edgeTargetProto(this: Envelope): Envelope {
    return edgeTarget(this);
};
Envelope.prototype.edgeSubject = function edgeSubjectProto(this: Envelope): Envelope {
    return edgeSubject(this);
};
Envelope.prototype.edgesMatching = function edgesMatchingProto(
    this: Envelope,
    isA?: Envelope,
    source?: Envelope,
    target?: Envelope,
    subject?: Envelope,
): Envelope[] {
    return edgesMatching(this, isA, source, target, subject);
};
