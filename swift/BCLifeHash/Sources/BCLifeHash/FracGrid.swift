final class FracGrid {
    let grid: Grid<Double>

    init(width: Int, height: Int) {
        self.grid = Grid(width: width, height: height, defaultValue: 0.0)
    }

    func overlay(_ cellGrid: CellGrid, _ frac: Double) {
        let width = grid.width
        let height = grid.height
        for y in 0..<height {
            for x in 0..<width {
                if cellGrid.grid.getValue(x, y) {
                    grid.setValue(frac, x, y)
                }
            }
        }
    }
}
