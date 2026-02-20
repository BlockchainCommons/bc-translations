final class ColorGrid {
    struct Transform {
        let transpose: Bool
        let reflectX: Bool
        let reflectY: Bool
    }

    let grid: Grid<Color>

    init(fracGrid: FracGrid, gradient: @escaping ColorFunc, pattern: Pattern) {
        let multiplier = pattern == .fiducial ? 1 : 2
        let targetWidth = fracGrid.grid.width * multiplier
        let targetHeight = fracGrid.grid.height * multiplier

        self.grid = Grid(width: targetWidth, height: targetHeight, defaultValue: .black)
        let maxX = targetWidth - 1
        let maxY = targetHeight - 1

        let transforms: [Transform]
        switch pattern {
        case .snowflake:
            transforms = [
                Transform(transpose: false, reflectX: false, reflectY: false),
                Transform(transpose: false, reflectX: true, reflectY: false),
                Transform(transpose: false, reflectX: false, reflectY: true),
                Transform(transpose: false, reflectX: true, reflectY: true),
            ]
        case .pinwheel:
            transforms = [
                Transform(transpose: false, reflectX: false, reflectY: false),
                Transform(transpose: true, reflectX: true, reflectY: false),
                Transform(transpose: true, reflectX: false, reflectY: true),
                Transform(transpose: false, reflectX: true, reflectY: true),
            ]
        case .fiducial:
            transforms = [
                Transform(transpose: false, reflectX: false, reflectY: false),
            ]
        }

        let fracWidth = fracGrid.grid.width
        let fracHeight = fracGrid.grid.height
        for y in 0..<fracHeight {
            for x in 0..<fracWidth {
                let value = fracGrid.grid.getValue(x, y)
                let color = gradient(value)
                for t in transforms {
                    var px = x
                    var py = y
                    if t.transpose {
                        swap(&px, &py)
                    }
                    if t.reflectX {
                        px = maxX - px
                    }
                    if t.reflectY {
                        py = maxY - py
                    }
                    grid.setValue(color, px, py)
                }
            }
        }
    }

    func colors() -> [Double] {
        var result: [Double] = []
        result.reserveCapacity(grid.storage.count * 3)
        for c in grid.storage {
            result.append(c.r)
            result.append(c.g)
            result.append(c.b)
        }
        return result
    }
}
