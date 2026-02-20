package com.blockchaincommons.bclifehash

typealias ColorFunc = (Double) -> Color

fun reverse(colorFunc: ColorFunc): ColorFunc = { t -> colorFunc(1.0 - t) }

fun blend2(color1: Color, color2: Color): ColorFunc = { t -> color1.lerpTo(color2, t) }

fun blend(colors: List<Color>): ColorFunc {
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
