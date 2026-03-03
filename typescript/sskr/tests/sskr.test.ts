import { describe, expect, test } from 'vitest';
import {
    createFakeRandomNumberGenerator,
    rngNextInClosedRange,
} from '@bc/rand';
import type { RandomNumberGenerator } from '@bc/rand';

import {
    Secret,
    GroupSpec,
    Spec,
    sskrGenerate,
    sskrGenerateUsing,
    sskrCombine,
    METADATA_SIZE_BYTES,
    MIN_SECRET_LEN,
    MAX_SECRET_LEN,
    MAX_GROUPS_COUNT,
    MAX_SHARE_COUNT,
} from '../src/index.js';
import { FakeRandomNumberGenerator, hexToBytes } from './test-helpers.js';

describe('sskr', () => {
    test('split 3/5', () => {
        const rng = new FakeRandomNumberGenerator();
        const secret = Secret.create(
            hexToBytes('0ff784df000c4380a5ed683f7e6e3dcf'),
        );
        const group = GroupSpec.create(3, 5);
        const spec = Spec.create(1, [group]);
        const shares = sskrGenerateUsing(spec, secret, rng);
        const flattenedShares = shares.flat();
        expect(flattenedShares.length).toBe(5);
        for (const share of flattenedShares) {
            expect(share.length).toBe(METADATA_SIZE_BYTES + secret.length);
        }

        const recoveredShareIndexes = [1, 2, 4];
        const recoveredShares = recoveredShareIndexes.map(i => flattenedShares[i]!);
        const recoveredSecret = sskrCombine(recoveredShares);
        expect(recoveredSecret.equals(secret)).toBe(true);
    });

    test('split 2/7', () => {
        const rng = new FakeRandomNumberGenerator();
        const secret = Secret.create(
            hexToBytes(
                '204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a',
            ),
        );
        const group = GroupSpec.create(2, 7);
        const spec = Spec.create(1, [group]);
        const shares = sskrGenerateUsing(spec, secret, rng);
        expect(shares.length).toBe(1);
        expect(shares[0]!.length).toBe(7);
        const flattenedShares = shares.flat();
        expect(flattenedShares.length).toBe(7);
        for (const share of flattenedShares) {
            expect(share.length).toBe(METADATA_SIZE_BYTES + secret.length);
        }

        const recoveredShareIndexes = [3, 4];
        const recoveredShares = recoveredShareIndexes.map(i => flattenedShares[i]!);
        const recoveredSecret = sskrCombine(recoveredShares);
        expect(recoveredSecret.equals(secret)).toBe(true);
    });

    test('split 2-3/2-3', () => {
        const rng = new FakeRandomNumberGenerator();
        const secret = Secret.create(
            hexToBytes(
                '204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a',
            ),
        );
        const group1 = GroupSpec.create(2, 3);
        const group2 = GroupSpec.create(2, 3);
        const spec = Spec.create(2, [group1, group2]);
        const shares = sskrGenerateUsing(spec, secret, rng);
        expect(shares.length).toBe(2);
        expect(shares[0]!.length).toBe(3);
        expect(shares[1]!.length).toBe(3);
        const flattenedShares = shares.flat();
        expect(flattenedShares.length).toBe(6);
        for (const share of flattenedShares) {
            expect(share.length).toBe(METADATA_SIZE_BYTES + secret.length);
        }

        const recoveredShareIndexes = [0, 1, 3, 5];
        const recoveredShares = recoveredShareIndexes.map(i => flattenedShares[i]!);
        const recoveredSecret = sskrCombine(recoveredShares);
        expect(recoveredSecret.equals(secret)).toBe(true);
    });

    function fisherYatesShuffle<T>(
        arr: T[],
        rng: RandomNumberGenerator,
    ): void {
        let i = arr.length;
        while (i > 1) {
            i -= 1;
            const j = Number(rngNextInClosedRange(rng, 0n, BigInt(i)));
            const tmp = arr[i]!;
            arr[i] = arr[j]!;
            arr[j] = tmp;
        }
    }

    test('shuffle', () => {
        const rng = createFakeRandomNumberGenerator();
        const v: number[] = [];
        for (let i = 0; i < 100; i++) {
            v.push(i);
        }
        fisherYatesShuffle(v, rng);
        expect(v.length).toBe(100);
        expect(v).toEqual([
            79, 70, 40, 53, 25, 30, 31, 88, 10, 1, 45, 54, 81, 58, 55, 59,
            69, 78, 65, 47, 75, 61, 0, 72, 20, 9, 80, 13, 73, 11, 60, 56,
            19, 42, 33, 12, 36, 38, 6, 35, 68, 77, 50, 18, 97, 49, 98, 85,
            89, 91, 15, 71, 99, 67, 84, 23, 64, 14, 57, 48, 62, 29, 28, 94,
            44, 8, 66, 34, 43, 21, 63, 16, 92, 95, 27, 51, 26, 86, 22, 41,
            93, 82, 7, 87, 74, 37, 46, 3, 96, 24, 90, 39, 32, 17, 76, 4,
            83, 2, 52, 5,
        ]);
    });

    interface RecoverSpec {
        secret: Secret;
        spec: Spec;
        shares: Uint8Array[][];
        recoveredGroupIndexes: number[];
        recoveredMemberIndexes: number[][];
        recoveredShares: Uint8Array[];
    }

    function createRecoverSpec(
        secret: Secret,
        spec: Spec,
        shares: Uint8Array[][],
        rng: RandomNumberGenerator,
    ): RecoverSpec {
        const groupIndexes: number[] = [];
        for (let i = 0; i < spec.groupCount; i++) {
            groupIndexes.push(i);
        }
        fisherYatesShuffle(groupIndexes, rng);
        const recoveredGroupIndexes = groupIndexes.slice(0, spec.groupThreshold);

        const recoveredMemberIndexes: number[][] = [];
        for (const groupIndex of recoveredGroupIndexes) {
            const group = spec.groups[groupIndex]!;
            const memberIndexes: number[] = [];
            for (let i = 0; i < group.memberCount; i++) {
                memberIndexes.push(i);
            }
            fisherYatesShuffle(memberIndexes, rng);
            recoveredMemberIndexes.push(
                memberIndexes.slice(0, group.memberThreshold),
            );
        }

        const recoveredShares: Uint8Array[] = [];
        for (let i = 0; i < recoveredGroupIndexes.length; i++) {
            const groupShares = shares[recoveredGroupIndexes[i]!]!;
            for (const memberIndex of recoveredMemberIndexes[i]!) {
                recoveredShares.push(groupShares[memberIndex]!);
            }
        }
        fisherYatesShuffle(recoveredShares, rng);

        return {
            secret,
            spec,
            shares,
            recoveredGroupIndexes,
            recoveredMemberIndexes,
            recoveredShares,
        };
    }

    function recoverFromSpec(rs: RecoverSpec): void {
        const recoveredSecret = sskrCombine(rs.recoveredShares);
        expect(recoveredSecret.equals(rs.secret)).toBe(true);
    }

    function oneFuzzTest(rng: RandomNumberGenerator): void {
        const secretLen =
            Number(
                rngNextInClosedRange(
                    rng,
                    BigInt(MIN_SECRET_LEN),
                    BigInt(MAX_SECRET_LEN),
                ),
            ) & ~1;
        const secret = Secret.create(rng.randomData(secretLen));
        const groupCount = Number(
            rngNextInClosedRange(rng, 1n, BigInt(MAX_GROUPS_COUNT)),
        );
        const groupSpecs: GroupSpec[] = [];
        for (let i = 0; i < groupCount; i++) {
            const memberCount = Number(
                rngNextInClosedRange(rng, 1n, BigInt(MAX_SHARE_COUNT)),
            );
            const memberThreshold = Number(
                rngNextInClosedRange(rng, 1n, BigInt(memberCount)),
            );
            groupSpecs.push(GroupSpec.create(memberThreshold, memberCount));
        }
        const groupThreshold = Number(
            rngNextInClosedRange(rng, 1n, BigInt(groupCount)),
        );
        const spec = Spec.create(groupThreshold, groupSpecs);
        const shares = sskrGenerateUsing(spec, secret, rng);

        const rs = createRecoverSpec(secret, spec, shares, rng);
        recoverFromSpec(rs);
    }

    test('fuzz test', () => {
        const rng = createFakeRandomNumberGenerator();
        for (let i = 0; i < 100; i++) {
            oneFuzzTest(rng);
        }
    });

    test('example encode', () => {
        const secretString = new TextEncoder().encode('my secret belongs to me.');
        const secret = Secret.create(secretString);

        const group1 = GroupSpec.create(2, 3);
        const group2 = GroupSpec.create(3, 5);
        const spec = Spec.create(2, [group1, group2]);

        const shares = sskrGenerate(spec, secret);

        expect(shares.length).toBe(2);
        expect(shares[0]!.length).toBe(3);
        expect(shares[1]!.length).toBe(5);

        const recoveredShares = [
            shares[0]![0]!,
            shares[0]![2]!,
            shares[1]![0]!,
            shares[1]![1]!,
            shares[1]![4]!,
        ];

        const recoveredSecret = sskrCombine(recoveredShares);
        expect(recoveredSecret.equals(secret)).toBe(true);
    });

    test('example encode 3', () => {
        const TEXT = 'my secret belongs to me.';

        function roundtrip(m: number, n: number): Secret {
            const secret = Secret.create(new TextEncoder().encode(TEXT));
            const spec = Spec.create(1, [GroupSpec.create(m, n)]);
            const shares = sskrGenerate(spec, secret);
            return sskrCombine(shares.flat());
        }

        // Good, uses a 2/3 group
        {
            const result = roundtrip(2, 3);
            expect(new TextDecoder().decode(result.data)).toBe(TEXT);
        }

        // Still ok, uses a 1/1 group
        {
            const result = roundtrip(1, 1);
            expect(new TextDecoder().decode(result.data)).toBe(TEXT);
        }

        // Fixed, uses a 1/3 group
        {
            const result = roundtrip(1, 3);
            expect(new TextDecoder().decode(result.data)).toBe(TEXT);
        }
    });

    test('example encode 4', () => {
        const TEXT = 'my secret belongs to me.';
        const secret = Secret.create(new TextEncoder().encode(TEXT));
        const spec = Spec.create(1, [
            GroupSpec.create(2, 3),
            GroupSpec.create(2, 3),
        ]);
        const groupedShares = sskrGenerate(spec, secret);
        const flattenedShares = groupedShares.flat();
        // The group threshold is 1, but we're providing an additional share
        // from the second group. This was previously causing an error,
        // because the second group could not be decoded. The correct
        // behavior is to ignore any group's shares that cannot be decoded.
        const recoveredShareIndexes = [0, 1, 3];
        const recoveredShares = recoveredShareIndexes.map(
            i => flattenedShares[i]!,
        );
        const recoveredSecret = sskrCombine(recoveredShares);
        expect(new TextDecoder().decode(recoveredSecret.data)).toBe(TEXT);
    });
});
