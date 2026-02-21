import { describe, expect, test } from 'vitest';

import { splitSecret, recoverSecret } from '../src/index.js';
import { FakeRandomNumberGenerator, hexToBytes } from './test-helpers.js';

describe('bc-shamir', () => {
    test('should split secret 3/5', () => {
        const rng = new FakeRandomNumberGenerator();
        const secret = hexToBytes('0ff784df000c4380a5ed683f7e6e3dcf');
        const shares = splitSecret(3, 5, secret, rng);
        expect(shares.length).toBe(5);

        expect(shares[0]).toEqual(hexToBytes('00112233445566778899aabbccddeeff'));
        expect(shares[1]).toEqual(hexToBytes('d43099fe444807c46921a4f33a2a798b'));
        expect(shares[2]).toEqual(hexToBytes('d9ad4e3bec2e1a7485698823abf05d36'));
        expect(shares[3]).toEqual(hexToBytes('0d8cf5f6ec337bc764d1866b5d07ca42'));
        expect(shares[4]).toEqual(hexToBytes('1aa7fe3199bc5092ef3816b074cabdf2'));

        const recoveredShareIndexes = [1, 2, 4];
        const recoveredShares = recoveredShareIndexes.map(index => shares[index]!);
        const recoveredSecret = recoverSecret(recoveredShareIndexes, recoveredShares);
        expect(recoveredSecret).toEqual(secret);
    });

    test('should split secret 2/7', () => {
        const rng = new FakeRandomNumberGenerator();
        const secret = hexToBytes(
            '204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a',
        );
        const shares = splitSecret(2, 7, secret, rng);
        expect(shares.length).toBe(7);

        expect(shares[0]).toEqual(
            hexToBytes('2dcd14c2252dc8489af3985030e74d5a48e8eff1478ab86e65b43869bf39d556'),
        );
        expect(shares[1]).toEqual(
            hexToBytes('a1dfdd798388aada635b9974472b4fc59a32ae520c42c9f6a0af70149b882487'),
        );
        expect(shares[2]).toEqual(
            hexToBytes('2ee99daf727c0c7773b89a18de64497ff7476dacd1015a45f482a893f7402cef'),
        );
        expect(shares[3]).toEqual(
            hexToBytes('a2fb5414d4d96ee58a109b3ca9a84be0259d2c0f9ac92bdd3199e0eed3f1dd3e'),
        );
        expect(shares[4]).toEqual(
            hexToBytes('2b851d188b8f5b3653659cc0f7fa45102dadf04b708767385cd803862fcb3c3f'),
        );
        expect(shares[5]).toEqual(
            hexToBytes('a797d4a32d2a39a4aacd9de48036478fff77b1e83b4f16a099c34bfb0b7acdee'),
        );
        expect(shares[6]).toEqual(
            hexToBytes('28a19475dcde9f09ba2e9e881979413592027216e60c8513cdee937c67b2c586'),
        );

        const recoveredShareIndexes = [3, 4];
        const recoveredShares = recoveredShareIndexes.map(index => shares[index]!);
        const recoveredSecret = recoverSecret(recoveredShareIndexes, recoveredShares);
        expect(recoveredSecret).toEqual(secret);
    });
});
