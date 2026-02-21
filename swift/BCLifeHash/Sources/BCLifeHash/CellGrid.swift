final class CellGrid {
    let grid: Grid<Bool>

    init(width: Int, height: Int) {
        self.grid = Grid(width: width, height: height, defaultValue: false)
    }

    private static func isAliveInNextGeneration(_ currentAlive: Bool, _ neighborsCount: Int) -> Bool {
        if currentAlive {
            return neighborsCount == 2 || neighborsCount == 3
        }
        return neighborsCount == 3
    }

    private func countNeighbors(_ px: Int, _ py: Int) -> Int {
        var total = 0
        grid.forNeighborhood(px, py) { ox, oy, nx, ny in
            if ox == 0 && oy == 0 {
                return
            }
            if grid[nx, ny] {
                total += 1
            }
        }
        return total
    }

    func data() -> [UInt8] {
        let aggregator = BitAggregator()
        grid.forEachCell { x, y in
            aggregator.append(grid[x, y])
        }
        return aggregator.valueData()
    }

    func setData(_ data: [UInt8]) {
        precondition(grid.width * grid.height == data.count * 8)
        let enumerator = BitEnumerator(data: data)
        var i = 0
        enumerator.forAll { bit in
            grid.storage[i] = bit
            i += 1
        }
        precondition(i == grid.storage.count)
    }

    func nextGeneration(
        currentChangeGrid: ChangeGrid,
        nextCellGrid: CellGrid,
        nextChangeGrid: ChangeGrid
    ) {
        nextCellGrid.grid.setAll(false)
        nextChangeGrid.grid.setAll(false)

        let width = grid.width
        let height = grid.height

        for y in 0..<height {
            for x in 0..<width {
                let currentAlive = grid[x, y]
                if currentChangeGrid.grid[x, y] {
                    let neighborsCount = countNeighbors(x, y)
                    let nextAlive = Self.isAliveInNextGeneration(currentAlive, neighborsCount)
                    if nextAlive {
                        nextCellGrid.grid[x, y] = true
                    }
                    if currentAlive != nextAlive {
                        nextChangeGrid.setChanged(x, y)
                    }
                } else {
                    nextCellGrid.grid[x, y] = currentAlive
                }
            }
        }
    }
}
