// Package bclifehash implements the LifeHash visual hashing algorithm.
//
// LifeHash is a method of hash visualization based on Conway's Game of Life
// that creates beautiful icons that are deterministic, yet distinct and unique
// given the input data.
//
// The basic concept is to take a SHA-256 hash of the input data and then use
// the 256-bit digest as a 16×16 pixel "seed" for running Conway's Game of
// Life. After the pattern becomes stable (or begins repeating) the resulting
// history is used to compile a grayscale image. Bits of the initial hash are
// then used to deterministically apply symmetry and color.
package bclifehash

import (
	"crypto/sha256"
	"math"
)

// Version selects which LifeHash algorithm variant to use.
type Version int

const (
	// Version1 uses a 16×16 grid, up to 150 generations.
	Version1 Version = iota
	// Version2 uses a 16×16 grid with an extra SHA-256 pass, up to 150 generations.
	Version2
	// Detailed uses a 32×32 grid, up to 300 generations, richer color gradients.
	Detailed
	// Fiducial uses a 32×32 grid designed for use as fiducial markers.
	Fiducial
	// GrayscaleFiducial is the same as Fiducial but rendered in grayscale.
	GrayscaleFiducial
)

func (v Version) String() string {
	switch v {
	case Version1:
		return "version1"
	case Version2:
		return "version2"
	case Detailed:
		return "detailed"
	case Fiducial:
		return "fiducial"
	case GrayscaleFiducial:
		return "grayscale_fiducial"
	default:
		return "unknown"
	}
}

// Image holds the output of a LifeHash generation.
type Image struct {
	Width  int
	Height int
	// Colors contains RGB (or RGBA if hasAlpha was true) pixel data,
	// row-major, 3 (or 4) bytes per pixel.
	Colors []byte
}

func sha256Hash(data []byte) []byte {
	h := sha256.Sum256(data)
	return h[:]
}

func makeImage(width, height int, floatColors []float64, moduleSize int, hasAlpha bool) Image {
	if moduleSize <= 0 {
		panic("invalid module size")
	}

	scaledWidth := width * moduleSize
	scaledHeight := height * moduleSize
	resultComponents := 3
	if hasAlpha {
		resultComponents = 4
	}
	scaledCapacity := scaledWidth * scaledHeight * resultComponents

	resultColors := make([]byte, scaledCapacity)

	for targetY := 0; targetY < scaledHeight; targetY++ {
		for targetX := 0; targetX < scaledWidth; targetX++ {
			sourceX := targetX / moduleSize
			sourceY := targetY / moduleSize
			sourceOffset := (sourceY*width + sourceX) * 3

			targetOffset := (targetY*scaledWidth + targetX) * resultComponents

			resultColors[targetOffset] = byte(clamped(floatColors[sourceOffset]) * 255.0)
			resultColors[targetOffset+1] = byte(clamped(floatColors[sourceOffset+1]) * 255.0)
			resultColors[targetOffset+2] = byte(clamped(floatColors[sourceOffset+2]) * 255.0)
			if hasAlpha {
				resultColors[targetOffset+3] = 255
			}
		}
	}

	return Image{
		Width:  scaledWidth,
		Height: scaledHeight,
		Colors: resultColors,
	}
}

// MakeFromUTF8 generates a LifeHash image from a UTF-8 string.
func MakeFromUTF8(s string, version Version, moduleSize int, hasAlpha bool) Image {
	return MakeFromData([]byte(s), version, moduleSize, hasAlpha)
}

// MakeFromData generates a LifeHash image from raw bytes.
func MakeFromData(data []byte, version Version, moduleSize int, hasAlpha bool) Image {
	digest := sha256Hash(data)
	return MakeFromDigest(digest, version, moduleSize, hasAlpha)
}

// MakeFromDigest generates a LifeHash image from a pre-computed 32-byte SHA-256 digest.
func MakeFromDigest(digest []byte, version Version, moduleSize int, hasAlpha bool) Image {
	if len(digest) != 32 {
		panic("digest must be 32 bytes")
	}

	var length, maxGenerations int
	switch version {
	case Version1, Version2:
		length = 16
		maxGenerations = 150
	default:
		length = 32
		maxGenerations = 300
	}

	currentCellGrid := newCellGrid(length, length)
	nextCellGrid := newCellGrid(length, length)
	currentChangeGrid := newChangeGrid(length, length)
	nextChangeGrid := newChangeGrid(length, length)

	switch version {
	case Version1:
		nextCellGrid.setData(digest)
	case Version2:
		hashed := sha256Hash(digest)
		nextCellGrid.setData(hashed)
	default: // Detailed, Fiducial, GrayscaleFiducial
		digest1 := make([]byte, len(digest))
		copy(digest1, digest)
		if version == GrayscaleFiducial {
			digest1 = sha256Hash(digest1)
		}
		digest2 := sha256Hash(digest1)
		digest3 := sha256Hash(digest2)
		digest4 := sha256Hash(digest3)
		digestFinal := make([]byte, 0, 128)
		digestFinal = append(digestFinal, digest1...)
		digestFinal = append(digestFinal, digest2...)
		digestFinal = append(digestFinal, digest3...)
		digestFinal = append(digestFinal, digest4...)
		nextCellGrid.setData(digestFinal)
	}

	nextChangeGrid.grid.setAll(true)

	historySet := make(map[string]struct{})
	var history [][]byte

	for len(history) < maxGenerations {
		currentCellGrid, nextCellGrid = nextCellGrid, currentCellGrid
		currentChangeGrid, nextChangeGrid = nextChangeGrid, currentChangeGrid

		data := currentCellGrid.data()
		hash := sha256Hash(data)
		hashKey := string(hash)
		if _, exists := historySet[hashKey]; exists {
			break
		}
		historySet[hashKey] = struct{}{}
		history = append(history, data)

		currentCellGrid.nextGeneration(
			&currentChangeGrid,
			&nextCellGrid,
			&nextChangeGrid,
		)
	}

	fg := newFracGrid(length, length)
	for i, h := range history {
		currentCellGrid.setData(h)
		frac := clamped(lerpFrom(0.0, float64(len(history)), float64(i+1)))
		fg.overlay(&currentCellGrid, frac)
	}

	// Normalize the frac grid to [0, 1] (except version1)
	if version != Version1 {
		minValue := math.Inf(1)
		maxValue := math.Inf(-1)
		fg.grid.forAll(func(x, y int) {
			value := fg.grid.getValue(x, y)
			if value < minValue {
				minValue = value
			}
			if value > maxValue {
				maxValue = value
			}
		})

		w := fg.grid.width
		h := fg.grid.height
		for y := 0; y < h; y++ {
			for x := 0; x < w; x++ {
				value := fg.grid.getValue(x, y)
				normalized := lerpFrom(minValue, maxValue, value)
				fg.grid.setValue(normalized, x, y)
			}
		}
	}

	entropy := newBitEnumerator(append([]byte(nil), digest...))

	switch version {
	case Detailed:
		entropy.next()
	case Version2:
		entropy.nextUint2()
	}

	gradient := selectGradient(&entropy, version)
	pat := selectPattern(&entropy, version)
	cg := newColorGrid(&fg, gradient, pat)

	return makeImage(
		cg.grid.width,
		cg.grid.height,
		cg.colors(),
		moduleSize,
		hasAlpha,
	)
}
