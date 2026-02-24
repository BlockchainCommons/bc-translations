import Foundation
import BCComponents

public extension Envelope {
    /// Seals an envelope by signing it with the sender's key and then
    /// encrypting it to the recipient.
    func seal(_ sender: any Signer, _ recipient: any Encrypter) -> Envelope {
        sign(sender).encryptToRecipient(recipient)
    }

    /// Seals an envelope with optional signing options.
    func sealOpt(
        _ sender: any Signer,
        _ recipient: any Encrypter,
        _ options: SigningOptions?
    ) -> Envelope {
        signOpt(sender, options).encryptToRecipient(recipient)
    }

    /// Unseals an envelope by decrypting it and then verifying the signature.
    func unseal(
        _ sender: any Verifier,
        _ recipient: any Decrypter
    ) throws -> Envelope {
        try decryptToRecipient(recipient).verify(sender)
    }
}
