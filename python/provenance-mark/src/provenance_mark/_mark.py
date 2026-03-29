"""Provenance mark core type."""

from __future__ import annotations

from dataclasses import dataclass
from urllib.parse import parse_qsl, urlencode, urlparse, urlunparse

import bc_tags
from bc_envelope import Envelope, FormatContext, register_tags as envelope_register_tags
from bc_envelope import register_tags_in as envelope_register_tags_in
from bc_ur import (
    BytewordsStyle,
    UR,
    bytewords,
    from_ur,
    from_ur_string,
    to_ur,
    to_ur_string,
)
from bc_ur._bytewords_constants import BYTEMOJIS, BYTEWORDS, MINIMALS
from dcbor import CBOR, Date, Tag

from ._crypto_utils import SHA256_SIZE, obfuscate, sha256, sha256_prefix
from ._error import Error
from ._resolution import (
    ProvenanceMarkResolution,
    deserialize_seq,
    resolution_from_cbor,
    resolution_to_cbor,
)
from ._util import (
    b64decode,
    b64encode,
    deserialize_cbor,
    deserialize_iso8601,
    to_cbor_value,
)
from ._validate import ValidationIssue, ValidationReport, _date_ordering, _hash_mismatch, _sequence_gap

PROVENANCE_MARK_PREFIX = "\U0001F15F"


def _encode_words(data: bytes) -> str:
    return " ".join(BYTEWORDS[byte] for byte in data).upper()


def _encode_bytemojis(data: bytes) -> str:
    return " ".join(BYTEMOJIS[byte] for byte in data).upper()


def _encode_minimal(data: bytes) -> str:
    return "".join(MINIMALS[byte] for byte in data).upper()


