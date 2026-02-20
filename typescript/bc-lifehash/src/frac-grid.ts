import { CellGrid } from './cell-grid.js';
import { Grid } from './grid.js';

export class FracGrid {
    readonly grid: Grid<number>;

    constructor(width: number, height: number) {
        this.grid = new Grid(width, height, 0.0);
    }

    overlay(cellGrid: CellGrid, frac: number): void {
        const width = this.grid.width;
        const height = this.grid.height;
        for (let y = 0; y < height; y++) {
            for (let x = 0; x < width; x++) {
                if (cellGrid.grid.getValue(x, y)) {
                    this.grid.setValue(frac, x, y);
                }
            }
        }
    }
}
