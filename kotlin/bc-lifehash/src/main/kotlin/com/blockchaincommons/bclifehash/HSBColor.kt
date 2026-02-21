package com.blockchaincommons.bclifehash

/**
 * A color in the HSB (hue-saturation-brightness) color space.
 *
 * All components are normalized to the range 0.0..1.0.
 */
internal data class HSBColor(
    val hue: Double,
    val saturation: Double,
    val brightness: Double,
) {
    companion object {
        /** Creates a fully saturated, full-brightness color from a hue value (0.0..1.0). */
        fun fromHue(hue: Double): HSBColor = HSBColor(hue, 1.0, 1.0)
    }

    /** Converts this HSB color to an RGB [Color]. */
    fun toColor(): Color {
        val v = clamped(brightness)
        val s = clamped(saturation)

        if (s <= 0.0) {
            return Color(v, v, v)
        }

        var h = modulo(hue, 1.0)
        if (h < 0.0) {
            h += 1.0
        }
        h *= 6.0

        val i = kotlin.math.floor(h.toFloat().toDouble()).toInt()
        val f = h - i.toDouble()
        val p = v * (1.0 - s)
        val q = v * (1.0 - s * f)
        val t = v * (1.0 - s * (1.0 - f))

        return when (i) {
            0 -> Color(v, t, p)
            1 -> Color(q, v, p)
            2 -> Color(p, v, t)
            3 -> Color(p, q, v)
            4 -> Color(t, p, v)
            5 -> Color(v, p, q)
            else -> error("Internal error in HSB conversion")
        }
    }
}
