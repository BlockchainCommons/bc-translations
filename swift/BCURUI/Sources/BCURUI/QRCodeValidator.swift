import Foundation
import CoreImage
import CoreImage.CIFilterBuiltins

/// Default maximum QR module count for reliable phone scanning (QR version 25).
public let defaultMaxQRModules = 117

/// Get the QR module count for a message at a given correction level.
public func qrModuleCount(
    for message: Data,
    correctionLevel: QRCorrectionLevel = .medium
) -> Int {
    let generator = CIFilter.qrCodeGenerator()
    generator.correctionLevel = correctionLevel.rawValue
    generator.message = message
    guard let output = generator.outputImage else { return 0 }
    return Int(output.extent.width)
}

/// Validate that a QR module count is within a density limit.
///
/// Throws ``QRGenerationError/qrCodeTooDense(moduleCount:maxModules:)``
/// if `moduleCount` exceeds `maxModules`.
public func checkQRDensity(
    moduleCount: Int,
    maxModules: Int = defaultMaxQRModules
) throws {
    if moduleCount > maxModules {
        throw QRGenerationError.qrCodeTooDense(
            moduleCount: moduleCount,
            maxModules: maxModules
        )
    }
}
