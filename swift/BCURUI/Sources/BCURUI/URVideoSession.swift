import Foundation
import AVFoundation
import Observation
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
    let isSupported: Bool
    public weak var codesReceiver: (any URCodesReceiver)?

    public private(set) var captureDevices: [AVCaptureDevice] = []
    public private(set) var currentCaptureDevice: AVCaptureDevice?

    private(set) var captureSession: AVCaptureSession!
    private(set) var previewLayer: AVCaptureVideoPreviewLayer?
    private var discoverySession: AVCaptureDevice.DiscoverySession!
    private var metadataObjectsDelegate: MetadataObjectsDelegate!
    private let queue = DispatchQueue(label: "codes", qos: .userInteractive)

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

    fileprivate func deliverCodes(_ codes: Swift.Set<String>) {
        Task { @MainActor in
            codesReceiver?.receiveCodes(codes)
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
