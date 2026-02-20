package bclifehash

// fracGrid accumulates fractional values over generations.
type fracGrid struct {
	grid grid[float64]
}

func newFracGrid(width, height int) fracGrid {
	return fracGrid{grid: newGrid[float64](width, height)}
}

func (fg *fracGrid) overlay(cg *cellGrid, frac float64) {
	width := fg.grid.width
	height := fg.grid.height
	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			if cg.grid.getValue(x, y) {
				fg.grid.setValue(frac, x, y)
			}
		}
	}
}
