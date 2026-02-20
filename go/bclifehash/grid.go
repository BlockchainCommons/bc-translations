package bclifehash

// grid is a 2D grid with toroidal (wrapping) neighborhoods.
type grid[T any] struct {
	width   int
	height  int
	storage []T
}

func newGrid[T any](width, height int) grid[T] {
	return grid[T]{
		width:   width,
		height:  height,
		storage: make([]T, width*height),
	}
}

func (g *grid[T]) offset(x, y int) int {
	return y*g.width + x
}

func circularIndex(index, modulus int) int {
	return ((index % modulus) + modulus) % modulus
}

func (g *grid[T]) setAll(value T) {
	for i := range g.storage {
		g.storage[i] = value
	}
}

func (g *grid[T]) setValue(value T, x, y int) {
	g.storage[g.offset(x, y)] = value
}

func (g *grid[T]) getValue(x, y int) T {
	return g.storage[g.offset(x, y)]
}

func (g *grid[T]) forAll(f func(x, y int)) {
	for y := 0; y < g.height; y++ {
		for x := 0; x < g.width; x++ {
			f(x, y)
		}
	}
}

func (g *grid[T]) forNeighborhood(px, py int, f func(ox, oy, nx, ny int)) {
	for oy := -1; oy <= 1; oy++ {
		for ox := -1; ox <= 1; ox++ {
			nx := circularIndex(ox+px, g.width)
			ny := circularIndex(oy+py, g.height)
			f(ox, oy, nx, ny)
		}
	}
}
