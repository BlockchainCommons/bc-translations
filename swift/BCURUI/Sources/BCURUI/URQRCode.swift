import Foundation
import UIKit
import CoreImage
import CoreImage.CIFilterBuiltins
import SwiftUI

/// Displays a (possibly animated) QR code.
public struct URQRCode: View {
    @Binding var data: Data
    let foregroundColor: Color
    let backgroundColor: Color

    public init(data: Binding<Data>, foregroundColor: Color = .primary, backgroundColor: Color = .clear) {
        self._data = data
        self.foregroundColor = foregroundColor
        self.backgroundColor = backgroundColor
    }

    public var body: some View {
        makeQRCode(data, correctionLevel: .low)
            .resizable()
            .aspectRatio(contentMode: .fit)
            .background(backgroundColor)
            .foregroundColor(foregroundColor)
    }
}

public enum QRCorrectionLevel: String {
    case low = "L"
    case medium = "M"
    case quartile = "Q"
    case high = "H"
}

public func makeQRCode(_ message: Data, correctionLevel: QRCorrectionLevel = .medium) -> Image {
    Image(uiImage: makeQRCodeImage(message, correctionLevel: correctionLevel))
        .renderingMode(.template)
        .interpolation(.none)
}

public func makeQRCodeImage(
    _ message: Data,
    correctionLevel: QRCorrectionLevel = .medium,
    foregroundColor: UIColor = .black,
    backgroundColor: UIColor = .clear
) -> UIImage {
    let qrCodeGenerator = CIFilter.qrCodeGenerator()
    qrCodeGenerator.correctionLevel = correctionLevel.rawValue
    qrCodeGenerator.message = message

    let falseColor = CIFilter.falseColor()
    falseColor.inputImage = qrCodeGenerator.outputImage
    falseColor.color0 = foregroundColor.ciColorValue
    falseColor.color1 = backgroundColor.ciColorValue

    let output = falseColor.outputImage!
    let cgImage = CIContext().createCGImage(output, from: output.extent)!
    return UIImage(cgImage: cgImage, scale: 1, orientation: .up)
}

extension UIColor {
    var ciColorValue: CIColor {
        var red: CGFloat = 0, green: CGFloat = 0, blue: CGFloat = 0, alpha: CGFloat = 0
        getRed(&red, green: &green, blue: &blue, alpha: &alpha)
        return CIColor(red: red, green: green, blue: blue, alpha: alpha)
    }
}
