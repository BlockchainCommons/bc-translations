import Foundation

@inline(__always)
func clamped(_ n: Double) -> Double {
    min(max(n, 0.0), 1.0)
}

/// Computes `dividend mod divisor` with correct wrapping for negative values.
///
/// Arithmetic is performed in single precision (`Float`) to match the
/// reference implementation's f32 intermediate results.
@inline(__always)
func modulo(_ dividend: Double, _ divisor: Double) -> Double {
    let a = Float(dividend).truncatingRemainder(dividingBy: Float(divisor))
    let b = (a + Float(divisor)).truncatingRemainder(dividingBy: Float(divisor))
    return Double(b)
}

@inline(__always)
func lerpTo(_ toA: Double, _ toB: Double, _ t: Double) -> Double {
    t * (toB - toA) + toA
}

@inline(__always)
func lerpFrom(_ fromA: Double, _ fromB: Double, _ t: Double) -> Double {
    (fromA - t) / (fromA - fromB)
}

@inline(__always)
func lerp(_ fromA: Double, _ fromB: Double, _ toC: Double, _ toD: Double, _ t: Double) -> Double {
    lerpTo(toC, toD, lerpFrom(fromA, fromB, t))
}

struct Color {
    let r: Double
    let g: Double
    let b: Double

    static let white = Color(r: 1.0, g: 1.0, b: 1.0)
    static let black = Color(r: 0.0, g: 0.0, b: 0.0)
    static let red = Color(r: 1.0, g: 0.0, b: 0.0)
    static let green = Color(r: 0.0, g: 1.0, b: 0.0)
    static let blue = Color(r: 0.0, g: 0.0, b: 1.0)
    static let cyan = Color(r: 0.0, g: 1.0, b: 1.0)
    static let magenta = Color(r: 1.0, g: 0.0, b: 1.0)
    static let yellow = Color(r: 1.0, g: 1.0, b: 0.0)

    static func fromUInt8Values(_ r: UInt8, _ g: UInt8, _ b: UInt8) -> Color {
        Color(
            r: Double(r) / 255.0,
            g: Double(g) / 255.0,
            b: Double(b) / 255.0
        )
    }

    func lerpTo(_ other: Color, _ t: Double) -> Color {
        let f = clamped(t)
        let red = clamped(r * (1.0 - f) + other.r * f)
        let green = clamped(g * (1.0 - f) + other.g * f)
        let blue = clamped(b * (1.0 - f) + other.b * f)
        return Color(r: red, g: green, b: blue)
    }

    func lighten(_ t: Double) -> Color {
        lerpTo(.white, t)
    }

    func darken(_ t: Double) -> Color {
        lerpTo(.black, t)
    }

    func burn(_ t: Double) -> Color {
        let f = max(1.0 - t, 1.0e-7)
        return Color(
            r: min(1.0 - (1.0 - r) / f, 1.0),
            g: min(1.0 - (1.0 - g) / f, 1.0),
            b: min(1.0 - (1.0 - b) / f, 1.0)
        )
    }

    /// Computes perceived luminance using weighted RGB components.
    ///
    /// Intermediate calculations use single precision (`Float`) to match
    /// the reference implementation's f32 behavior.
    func luminance() -> Double {
        let r = Float(0.299 * self.r)
        let g = Float(0.587 * self.g)
        let b = Float(0.114 * self.b)
        let val = (r * r) + (g * g) + (b * b)
        return Double(sqrt(val))
    }
}
