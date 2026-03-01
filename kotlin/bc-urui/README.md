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

## Features

### UR Display
Animated multi-part QR code display using fountain codes.

- **`URDisplayState`** — Compose-observable state machine that drives animated UR display. Wraps `MultipartEncoder`, cycles through fountain-coded parts on a configurable timer (`framesPerSecond`), and exposes the current QR data and fragment states.
- **`URQRCode`** — Composable that renders a QR code from `ByteArray`, with configurable foreground/background colors.

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
- **`makeQRCodeBitmap()`** — Generates an Android `Bitmap` QR code from byte data.
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
