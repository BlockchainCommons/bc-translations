"""Format tests.

Translated from rust/bc-envelope/tests/format_tests.rs
"""

from textwrap import dedent

from bc_components import ARID, Digest, SymmetricKey
from bc_components.signing import SchnorrSigningOptions
from bc_envelope import (
    Envelope,
    FormatContextOpt,
    DigestDisplayFormat,
    MermaidFormatOpts,
    MermaidOrientation,
    MermaidTheme,
    TreeFormatOpts,
)
from bc_rand import make_fake_random_number_generator
from dcbor import Date
import known_values

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    PLAINTEXT_HELLO,
    alice_private_keys,
    bob_private_keys,
    bob_public_keys,
    carol_public_keys,
    credential,
    fake_content_key,
    fake_nonce,
    redacted_credential,
)


# ---------------------------------------------------------------------------
# test_plaintext
# ---------------------------------------------------------------------------


def test_plaintext():
    envelope = Envelope(PLAINTEXT_HELLO)

    assert envelope.format() == '"Hello."'
    assert envelope.format_flat() == '"Hello."'

    assert envelope.tree_format() == '8cc96cdb "Hello."'

    assert envelope.tree_format(
        TreeFormatOpts(context=FormatContextOpt.none())
    ) == '8cc96cdb "Hello."'

    assert envelope.tree_format(
        TreeFormatOpts(digest_display=DigestDisplayFormat.FULL)
    ) == '8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59 "Hello."'

    assert envelope.tree_format(
        TreeFormatOpts(digest_display=DigestDisplayFormat.UR)
    ) == 'ur:digest/hdcxlksojzuyktbykovsecbygebsldeninbdfptkwebtwzdpadglwetbgltnwdmwhlhksbbthtpy "Hello."'

    assert envelope.tree_format(
        TreeFormatOpts(hide_nodes=True)
    ) == '"Hello."'

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_signed_plaintext
# ---------------------------------------------------------------------------


def test_signed_plaintext():
    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)
    envelope = Envelope(PLAINTEXT_HELLO).add_signature_opt(
        alice_private_keys(), options, None,
    )

    assert envelope.format() == dedent("""\
        "Hello." [
            'signed': Signature
        ]""")

    assert envelope.format_flat() == '"Hello." [ \'signed\': Signature ]'

    assert envelope.tree_format() == dedent("""\
        949a991e NODE
            8cc96cdb subj "Hello."
            fcb4e2be ASSERTION
                d0e39e78 pred 'signed'
                b8bb043f obj Signature""")

    assert envelope.tree_format(
        TreeFormatOpts(context=FormatContextOpt.none())
    ) == dedent("""\
        949a991e NODE
            8cc96cdb subj "Hello."
            fcb4e2be ASSERTION
                d0e39e78 pred '3'
                b8bb043f obj 40020(h'd0f6b2577edb3f4b0f533e21577bc12a58aaca2604bc71e84bd4e2c81421900bca361a1a8de3b7dbfe1cb5c16e34cb8c9a78fe6f7a387e959bbb15f6f3d898d3')""")

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        subj "Hello."
            ASSERTION
                pred 'signed'
                obj Signature""")

    assert envelope.tree_format(
        TreeFormatOpts(hide_nodes=True, context=FormatContextOpt.none())
    ) == dedent("""\
        subj "Hello."
            ASSERTION
                pred '3'
                obj 40020(h'd0f6b2577edb3f4b0f533e21577bc12a58aaca2604bc71e84bd4e2c81421900bca361a1a8de3b7dbfe1cb5c16e34cb8c9a78fe6f7a387e959bbb15f6f3d898d3')""")

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_encrypt_subject
# ---------------------------------------------------------------------------


def test_encrypt_subject():
    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .encrypt_subject(SymmetricKey.generate())
    )

    assert envelope.format() == dedent("""\
        ENCRYPTED [
            "knows": "Bob"
        ]""")

    assert envelope.format_flat() == 'ENCRYPTED [ "knows": "Bob" ]'

    assert envelope.tree_format() == dedent("""\
        8955db5e NODE
            13941b48 subj ENCRYPTED
            78d666eb ASSERTION
                db7dd21c pred "knows"
                13b74194 obj "Bob"
    """).rstrip()

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        subj ENCRYPTED
            ASSERTION
                pred "knows"
                obj "Bob"\
""")

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_top_level_assertion
# ---------------------------------------------------------------------------


