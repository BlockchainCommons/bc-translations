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
            discoverySession = .init(
                deviceTypes: [.builtInWideAngleCamera],
                mediaType: .video,
                position: .unspecified
            )
            captureDevices = discoverySession.devices

            guard let currentCaptureDevice = AVCaptureDevice.default(for: .video) else {
                throw URVideoSessionError("Could not open video capture device.")
            }

            self.currentCaptureDevice = currentCaptureDevice

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

            let request = VNRecognizeTextRequest()
            request.recognitionLevel = .fast
            request.usesLanguageCorrection = false

            let handler = VNImageRequestHandler(cvPixelBuffer: pixelBuffer, orientation: imageOrientation, options: [:])
            do {
                try handler.perform([request])
            } catch {
                return
            }

            guard let observations = request.results, !observations.isEmpty else { return }

            let texts = observations.compactMap { observation -> URRecognizedText? in
                guard let candidate = observation.topCandidates(1).first else { return nil }
                // Vision uses bottom-left origin; flip to top-left origin.
                let visionBox = observation.boundingBox
                let flippedBox = CGRect(
                    x: visionBox.origin.x,
                    y: 1.0 - visionBox.origin.y - visionBox.height,
                    width: visionBox.width,
                    height: visionBox.height
                )
                return URRecognizedText(
                    text: candidate.string,
                    boundingBox: flippedBox,
                    confidence: candidate.confidence
                )
            }

            guard !texts.isEmpty else { return }
            let session = videoSession
            Task { @MainActor in
                session?.textReceiver?.receiveRecognizedText(texts)
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
