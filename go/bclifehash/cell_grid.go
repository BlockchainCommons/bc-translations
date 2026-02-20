package bclifehash

// cellGrid is a 2D grid of boolean cells for Conway's Game of Life.
type cellGrid struct {
	grid grid[bool]
}

func newCellGrid(width, height int) cellGrid {
	return cellGrid{grid: newGrid[bool](width, height)}
}

func isAliveInNextGeneration(currentAlive bool, neighborsCount int) bool {
	if currentAlive {
		return neighborsCount == 2 || neighborsCount == 3
	}
	return neighborsCount == 3
}

func (cg *cellGrid) countNeighbors(px, py int) int {
	total := 0
	cg.grid.forNeighborhood(px, py, func(ox, oy, nx, ny int) {
		if ox == 0 && oy == 0 {
			return
		}
		if cg.grid.getValue(nx, ny) {
			total++
		}
	})
	return total
}

func (cg *cellGrid) data() []byte {
	a := newBitAggregator()
	cg.grid.forAll(func(x, y int) {
		a.append(cg.grid.getValue(x, y))
	})
	return a.bytes()
}

func (cg *cellGrid) setData(data []byte) {
	e := newBitEnumerator(data)
	i := 0
	e.forAll(func(b bool) {
		cg.grid.storage[i] = b
		i++
	})
}

func (cg *cellGrid) nextGeneration(
	currentChangeGrid *changeGrid,
	nextCellGrid *cellGrid,
	nextChangeGrid *changeGrid,
) {
	nextCellGrid.grid.setAll(false)
	nextChangeGrid.grid.setAll(false)
	width := cg.grid.width
	height := cg.grid.height
	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			currentAlive := cg.grid.getValue(x, y)
			if currentChangeGrid.grid.getValue(x, y) {
				neighborsCount := cg.countNeighbors(x, y)
				nextAlive := isAliveInNextGeneration(currentAlive, neighborsCount)
				if nextAlive {
					nextCellGrid.grid.setValue(true, x, y)
				}
				if currentAlive != nextAlive {
					nextChangeGrid.setChanged(x, y)
				}
			} else {
				nextCellGrid.grid.setValue(currentAlive, x, y)
			}
		}
	}
}