def test_top_level_assertion():
    envelope = Envelope.new_assertion("knows", "Bob")

    assert envelope.format() == '"knows": "Bob"'
    assert envelope.format_flat() == '"knows": "Bob"'

    assert envelope.tree_format() == dedent("""\
        78d666eb ASSERTION
            db7dd21c pred "knows"
            13b74194 obj "Bob"\
""")

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        ASSERTION
            pred "knows"
            obj "Bob"\
""")

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_elided_object
# ---------------------------------------------------------------------------


def test_elided_object():
    envelope = Envelope("Alice").add_assertion("knows", "Bob")
    elided = envelope.elide_removing_target(Envelope("Bob"))

    assert elided.format() == dedent("""\
        "Alice" [
            "knows": ELIDED
        ]""")

    assert elided.format_flat() == '"Alice" [ "knows": ELIDED ]'

    assert elided.tree_format() == dedent("""\
        8955db5e NODE
            13941b48 subj "Alice"
            78d666eb ASSERTION
                db7dd21c pred "knows"
                13b74194 obj ELIDED""")

    assert elided.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        subj "Alice"
            ASSERTION
                pred "knows"
                obj ELIDED""")

    assert elided.elements_count() == elided.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_signed_subject
# ---------------------------------------------------------------------------


def test_signed_subject():
    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)
    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("knows", "Carol")
        .add_signature_opt(alice_private_keys(), options, None)
    )

    assert envelope.format() == dedent("""\
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
            'signed': Signature
        ]""")

    assert envelope.format_flat() == (
        '"Alice" [ "knows": "Bob", "knows": "Carol", \'signed\': Signature ]'
    )

    assert envelope.tree_format() == dedent("""\
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
    """).rstrip()

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
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
    """).rstrip()

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1

    # Elided assertions
    target = set()
    target.add(envelope.digest())
    target.add(envelope.subject().digest())
    elided = envelope.elide_revealing_set(target)

    assert elided.format() == dedent("""\
        "Alice" [
            ELIDED (3)
        ]""")

    assert elided.format_flat() == '"Alice" [ ELIDED (3) ]'

    assert elided.tree_format() == dedent("""\
        d595106e NODE
            13941b48 subj "Alice"
            399c974c ELIDED
            4012caf2 ELIDED
            78d666eb ELIDED""")

    assert elided.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        subj "Alice"
            ELIDED
            ELIDED
            ELIDED""")

    assert elided.elements_count() == elided.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_wrap_then_signed
# ---------------------------------------------------------------------------


