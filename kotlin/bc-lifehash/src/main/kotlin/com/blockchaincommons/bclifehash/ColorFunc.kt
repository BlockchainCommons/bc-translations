package com.blockchaincommons.bclifehash

/** A function that maps a value in 0.0..1.0 to a [Color]. */
internal typealias ColorFunc = (Double) -> Color

/** Returns a [ColorFunc] that reverses the gradient direction. */
internal fun reverse(colorFunc: ColorFunc): ColorFunc = { t -> colorFunc(1.0 - t) }

/** Returns a [ColorFunc] that linearly blends between two colors. */
internal fun blend2(color1: Color, color2: Color): ColorFunc = { t -> color1.lerpTo(color2, t) }

/** Returns a [ColorFunc] that linearly blends through a list of colors. */
internal fun blend(colors: List<Color>): ColorFunc {
    val count = colors.size
    return when (count) {
        0 -> blend2(Color.BLACK, Color.BLACK)
        1 -> blend2(colors[0], colors[0])
        2 -> blend2(colors[0], colors[1])
        else -> { t ->
            when {
                t >= 1.0 -> colors[count - 1]
                t <= 0.0 -> colors[0]
                else -> {
                    val segments = count - 1
                    val s = t * segments.toDouble()
                    val segment = s.toInt()
                    val segmentFrac = modulo(s, 1.0)
                    val c1 = colors[segment]
                    val c2 = colors[segment + 1]
                    c1.lerpTo(c2, segmentFrac)
                }
            }
        }
    }
}
