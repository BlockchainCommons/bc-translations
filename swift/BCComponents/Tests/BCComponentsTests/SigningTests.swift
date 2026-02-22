import BCComponents
import BCRand
import XCTest

@MainActor
final class SigningTests: XCTestCase {
    private let message = Data("Wolf McNally".utf8)
    private let keyData = hexData("322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36")

    private var schnorrPrivateKey: SigningPrivateKey {
        .newSchnorr(try! ECPrivateKey(keyData))
    }

    private var ecdsaPrivateKey: SigningPrivateKey {
        .newEcdsa(try! ECPrivateKey(keyData))
    }

    private var ed25519PrivateKey: SigningPrivateKey {
        .newEd25519(try! Ed25519PrivateKey(keyData))
    }

    private func assertSSHKeypair(_ scheme: SignatureScheme) throws {
        let (privateKey, publicKey) = scheme.keypair()
        let signature = try privateKey.signWithOptions(
            message,
            options: .ssh(namespace: "ssh", hashAlg: .sha512)
        )
        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertEqual(signature.scheme(), scheme)
    }

    func testLibECDSAAndSchnorrVector() {
        var rng = makeFakeRandomNumberGenerator()
        let privateKey = ecdsaNewPrivateKeyUsing(&rng)
        let message = Data(
            "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".utf8
        )

        let ecdsaPublicKey = ecdsaPublicKeyFromPrivateKey(privateKey)
        let ecdsaSignature = ecdsaSign(privateKey, message)
        XCTAssertEqual(
            hexString(ecdsaSignature),
            "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"
        )
        XCTAssertTrue(ecdsaVerify(ecdsaPublicKey, ecdsaSignature, message))

        let schnorrPublicKey = schnorrPublicKeyFromPrivateKey(privateKey)
        let schnorrSignature = schnorrSignUsing(privateKey, message, &rng)
        XCTAssertEqual(
            hexString(schnorrSignature),
            "df3e33900f0b94e23b6f8685f620ed92705ebfcf885ccb321620acb9927bce1e2218dcfba7cb9c3bba11611446f38774a564f265917899194e82945c8b60a996"
        )
        XCTAssertTrue(schnorrVerify(schnorrPublicKey, schnorrSignature, message))
    }

    func testSchnorrSigning() throws {
        let publicKey = try schnorrPrivateKey.publicKey()
        let signature = try schnorrPrivateKey.sign(message)

        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertFalse(publicKey.verify(signature, Data("Wolf Mcnally".utf8)))

        let anotherSignature = try schnorrPrivateKey.sign(message)
        XCTAssertNotEqual(signature, anotherSignature)
        XCTAssertTrue(publicKey.verify(anotherSignature, message))
    }

    func testSchnorrCBORVector() throws {
        registerTags()

        let rng = AnyBCRandomNumberGenerator(makeFakeRandomNumberGenerator())
        let signature = try schnorrPrivateKey.signWithOptions(
            message,
            options: .schnorr(rng: rng)
        )
        XCTAssertEqual(
            hexString(signature.toSchnorr()!),
            "9d113392074dd52dfb7f309afb3698a1993cd14d32bc27c00070407092c9ec8c096643b5b1b535bb5277c44f256441ac660cd600739aa910b150d4f94757cf95"
        )

        let cbor = signature.taggedCBOR
        XCTAssertEqual(
            hexString(cbor.cborData),
            "d99c5458409d113392074dd52dfb7f309afb3698a1993cd14d32bc27c00070407092c9ec8c096643b5b1b535bb5277c44f256441ac660cd600739aa910b150d4f94757cf95"
        )

        let decoded = try Signature(cbor: cbor)
        XCTAssertEqual(decoded, signature)
    }

    func testECDSASigning() throws {
        let publicKey = try ecdsaPrivateKey.publicKey()
        let signature = try ecdsaPrivateKey.sign(message)

        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertFalse(publicKey.verify(signature, Data("Wolf Mcnally".utf8)))

        let anotherSignature = try ecdsaPrivateKey.sign(message)
        XCTAssertEqual(signature, anotherSignature)
        XCTAssertTrue(publicKey.verify(anotherSignature, message))
    }

