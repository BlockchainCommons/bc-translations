"""Sequential provenance mark generator."""

from __future__ import annotations

import json

from bc_envelope import Envelope
from dcbor import Date

from ._crypto_utils import sha256
from ._error import Error
from ._mark import ProvenanceMark
from ._resolution import ProvenanceMarkResolution, resolution_from_cbor, resolution_to_cbor
from ._rng_state import RngState
from ._seed import ProvenanceSeed
from ._util import b64decode, b64encode
from ._xoshiro256starstar import Xoshiro256StarStar


class ProvenanceMarkGenerator:
    """Stateful generator for sequential provenance marks."""

    __slots__ = ("_res", "_seed", "_chain_id", "_next_seq", "_rng_state")

    def __init__(
        self,
        res: ProvenanceMarkResolution,
        seed: ProvenanceSeed,
        chain_id: bytes,
        next_seq: int,
        rng_state: RngState,
    ) -> None:
        expected = res.link_length()
        if len(chain_id) != expected:
            raise Error(
                "InvalidChainIdLength",
                f"invalid chain ID length: expected {expected}, got {len(chain_id)}",
            )
        self._res = res
        self._seed = seed
        self._chain_id = bytes(chain_id)
        self._next_seq = next_seq
        self._rng_state = rng_state

    @staticmethod
    def new(
        res: ProvenanceMarkResolution,
        seed: ProvenanceSeed,
        chain_id: bytes | bytearray,
        next_seq: int,
        rng_state: RngState,
    ) -> ProvenanceMarkGenerator:
        return ProvenanceMarkGenerator(res, seed, bytes(chain_id), next_seq, rng_state)

    @staticmethod
    def new_with_seed(
        res: ProvenanceMarkResolution,
        seed: ProvenanceSeed,
    ) -> ProvenanceMarkGenerator:
        digest1 = sha256(seed.to_bytes())
        chain_id = digest1[: res.link_length()]
        digest2 = sha256(digest1)
        return ProvenanceMarkGenerator.new(
            res,
            seed,
            chain_id,
            0,
            RngState.from_bytes(digest2),
        )

    @staticmethod
    def new_with_passphrase(
        res: ProvenanceMarkResolution,
        passphrase: str,
    ) -> ProvenanceMarkGenerator:
        return ProvenanceMarkGenerator.new_with_seed(
            res,
            ProvenanceSeed.new_with_passphrase(passphrase),
        )

    @staticmethod
    def new_using(
        res: ProvenanceMarkResolution,
        rng,
    ) -> ProvenanceMarkGenerator:
        return ProvenanceMarkGenerator.new_with_seed(res, ProvenanceSeed.new_using(rng))

    @staticmethod
    def new_random(res: ProvenanceMarkResolution) -> ProvenanceMarkGenerator:
        return ProvenanceMarkGenerator.new_with_seed(res, ProvenanceSeed.new())

    def res(self) -> ProvenanceMarkResolution:
        return self._res

    def seed(self) -> ProvenanceSeed:
        return self._seed

    def chain_id(self) -> bytes:
        return self._chain_id

    def next_seq(self) -> int:
        return self._next_seq

    def rng_state(self) -> RngState:
        return self._rng_state

    def next(self, date: Date, info: object | None = None) -> ProvenanceMark:
        rng = Xoshiro256StarStar.from_data(self._rng_state.to_bytes())
        seq = self._next_seq
        self._next_seq += 1

        if seq == 0:
            key = self._chain_id
        else:
            key = rng.next_bytes(self._res.link_length())
            self._rng_state = RngState.from_bytes(rng.to_data())

        next_rng = rng.clone()
        next_key = next_rng.next_bytes(self._res.link_length())
        return ProvenanceMark.new(
            self._res,
            key,
            next_key,
            self._chain_id,
            seq,
            date,
            info,
        )

    def to_envelope(self) -> Envelope:
        return (
            Envelope(self._chain_id)
            .add_type("provenance-generator")
            .add_assertion("res", resolution_to_cbor(self._res))
            .add_assertion("seed", self._seed.to_cbor())
            .add_assertion("next-seq", self._next_seq)
            .add_assertion("rng-state", self._rng_state.to_cbor())
        )

    @staticmethod
    def from_envelope(envelope: Envelope) -> ProvenanceMarkGenerator:
        envelope.check_type("provenance-generator")
        chain_id = envelope.subject().try_leaf().try_byte_string()
        if len(envelope.assertions()) != 5:
            raise Error(
                "ExtraKeys",
                f"wrong number of keys: expected 5, got {len(envelope.assertions())}",
            )
        return ProvenanceMarkGenerator.new(
            resolution_from_cbor(envelope.object_for_predicate("res").try_leaf()),
            ProvenanceSeed.from_cbor(envelope.object_for_predicate("seed").try_leaf()),
            chain_id,
            envelope.object_for_predicate("next-seq").try_leaf().try_int(),
            RngState.from_cbor(envelope.object_for_predicate("rng-state").try_leaf()),
        )

    def to_json(self) -> dict[str, object]:
        return {
            "res": int(self._res),
            "seed": self._seed.to_json(),
            "chainID": b64encode(self._chain_id),
            "nextSeq": self._next_seq,
            "rngState": self._rng_state.to_json(),
        }

    @staticmethod
    def from_json(value: str | dict[str, object]) -> ProvenanceMarkGenerator:
        payload = value if isinstance(value, dict) else json.loads(value)
        return ProvenanceMarkGenerator.new(
            ProvenanceMarkResolution(int(payload["res"])),
            ProvenanceSeed.from_json(str(payload["seed"])),
            b64decode(str(payload["chainID"])),
            int(payload["nextSeq"]),
            RngState.from_json(str(payload["rngState"])),
        )

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, ProvenanceMarkGenerator):
            return NotImplemented
        return (
            self._res == other._res
            and self._seed == other._seed
            and self._chain_id == other._chain_id
            and self._next_seq == other._next_seq
            and self._rng_state == other._rng_state
        )

    def __repr__(self) -> str:
        return (
            "ProvenanceMarkGenerator("
            f"chainID: {self._chain_id.hex()}, "
            f"res: {self._res}, "
            f"seed: {self._seed.hex()}, "
            f"nextSeq: {self._next_seq}, "
            f"rngState: {self._rng_state.hex()})"
        )

    __str__ = __repr__

