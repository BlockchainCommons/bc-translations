import Foundation

/// A provider of deterministic key material used to derive private keys.
public protocol PrivateKeyDataProvider {
    func privateKeyData() -> Data
}

extension Data: PrivateKeyDataProvider {
    public func privateKeyData() -> Data {
        self
    }
}