    func testECDSACBORVector() throws {
        registerTags()

        let signature = try ecdsaPrivateKey.sign(message)
        XCTAssertEqual(
            hexString(signature.toEcdsa()!),
            "1458d0f3d97e25109b38fd965782b43213134d02b01388a14e74ebf21e5dea4866f25a23866de9ecf0f9b72404d8192ed71fba4dc355cd89b47213e855cf6d23"
        )

        let cbor = signature.taggedCBOR
        XCTAssertEqual(
            hexString(cbor.cborData),
            "d99c54820158401458d0f3d97e25109b38fd965782b43213134d02b01388a14e74ebf21e5dea4866f25a23866de9ecf0f9b72404d8192ed71fba4dc355cd89b47213e855cf6d23"
        )

        let decoded = try Signature(cbor: cbor)
        XCTAssertEqual(decoded, signature)
    }

    func testEd25519Signing() throws {
        let publicKey = try ed25519PrivateKey.publicKey()
        let signature = try ed25519PrivateKey.sign(message)

        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertFalse(publicKey.verify(signature, Data("Wolf Mcnally".utf8)))

        let anotherSignature = try ed25519PrivateKey.sign(message)
        XCTAssertEqual(signature, anotherSignature)
        XCTAssertTrue(publicKey.verify(anotherSignature, message))
    }

    func testSchnorrKeypair() throws {
        let (privateKey, publicKey) = SignatureScheme.default.keypair()
        let signature = try privateKey.sign(message)
        XCTAssertTrue(publicKey.verify(signature, message))
    }

    func testECDSAKeypair() throws {
        let (privateKey, publicKey) = SignatureScheme.ecdsa.keypair()
        let signature = try privateKey.sign(message)
        XCTAssertTrue(publicKey.verify(signature, message))
    }

    func testEd25519Keypair() throws {
        let (privateKey, publicKey) = SignatureScheme.ed25519.keypair()
        let signature = try privateKey.sign(message)
        XCTAssertTrue(publicKey.verify(signature, message))
    }

    func testSigningKeyURVectors() throws {
        registerTags()

        var rng = makeFakeRandomNumberGenerator()

        let schnorrPrivate = SigningPrivateKey.newSchnorr(ECPrivateKey.newUsing(rng: &rng))
        let schnorrPrivateUR = schnorrPrivate.urString()
        XCTAssertEqual(
            schnorrPrivateUR,
            "ur:signing-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct"
        )
        XCTAssertEqual(try SigningPrivateKey.fromURString(schnorrPrivateUR), schnorrPrivate)

        let ecdsaPrivate = SigningPrivateKey.newEcdsa(ECPrivateKey.newUsing(rng: &rng))
        let ecdsaPublic = try ecdsaPrivate.publicKey()
        let ecdsaPublicUR = ecdsaPublic.urString()
        XCTAssertEqual(
            ecdsaPublicUR,
            "ur:signing-public-key/lfadhdclaxbzutckgevlpkmdfnuoemlnvsgllokicfdekesswnfdtibkylrskomwgubaahyntaktbksbdt"
        )
        XCTAssertEqual(try SigningPublicKey.fromURString(ecdsaPublicUR), ecdsaPublic)

        let schnorrPublic = try schnorrPrivate.publicKey()
        let schnorrPublicUR = schnorrPublic.urString()
        XCTAssertEqual(
            schnorrPublicUR,
            "ur:signing-public-key/hdcxjsrhdnidbgosndmobzwntdglzonnidmwoyrnuomdrpsptkcskerhfljssgaoidjewyjymhcp"
        )
        XCTAssertEqual(try SigningPublicKey.fromURString(schnorrPublicUR), schnorrPublic)

        let derivedPrivate = SigningPrivateKey.newSchnorr(
            ECPrivateKey.deriveFromKeyMaterial(Data("password".utf8))
        )
        XCTAssertEqual(
            derivedPrivate.urString(),
            "ur:signing-private-key/hdcxahsfgobtpkkpahmnhsfmhnjnmkmkzeuraonneshkbysseyjkoeayrlvtvsmndicwkkvattfs"
        )
    }

    func testSSHEd25519Keypair() throws {
        try assertSSHKeypair(.sshEd25519)
    }

    func testSSHEcdsaP256Keypair() throws {
        try assertSSHKeypair(.sshEcdsaP256)
    }

    func testSSHEcdsaP384Keypair() throws {
        try assertSSHKeypair(.sshEcdsaP384)
    }

