import Foundation

struct HSBColor {
    let hue: Double
    let saturation: Double
    let brightness: Double

    static func fromHue(_ hue: Double) -> HSBColor {
        HSBColor(hue: hue, saturation: 1.0, brightness: 1.0)
    }

    func color() -> Color {
        let v = clamped(brightness)
        let s = clamped(saturation)

        if s <= 0.0 {
            return Color(r: v, g: v, b: v)
        }

        var h = modulo(hue, 1.0)
        if h < 0.0 {
            h += 1.0
        }
        h *= 6.0

        let i = Int(floor(Float(h)))
        let f = h - Double(i)
        let p = v * (1.0 - s)
        let q = v * (1.0 - s * f)
        let t = v * (1.0 - s * (1.0 - f))

        switch i {
        case 0:
            return Color(r: v, g: t, b: p)
        case 1:
            return Color(r: q, g: v, b: p)
        case 2:
            return Color(r: p, g: v, b: t)
        case 3:
            return Color(r: p, g: q, b: v)
        case 4:
            return Color(r: t, g: p, b: v)
        case 5:
            return Color(r: v, g: p, b: q)
        default:
            preconditionFailure("Internal error in HSB conversion")
        }
    }
}
