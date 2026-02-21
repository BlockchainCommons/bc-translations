# ``DCBOR``

Deterministic CBOR ("DCBOR") codec.

## Overview

`DCBOR` is a CBOR codec that focuses on writing and parsing “deterministic” CBOR per §4.2 of RFC-8949. It does not support parts of the spec forbidden by deterministic CBOR (such as indefinite length arrays and maps). It is strict in both what it writes and reads: in particular it will throw decoding errors if variable-length integers are not encoded in their minimal form, or CBOR map keys are not in lexicographic order, or there is extra data past the end of the decoded CBOR item.
