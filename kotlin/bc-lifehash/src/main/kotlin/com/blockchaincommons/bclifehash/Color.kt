package com.blockchaincommons.bclifehash

fun clamped(n: Double): Double = n.coerceIn(0.0, 1.0)

fun modulo(dividend: Double, divisor: Double): Double {
    val a = dividend.toFloat() % divisor.toFloat()
    val b = (a + divisor.toFloat()) % divisor.toFloat()
    return b.toDouble()
}

fun lerpTo(toA: Double, toB: Double, t: Double): Double = t * (toB - toA) + toA

fun lerpFrom(fromA: Double, fromB: Double, t: Double): Double = (fromA - t) / (fromA - fromB)

fun lerp(fromA: Double, fromB: Double, toC: Double, toD: Double, t: Double): Double =
    lerpTo(toC, toD, lerpFrom(fromA, fromB, t))

data class Color(
    val r: Double,
    val g: Double,
    val b: Double,
) {
    companion object {
        val WHITE = Color(1.0, 1.0, 1.0)
        val BLACK = Color(0.0, 0.0, 0.0)
        val RED = Color(1.0, 0.0, 0.0)
        val GREEN = Color(0.0, 1.0, 0.0)
        val BLUE = Color(0.0, 0.0, 1.0)
        val CYAN = Color(0.0, 1.0, 1.0)
        val MAGENTA = Color(1.0, 0.0, 1.0)
        val YELLOW = Color(1.0, 1.0, 0.0)

        fun fromUInt8Values(r: Int, g: Int, b: Int): Color =
            Color(
                r.toDouble() / 255.0,
                g.toDouble() / 255.0,
                b.toDouble() / 255.0,
            )
    }

    fun lerpTo(other: Color, t: Double): Color {
        val f = clamped(t)
        val red = clamped(r * (1.0 - f) + other.r * f)
        val green = clamped(g * (1.0 - f) + other.g * f)
        val blue = clamped(b * (1.0 - f) + other.b * f)
        return Color(red, green, blue)
    }

    fun lighten(t: Double): Color = lerpTo(WHITE, t)

    fun darken(t: Double): Color = lerpTo(BLACK, t)

    fun burn(t: Double): Color {
        val f = maxOf(1.0 - t, 1.0e-7)
        return Color(
            minOf(1.0 - (1.0 - r) / f, 1.0),
            minOf(1.0 - (1.0 - g) / f, 1.0),
            minOf(1.0 - (1.0 - b) / f, 1.0),
        )
    }

    fun luminance(): Double {
        val r = (0.299f * this.r.toFloat())
        val g = (0.587f * this.g.toFloat())
        val b = (0.114f * this.b.toFloat())
        val value = r * r + g * g + b * b
        return kotlin.math.sqrt(value).toDouble()
    }
}
