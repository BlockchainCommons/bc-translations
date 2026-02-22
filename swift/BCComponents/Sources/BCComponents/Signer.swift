import Foundation

public protocol Signer {
    func signWithOptions(
        _ message: some DataProtocol,
        options: SigningOptions?
    ) throws(BCComponentsError) -> Signature

    func sign(_ message: some DataProtocol) throws(BCComponentsError) -> Signature
}

public extension Signer {
    func sign(_ message: some DataProtocol) throws(BCComponentsError) -> Signature {
        try signWithOptions(message, options: nil)
    }
}

public protocol Verifier {
    func verify(_ signature: Signature, _ message: some DataProtocol) -> Bool
}
