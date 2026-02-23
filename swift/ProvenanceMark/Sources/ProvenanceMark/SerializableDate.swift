import Foundation

// MARK: - UTC Calendar

/// A Gregorian calendar fixed to the UTC time zone, used for all date
/// component extraction and construction in provenance mark serialization.
private let utcCalendar: Calendar = {
    var cal = Calendar(identifier: .gregorian)
    cal.timeZone = TimeZone(identifier: "UTC")!
    return cal
}()

// MARK: - 2-byte date serialization

/// Year/month/day packed into 16 bits:
///   `(yy << 9) | (month << 5) | day`
/// where `yy = year - 2023` (valid range 0...127 → years 2023...2150).

public extension Date {
    /// Serializes the date as a 2-byte big-endian value encoding year, month,
    /// and day. Year must be in the range 2023...2150.
    func serialize2Bytes() throws -> [UInt8] {
        let components = utcCalendar.dateComponents([.year, .month, .day], from: self)
        let year = components.year!
        let month = components.month!
        let day = components.day!

        let yy = year - 2023
        guard (0..<128).contains(yy) else {
            throw ProvenanceMarkError.yearOutOfRange(year: year)
        }
        guard (1...12).contains(month), (1...31).contains(day) else {
            throw ProvenanceMarkError.invalidMonthOrDay(year: year, month: month, day: day)
        }

        let value = UInt16(yy) << 9 | UInt16(month) << 5 | UInt16(day)
        return [UInt8(value >> 8), UInt8(value & 0xFF)]
    }

    /// Deserializes a 2-byte big-endian packed date.
    static func deserialize2Bytes(_ bytes: [UInt8]) throws -> Date {
        precondition(bytes.count == 2)
        let value = UInt16(bytes[0]) << 8 | UInt16(bytes[1])
        let day = Int(value & 0b11111)
        let month = Int((value >> 5) & 0b1111)
        let yy = Int((value >> 9) & 0b1111111)
        let year = yy + 2023

        let validDays = rangeOfDaysInMonth(year: year, month: month)
        guard (1...12).contains(month), validDays.contains(day) else {
            throw ProvenanceMarkError.invalidMonthOrDay(year: year, month: month, day: day)
        }

        var comps = DateComponents()
        comps.year = year
        comps.month = month
        comps.day = day
        comps.hour = 0
        comps.minute = 0
        comps.second = 0

        guard let date = utcCalendar.date(from: comps) else {
            throw ProvenanceMarkError.invalidDate(
                details: "Cannot construct date \(year)-\(String(format: "%02d", month))-\(String(format: "%02d", day))")
        }
        return date
    }
}

// MARK: - 4-byte date serialization

/// Seconds since 2001-01-01 00:00:00 UTC as a big-endian UInt32.
/// Foundation's `timeIntervalSinceReferenceDate` uses the same epoch.

public extension Date {
    /// Serializes the date as 4 big-endian bytes representing seconds since
    /// the reference date (2001-01-01 00:00:00 UTC).
    func serialize4Bytes() throws -> [UInt8] {
        let seconds = timeIntervalSinceReferenceDate
        guard seconds >= 0, seconds <= Double(UInt32.max) else {
            throw ProvenanceMarkError.dateOutOfRange(
                details: "seconds value too large for UInt32")
        }
        let n = UInt32(seconds)
        return [
            UInt8((n >> 24) & 0xFF),
            UInt8((n >> 16) & 0xFF),
            UInt8((n >> 8) & 0xFF),
            UInt8(n & 0xFF),
        ]
    }

    /// Deserializes 4 big-endian bytes as seconds since the reference date.
    static func deserialize4Bytes(_ bytes: [UInt8]) throws -> Date {
        precondition(bytes.count == 4)
        let n = UInt32(bytes[0]) << 24
            | UInt32(bytes[1]) << 16
            | UInt32(bytes[2]) << 8
            | UInt32(bytes[3])
        return Date(timeIntervalSinceReferenceDate: TimeInterval(n))
    }
}

// MARK: - 6-byte date serialization

/// Milliseconds since 2001-01-01 00:00:00 UTC as a big-endian 48-bit value.
/// Maximum representable value: 0xe5940a78a7ff.

public extension Date {
    /// Serializes the date as 6 big-endian bytes representing milliseconds since
    /// the reference date (2001-01-01 00:00:00 UTC).
    func serialize6Bytes() throws -> [UInt8] {
        let milliseconds = timeIntervalSinceReferenceDate * 1000.0
        guard milliseconds >= 0 else {
            throw ProvenanceMarkError.dateOutOfRange(
                details: "milliseconds value too large for UInt64")
        }
        let n = UInt64(milliseconds)
        guard n <= 0xe5940a78a7ff else {
            throw ProvenanceMarkError.dateOutOfRange(
                details: "date exceeds maximum representable value")
        }
        let fullBytes = [
            UInt8((n >> 56) & 0xFF),
            UInt8((n >> 48) & 0xFF),
            UInt8((n >> 40) & 0xFF),
            UInt8((n >> 32) & 0xFF),
            UInt8((n >> 24) & 0xFF),
            UInt8((n >> 16) & 0xFF),
            UInt8((n >> 8) & 0xFF),
            UInt8(n & 0xFF),
        ]
        // Take the last 6 bytes (drop the leading 2 zero bytes).
        return Array(fullBytes[2...7])
    }

    /// Deserializes 6 big-endian bytes as milliseconds since the reference date.
    static func deserialize6Bytes(_ bytes: [UInt8]) throws -> Date {
        precondition(bytes.count == 6)
        var fullBytes = [UInt8](repeating: 0, count: 8)
        fullBytes[2] = bytes[0]
        fullBytes[3] = bytes[1]
        fullBytes[4] = bytes[2]
        fullBytes[5] = bytes[3]
        fullBytes[6] = bytes[4]
        fullBytes[7] = bytes[5]

        let n = UInt64(fullBytes[0]) << 56
            | UInt64(fullBytes[1]) << 48
            | UInt64(fullBytes[2]) << 40
            | UInt64(fullBytes[3]) << 32
            | UInt64(fullBytes[4]) << 24
            | UInt64(fullBytes[5]) << 16
            | UInt64(fullBytes[6]) << 8
            | UInt64(fullBytes[7])

        guard n <= 0xe5940a78a7ff else {
            throw ProvenanceMarkError.dateOutOfRange(
                details: "date exceeds maximum representable value")
        }

        let seconds = TimeInterval(n) / 1000.0
        return Date(timeIntervalSinceReferenceDate: seconds)
    }
}

// MARK: - Day-range helper

/// Returns the valid range of days (1-based) for the given year and month.
///
/// For example, `rangeOfDaysInMonth(year: 2024, month: 2)` returns `1..<30`
/// (February in a leap year has 29 days).
public func rangeOfDaysInMonth(year: Int, month: Int) -> Range<Int> {
    var comps = DateComponents()
    if month == 12 {
        comps.year = year + 1
        comps.month = 1
    } else {
        comps.year = year
        comps.month = month + 1
    }
    comps.day = 1
    comps.hour = 0
    comps.minute = 0
    comps.second = 0

    let firstOfNext = utcCalendar.date(from: comps)!
    let lastDay = utcCalendar.component(.day,
        from: utcCalendar.date(byAdding: .day, value: -1, to: firstOfNext)!)
    return 1..<(lastDay + 1)
}
