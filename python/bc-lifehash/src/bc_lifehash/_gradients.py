from __future__ import annotations

from typing import TYPE_CHECKING

from ._bit_enumerator import BitEnumerator
from ._color import BLACK, WHITE, Color, lerp, modulo
from ._color_func import ColorFunc, blend, blend2, reverse
from ._hsb_color import HSBColor

if TYPE_CHECKING:
    from ._lifehash import Version


def _grayscale() -> ColorFunc:
    return blend2(BLACK, WHITE)


def _select_grayscale(entropy: BitEnumerator) -> ColorFunc:
    if entropy.next():
        return _grayscale()
    return reverse(_grayscale())


def _make_hue(t: float) -> Color:
    return HSBColor.from_hue(t).color()


def _spectrum() -> ColorFunc:
    return blend([
        Color.from_uint8_values(0, 168, 222),
        Color.from_uint8_values(51, 51, 145),
        Color.from_uint8_values(233, 19, 136),
        Color.from_uint8_values(235, 45, 46),
        Color.from_uint8_values(253, 233, 43),
        Color.from_uint8_values(0, 158, 84),
        Color.from_uint8_values(0, 168, 222),
    ])


def _spectrum_cmyk_safe() -> ColorFunc:
    return blend([
        Color.from_uint8_values(0, 168, 222),
        Color.from_uint8_values(41, 60, 130),
        Color.from_uint8_values(210, 59, 130),
        Color.from_uint8_values(217, 63, 53),
        Color.from_uint8_values(244, 228, 81),
        Color.from_uint8_values(0, 158, 84),
        Color.from_uint8_values(0, 168, 222),
    ])


def _adjust_for_luminance(color: Color, contrast_color: Color) -> Color:
    lum = color.luminance()
    contrast_lum = contrast_color.luminance()
    threshold = 0.6
    offset = abs(lum - contrast_lum)
    if offset > threshold:
        return color
    boost = 0.7
    t = lerp(0.0, threshold, boost, 0.0, offset)
    if contrast_lum > lum:
        return color.darken(t).burn(t * 0.6)
    return color.lighten(t).burn(t * 0.6)


def _monochromatic(entropy: BitEnumerator, hue_generator: ColorFunc) -> ColorFunc:
    hue = entropy.next_frac()
    is_tint = entropy.next()
    is_reversed = entropy.next()
    key_advance = entropy.next_frac() * 0.3 + 0.05
    neutral_advance = entropy.next_frac() * 0.3 + 0.05

    key_color = hue_generator(hue)

    if is_tint:
        contrast_brightness = 1.0
        key_color = key_color.darken(0.5)
    else:
        contrast_brightness = 0.0

    gs = _grayscale()
    neutral_color = gs(contrast_brightness)

    key_color_2 = key_color.lerp_to(neutral_color, key_advance)
    neutral_color_2 = neutral_color.lerp_to(key_color, neutral_advance)

    gradient = blend2(key_color_2, neutral_color_2)
    return reverse(gradient) if is_reversed else gradient


def _monochromatic_fiducial(entropy: BitEnumerator) -> ColorFunc:
    hue = entropy.next_frac()
    is_reversed = entropy.next()
    is_tint = entropy.next()

    contrast_color = WHITE if is_tint else BLACK
    spec = _spectrum_cmyk_safe()
    key_color = _adjust_for_luminance(spec(hue), contrast_color)

    gradient = blend([key_color, contrast_color, key_color])
    return reverse(gradient) if is_reversed else gradient


