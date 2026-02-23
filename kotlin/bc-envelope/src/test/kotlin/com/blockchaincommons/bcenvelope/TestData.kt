@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.knownvalues.*

const val PLAINTEXT_HELLO = "Hello."

fun helloEnvelope(): Envelope = Envelope.from(PLAINTEXT_HELLO)

fun knownValueEnvelope(): Envelope = NOTE.toEnvelope()

fun assertionEnvelope(): Envelope = Envelope.newAssertion("knows", "Bob")

fun singleAssertionEnvelope(): Envelope =
    Envelope.from("Alice").addAssertion("knows", "Bob")

fun doubleAssertionEnvelope(): Envelope =
    singleAssertionEnvelope().addAssertion("knows", "Carol")

fun wrappedEnvelope(): Envelope = helloEnvelope().wrap()

fun doubleWrappedEnvelope(): Envelope = wrappedEnvelope().wrap()

fun aliceSeed(): ByteArray = "82f32c855d3d542256180810797e0073".hexToByteArray()

fun alicePrivateKey(): PrivateKeyBase = PrivateKeyBase.fromData(aliceSeed())

fun alicePublicKey(): PublicKeys = alicePrivateKey().publicKeys()

fun bobSeed(): ByteArray = "187a5973c64d359c836eba466a44db7b".hexToByteArray()

fun bobPrivateKey(): PrivateKeyBase = PrivateKeyBase.fromData(bobSeed())

fun bobPublicKey(): PublicKeys = bobPrivateKey().publicKeys()

fun carolSeed(): ByteArray = "8574afab18e229651c1be8f76ffee523".hexToByteArray()

fun carolPrivateKey(): PrivateKeyBase = PrivateKeyBase.fromData(carolSeed())

fun carolPublicKey(): PublicKeys = carolPrivateKey().publicKeys()

fun fakeContentKey(): SymmetricKey = SymmetricKey.fromData(
    "526afd95b2229c5381baec4a1788507a3c4a566ca5cce64543b46ad12aff0035".hexToByteArray()
)

fun fakeNonce(): Nonce = Nonce.fromData(
    "4d785658f36c22fb5aed3ac0".hexToByteArray()
)

fun credential(): Envelope {
    val rng = fakeRandomNumberGenerator()
    val auxRand = rng.randomData(32)
    val options = SigningOptions.SchnorrAuxRand(auxRand)
    return Envelope.from(
        ARID.fromData(
            "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d".hexToByteArray()
        )
    )
        .addAssertion(IS_A, "Certificate of Completion")
        .addAssertion(ISSUER, "Example Electrical Engineering Board")
        .addAssertion(CONTROLLER, "Example Electrical Engineering Board")
        .addAssertion("firstName", "James")
        .addAssertion("lastName", "Maxwell")
        .addAssertion("issueDate", CborDate.fromString("2020-01-01"))
        .addAssertion("expirationDate", CborDate.fromString("2028-01-01"))
        .addAssertion("photo", "This is James Maxwell's photo.")
        .addAssertion("certificateNumber", "123-456-789")
        .addAssertion("subject", "RF and Microwave Engineering")
        .addAssertion("continuingEducationUnits", 1)
        .addAssertion("professionalDevelopmentHours", 15)
        .addAssertion("topics", Cbor.fromArray(listOf(Cbor.fromString("Subject 1"), Cbor.fromString("Subject 2"))))
        .wrap()
        .addSignatureOpt(alicePrivateKey(), options)
        .addAssertion(NOTE, "Signed by Example Electrical Engineering Board")
        .checkEncoding()
}

fun redactedCredential(): Envelope {
    val cred = credential()
    val target = mutableSetOf(cred.digest())
    for (assertion in cred.assertions()) {
        target.addAll(assertion.deepDigests())
    }
    target.add(cred.subject().digest())
    val content = cred.subject().unwrap()
    target.add(content.digest())
    target.add(content.subject().digest())

    target.addAll(
        content.assertionWithPredicate("firstName").shallowDigests()
    )
    target.addAll(
        content.assertionWithPredicate("lastName").shallowDigests()
    )
    target.addAll(
        content.assertionWithPredicate(IS_A).shallowDigests()
    )
    target.addAll(
        content.assertionWithPredicate(ISSUER).shallowDigests()
    )
    target.addAll(
        content.assertionWithPredicate("subject").shallowDigests()
    )
    target.addAll(
        content.assertionWithPredicate("expirationDate").shallowDigests()
    )
    return cred.elideRevealingSet(target)
}
