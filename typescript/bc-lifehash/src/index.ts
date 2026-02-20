/**
 * LifeHash visual hashing algorithm.
 *
 * `@bc/lifehash` generates beautiful, deterministic icons from arbitrary data
 * using Conway's Game of Life as a visual hash function. A SHA-256 digest seeds
 * a cellular automaton whose history is rendered as a colorful, symmetric image.
 *
 * @example
 * ```ts
 * import { makeFromUtf8, Version } from '@bc/lifehash';
 *
 * const image = makeFromUtf8('Hello', Version.Version2, 1, false);
 * // image.width === 32, image.height === 32
 * // image.colors contains RGB bytes (width * height * 3)
 * ```
 *
 * @packageDocumentation
 */

export { Version } from './version.js';
export type { Image } from './lifehash.js';
export { makeFromUtf8, makeFromData, makeFromDigest } from './lifehash.js';
