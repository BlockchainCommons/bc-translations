export function clamped(n: number): number {
    return Math.min(Math.max(n, 0.0), 1.0);
}

/** C++ uses fmodf (f32 precision) even though arguments are f64. */
export function modulo(dividend: number, divisor: number): number {
    const a = Math.fround(Math.fround(dividend) % Math.fround(divisor));
    const b = Math.fround(Math.fround(a + Math.fround(divisor)) % Math.fround(divisor));
    return b;
}

export function lerpTo(toA: number, toB: number, t: number): number {
    return t * (toB - toA) + toA;
}

export function lerpFrom(fromA: number, fromB: number, t: number): number {
    return (fromA - t) / (fromA - fromB);
}

export function lerp(
    fromA: number,
    fromB: number,
    toC: number,
    toD: number,
    t: number,
): number {
    return lerpTo(toC, toD, lerpFrom(fromA, fromB, t));
}

export class Color {
    static readonly WHITE = new Color(1.0, 1.0, 1.0);
    static readonly BLACK = new Color(0.0, 0.0, 0.0);

    readonly r: number;
    readonly g: number;
    readonly b: number;

    constructor(r: number, g: number, b: number) {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    static fromUint8Values(r: number, g: number, b: number): Color {
        return new Color(r / 255.0, g / 255.0, b / 255.0);
    }

    lerpTo(other: Color, t: number): Color {
        const f = clamped(t);
        const red = clamped(this.r * (1.0 - f) + other.r * f);
        const green = clamped(this.g * (1.0 - f) + other.g * f);
        const blue = clamped(this.b * (1.0 - f) + other.b * f);
        return new Color(red, green, blue);
    }

    lighten(t: number): Color {
        return this.lerpTo(Color.WHITE, t);
    }

    darken(t: number): Color {
        return this.lerpTo(Color.BLACK, t);
    }

    burn(t: number): Color {
        const f = Math.max(1.0 - t, 1.0e-7);
        return new Color(
            Math.min(1.0 - (1.0 - this.r) / f, 1.0),
            Math.min(1.0 - (1.0 - this.g) / f, 1.0),
            Math.min(1.0 - (1.0 - this.b) / f, 1.0),
        );
    }

    /** Luminance using C++ sqrtf/powf (f32 precision). */
    luminance(): number {
        const r = Math.fround(0.299 * this.r);
        const g = Math.fround(0.587 * this.g);
        const b = Math.fround(0.114 * this.b);
        const val = Math.fround(
            Math.fround(Math.fround(r * r) + Math.fround(g * g)) +
            Math.fround(b * b),
        );
        return Math.fround(Math.sqrt(val));
    }
}