def _complementary(entropy: BitEnumerator, hue_generator: ColorFunc) -> ColorFunc:
    spectrum1 = entropy.next_frac()
    spectrum2 = modulo(spectrum1 + 0.5, 1.0)
    lighter_advance = entropy.next_frac() * 0.3
    darker_advance = entropy.next_frac() * 0.3
    is_reversed = entropy.next()

    color1 = hue_generator(spectrum1)
    color2 = hue_generator(spectrum2)

    luma1 = color1.luminance()
    luma2 = color2.luminance()

    if luma1 > luma2:
        darker_color, lighter_color = color2, color1
    else:
        darker_color, lighter_color = color1, color2

    adjusted_lighter = lighter_color.lighten(lighter_advance)
    adjusted_darker = darker_color.darken(darker_advance)

    gradient = blend2(adjusted_darker, adjusted_lighter)
    return reverse(gradient) if is_reversed else gradient


def _complementary_fiducial(entropy: BitEnumerator) -> ColorFunc:
    spectrum1 = entropy.next_frac()
    spectrum2 = modulo(spectrum1 + 0.5, 1.0)
    is_tint = entropy.next()
    is_reversed = entropy.next()
    neutral_color_bias = entropy.next()

    neutral_color = WHITE if is_tint else BLACK
    spec = _spectrum_cmyk_safe()
    color1 = spec(spectrum1)
    color2 = spec(spectrum2)

    bias_color = color1 if neutral_color_bias else color2
    biased_neutral_color = neutral_color.lerp_to(bias_color, 0.2).burn(0.1)

    gradient = blend([
        _adjust_for_luminance(color1, biased_neutral_color),
        biased_neutral_color,
        _adjust_for_luminance(color2, biased_neutral_color),
    ])
    return reverse(gradient) if is_reversed else gradient


def _triadic(entropy: BitEnumerator, hue_generator: ColorFunc) -> ColorFunc:
    spectrum1 = entropy.next_frac()
    spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0)
    spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0)
    lighter_advance = entropy.next_frac() * 0.3
    darker_advance = entropy.next_frac() * 0.3
    is_reversed = entropy.next()

    color1 = hue_generator(spectrum1)
    color2 = hue_generator(spectrum2)
    color3 = hue_generator(spectrum3)

    colors = sorted([color1, color2, color3], key=lambda c: c.luminance())
    darker_color = colors[0]
    middle_color = colors[1]
    lighter_color = colors[2]

    adjusted_lighter = lighter_color.lighten(lighter_advance)
    adjusted_darker = darker_color.darken(darker_advance)

    gradient = blend([adjusted_lighter, middle_color, adjusted_darker])
    return reverse(gradient) if is_reversed else gradient


def _triadic_fiducial(entropy: BitEnumerator) -> ColorFunc:
    spectrum1 = entropy.next_frac()
    spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0)
    spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0)
    is_tint = entropy.next()
    neutral_insert_index = (entropy.next_uint8() % 2 + 1)
    is_reversed = entropy.next()

    neutral_color = WHITE if is_tint else BLACK

    spec = _spectrum_cmyk_safe()
    colors = [spec(spectrum1), spec(spectrum2), spec(spectrum3)]

    if neutral_insert_index == 1:
        colors[0] = _adjust_for_luminance(colors[0], neutral_color)
        colors[1] = _adjust_for_luminance(colors[1], neutral_color)
        colors[2] = _adjust_for_luminance(colors[2], colors[1])
    elif neutral_insert_index == 2:
        colors[1] = _adjust_for_luminance(colors[1], neutral_color)
        colors[2] = _adjust_for_luminance(colors[2], neutral_color)
        colors[0] = _adjust_for_luminance(colors[0], colors[1])
    else:
        raise RuntimeError("Internal error")

    colors.insert(neutral_insert_index, neutral_color)

    gradient = blend(colors)
    return reverse(gradient) if is_reversed else gradient


