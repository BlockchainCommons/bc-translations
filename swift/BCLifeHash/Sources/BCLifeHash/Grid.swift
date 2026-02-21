final class Grid<T> {
    let width: Int
    let height: Int
    var storage: [T]

    init(width: Int, height: Int, defaultValue: T) {
        self.width = width
        self.height = height
        self.storage = Array(repeating: defaultValue, count: width * height)
    }

    @inline(__always)
    private func offset(_ x: Int, _ y: Int) -> Int {
        y * width + x
    }

    @inline(__always)
    private static func circularIndex(_ index: Int, _ modulus: Int) -> Int {
        ((index % modulus) + modulus) % modulus
    }

    func setAll(_ value: T) {
        storage = Array(repeating: value, count: storage.count)
    }

    @inline(__always)
    subscript(x: Int, y: Int) -> T {
        get { storage[offset(x, y)] }
        set { storage[offset(x, y)] = newValue }
    }

    func forEachCell(_ body: (_ x: Int, _ y: Int) -> Void) {
        for y in 0..<height {
            for x in 0..<width {
                body(x, y)
            }
        }
    }

    func forNeighborhood(_ px: Int, _ py: Int, _ body: (_ ox: Int, _ oy: Int, _ nx: Int, _ ny: Int) -> Void) {
        for oy in -1...1 {
            for ox in -1...1 {
                let nx = Grid.circularIndex(ox + px, width)
                let ny = Grid.circularIndex(oy + py, height)
                body(ox, oy, nx, ny)
            }
        }
    }
}
