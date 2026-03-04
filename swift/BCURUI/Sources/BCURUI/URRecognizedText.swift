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

    /// The rotation (in degrees, 0/90/180/270) at which the text was detected.
    public let rotation: Int

    public init(text: String, boundingBox: CGRect, confidence: Float, rotation: Int = 0) {
        self.text = text
        self.boundingBox = boundingBox
        self.confidence = confidence
        self.rotation = rotation
    }
}
