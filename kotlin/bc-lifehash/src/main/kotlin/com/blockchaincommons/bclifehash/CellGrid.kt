package com.blockchaincommons.bclifehash

/**
 * A boolean grid representing alive/dead cells for Conway's Game of Life.
 */
internal class CellGrid(width: Int, height: Int) {
    val grid = Grid(width, height, false)

    private fun isAliveInNextGeneration(currentAlive: Boolean, neighborsCount: Int): Boolean =
        if (currentAlive) {
            neighborsCount == 2 || neighborsCount == 3
        } else {
            neighborsCount == 3
        }

    private fun countNeighbors(px: Int, py: Int): Int {
        var total = 0
        grid.forNeighborhood(px, py) { offsetX, offsetY, neighborX, neighborY ->
            if (offsetX == 0 && offsetY == 0) {
                return@forNeighborhood
            }
            if (grid.getValue(neighborX, neighborY)) {
                total += 1
            }
        }
        return total
    }

    fun toByteArray(): ByteArray {
        val aggregator = BitAggregator()
        grid.forEach { x, y ->
            aggregator.append(grid.getValue(x, y))
        }
        return aggregator.toByteArray()
    }

    fun loadFrom(data: ByteArray) {
        val enumerator = BitEnumerator(data.copyOf())
        var index = 0
        enumerator.forEach { bit ->
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
        nextCellGrid.grid.fill(false)
        nextChangeGrid.grid.fill(false)

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
