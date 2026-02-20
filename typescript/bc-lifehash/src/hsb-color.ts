import { Color, clamped, modulo } from './color.js';

export class HSBColor {
    readonly hue: number;
    readonly saturation: number;
    readonly brightness: number;

    constructor(hue: number, saturation: number, brightness: number) {
        this.hue = hue;
        this.saturation = saturation;
        this.brightness = brightness;
    }

    static fromHue(hue: number): HSBColor {
        return new HSBColor(hue, 1.0, 1.0);
    }

    color(): Color {
        const v = clamped(this.brightness);
        const s = clamped(this.saturation);

        if (s <= 0.0) {
            return new Color(v, v, v);
        }

        let h = modulo(this.hue, 1.0);
        if (h < 0.0) {
            h += 1.0;
        }
        h *= 6.0;
        // C++ uses floorf (f32 precision)
        const i = Math.floor(Math.fround(h));
        const f = h - i;
        const p = v * (1.0 - s);
        const q = v * (1.0 - s * f);
        const t = v * (1.0 - s * (1.0 - f));

        switch (i) {
            case 0: return new Color(v, t, p);
            case 1: return new Color(q, v, p);
            case 2: return new Color(p, v, t);
            case 3: return new Color(p, q, v);
            case 4: return new Color(t, p, v);
            case 5: return new Color(v, p, q);
            default: throw new Error('Internal error in HSB conversion');
        }
    }
}
