export class Grid<T> {
    readonly width: number;
    readonly height: number;
    readonly storage: T[];

    constructor(width: number, height: number, defaultValue: T) {
        this.width = width;
        this.height = height;
        this.storage = new Array<T>(width * height).fill(defaultValue);
    }

    private offset(x: number, y: number): number {
        return y * this.width + x;
    }

    private static circularIndex(index: number, modulus: number): number {
        return ((index % modulus) + modulus) % modulus;
    }

    setAll(value: T): void {
        this.storage.fill(value);
    }

    setValue(value: T, x: number, y: number): void {
        this.storage[this.offset(x, y)] = value;
    }

    getValue(x: number, y: number): T {
        return this.storage[this.offset(x, y)];
    }

    forAll(f: (x: number, y: number) => void): void {
        for (let y = 0; y < this.height; y++) {
            for (let x = 0; x < this.width; x++) {
                f(x, y);
            }
        }
    }

    forNeighborhood(
        px: number,
        py: number,
        f: (ox: number, oy: number, nx: number, ny: number) => void,
    ): void {
        for (let oy = -1; oy <= 1; oy++) {
            for (let ox = -1; ox <= 1; ox++) {
                const nx = Grid.circularIndex(ox + px, this.width);
                const ny = Grid.circularIndex(oy + py, this.height);
                f(ox, oy, nx, ny);
            }
        }
    }
}
