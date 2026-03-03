# BCURUI (Swift)

A SwiftUI component library for displaying and scanning [Uniform Resources (URs)](https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2020-005-ur.md) encoded as QR codes. Supports both real camera capture (via AVFoundation) and simulated scanning for use in iOS Simulator and automated tests.

## Requirements

- Swift 6.0+
- iOS 18+ / Mac Catalyst 18+ / visionOS 2+
- Depends on the in-repo `BCUR` Swift package

## Features

### UR Display
Animated multi-part QR code display using fountain codes.

- **`URDisplayState`** — Observable state machine that drives animated UR display. Wraps `MultipartEncoder`, cycles through fountain-coded parts on a configurable timer (`framesPerSecond`), and exposes the current QR data and fragment states.
- **`URQRCode`** — SwiftUI view that renders a QR code from `Data`, with configurable foreground/background colors and optional logo overlay.
- **`QRLogo`** — A logo image to superimpose on the center of a QR code. Can be created from SVG data or a `UIImage`. Supports configurable clear-module border width (0–5) and square or circular clearing shape. When a logo is present, error correction is automatically raised to Level Q (Quartile) to ensure scannability.

### UR Scanning (Camera)
Real-time QR code scanning via the device camera.

- **`URVideoSession`** — Manages an AVFoundation capture session for QR code detection. Delivers scanned code strings to a `URCodesReceiver` delegate. Gracefully reports unsupported on Simulator.
- **`URVideo`** — `UIViewRepresentable` wrapper that embeds `URVideoSession` in a SwiftUI layout.
- **`URUIVideoView`** — The underlying UIKit view for camera preview. Handles orientation and lifecycle.
- **`URScanState`** — Observable scan state machine. Accepts QR code strings via `receiveCodes(_:)`, handles both single-part and multi-part URs, and reports progress or decoded results through `lastResult`.
- **`URCodesReceiver`** — Protocol for objects that receive scanned QR code sets.

### UR Scanning (Simulated)
Software-only scan simulation for environments without camera hardware.

- **`URSimulatedScanState`** — Observable state machine that encodes a UR into fountain-coded parts, feeds the part strings to a `URScanState` on a timer, and auto-stops on successful decode. Configurable scan speed, start fragment, and fragment length.
- **`URSimulatedScan`** — SwiftUI view that shows the QR codes being "scanned" as visual feedback alongside a fragment progress bar.
- **`StartFragment`** — Enum specifying where to begin simulated scanning: `.first`, `.index(Int)`, or `.random`.

### Shared UI Components

- **`URFragmentBar`** — Displays fragment status indicators (off / on / highlighted) for multi-part UR progress.
- **`URProgressBar`** — Animated linear progress bar.

## API Overview

### Displaying a UR

```swift
let displayState = URDisplayState(ur: myUR, maxFragmentLen: 200)
displayState.framesPerSecond = 8.0

// In a SwiftUI view:
URQRCode(data: $displayState.part)
URFragmentBar(states: $displayState.fragmentStates)

// Lifecycle:
displayState.run()    // start animation
displayState.stop()   // pause animation
displayState.restart() // reset to beginning
```

### Scanning with Camera

```swift
let scanState = URScanState()
let videoSession = URVideoSession()
videoSession.codesReceiver = scanState

// In a SwiftUI view:
URVideo(videoSession: videoSession)

// Observe results:
switch scanState.lastResult {
case .ur(let ur):       // complete UR decoded
case .progress(let p):  // p.estimatedPercentComplete, p.fragmentStates
case .other(let code):  // non-UR QR code
case .reject:           // rejected fragment
case .failure(let err): // error
case nil:               // no result yet
}
```

### Simulated Scanning

```swift
let scanState = URScanState()
let simState = URSimulatedScanState(
    ur: myUR,
    scanState: scanState,
    maxFragmentLen: 200,
    secondsPerFrame: 0.5,
    startFragment: .first  // or .index(3), .random
)

// In a SwiftUI view:
URSimulatedScan(state: simState)  // starts/stops on appear/disappear

// Or manual control:
simState.run()      // start feeding parts
simState.stop()     // pause
simState.restart()  // reset encoder and scan state, restart
```

### QR Code Generation

```swift
let image: Image = makeQRCode(data, correctionLevel: .low)
let uiImage: UIImage = makeQRCodeImage(data, correctionLevel: .medium,
    foregroundColor: .black, backgroundColor: .white)
```

### Logo Overlay

Superimpose an SVG or image logo on the center of a QR code. Error correction is automatically raised to Level Q when a logo is present.

```swift
// From SVG data:
let logo = QRLogo(svgData: mySVGData, fraction: 0.25)

// From a UIImage:
let logo = QRLogo(image: myUIImage, fraction: 0.25)

// With custom clear border and circular mask:
let logo = QRLogo(image: myUIImage, clearBorder: 2, clearShape: .circle)

// Use with URQRCode view:
URQRCode(data: $displayState.part, logo: logo)

// Use with standalone generation:
let image: Image = makeQRCode(data, logo: logo)
let uiImage: UIImage = makeQRCodeImage(data, logo: logo)
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `fraction` | `CGFloat` | `0.25` | Logo width as a fraction of the QR code width. Clamped to 0.01...0.99. The logo area is capped so the cleared region never exceeds 40% of the QR width. |
| `clearBorder` | `Int` | `1` | Number of clear modules around the logo (0...5). |
| `clearShape` | `QRLogoClearShape` | `.square` | Shape of the cleared area: `.square` (rectangular) or `.circle` (circular module-level mask — a module is cleared only if its center falls within the circle radius). |
