typealias ColorFunc = (Double) -> Color

func reverse(_ c: @escaping ColorFunc) -> ColorFunc {
    { t in c(1.0 - t) }
}

func blend2(_ color1: Color, _ color2: Color) -> ColorFunc {
    { t in color1.lerpTo(color2, t) }
}

func blend(_ colors: [Color]) -> ColorFunc {
    let count = colors.count
    switch count {
    case 0:
        return blend2(.black, .black)
    case 1:
        return blend2(colors[0], colors[0])
    case 2:
        return blend2(colors[0], colors[1])
    default:
        return { t in
            if t >= 1.0 {
                return colors[count - 1]
            }
            if t <= 0.0 {
                return colors[0]
            }
            let segments = count - 1
            let s = t * Double(segments)
            let segment = Int(s)
            let segmentFrac = modulo(s, 1.0)
            let c1 = colors[segment]
            let c2 = colors[segment + 1]
            return c1.lerpTo(c2, segmentFrac)
        }
    }
}
