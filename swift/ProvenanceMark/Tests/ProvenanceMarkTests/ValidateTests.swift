import Testing
import Foundation
import BCUR
import DCBOR
import BCTags
@testable import ProvenanceMark

@MainActor
struct ValidateTests {
    // MARK: - Helpers

    /// Creates `count` provenance marks at low resolution using the given
    /// passphrase, starting from 2023-06-20T12:00:00Z with one-day increments.
    private func createTestMarks(
        count: Int,
        resolution: ProvenanceMarkResolution,
        passphrase: String
    ) -> [ProvenanceMark] {
        BCTags.registerTags()

        var generator = ProvenanceMarkGenerator(
            resolution: resolution,
            passphrase: passphrase
        )

        var calendar = Calendar(identifier: .gregorian)
        calendar.timeZone = TimeZone(identifier: "UTC")!

        return (0..<count).map { i in
            let date = calendar.date(from: DateComponents(
                year: 2023, month: 6, day: 20 + i,
                hour: 12, minute: 0, second: 0
            ))!
            return generator.next(date: date)
        }
    }

    /// Creates a UTC date from components.
    private func utcDate(
        year: Int, month: Int, day: Int,
        hour: Int = 0, minute: Int = 0, second: Int = 0
    ) -> Date {
        var calendar = Calendar(identifier: .gregorian)
        calendar.timeZone = TimeZone(identifier: "UTC")!
        return calendar.date(from: DateComponents(
            year: year, month: month, day: day,
            hour: hour, minute: minute, second: second
        ))!
    }

    // MARK: - test_validate_empty

    @Test func testValidateEmpty() {
        let report = ProvenanceMark.validate([])

        let json = report.format(.jsonPretty)
        let expectedJSON = """
        {
          "chains" : [

          ],
          "marks" : [

          ]
        }
        """
        #expect(json == expectedJSON)

        // Test compact JSON format
        let jsonCompact = report.format(.jsonCompact)
        #expect(jsonCompact == #"{"chains":[],"marks":[]}"#)

        // Format should return empty string for empty report
        #expect(report.format(.text) == "")
    }

    // MARK: - test_validate_single_mark

    @Test func testValidateSingleMark() throws {
        let marks = createTestMarks(count: 1, resolution: .low, passphrase: "test")
        let report = ProvenanceMark.validate(marks)

        let json = report.format(.jsonPretty)
        let expectedJSON = """
        {
          "chains" : [
            {
              "chain_id" : "b16a7cbd",
              "has_genesis" : true,
              "marks" : [
                "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
              ],
              "sequences" : [
                {
                  "end_seq" : 0,
                  "marks" : [
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
                    }
                  ],
                  "start_seq" : 0
                }
              ]
            }
          ],
          "marks" : [
            "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
          ]
        }
        """
        #expect(json == expectedJSON)

        // Test compact JSON format
        let jsonCompact = report.format(.jsonCompact)
        #expect(jsonCompact == #"{"chains":[{"chain_id":"b16a7cbd","has_genesis":true,"marks":["ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"],"sequences":[{"end_seq":0,"marks":[{"issues":[],"mark":"ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"}],"start_seq":0}]}],"marks":["ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"]}"#)

        // Format should return empty string for single perfect chain
        #expect(report.format(.text) == "")
    }

    // MARK: - test_validate_valid_sequence

    @Test func testValidateValidSequence() throws {
        let marks = createTestMarks(count: 5, resolution: .low, passphrase: "test")
        let report = ProvenanceMark.validate(marks)

        let json = report.format(.jsonPretty)
        let expectedJSON = """
        {
          "chains" : [
            {
              "chain_id" : "b16a7cbd",
              "has_genesis" : true,
              "marks" : [
                "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
                "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
                "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
                "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
                "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
              ],
              "sequences" : [
                {
                  "end_seq" : 4,
                  "marks" : [
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
                    }
                  ],
                  "start_seq" : 0
                }
              ]
            }
          ],
          "marks" : [
            "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
            "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
            "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
            "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
            "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
          ]
        }
        """
        #expect(json == expectedJSON)

        // Format should return empty string for single perfect chain
        #expect(report.format(.text) == "")
    }

