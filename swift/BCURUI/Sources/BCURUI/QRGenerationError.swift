import Foundation

/// Errors related to QR code generation parameters.
public enum QRGenerationError: Error, Equatable, Sendable {
    /// The QR code's module count exceeds the recommended scanning limit.
    case qrCodeTooDense(moduleCount: Int, maxModules: Int)
    /// Fewer frames were requested than the message has fountain-coded fragments.
    case insufficientFrames(requested: Int, fragments: Int)
}

extension QRGenerationError: LocalizedError {
    public var errorDescription: String? {
        switch self {
        case .qrCodeTooDense(let moduleCount, let maxModules):
            "QR code too dense: \(moduleCount) modules exceeds limit of \(maxModules)"
        case .insufficientFrames(let requested, let fragments):
            "Insufficient frames: \(requested) requested but message requires at least \(fragments) fragments"
        }
    }
}
