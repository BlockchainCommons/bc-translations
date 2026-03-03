import { describe, expect, test } from 'vitest';

import {
    PrivateKeyBase,
    registerTags,
} from '../src/index.js';
import { expectBytes, hexToBytes } from './test-helpers.js';

const SEED = hexToBytes('59f2293a5bce7d4de59e71b4207ac5d2');

describe('PrivateKeyBase', () => {
    test('derives expected deterministic keys', () => {
        registerTags();
        const base = PrivateKeyBase.fromData(SEED);

        expectBytes(
            base.ecdsaSigningPrivateKey().toEcdsa()!.data(),
            '9505a44aaf385ce633cf0e2bc49e65cc88794213bdfbf8caf04150b9c4905f5a',
        );

        expectBytes(
            base
                .schnorrSigningPrivateKey()
                .publicKey()
                .toSchnorr()!
                .data(),
            'fd4d22f9e8493da52d730aa402ac9e661deca099ef4db5503f519a73c3493e18',
        );

        expectBytes(
            base.x25519PrivateKey().data,
            '77ff838285a0403d3618aa8c30491f99f55221be0b944f50bfb371f43b897485',
        );

        expectBytes(
            base.x25519PrivateKey().publicKey().data,
            '863cf3facee3ba45dc54e5eedecb21d791d64adfb0a1c63bfb6fea366c1ee62b',
        );

        const ur = base.urString();
        expect(ur).toBe('ur:crypto-prvkey-base/gdhkwzdtfthptokigtvwnnjsqzcxknsktdsfecsbbk');
        expect(PrivateKeyBase.fromURString(ur).equals(base)).toBe(true);
    });
});
