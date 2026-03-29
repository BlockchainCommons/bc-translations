package provenancemark

import (
	"encoding/json"
	"strings"
	"testing"
)

func TestMarkVectorsAndRoundTrips(t *testing.T) {
	type testCase struct {
		name          string
		res           ProvenanceMarkResolution
		withInfo      bool
		expectedDebug string
		expectedBW    string
		expectedID    string
		expectedEmoji string
		expectedUR    string
		expectedURL   string
	}

	cases := []testCase{
		{
			name:          "low",
			res:           ProvenanceMarkResolutionLow,
			expectedDebug: "ProvenanceMark(key: 090bf2f8, hash: 5bdcec81, chainID: 090bf2f8, seq: 0, date: 2023-06-20)",
			expectedBW:    "axis bald whiz yoga rich join body jazz yurt wall monk fact urge cola exam arch kick fuel omit echo",
			expectedID:    "HELP UNDO WASP LAZY",
			expectedEmoji: "🌮 🐰 🦄 💔",
			expectedUR:    "ur:provenance/lfaegdasbdwzyarhjnbyjzytwlmkftuecaemahwmfgaxcl",
			expectedURL:   "https://example.com/validate?provenance=tngdgmgwhflfaegdasbdwzyarhjnbyjzytwlmkftuecaemahdpbswmkb",
		},
		{
			name:          "low_with_info",
			res:           ProvenanceMarkResolutionLow,
			withInfo:      true,
			expectedDebug: "ProvenanceMark(key: 090bf2f8, hash: baee34c2, chainID: 090bf2f8, seq: 0, date: 2023-06-20, info: \"Lorem ipsum sit dolor amet.\")",
			expectedBW:    "axis bald whiz yoga rich join body jazz cats ugly fizz kick urge cola exam arch girl navy jugs flew unit keys flap very cyan cola flew rock zero jazz yoga owls fair glow film quad runs scar barn glow belt onyx foxy cost apex purr data wave poem",
			expectedID:    "ROAD WAXY EDGE SAGA",
			expectedEmoji: "🔭 🐛 💪 🎾",
			expectedUR:    "ur:provenance/lfaehddpasbdwzyarhjnbyjzcsuyfzkkuecaemahglnyjsfwutksfpvycncafwrkzojzyaosfrgwfmqdrssrbngwbtoxfyctaxgwgewmqd",
			expectedURL:   "https://example.com/validate?provenance=tngdgmgwhflfaehddpasbdwzyarhjnbyjzcsuyfzkkuecaemahglnyjsfwutksfpvycncafwrkzojzyaosfrgwfmqdrssrbngwbtoxfyctaxpdkpyahl",
		},
		{
			name:          "medium",
			res:           ProvenanceMarkResolutionMedium,
			expectedDebug: "ProvenanceMark(key: 090bf2f8b55be45b, hash: 188d6bd9ad8bc4f3, chainID: 090bf2f8b55be45b, seq: 0, date: 2023-06-20T12:00:00Z)",
			expectedBW:    "axis bald whiz yoga race help vibe help scar many list undo buzz puma urge hawk omit memo judo away void leaf maze numb slot back back vial join play days open tent cola visa memo",
			expectedID:    "CATS LUNG JADE TUNA",
			expectedEmoji: "🤠 🛑 🌹 🐶",
			expectedUR:    "ur:provenance/lfadhdcxasbdwzyarehpvehpsrmyltuobzpauehkotmojoayvdlfmenbstbkbkvljnpydsonurlefxhf",
			expectedURL:   "https://example.com/validate?provenance=tngdgmgwhflfadhdcxasbdwzyarehpvehpsrmyltuobzpauehkotmojoayvdlfmenbstbkbkvljnpydsonsrehgsly",
		},
		{
			name:          "medium_with_info",
			res:           ProvenanceMarkResolutionMedium,
			withInfo:      true,
			expectedDebug: "ProvenanceMark(key: 090bf2f8b55be45b, hash: 999b6a32516e7ff8, chainID: 090bf2f8b55be45b, seq: 0, date: 2023-06-20T12:00:00Z, info: \"Lorem ipsum sit dolor amet.\")",
			expectedBW:    "axis bald whiz yoga race help vibe help scar many list undo buzz puma urge hawk cusp liar jugs vial claw into door play slot back back vial join play days open void mint visa help grim peck waxy jowl tuna play onyx yank many fuel brag cash brew girl tiny arch webs very vial lamb safe owls iron onyx fair lamb data flux wall",
			expectedID:    "NAIL NEED ITEM EASY",
			expectedEmoji: "🏰 🎢 🍄 👈",
			expectedUR:    "ur:provenance/lfadhdfsasbdwzyarehpvehpsrmyltuobzpauehkcplrjsvlcwiodrpystbkbkvljnpydsonvdmtvahpgmpkwyjltapyoxykmyflbgchbwgltyahwsvyvllbseosinoxfrdttlhffg",
			expectedURL:   "https://example.com/validate?provenance=tngdgmgwhflfadhdfsasbdwzyarehpvehpsrmyltuobzpauehkcplrjsvlcwiodrpystbkbkvljnpydsonvdmtvahpgmpkwyjltapyoxykmyflbgchbwgltyahwsvyvllbseosinoxfrchhhfnzo",
		},
		{
			name:          "quartile",
			res:           ProvenanceMarkResolutionQuartile,
			expectedDebug: "ProvenanceMark(key: 090bf2f8b55be45b4661b24b7e9c340c, hash: 4a0738a31a3e9073f1c01999cd01ff0a, chainID: 090bf2f8b55be45b4661b24b7e9c340c, seq: 0, date: 2023-06-20T12:00:00Z)",
			expectedBW:    "axis bald whiz yoga race help vibe help frog huts purr gear knob news edge barn inky jump mild warm warm pose obey ruby very hill yank song frog into maze work exam veto foxy iron quiz edge arch cusp blue gray zaps task saga half monk jolt menu peck fern fizz item cost rich mild nail omit lazy song meow claw",
			expectedID:    "GAME AUNT EXIT OMIT",
			expectedEmoji: "🥑 😍 👃 💌",
			expectedUR:    "ur:provenance/lfaohdftasbdwzyarehpvehpfghsprgrkbnseebniyjpmdwmwmpeoyryvyhlyksgfgiomewkemvofyinqzeeahcpbegyzstksahfmkjtmupkfnfzimctrhmdnlotcywzfrzo",
			expectedURL:   "https://example.com/validate?provenance=tngdgmgwhflfaohdftasbdwzyarehpvehpfghsprgrkbnseebniyjpmdwmwmpeoyryvyhlyksgfgiomewkemvofyinqzeeahcpbegyzstksahfmkjtmupkfnfzimctrhmdnlothposhyuo",
		},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			marks := makeTestMarks(t, 10, tc.res, "Wolf", tc.withInfo)
			if !IsSequenceValid(marks) {
				t.Fatal("expected generated marks to form a valid sequence")
			}
			if marks[1].Precedes(marks[0]) {
				t.Fatal("mark 1 must not precede mark 0")
			}

			first := marks[0]
			if got := first.DebugString(); got != tc.expectedDebug {
				t.Fatalf("DebugString mismatch:\n got: %q\nwant: %q", got, tc.expectedDebug)
			}
			if got := first.ToBytewords(); got != tc.expectedBW {
				t.Fatalf("ToBytewords mismatch:\n got: %q\nwant: %q", got, tc.expectedBW)
			}
			if got := first.IDBytewords(4, false); got != tc.expectedID {
				t.Fatalf("IDBytewords mismatch: got=%q want=%q", got, tc.expectedID)
			}
			if got := first.IDBytemoji(4, false); got != tc.expectedEmoji {
				t.Fatalf("IDBytemoji mismatch: got=%q want=%q", got, tc.expectedEmoji)
			}
			if got := first.URString(); got != tc.expectedUR {
				t.Fatalf("URString mismatch:\n got: %q\nwant: %q", got, tc.expectedUR)
			}
			if got := first.ToURL("https://example.com/validate").String(); got != tc.expectedURL {
				t.Fatalf("ToURL mismatch:\n got: %q\nwant: %q", got, tc.expectedURL)
			}
			if got := first.String(); !strings.HasPrefix(got, "ProvenanceMark(") {
				t.Fatalf("String() format mismatch: %q", got)
			}

			for i, mark := range marks {
				bytewords := mark.ToBytewords()
				decodedBW, err := ProvenanceMarkFromBytewords(tc.res, bytewords)
				if err != nil {
					t.Fatalf("ProvenanceMarkFromBytewords[%d] failed: %v", i, err)
				}
				if !mark.Equal(decodedBW) {
					t.Fatalf("bytewords round-trip mismatch at index %d", i)
				}

				decodedUR, err := ProvenanceMarkFromURString(mark.URString())
				if err != nil {
					t.Fatalf("ProvenanceMarkFromURString[%d] failed: %v", i, err)
				}
				if !mark.Equal(decodedUR) {
					t.Fatalf("UR round-trip mismatch at index %d", i)
				}

				decodedURL, err := ProvenanceMarkFromURL(mark.ToURL("https://example.com/validate"))
				if err != nil {
					t.Fatalf("ProvenanceMarkFromURL[%d] failed: %v", i, err)
				}
				if !mark.Equal(decodedURL) {
					t.Fatalf("URL round-trip mismatch at index %d", i)
				}

				decodedCBOR, err := ProvenanceMarkFromTaggedCBORData(mark.TaggedCBOR().ToCBORData())
				if err != nil {
					t.Fatalf("ProvenanceMarkFromTaggedCBORData[%d] failed: %v", i, err)
				}
				if !mark.Equal(decodedCBOR) {
					t.Fatalf("CBOR round-trip mismatch at index %d", i)
				}

				payload, err := json.Marshal(mark)
				if err != nil {
					t.Fatalf("json.Marshal[%d] failed: %v", i, err)
				}
				var decodedJSON ProvenanceMark
				if err := json.Unmarshal(payload, &decodedJSON); err != nil {
					t.Fatalf("json.Unmarshal[%d] failed: %v", i, err)
				}
				if !mark.Equal(decodedJSON) {
					t.Fatalf("JSON round-trip mismatch at index %d", i)
				}
			}
		})
	}
}

func TestHighResolutionRoundTrips(t *testing.T) {
	marks := makeTestMarks(t, 5, ProvenanceMarkResolutionHigh, "Wolf", true)
	if !IsSequenceValid(marks) {
		t.Fatal("expected high-resolution marks to form a valid sequence")
	}
	for i, mark := range marks {
		decodedBW, err := ProvenanceMarkFromBytewords(ProvenanceMarkResolutionHigh, mark.ToBytewords())
		if err != nil {
			t.Fatalf("ProvenanceMarkFromBytewords[%d] failed: %v", i, err)
		}
		if !mark.Equal(decodedBW) {
			t.Fatalf("bytewords round-trip mismatch at index %d", i)
		}
		decodedUR, err := ProvenanceMarkFromURString(mark.URString())
		if err != nil {
			t.Fatalf("ProvenanceMarkFromURString[%d] failed: %v", i, err)
		}
		if !mark.Equal(decodedUR) {
			t.Fatalf("UR round-trip mismatch at index %d", i)
		}
	}
}
