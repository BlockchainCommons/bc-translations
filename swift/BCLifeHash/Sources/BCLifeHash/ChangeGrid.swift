final class ChangeGrid {
    let grid: Grid<Bool>

    init(width: Int, height: Int) {
        self.grid = Grid(width: width, height: height, defaultValue: false)
    }

    func setChanged(_ px: Int, _ py: Int) {
        grid.forNeighborhood(px, py) { _, _, nx, ny in
            grid[nx, ny] = true
        }
    }
}
