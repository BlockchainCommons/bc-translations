import Foundation

/// A handler that receives recognized text blocks from the video session's
/// text recognition pipeline.
public protocol URTextReceiver: AnyObject, Sendable {
    @MainActor func receiveRecognizedText(_ texts: [URRecognizedText])
}
