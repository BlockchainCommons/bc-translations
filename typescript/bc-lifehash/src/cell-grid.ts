import { BitAggregator, BitEnumerator } from './bit-enumerator.js';
import { ChangeGrid } from './change-grid.js';
import { Grid } from './grid.js';

export class CellGrid {
    readonly grid: Grid<boolean>;

    constructor(width: number, height: number) {
        this.grid = new Grid(width, height, false);
    }

    private static isAliveInNextGeneration(
        currentAlive: boolean,
        neighborsCount: number,
    ): boolean {
        if (currentAlive) {
            return neighborsCount === 2 || neighborsCount === 3;
        }
        return neighborsCount === 3;
    }

    private countNeighbors(px: number, py: number): number {
        let total = 0;
        this.grid.forNeighborhood(px, py, (ox, oy, nx, ny) => {
            if (ox === 0 && oy === 0) return;
            if (this.grid.getValue(nx, ny)) {
                total += 1;
            }
        });
        return total;
    }

    data(): Uint8Array {
        const a = new BitAggregator();
        this.grid.forAll((x, y) => {
            a.append(this.grid.getValue(x, y));
        });
        return a.data();
    }

    setData(data: Uint8Array): void {
        const e = new BitEnumerator(data);
        let i = 0;
        e.forAll((b) => {
            this.grid.storage[i] = b;
            i += 1;
        });
    }

    nextGeneration(
        currentChangeGrid: ChangeGrid,
        nextCellGrid: CellGrid,
        nextChangeGrid: ChangeGrid,
    ): void {
        nextCellGrid.grid.setAll(false);
        nextChangeGrid.grid.setAll(false);
        const width = this.grid.width;
        const height = this.grid.height;
        for (let y = 0; y < height; y++) {
            for (let x = 0; x < width; x++) {
                const currentAlive = this.grid.getValue(x, y);
                if (currentChangeGrid.grid.getValue(x, y)) {
                    const neighborsCount = this.countNeighbors(x, y);
                    const nextAlive = CellGrid.isAliveInNextGeneration(
                        currentAlive,
                        neighborsCount,
                    );
                    if (nextAlive) {
                        nextCellGrid.grid.setValue(true, x, y);
                    }
                    if (currentAlive !== nextAlive) {
                        nextChangeGrid.setChanged(x, y);
                    }
                } else {
                    nextCellGrid.grid.setValue(currentAlive, x, y);
                }
            }
        }
    }
}
