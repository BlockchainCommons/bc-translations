import SwiftUI
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

    private var decoder: MultipartDecoder
    private var expectedFragmentCount: Int?
    private var receivedCount: Int = 0
    private var hasReceivedFirstPart: Bool = false

    public init() {
        self.decoder = MultipartDecoder()
    }

    public func restart() {
        decoder = MultipartDecoder()
        expectedFragmentCount = nil
        receivedCount = 0
        hasReceivedFirstPart = false
        lastResult = nil
    }

    public func receiveCodes(_ codes: Swift.Set<String>) {
        for code in codes {
            processCode(code)
        }
    }

    public func receiveError(_ error: Error) {
        lastResult = .failure(error)
    }

    private var progress: URScanProgress {
        let count = expectedFragmentCount ?? 1
        let percent = count > 0 ? min(Double(receivedCount) / Double(count), 1.0) : 0.0
        let fragmentStates: [URFragmentBar.FragmentState] = (0..<count).map { i in
            if i < receivedCount {
                return .highlighted
            } else if i == receivedCount {
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
                return
            }

            // Multi-part UR
            if !hasReceivedFirstPart {
                expectedFragmentCount = extractFragmentCount(from: trimmed)
                hasReceivedFirstPart = true
            }

            try decoder.receive(trimmed)
            receivedCount += 1

            if decoder.isComplete {
                if let ur = try decoder.message() {
                    lastResult = .ur(ur)
                    restart()
                }
            } else {
                lastResult = .progress(progress)
            }
        } catch {
            if hasReceivedFirstPart {
                lastResult = .reject
            } else {
                lastResult = .failure(error)
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

    /// Extracts the total fragment count from a multipart UR sequence ID.
    private func extractFragmentCount(from ur: String) -> Int? {
        let withoutScheme = ur.dropFirst(3)
        let components = withoutScheme.split(separator: "/")
        guard components.count >= 2 else { return nil }
        let seqParts = components[1].split(separator: "-")
        guard seqParts.count == 2, let count = Int(seqParts[1]) else { return nil }
        return count
    }
}
