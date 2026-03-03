# bc-urui (Kotlin/Android)

A Jetpack Compose component library for displaying and scanning [Uniform Resources (URs)](https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2020-005-ur.md) encoded as QR codes. Supports both real camera capture (via CameraX + ML Kit) and simulated scanning for use in Android Emulator and automated tests.

## Requirements

- Android SDK 26+ (compile SDK 35)
- Kotlin 2.2+ with Jetpack Compose
- Java 21
- Depends on the in-repo `bc-ur` Kotlin package

### External Dependencies

- Jetpack Compose (UI, Foundation, Material3)
- CameraX (camera-core, camera2, camera-lifecycle, camera-view)
- ML Kit Barcode Scanning
- ZXing Core (QR code generation)
- AndroidSVG (SVG rendering for logo overlay)

## Features

### UR Display
Animated multi-part QR code display using fountain codes.

- **`URDisplayState`** — Compose-observable state machine that drives animated UR display. Wraps `MultipartEncoder`, cycles through fountain-coded parts on a configurable timer (`framesPerSecond`), and exposes the current QR data and fragment states.
- **`URQRCode`** — Composable that renders a QR code from `ByteArray`, with configurable foreground/background colors and optional logo overlay.
- **`QRLogo`** — A logo image to superimpose on the center of a QR code. Can be created from SVG data/string or a `Bitmap`. Supports configurable clear-module border width (0–5) and square or circular clearing shape. When a logo is present, error correction is automatically raised to Level Q (Quartile) to ensure scannability.

### UR Scanning (Camera)
Real-time QR code scanning via the device camera.

- **`URVideoSession`** — Manages a CameraX capture session with ML Kit barcode detection. Delivers scanned code strings via `onCodesScanned` callback. Deduplicates consecutive identical scans.
- **`URVideo`** — Composable wrapper that embeds `URVideoSession` in a Compose layout via `AndroidView`.
- **`URScanState`** — Compose-observable scan state machine. Accepts QR code strings via `receiveCodes()`, handles both single-part and multi-part URs, and reports progress or decoded results through `lastResult`.

### UR Scanning (Simulated)
Software-only scan simulation for environments without camera hardware.

- **`URSimulatedScanState`** — Compose-observable state machine that encodes a UR into fountain-coded parts, feeds the part strings to a `URScanState` on a coroutine timer, and auto-stops on successful decode. Configurable scan speed, start fragment, and fragment length.
- **`URSimulatedScan`** — Composable that shows the QR codes being "scanned" as visual feedback alongside a fragment progress bar.
- **`StartFragment`** — Sealed class specifying where to begin simulated scanning: `First`, `Index(index)`, or `Random`.

### Shared UI Components

- **`URFragmentBar`** — Displays fragment status indicators (off / on / highlighted) for multi-part UR progress.
- **`URProgressBar`** — Animated linear progress bar.
- **`FragmentState`** — Enum representing individual fragment display state: `Off`, `On`, `Highlighted`.

### Utilities

- **`QRCorrectionLevel`** — Enum wrapping ZXing error correction levels (Low, Medium, Quartile, High).
- **`QRLogo`** — Logo image for QR code overlay, with `fromSVG()` and `fromBitmap()` factory methods.
- **`makeQRCodeBitmap()`** — Generates an Android `Bitmap` QR code from byte data, with optional logo overlay.
- **`URScanResult`** — Sealed class for scan outcomes: `Ur`, `Other`, `Progress`, `Reject`, `Failure`.
- **`URScanProgress`** — Data class holding completion percentage and fragment states.

## API Overview

### Displaying a UR

```kotlin
val displayState = URDisplayState(ur = myUR, maxFragmentLen = 200)
displayState.framesPerSecond = 8.0

// In a Composable:
URQRCode(data = displayState.part)
URFragmentBar(states = displayState.fragmentStates)

// Lifecycle (needs a CoroutineScope):
displayState.run(scope)  // start animation
displayState.stop()      // pause animation
displayState.restart()   // reset to beginning
```

### Scanning with Camera

```kotlin
val scanState = URScanState()
val videoSession = URVideoSession { codes ->
    scanState.receiveCodes(codes)
}

// In a Composable:
URVideo(videoSession = videoSession)

// Observe results:
when (val result = scanState.lastResult) {
    is URScanResult.Ur       -> result.ur          // complete UR decoded
    is URScanResult.Progress -> result.progress     // estimatedPercentComplete, fragmentStates
    is URScanResult.Other    -> result.code         // non-UR QR code
    is URScanResult.Reject   -> { /* rejected */ }
    is URScanResult.Failure  -> result.error        // Throwable
    null                     -> { /* no result */ }
}
```

### Simulated Scanning

```kotlin
val scanState = URScanState()
val simState = URSimulatedScanState(
    ur = myUR,
    scanState = scanState,
    maxFragmentLen = 200,
    secondsPerFrame = 0.5,
    startFragment = StartFragment.First  // or Index(3), Random
)

// In a Composable:
URSimulatedScan(state = simState)  // starts/stops with lifecycle

// Or manual control (needs a CoroutineScope):
simState.run(scope)        // start feeding parts
simState.stop()            // pause
simState.restart(scope)    // reset encoder and scan state, restart
```

### QR Code Generation

```kotlin
val bitmap: Bitmap = makeQRCodeBitmap(
    message = data,
    correctionLevel = QRCorrectionLevel.Low,
    size = 512,
    foregroundColor = Color.BLACK,
    backgroundColor = Color.TRANSPARENT
)
```

### Logo Overlay

Superimpose an SVG or bitmap logo on the center of a QR code. Error correction is automatically raised to Level Q when a logo is present.

```kotlin
// From SVG data:
val logo = QRLogo.fromSVG(svgBytes, fraction = 0.25f)

// From an SVG string:
val logo = QRLogo.fromSVG(svgString, fraction = 0.25f)

// From a Bitmap:
val logo = QRLogo.fromBitmap(myBitmap, fraction = 0.25f)

// With custom clear border and circular mask:
val logo = QRLogo.fromSVG(svgBytes, clearBorder = 2, clearShape = QRLogoClearShape.Circle)

// Use with URQRCode composable:
URQRCode(data = displayState.part, logo = logo)

// Use with standalone generation:
val bitmap = makeQRCodeBitmap(message = data, logo = logo)
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `fraction` | `Float` | `0.25f` | Logo width as a fraction of the QR code width. Clamped to 0.01..0.99. The logo area is capped so the cleared region never exceeds 40% of the QR width. |
| `clearBorder` | `Int` | `1` | Number of clear modules around the logo (0..5). |
| `clearShape` | `QRLogoClearShape` | `Square` | Shape of the cleared area: `Square` (rectangular) or `Circle` (circular module-level mask — a module is cleared only if its center falls within the circle radius). |
