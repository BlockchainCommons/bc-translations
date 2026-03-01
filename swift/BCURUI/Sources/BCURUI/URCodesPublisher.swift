import Foundation

/// A handler that receives scanned QR code sets from the video session.
public protocol URCodesReceiver: AnyObject, Sendable {
    @MainActor func receiveCodes(_ codes: Swift.Set<String>)
    @MainActor func receiveError(_ error: Error)
}