    // MARK: - test_validate_deduplication

    @Test func testValidateDeduplication() throws {
        let marks = createTestMarks(count: 3, resolution: .low, passphrase: "test")

        // Create duplicates
        var marksWithDups = marks
        marksWithDups.append(marks[0])
        marksWithDups.append(marks[1])
        marksWithDups.append(marks[0])

        let report = ProvenanceMark.validate(marksWithDups)

        let json = report.format(.jsonPretty)
        let expectedJSON = """
        {
          "chains" : [
            {
              "chain_id" : "b16a7cbd",
              "has_genesis" : true,
              "marks" : [
                "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
                "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
                "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
              ],
              "sequences" : [
                {
                  "end_seq" : 2,
                  "marks" : [
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
                    }
                  ],
                  "start_seq" : 0
                }
              ]
            }
          ],
          "marks" : [
            "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
            "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
            "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
          ]
        }
        """
        #expect(json == expectedJSON)

        // Format should return empty string - single perfect chain after deduplication
        #expect(report.format(.text) == "")
    }

    // MARK: - test_validate_multiple_chains

    @Test func testValidateMultipleChains() throws {
        let marks1 = createTestMarks(count: 3, resolution: .low, passphrase: "alice")
        let marks2 = createTestMarks(count: 3, resolution: .low, passphrase: "bob")

        var allMarks = marks1
        allMarks.append(contentsOf: marks2)

        let report = ProvenanceMark.validate(allMarks)

        // The text output should show both chains
        let text = report.format(.text)
        let expectedText = """
        Total marks: 6
        Chains: 2

        Chain 1: 7a9c3f5e
          0: 0d6e0afd (genesis mark)
          1: 6cd504e7
          2: dc07895c

        Chain 2: a33e10de
          0: c2a985ff (genesis mark)
          1: 5567cd24
          2: f759ad4c
        """
        #expect(text == expectedText)
    }

    // MARK: - test_validate_missing_genesis

    @Test func testValidateMissingGenesis() throws {
        let marks = createTestMarks(count: 5, resolution: .low, passphrase: "test")

        // Remove genesis mark (index 0)
        let marksNoGenesis = Array(marks.dropFirst())

        let report = ProvenanceMark.validate(marksNoGenesis)

        // Format should show missing genesis warning
        let text = report.format(.text)
        let expectedText = """
        Total marks: 4
        Chains: 1

        Chain 1: b16a7cbd
          Warning: No genesis mark found
          1: 1b806d6c
          2: b292f357
          3: 761a5e74
          4: 42d12de5
        """
        #expect(text == expectedText)
    }

    // MARK: - test_validate_sequence_gap

    @Test func testValidateSequenceGap() throws {
        let marks = createTestMarks(count: 5, resolution: .low, passphrase: "test")

        // Create a gap by removing mark at index 2 (sequence 2)
        let marksWithGap = [
            marks[0],
            marks[1],
            marks[3], // Gap: skips seq 2, this is seq 3
            marks[4],
        ]

        let report = ProvenanceMark.validate(marksWithGap)

        // Format should show gap issue and multiple sequences
        let text = report.format(.text)
        let expectedText = """
        Total marks: 4
        Chains: 1

        Chain 1: b16a7cbd
          0: f057c8c4 (genesis mark)
          1: 1b806d6c
          3: 761a5e74 (gap: 2 missing)
          4: 42d12de5
        """
        #expect(text == expectedText)
    }

    // MARK: - test_validate_out_of_order

