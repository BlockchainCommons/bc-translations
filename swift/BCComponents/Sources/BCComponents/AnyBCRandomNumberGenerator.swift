import BCRand
import Foundation

/// Reference wrapper that preserves RNG mutation across calls.
public final class AnyBCRandomNumberGenerator {
    private let _nextUInt32: () -> UInt32
    private let _nextUInt64: () -> UInt64
    private let _randomData: (Int) -> Data
    private let _fillRandomData: (inout Data) -> Void

    public init<G: BCRandomNumberGenerator>(_ base: G) {
        var base = base
        _nextUInt32 = { base.nextUInt32() }
        _nextUInt64 = { base.nextUInt64() }
        _randomData = { base.randomData(count: $0) }
        _fillRandomData = { base.fillRandomData(&$0) }
    }

    public func nextUInt32() -> UInt32 {
        _nextUInt32()
    }

    public func nextUInt64() -> UInt64 {
        _nextUInt64()
    }

    public func randomData(count: Int) -> Data {
        _randomData(count)
    }

    public func fillRandomData(_ data: inout Data) {
        _fillRandomData(&data)
    }
}
