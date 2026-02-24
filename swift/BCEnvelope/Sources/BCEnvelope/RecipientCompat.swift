import Foundation
import BCComponents

public extension Envelope {
    func encryptSubjectToRecipients(_ recipients: [any Encrypter]) throws -> Envelope {
        try encryptSubject(to: recipients)
    }

    func encryptSubjectToRecipient(_ recipient: any Encrypter) throws -> Envelope {
        try encryptSubject(to: recipient)
    }

    func decryptSubjectToRecipient(_ recipient: any Decrypter) throws -> Envelope {
        let sealedMessages = try self.recipients
        let contentKeyData = try Self.firstPlaintext(in: sealedMessages, for: recipient)
        let contentKey = try SymmetricKey(taggedCBORData: contentKeyData)
        return try decryptSubject(with: contentKey)
    }

    func encryptToRecipient(_ recipient: any Encrypter) -> Envelope {
        try! wrap().encryptSubjectToRecipient(recipient)
    }

    func decryptToRecipient(_ recipient: any Decrypter) throws -> Envelope {
        try decryptSubjectToRecipient(recipient).unwrap()
    }
}

private extension Envelope {
    static func firstPlaintext(
        in sealedMessages: [SealedMessage],
        for recipient: any Decrypter
    ) throws -> Data {
        for sealedMessage in sealedMessages {
            if let plaintext = try? sealedMessage.decrypt(recipient) {
                return plaintext
            }
        }
        throw EnvelopeError.invalidRecipient
    }
}