    @Test func testValidateOutOfOrder() throws {
        let marks = createTestMarks(count: 5, resolution: .low, passphrase: "test")

        // Swap marks 2 and 3
        let marksOutOfOrder = [
            marks[0],
            marks[1],
            marks[3], // Out of order
            marks[2],
            marks[4],
        ]

        let report = ProvenanceMark.validate(marksOutOfOrder)

        // Format should return empty string - validation sorts by seq number
        #expect(report.format(.text) == "")
    }

    // MARK: - test_validate_hash_mismatch

    @Test func testValidateHashMismatch() throws {
        BCTags.registerTags()

        let marks = createTestMarks(count: 3, resolution: .low, passphrase: "test")
        let mark0 = marks[0]
        let mark1 = marks[1]

        // Create a third mark that claims to follow mark1 but with wrong next key.
        // Use mark0's hash as nextKey (wrong! should be the actual next key).
        let date = utcDate(year: 2023, month: 6, day: 22, hour: 12)

        let badMark = try ProvenanceMark(
            resolution: mark1.resolution,
            key: mark1.key,
            nextKey: mark0.hash, // Wrong! Should be mark1's actual next key
            chainId: mark1.chainId,
            seq: 2,
            date: date
        )

        let report = ProvenanceMark.validate([mark0, mark1, badMark])

        // Format should show hash mismatch issue
        let text = report.format(.text).trimmingCharacters(in: .whitespacesAndNewlines)
        let expectedText = """
        Total marks: 3
        Chains: 1

        Chain 1: b16a7cbd
          0: f057c8c4 (genesis mark)
          1: 1b806d6c
          2: 09cca821 (hash mismatch)
        """
        #expect(text == expectedText)
    }

    // MARK: - test_validate_date_ordering_violation

    @Test func testValidateDateOrderingViolation() throws {
        let marks = createTestMarks(count: 3, resolution: .low, passphrase: "test")

        // We can't actually create marks with wrong date ordering using the
        // generator, since it enforces consistency. This test demonstrates that
        // the validator would catch it if such marks existed.

        let report = ProvenanceMark.validate(marks)

        // This just validates the normal 3-mark chain; no issues expected.
        #expect(report.format(.text) == "")
    }

    // MARK: - test_validate_multiple_sequences_in_chain

    @Test func testValidateMultipleSequencesInChain() throws {
        let marks = createTestMarks(count: 7, resolution: .low, passphrase: "test")

        // Create multiple gaps
        let marksWithGaps = [
            marks[0], // Sequence 1: [0,1]
            marks[1],
            marks[3], // Sequence 2: [3,4] (gap from 1 to 3)
            marks[4],
            marks[6], // Sequence 3: [6] (gap from 4 to 6)
        ]

        let report = ProvenanceMark.validate(marksWithGaps)

        // Format should show multiple sequences with gap annotations
        let text = report.format(.text)
        let expectedText = """
        Total marks: 5
        Chains: 1

        Chain 1: b16a7cbd
          0: f057c8c4 (genesis mark)
          1: 1b806d6c
          3: 761a5e74 (gap: 2 missing)
          4: 42d12de5
          6: 8a9b06e1 (gap: 5 missing)
        """
        #expect(text == expectedText)
    }

    // MARK: - test_validate_precedes_opt

    @Test func testValidatePrecedesOpt() throws {
        let marks = createTestMarks(count: 3, resolution: .low, passphrase: "test")

        // Test valid precedes
        #expect(marks[0].precedes(marks[1]))
        #expect(marks[1].precedes(marks[2]))

        // Test invalid precedes (reverse order)
        #expect(!marks[1].precedes(marks[0]))

        // Test gap
        #expect(!marks[0].precedes(marks[2]))
    }

    // MARK: - test_validate_chain_id_hex

    @Test func testValidateChainIdHex() throws {
        let marks = createTestMarks(count: 2, resolution: .low, passphrase: "test")
        let report = ProvenanceMark.validate(marks)

        let chain = report.chains[0]
        let chainIdHex = chain.chainIdHex

        // Verify hex encoding
        #expect(chainIdHex.allSatisfy { $0.isHexDigit })
        #expect(chainIdHex == marks[0].chainId.hex)
    }

