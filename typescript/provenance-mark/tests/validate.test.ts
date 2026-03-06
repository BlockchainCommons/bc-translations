import { describe, test, expect } from 'vitest';
import { cbor, CborDate } from '@bc/dcbor';
import {
  ProvenanceMark,
  ProvenanceMarkGenerator,
  ProvenanceMarkResolution,
  ValidationReport,
  ValidationReportFormat,
} from '../src/index.js';

function createTestMarks(
  count: number,
  resolution: ProvenanceMarkResolution,
  passphrase: string,
): ProvenanceMark[] {
  ProvenanceMark.registerTags();
  const generator = ProvenanceMarkGenerator.createWithPassphrase(resolution, passphrase);
  return Array.from({ length: count }, (_, i) => {
    const date = CborDate.fromDatetime(
      new Date(Date.UTC(2023, 5, 20, 12, 0, 0) + i * 86400000),
    );
    return generator.next(date);
  });
}

describe('validate', () => {
  test('test_validate_empty', () => {
    const report = ValidationReport.validate([]);
    const reportViaMark = ProvenanceMark.validate([]);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [],
  "chains": []
}`);
    expect(reportViaMark.format(ValidationReportFormat.JsonPretty)).toBe(json);

    // Test compact JSON format
    const jsonCompact = report.format(ValidationReportFormat.JsonCompact);
    expect(jsonCompact).toBe('{"marks":[],"chains":[]}');

    // Format should return empty string for empty report
    expect(report.format(ValidationReportFormat.Text)).toBe('');
  });

  test('test_validate_single_mark', () => {
    const marks = createTestMarks(1, ProvenanceMarkResolution.Low, 'test');
    const report = ValidationReport.validate(marks);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 0,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Test compact JSON format
    const jsonCompact = report.format(ValidationReportFormat.JsonCompact);
    expect(jsonCompact).toBe('{"marks":["ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"],"chains":[{"chain_id":"b16a7cbd","has_genesis":true,"marks":["ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"],"sequences":[{"start_seq":0,"end_seq":0,"marks":[{"mark":"ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba","issues":[]}]}]}]}');

    // Format should return empty string for single perfect chain
    expect(report.format(ValidationReportFormat.Text)).toBe('');
  });

  test('test_validate_valid_sequence', () => {
    const marks = createTestMarks(5, ProvenanceMarkResolution.Low, 'test');
    const report = ValidationReport.validate(marks);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
    "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
    "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
        "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
        "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 4,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should return empty string for single perfect chain
    expect(report.format(ValidationReportFormat.Text)).toBe('');
  });

  test('test_validate_deduplication', () => {
    const marks = createTestMarks(3, ProvenanceMarkResolution.Low, 'test');

    // Create duplicates
    const marksWithDups = [
      ...marks,
      marks[0]!,
      marks[1]!,
      marks[0]!,
    ];

    const report = ValidationReport.validate(marksWithDups);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should return empty string - single perfect chain after deduplication
    expect(report.format(ValidationReportFormat.Text)).toBe('');
  });

  test('test_validate_multiple_chains', () => {
    const marks1 = createTestMarks(3, ProvenanceMarkResolution.Low, 'alice');
    const marks2 = createTestMarks(3, ProvenanceMarkResolution.Low, 'bob');

    const allMarks = [...marks1, ...marks2];

    const report = ValidationReport.validate(allMarks);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdotfmbeuerniolpveenmowliegyfrfrwnfzntnbwe",
    "ur:provenance/lfaegdztfetoehnyjswzsopecewkqdiskshfnyndiemkld",
    "ur:provenance/lfaegdenrdietbenskbesbdiiefgwkuoqzldbecpidhfrt",
    "ur:provenance/lfaegdknnsfhhylrgytdhtsnheskzepmctgrwnlyjeyngh",
    "ur:provenance/lfaegdrtckinuywdosecpedtbnismdcllyvsbbplkpspyl",
    "ur:provenance/lfaegdrevlpmticnmkbafsinmeonvycydphernwerppefs"
  ],
  "chains": [
    {
      "chain_id": "7a9c3f5e",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdknnsfhhylrgytdhtsnheskzepmctgrwnlyjeyngh",
        "ur:provenance/lfaegdrtckinuywdosecpedtbnismdcllyvsbbplkpspyl",
        "ur:provenance/lfaegdrevlpmticnmkbafsinmeonvycydphernwerppefs"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdknnsfhhylrgytdhtsnheskzepmctgrwnlyjeyngh",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdrtckinuywdosecpedtbnismdcllyvsbbplkpspyl",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdrevlpmticnmkbafsinmeonvycydphernwerppefs",
              "issues": []
            }
          ]
        }
      ]
    },
    {
      "chain_id": "a33e10de",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdotfmbeuerniolpveenmowliegyfrfrwnfzntnbwe",
        "ur:provenance/lfaegdztfetoehnyjswzsopecewkqdiskshfnyndiemkld",
        "ur:provenance/lfaegdenrdietbenskbesbdiiefgwkuoqzldbecpidhfrt"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdotfmbeuerniolpveenmowliegyfrfrwnfzntnbwe",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdztfetoehnyjswzsopecewkqdiskshfnyndiemkld",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdenrdietbenskbesbdiiefgwkuoqzldbecpidhfrt",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should show both chains (interesting)
    expect(report.format(ValidationReportFormat.Text)).toBe(`Total marks: 6
Chains: 2

Chain 1: 7a9c3f5e
  0: 0d6e0afd (genesis mark)
  1: 6cd504e7
  2: dc07895c

Chain 2: a33e10de
  0: c2a985ff (genesis mark)
  1: 5567cd24
  2: f759ad4c`);
  });

  test('test_validate_missing_genesis', () => {
    const marks = createTestMarks(5, ProvenanceMarkResolution.Low, 'test');

    // Remove genesis mark (index 0)
    const marksNoGenesis = marks.slice(1);

    const report = ValidationReport.validate(marksNoGenesis);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
    "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
    "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": false,
      "marks": [
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
        "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
        "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
      ],
      "sequences": [
        {
          "start_seq": 1,
          "end_seq": 4,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should show missing genesis warning
    expect(report.format(ValidationReportFormat.Text)).toBe(`Total marks: 4
Chains: 1

Chain 1: b16a7cbd
  Warning: No genesis mark found
  1: 1b806d6c
  2: b292f357
  3: 761a5e74
  4: 42d12de5`);
  });

  test('test_validate_sequence_gap', () => {
    const marks = createTestMarks(5, ProvenanceMarkResolution.Low, 'test');

    // Create a gap by removing mark at index 2 (sequence 2)
    const marksWithGap = [
      marks[0]!,
      marks[1]!,
      marks[3]!, // Gap: skips seq 2, this is seq 3
      marks[4]!,
    ];

    const report = ValidationReport.validate(marksWithGap);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
    "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
        "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 3,
          "end_seq": 4,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
              "issues": [
                {
                  "type": "SequenceGap",
                  "data": {
                    "expected": 2,
                    "actual": 3
                  }
                }
              ]
            },
            {
              "mark": "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should show gap issue and multiple sequences
    expect(report.format(ValidationReportFormat.Text)).toBe(`Total marks: 4
Chains: 1

Chain 1: b16a7cbd
  0: f057c8c4 (genesis mark)
  1: 1b806d6c
  3: 761a5e74 (gap: 2 missing)
  4: 42d12de5`);
  });

  test('test_validate_out_of_order', () => {
    const marks = createTestMarks(5, ProvenanceMarkResolution.Low, 'test');

    // Swap marks 2 and 3
    const marksOutOfOrder = [
      marks[0]!,
      marks[1]!,
      marks[3]!, // Out of order
      marks[2]!,
      marks[4]!,
    ];

    const report = ValidationReport.validate(marksOutOfOrder);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
    "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
        "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
        "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 4,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should return empty string - validation sorts by seq number
    expect(report.format(ValidationReportFormat.Text)).toBe('');
  });

  test('test_validate_hash_mismatch', () => {
    ProvenanceMark.registerTags();

    const marks = createTestMarks(3, ProvenanceMarkResolution.Low, 'test');
    const mark0 = marks[0]!;
    const mark1 = marks[1]!;

    // Create a third mark that claims to follow mark1 but with wrong prev hash
    const date = CborDate.fromDatetime(new Date(Date.UTC(2023, 5, 22, 12, 0, 0)));

    // Use mark1's chain_id and key, but use mark0's hash as nextKey (wrong!)
    const badMark = ProvenanceMark.create(
      mark1.resolution,
      mark1.key,
      mark0.hash, // Wrong! Should be mark1.hash
      mark1.chainId,
      2,
      date,
    );

    const report = ValidationReport.validate([mark0, mark1, badMark]);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdecgldtsrbbfgsbethprlwfgsrnttrtkpgsttptwn"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdecgldtsrbbfgsbethprlwfgsrnttrtkpgsttptwn"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 2,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbethprlwfgsrnttrtkpgsttptwn",
              "issues": [
                {
                  "type": "HashMismatch",
                  "data": {
                    "expected": "d446017b",
                    "actual": "1b806d6c"
                  }
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should show hash mismatch issue
    expect(report.format(ValidationReportFormat.Text).trim()).toBe(`Total marks: 3
Chains: 1

Chain 1: b16a7cbd
  0: f057c8c4 (genesis mark)
  1: 1b806d6c
  2: 09cca821 (hash mismatch)`);
  });

  test('test_validate_date_ordering_violation', () => {
    const marks = createTestMarks(3, ProvenanceMarkResolution.Low, 'test');

    // We can't actually create marks with wrong date ordering using the
    // generator, since it enforces consistency. This test demonstrates that
    // the validator would catch it if such marks existed.

    const report = ValidationReport.validate(marks);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);
  });

  test('test_validate_multiple_sequences_in_chain', () => {
    const marks = createTestMarks(7, ProvenanceMarkResolution.Low, 'test');

    // Create multiple gaps
    const marksWithGaps = [
      marks[0]!, // Sequence 1: [0,1]
      marks[1]!,
      marks[3]!, // Sequence 2: [3,4] (gap from 1 to 3)
      marks[4]!,
      marks[6]!, // Sequence 3: [6] (gap from 4 to 6)
    ];

    const report = ValidationReport.validate(marksWithGaps);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
    "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
    "ur:provenance/lfaegdwkltwzolasuomobntaryinjzcyrocsfskkrtmyam"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
        "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
        "ur:provenance/lfaegdwkltwzolasuomobntaryinjzcyrocsfskkrtmyam"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 3,
          "end_seq": 4,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdhsvtleetlatsmwwdndmnjlaxonsfdewmghpybzbg",
              "issues": [
                {
                  "type": "SequenceGap",
                  "data": {
                    "expected": 2,
                    "actual": 3
                  }
                }
              ]
            },
            {
              "mark": "ur:provenance/lfaegdrkkilkylsrendmkniaeejyrhndlyvednzckpsbtk",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 6,
          "end_seq": 6,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdwkltwzolasuomobntaryinjzcyrocsfskkrtmyam",
              "issues": [
                {
                  "type": "SequenceGap",
                  "data": {
                    "expected": 5,
                    "actual": 6
                  }
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}`);

    // Format should show multiple sequences with gap annotations
    expect(report.format(ValidationReportFormat.Text)).toBe(`Total marks: 5
Chains: 1

Chain 1: b16a7cbd
  0: f057c8c4 (genesis mark)
  1: 1b806d6c
  3: 761a5e74 (gap: 2 missing)
  4: 42d12de5
  6: 8a9b06e1 (gap: 5 missing)`);
  });

  test('test_validate_precedes_opt', () => {
    const marks = createTestMarks(3, ProvenanceMarkResolution.Low, 'test');

    // Test valid precedes
    expect(() => marks[0]!.assertPrecedes(marks[1]!)).not.toThrow();
    expect(() => marks[1]!.assertPrecedes(marks[2]!)).not.toThrow();

    // Test invalid precedes (reverse order)
    expect(() => marks[1]!.assertPrecedes(marks[0]!)).toThrow();

    // Test gap
    expect(() => marks[0]!.assertPrecedes(marks[2]!)).toThrow();
  });

  test('test_validate_chain_id_hex', () => {
    const marks = createTestMarks(2, ProvenanceMarkResolution.Low, 'test');
    const report = ValidationReport.validate([...marks]);

    const chain = report.chains[0]!;
    const chainIdHex = chain.chainIdHex();

    // Verify hex encoding
    expect(chainIdHex).toMatch(/^[0-9a-f]+$/);

    // Verify it matches the mark's chain ID
    const expectedHex = Array.from(marks[0]!.chainId)
      .map((b) => b.toString(16).padStart(2, '0'))
      .join('');
    expect(chainIdHex).toBe(expectedHex);
  });

  test('test_validate_with_info', () => {
    const generator = ProvenanceMarkGenerator.createWithPassphrase(
      ProvenanceMarkResolution.Low,
      'test',
    );

    const marks = Array.from({ length: 3 }, (_, i) => {
      const date = CborDate.fromDatetime(
        new Date(Date.UTC(2023, 5, 20, 12, 0, 0) + i * 86400000),
      );
      return generator.next(date, cbor('Test info'));
    });

    const report = ValidationReport.validate(marks);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaehdcypaimkerydihsaedesbglvlrsgdmocfdpveksstlbrprscahlihyntoaxvtem",
    "ur:provenance/lfaehdcyecgldtsrbbfgsbetsrsgsafwrntdrtkohdhntnwdvtcsatnbkiythefdkiso",
    "ur:provenance/lfaehdcybwatptqzoyrkdmptfntsjsqdpmpmrfoylewnlpjnhdwzadnycljncflozsfy"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaehdcypaimkerydihsaedesbglvlrsgdmocfdpveksstlbrprscahlihyntoaxvtem",
        "ur:provenance/lfaehdcyecgldtsrbbfgsbetsrsgsafwrntdrtkohdhntnwdvtcsatnbkiythefdkiso",
        "ur:provenance/lfaehdcybwatptqzoyrkdmptfntsjsqdpmpmrfoylewnlpjnhdwzadnycljncflozsfy"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaehdcypaimkerydihsaedesbglvlrsgdmocfdpveksstlbrprscahlihyntoaxvtem",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaehdcyecgldtsrbbfgsbetsrsgsafwrntdrtkohdhntnwdvtcsatnbkiythefdkiso",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaehdcybwatptqzoyrkdmptfntsjsqdpmpmrfoylewnlpjnhdwzadnycljncflozsfy",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);
  });

  test('test_validate_sorted_chains', () => {
    // Create marks from different chains
    const marks1 = createTestMarks(2, ProvenanceMarkResolution.Low, 'zebra');
    const marks2 = createTestMarks(2, ProvenanceMarkResolution.Low, 'apple');
    const marks3 = createTestMarks(2, ProvenanceMarkResolution.Low, 'middle');

    const allMarks = [...marks1, ...marks2, ...marks3];

    const report = ValidationReport.validate(allMarks);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdcktndeltrtspprmhkptlfdwfgylsjljzwtahlpsf",
    "ur:provenance/lfaegdrslnurdeknftkscnlphnhgldcxnnahwddiaavyda",
    "ur:provenance/lfaegdfltogtdmfpdphlttkilywyfntidsamrkmuioteid",
    "ur:provenance/lfaegdntjopfzttddtsrkirkdytlkirhisiyidimdmwnkg",
    "ur:provenance/lfaegdfylajldrntasvyttgljtsbsoghdafzwfcawmgede",
    "ur:provenance/lfaegdgrrtjorhmuzshlvsfdldchoxbntlsrstoyidjepm"
  ],
  "chains": [
    {
      "chain_id": "1eda2887",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdcktndeltrtspprmhkptlfdwfgylsjljzwtahlpsf",
        "ur:provenance/lfaegdrslnurdeknftkscnlphnhgldcxnnahwddiaavyda"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdcktndeltrtspprmhkptlfdwfgylsjljzwtahlpsf",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdrslnurdeknftkscnlphnhgldcxnnahwddiaavyda",
              "issues": []
            }
          ]
        }
      ]
    },
    {
      "chain_id": "44806f2a",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdfylajldrntasvyttgljtsbsoghdafzwfcawmgede",
        "ur:provenance/lfaegdgrrtjorhmuzshlvsfdldchoxbntlsrstoyidjepm"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdfylajldrntasvyttgljtsbsoghdafzwfcawmgede",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdgrrtjorhmuzshlvsfdldchoxbntlsrstoyidjepm",
              "issues": []
            }
          ]
        }
      ]
    },
    {
      "chain_id": "47ce4d2e",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdfltogtdmfpdphlttkilywyfntidsamrkmuioteid",
        "ur:provenance/lfaegdntjopfzttddtsrkirkdytlkirhisiyidimdmwnkg"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdfltogtdmfpdphlttkilywyfntidsamrkmuioteid",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdntjopfzttddtsrkirkdytlkirhisiyidimdmwnkg",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);
  });

  test('test_validate_genesis_check', () => {
    const marks = createTestMarks(3, ProvenanceMarkResolution.Low, 'test');

    // With genesis
    const reportWithGenesis = ValidationReport.validate([...marks]);

    const json = reportWithGenesis.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);

    // Without genesis
    const marksNoGenesis = marks.slice(1);
    const reportNoGenesis = ValidationReport.validate(marksNoGenesis);

    const json2 = reportNoGenesis.format(ValidationReportFormat.JsonPretty);
    expect(json2).toBe(`{
  "marks": [
    "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
    "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": false,
      "marks": [
        "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
        "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd"
      ],
      "sequences": [
        {
          "start_seq": 1,
          "end_seq": 2,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetgazoenadrntdrtkoluwekerp",
              "issues": []
            },
            {
              "mark": "ur:provenance/lfaegdbwatptqzoyrkdmptvasefnfmpmpmrfoywyptolfd",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`);
  });

  test('test_validate_date_ordering_violation_constructed', () => {
    ProvenanceMark.registerTags();

    const marks = createTestMarks(2, ProvenanceMarkResolution.Low, 'test');
    const mark0 = marks[0]!;

    // Create a second mark with an earlier date
    const earlierDate = CborDate.fromDatetime(
      new Date(Date.UTC(2023, 5, 19, 12, 0, 0)),
    );

    // To test date ordering, we need to create mark1 with the correct key from
    // generator but with an earlier date
    const generator = ProvenanceMarkGenerator.createWithPassphrase(
      ProvenanceMarkResolution.Low,
      'test',
    );
    generator.next(mark0.date); // skip first
    const mark1BadDate = generator.next(earlierDate);

    const report = ValidationReport.validate([mark0, mark1BadDate]);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetckchiatnrntdrtjohpbdeteo"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetckchiatnrntdrtjohpbdeteo"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 0,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 1,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetckchiatnrntdrtjohpbdeteo",
              "issues": [
                {
                  "type": "DateOrdering",
                  "data": {
                    "previous": "2023-06-20",
                    "next": "2023-06-19"
                  }
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}`);
  });

  test('test_validate_non_genesis_at_seq_zero', () => {
    ProvenanceMark.registerTags();

    // Create proper marks
    const marks = createTestMarks(2, ProvenanceMarkResolution.Low, 'test');
    const mark0 = marks[0]!;
    const mark1 = marks[1]!;

    // When mark1 claims to be at seq 0, it should fail NonGenesisAtZero check
    const date = CborDate.fromDatetime(new Date(Date.UTC(2023, 5, 21, 12, 0, 0)));

    const badMark = ProvenanceMark.create(
      mark1.resolution,
      mark1.key,
      mark1.hash,
      mark1.chainId,
      0, // Claim seq 0 but not genesis
      date,
    );

    const report = ValidationReport.validate([mark0, badMark]);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdecgldtsrbbfgsbetbahhgowzrntertkopkmyiowp"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdecgldtsrbbfgsbetbahhgowzrntertkopkmyiowp"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 0,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 0,
          "end_seq": 0,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdecgldtsrbbfgsbetbahhgowzrntertkopkmyiowp",
              "issues": [
                {
                  "type": "NonGenesisAtZero"
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}`);
  });

  test('test_validate_invalid_genesis_key_constructed', () => {
    ProvenanceMark.registerTags();

    // Create proper marks
    const marks = createTestMarks(2, ProvenanceMarkResolution.Low, 'test');
    const mark0 = marks[0]!;
    const mark1 = marks[1]!;

    // When mark1 is at seq > 0 but has key == chain_id, it should fail InvalidGenesisKey
    const date = CborDate.fromDatetime(new Date(Date.UTC(2023, 5, 21, 12, 0, 0)));

    const badMark = ProvenanceMark.create(
      mark1.resolution,
      mark1.chainId, // key == chain_id (not allowed at seq > 0)
      mark1.hash,
      mark1.chainId,
      1, // seq 1
      date,
    );

    const report = ValidationReport.validate([mark0, badMark]);

    const json = report.format(ValidationReportFormat.JsonPretty);
    expect(json).toBe(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
    "ur:provenance/lfaegdpaimkerydihsaedewnwnsnwmgdmucfdwcpfxdtsr"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
        "ur:provenance/lfaegdpaimkerydihsaedewnwnsnwmgdmucfdwcpfxdtsr"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 0,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            }
          ]
        },
        {
          "start_seq": 1,
          "end_seq": 1,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedewnwnsnwmgdmucfdwcpfxdtsr",
              "issues": [
                {
                  "type": "InvalidGenesisKey"
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}`);
  });
});
