import Foundation
import AVFoundation
import CoreGraphics
import ImageIO
import Observation
import Vision
import os

let logger = Logger(subsystem: Bundle.main.bundleIdentifier!, category: "BCURUI")

public struct URVideoSessionError: LocalizedError, Sendable {
    let description: String

    init(_ description: String) {
        self.description = description
    }

    public var errorDescription: String? {
        description
    }
}

@MainActor
@Observable
public final class URVideoSession {
    public let isSupported: Bool
    public weak var codesReceiver: (any URCodesReceiver)?
    public weak var textReceiver: (any URTextReceiver)? {
        didSet {
            videoDataDelegate?.isEnabled = textReceiver != nil
        }
    }

    /// The set of rotation angles (in degrees: 0, 90, 180, 270) at which text
    /// recognition should be attempted each frame. Defaults to `[0]` (upright only).
    public var textRecognitionRotations: Set<Int> = [0] {
        didSet {
            videoDataDelegate?.textRecognitionRotations = textRecognitionRotations
        }
    }

    /// The aspect ratio (width / height) of the camera image in the current
    /// device orientation. Used by overlays to correct for `resizeAspectFill` crop.
    public private(set) var orientedImageAspectRatio: CGFloat = 1.0

    public private(set) var captureDevices: [AVCaptureDevice] = []
    public private(set) var currentCaptureDevice: AVCaptureDevice?

    private(set) var captureSession: AVCaptureSession!
    private(set) var previewLayer: AVCaptureVideoPreviewLayer?
    private var discoverySession: AVCaptureDevice.DiscoverySession!
    private var metadataObjectsDelegate: MetadataObjectsDelegate!
    private var videoDataDelegate: VideoDataDelegate!
    private let queue = DispatchQueue(label: "codes", qos: .userInteractive)
    private let videoDataQueue = DispatchQueue(label: "textRecognition", qos: .userInitiated)

    public func setCaptureDevice(_ newDevice: AVCaptureDevice) {
        do {
            guard let captureSession else { return }

            captureSession.beginConfiguration()
            if let currentInput = captureSession.inputs.first {
                captureSession.removeInput(currentInput)
            }
            let newInput = try AVCaptureDeviceInput(device: newDevice)
            captureSession.addInput(newInput)
            captureSession.commitConfiguration()
            currentCaptureDevice = newDevice
            Self.configureAutofocus(for: newDevice)
            updateOrientedAspectRatio()
        } catch {
            logger.error("⛔️ \(error.localizedDescription)")
        }
    }

    public init() {
        #if targetEnvironment(simulator)
        isSupported = false
        return
        #else

        isSupported = true

        do {
            // Prefer virtual multi-camera devices that include the ultra-wide
            // lens for automatic macro switching at close range. Fall back to
            // the plain wide-angle camera on older devices.
            discoverySession = .init(
                deviceTypes: [
                    .builtInTripleCamera,
                    .builtInDualWideCamera,
                    .builtInWideAngleCamera,
                ],
                mediaType: .video,
                position: .back
            )
            captureDevices = discoverySession.devices

            guard let currentCaptureDevice = discoverySession.devices.first else {
                throw URVideoSessionError("Could not open video capture device.")
            }

            self.currentCaptureDevice = currentCaptureDevice
            Self.configureAutofocus(for: currentCaptureDevice)

            let videoInput = try AVCaptureDeviceInput(device: currentCaptureDevice)
            captureSession = AVCaptureSession()
            guard captureSession.canAddInput(videoInput) else {
                throw URVideoSessionError("Could not add video input device.")
            }
            captureSession.addInput(videoInput)

            metadataObjectsDelegate = MetadataObjectsDelegate(videoSession: self)

            let metadataOutput = AVCaptureMetadataOutput()
            guard captureSession.canAddOutput(metadataOutput) else {
                throw URVideoSessionError("Could not add metadata output.")
            }
            captureSession.addOutput(metadataOutput)

            metadataOutput.metadataObjectTypes = [.qr]
            metadataOutput.setMetadataObjectsDelegate(metadataObjectsDelegate, queue: queue)

            videoDataDelegate = VideoDataDelegate(videoSession: self)
            videoDataDelegate.textRecognitionRotations = textRecognitionRotations
            updateOrientedAspectRatio()
            let videoDataOutput = AVCaptureVideoDataOutput()
            videoDataOutput.setSampleBufferDelegate(videoDataDelegate, queue: videoDataQueue)
            videoDataOutput.alwaysDiscardsLateVideoFrames = true
            if captureSession.canAddOutput(videoDataOutput) {
                captureSession.addOutput(videoDataOutput)
            }

            previewLayer = AVCaptureVideoPreviewLayer(session: captureSession)
            previewLayer!.videoGravity = .resizeAspectFill
        } catch {
            logger.error("⛔️ \(error.localizedDescription)")
        }
        #endif
    }

