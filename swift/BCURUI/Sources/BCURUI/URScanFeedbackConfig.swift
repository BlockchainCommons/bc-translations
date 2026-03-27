import Foundation

public struct URScanFeedbackConfig: Sendable {
    public var hapticEnabled: Bool
    public var soundEnabled: Bool
    public var clickSoundURL: URL?
    public var successSoundURL: URL?
    public var failureSoundURL: URL?

    public init(
        hapticEnabled: Bool = true,
        soundEnabled: Bool = false,
        clickSoundURL: URL? = nil,
        successSoundURL: URL? = nil,
        failureSoundURL: URL? = nil
    ) {
        self.hapticEnabled = hapticEnabled
        self.soundEnabled = soundEnabled
        self.clickSoundURL = clickSoundURL
        self.successSoundURL = successSoundURL
        self.failureSoundURL = failureSoundURL
    }

    public static let `default` = URScanFeedbackConfig()

    public static func hapticOnly(_ enabled: Bool) -> URScanFeedbackConfig {
        URScanFeedbackConfig(hapticEnabled: enabled)
    }
}
