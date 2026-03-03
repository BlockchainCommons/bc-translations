"""Identifier types for various purposes.

- ARID: Apparently Random Identifier (32-byte)
- XID: eXtensible IDentifier (32-byte, tied to a public key)
- UUID: Universally Unique Identifier (16-byte)
- URI: Uniform Resource Identifier (string)
"""

from ._arid import ARID, ARID_SIZE
from ._uri import URI
from ._uuid import UUID, UUID_SIZE
from ._xid import XID, XID_SIZE, XIDProvider

__all__ = [
    "ARID",
    "ARID_SIZE",
    "URI",
    "UUID",
    "UUID_SIZE",
    "XID",
    "XID_SIZE",
    "XIDProvider",
]
