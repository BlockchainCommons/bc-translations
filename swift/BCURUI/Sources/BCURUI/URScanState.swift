import SwiftUI
import UIKit
import BCUR
import Observation
import AudioToolbox

public enum URScanResult: Sendable {
    /// A complete UR was decoded.
    case ur(UR)

    /// A non-UR QR code was read.
    case other(String)

    /// A part of a multi-part QR code was read.
    case progress(URScanProgress)

    /// A part of a multi-part QR code was rejected.
    case reject

    /// An error occurred that aborted the scan session.
    case failure(Error)
}

public struct URScanProgress: Sendable {
    public let estimatedPercentComplete: Double
    public let fragmentStates: [URFragmentBar.FragmentState]
}

/// Tracks and reports state of ongoing capture.
@MainActor
@Observable
public final class URScanState: URCodesReceiver {
    public private(set) var lastResult: URScanResult?

    public var feedbackConfig: URScanFeedbackConfig {
        didSet { reloadSounds() }
    }

    public var hapticFeedback: Bool { feedbackConfig.hapticEnabled }

    private var decoder: MultipartDecoder
    private var hasReceivedFirstPart: Bool = false

    private let impactGenerator = UIImpactFeedbackGenerator(style: .light)
    private let notificationGenerator = UINotificationFeedbackGenerator()

    @ObservationIgnored private var clickSoundID: SystemSoundID = 0
    @ObservationIgnored private var successSoundID: SystemSoundID = 0
    @ObservationIgnored private var failureSoundID: SystemSoundID = 0

    public init(feedbackConfig: URScanFeedbackConfig = .default) {
        self.feedbackConfig = feedbackConfig
        self.decoder = MultipartDecoder()
        loadSounds()
    }

    public convenience init(hapticFeedback: Bool) {
        self.init(feedbackConfig: .hapticOnly(hapticFeedback))
    }

    deinit {
        MainActor.assumeIsolated {
            disposeSounds()
        }
    }

    // MARK: - Sound Management

    private func loadSounds() {
        guard feedbackConfig.soundEnabled else { return }
        if let url = feedbackConfig.clickSoundURL {
            AudioServicesCreateSystemSoundID(url as CFURL, &clickSoundID)
        }
        if let url = feedbackConfig.successSoundURL {
            AudioServicesCreateSystemSoundID(url as CFURL, &successSoundID)
        }
        if let url = feedbackConfig.failureSoundURL {
            AudioServicesCreateSystemSoundID(url as CFURL, &failureSoundID)
        }
    }

    private func disposeSounds() {
        if clickSoundID != 0 {
            AudioServicesDisposeSystemSoundID(clickSoundID)
            clickSoundID = 0
        }
        if successSoundID != 0 {
            AudioServicesDisposeSystemSoundID(successSoundID)
            successSoundID = 0
        }
        if failureSoundID != 0 {
            AudioServicesDisposeSystemSoundID(failureSoundID)
            failureSoundID = 0
        }
    }

    private func reloadSounds() {
        disposeSounds()
        loadSounds()
    }

    private func playClick() {
        if feedbackConfig.soundEnabled && clickSoundID != 0 {
            AudioServicesPlaySystemSound(clickSoundID)
        }
    }

    private func playSuccess() {
        if feedbackConfig.soundEnabled && successSoundID != 0 {
            AudioServicesPlaySystemSound(successSoundID)
        }
    }

    private func playFailure() {
        if feedbackConfig.soundEnabled && failureSoundID != 0 {
            AudioServicesPlaySystemSound(failureSoundID)
        }
    }

    public func restart() {
        decoder = MultipartDecoder()
        hasReceivedFirstPart = false
        lastResult = nil
        if feedbackConfig.hapticEnabled {
            impactGenerator.prepare()
            notificationGenerator.prepare()
        }
    }

    public func receiveCodes(_ codes: Swift.Set<String>) {
        for code in codes {
            processCode(code)
        }
    }

    public func receiveError(_ error: Error) {
        lastResult = .failure(error)
        if feedbackConfig.hapticEnabled {
            notificationGenerator.notificationOccurred(.error)
        }
        playFailure()
    }

    /// Signals a successful non-QR scan result (e.g., text recognition match).
    ///
    /// Fires success haptic feedback but does not set `lastResult` — the host
    /// manages its own result state for non-QR outcomes.
    public func completeWithSuccess() {
        if feedbackConfig.hapticEnabled {
            notificationGenerator.notificationOccurred(.success)
        }
        playSuccess()
    }

    /// Signals a failed non-QR scan result.
    ///
    /// Sets `lastResult` to `.failure(error)` and fires error haptic feedback.
    public func completeWithFailure(_ error: Error) {
        lastResult = .failure(error)
        if feedbackConfig.hapticEnabled {
            notificationGenerator.notificationOccurred(.error)
        }
        playFailure()
    }

    private var progress: URScanProgress {
        let count = decoder.expectedFragmentCount > 0 ? decoder.expectedFragmentCount : 1
        let decodedCount = decoder.decodedFragmentCount
        let percent = min(
            (Double(decodedCount) + decoder.bufferContribution) / Double(count), 1.0
        )
        let filledCount = Int(percent * Double(count))
        let fragmentStates: [URFragmentBar.FragmentState] = (0..<count).map { i in
            if i < filledCount {
                return .highlighted
            } else if i == filledCount {
                return .on
            } else {
                return .off
            }
        }
        return URScanProgress(estimatedPercentComplete: percent, fragmentStates: fragmentStates)
    }

    private func processCode(_ code: String) {
        if case .ur = lastResult { return }

        let trimmed = code.trimmingCharacters(in: .whitespacesAndNewlines)

        guard trimmed.lowercased().hasPrefix("ur:") else {
            lastResult = .other(code)
            return
        }

        do {
            // Try single-part UR first
            if isSinglePartUR(trimmed) {
                let ur = try UR(urString: trimmed)
                lastResult = .ur(ur)
                if feedbackConfig.hapticEnabled {
                    notificationGenerator.notificationOccurred(.success)
                }
                playSuccess()
                return
            }

            // Multi-part UR
            if !hasReceivedFirstPart {
                hasReceivedFirstPart = true
            }

            try decoder.receive(trimmed)

            if decoder.isComplete {
                if let ur = try decoder.message() {
                    lastResult = .ur(ur)
                    if feedbackConfig.hapticEnabled {
                        notificationGenerator.notificationOccurred(.success)
                    }
                    playSuccess()
                }
            } else {
                lastResult = .progress(progress)
                if feedbackConfig.hapticEnabled && hasReceivedFirstPart {
                    impactGenerator.impactOccurred()
                }
                if hasReceivedFirstPart {
                    playClick()
                }
            }
        } catch {
            if hasReceivedFirstPart {
                lastResult = .reject
            } else {
                lastResult = .failure(error)
                if feedbackConfig.hapticEnabled {
                    notificationGenerator.notificationOccurred(.error)
                }
                playFailure()
                restart()
            }
        }
    }

    /// Checks whether a UR string is single-part (no sequence component).
    private func isSinglePartUR(_ ur: String) -> Bool {
        let withoutScheme = ur.dropFirst(3)
        let components = withoutScheme.split(separator: "/")
        if components.count >= 2 {
            // Multi-part has "N-M" as second component
            let secondPart = components[1]
            let dashParts = secondPart.split(separator: "-")
            if dashParts.count == 2,
               dashParts[0].allSatisfy(\.isNumber),
               dashParts[1].allSatisfy(\.isNumber) {
                return false
            }
        }
        return true
    }

}
