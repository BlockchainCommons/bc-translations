"""CBOR round-trip verification utility for Envelope tests."""


def check_encoding(envelope):
    """Verify CBOR encoding round-trips correctly.

    Encodes the envelope to tagged CBOR, decodes it back, and asserts that
    the digest is unchanged.  Returns the original envelope on success.
    """
    from bc_envelope import Envelope

    cbor = envelope.tagged_cbor()
    restored = Envelope.from_tagged_cbor(cbor)
    assert envelope.digest() == restored.digest(), (
        f"Digest mismatch after round-trip:\n"
        f"  original: {envelope.format()}\n"
        f"  decoded:  {restored.format()}"
    )
    return envelope
