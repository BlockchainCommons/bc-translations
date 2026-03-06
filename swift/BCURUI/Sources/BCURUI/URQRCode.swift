import Foundation
import UIKit
import CoreImage
import CoreImage.CIFilterBuiltins
import SwiftUI

/// Displays a (possibly animated) QR code, optionally with a centered logo overlay.
public struct URQRCode: View {
    @Binding var data: Data
    let foregroundColor: Color
    let backgroundColor: Color
    let logo: QRLogo?
    let quietZone: Int
    @Environment(\.colorScheme) private var colorScheme

    public init(
        data: Binding<Data>,
        foregroundColor: Color = .primary,
        backgroundColor: Color = .clear,
        logo: QRLogo? = nil,
        quietZone: Int = 1
    ) {
        self._data = data
        self.foregroundColor = foregroundColor
        self.backgroundColor = backgroundColor
        self.logo = logo
        self.quietZone = quietZone
    }

    public var body: some View {
        if let logo {
            // Baked-in colors, no template rendering (logo needs real colors)
            // maxModules is nil here so this won't throw.
            try! makeQRCode(
                data,
                correctionLevel: .high,
                foregroundColor: UIColor(foregroundColor),
                backgroundColor: UIColor(backgroundColor),
                logo: logo,
                quietZone: quietZone
            )
            .resizable()
            .aspectRatio(contentMode: .fit)
        } else {
            try! makeQRCode(data, correctionLevel: .low, quietZone: quietZone)
                .resizable()
                .aspectRatio(contentMode: .fit)
                .background(backgroundColor)
                .foregroundColor(foregroundColor)
        }
    }
}

public enum QRCorrectionLevel: String {
    case low = "L"
    case medium = "M"
    case quartile = "Q"
    case high = "H"
}

/// Create a SwiftUI `Image` from data, optionally with a logo overlay.
///
/// - Parameter maxModules: If non-nil, throws ``QRGenerationError/qrCodeTooDense(moduleCount:maxModules:)``
///   when the QR code exceeds this module count.
/// - Parameter quietZone: Number of background-colored modules around the QR data area (default 1).
public func makeQRCode(
    _ message: Data,
    correctionLevel: QRCorrectionLevel = .medium,
    foregroundColor: UIColor? = nil,
    backgroundColor: UIColor? = nil,
    logo: QRLogo? = nil,
    maxModules: Int? = nil,
    quietZone: Int = 1
) throws -> Image {
    let effectiveCorrection = logo != nil ? .high : correctionLevel
    let uiImage = try makeQRCodeImage(
        message,
        correctionLevel: effectiveCorrection,
        foregroundColor: foregroundColor ?? .black,
        backgroundColor: backgroundColor ?? .clear,
        logo: logo,
        maxModules: maxModules,
        quietZone: quietZone
    )
    if logo != nil {
        // Baked-in colors: no template rendering
        return Image(uiImage: uiImage)
            .interpolation(.none)
    } else {
        return Image(uiImage: uiImage)
            .renderingMode(.template)
            .interpolation(.none)
    }
}

