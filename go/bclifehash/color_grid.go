package bclifehash

type transform struct {
	transpose bool
	reflectX  bool
	reflectY  bool
}

type colorGrid struct {
	grid grid[color]
}

func newColorGrid(fg *fracGrid, gradient colorFunc, pat pattern) colorGrid {
	multiplier := 2
	if pat == patternFiducial {
		multiplier = 1
	}
	targetWidth := fg.grid.width * multiplier
	targetHeight := fg.grid.height * multiplier

	g := newGrid[color](targetWidth, targetHeight)
	maxX := targetWidth - 1
	maxY := targetHeight - 1

	var transforms []transform
	switch pat {
	case patternSnowflake:
		transforms = []transform{
			{transpose: false, reflectX: false, reflectY: false},
			{transpose: false, reflectX: true, reflectY: false},
			{transpose: false, reflectX: false, reflectY: true},
			{transpose: false, reflectX: true, reflectY: true},
		}
	case patternPinwheel:
		transforms = []transform{
			{transpose: false, reflectX: false, reflectY: false},
			{transpose: true, reflectX: true, reflectY: false},
			{transpose: true, reflectX: false, reflectY: true},
			{transpose: false, reflectX: true, reflectY: true},
		}
	case patternFiducial:
		transforms = []transform{
			{transpose: false, reflectX: false, reflectY: false},
		}
	}

	fracWidth := fg.grid.width
	fracHeight := fg.grid.height
	for y := 0; y < fracHeight; y++ {
		for x := 0; x < fracWidth; x++ {
			value := fg.grid.getValue(x, y)
			c := gradient(value)
			for _, t := range transforms {
				px := x
				py := y
				if t.transpose {
					px, py = py, px
				}
				if t.reflectX {
					px = maxX - px
				}
				if t.reflectY {
					py = maxY - py
				}
				g.setValue(c, px, py)
			}
		}
	}

	return colorGrid{grid: g}
}

func (cg *colorGrid) colors() []float64 {
	result := make([]float64, 0, len(cg.grid.storage)*3)
	for _, c := range cg.grid.storage {
		result = append(result, c.r, c.g, c.b)
	}
	return result
}