def test_wrap_then_signed():
    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)
    envelope = (
        Envelope("Alice")
        .add_assertion("knows", "Bob")
        .add_assertion("knows", "Carol")
        .wrap()
        .add_signature_opt(alice_private_keys(), options, None)
    )

    assert envelope.format() == dedent("""\
        {
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
        } [
            'signed': Signature
        ]""")

    assert envelope.format_flat() == (
        '{ "Alice" [ "knows": "Bob", "knows": "Carol" ] }'
        " [ 'signed': Signature ]"
    )

    assert envelope.tree_format() == dedent("""\
        66c9d594 NODE
            9e3b0673 subj WRAPPED
                b8d857f6 cont NODE
                    13941b48 subj "Alice"
                    4012caf2 ASSERTION
                        db7dd21c pred "knows"
                        afb8122e obj "Carol"
                    78d666eb ASSERTION
                        db7dd21c pred "knows"
                        13b74194 obj "Bob"
            f13623da ASSERTION
                d0e39e78 pred 'signed'
                e30a727c obj Signature""")

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
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
                obj Signature""")

    assert envelope.tree_format(
        TreeFormatOpts(digest_display=DigestDisplayFormat.FULL)
    ) == dedent("""\
        66c9d5944eaedc418d8acf52df7842f50c2aa2d0da2857ad1048412cd070c3e8 NODE
            9e3b06737407b10cac0b9353dd978c4a68537709554dabdd66a8b68b8bd36cf6 subj WRAPPED
                b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471 cont NODE
                    13941b487c1ddebce827b6ec3f46d982938acdc7e3b6a140db36062d9519dd2f subj "Alice"
                    4012caf2d96bf3962514bcfdcf8dd70c351735dec72c856ec5cdcf2ee35d6a91 ASSERTION
                        db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba pred "knows"
                        afb8122e3227657b415f9f1c930d4891fb040b3e23c1f7770f185e2d0396c737 obj "Carol"
                    78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2 ASSERTION
                        db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba pred "knows"
                        13b741949c37b8e09cc3daa3194c58e4fd6b2f14d4b1d0f035a46d6d5a1d3f11 obj "Bob"
            f13623dac926c57e2ac128868dfaa22fb8e434a7e4a552029992d6f6283da533 ASSERTION
                d0e39e788c0d8f0343af4588db21d3d51381db454bdf710a9a1891aaa537693c pred 'signed'
                e30a727cc1f43fbe3c9fd228447c34faaf6b54101bf7bcd766e280f8449ceade obj Signature""")

    assert envelope.tree_format(
        TreeFormatOpts(digest_display=DigestDisplayFormat.UR)
    ) == dedent("""\
        ur:digest/hdcxiysotlmwglpluofplgletkgmurksfwykbndroetitndehgpmbefdfpdwtijosrvsbsdlsndm NODE
            ur:digest/hdcxnnframjkjyatpabnpsbdmuguutmslkgeisguktasgogtpyutiypdrplulutejzynmygrnlly subj WRAPPED
                ur:digest/hdcxrotphgynvtimlsjlrfissgbnvefmgotorhmnwstagabyntpyeeglbyssrdhtaajsaetafrbw cont NODE
                    ur:digest/hdcxbwmwcwfdkecauerfvsdirpwpfhfgtalfmulesnstvlrpoyfzuyenamdpmdcfutdlstyaqzrk subj "Alice"
                    ur:digest/hdcxfzbgsgwztajewfmtdabbrfzctklgtsbnecchecuestdwlpjtsksntkdmvlhlimmetlcpiyms ASSERTION
                        ur:digest/hdcxuykitdcegyinqzlrlgdrcwsbbkihcemtchsntabdpldtbzjepkwsrkdrlernykrddpjtgdfh pred "knows"
                        ur:digest/hdcxperobgdmeydiihkgfphenecemubtfdmezoaabdfmcnseylktbscshydpaxmtstemtarhmngd obj "Carol"
                    ur:digest/hdcxkstbiywmmygsasktnbfwhtrppkclwdcmmugejesokejlbnftrdwspsmdcechbboerhzebtws ASSERTION
                        ur:digest/hdcxuykitdcegyinqzlrlgdrcwsbbkihcemtchsntabdpldtbzjepkwsrkdrlernykrddpjtgdfh pred "knows"
                        ur:digest/hdcxbwrlfpmwnsemrovtnssrtnotcfgshdvezcjedlbbtypatiwtecoxjnjnhtcafhbysptsnsnl obj "Bob"
            ur:digest/hdcxwnencntnsodsskkbdrsedelnlgzsoedlroveeeosveongmaonlmotbyndefsoneorfutayas ASSERTION
                ur:digest/hdcxtivlnnkslkbtmyaxfxpefelouycltetlbwlyuyfegrurjsbknycsmepkoneminfnrpjpssla pred 'signed'
                ur:digest/hdcxvlbkjpkesewkfhrnfnnetddefykeeezspejeghbecwylrftsiyvolayafynswduefytsgaos obj Signature""")

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_encrypt_to_recipients
# ---------------------------------------------------------------------------


def test_encrypt_to_recipients():
    envelope = (
        Envelope(PLAINTEXT_HELLO)
        .encrypt_subject_opt(fake_content_key(), fake_nonce())
    )
    envelope = check_encoding(envelope)
    envelope = envelope.add_recipient_opt(
        bob_public_keys(), fake_content_key(), fake_nonce(),
    )
    envelope = check_encoding(envelope)
    envelope = envelope.add_recipient_opt(
        carol_public_keys(), fake_content_key(), fake_nonce(),
    )
    envelope = check_encoding(envelope)

    assert envelope.format() == dedent("""\
        ENCRYPTED [
            'hasRecipient': SealedMessage
            'hasRecipient': SealedMessage
        ]""")

    assert envelope.format_flat() == (
        "ENCRYPTED [ 'hasRecipient': SealedMessage, 'hasRecipient': SealedMessage ]"
    )

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        subj ENCRYPTED
            ASSERTION
                pred 'hasRecipient'
                obj SealedMessage
            ASSERTION
                pred 'hasRecipient'
                obj SealedMessage""")

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_assertion_positions
# ---------------------------------------------------------------------------


