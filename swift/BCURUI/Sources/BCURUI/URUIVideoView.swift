import UIKit
import AVFoundation

/// A UIKit view that shows video preview, intended to be wrapped by `URVideo`.
public class URUIVideoView: UIView {
    let videoSession: URVideoSession

    init(videoSession: URVideoSession) {
        self.videoSession = videoSession
        super.init(frame: .zero)
        guard videoSession.isSupported else { return }
        translatesAutoresizingMaskIntoConstraints = false
        guard let previewLayer = videoSession.previewLayer else { return }
        layer.addSublayer(previewLayer)
    }

    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }

    public override func layoutSubviews() {
        super.layoutSubviews()
        syncVideoSizeAndOrientation()
    }

    public override func didMoveToSuperview() {
        super.didMoveToSuperview()
        guard videoSession.isSupported else { return }
        if superview == nil {
            videoSession.stopRunning()
        } else {
            videoSession.startRunning()
        }
    }

    private func syncVideoSizeAndOrientation() {
        guard
            videoSession.isSupported,
            let previewLayer = videoSession.previewLayer
        else {
            return
        }
        previewLayer.frame = bounds
        if let connection = videoSession.captureSession?.connections.last {
            let angle = rotationAngle(for: window?.windowScene?.interfaceOrientation ?? .portrait)
            if connection.isVideoRotationAngleSupported(angle) {
                connection.videoRotationAngle = angle
            }
        }
    }

    private func rotationAngle(for orientation: UIInterfaceOrientation) -> CGFloat {
        switch orientation {
        case .portrait:           return 90
        case .portraitUpsideDown: return 270
        case .landscapeLeft:      return 180
        case .landscapeRight:     return 0
        default:                  return 90
        }
    }
}
