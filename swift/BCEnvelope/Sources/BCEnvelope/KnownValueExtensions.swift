import Foundation
import BCComponents

extension KnownValue: EnvelopeEncodable {
    public var envelope: Envelope {
        Envelope(self)
    }
}
