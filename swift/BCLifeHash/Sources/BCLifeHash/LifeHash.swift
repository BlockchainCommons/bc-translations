import CryptoKit
import Foundation

/// The LifeHash version, which controls grid resolution, generation count,
/// and gradient/pattern selection.
public enum Version: Sendable {
    case version1
    case version2
    case detailed
    case fiducial
    case grayscaleFiducial
}

/// A raster image produced by the LifeHash algorithm.
///
/// Pixel data is stored as a flat array of component bytes in row-major order
/// (RGB or RGBA, depending on whether the image was generated with alpha).
public struct Image: Sendable {
    /// The width of the image in pixels.
    public let width: Int

    /// The height of the image in pixels.
    public let height: Int

    /// The raw pixel data as component bytes (RGB or RGBA, row-major).
    public let colors: [UInt8]

    public init(width: Int, height: Int, colors: [UInt8]) {
        self.width = width
        self.height = height
        self.colors = colors
    }
}

private func sha256(_ data: [UInt8]) -> [UInt8] {
    let digest = SHA256.hash(data: Data(data))
    return Array(digest)
}

private func makeImage(
    width: Int,
    height: Int,
    floatColors: [Double],
    moduleSize: Int,
    hasAlpha: Bool
) -> Image {
    precondition(moduleSize > 0, "Invalid module size")

    let scaledWidth = width * moduleSize
    let scaledHeight = height * moduleSize
    let resultComponents = hasAlpha ? 4 : 3
    let scaledCapacity = scaledWidth * scaledHeight * resultComponents

    var resultColors = Array(repeating: UInt8(0), count: scaledCapacity)

    for targetY in 0..<scaledWidth {
        for targetX in 0..<scaledHeight {
            let sourceX = targetX / moduleSize
            let sourceY = targetY / moduleSize
            let sourceOffset = (sourceY * width + sourceX) * 3

            let targetOffset = (targetY * scaledWidth + targetX) * resultComponents

            resultColors[targetOffset] = UInt8(clamped(floatColors[sourceOffset]) * 255.0)
            resultColors[targetOffset + 1] = UInt8(clamped(floatColors[sourceOffset + 1]) * 255.0)
            resultColors[targetOffset + 2] = UInt8(clamped(floatColors[sourceOffset + 2]) * 255.0)

            if hasAlpha {
                resultColors[targetOffset + 3] = 255
            }
        }
    }

    return Image(width: scaledWidth, height: scaledHeight, colors: resultColors)
}

extension Image {
    /// Creates a LifeHash image from a UTF-8 string.
    ///
    /// The string is converted to bytes and hashed before generation.
    ///
    /// - Parameters:
    ///   - utf8: The input string.
    ///   - version: The LifeHash version to use.
    ///   - moduleSize: Scale factor for each cell (1 = no scaling).
    ///   - hasAlpha: Whether to include an alpha channel in the output.
    public static func fromUTF8(
        _ utf8: String,
        version: Version,
        moduleSize: Int,
        hasAlpha: Bool
    ) -> Image {
        fromData(Array(utf8.utf8), version: version, moduleSize: moduleSize, hasAlpha: hasAlpha)
    }

    /// Creates a LifeHash image from raw byte data.
    ///
    /// The data is hashed before generation.
    ///
    /// - Parameters:
    ///   - data: The input bytes.
    ///   - version: The LifeHash version to use.
    ///   - moduleSize: Scale factor for each cell (1 = no scaling).
    ///   - hasAlpha: Whether to include an alpha channel in the output.
    public static func fromData(
        _ data: [UInt8],
        version: Version,
        moduleSize: Int,
        hasAlpha: Bool
    ) -> Image {
        let digest = sha256(data)
        return fromDigest(digest, version: version, moduleSize: moduleSize, hasAlpha: hasAlpha)
    }