def test_assertion_positions():
    predicate = Envelope("predicate").add_assertion(
        "predicate-predicate", "predicate-object",
    )
    object_ = Envelope("object").add_assertion(
        "object-predicate", "object-object",
    )
    envelope = check_encoding(
        Envelope("subject").add_assertion(predicate, object_)
    )

    assert envelope.format() == dedent("""\
        "subject" [
            "predicate" [
                "predicate-predicate": "predicate-object"
            ]
            : "object" [
                "object-predicate": "object-object"
            ]
        ]""")

    assert envelope.format_flat() == (
        '"subject" [ "predicate" [ "predicate-predicate": "predicate-object" ]'
        ' : "object" [ "object-predicate": "object-object" ] ]'
    )

    assert envelope.tree_format() == dedent("""\
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
                        0bdb89a6 obj "object-object"\
""")

    assert envelope.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
        subj "subject"
            ASSERTION
                subj "predicate"
                    ASSERTION
                        pred "predicate-predicate"
                        obj "predicate-object"
                subj "object"
                    ASSERTION
                        pred "object-predicate"
                        obj "object-object"\
""")

    assert envelope.elements_count() == envelope.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_complex_metadata
# ---------------------------------------------------------------------------


def test_complex_metadata():
    author = check_encoding(
        Envelope(ARID.from_data(bytes.fromhex(
            "9c747ace78a4c826392510dd6285551e7df4e5164729a1b36198e56e017666c8"
        )))
        .add_assertion(known_values.DEREFERENCE_VIA, "LibraryOfCongress")
        .add_assertion(known_values.NAME, "Ayn Rand")
    )

    name_en = (
        Envelope("Atlas Shrugged")
        .add_assertion(known_values.LANGUAGE, "en")
    )

    name_es = (
        Envelope("La rebeli\u00f3n de Atlas")
        .add_assertion(known_values.LANGUAGE, "es")
    )

    work = check_encoding(
        Envelope(ARID.from_data(bytes.fromhex(
            "7fb90a9d96c07f39f75ea6acf392d79f241fac4ec0be2120f7c82489711e3e80"
        )))
        .add_assertion(known_values.IS_A, "novel")
        .add_assertion("isbn", "9780451191144")
        .add_assertion("author", author)
        .add_assertion(known_values.DEREFERENCE_VIA, "LibraryOfCongress")
        .add_assertion(known_values.NAME, name_en)
        .add_assertion(known_values.NAME, name_es)
    )

    book_data = "This is the entire book \u201CAtlas Shrugged\u201D in EPUB format.".encode("utf-8")
    book_metadata = check_encoding(
        Envelope(Digest.from_image(book_data))
        .add_assertion("work", work)
        .add_assertion("format", "EPUB")
        .add_assertion(known_values.DEREFERENCE_VIA, "IPFS")
    )

    assert book_metadata.format() == dedent("""\
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
                'name': "La rebeli\u00f3n de Atlas" [
                    'language': "es"
                ]
            ]
            'dereferenceVia': "IPFS"
        ]""")

    assert book_metadata.format_flat() == (
        'Digest(26d05af5) [ "format": "EPUB", "work": ARID(7fb90a9d) '
        "[ 'isA': \"novel\", "
        '"author": ARID(9c747ace) '
        "[ 'dereferenceVia': \"LibraryOfCongress\", 'name': \"Ayn Rand\" ], "
        '"isbn": "9780451191144", '
        "'dereferenceVia': \"LibraryOfCongress\", "
        "'name': \"Atlas Shrugged\" [ 'language': \"en\" ], "
        "'name': \"La rebeli\u00f3n de Atlas\" [ 'language': \"es\" ] ], "
        "'dereferenceVia': \"IPFS\" ]"
    )

    assert book_metadata.tree_format() == dedent("""\
        c93370e7 NODE
            0c1e45b9 subj Digest(26d05af5)
            83b00bef ASSERTION
                cdb6a696 pred 'dereferenceVia'
                15eac58f obj "IPFS"
            953cdab2 ASSERTION
                a9a86b03 pred "format"
                9536cfe0 obj "EPUB"
            eec25a61 ASSERTION
                2ddb0b05 pred "work"
                26681136 obj NODE
                    0c69be6e subj ARID(7fb90a9d)
                    1786d8b5 ASSERTION
                        4019420b pred "isbn"
                        69ff76b1 obj "9780451191144"
                    5355d973 ASSERTION
                        2be2d79b pred 'isA'
                        6d7c7189 obj "novel"
                    63cd143a ASSERTION
                        14ff9eac pred 'name'
                        29fa40b1 obj NODE
                            5e825721 subj "La rebeli\u00f3n de Atlas"
                            c8db157b ASSERTION
                                60dfb783 pred 'language'
                                b33e79c2 obj "es"
                    7d6d5c1d ASSERTION
                        29c09059 pred "author"
                        1ba13788 obj NODE
                            3c47e105 subj ARID(9c747ace)
                            9c10d60f ASSERTION
                                cdb6a696 pred 'dereferenceVia'
                                34a04547 obj "LibraryOfCongress"
                            bff8435a ASSERTION
                                14ff9eac pred 'name'
                                98985bd5 obj "Ayn Rand"
                    9c10d60f ASSERTION
                        cdb6a696 pred 'dereferenceVia'
                        34a04547 obj "LibraryOfCongress"
                    b722c07c ASSERTION
                        14ff9eac pred 'name'
                        0cfacc06 obj NODE
                            e84c3091 subj "Atlas Shrugged"
                            b80d3b05 ASSERTION
                                60dfb783 pred 'language'
                                6700869c obj "en"
    """).rstrip()

    assert book_metadata.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
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
                        subj "La rebeli\u00f3n de Atlas"
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
    """).rstrip()

    assert book_metadata.elements_count() == book_metadata.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_credential
