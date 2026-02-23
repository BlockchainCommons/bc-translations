@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.ARID
import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.SigningOptions
import com.blockchaincommons.bccomponents.SymmetricKey
import com.blockchaincommons.bccomponents.toDigest
import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.knownvalues.CONTROLLER
import com.blockchaincommons.knownvalues.DEREFERENCE_VIA
import com.blockchaincommons.knownvalues.IS_A
import com.blockchaincommons.knownvalues.ISSUER
import com.blockchaincommons.knownvalues.LANGUAGE
import com.blockchaincommons.knownvalues.NAME
import com.blockchaincommons.knownvalues.NOTE
import kotlin.test.Test
import kotlin.test.assertEquals

class FormatTest {

    @Test
    fun testPlaintext() {
        registerTags()

        val envelope = Envelope.from("Hello.")

        assertEquals(
            "\"Hello.\"",
            envelope.format()
        )

        assertEquals(
            "\"Hello.\"",
            envelope.formatFlat()
        )

        val treeExpected = """
            8cc96cdb "Hello."
        """.trimIndent()
        assertEquals(treeExpected, envelope.treeFormat())

        val treeNoContextExpected = """
            8cc96cdb "Hello."
        """.trimIndent()
        assertEquals(
            treeNoContextExpected,
            envelope.treeFormatOpt(TreeFormatOpts(context = FormatContextOpt.None))
        )

        val treeFullExpected = """
            8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59 "Hello."
        """.trimIndent()
        assertEquals(
            treeFullExpected,
            envelope.treeFormatOpt(
                TreeFormatOpts(digestDisplay = DigestDisplayFormat.Full)
            )
        )

        val treeUrExpected = """
            ur:digest/hdcxlksojzuyktbykovsecbygebsldeninbdfptkwebtwzdpadglwetbgltnwdmwhlhksbbthtpy "Hello."
        """.trimIndent()
        assertEquals(
            treeUrExpected,
            envelope.treeFormatOpt(
                TreeFormatOpts(digestDisplay = DigestDisplayFormat.UR)
            )
        )

        val treeHideNodesExpected = """
            "Hello."
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testEncryptSubject() {
        registerTags()

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .encryptSubject(SymmetricKey.create())

        assertEquals(
            """
            ENCRYPTED [
                "knows": "Bob"
            ]
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """ENCRYPTED [ "knows": "Bob" ]""",
            envelope.formatFlat()
        )

        val treeExpected = """
            8955db5e NODE
                13941b48 subj ENCRYPTED
                78d666eb ASSERTION
                    db7dd21c pred "knows"
                    13b74194 obj "Bob"
        """.trimIndent()
        assertEquals(treeExpected, envelope.treeFormat())