    /// Creates a LifeHash image from a 32-byte SHA-256 digest.
    ///
    /// - Parameters:
    ///   - digest: A 32-byte SHA-256 digest.
    ///   - version: The LifeHash version to use.
    ///   - moduleSize: Scale factor for each cell (1 = no scaling).
    ///   - hasAlpha: Whether to include an alpha channel in the output.
    public static func fromDigest(
        _ digest: [UInt8],
        version: Version,
        moduleSize: Int,
        hasAlpha: Bool
    ) -> Image {
        precondition(digest.count == 32, "Digest must be 32 bytes")

        let length: Int
        let maxGenerations: Int

        switch version {
        case .version1, .version2:
            length = 16
            maxGenerations = 150
        case .detailed, .fiducial, .grayscaleFiducial:
            length = 32
            maxGenerations = 300
        }

        var currentCellGrid = CellGrid(width: length, height: length)
        var nextCellGrid = CellGrid(width: length, height: length)
        var currentChangeGrid = ChangeGrid(width: length, height: length)
        var nextChangeGrid = ChangeGrid(width: length, height: length)

        switch version {
        case .version1:
            nextCellGrid.setData(digest)
        case .version2:
            let hashed = sha256(digest)
            nextCellGrid.setData(hashed)
        case .detailed, .fiducial, .grayscaleFiducial:
            var digest1 = digest
            if version == .grayscaleFiducial {
                digest1 = sha256(digest1)
            }
            let digest2 = sha256(digest1)
            let digest3 = sha256(digest2)
            let digest4 = sha256(digest3)

            var digestFinal = digest1
            digestFinal.append(contentsOf: digest2)
            digestFinal.append(contentsOf: digest3)
            digestFinal.append(contentsOf: digest4)

            nextCellGrid.setData(digestFinal)
        }

        nextChangeGrid.grid.setAll(true)

        var historySet: Set<Data> = []
        var history: [[UInt8]] = []

        while history.count < maxGenerations {
            swap(&currentCellGrid, &nextCellGrid)
            swap(&currentChangeGrid, &nextChangeGrid)

            let data = currentCellGrid.data()
            let hash = sha256(data)
            let hashData = Data(hash)
            if historySet.contains(hashData) {
                break
            }

            historySet.insert(hashData)
            history.append(data)

            currentCellGrid.nextGeneration(
                currentChangeGrid: currentChangeGrid,
                nextCellGrid: nextCellGrid,
                nextChangeGrid: nextChangeGrid
            )
        }

        let fracGrid = FracGrid(width: length, height: length)
        for (index, snapshot) in history.enumerated() {
            currentCellGrid.setData(snapshot)
            let frac = clamped(lerpFrom(0.0, Double(history.count), Double(index + 1)))
            fracGrid.overlay(currentCellGrid, frac)
        }

        if version != .version1 {
            var minValue = Double.infinity
            var maxValue = -Double.infinity

            fracGrid.grid.forEachCell { x, y in
                let value = fracGrid.grid[x, y]
                if value < minValue {
                    minValue = value
                }
                if value > maxValue {
                    maxValue = value
                }
            }

            let width = fracGrid.grid.width
            let height = fracGrid.grid.height

            for y in 0..<height {
                for x in 0..<width {
                    let value = fracGrid.grid[x, y]
                    let normalized = lerpFrom(minValue, maxValue, value)
                    fracGrid.grid[x, y] = normalized
                }
            }
        }

        let entropy = BitEnumerator(data: digest)

        switch version {
        case .detailed:
            _ = entropy.next()
        case .version2:
            _ = entropy.nextUInt2()
        case .version1, .fiducial, .grayscaleFiducial:
            break
        }

        let gradient = selectGradient(entropy, version)
        let pattern = selectPattern(entropy, version)
        let colorGrid = ColorGrid(fracGrid: fracGrid, gradient: gradient, pattern: pattern)

        return makeImage(
            width: colorGrid.grid.width,
            height: colorGrid.grid.height,
            floatColors: colorGrid.colors(),
            moduleSize: moduleSize,
            hasAlpha: hasAlpha
        )
    }
}
