import Foundation
import BCUR
import Observation

/// Simulates scanning a UR by encoding it into fountain-coded parts and
/// feeding the part strings to a `URScanState` on a timer.
///
/// This exercises the full encode→decode round-trip without camera hardware,
/// making it suitable for use in iOS Simulator or automated testing.
@MainActor
@Observable
public final class URSimulatedScanState {
    /// The UR being simulated.
    public let ur: UR

    /// The scan state receiving decoded parts.
    public weak var scanState: URScanState?

    /// Maximum fragment length for encoding.
    public let maxFragmentLen: Int

    /// Seconds between each simulated scan frame.
    public var secondsPerFrame: Double

    /// Which fragment to start scanning from.
    public let startFragment: StartFragment

    /// The current QR part as uppercase UTF-8 bytes (suitable for QR rendering).
    public private(set) var currentPart: Data = Data()

    /// Fragment state indicators for the progress bar.
    public private(set) var fragmentStates: [URFragmentBar.FragmentState] = [.off]

    /// Whether the simulated scan loop is currently running.
    public private(set) var isRunning: Bool = false

    private var encoder: MultipartEncoder!
    private var partsCount: Int = 0
    private var currentSequence: Int = 0
    private var timerTask: Task<Void, Never>?

    public init(
        ur: UR,
        scanState: URScanState,
        maxFragmentLen: Int,
        secondsPerFrame: Double = 0.5,
        startFragment: StartFragment = .first
    ) {
        self.ur = ur
        self.scanState = scanState
        self.maxFragmentLen = maxFragmentLen
        self.secondsPerFrame = secondsPerFrame
        self.startFragment = startFragment
        initEncoder()
    }

    /// Starts the simulated scan loop.
    public func run() {
        timerTask?.cancel()
        isRunning = true
        timerTask = Task { [weak self] in
            while !Task.isCancelled {
                guard let self else { break }
                self.emitAndDeliver()

                // Auto-stop on successful decode
                if case .ur = self.scanState?.lastResult {
                    self.isRunning = false
                    return
                }

                try? await Task.sleep(for: .seconds(self.secondsPerFrame))
            }
        }
    }

    /// Stops the simulated scan loop.
    public func stop() {
        timerTask?.cancel()
        timerTask = nil
        isRunning = false
    }

    /// Restarts the encoder and scan loop from the configured start fragment.
    public func restart() {
        stop()
        scanState?.restart()
        initEncoder()
        run()
    }

    // MARK: - Private

    private func initEncoder() {
        encoder = try! MultipartEncoder(ur, maxFragmentLen)
        partsCount = encoder.partsCount
        currentSequence = 0
        advanceToStart()
    }

    private func advanceToStart() {
        let skipCount: Int
        switch startFragment {
        case .first:
            skipCount = 0
        case .index(let i):
            precondition(i >= 0 && i < partsCount, "Start index \(i) out of range [0, \(partsCount))")
            skipCount = i
        case .random:
            skipCount = Int.random(in: 0..<partsCount)
        }

        for _ in 0..<skipCount {
            _ = try! encoder.nextPart()
        }
    }

    private func emitAndDeliver() {
        let partString = try! encoder.nextPart()
        currentSequence = encoder.currentIndex
        currentPart = partString.uppercased().utf8Data

        // Update fragment state display (same logic as URDisplayState)
        if currentSequence <= partsCount {
            let fragmentIndex = currentSequence - 1
            fragmentStates = (0..<partsCount).map { i in
                i == fragmentIndex ? .on : .off
            }
        } else {
            fragmentStates = Array(repeating: .on, count: partsCount)
        }

        // Deliver to scan state
        scanState?.receiveCodes(Swift.Set([partString]))
    }
}
