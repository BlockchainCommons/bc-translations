@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.ARID
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.knownvalues.CONTROLLER
import com.blockchaincommons.knownvalues.ISSUER
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class ProofTest {

    @Test
    fun testFriendsList() {
        val aliceFriends = Envelope.from("Alice")
            .addAssertionSalted("knows", "Bob", true)
            .addAssertionSalted("knows", "Carol", true)
            .addAssertionSalted("knows", "Dan", true)

        assertEquals(
            """
            "Alice" [
                {
                    "knows": "Bob"
                } [
                    'salt': Salt
                ]
                {
                    "knows": "Carol"
                } [
                    'salt': Salt
                ]
                {
                    "knows": "Dan"
                } [
                    'salt': Salt
                ]
            ]
            """.trimIndent(),
            aliceFriends.format()
        )

        val aliceFriendsRoot = aliceFriends.elideRevealingSet(emptySet())
        assertEquals("ELIDED", aliceFriendsRoot.format())

        val knowsBobAssertion = Envelope.newAssertion("knows", "Bob")
        val aliceKnowsBobProof = aliceFriends
            .proofContainsTarget(knowsBobAssertion)!!
            .checkEncoding()

        assertEquals(
            """
            ELIDED [
                ELIDED [
                    ELIDED
                ]
                ELIDED (2)
            ]
            """.trimIndent(),
            aliceKnowsBobProof.format()
        )

        assertTrue(
            aliceFriendsRoot.confirmContainsTarget(knowsBobAssertion, aliceKnowsBobProof)
        )
    }

    @Test
    fun testMultiPosition() {
        val aliceFriends = Envelope.from("Alice")
            .addAssertionSalted("knows", "Bob", true)
            .addAssertionSalted("knows", "Carol", true)
            .addAssertionSalted("knows", "Dan", true)

        val knowsProof = aliceFriends
            .proofContainsTarget(Envelope.from("knows"))!!
            .checkEncoding()

        assertEquals(
            """
            ELIDED [
                {
                    ELIDED: ELIDED
                } [
                    ELIDED
                ]
                {
                    ELIDED: ELIDED
                } [
                    ELIDED
                ]
                {
                    ELIDED: ELIDED
                } [
                    ELIDED
                ]
            ]
            """.trimIndent(),
            knowsProof.format()
        )
    }

    @Test
    fun testVerifiableCredential() {
        val aliceSeed = "82f32c855d3d542256180810797e0073".hexToByteArray()
        val alicePrivateKey = com.blockchaincommons.bccomponents.PrivateKeyBase.fromData(aliceSeed)
        val arid = Envelope.from(
            ARID.fromData(
                "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d".hexToByteArray()
            )
        )
        val credential = arid
            .addAssertionSalted("firstName", "John", true)
            .addAssertionSalted("lastName", "Smith", true)
            .addAssertionSalted("address", "123 Main St.", true)
            .addAssertionSalted(
                "birthDate",
                CborDate.fromString("1970-01-01"),
                true,
            )
            .addAssertionSalted("photo", "This is John Smith's photo.", true)
            .addAssertionSalted("dlNumber", "123-456-789", true)
            .addAssertionSalted("nonCommercialVehicleEndorsement", true, true)
            .addAssertionSalted("motorocycleEndorsement", true, true)
            .addAssertion(ISSUER, "State of Example")
            .addAssertion(CONTROLLER, "State of Example")
            .wrap()
            .addSignature(alicePrivateKey)
            .addAssertion(com.blockchaincommons.knownvalues.NOTE, "Signed by the State of Example")

        val credentialRoot = credential.elideRevealingSet(emptySet())

        // In this case the holder of a credential wants to prove a single assertion
        // from it, the address.
        val addressAssertion = Envelope.newAssertion("address", "123 Main St.")
        val addressProof = credential
            .proofContainsTarget(addressAssertion)!!
            .checkEncoding()

        // The proof includes digests from all the elided assertions.
        assertEquals(
            """
            {
                ELIDED [
                    ELIDED [
                        ELIDED
                    ]
                    ELIDED (9)
                ]
            } [
                ELIDED (2)
            ]
            """.trimIndent(),
            addressProof.format()
        )

        // The proof confirms the address, as intended.
        assertTrue(
            credentialRoot.confirmContainsTarget(addressAssertion, addressProof)
        )

        // Assertions without salt can also be confirmed.
        val issuerAssertion = Envelope.newAssertion(ISSUER, "State of Example")
        assertTrue(
            credentialRoot.confirmContainsTarget(issuerAssertion, addressProof)
        )

        // The proof cannot be used to confirm salted assertions.
        val firstNameAssertion = Envelope.newAssertion("firstName", "John")
        assertFalse(
            credentialRoot.confirmContainsTarget(firstNameAssertion, addressProof)
        )
    }
}
