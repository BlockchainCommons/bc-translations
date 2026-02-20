package bclifehash

import (
	"encoding/hex"
	"encoding/json"
	"fmt"
	"image"
	"image/png"
	"os"
	"path/filepath"
	"strconv"
	"testing"
)

type testVector struct {
	Input      string `json:"input"`
	InputType  string `json:"input_type"`
	Version    string `json:"version"`
	ModuleSize int    `json:"module_size"`
	HasAlpha   bool   `json:"has_alpha"`
	Width      int    `json:"width"`
	Height     int    `json:"height"`
	Colors     []byte `json:"colors"`
}

func parseVersion(s string) Version {
	switch s {
	case "version1":
		return Version1
	case "version2":
		return Version2
	case "detailed":
		return Detailed
	case "fiducial":
		return Fiducial
	case "grayscale_fiducial":
		return GrayscaleFiducial
	default:
		panic("unknown version: " + s)
	}
}

// TestGeneratePNGs generates 100 sample PNGs per version for visual inspection.
// Output goes to out/<version>/<n>.png
func TestGeneratePNGs(t *testing.T) {

	versions := []struct {
		name    string
		version Version
	}{
		{"version1", Version1},
		{"version2", Version2},
		{"detailed", Detailed},
		{"fiducial", Fiducial},
		{"grayscale_fiducial", GrayscaleFiducial},
	}

	outDir := "out"

	for _, v := range versions {
		dir := filepath.Join(outDir, v.name)
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}

		for i := 0; i < 100; i++ {
			input := strconv.Itoa(i)
			img := MakeFromUTF8(input, v.version, 1, false)

			rgba := image.NewNRGBA(image.Rect(0, 0, img.Width, img.Height))
			for y := 0; y < img.Height; y++ {
				for x := 0; x < img.Width; x++ {
					srcOff := (y*img.Width + x) * 3
					dstOff := (y*img.Width + x) * 4
					rgba.Pix[dstOff+0] = img.Colors[srcOff+0]
					rgba.Pix[dstOff+1] = img.Colors[srcOff+1]
					rgba.Pix[dstOff+2] = img.Colors[srcOff+2]
					rgba.Pix[dstOff+3] = 255
				}
			}

			path := filepath.Join(dir, fmt.Sprintf("%d.png", i))
			f, err := os.Create(path)
			if err != nil {
				t.Fatal(err)
			}
			if err := png.Encode(f, rgba); err != nil {
				f.Close()
				t.Fatal(err)
			}
			f.Close()
		}
	}
	t.Logf("Generated PNGs in %s/", outDir)
}

func TestAllVectors(t *testing.T) {
	data, err := os.ReadFile("testdata/test-vectors.json")
	if err != nil {
		t.Fatal(err)
	}

	var vectors []testVector
	if err := json.Unmarshal(data, &vectors); err != nil {
		t.Fatal(err)
	}

	if len(vectors) != 35 {
		t.Fatalf("expected 35 test vectors, got %d", len(vectors))
	}

	for i, tv := range vectors {
		t.Run(fmt.Sprintf("vector_%d_%s_%s", i, tv.Version, tv.Input), func(t *testing.T) {
			version := parseVersion(tv.Version)

			var image Image
			if tv.InputType == "hex" {
				if tv.Input == "" {
					image = MakeFromData([]byte{}, version, tv.ModuleSize, tv.HasAlpha)
				} else {
					inputData, err := hex.DecodeString(tv.Input)
					if err != nil {
						t.Fatal(err)
					}
					image = MakeFromData(inputData, version, tv.ModuleSize, tv.HasAlpha)
				}
			} else {
				image = MakeFromUTF8(tv.Input, version, tv.ModuleSize, tv.HasAlpha)
			}

			if image.Width != tv.Width {
				t.Errorf("width mismatch: got %d, want %d", image.Width, tv.Width)
			}
			if image.Height != tv.Height {
				t.Errorf("height mismatch: got %d, want %d", image.Height, tv.Height)
			}
			if len(image.Colors) != len(tv.Colors) {
				t.Fatalf("colors length mismatch: got %d, want %d", len(image.Colors), len(tv.Colors))
			}

			for j := range image.Colors {
				if image.Colors[j] != tv.Colors[j] {
					components := 3
					if tv.HasAlpha {
						components = 4
					}
					pixel := j / components
					component := j % components
					compName := []string{"R", "G", "B", "A"}[component]
					t.Fatalf("pixel data mismatch at byte %d (pixel %d, %s): got %d, want %d",
						j, pixel, compName, image.Colors[j], tv.Colors[j])
				}
			}
		})
	}
}
