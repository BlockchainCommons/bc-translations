import { Color, modulo } from './color.js';

export type ColorFunc = (t: number) => Color;

export function reverse(c: ColorFunc): ColorFunc {
    return (t: number) => c(1.0 - t);
}

export function blend2(color1: Color, color2: Color): ColorFunc {
    return (t: number) => color1.lerpTo(color2, t);
}

export function blend(colors: readonly Color[]): ColorFunc {
    const count = colors.length;
    if (count === 0) return blend2(Color.BLACK, Color.BLACK);
    if (count === 1) return blend2(colors[0], colors[0]);
    if (count === 2) return blend2(colors[0], colors[1]);
    const captured = colors.slice();
    return (t: number) => {
        if (t >= 1.0) return captured[count - 1];
        if (t <= 0.0) return captured[0];
        const segments = count - 1;
        const s = t * segments;
        const segment = Math.trunc(s);
        const segmentFrac = modulo(s, 1.0);
        const c1 = captured[segment];
        const c2 = captured[segment + 1];
        return c1.lerpTo(c2, segmentFrac);
    };
}
