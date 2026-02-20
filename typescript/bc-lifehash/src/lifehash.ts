import { createHash } from 'node:crypto';

import { BitEnumerator } from './bit-enumerator.js';
import { CellGrid } from './cell-grid.js';
import { ChangeGrid } from './change-grid.js';
import { clamped, lerpFrom } from './color.js';
import { ColorGrid } from './color-grid.js';
import { FracGrid } from './frac-grid.js';
import { selectGradient } from './gradients.js';
import { selectPattern } from './patterns.js';
import { Version } from './version.js';

/** The result of generating a LifeHash image. */
export interface Image {
    /** Image width in pixels. */
    readonly width: number;
    /** Image height in pixels. */
    readonly height: number;
    /** Raw pixel data (RGB or RGBA depending on the `hasAlpha` parameter). */
    readonly colors: Uint8Array;
}

function sha256(data: Uint8Array): Uint8Array {
    return new Uint8Array(createHash('sha256').update(data).digest());
}

function toHex(bytes: Uint8Array): string {
    let hex = '';
    for (let i = 0; i < bytes.length; i++) {
        hex += bytes[i].toString(16).padStart(2, '0');
    }
    return hex;
}

function makeImage(
    width: number,
    height: number,
    floatColors: number[],
    moduleSize: number,
    hasAlpha: boolean,
): Image {
    if (moduleSize <= 0) {
        throw new RangeError('Invalid module size');
    }

    const scaledWidth = width * moduleSize;
    const scaledHeight = height * moduleSize;
    const resultComponents = hasAlpha ? 4 : 3;
    const scaledCapacity = scaledWidth * scaledHeight * resultComponents;

    const resultColors = new Uint8Array(scaledCapacity);

    // Match C++ loop order: outer loop uses scaledWidth, inner uses scaledHeight
    for (let targetY = 0; targetY < scaledWidth; targetY++) {
        for (let targetX = 0; targetX < scaledHeight; targetX++) {
            const sourceX = Math.trunc(targetX / moduleSize);
            const sourceY = Math.trunc(targetY / moduleSize);
            const sourceOffset = (sourceY * width + sourceX) * 3;

            const targetOffset =
                (targetY * scaledWidth + targetX) * resultComponents;

            resultColors[targetOffset] =
                (clamped(floatColors[sourceOffset]) * 255.0) | 0;
            resultColors[targetOffset + 1] =
                (clamped(floatColors[sourceOffset + 1]) * 255.0) | 0;
            resultColors[targetOffset + 2] =
                (clamped(floatColors[sourceOffset + 2]) * 255.0) | 0;
            if (hasAlpha) {
                resultColors[targetOffset + 3] = 255;
            }
        }
    }

    return { width: scaledWidth, height: scaledHeight, colors: resultColors };
}

/**
 * Generate a LifeHash image from a UTF-8 string.
 *
 * The string is converted to bytes and hashed with SHA-256 before
 * being used as the seed for the LifeHash algorithm.
 */
export function makeFromUtf8(
    s: string,
    version: Version,
    moduleSize: number,
    hasAlpha: boolean,
): Image {
    const encoder = new TextEncoder();
    return makeFromData(encoder.encode(s), version, moduleSize, hasAlpha);
}

/**
 * Generate a LifeHash image from arbitrary binary data.
 *
 * The data is hashed with SHA-256 before being used as the seed for
 * the LifeHash algorithm.
 */
export function makeFromData(
    data: Uint8Array,
    version: Version,
    moduleSize: number,
    hasAlpha: boolean,
): Image {
    const digest = sha256(data);
    return makeFromDigest(digest, version, moduleSize, hasAlpha);
}

/**
 * Generate a LifeHash image from a pre-computed 32-byte SHA-256 digest.
 *
 * @throws {RangeError} If the digest is not exactly 32 bytes.
 */
