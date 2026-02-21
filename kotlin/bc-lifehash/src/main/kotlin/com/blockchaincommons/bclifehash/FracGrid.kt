package com.blockchaincommons.bclifehash

/**
 * A grid of fractional values recording how late in the Game of Life
 * simulation each cell was last alive.
 */
internal class FracGrid(width: Int, height: Int) {
    val grid = Grid(width, height, 0.0)

    fun overlay(cellGrid: CellGrid, frac: Double) {
        val width = grid.width
        val height = grid.height
        for (y in 0 until height) {
            for (x in 0 until width) {
                if (cellGrid.grid.getValue(x, y)) {
                    grid.setValue(frac, x, y)
                }
            }
        }
    }
}