def _analogous(entropy: BitEnumerator, hue_generator: ColorFunc) -> ColorFunc:
    spectrum1 = entropy.next_frac()
    spectrum2 = modulo(spectrum1 + 1.0 / 12.0, 1.0)
    spectrum3 = modulo(spectrum1 + 2.0 / 12.0, 1.0)
    spectrum4 = modulo(spectrum1 + 3.0 / 12.0, 1.0)
    advance = entropy.next_frac() * 0.5 + 0.2
    is_reversed = entropy.next()

    color1 = hue_generator(spectrum1)
    color2 = hue_generator(spectrum2)
    color3 = hue_generator(spectrum3)
    color4 = hue_generator(spectrum4)

    if color1.luminance() < color4.luminance():
        darkest_color, dark_color, light_color, lightest_color = (
            color1, color2, color3, color4
        )
    else:
        darkest_color, dark_color, light_color, lightest_color = (
            color4, color3, color2, color1
        )

    adjusted_darkest = darkest_color.darken(advance)
    adjusted_dark = dark_color.darken(advance / 2.0)
    adjusted_light = light_color.lighten(advance / 2.0)
    adjusted_lightest = lightest_color.lighten(advance)

    gradient = blend([
        adjusted_darkest, adjusted_dark, adjusted_light, adjusted_lightest
    ])
    return reverse(gradient) if is_reversed else gradient


def _analogous_fiducial(entropy: BitEnumerator) -> ColorFunc:
    spectrum1 = entropy.next_frac()
    spectrum2 = modulo(spectrum1 + 1.0 / 10.0, 1.0)
    spectrum3 = modulo(spectrum1 + 2.0 / 10.0, 1.0)
    is_tint = entropy.next()
    neutral_insert_index = (entropy.next_uint8() % 2 + 1)
    is_reversed = entropy.next()

    neutral_color = WHITE if is_tint else BLACK

    spec = _spectrum_cmyk_safe()
    colors = [spec(spectrum1), spec(spectrum2), spec(spectrum3)]

    if neutral_insert_index == 1:
        colors[0] = _adjust_for_luminance(colors[0], neutral_color)
        colors[1] = _adjust_for_luminance(colors[1], neutral_color)
        colors[2] = _adjust_for_luminance(colors[2], colors[1])
    elif neutral_insert_index == 2:
        colors[1] = _adjust_for_luminance(colors[1], neutral_color)
        colors[2] = _adjust_for_luminance(colors[2], neutral_color)
        colors[0] = _adjust_for_luminance(colors[0], colors[1])
    else:
        raise RuntimeError("Internal error")

    colors.insert(neutral_insert_index, neutral_color)

    gradient = blend(colors)
    return reverse(gradient) if is_reversed else gradient


def select_gradient(entropy: BitEnumerator, version: Version) -> ColorFunc:
    from ._lifehash import Version

    if version == Version.GRAYSCALE_FIDUCIAL:
        return _select_grayscale(entropy)

    value = entropy.next_uint2()

    if value == 0:
        if version == Version.VERSION1:
            return _monochromatic(entropy, _make_hue)
        if version in (Version.VERSION2, Version.DETAILED):
            return _monochromatic(entropy, _spectrum_cmyk_safe())
        if version == Version.FIDUCIAL:
            return _monochromatic_fiducial(entropy)
        return _grayscale()
    elif value == 1:
        if version == Version.VERSION1:
            return _complementary(entropy, _spectrum())
        if version in (Version.VERSION2, Version.DETAILED):
            return _complementary(entropy, _spectrum_cmyk_safe())
        if version == Version.FIDUCIAL:
            return _complementary_fiducial(entropy)
        return _grayscale()
    elif value == 2:
        if version == Version.VERSION1:
            return _triadic(entropy, _spectrum())
        if version in (Version.VERSION2, Version.DETAILED):
            return _triadic(entropy, _spectrum_cmyk_safe())
        if version == Version.FIDUCIAL:
            return _triadic_fiducial(entropy)
        return _grayscale()
    elif value == 3:
        if version == Version.VERSION1:
            return _analogous(entropy, _spectrum())
        if version in (Version.VERSION2, Version.DETAILED):
            return _analogous(entropy, _spectrum_cmyk_safe())
        if version == Version.FIDUCIAL:
            return _analogous_fiducial(entropy)
        return _grayscale()
    else:
        return _grayscale()