export function makeFromDigest(
    digest: Uint8Array,
    version: Version,
    moduleSize: number,
    hasAlpha: boolean,
): Image {
    if (digest.length !== 32) {
        throw new RangeError('Digest must be 32 bytes');
    }

    let length: number;
    let maxGenerations: number;
    switch (version) {
        case Version.Version1:
        case Version.Version2:
            length = 16;
            maxGenerations = 150;
            break;
        case Version.Detailed:
        case Version.Fiducial:
        case Version.GrayscaleFiducial:
            length = 32;
            maxGenerations = 300;
            break;
    }

    let currentCellGrid = new CellGrid(length, length);
    let nextCellGrid = new CellGrid(length, length);
    let currentChangeGrid = new ChangeGrid(length, length);
    let nextChangeGrid = new ChangeGrid(length, length);

    switch (version) {
        case Version.Version1:
            nextCellGrid.setData(digest);
            break;
        case Version.Version2: {
            const hashed = sha256(digest);
            nextCellGrid.setData(hashed);
            break;
        }
        case Version.Detailed:
        case Version.Fiducial:
        case Version.GrayscaleFiducial: {
            let digest1 = digest;
            if (version === Version.GrayscaleFiducial) {
                digest1 = sha256(digest1);
            }
            const digest2 = sha256(digest1);
            const digest3 = sha256(digest2);
            const digest4 = sha256(digest3);
            const digestFinal = new Uint8Array(
                digest1.length + digest2.length + digest3.length + digest4.length,
            );
            digestFinal.set(digest1, 0);
            digestFinal.set(digest2, digest1.length);
            digestFinal.set(digest3, digest1.length + digest2.length);
            digestFinal.set(
                digest4,
                digest1.length + digest2.length + digest3.length,
            );
            nextCellGrid.setData(digestFinal);
            break;
        }
    }

    nextChangeGrid.grid.setAll(true);

    const historySet = new Set<string>();
    const history: Uint8Array[] = [];

    while (history.length < maxGenerations) {
        // Swap current and next
        [currentCellGrid, nextCellGrid] = [nextCellGrid, currentCellGrid];
        [currentChangeGrid, nextChangeGrid] = [nextChangeGrid, currentChangeGrid];

        const data = currentCellGrid.data();
        const hash = sha256(data);
        const hashHex = toHex(hash);
        if (historySet.has(hashHex)) {
            break;
        }
        historySet.add(hashHex);
        history.push(data);

        currentCellGrid.nextGeneration(
            currentChangeGrid,
            nextCellGrid,
            nextChangeGrid,
        );
    }

    const fracGrid = new FracGrid(length, length);
    for (let i = 0; i < history.length; i++) {
        currentCellGrid.setData(history[i]);
        const frac = clamped(lerpFrom(0.0, history.length, i + 1));
        fracGrid.overlay(currentCellGrid, frac);
    }

    // Normalize the frac_grid to [0, 1] (except version1)
    if (version !== Version.Version1) {
        let minValue = Infinity;
        let maxValue = -Infinity;
        fracGrid.grid.forAll((x, y) => {
            const value = fracGrid.grid.getValue(x, y);
            if (value < minValue) minValue = value;
            if (value > maxValue) maxValue = value;
        });

        const w = fracGrid.grid.width;
        const h = fracGrid.grid.height;
        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                const value = fracGrid.grid.getValue(x, y);
                const normalized = lerpFrom(minValue, maxValue, value);
                fracGrid.grid.setValue(normalized, x, y);
            }
        }
    }

    const entropy = new BitEnumerator(new Uint8Array(digest));

    switch (version) {
        case Version.Detailed:
            entropy.next();
            break;
        case Version.Version2:
            entropy.nextUint2();
            break;
        default:
            break;
    }

    const gradient = selectGradient(entropy, version);
    const pattern = selectPattern(entropy, version);
    const colorGrid = new ColorGrid(fracGrid, gradient, pattern);

    return makeImage(
        colorGrid.grid.width,
        colorGrid.grid.height,
        colorGrid.colors(),
        moduleSize,
        hasAlpha,
    );
}