    func testSSHSignRequiresOptions() throws {
        let (privateKey, _) = SignatureScheme.sshEd25519.keypair()
        XCTAssertThrowsError(try privateKey.sign(message))
    }

    func testSSHKeypairUsingSupported() throws {
        var rng = makeFakeRandomNumberGenerator()
        let (privateKey, publicKey) = try SignatureScheme.sshEd25519.keypairUsing(&rng)
        let signature = try privateKey.signWithOptions(
            message,
            options: .ssh(namespace: "ssh", hashAlg: .sha512)
        )
        XCTAssertTrue(publicKey.verify(signature, message))
    }

    func testSSHCBORRoundtrip() throws {
        let (privateKey, publicKey) = SignatureScheme.sshEd25519.keypairOpt("Key comment.")
        let signature = try privateKey.signWithOptions(
            message,
            options: .ssh(namespace: "test", hashAlg: .sha256)
        )

        let privateKeyDecoded = try SigningPrivateKey(cbor: privateKey.cbor)
        let publicKeyDecoded = try SigningPublicKey(cbor: publicKey.cbor)
        let signatureDecoded = try Signature(cbor: signature.cbor)

        XCTAssertEqual(privateKeyDecoded, privateKey)
        XCTAssertEqual(publicKeyDecoded, publicKey)
        XCTAssertEqual(signatureDecoded, signature)
    }

    func testSSHEd25519DeterministicVector() throws {
        let seed = hexData("59f2293a5bce7d4de59e71b4207ac5d2")
        let privateKey = try PrivateKeyBase.fromData(seed)
            .sshSigningPrivateKey(.ed25519, comment: "Key comment.")
        let publicKey = try privateKey.publicKey()

        let expectedPrivate = """
        -----BEGIN OPENSSH PRIVATE KEY-----
        b3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW
        QyNTUxOQAAACBUe4FDGyGIgHf75yVdE4hYl9guj02FdsIadgLC04zObQAAAJA+TyZiPk8m
        YgAAAAtzc2gtZWQyNTUxOQAAACBUe4FDGyGIgHf75yVdE4hYl9guj02FdsIadgLC04zObQ
        AAAECsX3CKi3hm5VrrU26ffa2FB2YrFogg45ucOVbIz4FQo1R7gUMbIYiAd/vnJV0TiFiX
        2C6PTYV2whp2AsLTjM5tAAAADEtleSBjb21tZW50LgE=
        -----END OPENSSH PRIVATE KEY-----
        """
        XCTAssertEqual(privateKey.toSSH()?.openssh, expectedPrivate)
        XCTAssertEqual(
            publicKey.toSSH()?.openssh,
            "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIFR7gUMbIYiAd/vnJV0TiFiX2C6PTYV2whp2AsLTjM5t Key comment."
        )

        let message = Data(
            "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".utf8
        )
        let signature = try privateKey.signWithOptions(
            message,
            options: .ssh(namespace: "test", hashAlg: .sha256)
        )
        XCTAssertTrue(publicKey.verify(signature, message))
    }

    func testSSHDSAKeypair() throws {
        try assertSSHKeypair(.sshDsa)
    }

