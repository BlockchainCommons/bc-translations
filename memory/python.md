# Python Translation Memory

- When the host Python is externally managed (PEP 668), run translation tests in an isolated venv (for example under `/tmp`) instead of using `pip --break-system-packages`.
- For Rust-style in-place slice fills (`buf[4..]`), remember that Python `bytearray` slicing copies; use a temporary buffer or a `memoryview` to avoid silently writing to the wrong object.
