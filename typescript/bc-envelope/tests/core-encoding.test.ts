import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';
import { checkEncoding, doubleAssertionEnvelope } from './test-data.js';

describe('core encoding tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('tagged cbor roundtrip preserves identity', () => {
        const envelope = checkEncoding(doubleAssertionEnvelope());
        const restored = Envelope.fromTaggedCborData(envelope.taggedCborData());
        expect(restored.isIdenticalTo(envelope)).toBe(true);
    });

    test('untagged cbor roundtrip for assertion', () => {
        const envelope = Envelope.newAssertion('knows', 'Bob');
        const restored = Envelope.fromUntaggedCbor(envelope.untaggedCbor());
        expect(restored.isIdenticalTo(envelope)).toBe(true);
    });

    test('ur roundtrip preserves digest', () => {
        const envelope = Envelope.from('UR roundtrip').addAssertion('v', 1);
        const restored = Envelope.fromUrString(envelope.urString());
        expect(restored.digest().equals(envelope.digest())).toBe(true);
    });
});
