import CoreGraphics

/// A block of text recognized by the camera's text recognition pipeline.
///
/// Bounding boxes use normalized coordinates (0-1) with a top-left origin,
/// matching UIKit's coordinate system.
public struct URRecognizedText: Sendable {
    /// The recognized text string.
    public let text: String

    /// Normalized bounding box (0-1, top-left origin) of the text in the camera frame.
    public let boundingBox: CGRect

    /// Recognition confidence (0-1).
    public let confidence: Float

    public init(text: String, boundingBox: CGRect, confidence: Float) {
        self.text = text
        self.boundingBox = boundingBox
        self.confidence = confidence
    }
}
