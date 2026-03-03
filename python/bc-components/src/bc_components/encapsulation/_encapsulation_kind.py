"""Internal enum for encapsulation variant dispatch."""

from enum import Enum, auto


class EncapsulationKind(Enum):
    """Discriminator for encapsulation union types."""

    X25519 = auto()
    MLKEM = auto()
