/// Xoshiro256** pseudo-random number generator.
///
/// Reference: David Blackman and Sebastiano Vigna, "Scrambled Linear
/// Pseudorandom Number Generators", ACM Trans. Math. Softw. 47(4), 2021.
/// https://prng.di.unimi.it/xoshiro256starstar.c
internal struct Xoshiro256StarStar: Sendable {
    private var state: (UInt64, UInt64, UInt64, UInt64)

    init(seed: (UInt64, UInt64, UInt64, UInt64)) {
        self.state = seed
    }

    mutating func next() -> UInt64 {
        let result = rotl(state.1 &* 5, 7) &* 9
        let t = state.1 << 17

        state.2 ^= state.0
        state.3 ^= state.1
        state.1 ^= state.2
        state.0 ^= state.3

        state.2 ^= t
        state.3 = rotl(state.3, 45)

        return result
    }

    private func rotl(_ x: UInt64, _ k: Int) -> UInt64 {
        (x << k) | (x >> (64 - k))
    }
}
