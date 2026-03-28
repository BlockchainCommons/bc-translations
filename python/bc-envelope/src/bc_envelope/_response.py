"""Response class for Gordian Envelope expressions.

A ``Response`` contains either a successful result or an error, paired
with the original request's ARID identifier.
Tagged with #6.40005 (TAG_RESPONSE).
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_components import ARID
from bc_tags import TAG_RESPONSE
from dcbor import CBOR
from known_values import KnownValue
import known_values

from ._error import GeneralError

if TYPE_CHECKING:
    from ._envelope import Envelope


# ===========================================================================
# ResponseBehavior protocol
# ===========================================================================

@runtime_checkable
class ResponseBehavior(Protocol):
    """Protocol for types that behave like responses."""

    def with_result(self, result: object) -> ResponseBehavior: ...
    def with_error(self, error: object) -> ResponseBehavior: ...

    @property
    def is_success(self) -> bool: ...
    @property
    def is_failure(self) -> bool: ...
    def result_envelope(self) -> Envelope: ...
    def error_envelope(self) -> Envelope: ...
    @property
    def id(self) -> ARID | None: ...


# ===========================================================================
# Response class
# ===========================================================================

class Response:
    """A response to a Request, containing either a result or an error."""

    __slots__ = ("_is_success_flag", "_id", "_result_or_error")

    def __init__(
        self,
        *,
        success: bool,
        response_id: ARID | None,
        result_or_error: Envelope,
    ) -> None:
        self._is_success_flag = success
        self._id = response_id
        self._result_or_error = result_or_error

    # --- Factories ---

    @staticmethod
    def new_success(request_id: ARID) -> Response:
        from ._envelope import Envelope as Env
        return Response(
            success=True,
            response_id=request_id,
            result_or_error=Env(known_values.OK_VALUE),
        )

    @staticmethod
    def new_failure(request_id: ARID) -> Response:
        from ._envelope import Envelope as Env
        return Response(
            success=False,
            response_id=request_id,
            result_or_error=Env(known_values.UNKNOWN_VALUE),
        )

    @staticmethod
    def new_early_failure() -> Response:
        from ._envelope import Envelope as Env
        return Response(
            success=False,
            response_id=None,
            result_or_error=Env(known_values.UNKNOWN_VALUE),
        )

    # --- Composition ---

    def with_result(self, result: object) -> Response:
        from ._envelope import Envelope as Env

        if not self._is_success_flag:
            raise ValueError("Cannot set result on a failed response")
        self._result_or_error = Env(result)
        return self

    def with_optional_result(self, result: object | None) -> Response:
        from ._envelope import Envelope as Env

        if result is not None:
            return self.with_result(result)
        self._result_or_error = Env.null()
        return self

    def with_error(self, error: object) -> Response:
        from ._envelope import Envelope as Env

        if self._is_success_flag:
            raise ValueError("Cannot set error on a successful response")
        self._result_or_error = Env(error)
        return self

    def with_optional_error(self, error: object | None) -> Response:
        if error is not None:
            return self.with_error(error)
        return self

    # --- Parsing ---

    @property
    def is_success(self) -> bool:
        return self._is_success_flag

    @property
    def is_failure(self) -> bool:
        return not self._is_success_flag

    @property
    def id(self) -> ARID | None:
        return self._id

    def expect_id(self) -> ARID:
        assert self._id is not None, "Expected an ID"
        return self._id

    def result_envelope(self) -> Envelope:
        if not self._is_success_flag:
            raise GeneralError("Cannot get result from failed response")
        return self._result_or_error

    def extract_result(self) -> object:
        from ._envelope_encodable import _auto_extract_subject
        return _auto_extract_subject(self.result_envelope())

    def error_envelope(self) -> Envelope:
        if self._is_success_flag:
            raise GeneralError("Cannot get error from successful response")
        return self._result_or_error

    def extract_error(self) -> object:
        from ._envelope_encodable import _auto_extract_subject
        return _auto_extract_subject(self.error_envelope())

    # --- Summary ---

    def summary(self) -> str:
        if self._is_success_flag:
            return (
                f"id: {self._id.short_description()}, "
                f"result: {self._result_or_error.format_flat()}"
            )
        if self._id is not None:
            return (
                f"id: {self._id.short_description()} "
                f"error: {self._result_or_error.format_flat()}"
            )
        return f"id: 'Unknown' error: {self._result_or_error.format_flat()}"

    # --- Envelope conversion ---

    def to_envelope(self) -> Envelope:
        """Convert this response to an Envelope."""
        from ._envelope import Envelope as Env

        if self._is_success_flag:
            subject = Env(
                CBOR.from_tagged_value(CBOR.from_int(TAG_RESPONSE), self._id.to_cbor())
            )
            return subject.add_assertion(known_values.RESULT, self._result_or_error)
        else:
            if self._id is not None:
                subject = Env(
                    CBOR.from_tagged_value(
                        CBOR.from_int(TAG_RESPONSE), self._id.to_cbor(),
                    )
                )
            else:
                subject = Env(
                    CBOR.from_tagged_value(
                        CBOR.from_int(TAG_RESPONSE),
                        known_values.UNKNOWN_VALUE.tagged_cbor(),
                    )
                )
            return subject.add_assertion(known_values.ERROR, self._result_or_error)

    @staticmethod
    def from_envelope(envelope: Envelope) -> Response:
        """Parse a Response from an envelope."""
        from ._error import InvalidResponse

        has_result = False
        has_error = False
        try:
            envelope.assertion_with_predicate(known_values.RESULT)
            has_result = True
        except Exception:
            pass
        try:
            envelope.assertion_with_predicate(known_values.ERROR)
            has_error = True
        except Exception:
            pass

        if has_result == has_error:
            raise InvalidResponse()

        subject_cbor = envelope.subject().try_leaf()
        id_value = subject_cbor.try_expected_tagged_value(CBOR.from_int(TAG_RESPONSE))

        if has_result:
            arid = ARID.from_tagged_cbor(id_value)
            result_env = envelope.object_for_predicate(known_values.RESULT)
            return Response(success=True, response_id=arid, result_or_error=result_env)

        # Error path — id_value is either tagged(ARID, bytes) or tagged(KNOWN_VALUE, int)
        try:
            kv = KnownValue.from_tagged_cbor(id_value)
            if kv == known_values.UNKNOWN_VALUE:
                response_id = None
            else:
                raise InvalidResponse()
        except InvalidResponse:
            raise
        except Exception:
            try:
                response_id = ARID.from_tagged_cbor(id_value)
            except Exception:
                raise InvalidResponse()

        error_env = envelope.object_for_predicate(known_values.ERROR)
        return Response(success=False, response_id=response_id, result_or_error=error_env)

    # --- Equality ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Response):
            return NotImplemented
        return (
            self._is_success_flag == other._is_success_flag
            and self._id == other._id
            and self._result_or_error.digest() == other._result_or_error.digest()
        )

    def __repr__(self) -> str:
        return f"Response({self.summary()})"

    def __str__(self) -> str:
        return f"Response({self.summary()})"
