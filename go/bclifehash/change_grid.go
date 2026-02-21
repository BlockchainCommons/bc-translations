package bclifehash

// changeGrid tracks which cells (and their neighborhoods) have changed.
type changeGrid struct {
	grid grid[bool]
}

func newChangeGrid(width, height int) changeGrid {
	return changeGrid{grid: newGrid[bool](width, height)}
}

func (cg *changeGrid) setChanged(px, py int) {
	width := cg.grid.width
	height := cg.grid.height
	for oy := -1; oy <= 1; oy++ {
		for ox := -1; ox <= 1; ox++ {
			nx := circularIndex(ox+px, width)
			ny := circularIndex(oy+py, height)
			cg.grid.setValue(true, nx, ny)
		}
	}
}
