import {
    Compressed,
    Digest,
    EncryptedMessage,
} from '@bc/components';
import type { Cbor } from '@bc/dcbor';
import type { KnownValue } from '@bc/known-values';

import type { Assertion } from './assertion.js';
import type { Envelope } from './envelope.js';

export type EnvelopeCase =
    | {
        kind: 'node';
        subject: Envelope;
        assertions: Envelope[];
        digest: Digest;
    }
    | {
        kind: 'leaf';
        cbor: Cbor;
        digest: Digest;
    }
    | {
        kind: 'wrapped';
        envelope: Envelope;
        digest: Digest;
    }
    | {
        kind: 'assertion';
        assertion: Assertion;
    }
    | {
        kind: 'elided';
        digest: Digest;
    }
    | {
        kind: 'known-value';
        value: KnownValue;
        digest: Digest;
    }
    | {
        kind: 'encrypted';
        encryptedMessage: EncryptedMessage;
    }
    | {
        kind: 'compressed';
        compressed: Compressed;
    };