# ---------------------------------------------------------------------------


def test_credential():
    cred = credential()

    assert cred.format() == dedent("""\
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
        ]""")

    assert cred.format_flat() == (
        '{ ARID(4676635a) [ \'isA\': "Certificate of Completion", '
        '"certificateNumber": "123-456-789", '
        '"continuingEducationUnits": 1, '
        '"expirationDate": 2028-01-01, '
        '"firstName": "James", '
        '"issueDate": 2020-01-01, '
        '"lastName": "Maxwell", '
        '"photo": "This is James Maxwell\'s photo.", '
        '"professionalDevelopmentHours": 15, '
        '"subject": "RF and Microwave Engineering", '
        '"topics": ["Subject 1", "Subject 2"], '
        '\'controller\': "Example Electrical Engineering Board", '
        '\'issuer\': "Example Electrical Engineering Board" ] } '
        '[ \'note\': "Signed by Example Electrical Engineering Board", '
        '\'signed\': Signature ]'
    )

    assert cred.tree_format() == dedent("""\
        0b721f78 NODE
            397a2d4c subj WRAPPED
                8122ffa9 cont NODE
                    10d3de01 subj ARID(4676635a)
                    1f9ff098 ASSERTION
                        9e3bff3a pred "certificateNumber"
                        21c21808 obj "123-456-789"
                    36c254d0 ASSERTION
                        6e5d379f pred "expirationDate"
                        639ae9bf obj 2028-01-01
                    3c114201 ASSERTION
                        5f82a16a pred "lastName"
                        fe4d5230 obj "Maxwell"
                    4a9b2e4d ASSERTION
                        222afe69 pred "issueDate"
                        cb67f31d obj 2020-01-01
                    4d67bba0 ASSERTION
                        2be2d79b pred 'isA'
                        051beee6 obj "Certificate of Completion"
                    5171cbaf ASSERTION
                        3976ef74 pred "photo"
                        231b8527 obj "This is James Maxwell's photo."
                    54b3e1e7 ASSERTION
                        f13aa855 pred "professionalDevelopmentHours"
                        dc0e9c36 obj 15
                    5dc6d4e3 ASSERTION
                        4395643b pred "firstName"
                        d6d0b768 obj "James"
                    68895d8e ASSERTION
                        e6bf4dd3 pred "topics"
                        543fcc09 obj ["Subject 1", "Subject 2"]
                    8ec5e912 ASSERTION
                        2b191589 pred "continuingEducationUnits"
                        4bf5122f obj 1
                    9b3d4785 ASSERTION
                        af10ee92 pred 'controller'
                        f8489ac1 obj "Example Electrical Engineering Board"
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
                f106bad1 obj "Signed by Example Electrical Engineering\u2026"
    """).rstrip()

    assert cred.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
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
                obj "Signed by Example Electrical Engineering\u2026"
    """).rstrip()

    assert cred.elements_count() == cred.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_redacted_credential
# ---------------------------------------------------------------------------


def test_redacted_credential():
    redacted = redacted_credential()
    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)

    warranty = (
        redacted
        .wrap()
        .add_assertion(
            "employeeHiredDate",
            Date.from_string("2022-01-01"),
        )
        .add_assertion("employeeStatus", "active")
        .wrap()
        .add_assertion(known_values.NOTE, "Signed by Employer Corp.")
        .add_signature_opt(bob_private_keys(), options, None)
    )
    warranty = check_encoding(warranty)

    assert warranty.format() == dedent("""\
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
        ]""")

    assert warranty.format_flat() == (
        '{ { { ARID(4676635a) [ \'isA\': "Certificate of Completion", '
        '"expirationDate": 2028-01-01, '
        '"firstName": "James", '
        '"lastName": "Maxwell", '
        '"subject": "RF and Microwave Engineering", '
        '\'issuer\': "Example Electrical Engineering Board", '
        'ELIDED (7) ] } '
        '[ \'note\': "Signed by Example Electrical Engineering Board", '
        '\'signed\': Signature ] } '
        '[ "employeeHiredDate": 2022-01-01, "employeeStatus": "active" ] } '
        '[ \'note\': "Signed by Employer Corp.", \'signed\': Signature ]'
    )

    assert warranty.tree_format() == dedent("""\
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
                                f106bad1 obj "Signed by Example Electrical Engineering\u2026"
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
                5ba600c9 obj Signature""")

    assert warranty.tree_format(TreeFormatOpts(hide_nodes=True)) == dedent("""\
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
                        obj "Signed by Example Electrical Engineering\u2026"
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
                obj Signature""")

    assert warranty.elements_count() == warranty.tree_format().count("\n") + 1


# ---------------------------------------------------------------------------
# test_mermaid (redacted credential with mermaid diagram output)
# ---------------------------------------------------------------------------


def test_mermaid():
    redacted = redacted_credential()
    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)

    warranty = (
        redacted
        .wrap()
        .add_assertion(
            "employeeHiredDate",
            Date.from_string("2022-01-01"),
        )
        .add_assertion("employeeStatus", "active")
        .wrap()
        .add_assertion(known_values.NOTE, "Signed by Employer Corp.")
        .add_signature_opt(bob_private_keys(), options, None)
    )
    warranty = check_encoding(warranty)

    actual = warranty.mermaid_format(
        MermaidFormatOpts(theme=MermaidTheme.DARK)
    )
    expected = dedent("""\
        %%{ init: { 'theme': 'dark', 'flowchart': { 'curve': 'basis' } } }%%
        graph LR
        0(("NODE<br>7ab3e6b1"))
            0 -- subj --> 1[/"WRAPPED<br>3907ee6f"\\]
                1 -- cont --> 2(("NODE<br>719d5955"))
                    2 -- subj --> 3[/"WRAPPED<br>10fb2e18"\\]
                        3 -- cont --> 4(("NODE<br>0b721f78"))
                            4 -- subj --> 5[/"WRAPPED<br>397a2d4c"\\]
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
                                        16 -- obj --> 18["&quot;Certificate of Compl\u2026&quot;<br>051beee6"]
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
                                        27 -- obj --> 29["&quot;RF and Microwave Eng\u2026&quot;<br>202c10ef"]
                                    6 --> 30(["ASSERTION<br>d3e0cc15"])
                                        30 -- pred --> 31[/"'issuer'<br>6dd16ba3"/]
                                        30 -- obj --> 32["&quot;Example Electrical E\u2026&quot;<br>f8489ac1"]
                            4 --> 33(["ASSERTION<br>46a02aaf"])
                                33 -- pred --> 34[/"'signed'<br>d0e39e78"/]
                                33 -- obj --> 35["Signature<br>34c14941"]
                            4 --> 36(["ASSERTION<br>e6d7fca0"])
                                36 -- pred --> 37[/"'note'<br>0fcd6a39"/]
                                36 -- obj --> 38["&quot;Signed by Example El\u2026&quot;<br>f106bad1"]
                    2 --> 39(["ASSERTION<br>4c159c16"])
                        39 -- pred --> 40["&quot;employeeHiredDate&quot;<br>e1ae011e"]
                        39 -- obj --> 41["2022-01-01<br>13b5a817"]
                    2 --> 42(["ASSERTION<br>e071508b"])
                        42 -- pred --> 43["&quot;employeeStatus&quot;<br>d03e7352"]
                        42 -- obj --> 44["&quot;active&quot;<br>1d7a790d"]
            0 --> 45(["ASSERTION<br>874aa7e1"])
                45 -- pred --> 46[/"'note'<br>0fcd6a39"/]
                45 -- obj --> 47["&quot;Signed by Employer C\u2026&quot;<br>f59806d2"]
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
        linkStyle 49 stroke:magenta,stroke-width:2px""")
    assert actual == expected

    actual2 = warranty.mermaid_format(
        MermaidFormatOpts(
            monochrome=True,
            theme=MermaidTheme.FOREST,
            orientation=MermaidOrientation.TOP_TO_BOTTOM,
            hide_nodes=True,
        )
    )
    expected2 = dedent("""\
        %%{ init: { 'theme': 'forest', 'flowchart': { 'curve': 'basis' } } }%%
        graph TB
        0[/"WRAPPED"\\]
            0 -- subj --> 1[/"WRAPPED"\\]
                1 -- subj --> 2[/"WRAPPED"\\]
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
                            12 -- obj --> 14["&quot;Certificate of Compl\u2026&quot;"]
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
                            23 -- obj --> 25["&quot;RF and Microwave Eng\u2026&quot;"]
                        3 --> 26(["ASSERTION"])
                            26 -- pred --> 27[/"'issuer'"/]
                            26 -- obj --> 28["&quot;Example Electrical E\u2026&quot;"]
                    2 --> 29(["ASSERTION"])
                        29 -- pred --> 30[/"'signed'"/]
                        29 -- obj --> 31["Signature"]
                    2 --> 32(["ASSERTION"])
                        32 -- pred --> 33[/"'note'"/]
                        32 -- obj --> 34["&quot;Signed by Example El\u2026&quot;"]
                1 --> 35(["ASSERTION"])
                    35 -- pred --> 36["&quot;employeeHiredDate&quot;"]
                    35 -- obj --> 37["2022-01-01"]
                1 --> 38(["ASSERTION"])
                    38 -- pred --> 39["&quot;employeeStatus&quot;"]
                    38 -- obj --> 40["&quot;active&quot;"]
            0 --> 41(["ASSERTION"])
                41 -- pred --> 42[/"'note'"/]
                41 -- obj --> 43["&quot;Signed by Employer C\u2026&quot;"]
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
        linkStyle 45 stroke-width:2px""")
    assert actual2 == expected2
