import Foundation

public protocol DigestProvider {
    func digest() -> Digest
}

extension Data: DigestProvider {
    public func digest() -> Digest {
        Digest.fromImage(self)
    }
}
