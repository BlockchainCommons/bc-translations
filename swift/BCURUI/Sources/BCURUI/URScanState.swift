import SwiftUI
import UIKit
import BCUR
import Observation

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
    public let hapticFeedback: Bool

    private var decoder: MultipartDecoder
    private var hasReceivedFirstPart: Bool = false

    private let impactGenerator: UIImpactFeedbackGenerator?
    private let notificationGenerator: UINotificationFeedbackGenerator?

    public init(hapticFeedback: Bool = true) {
        self.hapticFeedback = hapticFeedback
        self.decoder = MultipartDecoder()
        if hapticFeedback {
            self.impactGenerator = UIImpactFeedbackGenerator(style: .light)
            self.notificationGenerator = UINotificationFeedbackGenerator()
        } else {
            self.impactGenerator = nil
            self.notificationGenerator = nil
        }
    }

    public func restart() {
        decoder = MultipartDecoder()
        hasReceivedFirstPart = false
        lastResult = nil
        if hapticFeedback {
            impactGenerator?.prepare()
            notificationGenerator?.prepare()
        }
    }

    public func receiveCodes(_ codes: Swift.Set<String>) {
        for code in codes {
            processCode(code)
        }
    }

    public func receiveError(_ error: Error) {
        lastResult = .failure(error)
        if hapticFeedback {
            notificationGenerator?.notificationOccurred(.error)
        }
    }

    /// Signals a successful non-QR scan result (e.g., text recognition match).
    ///
    /// Fires success haptic feedback but does not set `lastResult` — the host
    /// manages its own result state for non-QR outcomes.
    public func completeWithSuccess() {
        if hapticFeedback {
            notificationGenerator?.notificationOccurred(.success)
        }
    }

    /// Signals a failed non-QR scan result.
    ///
    /// Sets `lastResult` to `.failure(error)` and fires error haptic feedback.
    public func completeWithFailure(_ error: Error) {
        lastResult = .failure(error)
        if hapticFeedback {
            notificationGenerator?.notificationOccurred(.error)
        }
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
                if hapticFeedback {
                    notificationGenerator?.notificationOccurred(.success)
                }
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
                    if hapticFeedback {
                        notificationGenerator?.notificationOccurred(.success)
                    }
                }
            } else {
                lastResult = .progress(progress)
                if hapticFeedback && hasReceivedFirstPart {
                    impactGenerator?.impactOccurred()
                }
            }
        } catch {
            if hasReceivedFirstPart {
                lastResult = .reject
            } else {
                lastResult = .failure(error)
                if hapticFeedback {
                    notificationGenerator?.notificationOccurred(.error)
                }
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