    /// Configures continuous autofocus biased toward near (macro) distances
    /// for reliable QR code and text scanning at close range.
    private static func configureAutofocus(for device: AVCaptureDevice) {
        do {
            try device.lockForConfiguration()
            if device.isFocusModeSupported(.continuousAutoFocus) {
                device.focusMode = .continuousAutoFocus
            }
            if device.isAutoFocusRangeRestrictionSupported {
                device.autoFocusRangeRestriction = .near
            }
            device.unlockForConfiguration()
        } catch {
            logger.error("⛔️ Failed to configure autofocus: \(error.localizedDescription)")
        }
    }

    func startRunning() {
        guard let captureSession else { return }
        if !captureSession.isRunning {
            Task {
                captureSession.startRunning()
            }
        }
    }

    func stopRunning() {
        guard let captureSession else { return }
        if captureSession.isRunning {
            captureSession.stopRunning()
        }
    }

    var isRunning: Bool {
        captureSession?.isRunning ?? false
    }

    func updateTextRecognitionOrientation(_ orientation: CGImagePropertyOrientation) {
        videoDataDelegate?.imageOrientation = orientation
        updateOrientedAspectRatio()
    }

    private func updateOrientedAspectRatio() {
        guard let device = currentCaptureDevice else { return }
        let dims = CMVideoFormatDescriptionGetDimensions(device.activeFormat.formatDescription)
        let sensorW = CGFloat(dims.width)
        let sensorH = CGFloat(dims.height)
        // Sensor dimensions are landscape. For portrait orientations, swap.
        let orientation = videoDataDelegate?.imageOrientation ?? .right
        switch orientation {
        case .left, .right, .leftMirrored, .rightMirrored:
            orientedImageAspectRatio = sensorH / sensorW
        default:
            orientedImageAspectRatio = sensorW / sensorH
        }
    }

    fileprivate func deliverCodes(_ codes: Swift.Set<String>) {
        Task { @MainActor in
            codesReceiver?.receiveCodes(codes)
        }
    }

    @objc
    class VideoDataDelegate: NSObject, AVCaptureVideoDataOutputSampleBufferDelegate {
        weak var videoSession: URVideoSession?
        /// Toggled from `@MainActor` when `textReceiver` is set/cleared.
        /// Read from the video data queue. A benign race at most skips or
        /// processes one extra frame.
        nonisolated(unsafe) var isEnabled: Bool = false
        nonisolated(unsafe) var imageOrientation: CGImagePropertyOrientation = .right
        nonisolated(unsafe) var textRecognitionRotations: Set<Int> = [0]
        private var lastProcessTime: CFAbsoluteTime = 0
        /// Process at ~6-7 fps to keep CPU usage low.
        private let minInterval: CFAbsoluteTime = 0.15

        init(videoSession: URVideoSession) {
            self.videoSession = videoSession
        }

