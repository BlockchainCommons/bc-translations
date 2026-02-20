import { Color } from './color.js';
import { type ColorFunc } from './color-func.js';
import { FracGrid } from './frac-grid.js';
import { Grid } from './grid.js';
import { Pattern } from './patterns.js';

interface Transform {
    transpose: boolean;
    reflectX: boolean;
    reflectY: boolean;
}

export class ColorGrid {
    readonly grid: Grid<Color>;

    constructor(fracGrid: FracGrid, gradient: ColorFunc, pattern: Pattern) {
        const multiplier = pattern === Pattern.Fiducial ? 1 : 2;
        const targetWidth = fracGrid.grid.width * multiplier;
        const targetHeight = fracGrid.grid.height * multiplier;

        this.grid = new Grid(targetWidth, targetHeight, Color.BLACK);
        const maxX = targetWidth - 1;
        const maxY = targetHeight - 1;

        const transforms: Transform[] = (() => {
            switch (pattern) {
                case Pattern.Snowflake:
                    return [
                        { transpose: false, reflectX: false, reflectY: false },
                        { transpose: false, reflectX: true, reflectY: false },
                        { transpose: false, reflectX: false, reflectY: true },
                        { transpose: false, reflectX: true, reflectY: true },
                    ];
                case Pattern.Pinwheel:
                    return [
                        { transpose: false, reflectX: false, reflectY: false },
                        { transpose: true, reflectX: true, reflectY: false },
                        { transpose: true, reflectX: false, reflectY: true },
                        { transpose: false, reflectX: true, reflectY: true },
                    ];
                case Pattern.Fiducial:
                    return [
                        { transpose: false, reflectX: false, reflectY: false },
                    ];
            }
        })();

        const fracWidth = fracGrid.grid.width;
        const fracHeight = fracGrid.grid.height;
        for (let y = 0; y < fracHeight; y++) {
            for (let x = 0; x < fracWidth; x++) {
                const value = fracGrid.grid.getValue(x, y);
                const color = gradient(value);
                for (const t of transforms) {
                    let px = x;
                    let py = y;
                    if (t.transpose) {
                        [px, py] = [py, px];
                    }
                    if (t.reflectX) {
                        px = maxX - px;
                    }
                    if (t.reflectY) {
                        py = maxY - py;
                    }
                    this.grid.setValue(color, px, py);
                }
            }
        }
    }

    colors(): number[] {
        const result: number[] = new Array(this.grid.storage.length * 3);
        let idx = 0;
        for (const c of this.grid.storage) {
            result[idx++] = c.r;
            result[idx++] = c.g;
            result[idx++] = c.b;
        }
        return result;
    }
}
