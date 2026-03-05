import {
    SSKRShare,
    type SSKRSpec,
    sskrCombine,
    sskrGenerateUsing,
    SymmetricKey,
} from '@bc/components';
import { type RandomNumberGenerator, SecureRandomNumberGenerator } from '@bc/rand';
import { SSKR_SHARE } from '@bc/known-values';
import { Secret as SSKRSecret } from '@bc/sskr';

import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

function addSskrShare(envelope: Envelope, share: SSKRShare): Envelope {
    return envelope.addAssertion(SSKR_SHARE, share);
}

function sskrSharesIn(envelopes: Envelope[]): Map<number, SSKRShare[]> {
    const result = new Map<number, SSKRShare[]>();
    for (const envelope of envelopes) {
        for (const assertion of envelope.assertionsWithPredicate(SSKR_SHARE)) {
            const share = assertion.asObject()!.extractSubject<SSKRShare>();
            const id = share.identifier();
            const group = result.get(id) ?? [];
            group.push(share);
            result.set(id, group);
        }
    }
    return result;
}

export function sskrSplit(
    envelope: Envelope,
    spec: SSKRSpec,
    contentKey: SymmetricKey,
): Envelope[][] {
    return sskrSplitUsing(envelope, spec, contentKey, new SecureRandomNumberGenerator());
}

export function sskrSplitFlattened(
    envelope: Envelope,
    spec: SSKRSpec,
    contentKey: SymmetricKey,
): Envelope[] {
    return sskrSplit(envelope, spec, contentKey).flat();
}

export function sskrSplitUsing(
    envelope: Envelope,
    spec: SSKRSpec,
    contentKey: SymmetricKey,
    testRng: RandomNumberGenerator,
): Envelope[][] {
    const masterSecret = SSKRSecret.create(contentKey.data);
    const shares = sskrGenerateUsing(spec, masterSecret, testRng);
    return shares.map((group) => group.map((share) => addSskrShare(envelope, share)));
}

export function sskrJoin(envelopes: Envelope[]): Envelope {
    if (envelopes.length === 0) {
        throw EnvelopeError.invalidShares();
    }

    const grouped = sskrSharesIn(envelopes);
    for (const shares of grouped.values()) {
        try {
            const secret = sskrCombine(shares);
            const contentKey = SymmetricKey.fromData(secret.data);
            const envelope = envelopes[0]!.decryptSubject(contentKey);
            return envelope.subject();
        } catch {
            // try next group
        }
    }
    throw EnvelopeError.invalidShares();
}

declare module './envelope.js' {
    interface Envelope {
        sskrSplit(spec: SSKRSpec, contentKey: SymmetricKey): Envelope[][];
        sskrSplitFlattened(spec: SSKRSpec, contentKey: SymmetricKey): Envelope[];
        sskrSplitUsing(spec: SSKRSpec, contentKey: SymmetricKey, testRng: RandomNumberGenerator): Envelope[][];
    }

    namespace Envelope {
        let sskrJoin: (envelopes: Envelope[]) => Envelope;
    }
}

Envelope.prototype.sskrSplit = function sskrSplitProto(this: Envelope, spec: SSKRSpec, contentKey: SymmetricKey): Envelope[][] {
    return sskrSplit(this, spec, contentKey);
};
Envelope.prototype.sskrSplitFlattened = function sskrSplitFlattenedProto(this: Envelope, spec: SSKRSpec, contentKey: SymmetricKey): Envelope[] {
    return sskrSplitFlattened(this, spec, contentKey);
};
Envelope.prototype.sskrSplitUsing = function sskrSplitUsingProto(
    this: Envelope,
    spec: SSKRSpec,
    contentKey: SymmetricKey,
    testRng: RandomNumberGenerator,
): Envelope[][] {
    return sskrSplitUsing(this, spec, contentKey, testRng);
};

(Envelope as typeof Envelope & { sskrJoin: (envelopes: Envelope[]) => Envelope }).sskrJoin = sskrJoin;