        val treeHideNodesExpected = """
            subj ENCRYPTED
                ASSERTION
                    pred "knows"
                    obj "Bob"
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testTopLevelAssertion() {
        registerTags()

        val envelope = Envelope.newAssertion("knows", "Bob")

        assertEquals(
            """
            "knows": "Bob"
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """"knows": "Bob"""",
            envelope.formatFlat()
        )

        val treeExpected = """
            78d666eb ASSERTION
                db7dd21c pred "knows"
                13b74194 obj "Bob"
        """.trimIndent()
        assertEquals(treeExpected, envelope.treeFormat())

        val treeHideNodesExpected = """
            ASSERTION
                pred "knows"
                obj "Bob"
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testElidedObject() {
        registerTags()

        val envelope = Envelope.from("Alice").addAssertion("knows", "Bob")
        val elided = envelope.elideRemovingTarget("Bob".toEnvelope())

        assertEquals(
            """
            "Alice" [
                "knows": ELIDED
            ]
            """.trimIndent(),
            elided.format()
        )

        assertEquals(
            """"Alice" [ "knows": ELIDED ]""",
            elided.formatFlat()
        )

        val treeExpected = """
            8955db5e NODE
                13941b48 subj "Alice"
                78d666eb ASSERTION
                    db7dd21c pred "knows"
                    13b74194 obj ELIDED
        """.trimIndent()
        assertEquals(treeExpected, elided.treeFormat())

        val treeHideNodesExpected = """
            subj "Alice"
                ASSERTION
                    pred "knows"
                    obj ELIDED
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            elided.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            elided.elementsCount(),
            elided.treeFormat().split('\n').size
        )
    }

    @Test
    fun testSignedPlaintext() {
        registerTags()

        // Use non-deterministic signing -- only test format output (not tree digests)
        val envelope = helloEnvelope()
            .addSignature(alicePrivateKey())

        assertEquals(
            """
            "Hello." [
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """"Hello." [ 'signed': Signature ]""",
            envelope.formatFlat()
        )

        // Tree format with hidden nodes is deterministic
        val treeHideNodesExpected = """
            subj "Hello."
                ASSERTION
                    pred 'signed'
                    obj Signature
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testSignedSubject() {
        registerTags()

        val rng = fakeRandomNumberGenerator()
        val auxRand = rng.randomData(32)
        val options = SigningOptions.SchnorrAuxRand(auxRand)
        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("knows", "Carol")
            .addSignatureOpt(alicePrivateKey(), options)

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """"Alice" [ "knows": "Bob", "knows": "Carol", 'signed': Signature ]""",
            envelope.formatFlat()
        )

        // Full tree format (with digests)
        val treeExpected = """
            d595106e NODE
                13941b48 subj "Alice"
                399c974c ASSERTION
                    d0e39e78 pred 'signed'
                    ff10427c obj Signature
                4012caf2 ASSERTION
                    db7dd21c pred "knows"
                    afb8122e obj "Carol"
                78d666eb ASSERTION
                    db7dd21c pred "knows"
                    13b74194 obj "Bob"
        """.trimIndent()
        assertEquals(treeExpected, envelope.treeFormat())

        // Tree format with hidden nodes
        val treeHideNodesExpected = """
            subj "Alice"
                ASSERTION
                    pred 'signed'
                    obj Signature
                ASSERTION
                    pred "knows"
                    obj "Carol"
                ASSERTION
                    pred "knows"
                    obj "Bob"
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )

        // Elided assertions
        val target = mutableSetOf(envelope.digest(), envelope.subject().digest())
        val elided = envelope.elideRevealingSet(target)

        assertEquals(
            """
            "Alice" [
                ELIDED (3)
            ]
            """.trimIndent(),
            elided.format()
        )

        assertEquals(
            """"Alice" [ ELIDED (3) ]""",
            elided.formatFlat()
        )

        val elidedTreeHideNodesExpected = """
            subj "Alice"
                ELIDED
                ELIDED
                ELIDED
        """.trimIndent()
        assertEquals(
            elidedTreeHideNodesExpected,
            elided.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            elided.elementsCount(),
            elided.treeFormat().split('\n').size
        )
    }

    @Test
    fun testWrapThenSigned() {
        registerTags()

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("knows", "Carol")
            .wrap()
            .addSignature(alicePrivateKey())

        assertEquals(
            """
            {
                "Alice" [
                    "knows": "Bob"
                    "knows": "Carol"
                ]
            } [
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """{ "Alice" [ "knows": "Bob", "knows": "Carol" ] } [ 'signed': Signature ]""",
            envelope.formatFlat()
        )

        // Tree format with hidden nodes
        val treeHideNodesExpected = """
            subj WRAPPED
                subj "Alice"
                    ASSERTION
                        pred "knows"
                        obj "Carol"
                    ASSERTION
                        pred "knows"
                        obj "Bob"
                ASSERTION
                    pred 'signed'
                    obj Signature
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testEncryptToRecipients() {
        registerTags()

        val contentKey = SymmetricKey.create()
        val envelope = helloEnvelope()
            .encryptSubject(contentKey)
            .checkEncoding()
            .addRecipient(bobPublicKey(), contentKey)
            .checkEncoding()
            .addRecipient(carolPublicKey(), contentKey)
            .checkEncoding()

        assertEquals(
            """
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
            ]
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """ENCRYPTED [ 'hasRecipient': SealedMessage, 'hasRecipient': SealedMessage ]""",
            envelope.formatFlat()
        )

        val treeHideNodesExpected = """
            subj ENCRYPTED
                ASSERTION
                    pred 'hasRecipient'
                    obj SealedMessage
                ASSERTION
                    pred 'hasRecipient'
                    obj SealedMessage
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testAssertionPositions() {
        registerTags()

        val predicate = Envelope.from("predicate")
            .addAssertion("predicate-predicate", "predicate-object")
        val objectEnv = Envelope.from("object")
            .addAssertion("object-predicate", "object-object")
        val envelope = Envelope.from("subject")
            .addAssertion(predicate, objectEnv)
            .checkEncoding()

        assertEquals(
            """
            "subject" [
                "predicate" [
                    "predicate-predicate": "predicate-object"
                ]
                : "object" [
                    "object-predicate": "object-object"
                ]
            ]
            """.trimIndent(),
            envelope.format()
        )

        assertEquals(
            """"subject" [ "predicate" [ "predicate-predicate": "predicate-object" ] : "object" [ "object-predicate": "object-object" ] ]""",
            envelope.formatFlat()
        )

        val treeExpected = """
            e06d7003 NODE
                8e4e62eb subj "subject"
                91a436e0 ASSERTION
                    cece8b2c pred NODE
                        d21efb76 subj "predicate"
                        66a0c92b ASSERTION
                            ab829e9f pred "predicate-predicate"
                            f1098628 obj "predicate-object"
                    03a99a27 obj NODE
                        fda63155 subj "object"
                        d1878aea ASSERTION
                            88bb262f pred "object-predicate"
                            0bdb89a6 obj "object-object"
        """.trimIndent()
        assertEquals(treeExpected, envelope.treeFormat())

        val treeHideNodesExpected = """
            subj "subject"
                ASSERTION
                    subj "predicate"
                        ASSERTION
                            pred "predicate-predicate"
                            obj "predicate-object"
                    subj "object"
                        ASSERTION
                            pred "object-predicate"
                            obj "object-object"
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            envelope.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            envelope.elementsCount(),
            envelope.treeFormat().split('\n').size
        )
    }

    @Test
    fun testComplexMetadata() {
        registerTags()

        val author = Envelope.from(
            ARID.fromData(
                "9c747ace78a4c826392510dd6285551e7df4e5164729a1b36198e56e017666c8"
                    .hexToByteArray()
            )
        )
            .addAssertion(DEREFERENCE_VIA, "LibraryOfCongress")
            .addAssertion(NAME, "Ayn Rand")
            .checkEncoding()

        val nameEn = Envelope.from("Atlas Shrugged")
            .addAssertion(LANGUAGE, "en")

        val nameEs = Envelope.from("La rebelión de Atlas")
            .addAssertion(LANGUAGE, "es")

        val work = Envelope.from(
            ARID.fromData(
                "7fb90a9d96c07f39f75ea6acf392d79f241fac4ec0be2120f7c82489711e3e80"
                    .hexToByteArray()
            )
        )
            .addAssertion(IS_A, "novel")
            .addAssertion("isbn", "9780451191144")
            .addAssertion("author", author)
            .addAssertion(DEREFERENCE_VIA, "LibraryOfCongress")
            .addAssertion(NAME, nameEn)
            .addAssertion(NAME, nameEs)
            .checkEncoding()

        val bookData = "This is the entire book \u201CAtlas Shrugged\u201D in EPUB format."
        val bookMetadata = Envelope.from(
            Digest.fromImage(bookData.toByteArray())
        )
            .addAssertion("work", work)
            .addAssertion("format", "EPUB")
            .addAssertion(DEREFERENCE_VIA, "IPFS")
            .checkEncoding()

        assertEquals(
            """
            Digest(26d05af5) [
                "format": "EPUB"
                "work": ARID(7fb90a9d) [
                    'isA': "novel"
                    "author": ARID(9c747ace) [
                        'dereferenceVia': "LibraryOfCongress"
                        'name': "Ayn Rand"
                    ]
                    "isbn": "9780451191144"
                    'dereferenceVia': "LibraryOfCongress"
                    'name': "Atlas Shrugged" [
                        'language': "en"
                    ]
                    'name': "La rebelión de Atlas" [
                        'language': "es"
                    ]
                ]
                'dereferenceVia': "IPFS"
            ]
            """.trimIndent(),
            bookMetadata.format()
        )

        val treeHideNodesExpected = """
            subj Digest(26d05af5)
                ASSERTION
                    pred 'dereferenceVia'
                    obj "IPFS"
                ASSERTION
                    pred "format"
                    obj "EPUB"
                ASSERTION
                    pred "work"
                    subj ARID(7fb90a9d)
                        ASSERTION
                            pred "isbn"
                            obj "9780451191144"
                        ASSERTION
                            pred 'isA'
                            obj "novel"
                        ASSERTION
                            pred 'name'
                            subj "La rebelión de Atlas"
                                ASSERTION
                                    pred 'language'
                                    obj "es"
                        ASSERTION
                            pred "author"
                            subj ARID(9c747ace)
                                ASSERTION
                                    pred 'dereferenceVia'
                                    obj "LibraryOfCongress"
                                ASSERTION
                                    pred 'name'
                                    obj "Ayn Rand"
                        ASSERTION
                            pred 'dereferenceVia'
                            obj "LibraryOfCongress"
                        ASSERTION
                            pred 'name'
                            subj "Atlas Shrugged"
                                ASSERTION
                                    pred 'language'
                                    obj "en"
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            bookMetadata.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            bookMetadata.elementsCount(),
            bookMetadata.treeFormat().split('\n').size
        )
    }

    @Test
    fun testCredential() {
        registerTags()

        val credential = credential()

        assertEquals(
            """
            {
                ARID(4676635a) [
                    'isA': "Certificate of Completion"
                    "certificateNumber": "123-456-789"
                    "continuingEducationUnits": 1
                    "expirationDate": 2028-01-01
                    "firstName": "James"
                    "issueDate": 2020-01-01
                    "lastName": "Maxwell"
                    "photo": "This is James Maxwell's photo."
                    "professionalDevelopmentHours": 15
                    "subject": "RF and Microwave Engineering"
                    "topics": ["Subject 1", "Subject 2"]
                    'controller': "Example Electrical Engineering Board"
                    'issuer': "Example Electrical Engineering Board"
                ]
            } [
                'note': "Signed by Example Electrical Engineering Board"
                'signed': Signature
            ]
            """.trimIndent(),
            credential.format()
        )

        // Tree format with hidden nodes
        val treeHideNodesExpected = """
            subj WRAPPED
                subj ARID(4676635a)
                    ASSERTION
                        pred "certificateNumber"
                        obj "123-456-789"
                    ASSERTION
                        pred "expirationDate"
                        obj 2028-01-01
                    ASSERTION
                        pred "lastName"
                        obj "Maxwell"
                    ASSERTION
                        pred "issueDate"
                        obj 2020-01-01
                    ASSERTION
                        pred 'isA'
                        obj "Certificate of Completion"
                    ASSERTION
                        pred "photo"
                        obj "This is James Maxwell's photo."
                    ASSERTION
                        pred "professionalDevelopmentHours"
                        obj 15
                    ASSERTION
                        pred "firstName"
                        obj "James"
                    ASSERTION
                        pred "topics"
                        obj ["Subject 1", "Subject 2"]
                    ASSERTION
                        pred "continuingEducationUnits"
                        obj 1
                    ASSERTION
                        pred 'controller'
                        obj "Example Electrical Engineering Board"
                    ASSERTION
                        pred "subject"
                        obj "RF and Microwave Engineering"
                    ASSERTION
                        pred 'issuer'
                        obj "Example Electrical Engineering Board"
                ASSERTION
                    pred 'signed'
                    obj Signature
                ASSERTION
                    pred 'note'
                    obj "Signed by Example Electrical Engineering…"
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            credential.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            credential.elementsCount(),
            credential.treeFormat().split('\n').size
        )
    }

    @Test
    fun testRedactedCredential() {
        registerTags()

        val rc = redactedCredential()
        val rng = fakeRandomNumberGenerator()
        val auxRand = rng.randomData(32)
        val options = SigningOptions.SchnorrAuxRand(auxRand)
        val warranty = rc
            .wrap()
            .addAssertion(
                "employeeHiredDate",
                CborDate.fromString("2022-01-01"),
            )
            .addAssertion("employeeStatus", "active")
            .wrap()
            .addAssertion(NOTE, "Signed by Employer Corp.")
            .addSignatureOpt(bobPrivateKey(), options)
            .checkEncoding()

        assertEquals(
            """
            {
                {
                    {
                        ARID(4676635a) [
                            'isA': "Certificate of Completion"
                            "expirationDate": 2028-01-01
                            "firstName": "James"
                            "lastName": "Maxwell"
                            "subject": "RF and Microwave Engineering"
                            'issuer': "Example Electrical Engineering Board"
                            ELIDED (7)
                        ]
                    } [
                        'note': "Signed by Example Electrical Engineering Board"
                        'signed': Signature
                    ]
                } [
                    "employeeHiredDate": 2022-01-01
                    "employeeStatus": "active"
                ]
            } [
                'note': "Signed by Employer Corp."
                'signed': Signature
            ]
            """.trimIndent(),
            warranty.format()
        )

        assertEquals(
            """{ { { ARID(4676635a) [ 'isA': "Certificate of Completion", "expirationDate": 2028-01-01, "firstName": "James", "lastName": "Maxwell", "subject": "RF and Microwave Engineering", 'issuer': "Example Electrical Engineering Board", ELIDED (7) ] } [ 'note': "Signed by Example Electrical Engineering Board", 'signed': Signature ] } [ "employeeHiredDate": 2022-01-01, "employeeStatus": "active" ] } [ 'note': "Signed by Employer Corp.", 'signed': Signature ]""",
            warranty.formatFlat()
        )

        val treeExpected = """
            7ab3e6b1 NODE
                3907ee6f subj WRAPPED
                    719d5955 cont NODE
                        10fb2e18 subj WRAPPED
                            0b721f78 cont NODE
                                397a2d4c subj WRAPPED
                                    8122ffa9 cont NODE
                                        10d3de01 subj ARID(4676635a)
                                        1f9ff098 ELIDED
                                        36c254d0 ASSERTION
                                            6e5d379f pred "expirationDate"
                                            639ae9bf obj 2028-01-01
                                        3c114201 ASSERTION
                                            5f82a16a pred "lastName"
                                            fe4d5230 obj "Maxwell"
                                        4a9b2e4d ELIDED
                                        4d67bba0 ASSERTION
                                            2be2d79b pred 'isA'
                                            051beee6 obj "Certificate of Completion"
                                        5171cbaf ELIDED
                                        54b3e1e7 ELIDED
                                        5dc6d4e3 ASSERTION
                                            4395643b pred "firstName"
                                            d6d0b768 obj "James"
                                        68895d8e ELIDED
                                        8ec5e912 ELIDED
                                        9b3d4785 ELIDED
                                        caf5ced3 ASSERTION
                                            8e4e62eb pred "subject"
                                            202c10ef obj "RF and Microwave Engineering"
                                        d3e0cc15 ASSERTION
                                            6dd16ba3 pred 'issuer'
                                            f8489ac1 obj "Example Electrical Engineering Board"
                                46a02aaf ASSERTION
                                    d0e39e78 pred 'signed'
                                    34c14941 obj Signature
                                e6d7fca0 ASSERTION
                                    0fcd6a39 pred 'note'
                                    f106bad1 obj "Signed by Example Electrical Engineering…"
                        4c159c16 ASSERTION
                            e1ae011e pred "employeeHiredDate"
                            13b5a817 obj 2022-01-01
                        e071508b ASSERTION
                            d03e7352 pred "employeeStatus"
                            1d7a790d obj "active"
                874aa7e1 ASSERTION
                    0fcd6a39 pred 'note'
                    f59806d2 obj "Signed by Employer Corp."
                d21d2033 ASSERTION
                    d0e39e78 pred 'signed'
                    5ba600c9 obj Signature
        """.trimIndent()
        assertEquals(treeExpected, warranty.treeFormat())

        val treeHideNodesExpected = """
            subj WRAPPED
                subj WRAPPED
                    subj WRAPPED
                        subj ARID(4676635a)
                            ELIDED
                            ASSERTION
                                pred "expirationDate"
                                obj 2028-01-01
                            ASSERTION
                                pred "lastName"
                                obj "Maxwell"
                            ELIDED
                            ASSERTION
                                pred 'isA'
                                obj "Certificate of Completion"
                            ELIDED
                            ELIDED
                            ASSERTION
                                pred "firstName"
                                obj "James"
                            ELIDED
                            ELIDED
                            ELIDED
                            ASSERTION
                                pred "subject"
                                obj "RF and Microwave Engineering"
                            ASSERTION
                                pred 'issuer'
                                obj "Example Electrical Engineering Board"
                        ASSERTION
                            pred 'signed'
                            obj Signature
                        ASSERTION
                            pred 'note'
                            obj "Signed by Example Electrical Engineering…"
                    ASSERTION
                        pred "employeeHiredDate"
                        obj 2022-01-01
                    ASSERTION
                        pred "employeeStatus"
                        obj "active"
                ASSERTION
                    pred 'note'
                    obj "Signed by Employer Corp."
                ASSERTION
                    pred 'signed'
                    obj Signature
        """.trimIndent()
        assertEquals(
            treeHideNodesExpected,
            warranty.treeFormatOpt(TreeFormatOpts(hideNodes = true))
        )

        assertEquals(
            warranty.elementsCount(),
            warranty.treeFormat().split('\n').size
        )

        val mermaidDark = warranty.mermaidFormatOpt(
            MermaidFormatOpts(theme = MermaidTheme.Dark)
        )
        val mermaidDarkExpected = """
            %%{ init: { 'theme': 'dark', 'flowchart': { 'curve': 'basis' } } }%%
            graph LR
            0(("NODE<br>7ab3e6b1"))
                0 -- subj --> 1[/"WRAPPED<br>3907ee6f"\]
                    1 -- cont --> 2(("NODE<br>719d5955"))
                        2 -- subj --> 3[/"WRAPPED<br>10fb2e18"\]
                            3 -- cont --> 4(("NODE<br>0b721f78"))
                                4 -- subj --> 5[/"WRAPPED<br>397a2d4c"\]
                                    5 -- cont --> 6(("NODE<br>8122ffa9"))
                                        6 -- subj --> 7["ARID(4676635a)<br>10d3de01"]
                                        6 --> 8{{"ELIDED<br>1f9ff098"}}
                                        6 --> 9(["ASSERTION<br>36c254d0"])
                                            9 -- pred --> 10["&quot;expirationDate&quot;<br>6e5d379f"]
                                            9 -- obj --> 11["2028-01-01<br>639ae9bf"]
                                        6 --> 12(["ASSERTION<br>3c114201"])
                                            12 -- pred --> 13["&quot;lastName&quot;<br>5f82a16a"]
                                            12 -- obj --> 14["&quot;Maxwell&quot;<br>fe4d5230"]
                                        6 --> 15{{"ELIDED<br>4a9b2e4d"}}
                                        6 --> 16(["ASSERTION<br>4d67bba0"])
                                            16 -- pred --> 17[/"'isA'<br>2be2d79b"/]
                                            16 -- obj --> 18["&quot;Certificate of Compl…&quot;<br>051beee6"]
                                        6 --> 19{{"ELIDED<br>5171cbaf"}}
                                        6 --> 20{{"ELIDED<br>54b3e1e7"}}
                                        6 --> 21(["ASSERTION<br>5dc6d4e3"])
                                            21 -- pred --> 22["&quot;firstName&quot;<br>4395643b"]
                                            21 -- obj --> 23["&quot;James&quot;<br>d6d0b768"]
                                        6 --> 24{{"ELIDED<br>68895d8e"}}
                                        6 --> 25{{"ELIDED<br>8ec5e912"}}
                                        6 --> 26{{"ELIDED<br>9b3d4785"}}
                                        6 --> 27(["ASSERTION<br>caf5ced3"])
                                            27 -- pred --> 28["&quot;subject&quot;<br>8e4e62eb"]
                                            27 -- obj --> 29["&quot;RF and Microwave Eng…&quot;<br>202c10ef"]
                                        6 --> 30(["ASSERTION<br>d3e0cc15"])
                                            30 -- pred --> 31[/"'issuer'<br>6dd16ba3"/]
                                            30 -- obj --> 32["&quot;Example Electrical E…&quot;<br>f8489ac1"]
                                4 --> 33(["ASSERTION<br>46a02aaf"])
                                    33 -- pred --> 34[/"'signed'<br>d0e39e78"/]
                                    33 -- obj --> 35["Signature<br>34c14941"]
                                4 --> 36(["ASSERTION<br>e6d7fca0"])
                                    36 -- pred --> 37[/"'note'<br>0fcd6a39"/]
                                    36 -- obj --> 38["&quot;Signed by Example El…&quot;<br>f106bad1"]
                        2 --> 39(["ASSERTION<br>4c159c16"])
                            39 -- pred --> 40["&quot;employeeHiredDate&quot;<br>e1ae011e"]
                            39 -- obj --> 41["2022-01-01<br>13b5a817"]
                        2 --> 42(["ASSERTION<br>e071508b"])
                            42 -- pred --> 43["&quot;employeeStatus&quot;<br>d03e7352"]
                            42 -- obj --> 44["&quot;active&quot;<br>1d7a790d"]
                0 --> 45(["ASSERTION<br>874aa7e1"])
                    45 -- pred --> 46[/"'note'<br>0fcd6a39"/]
                    45 -- obj --> 47["&quot;Signed by Employer C…&quot;<br>f59806d2"]
                0 --> 48(["ASSERTION<br>d21d2033"])
                    48 -- pred --> 49[/"'signed'<br>d0e39e78"/]
                    48 -- obj --> 50["Signature<br>5ba600c9"]
            style 0 stroke:red,stroke-width:4px
            style 1 stroke:blue,stroke-width:4px
            style 2 stroke:red,stroke-width:4px
            style 3 stroke:blue,stroke-width:4px
            style 4 stroke:red,stroke-width:4px
            style 5 stroke:blue,stroke-width:4px
            style 6 stroke:red,stroke-width:4px
            style 7 stroke:teal,stroke-width:4px
            style 8 stroke:gray,stroke-width:4px
            style 9 stroke:green,stroke-width:4px
            style 10 stroke:teal,stroke-width:4px
            style 11 stroke:teal,stroke-width:4px
            style 12 stroke:green,stroke-width:4px
            style 13 stroke:teal,stroke-width:4px
            style 14 stroke:teal,stroke-width:4px
            style 15 stroke:gray,stroke-width:4px
            style 16 stroke:green,stroke-width:4px
            style 17 stroke:goldenrod,stroke-width:4px
            style 18 stroke:teal,stroke-width:4px
            style 19 stroke:gray,stroke-width:4px
            style 20 stroke:gray,stroke-width:4px
            style 21 stroke:green,stroke-width:4px
            style 22 stroke:teal,stroke-width:4px
            style 23 stroke:teal,stroke-width:4px
            style 24 stroke:gray,stroke-width:4px
            style 25 stroke:gray,stroke-width:4px
            style 26 stroke:gray,stroke-width:4px
            style 27 stroke:green,stroke-width:4px
            style 28 stroke:teal,stroke-width:4px
            style 29 stroke:teal,stroke-width:4px
            style 30 stroke:green,stroke-width:4px
            style 31 stroke:goldenrod,stroke-width:4px
            style 32 stroke:teal,stroke-width:4px
            style 33 stroke:green,stroke-width:4px
            style 34 stroke:goldenrod,stroke-width:4px
            style 35 stroke:teal,stroke-width:4px
            style 36 stroke:green,stroke-width:4px
            style 37 stroke:goldenrod,stroke-width:4px
            style 38 stroke:teal,stroke-width:4px
            style 39 stroke:green,stroke-width:4px
            style 40 stroke:teal,stroke-width:4px
            style 41 stroke:teal,stroke-width:4px
            style 42 stroke:green,stroke-width:4px
            style 43 stroke:teal,stroke-width:4px
            style 44 stroke:teal,stroke-width:4px
            style 45 stroke:green,stroke-width:4px
            style 46 stroke:goldenrod,stroke-width:4px
            style 47 stroke:teal,stroke-width:4px
            style 48 stroke:green,stroke-width:4px
            style 49 stroke:goldenrod,stroke-width:4px
            style 50 stroke:teal,stroke-width:4px
            linkStyle 0 stroke:red,stroke-width:2px
            linkStyle 1 stroke:blue,stroke-width:2px
            linkStyle 2 stroke:red,stroke-width:2px
            linkStyle 3 stroke:blue,stroke-width:2px
            linkStyle 4 stroke:red,stroke-width:2px
            linkStyle 5 stroke:blue,stroke-width:2px
            linkStyle 6 stroke:red,stroke-width:2px
            linkStyle 7 stroke-width:2px
            linkStyle 8 stroke-width:2px
            linkStyle 9 stroke:cyan,stroke-width:2px
            linkStyle 10 stroke:magenta,stroke-width:2px
            linkStyle 11 stroke-width:2px
            linkStyle 12 stroke:cyan,stroke-width:2px
            linkStyle 13 stroke:magenta,stroke-width:2px
            linkStyle 14 stroke-width:2px
            linkStyle 15 stroke-width:2px
            linkStyle 16 stroke:cyan,stroke-width:2px
            linkStyle 17 stroke:magenta,stroke-width:2px
            linkStyle 18 stroke-width:2px
            linkStyle 19 stroke-width:2px
            linkStyle 20 stroke-width:2px
            linkStyle 21 stroke:cyan,stroke-width:2px
            linkStyle 22 stroke:magenta,stroke-width:2px
            linkStyle 23 stroke-width:2px
            linkStyle 24 stroke-width:2px
            linkStyle 25 stroke-width:2px
            linkStyle 26 stroke-width:2px
            linkStyle 27 stroke:cyan,stroke-width:2px
            linkStyle 28 stroke:magenta,stroke-width:2px
            linkStyle 29 stroke-width:2px
            linkStyle 30 stroke:cyan,stroke-width:2px
            linkStyle 31 stroke:magenta,stroke-width:2px
            linkStyle 32 stroke-width:2px
            linkStyle 33 stroke:cyan,stroke-width:2px
            linkStyle 34 stroke:magenta,stroke-width:2px
            linkStyle 35 stroke-width:2px
            linkStyle 36 stroke:cyan,stroke-width:2px
            linkStyle 37 stroke:magenta,stroke-width:2px
            linkStyle 38 stroke-width:2px
            linkStyle 39 stroke:cyan,stroke-width:2px
            linkStyle 40 stroke:magenta,stroke-width:2px
            linkStyle 41 stroke-width:2px
            linkStyle 42 stroke:cyan,stroke-width:2px
            linkStyle 43 stroke:magenta,stroke-width:2px
            linkStyle 44 stroke-width:2px
            linkStyle 45 stroke:cyan,stroke-width:2px
            linkStyle 46 stroke:magenta,stroke-width:2px
            linkStyle 47 stroke-width:2px
            linkStyle 48 stroke:cyan,stroke-width:2px
            linkStyle 49 stroke:magenta,stroke-width:2px
        """.trimIndent()
        assertEquals(mermaidDarkExpected, mermaidDark)

        val mermaidForest = warranty.mermaidFormatOpt(
            MermaidFormatOpts(
                monochrome = true,
                theme = MermaidTheme.Forest,
                orientation = MermaidOrientation.TopToBottom,
                hideNodes = true,
            )
        )
        val mermaidForestExpected = """
            %%{ init: { 'theme': 'forest', 'flowchart': { 'curve': 'basis' } } }%%
            graph TB
            0[/"WRAPPED"\]
                0 -- subj --> 1[/"WRAPPED"\]
                    1 -- subj --> 2[/"WRAPPED"\]
                        2 -- subj --> 3["ARID(4676635a)"]
                            3 --> 4{{"ELIDED"}}
                            3 --> 5(["ASSERTION"])
                                5 -- pred --> 6["&quot;expirationDate&quot;"]
                                5 -- obj --> 7["2028-01-01"]
                            3 --> 8(["ASSERTION"])
                                8 -- pred --> 9["&quot;lastName&quot;"]
                                8 -- obj --> 10["&quot;Maxwell&quot;"]
                            3 --> 11{{"ELIDED"}}
                            3 --> 12(["ASSERTION"])
                                12 -- pred --> 13[/"'isA'"/]
                                12 -- obj --> 14["&quot;Certificate of Compl…&quot;"]
                            3 --> 15{{"ELIDED"}}
                            3 --> 16{{"ELIDED"}}
                            3 --> 17(["ASSERTION"])
                                17 -- pred --> 18["&quot;firstName&quot;"]
                                17 -- obj --> 19["&quot;James&quot;"]
                            3 --> 20{{"ELIDED"}}
                            3 --> 21{{"ELIDED"}}
                            3 --> 22{{"ELIDED"}}
                            3 --> 23(["ASSERTION"])
                                23 -- pred --> 24["&quot;subject&quot;"]
                                23 -- obj --> 25["&quot;RF and Microwave Eng…&quot;"]
                            3 --> 26(["ASSERTION"])
                                26 -- pred --> 27[/"'issuer'"/]
                                26 -- obj --> 28["&quot;Example Electrical E…&quot;"]
                        2 --> 29(["ASSERTION"])
                            29 -- pred --> 30[/"'signed'"/]
                            29 -- obj --> 31["Signature"]
                        2 --> 32(["ASSERTION"])
                            32 -- pred --> 33[/"'note'"/]
                            32 -- obj --> 34["&quot;Signed by Example El…&quot;"]
                    1 --> 35(["ASSERTION"])
                        35 -- pred --> 36["&quot;employeeHiredDate&quot;"]
                        35 -- obj --> 37["2022-01-01"]
                    1 --> 38(["ASSERTION"])
                        38 -- pred --> 39["&quot;employeeStatus&quot;"]
                        38 -- obj --> 40["&quot;active&quot;"]
                0 --> 41(["ASSERTION"])
                    41 -- pred --> 42[/"'note'"/]
                    41 -- obj --> 43["&quot;Signed by Employer C…&quot;"]
                0 --> 44(["ASSERTION"])
                    44 -- pred --> 45[/"'signed'"/]
                    44 -- obj --> 46["Signature"]
            style 0 stroke-width:4px
            style 1 stroke-width:4px
            style 2 stroke-width:4px
            style 3 stroke-width:4px
            style 4 stroke-width:4px
            style 5 stroke-width:4px
            style 6 stroke-width:4px
            style 7 stroke-width:4px
            style 8 stroke-width:4px
            style 9 stroke-width:4px
            style 10 stroke-width:4px
            style 11 stroke-width:4px
            style 12 stroke-width:4px
            style 13 stroke-width:4px
            style 14 stroke-width:4px
            style 15 stroke-width:4px
            style 16 stroke-width:4px
            style 17 stroke-width:4px
            style 18 stroke-width:4px
            style 19 stroke-width:4px
            style 20 stroke-width:4px
            style 21 stroke-width:4px
            style 22 stroke-width:4px
            style 23 stroke-width:4px
            style 24 stroke-width:4px
            style 25 stroke-width:4px
            style 26 stroke-width:4px
            style 27 stroke-width:4px
            style 28 stroke-width:4px
            style 29 stroke-width:4px
            style 30 stroke-width:4px
            style 31 stroke-width:4px
            style 32 stroke-width:4px
            style 33 stroke-width:4px
            style 34 stroke-width:4px
            style 35 stroke-width:4px
            style 36 stroke-width:4px
            style 37 stroke-width:4px
            style 38 stroke-width:4px
            style 39 stroke-width:4px
            style 40 stroke-width:4px
            style 41 stroke-width:4px
            style 42 stroke-width:4px
            style 43 stroke-width:4px
            style 44 stroke-width:4px
            style 45 stroke-width:4px
            style 46 stroke-width:4px
            linkStyle 0 stroke-width:2px
            linkStyle 1 stroke-width:2px
            linkStyle 2 stroke-width:2px
            linkStyle 3 stroke-width:2px
            linkStyle 4 stroke-width:2px
            linkStyle 5 stroke-width:2px
            linkStyle 6 stroke-width:2px
            linkStyle 7 stroke-width:2px
            linkStyle 8 stroke-width:2px
            linkStyle 9 stroke-width:2px
            linkStyle 10 stroke-width:2px
            linkStyle 11 stroke-width:2px
            linkStyle 12 stroke-width:2px
            linkStyle 13 stroke-width:2px
            linkStyle 14 stroke-width:2px
            linkStyle 15 stroke-width:2px
            linkStyle 16 stroke-width:2px
            linkStyle 17 stroke-width:2px
            linkStyle 18 stroke-width:2px
            linkStyle 19 stroke-width:2px
            linkStyle 20 stroke-width:2px
            linkStyle 21 stroke-width:2px
            linkStyle 22 stroke-width:2px
            linkStyle 23 stroke-width:2px
            linkStyle 24 stroke-width:2px
            linkStyle 25 stroke-width:2px
            linkStyle 26 stroke-width:2px
            linkStyle 27 stroke-width:2px
            linkStyle 28 stroke-width:2px
            linkStyle 29 stroke-width:2px
            linkStyle 30 stroke-width:2px
            linkStyle 31 stroke-width:2px
            linkStyle 32 stroke-width:2px
            linkStyle 33 stroke-width:2px
            linkStyle 34 stroke-width:2px
            linkStyle 35 stroke-width:2px
            linkStyle 36 stroke-width:2px
            linkStyle 37 stroke-width:2px
            linkStyle 38 stroke-width:2px
            linkStyle 39 stroke-width:2px
            linkStyle 40 stroke-width:2px
            linkStyle 41 stroke-width:2px
            linkStyle 42 stroke-width:2px
            linkStyle 43 stroke-width:2px
            linkStyle 44 stroke-width:2px
            linkStyle 45 stroke-width:2px
        """.trimIndent()
        assertEquals(mermaidForestExpected, mermaidForest)
    }
}
