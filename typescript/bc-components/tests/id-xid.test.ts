import { describe, expect, test } from 'vitest';

import {
    Digest,
    ECPrivateKey,
    SigningPrivateKey,
    XID,
    registerTags,
} from '../src/index.js';
import { hexToBytes } from './test-helpers.js';

describe('XID', () => {
    test('vector and identifiers', () => {
        registerTags();
        const xid = XID.fromDataRef(
            hexToBytes('de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037'),
        );

        expect(xid.toHex()).toBe(
            'de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037',
        );
        expect(xid.shortDescription).toBe('de285368');
        expect(xid.toString()).toBe('XID(de285368)');

        const xidString = xid.urString();
        expect(xidString).toBe(
            'ur:xid/hdcxuedeguisgevwhdaxnbluenutlbglhfiygamsamadmojkdydtneteeowffhwprtemcaatledk',
        );
        expect(XID.fromURString(xidString).equals(xid)).toBe(true);
        expect(xid.bytewordsIdentifier(true)).toBe('🅧 URGE DICE GURU IRIS');
        expect(xid.bytemojiIdentifier(true)).toBe('🅧 🐻 😻 🍞 💐');
    });

    test('xid derived from signing key matches digest of cbor', () => {
        const privateKey = SigningPrivateKey.newSchnorr(
            ECPrivateKey.fromData(
                hexToBytes('322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36'),
            ),
        );
        const publicKey = privateKey.publicKey();

        const keyCborData = publicKey.taggedCborData();
        const digest = Digest.fromImage(keyCborData);
        const xid = XID.new(publicKey);

        expect(Buffer.from(digest.data).toString('hex')).toBe(
            'd40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47',
        );
        expect(Buffer.from(xid.data).toString('hex')).toBe(
            'd40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47',
        );
        expect(xid.validate(publicKey)).toBe(true);
    });
});
