import Testing
import Foundation
@testable import ProvenanceMark

struct DateTests {
    /// Creates a UTC date from the given components.
    private func utcDate(
        year: Int, month: Int, day: Int,
        hour: Int = 0, minute: Int = 0, second: Int = 0,
        nanosecond: Int = 0
    ) -> Date {
        var calendar = Calendar(identifier: .gregorian)
        calendar.timeZone = TimeZone(identifier: "UTC")!
        return calendar.date(from: DateComponents(
            year: year, month: month, day: day,
            hour: hour, minute: minute, second: second,
            nanosecond: nanosecond
        ))!
    }

    // MARK: - 2-byte date tests

    @Test func twoByteBaseDate() throws {
        // Base date: 2023-06-20 -> serialized "00d4"
        let baseDate = utcDate(year: 2023, month: 6, day: 20)
        let serialized = try baseDate.serialize2Bytes()
        #expect(serialized.hex == "00d4")
        let deserialized = try Date.deserialize2Bytes(serialized)
        #expect(baseDate == deserialized)
    }

    @Test func twoByteMinDate() throws {
        // Minimum date: 2023-01-01
        let minSerialized: [UInt8] = [0x00, 0x21]
        let minDate = utcDate(year: 2023, month: 1, day: 1)
        let deserializedMin = try Date.deserialize2Bytes(minSerialized)
        #expect(minDate == deserializedMin)
    }

    @Test func twoByteMaxDate() throws {
        // Maximum date: 2150-12-31
        let maxSerialized: [UInt8] = [0xff, 0x9f]
        let deserializedMax = try Date.deserialize2Bytes(maxSerialized)
        let expectedMax = utcDate(year: 2150, month: 12, day: 31)
        #expect(deserializedMax == expectedMax)
    }

    @Test func twoByteInvalidDate() throws {
        // Invalid date: 2023-02-30 (February never has 30 days)
        let invalidSerialized: [UInt8] = [0x00, 0x5e]
        #expect(throws: ProvenanceMarkError.self) {
            try Date.deserialize2Bytes(invalidSerialized)
        }
    }

    // MARK: - 4-byte date tests

    @Test func fourByteBaseDate() throws {
        // Base date: 2023-06-20T12:34:56Z -> serialized "2a41d470"
        let baseDate = utcDate(year: 2023, month: 6, day: 20, hour: 12, minute: 34, second: 56)
        let serialized = try baseDate.serialize4Bytes()
        #expect(serialized == [0x2a, 0x41, 0xd4, 0x70])
        let deserialized = try Date.deserialize4Bytes(serialized)
        #expect(baseDate == deserialized)
    }

    @Test func fourByteMinDate() throws {
        // Min: 2001-01-01T00:00:00Z -> 0x00000000
        let minSerialized: [UInt8] = [0x00, 0x00, 0x00, 0x00]
        let minDate = utcDate(year: 2001, month: 1, day: 1)
        let deserializedMin = try Date.deserialize4Bytes(minSerialized)
        #expect(minDate == deserializedMin)
    }

    @Test func fourByteMaxDate() throws {
        // Max: 0xffffffff -> 2137-02-07T06:28:15Z
        let maxSerialized: [UInt8] = [0xff, 0xff, 0xff, 0xff]
        let deserializedMax = try Date.deserialize4Bytes(maxSerialized)
        let expectedMax = utcDate(year: 2137, month: 2, day: 7, hour: 6, minute: 28, second: 15)
        #expect(deserializedMax == expectedMax)
    }

    // MARK: - 6-byte date tests

    @Test func sixByteBaseDate() throws {
        // Base date: 2023-06-20T12:34:56.789Z -> serialized "00a51125d895"
        let baseDate = utcDate(
            year: 2023, month: 6, day: 20,
            hour: 12, minute: 34, second: 56,
            nanosecond: 789_000_000
        )
        let serialized = try baseDate.serialize6Bytes()
        #expect(serialized == [0x00, 0xa5, 0x11, 0x25, 0xd8, 0x95])
        let deserialized = try Date.deserialize6Bytes(serialized)
        // Compare with millisecond precision (truncate sub-millisecond differences)
        let baseMs = (baseDate.timeIntervalSinceReferenceDate * 1000.0).rounded(.down)
        let deserializedMs = (deserialized.timeIntervalSinceReferenceDate * 1000.0).rounded(.down)
        #expect(baseMs == deserializedMs)
    }

    @Test func sixByteMinDate() throws {
        // Min: 2001-01-01T00:00:00Z -> 0x000000000000
        let minSerialized: [UInt8] = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00]
        let minDate = utcDate(year: 2001, month: 1, day: 1)
        let deserializedMin = try Date.deserialize6Bytes(minSerialized)
        #expect(minDate == deserializedMin)
    }

    @Test func sixByteMaxDate() throws {
        // Max: 0xe5940a78a7ff -> 9999-12-31T23:59:59.999Z
        let maxSerialized: [UInt8] = [0xe5, 0x94, 0x0a, 0x78, 0xa7, 0xff]
        let deserializedMax = try Date.deserialize6Bytes(maxSerialized)
        let expectedMax = utcDate(
            year: 9999, month: 12, day: 31,
            hour: 23, minute: 59, second: 59,
            nanosecond: 999_000_000
        )
        // Compare with millisecond precision
        let expectedMs = (expectedMax.timeIntervalSinceReferenceDate * 1000.0).rounded(.down)
        let deserializedMs = (deserializedMax.timeIntervalSinceReferenceDate * 1000.0).rounded(.down)
        #expect(expectedMs == deserializedMs)
    }

    @Test func sixByteInvalidExceedsMax() throws {
        // Invalid: exceeds max representable value
        let invalid: [UInt8] = [0xe5, 0x94, 0x0a, 0x78, 0xa8, 0x00]
        #expect(throws: ProvenanceMarkError.self) {
            try Date.deserialize6Bytes(invalid)
        }
    }
}
