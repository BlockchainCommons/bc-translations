import { beforeAll, describe, expect, test } from 'vitest';
import { GroupSpec, Spec } from '@bc/sskr';

import { Envelope, registerTags } from '../src/index.js';
import { fakeContentKey } from './test-data.js';

describe('sskr tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('sskr split and join', () => {
        const original = Envelope.from('Secret message').addAssertion('metadata', 'This is a test');
        const contentKey = fakeContentKey();
        const wrappedOriginal = original.wrap();
        const encrypted = wrappedOriginal.encryptSubject(contentKey);

        const group = GroupSpec.create(2, 3);
        const spec = Spec.create(1, [group]);

        const shares = encrypted.sskrSplit(spec, contentKey);
        expect(shares.length).toBe(1);
        expect(shares[0]!.length).toBe(3);

        const recoveredWrapped = Envelope.sskrJoin([shares[0]![0]!, shares[0]![1]!]);
        const recovered = recoveredWrapped.unwrap();
        expect(recovered.isIdenticalTo(original)).toBe(true);
    });
});
