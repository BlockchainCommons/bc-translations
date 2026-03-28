"""Shared test data for bc-envelope tests.

Translated from rust/bc-envelope/tests/common/test_data.rs
"""

from bc_envelope import Envelope
from bc_components import ARID, Nonce, PrivateKeyBase, SymmetricKey
from bc_components.signing import SchnorrSigningOptions
from bc_rand import make_fake_random_number_generator
from dcbor import CBOR, Date
import known_values

PLAINTEXT_HELLO = "Hello."


def hello_envelope() -> Envelope:
    return Envelope(PLAINTEXT_HELLO)


def known_value_envelope() -> Envelope:
    import known_values
    return Envelope(known_values.NOTE)


def assertion_envelope() -> Envelope:
    return Envelope.new_assertion("knows", "Bob")


def single_assertion_envelope() -> Envelope:
    return Envelope("Alice").add_assertion("knows", "Bob")


def double_assertion_envelope() -> Envelope:
    return single_assertion_envelope().add_assertion("knows", "Carol")


def wrapped_envelope() -> Envelope:
    return hello_envelope().wrap()


def double_wrapped_envelope() -> Envelope:
    return wrapped_envelope().wrap()


# --- Seed data ---

ALICE_SEED = bytes.fromhex("82f32c855d3d542256180810797e0073")
BOB_SEED = bytes.fromhex("187a5973c64d359c836eba466a44db7b")
CAROL_SEED = bytes.fromhex("8574afab18e229651c1be8f76ffee523")


def alice_private_keys() -> PrivateKeyBase:
    return PrivateKeyBase(ALICE_SEED)


def alice_public_keys():
    return alice_private_keys().public_keys()


def bob_private_keys() -> PrivateKeyBase:
    return PrivateKeyBase(BOB_SEED)


def bob_public_keys():
    return bob_private_keys().public_keys()


def carol_private_keys() -> PrivateKeyBase:
    return PrivateKeyBase(CAROL_SEED)


def carol_public_keys():
    return carol_private_keys().public_keys()


def fake_content_key() -> SymmetricKey:
    return SymmetricKey(bytes.fromhex(
        "526afd95b2229c5381baec4a1788507a3c4a566ca5cce64543b46ad12aff0035"
    ))


def fake_nonce() -> Nonce:
    return Nonce(bytes.fromhex("4d785658f36c22fb5aed3ac0"))


def credential() -> Envelope:
    """Build the standard credential envelope used in format and elision tests."""
    from tests.common.check_encoding import check_encoding

    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)

    topics_cbor = CBOR.from_array([
        CBOR.from_value("Subject 1"),
        CBOR.from_value("Subject 2"),
    ])

    return (
        Envelope(ARID.from_data(bytes.fromhex(
            "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d"
        )))
        .add_assertion(known_values.IS_A, "Certificate of Completion")
        .add_assertion(known_values.ISSUER, "Example Electrical Engineering Board")
        .add_assertion(
            known_values.CONTROLLER,
            "Example Electrical Engineering Board",
        )
        .add_assertion("firstName", "James")
        .add_assertion("lastName", "Maxwell")
        .add_assertion("issueDate", Date.from_string("2020-01-01"))
        .add_assertion("expirationDate", Date.from_string("2028-01-01"))
        .add_assertion("photo", "This is James Maxwell's photo.")
        .add_assertion("certificateNumber", "123-456-789")
        .add_assertion("subject", "RF and Microwave Engineering")
        .add_assertion("continuingEducationUnits", 1)
        .add_assertion("professionalDevelopmentHours", 15)
        .add_assertion("topics", topics_cbor)
        .wrap()
        .add_signature_opt(alice_private_keys(), options, None)
        .add_assertion(
            known_values.NOTE,
            "Signed by Example Electrical Engineering Board",
        )
    )


def redacted_credential() -> Envelope:
    """Build the redacted credential used in format tests."""
    cred = credential()
    target = set()
    target.add(cred.digest())
    for assertion in cred.assertions():
        target.update(assertion.deep_digests())
    target.add(cred.subject().digest())
    content = cred.subject().try_unwrap()
    target.add(content.digest())
    target.add(content.subject().digest())

    target.update(
        content.assertion_with_predicate("firstName").shallow_digests()
    )
    target.update(
        content.assertion_with_predicate("lastName").shallow_digests()
    )
    target.update(
        content.assertion_with_predicate(known_values.IS_A).shallow_digests()
    )
    target.update(
        content.assertion_with_predicate(known_values.ISSUER).shallow_digests()
    )
    target.update(
        content.assertion_with_predicate("subject").shallow_digests()
    )
    target.update(
        content.assertion_with_predicate("expirationDate").shallow_digests()
    )
    return cred.elide_revealing_set(target)
