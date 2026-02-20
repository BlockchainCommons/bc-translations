final class ChangeGrid {
    let grid: Grid<Bool>

    init(width: Int, height: Int) {
        self.grid = Grid(width: width, height: height, defaultValue: false)
    }

    func setChanged(_ px: Int, _ py: Int) {
        let width = grid.width
        let height = grid.height
        for oy in -1...1 {
            for ox in -1...1 {
                let nx = (((ox + px) % width) + width) % width
                let ny = (((oy + py) % height) + height) % height
                grid.setValue(true, nx, ny)
            }
        }
    }
}
