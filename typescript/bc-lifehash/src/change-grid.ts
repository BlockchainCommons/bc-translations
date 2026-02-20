import { Grid } from './grid.js';

export class ChangeGrid {
    readonly grid: Grid<boolean>;

    constructor(width: number, height: number) {
        this.grid = new Grid(width, height, false);
    }

    setChanged(px: number, py: number): void {
        const width = this.grid.width;
        const height = this.grid.height;
        for (let oy = -1; oy <= 1; oy++) {
            for (let ox = -1; ox <= 1; ox++) {
                const nx = ((ox + px) % width + width) % width;
                const ny = ((oy + py) % height + height) % height;
                this.grid.setValue(true, nx, ny);
            }
        }
    }
}
