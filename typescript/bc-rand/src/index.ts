/**
 * Blockchain Commons Random Number Utilities.
 *
 * `@bc/rand` exposes a uniform API for the random number primitives used
 * in higher-level Blockchain Commons projects, including a cryptographically
 * strong random number generator ({@link SecureRandomNumberGenerator}) and a
 * deterministic random number generator ({@link SeededRandomNumberGenerator}).
 *
 * Both generators implement the {@link RandomNumberGenerator} interface.
 *
 * The package also includes several convenience functions for generating secure
 * and deterministic random numbers.
 *
 * @packageDocumentation
 */

export type { RandomNumberGenerator, BitWidth } from './random-number-generator.js';
export {
    rngRandomData,
    rngFillRandomData,
    rngNextWithUpperBound,
    rngNextInRange,
    rngNextInClosedRange,
    rngRandomArray,
    rngRandomBool,
    rngRandomU32,
} from './random-number-generator.js';

export {
    SecureRandomNumberGenerator,
    secureRandomData,
    secureFillRandomData,
} from './secure-random.js';

export {
    SeededRandomNumberGenerator,
    createFakeRandomNumberGenerator,
    fakeRandomData,
} from './seeded-random.js';
