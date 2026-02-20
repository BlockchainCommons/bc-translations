package com.blockchaincommons.bclifehash

class CellGrid(width: Int, height: Int) {
    val grid = Grid(width, height, false)

    private fun isAliveInNextGeneration(currentAlive: Boolean, neighborsCount: Int): Boolean =
        if (currentAlive) {
            neighborsCount == 2 || neighborsCount == 3
        } else {
            neighborsCount == 3
        }

    private fun countNeighbors(px: Int, py: Int): Int {
        var total = 0
        grid.forNeighborhood(px, py) { ox, oy, nx, ny ->
            if (ox == 0 && oy == 0) {
                return@forNeighborhood
            }
            if (grid.getValue(nx, ny)) {
                total += 1
            }
        }
        return total
    }

    fun data(): ByteArray {
        val aggregator = BitAggregator()
        grid.forAll { x, y ->
            aggregator.append(grid.getValue(x, y))
        }
        return aggregator.data()
    }

    fun setData(data: ByteArray) {
        val enumerator = BitEnumerator(data.copyOf())
        var index = 0
        enumerator.forAll { bit ->
            grid.storage[index] = bit
            index += 1
        }
        check(index == grid.storage.size) { "CellGrid data size mismatch" }
    }

    fun nextGeneration(
        currentChangeGrid: ChangeGrid,
        nextCellGrid: CellGrid,
        nextChangeGrid: ChangeGrid,
    ) {
        nextCellGrid.grid.setAll(false)
        nextChangeGrid.grid.setAll(false)

        val width = grid.width
        val height = grid.height
        for (y in 0 until height) {
            for (x in 0 until width) {
                val currentAlive = grid.getValue(x, y)
                if (currentChangeGrid.grid.getValue(x, y)) {
                    val neighborsCount = countNeighbors(x, y)
                    val nextAlive = isAliveInNextGeneration(currentAlive, neighborsCount)
                    if (nextAlive) {
                        nextCellGrid.grid.setValue(true, x, y)
                    }
                    if (currentAlive != nextAlive) {
                        nextChangeGrid.setChanged(x, y)
                    }
                } else {
                    nextCellGrid.grid.setValue(currentAlive, x, y)
                }
            }
        }
    }
}