        func captureOutput(
            _ output: AVCaptureOutput,
            didOutput sampleBuffer: CMSampleBuffer,
            from connection: AVCaptureConnection
        ) {
            // Zero-cost gate: skip if no one is listening.
            guard isEnabled else { return }

            // Throttle to avoid saturating the CPU.
            let now = CFAbsoluteTimeGetCurrent()
            guard now - lastProcessTime >= minInterval else { return }
            lastProcessTime = now

            guard let pixelBuffer = CMSampleBufferGetImageBuffer(sampleBuffer) else { return }

            var allTexts: [URRecognizedText] = []

            for extraRotation in textRecognitionRotations {
                let composedOrientation = Self.composeOrientation(
                    base: imageOrientation,
                    extraRotation: extraRotation
                )

                let request = VNRecognizeTextRequest()
                request.recognitionLevel = .fast
                request.usesLanguageCorrection = false

                let handler = VNImageRequestHandler(
                    cvPixelBuffer: pixelBuffer,
                    orientation: composedOrientation,
                    options: [:]
                )
                do {
                    try handler.perform([request])
                } catch {
                    continue
                }

                guard let observations = request.results, !observations.isEmpty else { continue }

                let texts = observations.compactMap { observation -> URRecognizedText? in
                    guard let candidate = observation.topCandidates(1).first else { return nil }
                    guard candidate.confidence >= 0.5 else { return nil }
                    // Vision uses bottom-left origin; flip to top-left origin.
                    let visionBox = observation.boundingBox
                    let flippedBox = CGRect(
                        x: visionBox.origin.x,
                        y: 1.0 - visionBox.origin.y - visionBox.height,
                        width: visionBox.width,
                        height: visionBox.height
                    )
                    let displayBox = Self.transformToDisplay(
                        box: flippedBox,
                        rotation: extraRotation
                    )
                    return URRecognizedText(
                        text: candidate.string,
                        boundingBox: displayBox,
                        confidence: candidate.confidence,
                        rotation: extraRotation
                    )
                }

                allTexts.append(contentsOf: texts)
            }

            guard !allTexts.isEmpty else { return }
            let session = videoSession
            Task { @MainActor in
                session?.textReceiver?.receiveRecognizedText(allTexts)
            }
        }

        // MARK: - Orientation Helpers

        /// Maps a `CGImagePropertyOrientation` to degrees (0, 90, 180, 270).
        private static func orientationToDegrees(_ orientation: CGImagePropertyOrientation) -> Int {
            switch orientation {
            case .up:    return 0
            case .right: return 90
            case .down:  return 180
            case .left:  return 270
            default:     return 0
            }
        }

        /// Maps degrees (0, 90, 180, 270) to a `CGImagePropertyOrientation`.
        private static func degreesToOrientation(_ degrees: Int) -> CGImagePropertyOrientation {
            switch ((degrees % 360) + 360) % 360 {
            case 0:   return .up
            case 90:  return .right
            case 180: return .down
            case 270: return .left
            default:  return .up
            }
        }

        /// Composes the base device orientation with an extra rotation (0/90/180/270°).
        static func composeOrientation(
            base: CGImagePropertyOrientation,
            extraRotation: Int
        ) -> CGImagePropertyOrientation {
            let baseDegrees = orientationToDegrees(base)
            let total = (baseDegrees + extraRotation) % 360
            return degreesToOrientation(total)
        }

        /// Transforms a normalized bounding box from a rotated coordinate space
        /// back to display coordinates.
        ///
        /// Vision returns bounding boxes relative to the (rotated) image.
        /// This maps them back so they overlay correctly on the camera preview.
        static func transformToDisplay(box: CGRect, rotation: Int) -> CGRect {
            let vx = box.origin.x
            let vy = box.origin.y
            let vw = box.width
            let vh = box.height

            switch ((rotation % 360) + 360) % 360 {
            case 0:
                return box
            case 90:
                return CGRect(x: vy, y: 1 - vx - vw, width: vh, height: vw)
            case 180:
                return CGRect(x: 1 - vx - vw, y: 1 - vy - vh, width: vw, height: vh)
            case 270:
                return CGRect(x: 1 - vy - vh, y: vx, width: vh, height: vw)
            default:
                return box
            }
        }
    }

    @objc
    class MetadataObjectsDelegate: NSObject, AVCaptureMetadataOutputObjectsDelegate {
        weak var videoSession: URVideoSession?
        var lastFound: Swift.Set<String> = []

        init(videoSession: URVideoSession) {
            self.videoSession = videoSession
        }

        func metadataOutput(
            _ output: AVCaptureMetadataOutput,
            didOutput metadataObjects: [AVMetadataObject],
            from connection: AVCaptureConnection
        ) {
            let codes = Set(metadataObjects.compactMap {
                ($0 as? AVMetadataMachineReadableCodeObject)?.stringValue
            })
            if !codes.isEmpty, codes != lastFound {
                lastFound = codes
                let session = videoSession
                Task { @MainActor in
                    session?.deliverCodes(codes)
                }
            }
        }
    }
}
