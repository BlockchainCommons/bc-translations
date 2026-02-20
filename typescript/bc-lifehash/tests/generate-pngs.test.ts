import { mkdirSync, writeFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { describe, test } from 'vitest';
import { PNG } from 'pngjs';
import { Version, makeFromUtf8 } from '../src/index.js';

const __dirname = dirname(fileURLToPath(import.meta.url));

describe('generate PNGs', () => {
    const versions: [string, Version][] = [
        ['version1', Version.Version1],
        ['version2', Version.Version2],
        ['detailed', Version.Detailed],
        ['fiducial', Version.Fiducial],
        ['grayscale_fiducial', Version.GrayscaleFiducial],
    ];

    const outDir = resolve(__dirname, '..', 'out');

    for (const [name, version] of versions) {
        test(`generate ${name}`, () => {
            const dir = resolve(outDir, name);
            mkdirSync(dir, { recursive: true });

            for (let i = 0; i < 100; i++) {
                const input = i.toString();
                const image = makeFromUtf8(input, version, 1, false);

                const png = new PNG({
                    width: image.width,
                    height: image.height,
                });

                // Copy RGB data into the PNG's RGBA buffer
                for (let y = 0; y < image.height; y++) {
                    for (let x = 0; x < image.width; x++) {
                        const srcIdx = (y * image.width + x) * 3;
                        const dstIdx = (y * image.width + x) * 4;
                        png.data[dstIdx] = image.colors[srcIdx];
                        png.data[dstIdx + 1] = image.colors[srcIdx + 1];
                        png.data[dstIdx + 2] = image.colors[srcIdx + 2];
                        png.data[dstIdx + 3] = 255;
                    }
                }

                const buffer = PNG.sync.write(png);
                writeFileSync(resolve(dir, `${i}.png`), buffer);
            }
        });
    }
});
