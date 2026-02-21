/**
 * LifeHash visual hashing based on Conway's Game of Life.
 *
 * The output is deterministic for the same input bytes and rendering parameters.
 * Five rendering variants are available through [Version].
 */
package com.blockchaincommons.bclifehash

import java.security.MessageDigest

/**
 * The rendering variant for a LifeHash image.
 *
 * Each version produces a different grid size, generation count, and color
 * palette selection strategy.
 */
enum class Version {
    Version1,
    Version2,
    Detailed,
    Fiducial,
    GrayscaleFiducial,
}

/**
 * A LifeHash image stored as a flat array of RGB or RGBA bytes.
 *
 * @property width  Image width in pixels.
 * @property height Image height in pixels.
 * @property colors Raw pixel data in row-major order (3 bytes per pixel for
 *                  RGB, 4 for RGBA).
 */
class Image(
    val width: Int,
    val height: Int,
    val colors: ByteArray,
) {
    override fun equals(other: Any?): Boolean =
        other is Image &&
            width == other.width &&
            height == other.height &&
            colors.contentEquals(other.colors)

    override fun hashCode(): Int {
        var result = width
        result = 31 * result + height
        result = 31 * result + colors.contentHashCode()
        return result
    }

    override fun toString(): String = "Image(width=$width, height=$height, bytes=${colors.size})"
}

private class ByteArrayKey(
    private val bytes: ByteArray,
) {
    override fun equals(other: Any?): Boolean =
        other is ByteArrayKey && bytes.contentEquals(other.bytes)

    override fun hashCode(): Int = bytes.contentHashCode()
}

private fun sha256(data: ByteArray): ByteArray =
    MessageDigest
        .getInstance("SHA-256")
        .digest(data)

private fun renderImage(
    width: Int,
    height: Int,
    floatColors: DoubleArray,
    moduleSize: Int,
    hasAlpha: Boolean,
): Image {
    require(moduleSize > 0) { "Invalid module size" }

    val scaledWidth = width * moduleSize
    val scaledHeight = height * moduleSize
    val resultComponents = if (hasAlpha) 4 else 3
    val scaledCapacity = scaledWidth * scaledHeight * resultComponents

    val resultColors = ByteArray(scaledCapacity)

    // Match C++ loop order exactly.
    for (targetY in 0 until scaledWidth) {
        for (targetX in 0 until scaledHeight) {
            val sourceX = targetX / moduleSize
            val sourceY = targetY / moduleSize
            val sourceOffset = (sourceY * width + sourceX) * 3

            val targetOffset = (targetY * scaledWidth + targetX) * resultComponents

            resultColors[targetOffset] = (clamped(floatColors[sourceOffset]) * 255.0).toInt().toByte()
            resultColors[targetOffset + 1] = (clamped(floatColors[sourceOffset + 1]) * 255.0).toInt().toByte()
            resultColors[targetOffset + 2] = (clamped(floatColors[sourceOffset + 2]) * 255.0).toInt().toByte()
            if (hasAlpha) {
                resultColors[targetOffset + 3] = 0xFF.toByte()
            }
        }
    }

    return Image(
        width = scaledWidth,
        height = scaledHeight,
        colors = resultColors,
    )
}

/**
 * Entry point for generating LifeHash images.
 *
 * Three factory methods are provided depending on whether the input is a
 * UTF-8 string, arbitrary binary data, or a pre-computed 32-byte SHA-256
 * digest.
 */
object LifeHash {

    /**
     * Generates a LifeHash image from a UTF-8 string.
     *
     * The string is encoded to UTF-8 bytes, SHA-256 hashed, and then
     * rendered.
     *
     * @param input      The input string.
     * @param version    The rendering variant.
     * @param moduleSize Pixel scaling factor (must be > 0).
     * @param hasAlpha   Whether to include an alpha channel in the output.
     * @return The rendered [Image].
     */
    fun fromUtf8(
        input: String,
        version: Version,
        moduleSize: Int,
        hasAlpha: Boolean,
    ): Image = fromData(input.toByteArray(Charsets.UTF_8), version, moduleSize, hasAlpha)

    /**
     * Generates a LifeHash image from arbitrary binary data.
     *
     * The data is SHA-256 hashed and then rendered.
     *
     * @param data       The input bytes.
     * @param version    The rendering variant.
     * @param moduleSize Pixel scaling factor (must be > 0).
     * @param hasAlpha   Whether to include an alpha channel in the output.
     * @return The rendered [Image].
     */
    fun fromData(
        data: ByteArray,
        version: Version,
        moduleSize: Int,
        hasAlpha: Boolean,
    ): Image {
        val digest = sha256(data)
        return fromDigest(digest, version, moduleSize, hasAlpha)
    }