@dataclass(frozen=True, slots=True)
class ProvenanceMark:
    """An individual provenance mark in a sequence."""

    _resolution: ProvenanceMarkResolution
    _key: bytes
    _hash: bytes
    _chain_id: bytes
    _seq_bytes: bytes
    _date_bytes: bytes
    _info_bytes: bytes
    _seq: int
    _date: Date

    @staticmethod
    def new(
        resolution: ProvenanceMarkResolution,
        key: bytes | bytearray,
        next_key: bytes | bytearray,
        chain_id: bytes | bytearray,
        seq: int,
        date: Date,
        info: object | None = None,
    ) -> ProvenanceMark:
        key_bytes = bytes(key)
        next_key_bytes = bytes(next_key)
        chain_id_bytes = bytes(chain_id)
        expected = resolution.link_length()
        if len(key_bytes) != expected:
            raise Error(
                "InvalidKeyLength",
                f"invalid key length: expected {expected}, got {len(key_bytes)}",
            )
        if len(next_key_bytes) != expected:
            raise Error(
                "InvalidNextKeyLength",
                (
                    "invalid next key length: expected "
                    f"{expected}, got {len(next_key_bytes)}"
                ),
            )
        if len(chain_id_bytes) != expected:
            raise Error(
                "InvalidChainIdLength",
                (
                    "invalid chain ID length: expected "
                    f"{expected}, got {len(chain_id_bytes)}"
                ),
            )

        date_bytes = resolution.serialize_date(date)
        seq_bytes = resolution.serialize_seq(seq)
        round_tripped_date = resolution.deserialize_date(date_bytes)
        info_bytes = (
            to_cbor_value(info).to_cbor_data()
            if info is not None
            else b""
        )
        hash_bytes = ProvenanceMark._make_hash(
            resolution,
            key_bytes,
            next_key_bytes,
            chain_id_bytes,
            seq_bytes,
            date_bytes,
            info_bytes,
        )
        return ProvenanceMark(
            resolution,
            key_bytes,
            hash_bytes,
            chain_id_bytes,
            seq_bytes,
            date_bytes,
            info_bytes,
            seq,
            round_tripped_date,
        )

    @staticmethod
    def from_message(
        resolution: ProvenanceMarkResolution,
        message: bytes | bytearray,
    ) -> ProvenanceMark:
        message_bytes = bytes(message)
        if len(message_bytes) < resolution.fixed_length():
            raise Error(
                "InvalidMessageLength",
                (
                    "invalid message length: expected at least "
                    f"{resolution.fixed_length()}, got {len(message_bytes)}"
                ),
            )

        link_length = resolution.link_length()
        key = message_bytes[:link_length]
        payload = obfuscate(key, message_bytes[link_length:])
        hash_range = resolution.hash_range()
        seq_range = resolution.seq_bytes_range()
        date_range = resolution.date_bytes_range()
        hash_bytes = payload[hash_range.start : hash_range.stop]
        chain_id = payload[: resolution.chain_id_range().stop]
        seq_bytes = payload[seq_range.start : seq_range.stop]
        date_bytes = payload[date_range.start : date_range.stop]
        seq = deserialize_seq(resolution, seq_bytes)
        date = resolution.deserialize_date(date_bytes)
        info_bytes = payload[resolution.info_range().start :]
        if info_bytes:
            try:
                CBOR.from_data(info_bytes)
            except Exception as exc:
                raise Error("InvalidInfoCbor", "invalid CBOR data in info field") from exc
        return ProvenanceMark(
            resolution,
            key,
            hash_bytes,
            chain_id,
            seq_bytes,
            date_bytes,
            info_bytes,
            seq,
            date,
        )

    @staticmethod
    def _make_hash(
        resolution: ProvenanceMarkResolution,
        key: bytes,
        next_key: bytes,
        chain_id: bytes,
        seq_bytes: bytes,
        date_bytes: bytes,
        info_bytes: bytes,
    ) -> bytes:
        payload = key + next_key + chain_id + seq_bytes + date_bytes + info_bytes
        return sha256_prefix(payload, resolution.link_length())

    def res(self) -> ProvenanceMarkResolution:
        return self._resolution

    def key(self) -> bytes:
        return self._key

    def hash(self) -> bytes:
        return self._hash

    def chain_id(self) -> bytes:
        return self._chain_id

    def seq_bytes(self) -> bytes:
        return self._seq_bytes

    def date_bytes(self) -> bytes:
        return self._date_bytes

    def seq(self) -> int:
        return self._seq

    def date(self) -> Date:
        return self._date

    def message(self) -> bytes:
        payload = (
            self._chain_id
            + self._hash
            + self._seq_bytes
            + self._date_bytes
            + self._info_bytes
        )
        return self._key + obfuscate(self._key, payload)

    def info(self) -> CBOR | None:
        return CBOR.from_data(self._info_bytes) if self._info_bytes else None

    def id(self) -> bytes:
        result = bytearray(32)
        result[: len(self._hash)] = self._hash
        if len(self._hash) < 32:
            fingerprint = self.fingerprint()
            result[len(self._hash) :] = fingerprint[: 32 - len(self._hash)]
        return bytes(result)

    def id_hex(self) -> str:
        return self.id().hex()

    def id_bytewords(self, word_count: int, prefix: bool) -> str:
        if word_count < 4 or word_count > 32:
            raise ValueError(f"word_count must be 4..=32, got {word_count}")
        text = _encode_words(self.id()[:word_count])
        return f"{PROVENANCE_MARK_PREFIX} {text}" if prefix else text

    def id_bytemoji(self, word_count: int, prefix: bool) -> str:
        if word_count < 4 or word_count > 32:
            raise ValueError(f"word_count must be 4..=32, got {word_count}")
        text = _encode_bytemojis(self.id()[:word_count])
        return f"{PROVENANCE_MARK_PREFIX} {text}" if prefix else text

    def id_bytewords_minimal(self, word_count: int, prefix: bool) -> str:
        if word_count < 4 or word_count > 32:
            raise ValueError(f"word_count must be 4..=32, got {word_count}")
        text = _encode_minimal(self.id()[:word_count])
        return f"{PROVENANCE_MARK_PREFIX} {text}" if prefix else text

    @staticmethod
    def _minimal_noncolliding_prefix_lengths(ids: list[bytes]) -> list[int]:
        lengths = [4] * len(ids)
        groups: dict[bytes, list[int]] = {}
        for index, value in enumerate(ids):
            groups.setdefault(value[:4], []).append(index)
        for indices in groups.values():
            if len(indices) > 1:
                ProvenanceMark._resolve_collision_group(ids, indices, lengths)
        return lengths

    @staticmethod
    def _resolve_collision_group(
        ids: list[bytes],
        initial_indices: list[int],
        lengths: list[int],
    ) -> None:
        unresolved = list(initial_indices)
        for prefix_length in range(5, 33):
            sub_groups: dict[bytes, list[int]] = {}
            for index in unresolved:
                sub_groups.setdefault(ids[index][:prefix_length], []).append(index)
            next_unresolved: list[int] = []
            for indices in sub_groups.values():
                if len(indices) == 1:
                    lengths[indices[0]] = prefix_length
                else:
                    next_unresolved.extend(indices)
            if not next_unresolved:
                return
            unresolved = next_unresolved
        for index in unresolved:
            lengths[index] = 32

    @staticmethod
    def disambiguated_id_bytewords(
        marks: list[ProvenanceMark],
        prefix: bool,
    ) -> list[str]:
        ids = [mark.id() for mark in marks]
        lengths = ProvenanceMark._minimal_noncolliding_prefix_lengths(ids)
        return [
            (f"{PROVENANCE_MARK_PREFIX} {_encode_words(value[:length])}" if prefix else _encode_words(value[:length]))
            for value, length in zip(ids, lengths, strict=True)
        ]

    @staticmethod
    def disambiguated_id_bytemoji(
        marks: list[ProvenanceMark],
        prefix: bool,
    ) -> list[str]:
        ids = [mark.id() for mark in marks]
        lengths = ProvenanceMark._minimal_noncolliding_prefix_lengths(ids)
        return [
            (f"{PROVENANCE_MARK_PREFIX} {_encode_bytemojis(value[:length])}" if prefix else _encode_bytemojis(value[:length]))
            for value, length in zip(ids, lengths, strict=True)
        ]

    def precedes(self, next_mark: ProvenanceMark) -> bool:
        try:
            self.precedes_opt(next_mark)
            return True
        except Error:
            return False

    def precedes_opt(self, next_mark: ProvenanceMark) -> None:
        if next_mark._seq == 0:
            issue = ValidationIssue("NonGenesisAtZero")
            raise Error("Validation", str(issue), issue)
        if next_mark._key == next_mark._chain_id:
            issue = ValidationIssue("InvalidGenesisKey")
            raise Error("Validation", str(issue), issue)
        if self._seq != next_mark._seq - 1:
            issue = _sequence_gap(self._seq + 1, next_mark._seq)
            raise Error("Validation", str(issue), issue)
        if self._date > next_mark._date:
            issue = _date_ordering(self._date, next_mark._date)
            raise Error("Validation", str(issue), issue)

        expected_hash = self._make_hash(
            self._resolution,
            self._key,
            next_mark._key,
            self._chain_id,
            self._seq_bytes,
            self._date_bytes,
            self._info_bytes,
        )
        if self._hash != expected_hash:
            issue = _hash_mismatch(expected_hash, self._hash)
            raise Error("Validation", str(issue), issue)

    @staticmethod
    def is_sequence_valid(marks: list[ProvenanceMark]) -> bool:
        if len(marks) < 2:
            return False
        if marks[0]._seq == 0 and not marks[0].is_genesis():
            return False
        return all(
            previous.precedes(next_mark)
            for previous, next_mark in zip(marks, marks[1:])
        )

    def is_genesis(self) -> bool:
        return self._seq == 0 and self._key == self._chain_id

    def to_bytewords_with_style(self, style: BytewordsStyle) -> str:
        return bytewords.encode(self.message(), style)

    def to_bytewords(self) -> str:
        return self.to_bytewords_with_style(BytewordsStyle.STANDARD)

    @staticmethod
    def from_bytewords(
        resolution: ProvenanceMarkResolution,
        value: str,
    ) -> ProvenanceMark:
        try:
            return ProvenanceMark.from_message(
                resolution,
                bytewords.decode(value, BytewordsStyle.STANDARD),
            )
        except Exception as exc:
            raise Error("Bytewords", f"bytewords error: {exc}") from exc

    def to_url_encoding(self) -> str:
        return bytewords.encode(self.tagged_cbor_data(), BytewordsStyle.MINIMAL)

    @staticmethod
    def from_url_encoding(value: str) -> ProvenanceMark:
        try:
            return ProvenanceMark.from_tagged_cbor(
                CBOR.from_data(bytewords.decode(value, BytewordsStyle.MINIMAL))
            )
        except Exception as exc:
            raise Error("Bytewords", f"bytewords error: {exc}") from exc

    def to_url(self, base: str) -> str:
        parsed = urlparse(base)
        query = dict(parse_qsl(parsed.query, keep_blank_values=True))
        query["provenance"] = self.to_url_encoding()
        return urlunparse(parsed._replace(query=urlencode(query)))

    @staticmethod
    def from_url(url: str) -> ProvenanceMark:
        parsed = urlparse(url)
        query = dict(parse_qsl(parsed.query, keep_blank_values=True))
        provenance = query.get("provenance")
        if provenance is None:
            raise Error(
                "MissingUrlParameter",
                "missing required URL parameter: provenance",
            )
        return ProvenanceMark.from_url_encoding(provenance)

    @staticmethod
    def cbor_tags() -> list[Tag]:
        bc_tags.register_tags()
        return bc_tags.tags_for_values([bc_tags.TAG_PROVENANCE_MARK])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_array(
            [
                resolution_to_cbor(self._resolution),
                CBOR.from_bytes(self.message()),
            ]
        )

    def tagged_cbor(self) -> CBOR:
        return CBOR.from_tagged_value(self.cbor_tags()[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> ProvenanceMark:
        items = cbor.try_array()
        if len(items) != 2:
            raise Error("Cbor", "Invalid provenance mark length")
        resolution = resolution_from_cbor(items[0])
        message = items[1].try_byte_string()
        return cls.from_message(resolution, message)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> ProvenanceMark:
        item = cbor.try_expected_tagged_value(cls.cbor_tags()[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes | bytearray) -> ProvenanceMark:
        return cls.from_tagged_cbor(CBOR.from_data(data))

    def to_ur(self) -> UR:
        return to_ur(self)

    def ur_string(self) -> str:
        return to_ur_string(self)

    @classmethod
    def from_ur(cls, ur_value: UR) -> ProvenanceMark:
        return from_ur(cls, ur_value)

    @classmethod
    def from_ur_string(cls, value: str) -> ProvenanceMark:
        return from_ur_string(cls, value)

    def to_envelope(self) -> Envelope:
        return Envelope(self.tagged_cbor())

    @classmethod
    def from_envelope(cls, envelope: Envelope) -> ProvenanceMark:
        return cls.from_tagged_cbor(envelope.subject().try_leaf())

    def fingerprint(self) -> bytes:
        return sha256(self.tagged_cbor_data())

    @staticmethod
    def register_tags() -> None:
        bc_tags.register_tags()
        envelope_register_tags()
        from bc_envelope import with_format_context_mut

        def _register(context: FormatContext) -> None:
            ProvenanceMark.register_tags_in(context)

        with_format_context_mut(_register)

    @staticmethod
    def register_tags_in(context: FormatContext) -> None:
        bc_tags.register_tags()
        envelope_register_tags_in(context)
        context.tags.set_summarizer(
            bc_tags.TAG_PROVENANCE_MARK,
            lambda untagged_cbor, _flat: str(
                ProvenanceMark.from_untagged_cbor(untagged_cbor)
            ),
        )

    @staticmethod
    def validate(marks: list[ProvenanceMark]) -> ValidationReport:
        return ValidationReport.validate(list(marks))

    def to_json(self) -> dict[str, object]:
        result: dict[str, object] = {
            "seq": self._seq,
            "date": str(self._date),
            "res": int(self._resolution),
            "chain_id": b64encode(self._chain_id),
            "key": b64encode(self._key),
            "hash": b64encode(self._hash),
        }
        if self._info_bytes:
            result["info_bytes"] = b64encode(self._info_bytes)
        return result

    @staticmethod
    def from_json(value: str | dict[str, object]) -> ProvenanceMark:
        payload = value if isinstance(value, dict) else __import__("json").loads(value)
        resolution = ProvenanceMarkResolution(int(payload["res"]))
        date = deserialize_iso8601(str(payload["date"]))
        return ProvenanceMark(
            resolution,
            b64decode(str(payload["key"])),
            b64decode(str(payload["hash"])),
            b64decode(str(payload["chain_id"])),
            resolution.serialize_seq(int(payload["seq"])),
            resolution.serialize_date(date),
            b64decode(str(payload["info_bytes"])) if "info_bytes" in payload else b"",
            int(payload["seq"]),
            date,
        )

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, ProvenanceMark):
            return NotImplemented
        return (
            self._resolution == other._resolution
            and self.message() == other.message()
        )

    def __hash__(self) -> int:
        return hash((self._resolution, self.message()))

    def __repr__(self) -> str:
        parts = [
            f"key: {self._key.hex()}",
            f"hash: {self._hash.hex()}",
            f"chainID: {self._chain_id.hex()}",
            f"seq: {self._seq}",
            f"date: {self._date}",
        ]
        info = self.info()
        if info is not None:
            parts.append(f"info: {info.diagnostic()}")
        return f"ProvenanceMark({', '.join(parts)})"

    def __str__(self) -> str:
        return f"ProvenanceMark({self.id_hex()})"