    // MARK: - test_validate_with_info

    @Test func testValidateWithInfo() throws {
        BCTags.registerTags()

        var generator = ProvenanceMarkGenerator(
            resolution: .low,
            passphrase: "test"
        )

        var calendar = Calendar(identifier: .gregorian)
        calendar.timeZone = TimeZone(identifier: "UTC")!

        let marks: [ProvenanceMark] = (0..<3).map { i in
            let date = calendar.date(from: DateComponents(
                year: 2023, month: 6, day: 20 + i,
                hour: 12, minute: 0, second: 0
            ))!
            return generator.next(date: date, info: CBOR("Test info"))
        }

        let report = ProvenanceMark.validate(marks)

        let json = report.format(.jsonPretty)
        let expectedJSON = """
        {
          "chains" : [
            {
              "chain_id" : "b16a7cbd",
              "has_genesis" : true,
              "marks" : [
                "ur:provenance/lfaehdcypaimkerydihsaedesbglvlrsgdmocfdpveksstlbrprscahlihyntoaxvtem",
                "ur:provenance/lfaehdcyecgldtsrbbfgsbetsrsgsafwrntdrtkohdhntnwdvtcsatnbkiythefdkiso",
                "ur:provenance/lfaehdcybwatptqzoyrkdmptfntsjsqdpmpmrfoylewnlpjnhdwzadnycljncflozsfy"
              ],
              "sequences" : [
                {
                  "end_seq" : 2,
                  "marks" : [
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaehdcypaimkerydihsaedesbglvlrsgdmocfdpveksstlbrprscahlihyntoaxvtem"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaehdcyecgldtsrbbfgsbetsrsgsafwrntdrtkohdhntnwdvtcsatnbkiythefdkiso"
                    },
                    {
                      "issues" : [

                      ],
                      "mark" : "ur:provenance/lfaehdcybwatptqzoyrkdmptfntsjsqdpmpmrfoylewnlpjnhdwzadnycljncflozsfy"
                    }
                  ],
                  "start_seq" : 0
                }
              ]
            }
          ],
          "marks" : [
            "ur:provenance/lfaehdcypaimkerydihsaedesbglvlrsgdmocfdpveksstlbrprscahlihyntoaxvtem",
            "ur:provenance/lfaehdcyecgldtsrbbfgsbetsrsgsafwrntdrtkohdhntnwdvtcsatnbkiythefdkiso",
            "ur:provenance/lfaehdcybwatptqzoyrkdmptfntsjsqdpmpmrfoylewnlpjnhdwzadnycljncflozsfy"
          ]
        }
        """
        #expect(json == expectedJSON)
    }

    // MARK: - test_validate_sorted_chains

    @Test func testValidateSortedChains() throws {
        // Create marks from different chains
        let marks1 = createTestMarks(count: 2, resolution: .low, passphrase: "zebra")
        let marks2 = createTestMarks(count: 2, resolution: .low, passphrase: "apple")
        let marks3 = createTestMarks(count: 2, resolution: .low, passphrase: "middle")

        var allMarks = marks1
        allMarks.append(contentsOf: marks2)
        allMarks.append(contentsOf: marks3)

        let report = ProvenanceMark.validate(allMarks)

        // Chains should be sorted by chain_id hex
        #expect(report.chains.count == 3)
        #expect(report.chains[0].chainIdHex < report.chains[1].chainIdHex)
        #expect(report.chains[1].chainIdHex < report.chains[2].chainIdHex)
    }

    // MARK: - test_validate_genesis_check

