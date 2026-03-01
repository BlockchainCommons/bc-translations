import Foundation
import BCUR
import Observation

/// Tracks state of ongoing display of (possibly multi-part) UR.
@MainActor
@Observable
public final class URDisplayState {
    public let ur: UR
    public let maxFragmentLen: Int

    public var isSinglePart: Bool { partsCount == 1 }
    public var seqNum: Int { currentSequence }
    public var seqLen: Int { partsCount }

    public var framesPerSecond: Double = 10.0 {
        didSet { interval = 1.0 / framesPerSecond }
    }
    public private(set) var part: Data = Data()
    public private(set) var fragmentStates: [URFragmentBar.FragmentState] = [.off]

    private var encoder: MultipartEncoder!
    private var partsCount: Int = 0
    private var currentSequence: Int = 0
    private var timerTask: Task<Void, Never>?
    private var interval: TimeInterval = 1.0 / 10

    public init(ur: UR, maxFragmentLen: Int) {
        self.ur = ur
        self.maxFragmentLen = maxFragmentLen
        restart()
    }

    public func restart() {
        encoder = try! MultipartEncoder(ur, maxFragmentLen)
        partsCount = encoder.partsCount
        currentSequence = 0
        emitNextPart()
    }

    public func run() {
        guard !isSinglePart else { return }
        timerTask?.cancel()
        timerTask = Task { [weak self] in
            while !Task.isCancelled {
                try? await Task.sleep(for: .seconds(self?.interval ?? 0.1))
                guard !Task.isCancelled else { break }
                self?.emitNextPart()
            }
        }
    }

    public func stop() {
        timerTask?.cancel()
        timerTask = nil
    }

    private func emitNextPart() {
        let partString = try! encoder.nextPart()
        currentSequence = encoder.currentIndex
        part = partString.uppercased().utf8Data

        // For sequences 1..partsCount, the fountain encoder produces simple
        // (single-fragment) parts where fragment index = sequence - 1.
        // For sequences > partsCount, mixed parts are emitted; we show all
        // fragments as "on" since the exact mix indexes are internal to BCUR.
        if currentSequence <= partsCount {
            let fragmentIndex = currentSequence - 1
            fragmentStates = (0..<partsCount).map { i in
                i == fragmentIndex ? .on : .off
            }
        } else {
            fragmentStates = Array(repeating: .on, count: partsCount)
        }
    }
}
