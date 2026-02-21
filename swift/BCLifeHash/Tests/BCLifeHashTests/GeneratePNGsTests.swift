import CoreGraphics
import Foundation
import ImageIO
import UniformTypeIdentifiers
import Testing
@testable import BCLifeHash

private func writePNG(path: URL, width: Int, height: Int, rgb: [UInt8]) throws {
    let data = Data(rgb)
    guard let provider = CGDataProvider(data: data as NSData) else {
        Issue.record("Failed to create data provider for PNG")
        return
    }

    let colorSpace = CGColorSpaceCreateDeviceRGB()
    let bitmapInfo = CGBitmapInfo(rawValue: CGImageAlphaInfo.none.rawValue)

    guard let image = CGImage(
        width: width,
        height: height,
        bitsPerComponent: 8,
        bitsPerPixel: 24,
        bytesPerRow: width * 3,
        space: colorSpace,
        bitmapInfo: bitmapInfo,
        provider: provider,
        decode: nil,
        shouldInterpolate: false,
        intent: .defaultIntent
    ) else {
        Issue.record("Failed to create CGImage for PNG")
        return
    }

    guard let destination = CGImageDestinationCreateWithURL(
        path as CFURL,
        UTType.png.identifier as CFString,
        1,
        nil
    ) else {
        Issue.record("Failed to create PNG destination at \(path.path)")
        return
    }

    CGImageDestinationAddImage(destination, image, nil)
    #expect(CGImageDestinationFinalize(destination), "Failed to write PNG: \(path.path)")
}

private func packageRoot() -> URL {
    URL(fileURLWithPath: #filePath)
        .deletingLastPathComponent() // Tests/BCLifeHashTests
        .deletingLastPathComponent() // Tests
        .deletingLastPathComponent() // package root
}

@Test
func generatePNGs() throws {
    let versions: [(String, Version)] = [
        ("version1", .version1),
        ("version2", .version2),
        ("detailed", .detailed),
        ("fiducial", .fiducial),
        ("grayscale_fiducial", .grayscaleFiducial),
    ]

    let outDir = packageRoot().appendingPathComponent("out", isDirectory: true)
    let fileManager = FileManager.default

    for (name, version) in versions {
        let versionDir = outDir.appendingPathComponent(name, isDirectory: true)
        try fileManager.createDirectory(at: versionDir, withIntermediateDirectories: true)

        for i in 0..<100 {
            let image = Image.fromUTF8(String(i), version: version, moduleSize: 1, hasAlpha: false)
            let path = versionDir.appendingPathComponent("\(i).png")
            try writePNG(path: path, width: image.width, height: image.height, rgb: image.colors)
        }
    }
}
