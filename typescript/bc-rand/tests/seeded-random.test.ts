import { describe, test, expect } from 'vitest';
import {
    SeededRandomNumberGenerator,
    fakeRandomData,
    rngNextWithUpperBound,
    rngNextInRange,
} from '../src/index.js';

const TEST_SEED: [bigint, bigint, bigint, bigint] = [
    17295166580085024720n,
    422929670265678780n,
    5577237070365765850n,
    7953171132032326923n,
];

function hexToBytes(hex: string): Uint8Array {
    const bytes = new Uint8Array(hex.length / 2);
    for (let i = 0; i < hex.length; i += 2) {
        bytes[i / 2] = parseInt(hex.substring(i, i + 2), 16);
    }
    return bytes;
}

describe('SeededRandomNumberGenerator', () => {
    test('nextU64', () => {
        const rng = new SeededRandomNumberGenerator(TEST_SEED);
        expect(rng.nextU64()).toBe(1104683000648959614n);
    });

    test('next50', () => {
        const rng = new SeededRandomNumberGenerator(TEST_SEED);
        const expected: bigint[] = [
            1104683000648959614n,
            9817345228149227957n,
            546276821344993881n,
            15870950426333349563n,
            830653509032165567n,
            14772257893953840492n,
            3512633850838187726n,
            6358411077290857510n,
            7897285047238174514n,
            18314839336815726031n,
            4978716052961022367n,
            17373022694051233817n,
            663115362299242570n,
            9811238046242345451n,
            8113787839071393872n,
            16155047452816275860n,
            673245095821315645n,
            1610087492396736743n,
            1749670338128618977n,
            3927771759340679115n,
            9610589375631783853n,
            5311608497352460372n,
            11014490817524419548n,
            6320099928172676090n,
            12513554919020212402n,
            6823504187935853178n,
            1215405011954300226n,
            8109228150255944821n,
            4122548551796094879n,
            16544885818373129566n,
            5597102191057004591n,
            11690994260783567085n,
            9374498734039011409n,
            18246806104446739078n,
            2337407889179712900n,
            12608919248151905477n,
            7641631838640172886n,
            8421574250687361351n,
            8697189342072434208n,
            8766286633078002696n,
            14800090277885439654n,
            17865860059234099833n,
            4673315107448681522n,
            14288183874156623863n,
            7587575203648284614n,
            9109213819045273474n,
            11817665411945280786n,
            1745089530919138651n,
            5730370365819793488n,
            5496865518262805451n,
        ];
        for (const val of expected) {
            expect(rng.nextU64()).toBe(val);
        }
    });

    test('fakeRandomData', () => {
        const data = fakeRandomData(100);
        const expected = hexToBytes(
            '7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed' +
            '518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d354553' +
            '2daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a56' +
            '4e59b4e2',
        );
        expect(data).toEqual(expected);
    });

    test('nextWithUpperBound', () => {
        const rng = new SeededRandomNumberGenerator(TEST_SEED);
        expect(rngNextWithUpperBound(rng, 10000n, 32)).toBe(745n);
    });

    test('inRange', () => {
        const rng = new SeededRandomNumberGenerator(TEST_SEED);
        const values: bigint[] = [];
        for (let i = 0; i < 100; i++) {
            values.push(rngNextInRange(rng, 0n, 100n, 32));
        }
        const expected: bigint[] = [
            7n, 44n, 92n, 16n, 16n, 67n, 41n, 74n, 66n, 20n,
            18n, 6n, 62n, 34n, 4n, 69n, 99n, 19n, 0n, 85n,
            22n, 27n, 56n, 23n, 19n, 5n, 23n, 76n, 80n, 27n,
            74n, 69n, 17n, 92n, 31n, 32n, 55n, 36n, 49n, 23n,
            53n, 2n, 46n, 6n, 43n, 66n, 34n, 71n, 64n, 69n,
            25n, 14n, 17n, 23n, 32n, 6n, 23n, 65n, 35n, 11n,
            21n, 37n, 58n, 92n, 98n, 8n, 38n, 49n, 7n, 24n,
            24n, 71n, 37n, 63n, 91n, 21n, 11n, 66n, 52n, 54n,
            55n, 19n, 76n, 46n, 89n, 38n, 91n, 95n, 33n, 25n,
            4n, 30n, 66n, 51n, 5n, 91n, 62n, 27n, 92n, 39n,
        ];
        expect(values).toEqual(expected);
    });

    test('fillRandomData', () => {
        const rng1 = new SeededRandomNumberGenerator(TEST_SEED);
        const v1 = rng1.randomData(100);
        const rng2 = new SeededRandomNumberGenerator(TEST_SEED);
        const v2 = new Uint8Array(100);
        rng2.fillRandomData(v2);
        expect(v1).toEqual(v2);
    });
});
