# Python Translation Memory

- When the host Python is externally managed (PEP 668), run translation tests in an isolated venv (for example under `/tmp`) instead of using `pip --break-system-packages`.
- For Rust-style in-place slice fills (`buf[4..]`), remember that Python `bytearray` slicing copies; use a temporary buffer or a `memoryview` to avoid silently writing to the wrong object.
- Before using `dcbor.with_tags_mut(...)` in Python translations, first force global tag-store initialization via `dcbor.with_tags(lambda _: None)` to avoid first-call deadlocks in the current lock implementation.
- When running tests for Python crates that import `bc_shamir`, use a virtualenv with `bc_crypto` runtime dependencies installed (for example `python/bc-crypto/.venv`), or imports can fail on missing `argon2-cffi`.
