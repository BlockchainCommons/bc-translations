import Foundation
import UIKit

/// A logo image to overlay on the center of a QR code.
///
/// The logo is pre-rendered at construction time and cached as a `UIImage`.
/// During QR compositing, it is scaled to fit the calculated logo area.
public struct QRLogo: Sendable {
    /// The pre-rendered logo image.
    let image: UIImage

    /// The desired logo width as a fraction of the QR code width (0.0...1.0).
    public let requestedFraction: CGFloat

    /// Create a logo from SVG data.
    ///
    /// The SVG is rasterized once at 512x512 and cached for reuse across QR frames.
    ///
    /// - Parameters:
    ///   - svgData: Raw SVG file data.
    ///   - fraction: Desired logo width as a fraction of the QR code width. Default 0.25.
    public init(svgData: Data, fraction: CGFloat = 0.25) {
        self.requestedFraction = fraction.clamped(to: 0.01...0.99)
        self.image = Self.renderSVG(svgData, size: 512)
    }

    /// Create a logo from a pre-rendered image.
    ///
    /// - Parameters:
    ///   - image: A `UIImage` to use as the logo.
    ///   - fraction: Desired logo width as a fraction of the QR code width. Default 0.25.
    public init(image: UIImage, fraction: CGFloat = 0.25) {
        self.requestedFraction = fraction.clamped(to: 0.01...0.99)
        self.image = image
    }

    // MARK: - SVG Rendering via CoreSVG

    private static func renderSVG(_ data: Data, size: Int) -> UIImage {
        let cgSize = CGSize(width: size, height: size)
        let renderer = UIGraphicsImageRenderer(size: cgSize)
        return renderer.image { context in
            // Try CoreSVG (CGSVGDocument) for SVG rendering.
            // These symbols are in CoreGraphics public headers since Xcode 14.
            if renderSVGViaCoreGraphics(data, size: cgSize, context: context.cgContext) {
                // drawing happened inside the helper
            } else {
                // Fallback: treat SVG data as a PDF/image if CoreSVG unavailable
                if let img = UIImage(data: data) {
                    img.draw(in: CGRect(origin: .zero, size: cgSize))
                }
            }
        }
    }

    private static func renderSVGViaCoreGraphics(_ data: Data, size: CGSize, context: CGContext) -> Bool {
        // CGSVGDocument C API (available iOS 13+, in CoreGraphics headers since Xcode 14)
        typealias CreateFunc = @convention(c) (CFData, CFDictionary?) -> Unmanaged<AnyObject>?
        typealias DrawFunc = @convention(c) (AnyObject, CGContext) -> Void
        typealias GetSizeFunc = @convention(c) (AnyObject) -> CGSize

        guard let createSym = dlsym(dlopen(nil, RTLD_LAZY), "CGSVGDocumentCreateFromData"),
              let drawSym = dlsym(dlopen(nil, RTLD_LAZY), "CGSVGDocumentRenderInContext"),
              let getSizeSym = dlsym(dlopen(nil, RTLD_LAZY), "CGSVGDocumentGetCanvasSize") else {
            return false
        }

        let create = unsafeBitCast(createSym, to: CreateFunc.self)
        let draw = unsafeBitCast(drawSym, to: DrawFunc.self)
        let getSize = unsafeBitCast(getSizeSym, to: GetSizeFunc.self)

        guard let documentRef = create(data as CFData, nil) else { return false }
        let document = documentRef.takeRetainedValue()

        var svgSize = getSize(document)
        // Fallback: if no explicit width/height, parse the viewBox
        if svgSize.width <= 0 || svgSize.height <= 0 {
            guard let vb = parseViewBox(from: data) else { return false }
            svgSize = vb
        }

        // Scale SVG to fill the target size, preserving aspect ratio
        let scaleX = size.width / svgSize.width
        let scaleY = size.height / svgSize.height
        let scale = min(scaleX, scaleY)

        let scaledWidth = svgSize.width * scale
        let scaledHeight = svgSize.height * scale
        let offsetX = (size.width - scaledWidth) / 2
        let offsetY = (size.height - scaledHeight) / 2

        context.saveGState()
        // UIKit coordinate system: flip Y for CoreGraphics rendering
        context.translateBy(x: offsetX, y: size.height - offsetY)
        context.scaleBy(x: scale, y: -scale)
        draw(document, context)
        context.restoreGState()

        return true
    }

    /// Parse the viewBox attribute from SVG data to determine dimensions
    /// when explicit width/height are absent.
    private static func parseViewBox(from data: Data) -> CGSize? {
        guard let svg = String(data: data, encoding: .utf8),
              let range = svg.range(of: #"viewBox="([^"]+)""#, options: .regularExpression) else {
            return nil
        }
        let attr = svg[range]
        guard let open = attr.firstIndex(of: "\""),
              let close = attr[attr.index(after: open)...].firstIndex(of: "\"") else {
            return nil
        }
        let values = attr[attr.index(after: open)..<close]
            .split(separator: " ")
            .compactMap { Double($0) }
        guard values.count == 4, values[2] > 0, values[3] > 0 else { return nil }
        return CGSize(width: values[2], height: values[3])
    }
}

// MARK: - Helpers

extension Comparable {
    func clamped(to range: ClosedRange<Self>) -> Self {
        min(max(self, range.lowerBound), range.upperBound)
    }
}