    /**
     * Generates a LifeHash image from a pre-computed 32-byte SHA-256 digest.
     *
     * @param digest     A 32-byte SHA-256 digest.
     * @param version    The rendering variant.
     * @param moduleSize Pixel scaling factor (must be > 0).
     * @param hasAlpha   Whether to include an alpha channel in the output.
     * @return The rendered [Image].
     * @throws IllegalArgumentException if [digest] is not exactly 32 bytes.
     */
    fun fromDigest(
        digest: ByteArray,
        version: Version,
        moduleSize: Int,
        hasAlpha: Boolean,
    ): Image {
        require(digest.size == 32) { "Digest must be 32 bytes" }

        val (length, maxGenerations) = when (version) {
            Version.Version1,
            Version.Version2,
            -> 16 to 150

            Version.Detailed,
            Version.Fiducial,
            Version.GrayscaleFiducial,
            -> 32 to 300
        }

        var currentCellGrid = CellGrid(length, length)
        var nextCellGrid = CellGrid(length, length)
        var currentChangeGrid = ChangeGrid(length, length)
        var nextChangeGrid = ChangeGrid(length, length)

        when (version) {
            Version.Version1 -> {
                nextCellGrid.loadFrom(digest)
            }

            Version.Version2 -> {
                val hashed = sha256(digest)
                nextCellGrid.loadFrom(hashed)
            }

            Version.Detailed,
            Version.Fiducial,
            Version.GrayscaleFiducial,
            -> {
                var digest1 = digest.copyOf()
                if (version == Version.GrayscaleFiducial) {
                    digest1 = sha256(digest1)
                }
                val digest2 = sha256(digest1)
                val digest3 = sha256(digest2)
                val digest4 = sha256(digest3)
                val digestFinal = digest1 + digest2 + digest3 + digest4
                nextCellGrid.loadFrom(digestFinal)
            }
        }

        nextChangeGrid.grid.fill(true)

        val historySet = mutableSetOf<ByteArrayKey>()
        val history = mutableListOf<ByteArray>()

        while (history.size < maxGenerations) {
            val tmpCell = currentCellGrid
            currentCellGrid = nextCellGrid
            nextCellGrid = tmpCell

            val tmpChange = currentChangeGrid
            currentChangeGrid = nextChangeGrid
            nextChangeGrid = tmpChange

            val data = currentCellGrid.toByteArray()
            val hash = sha256(data)
            val key = ByteArrayKey(hash)

            if (key in historySet) {
                break
            }

            historySet.add(key)
            history.add(data)

            currentCellGrid.nextGeneration(currentChangeGrid, nextCellGrid, nextChangeGrid)
        }

        val fracGrid = FracGrid(length, length)
        for ((i, historyData) in history.withIndex()) {
            currentCellGrid.loadFrom(historyData)
            val frac = clamped(lerpFrom(0.0, history.size.toDouble(), (i + 1).toDouble()))
            fracGrid.overlay(currentCellGrid, frac)
        }

        if (version != Version.Version1) {
            var minValue = Double.POSITIVE_INFINITY
            var maxValue = Double.NEGATIVE_INFINITY

            fracGrid.grid.forEach { x, y ->
                val value = fracGrid.grid.getValue(x, y)
                if (value < minValue) {
                    minValue = value
                }
                if (value > maxValue) {
                    maxValue = value
                }
            }

            val width = fracGrid.grid.width
            val height = fracGrid.grid.height
            for (y in 0 until height) {
                for (x in 0 until width) {
                    val value = fracGrid.grid.getValue(x, y)
                    val normalized = lerpFrom(minValue, maxValue, value)
                    fracGrid.grid.setValue(normalized, x, y)
                }
            }
        }

        val entropy = BitEnumerator(digest.copyOf())
        when (version) {
            Version.Detailed -> {
                entropy.next()
            }

            Version.Version2 -> {
                entropy.nextUInt2()
            }

            else -> Unit
        }

        val gradient = selectGradient(entropy, version)
        val pattern = selectPattern(entropy, version)
        val colorGrid = ColorGrid(fracGrid, gradient, pattern)

        return renderImage(
            colorGrid.grid.width,
            colorGrid.grid.height,
            colorGrid.colors(),
            moduleSize,
            hasAlpha,
        )
    }
}
