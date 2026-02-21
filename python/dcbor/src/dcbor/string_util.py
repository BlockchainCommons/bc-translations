from __future__ import annotations

import unicodedata


def is_nfc(s: str) -> bool:
    return unicodedata.is_normalized("NFC", s)


def to_nfc(s: str) -> str:
    return unicodedata.normalize("NFC", s)


def flanked(s: str, left: str, right: str) -> str:
    return left + s + right


def _is_printable(c: str) -> bool:
    code = ord(c)
    return code > 127 or 32 <= code <= 126


def sanitized(s: str) -> str | None:
    has_printable = False
    chars: list[str] = []
    for c in s:
        if _is_printable(c):
            has_printable = True
            chars.append(c)
        else:
            chars.append(".")
    if not has_printable:
        return None
    return "".join(chars)
