package com.blockchaincommons.bclifehash

/**
 * Applies a color gradient and symmetry pattern to a [FracGrid] to produce the
 * final colored image data.
 */
internal class ColorGrid(fracGrid: FracGrid, gradient: ColorFunc, pattern: Pattern) {
    val grid: Grid<Color>

    init {
        val multiplier = if (pattern == Pattern.Fiducial) 1 else 2
        val targetWidth = fracGrid.grid.width * multiplier
        val targetHeight = fracGrid.grid.height * multiplier

        val generatedGrid = Grid(targetWidth, targetHeight, Color.BLACK)
        val maxX = targetWidth - 1
        val maxY = targetHeight - 1

        val transforms: List<Transform> = when (pattern) {
            Pattern.Snowflake -> listOf(
                Transform(transpose = false, reflectX = false, reflectY = false),
                Transform(transpose = false, reflectX = true, reflectY = false),
                Transform(transpose = false, reflectX = false, reflectY = true),
                Transform(transpose = false, reflectX = true, reflectY = true),
            )

            Pattern.Pinwheel -> listOf(
                Transform(transpose = false, reflectX = false, reflectY = false),
                Transform(transpose = true, reflectX = true, reflectY = false),
                Transform(transpose = true, reflectX = false, reflectY = true),
                Transform(transpose = false, reflectX = true, reflectY = true),
            )

            Pattern.Fiducial -> listOf(
                Transform(transpose = false, reflectX = false, reflectY = false),
            )
        }

        val fracWidth = fracGrid.grid.width
        val fracHeight = fracGrid.grid.height
        for (y in 0 until fracHeight) {
            for (x in 0 until fracWidth) {
                val value = fracGrid.grid.getValue(x, y)
                val color = gradient(value)
                for (transform in transforms) {
                    var px = x
                    var py = y
                    if (transform.transpose) {
                        val temp = px
                        px = py
                        py = temp
                    }
                    if (transform.reflectX) {
                        px = maxX - px
                    }
                    if (transform.reflectY) {
                        py = maxY - py
                    }
                    generatedGrid.setValue(color, px, py)
                }
            }
        }

        grid = generatedGrid
    }

    fun colors(): DoubleArray {
        val result = DoubleArray(grid.storage.size * 3)
        var index = 0
        for (color in grid.storage) {
            result[index] = color.r
            result[index + 1] = color.g
            result[index + 2] = color.b
            index += 3
        }
        return result
    }
}

private data class Transform(
    val transpose: Boolean,
    val reflectX: Boolean,
    val reflectY: Boolean,
)
