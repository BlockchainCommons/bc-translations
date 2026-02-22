import BCCrypto
import BCUR
import BCTags
import DCBOR
import Foundation
import zlib

public struct Compressed: Equatable, Sendable {
    private let checksumValue: UInt32
    private let decompressedSizeValue: Int
    private let compressedDataValue: Data
    private let digestValue: Digest?

    public init(
        checksum: UInt32,
        decompressedSize: Int,
        compressedData: some DataProtocol,
        digest: Digest?
    ) throws(BCComponentsError) {
        let compressedData = Data(compressedData)
        if compressedData.count > decompressedSize {
            throw .compression("compressed data is larger than decompressed size")
        }

        self.checksumValue = checksum
        self.decompressedSizeValue = decompressedSize
        self.compressedDataValue = compressedData
        self.digestValue = digest
    }

    public static func fromDecompressedData(
        _ decompressedData: some DataProtocol,
        digest: Digest?
    ) -> Compressed {
        let decompressedData = Data(decompressedData)
        let compressedData = (try? deflateRaw(decompressedData, level: 6)) ?? Data()
        let checksum = crc32(decompressedData)
        let decompressedSize = decompressedData.count
        let compressedSize = compressedData.count

        if compressedSize != 0 && compressedSize < decompressedSize {
            return try! Compressed(
                checksum: checksum,
                decompressedSize: decompressedSize,
                compressedData: compressedData,
                digest: digest
            )
        } else {
            return try! Compressed(
                checksum: checksum,
                decompressedSize: decompressedSize,
                compressedData: decompressedData,
                digest: digest
            )
        }
    }

    public func checksum() -> UInt32 {
        checksumValue
    }

    public func decompressedSize() -> Int {
        decompressedSizeValue
    }

    public func compressedData() -> Data {
        compressedDataValue
    }

    public func decompress() throws(BCComponentsError) -> Data {
        let compressedSize = compressedDataValue.count
        if compressedSize >= decompressedSizeValue {
            return compressedDataValue
        }

        let decompressed = try inflateRaw(compressedDataValue)
        if crc32(decompressed) != checksumValue {
            throw .compression("compressed data checksum mismatch")
        }

        return decompressed
    }

    public func compressedSize() -> Int {
        compressedDataValue.count
    }

    public func compressionRatio() -> Double {
        Double(compressedSize()) / Double(decompressedSizeValue)
    }

    public func digestOpt() -> Digest? {
        digestValue
    }

    public func hasDigest() -> Bool {
        digestValue != nil
    }
}

extension Compressed: DigestProvider {
    public func digest() -> Digest {
        digestValue!
    }
}

extension Compressed: CustomStringConvertible {
    public var description: String {
        debugDescription
    }
}

extension Compressed: CustomDebugStringConvertible {
    public var debugDescription: String {
        let ratio = compressionRatio()
        let ratioText = ratio.isNaN ? "NaN" : String(format: "%.2f", ratio)
        let digestText = digestOpt()?.shortDescription() ?? "None"
        return "Compressed(checksum: \(hexEncode(checksumValue.bigEndianData)), size: \(compressedSize())/\(decompressedSizeValue), ratio: \(ratioText), digest: \(digestText))"
    }
}

extension Compressed: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.compressed]
    }

    public var untaggedCBOR: CBOR {
        var elements: [CBOR] = [
            .unsigned(UInt64(checksumValue)),
            .unsigned(UInt64(decompressedSizeValue)),
            .bytes(compressedDataValue),
        ]
        if let digestValue {
            elements.append(digestValue.taggedCBOR)
        }
        return .array(elements)
    }
}

extension Compressed: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw CBORError.wrongType
        }
        if elements.count < 3 || elements.count > 4 {
            throw BCComponentsError.invalidData(
                dataType: "compressed",
                reason: "invalid number of elements in compressed"
            )
        }

        let checksum = try UInt32(cbor: elements[0])
        let decompressedSize = try Int(cbor: elements[1])
        let compressedData = try byteString(elements[2])
        let digest = try elements.count == 4 ? Digest(cbor: elements[3]) : nil
        try self.init(
            checksum: checksum,
            decompressedSize: decompressedSize,
            compressedData: compressedData,
            digest: digest
        )
    }
}

extension Compressed: URCodable {}

private func deflateRaw(
    _ input: Data,
    level: Int32
) throws(BCComponentsError) -> Data {
    var stream = z_stream()
    let initStatus = deflateInit2_(
        &stream,
        level,
        Z_DEFLATED,
        -MAX_WBITS,
        8,
        Z_DEFAULT_STRATEGY,
        zlibVersion(),
        Int32(MemoryLayout<z_stream>.size)
    )
    guard initStatus == Z_OK else {
        throw .compression("failed to initialize compressor")
    }
    defer { deflateEnd(&stream) }

    var source = [UInt8](input)
    stream.next_in = source.withUnsafeMutableBufferPointer { $0.baseAddress }
    stream.avail_in = uInt(source.count)

    var output = Data()
    var status = Z_OK
    var outBuffer = [UInt8](repeating: 0, count: 16_384)
    repeat {
        status = outBuffer.withUnsafeMutableBufferPointer { outPtr in
            stream.next_out = outPtr.baseAddress
            stream.avail_out = uInt(outPtr.count)
            return deflate(&stream, Z_FINISH)
        }

        guard status == Z_OK || status == Z_STREAM_END else {
            throw .compression("compression failed")
        }

        let produced = outBuffer.count - Int(stream.avail_out)
        if produced > 0 {
            output.append(contentsOf: outBuffer[0..<produced])
        }
    } while status != Z_STREAM_END

    return output
}

private func inflateRaw(_ input: Data) throws(BCComponentsError) -> Data {
    var stream = z_stream()
    let initStatus = inflateInit2_(
        &stream,
        -MAX_WBITS,
        zlibVersion(),
        Int32(MemoryLayout<z_stream>.size)
    )
    guard initStatus == Z_OK else {
        throw .compression("failed to initialize decompressor")
    }
    defer { inflateEnd(&stream) }

    var source = [UInt8](input)
    stream.next_in = source.withUnsafeMutableBufferPointer { $0.baseAddress }
    stream.avail_in = uInt(source.count)

    var output = Data()
    var outBuffer = [UInt8](repeating: 0, count: 16_384)

    while true {
        let status = outBuffer.withUnsafeMutableBufferPointer { outPtr in
            stream.next_out = outPtr.baseAddress
            stream.avail_out = uInt(outPtr.count)
            return inflate(&stream, Z_NO_FLUSH)
        }

        if status != Z_OK && status != Z_STREAM_END {
            throw .compression("corrupt compressed data")
        }

        let produced = outBuffer.count - Int(stream.avail_out)
        if produced > 0 {
            output.append(contentsOf: outBuffer[0..<produced])
        }

        if status == Z_STREAM_END {
            return output
        }

        if produced == 0 && stream.avail_in == 0 {
            throw .compression("corrupt compressed data")
        }
    }
}

private extension UInt32 {
    var bigEndianData: Data {
        var value = self.bigEndian
        return withUnsafeBytes(of: &value) { Data($0) }
    }
}
