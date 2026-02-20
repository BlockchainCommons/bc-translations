/**
 * LifeHash visual hashing based on Conway's Game of Life.
 *
 * The output is deterministic for the same input bytes and rendering parameters.
 * Five rendering variants are available through [Version].
 */
package com.blockchaincommons.bclifehash

import java.security.MessageDigest

enum class Version {
    Version1,
    Version2,
    Detailed,
    Fiducial,
    GrayscaleFiducial,
}

class Image(
    val width: Int,
    val height: Int,
    val colors: ByteArray,
)

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

private fun makeImage(
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

fun makeFromUtf8(
    s: String,
    version: Version,
    moduleSize: Int,
    hasAlpha: Boolean,
): Image = makeFromData(s.toByteArray(Charsets.UTF_8), version, moduleSize, hasAlpha)

fun makeFromData(
    data: ByteArray,
    version: Version,
    moduleSize: Int,
    hasAlpha: Boolean,
): Image {
    val digest = sha256(data)
    return makeFromDigest(digest, version, moduleSize, hasAlpha)
}

fun makeFromDigest(
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
            nextCellGrid.setData(digest)
        }

        Version.Version2 -> {
            val hashed = sha256(digest)
            nextCellGrid.setData(hashed)
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
            nextCellGrid.setData(digestFinal)
        }
    }

    nextChangeGrid.grid.setAll(true)

    val historySet = mutableSetOf<ByteArrayKey>()
    val history = mutableListOf<ByteArray>()

    while (history.size < maxGenerations) {
        val tmpCell = currentCellGrid
        currentCellGrid = nextCellGrid
        nextCellGrid = tmpCell

        val tmpChange = currentChangeGrid
        currentChangeGrid = nextChangeGrid
        nextChangeGrid = tmpChange

        val data = currentCellGrid.data()
        val hash = sha256(data)
        val key = ByteArrayKey(hash)

        if (historySet.contains(key)) {
            break
        }

        historySet.add(key)
        history.add(data)

        currentCellGrid.nextGeneration(currentChangeGrid, nextCellGrid, nextChangeGrid)
    }

    val fracGrid = FracGrid(length, length)
    for ((i, historyData) in history.withIndex()) {
        currentCellGrid.setData(historyData)
        val frac = clamped(lerpFrom(0.0, history.size.toDouble(), (i + 1).toDouble()))
        fracGrid.overlay(currentCellGrid, frac)
    }

    if (version != Version.Version1) {
        var minValue = Double.POSITIVE_INFINITY
        var maxValue = Double.NEGATIVE_INFINITY

        fracGrid.grid.forAll { x, y ->
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

    return makeImage(
        colorGrid.grid.width,
        colorGrid.grid.height,
        colorGrid.colors(),
        moduleSize,
        hasAlpha,
    )
}