    func testSSHDSASigningVector() throws {
        let seed = hexData("59f2293a5bce7d4de59e71b4207ac5d2")
        let privateKey = try PrivateKeyBase.fromData(seed)
            .sshSigningPrivateKey(.dsa, comment: "Key comment.")
        let publicKey = try privateKey.publicKey()

        let expectedPrivate = """
        -----BEGIN OPENSSH PRIVATE KEY-----
        b3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAABsgAAAAdzc2gtZH
        NzAAAAgQCWG4f7r8FAMT/IL11w9OfM/ZduIQ8vEq1Ub+uMdyJS8wS/jXL5OB2/dPnXCNSt
        L4vjSqpDzMs+Dtd5wJy6baSQ3zGEbYv71mkIRJB/AtSVmd8FZe5AEjLFvHxYMSlO0jpi1Y
        /1nLM7vLQu4QByDCLhYYjPxgrZKXB3cLxtjvly5wAAABUA4fIZLivnDVcg9PXzwcb5m07H
        9k0AAACBAJK5Vm6t1Sg7n+C63wrNgDA6LTNyGzxqRVM2unI16jisCOzuC98Dgs+IbAkLhT
        qWSY+nI+U9HBHc7sr+KKdWCzR76NLK5eSilXvtt8g+LfHIXvCjD4Q2puowtjDoXSEQAJYd
        c1gtef21KZ2eoKoyAwzQIehCbvLpwYbxnhap5usVAAAAgGCrsbfReaDZo1Cw4/dFlJWBDP
        sMGeG04/2hCThNmU+zLiKCwsEg0X6onOTMTonCXve3fVb5lNjIU92iTmt5QkmOj2hjsbgo
        q/0sa0lALHp7UcK/W4IdU4Abtc4m0SUflgJcds1nsy2rKUNEtAfRa/WwtDResWOa4T7L+3
        FEUdavAAAB6F0RJ3hdESd4AAAAB3NzaC1kc3MAAACBAJYbh/uvwUAxP8gvXXD058z9l24h
        Dy8SrVRv64x3IlLzBL+Ncvk4Hb90+dcI1K0vi+NKqkPMyz4O13nAnLptpJDfMYRti/vWaQ
        hEkH8C1JWZ3wVl7kASMsW8fFgxKU7SOmLVj/Wcszu8tC7hAHIMIuFhiM/GCtkpcHdwvG2O
        +XLnAAAAFQDh8hkuK+cNVyD09fPBxvmbTsf2TQAAAIEAkrlWbq3VKDuf4LrfCs2AMDotM3
        IbPGpFUza6cjXqOKwI7O4L3wOCz4hsCQuFOpZJj6cj5T0cEdzuyv4op1YLNHvo0srl5KKV
        e+23yD4t8che8KMPhDam6jC2MOhdIRAAlh1zWC15/bUpnZ6gqjIDDNAh6EJu8unBhvGeFq
        nm6xUAAACAYKuxt9F5oNmjULDj90WUlYEM+wwZ4bTj/aEJOE2ZT7MuIoLCwSDRfqic5MxO
        icJe97d9VvmU2MhT3aJOa3lCSY6PaGOxuCir/SxrSUAsentRwr9bgh1TgBu1zibRJR+WAl
        x2zWezLaspQ0S0B9Fr9bC0NF6xY5rhPsv7cURR1q8AAAAVANWljfuxQcmJ/T7wSmAUXmXo
        6ZI0AAAADEtleSBjb21tZW50LgECAwQF
        -----END OPENSSH PRIVATE KEY-----
        """
        XCTAssertEqual(privateKey.toSSH()?.openssh, expectedPrivate)
        XCTAssertEqual(
            publicKey.toSSH()?.openssh,
            "ssh-dss AAAAB3NzaC1kc3MAAACBAJYbh/uvwUAxP8gvXXD058z9l24hDy8SrVRv64x3IlLzBL+Ncvk4Hb90+dcI1K0vi+NKqkPMyz4O13nAnLptpJDfMYRti/vWaQhEkH8C1JWZ3wVl7kASMsW8fFgxKU7SOmLVj/Wcszu8tC7hAHIMIuFhiM/GCtkpcHdwvG2O+XLnAAAAFQDh8hkuK+cNVyD09fPBxvmbTsf2TQAAAIEAkrlWbq3VKDuf4LrfCs2AMDotM3IbPGpFUza6cjXqOKwI7O4L3wOCz4hsCQuFOpZJj6cj5T0cEdzuyv4op1YLNHvo0srl5KKVe+23yD4t8che8KMPhDam6jC2MOhdIRAAlh1zWC15/bUpnZ6gqjIDDNAh6EJu8unBhvGeFqnm6xUAAACAYKuxt9F5oNmjULDj90WUlYEM+wwZ4bTj/aEJOE2ZT7MuIoLCwSDRfqic5MxOicJe97d9VvmU2MhT3aJOa3lCSY6PaGOxuCir/SxrSUAsentRwr9bgh1TgBu1zibRJR+WAlx2zWezLaspQ0S0B9Fr9bC0NF6xY5rhPsv7cURR1q8= Key comment."
        )

        let message = Data(
            "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".utf8
        )
        let signature = try privateKey.signWithOptions(
            message,
            options: .ssh(namespace: "test", hashAlg: .sha256)
        )
        XCTAssertTrue(publicKey.verify(signature, message))
    }
}