/// Generate a `UIImage` QR code, optionally with a logo composited over the center.
///
/// - Parameter maxModules: If non-nil, throws ``QRGenerationError/qrCodeTooDense(moduleCount:maxModules:)``
///   when the QR code exceeds this module count.
/// - Parameter quietZone: Number of background-colored modules around the QR data area (default 1).
public func makeQRCodeImage(
    _ message: Data,
    correctionLevel: QRCorrectionLevel = .medium,
    foregroundColor: UIColor = .black,
    backgroundColor: UIColor = .clear,
    logo: QRLogo? = nil,
    maxModules: Int? = nil,
    quietZone: Int = 1
) throws -> UIImage {
    let qrCodeGenerator = CIFilter.qrCodeGenerator()
    qrCodeGenerator.correctionLevel = correctionLevel.rawValue
    qrCodeGenerator.message = message

    let falseColor = CIFilter.falseColor()
    falseColor.inputImage = qrCodeGenerator.outputImage
    falseColor.color0 = foregroundColor.ciColorValue
    falseColor.color1 = backgroundColor.ciColorValue

    let output = falseColor.outputImage!
    let moduleCount = Int(output.extent.width)

    // Check density if a limit was specified.
    if let maxModules {
        try checkQRDensity(moduleCount: moduleCount, maxModules: maxModules)
    }

    // Determine compositing resolution (crisp, module-aligned pixels)
    let targetSize = 512
    let totalModules = moduleCount + 2 * quietZone
    let pixelsPerModule = max(1, targetSize / totalModules)
    let compositingSize = totalModules * pixelsPerModule
    let qzPx = CGFloat(quietZone * pixelsPerModule)

    let cgImage = CIContext().createCGImage(output, from: output.extent)!
    let size = CGSize(width: compositingSize, height: compositingSize)
    let qrPixels = CGFloat(moduleCount * pixelsPerModule)
    let renderer = UIGraphicsImageRenderer(size: size)

    return renderer.image { ctx in
        let context = ctx.cgContext

        // 1. Fill entire image with background (covers quiet zone)
        let bgClearColor = backgroundColorForClearing(backgroundColor)
        context.setFillColor(backgroundColor.cgColor)
        context.fill(CGRect(origin: .zero, size: size))

        // 2. Draw QR modules offset by quiet zone with nearest-neighbor interpolation
        context.interpolationQuality = .none
        let qrRect = CGRect(x: qzPx, y: qzPx, width: qrPixels, height: qrPixels)
        UIImage(cgImage: cgImage).draw(in: qrRect)

        // 3. Logo overlay (if present and large enough)
        guard let logo else { return }
        let logoLayout = LogoLayout(moduleCount: moduleCount, requestedFraction: logo.requestedFraction, clearBorder: logo.clearBorder)
        guard logoLayout.logoModules >= 3 else { return }

        // Clear center area (within the QR data area)
        context.setFillColor(bgClearColor.cgColor)
        let centerModule = CGFloat(moduleCount) / 2.0

        switch logo.clearShape {
        case .square:
            let clearPixels = CGFloat(logoLayout.clearedModules * pixelsPerModule)
            let clearOrigin = qzPx + (qrPixels - clearPixels) / 2
            let clearRect = CGRect(x: clearOrigin, y: clearOrigin, width: clearPixels, height: clearPixels)
            context.fill(clearRect)

        case .circle:
            let radius = CGFloat(logoLayout.clearedModules) / 2.0
            let startModule = (moduleCount - logoLayout.clearedModules) / 2
            for row in 0..<logoLayout.clearedModules {
                for col in 0..<logoLayout.clearedModules {
                    let mx = CGFloat(startModule + col) + 0.5
                    let my = CGFloat(startModule + row) + 0.5
                    let dx = mx - centerModule
                    let dy = my - centerModule
                    if dx * dx + dy * dy <= radius * radius {
                        let px = qzPx + CGFloat((startModule + col) * pixelsPerModule)
                        let py = qzPx + CGFloat((startModule + row) * pixelsPerModule)
                        context.fill(CGRect(x: px, y: py, width: CGFloat(pixelsPerModule), height: CGFloat(pixelsPerModule)))
                    }
                }
            }
        }

        // Draw logo centered within the QR data area
        let logoPixels = CGFloat(logoLayout.logoModules * pixelsPerModule)
        let logoOrigin = qzPx + (qrPixels - logoPixels) / 2
        let logoRect = CGRect(x: logoOrigin, y: logoOrigin, width: logoPixels, height: logoPixels)

        context.interpolationQuality = .high
        logo.image.draw(in: logoRect)
    }
}

// MARK: - Logo Layout Calculation

struct LogoLayout {
    let logoModules: Int
    let clearedModules: Int

    init(moduleCount: Int, requestedFraction: CGFloat, clearBorder: Int) {
        // Calculate logo size in modules
        var logo = Int((CGFloat(moduleCount) * requestedFraction).rounded())
        // Make odd for symmetry
        if logo % 2 == 0 { logo += 1 }
        // Add clearBorder modules on each side
        var cleared = logo + 2 * clearBorder
        // Cap: cleared area must not exceed 40% of QR width
        let maxCleared = Int(floor(Double(moduleCount) * 0.40))
        if cleared > maxCleared {
            cleared = maxCleared
            logo = cleared - 2 * clearBorder
        }
        // Ensure logo has odd module count
        if logo % 2 == 0 { logo -= 1 }

        self.logoModules = max(0, logo)
        self.clearedModules = max(0, cleared)
    }
}

// MARK: - Helpers

private func backgroundColorForClearing(_ backgroundColor: UIColor) -> UIColor {
    var alpha: CGFloat = 0
    backgroundColor.getRed(nil, green: nil, blue: nil, alpha: &alpha)
    return alpha < 0.01 ? .white : backgroundColor
}

extension UIColor {
    var ciColorValue: CIColor {
        var red: CGFloat = 0, green: CGFloat = 0, blue: CGFloat = 0, alpha: CGFloat = 0
        getRed(&red, green: &green, blue: &blue, alpha: &alpha)
        return CIColor(red: red, green: green, blue: blue, alpha: alpha)
    }
}
