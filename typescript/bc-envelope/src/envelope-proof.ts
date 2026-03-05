import { Digest, type DigestProvider } from '@bc/components';

import { Envelope } from './envelope.js';

function digestIn(target: Set<Digest>, digest: Digest): boolean {
    for (const item of target) {
        if (item.equals(digest)) {
            return true;
        }
    }
    return false;
}

function revealSets(
    envelope: Envelope,
    target: Set<Digest>,
    current: Set<Digest>,
    result: Set<Digest>,
): void {
    const currentWithSelf = new Set<Digest>([...current, envelope.digest()]);
    if (digestIn(target, envelope.digest())) {
        for (const digest of currentWithSelf) {
            result.add(digest);
        }
    }

    const c = envelope.case();
    if (c.kind === 'node') {
        revealSets(c.subject, target, currentWithSelf, result);
        for (const assertion of c.assertions) {
            revealSets(assertion, target, currentWithSelf, result);
        }
    } else if (c.kind === 'wrapped') {
        revealSets(c.envelope, target, currentWithSelf, result);
    } else if (c.kind === 'assertion') {
        revealSets(c.assertion.predicate(), target, currentWithSelf, result);
        revealSets(c.assertion.objectEnvelope(), target, currentWithSelf, result);
    }
}

function revealSetOfSet(envelope: Envelope, target: Set<Digest>): Set<Digest> {
    const result = new Set<Digest>();
    revealSets(envelope, target, new Set<Digest>(), result);
    return result;
}

function removeAllFound(envelope: Envelope, target: Set<Digest>): void {
    for (const digest of [...target]) {
        if (digest.equals(envelope.digest())) {
            target.delete(digest);
            break;
        }
    }

    if (target.size === 0) {
        return;
    }

    const c = envelope.case();
    if (c.kind === 'node') {
        removeAllFound(c.subject, target);
        for (const assertion of c.assertions) {
            removeAllFound(assertion, target);
        }
    } else if (c.kind === 'wrapped') {
        removeAllFound(c.envelope, target);
    } else if (c.kind === 'assertion') {
        removeAllFound(c.assertion.predicate(), target);
        removeAllFound(c.assertion.objectEnvelope(), target);
    }
}

function containsAll(envelope: Envelope, target: Set<Digest>): boolean {
    const remaining = new Set<Digest>(target);
    removeAllFound(envelope, remaining);
    return remaining.size === 0;
}

export function proofContainsSet(envelope: Envelope, target: Set<Digest>): Envelope | undefined {
    const revealSet = revealSetOfSet(envelope, target);
    for (const digest of target) {
        if (!digestIn(revealSet, digest)) {
            return undefined;
        }
    }
    return envelope.elideRevealingSet(revealSet).elideRemovingSet(target);
}

export function proofContainsTarget(envelope: Envelope, target: DigestProvider): Envelope | undefined {
    return proofContainsSet(envelope, new Set([target.digest()]));
}

export function confirmContainsSet(envelope: Envelope, target: Set<Digest>, proof: Envelope): boolean {
    return envelope.digest().equals(proof.digest()) && containsAll(proof, target);
}

export function confirmContainsTarget(envelope: Envelope, target: DigestProvider, proof: Envelope): boolean {
    return confirmContainsSet(envelope, new Set([target.digest()]), proof);
}

declare module './envelope.js' {
    interface Envelope {
        proofContainsSet(target: Set<Digest>): Envelope | undefined;
        proofContainsTarget(target: DigestProvider): Envelope | undefined;
        confirmContainsSet(target: Set<Digest>, proof: Envelope): boolean;
        confirmContainsTarget(target: DigestProvider, proof: Envelope): boolean;
    }
}

Envelope.prototype.proofContainsSet = function proofContainsSetProto(this: Envelope, target: Set<Digest>): Envelope | undefined {
    return proofContainsSet(this, target);
};
Envelope.prototype.proofContainsTarget = function proofContainsTargetProto(this: Envelope, target: DigestProvider): Envelope | undefined {
    return proofContainsTarget(this, target);
};
Envelope.prototype.confirmContainsSet = function confirmContainsSetProto(this: Envelope, target: Set<Digest>, proof: Envelope): boolean {
    return confirmContainsSet(this, target, proof);
};
Envelope.prototype.confirmContainsTarget = function confirmContainsTargetProto(this: Envelope, target: DigestProvider, proof: Envelope): boolean {
    return confirmContainsTarget(this, target, proof);
};
