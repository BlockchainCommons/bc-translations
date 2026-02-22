/// Internal weighted sampler using Vose's alias method.
internal struct WeightedSampler: Sendable {
    private let aliases: [Int]
    private let probs: [Double]

    init(weights: [Double]) throws {
        guard !weights.contains(where: { $0 < 0 }) else {
            throw WeightedSamplerError.negativeProbability
        }

        let summed = weights.reduce(0, +)
        guard summed > 0 else {
            throw WeightedSamplerError.nonPositiveTotal
        }

        let count = weights.count
        var normalized = weights.map { $0 * Double(count) / summed }

        var probs = Array(repeating: 0.0, count: count)
        var aliases = Array(repeating: 0, count: count)

        var small: [Int] = []
        var large: [Int] = []

        if count > 0 {
            for index in stride(from: count - 1, through: 0, by: -1) {
                if normalized[index] < 1.0 {
                    small.append(index)
                } else {
                    large.append(index)
                }
            }
        }

        while !small.isEmpty && !large.isEmpty {
            let a = small.removeLast()
            let g = large.removeLast()

            probs[a] = normalized[a]
            aliases[a] = g

            normalized[g] += normalized[a] - 1.0

            if normalized[g] < 1.0 {
                small.append(g)
            } else {
                large.append(g)
            }
        }

        while !large.isEmpty {
            probs[large.removeLast()] = 1.0
        }

        while !small.isEmpty {
            probs[small.removeLast()] = 1.0
        }

        self.aliases = aliases
        self.probs = probs
    }

    mutating func next(using xoshiro: inout Xoshiro256) -> Int {
        let r1 = xoshiro.nextDouble()
        let r2 = xoshiro.nextDouble()

        let i = Int(Double(probs.count) * r1)
        return r2 < probs[i] ? i : aliases[i]
    }
}