    @Test func testValidateGenesisCheck() throws {
        let marks = createTestMarks(count: 3, resolution: .low, passphrase: "test")

        // With genesis
        let reportWithGenesis = ProvenanceMark.validate(marks)
        #expect(reportWithGenesis.chains[0].hasGenesis == true)

        // Without genesis
        let marksNoGenesis = Array(marks.dropFirst())
        let reportNoGenesis = ProvenanceMark.validate(marksNoGenesis)
        #expect(reportNoGenesis.chains[0].hasGenesis == false)
    }

    // MARK: - test_validate_date_ordering_violation_constructed

    @Test func testValidateDateOrderingViolationConstructed() throws {
        BCTags.registerTags()

        let marks = createTestMarks(count: 2, resolution: .low, passphrase: "test")
        let mark0 = marks[0]

        // Create a second mark with an earlier date
        let earlierDate = utcDate(year: 2023, month: 6, day: 19, hour: 12)

        // To test date ordering, we need to create mark1 with the correct key
        // from generator but with an earlier date
        var generator = ProvenanceMarkGenerator(
            resolution: .low,
            passphrase: "test"
        )
        let _ = generator.next(date: mark0.date) // skip first
        let mark1BadDate = generator.next(date: earlierDate)

        let report = ProvenanceMark.validate([mark0, mark1BadDate])

        // Verify the report contains the date ordering issue
        #expect(report.hasIssues)

        // Check the text format shows the issue
        let text = report.format(.text)
        #expect(text.contains("date"))
    }

    // MARK: - test_validate_non_genesis_at_seq_zero

    @Test func testValidateNonGenesisAtSeqZero() throws {
        BCTags.registerTags()

        // Create proper marks
        let marks = createTestMarks(count: 2, resolution: .low, passphrase: "test")
        let mark0 = marks[0]
        let mark1 = marks[1]

        // When mark1 claims to be at seq 0, it should fail NonGenesisAtZero
        // check when preceded by mark0
        let date = utcDate(year: 2023, month: 6, day: 21, hour: 12)

        let badMark = try ProvenanceMark(
            resolution: mark1.resolution,
            key: mark1.key,
            nextKey: mark1.hash,
            chainId: mark1.chainId,
            seq: 0, // Claim seq 0 but not genesis
            date: date
        )

        let report = ProvenanceMark.validate([mark0, badMark])

        // Verify the report detects the NonGenesisAtZero issue
        #expect(report.hasIssues)

        // Verify there are two sequence segments (the issue breaks the chain)
        #expect(report.chains[0].sequences.count == 2)

        // The second segment's first mark should have the NonGenesisAtZero issue
        let secondSegment = report.chains[0].sequences[1]
        #expect(secondSegment.marks[0].issues.contains(.nonGenesisAtZero))
    }

    // MARK: - test_validate_invalid_genesis_key_constructed

    @Test func testValidateInvalidGenesisKeyConstructed() throws {
        BCTags.registerTags()

        // Create proper marks
        let marks = createTestMarks(count: 2, resolution: .low, passphrase: "test")
        let mark0 = marks[0]
        let mark1 = marks[1]

        // When mark1 is at seq > 0 but has key == chain_id, it should fail
        // InvalidGenesisKey
        let date = utcDate(year: 2023, month: 6, day: 21, hour: 12)

        let badMark = try ProvenanceMark(
            resolution: mark1.resolution,
            key: mark1.chainId, // key == chain_id (not allowed at seq > 0)
            nextKey: mark1.hash,
            chainId: mark1.chainId,
            seq: 1,
            date: date
        )

        let report = ProvenanceMark.validate([mark0, badMark])

        // Verify the report detects the InvalidGenesisKey issue
        #expect(report.hasIssues)

        // Verify there are two sequence segments
        #expect(report.chains[0].sequences.count == 2)

        // The second segment's first mark should have the InvalidGenesisKey issue
        let secondSegment = report.chains[0].sequences[1]
        #expect(secondSegment.marks[0].issues.contains(.invalidGenesisKey))
    }
}
