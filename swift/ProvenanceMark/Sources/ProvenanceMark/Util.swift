import Foundation

// MARK: - Hex encoding/decoding on Data

public extension Data {
    /// The lowercase hexadecimal representation of the data.
    var hex: String {
        map { String(format: "%02x", $0) }.joined()
    }

    /// Creates `Data` from a hexadecimal string.
    ///
    /// Returns `nil` if the string has an odd number of characters or contains
    /// non-hexadecimal characters.
    init?(hex: String) {
        let chars = Array(hex)
        guard chars.count % 2 == 0 else { return nil }
        var bytes = [UInt8]()
        bytes.reserveCapacity(chars.count / 2)
        for i in stride(from: 0, to: chars.count, by: 2) {
            guard let byte = UInt8(String(chars[i...i+1]), radix: 16) else {
                return nil
            }
            bytes.append(byte)
        }
        self.init(bytes)
    }
}

// MARK: - Hex encoding on [UInt8]

public extension Array where Element == UInt8 {
    /// The lowercase hexadecimal representation of the byte array.
    var hex: String {
        map { String(format: "%02x", $0) }.joined()
    }
}

// MARK: - Hex decoding to [UInt8]

public extension Array where Element == UInt8 {
    /// Creates a byte array from a hexadecimal string.
    ///
    /// Returns `nil` if the string has an odd number of characters or contains
    /// non-hexadecimal characters.
    init?(hex: String) {
        guard let data = Data(hex: hex) else { return nil }
        self = Array(data)
    }
}

// MARK: - ISO 8601 date helpers

/// A shared ISO 8601 date formatter for full datetime strings in UTC.
///
/// `ISO8601DateFormatter` is thread-safe (immutable after configuration),
/// but is not marked `Sendable` in Foundation, so we use `nonisolated(unsafe)`.
nonisolated(unsafe) private let iso8601DateTimeFormatter: ISO8601DateFormatter = {
    let formatter = ISO8601DateFormatter()
    formatter.formatOptions = [.withInternetDateTime]
    formatter.timeZone = TimeZone(identifier: "UTC")!
    return formatter
}()

/// A shared ISO 8601 date formatter for date-only strings in UTC.
nonisolated(unsafe) private let iso8601DateOnlyFormatter: ISO8601DateFormatter = {
    let formatter = ISO8601DateFormatter()
    formatter.formatOptions = [.withFullDate, .withDashSeparatorInDate]
    formatter.timeZone = TimeZone(identifier: "UTC")!
    return formatter
}()

/// Parses an ISO 8601 string into a `Date`.
///
/// Accepts both date-only format (`"2023-06-20"`) and full datetime format
/// (`"2023-06-20T12:00:00Z"`).
///
/// - Parameter string: The ISO 8601 date string.
/// - Returns: The parsed `Date`.
/// - Throws: `ProvenanceMarkError.invalidDate` if the string cannot be parsed.
public func dateFromISO8601(_ string: String) throws -> Date {
    // Try full datetime first, then date-only
    if let date = iso8601DateTimeFormatter.date(from: string) {
        return date
    }
    if let date = iso8601DateOnlyFormatter.date(from: string) {
        return date
    }
    throw ProvenanceMarkError.invalidDate(
        details: "cannot parse ISO 8601 date string: \(string)")
}

/// A shared Gregorian calendar in UTC for time-component checks.
private let utcCalendarForFormat: Calendar = {
    var cal = Calendar(identifier: .gregorian)
    cal.timeZone = TimeZone(identifier: "UTC")!
    return cal
}()

/// Formats a `Date` as an ISO 8601 string in UTC.
///
/// If the time is midnight (00:00:00), only the date part is shown
/// (`"2023-06-20"`). Otherwise, the full datetime is shown
/// (`"2023-06-20T12:00:00Z"`).
///
/// This matches the Rust `dcbor::Date` Display behavior.
///
/// - Parameter date: The date to format.
/// - Returns: The ISO 8601 string representation.
public func dateToISO8601(_ date: Date) -> String {
    let components = utcCalendarForFormat.dateComponents(
        [.hour, .minute, .second], from: date)
    if components.hour == 0 && components.minute == 0 && components.second == 0 {
        return iso8601DateOnlyFormatter.string(from: date)
    }
    return iso8601DateTimeFormatter.string(from: date)
}

// MARK: - Seed parsing

/// Parses a Base64-encoded string into a `ProvenanceSeed`.
///
/// - Parameter string: A Base64 string encoding exactly 32 bytes.
/// - Returns: The decoded `ProvenanceSeed`.
/// - Throws: `ProvenanceMarkError` if the string is not valid Base64 or
///   the decoded data is not 32 bytes.
public func parseSeed(_ string: String) throws -> ProvenanceSeed {
    guard let data = Data(base64Encoded: string) else {
        throw ProvenanceMarkError.base64("invalid base64 string")
    }
    return try ProvenanceSeed(slice: [UInt8](data))
}

/// Parses an ISO 8601 date string.
///
/// - Parameter string: An ISO 8601 date or datetime string.
/// - Returns: The parsed `Date`.
/// - Throws: `ProvenanceMarkError.invalidDate` if the string cannot be parsed.
public func parseDate(_ string: String) throws -> Date {
    try dateFromISO8601(string)
}
