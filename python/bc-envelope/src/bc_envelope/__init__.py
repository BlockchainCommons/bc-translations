"""Gordian Envelope for Python.

Provides a hierarchical binary data format with selective disclosure,
cryptographic integrity, and deterministic CBOR serialization.
"""

# Core types (import order matters for circular-dependency resolution)
from ._error import (
    AlreadyCompressed,
    AlreadyElided,
    AlreadyEncrypted,
    AmbiguousAttachment,
    AmbiguousEdge,
    AmbiguousPredicate,
    AmbiguousType,
    EdgeDuplicateIsA,
    EdgeDuplicateSource,
    EdgeDuplicateTarget,
    EdgeMissingIsA,
    EdgeMissingSource,
    EdgeMissingTarget,
    EdgeUnexpectedAssertion,
    EnvelopeError,
    GeneralError,
    InvalidAssertion,
    InvalidAttachment,
    InvalidDigest,
    InvalidFormat,
    InvalidInnerSignatureType,
    InvalidOuterSignatureType,
    InvalidResponse,
    InvalidShares,
    InvalidSignatureType,
    InvalidType,
    MissingDigest,
    NonexistentAttachment,
    NonexistentEdge,
    NonexistentPredicate,
    NotAssertion,
    NotCompressed,
    NotEncrypted,
    NotKnownValue,
    NotLeaf,
    NotWrapped,
    SubjectNotUnit,
    UnexpectedResponseID,
    UnknownRecipient,
    UnknownSecret,
    UnverifiedInnerSignature,
    UnverifiedSignature,
)
from ._envelope_case import CaseType, EnvelopeCase
from ._assertion import Assertion
from ._envelope import Envelope, envelope_encodable
from ._envelope_encodable import extract_subject

# Import these modules for their side effects (they monkey-patch Envelope)
from . import _cbor as _cbor_mod  # noqa: F401

# Import leaf helpers and digest ops as standalone functions
from . import _leaf as leaf
from . import _digest_ops as digest_ops
from ._walk import EdgeType, Visitor, walk

# Attach digest-ops methods to Envelope
from . import _digest_ops

Envelope.is_equivalent_to = _digest_ops.is_equivalent_to  # type: ignore[attr-defined]
Envelope.is_identical_to = _digest_ops.is_identical_to  # type: ignore[attr-defined]
Envelope.structural_digest = _digest_ops.structural_digest  # type: ignore[attr-defined]
Envelope.digests = _digest_ops.digests  # type: ignore[attr-defined]
Envelope.deep_digests = _digest_ops.deep_digests  # type: ignore[attr-defined]
Envelope.shallow_digests = _digest_ops.shallow_digests  # type: ignore[attr-defined]

# Attach walk method
Envelope.walk = walk  # type: ignore[attr-defined]

# Attach leaf helper methods
Envelope.is_false = leaf.is_false  # type: ignore[attr-defined]
Envelope.is_true = leaf.is_true  # type: ignore[attr-defined]
Envelope.is_bool = leaf.is_bool  # type: ignore[attr-defined]
Envelope.is_number_leaf = leaf.is_number  # type: ignore[attr-defined]
Envelope.is_subject_number = leaf.is_subject_number  # type: ignore[attr-defined]
Envelope.is_nan_leaf = leaf.is_nan  # type: ignore[attr-defined]
Envelope.is_subject_nan = leaf.is_subject_nan  # type: ignore[attr-defined]
Envelope.is_null = leaf.is_null  # type: ignore[attr-defined]
Envelope.try_byte_string = leaf.try_byte_string  # type: ignore[attr-defined]
Envelope.as_byte_string = leaf.as_byte_string  # type: ignore[attr-defined]
Envelope.as_array = leaf.as_array  # type: ignore[attr-defined]
Envelope.as_map = leaf.as_map  # type: ignore[attr-defined]
Envelope.as_text = leaf.as_text  # type: ignore[attr-defined]
Envelope.as_known_value = leaf.as_known_value  # type: ignore[attr-defined]
Envelope.try_known_value = leaf.try_known_value  # type: ignore[attr-defined]
Envelope.is_known_value_leaf = leaf.is_known_value_case  # type: ignore[attr-defined]
Envelope.is_subject_unit = leaf.is_subject_unit  # type: ignore[attr-defined]
Envelope.check_subject_unit = leaf.check_subject_unit  # type: ignore[attr-defined]

# Attach elide methods
from . import _elide
from ._elide import ObscureAction, ObscureType

Envelope.elide = _elide.elide  # type: ignore[attr-defined]
Envelope.elide_removing_set_with_action = _elide.elide_removing_set_with_action  # type: ignore[attr-defined]
Envelope.elide_removing_set = _elide.elide_removing_set  # type: ignore[attr-defined]
Envelope.elide_removing_array_with_action = _elide.elide_removing_array_with_action  # type: ignore[attr-defined]
Envelope.elide_removing_array = _elide.elide_removing_array  # type: ignore[attr-defined]
Envelope.elide_removing_target_with_action = _elide.elide_removing_target_with_action  # type: ignore[attr-defined]
Envelope.elide_removing_target = _elide.elide_removing_target  # type: ignore[attr-defined]
Envelope.elide_revealing_set_with_action = _elide.elide_revealing_set_with_action  # type: ignore[attr-defined]
Envelope.elide_revealing_set = _elide.elide_revealing_set  # type: ignore[attr-defined]
Envelope.elide_revealing_array_with_action = _elide.elide_revealing_array_with_action  # type: ignore[attr-defined]
Envelope.elide_revealing_array = _elide.elide_revealing_array  # type: ignore[attr-defined]
Envelope.elide_revealing_target_with_action = _elide.elide_revealing_target_with_action  # type: ignore[attr-defined]
Envelope.elide_revealing_target = _elide.elide_revealing_target  # type: ignore[attr-defined]
Envelope.unelide = _elide.unelide  # type: ignore[attr-defined]
Envelope.nodes_matching = _elide.nodes_matching  # type: ignore[attr-defined]
Envelope.walk_unelide = _elide.walk_unelide  # type: ignore[attr-defined]
Envelope.walk_replace = _elide.walk_replace  # type: ignore[attr-defined]
Envelope.walk_decrypt = _elide.walk_decrypt  # type: ignore[attr-defined]
Envelope.walk_decompress = _elide.walk_decompress  # type: ignore[attr-defined]

# Attach assertion methods
from . import _assertions

Envelope.add_assertion = _assertions.add_assertion  # type: ignore[attr-defined]
Envelope.add_assertion_envelope = _assertions.add_assertion_envelope  # type: ignore[attr-defined]
Envelope.add_assertion_envelopes = _assertions.add_assertion_envelopes  # type: ignore[attr-defined]
Envelope.add_optional_assertion_envelope = _assertions.add_optional_assertion_envelope  # type: ignore[attr-defined]
Envelope.add_optional_assertion = _assertions.add_optional_assertion  # type: ignore[attr-defined]
Envelope.add_nonempty_string_assertion = _assertions.add_nonempty_string_assertion  # type: ignore[attr-defined]
Envelope.add_assertions = _assertions.add_assertions  # type: ignore[attr-defined]
Envelope.add_assertion_if = _assertions.add_assertion_if  # type: ignore[attr-defined]
Envelope.add_assertion_envelope_if = _assertions.add_assertion_envelope_if  # type: ignore[attr-defined]
Envelope.remove_assertion = _assertions.remove_assertion  # type: ignore[attr-defined]
Envelope.replace_assertion = _assertions.replace_assertion  # type: ignore[attr-defined]
Envelope.replace_subject = _assertions.replace_subject  # type: ignore[attr-defined]

# Attach query methods
from . import _queries

Envelope.assertions_with_predicate = _queries.assertions_with_predicate  # type: ignore[attr-defined]
Envelope.assertion_with_predicate = _queries.assertion_with_predicate  # type: ignore[attr-defined]
Envelope.objects_for_predicate = _queries.objects_for_predicate  # type: ignore[attr-defined]
Envelope.object_for_predicate = _queries.object_for_predicate  # type: ignore[attr-defined]
Envelope.extract_subject_type = _queries.extract_subject_method  # type: ignore[attr-defined]
Envelope.extract_subject = _queries.extract_subject_auto  # type: ignore[attr-defined]
Envelope.extract_object_type = _queries.extract_object_method  # type: ignore[attr-defined]
Envelope.extract_predicate_type = _queries.extract_predicate_method  # type: ignore[attr-defined]
Envelope.extract_object_for_predicate = _queries.extract_object_for_predicate  # type: ignore[attr-defined]
Envelope.extract_optional_object_for_predicate = _queries.extract_optional_object_for_predicate  # type: ignore[attr-defined]
Envelope.extract_object_for_predicate_with_default = _queries.extract_object_for_predicate_with_default  # type: ignore[attr-defined]
Envelope.extract_objects_for_predicate = _queries.extract_objects_for_predicate  # type: ignore[attr-defined]
Envelope.elements_count = _queries.elements_count  # type: ignore[attr-defined]

# Attach wrap/unwrap methods
from . import _wrap

Envelope.wrap = _wrap.wrap  # type: ignore[attr-defined]
Envelope.try_unwrap = _wrap.try_unwrap  # type: ignore[attr-defined]

# Attach encrypt/decrypt methods
from . import _encrypt

Envelope.encrypt_subject = _encrypt.encrypt_subject  # type: ignore[attr-defined]
Envelope.encrypt_subject_opt = _encrypt.encrypt_subject_opt  # type: ignore[attr-defined]
Envelope.decrypt_subject = _encrypt.decrypt_subject  # type: ignore[attr-defined]
Envelope.encrypt = _encrypt.encrypt  # type: ignore[attr-defined]
Envelope.decrypt = _encrypt.decrypt  # type: ignore[attr-defined]

# Attach compress/decompress methods
from . import _compress

Envelope.compress = _compress.compress  # type: ignore[attr-defined]
Envelope.decompress = _compress.decompress  # type: ignore[attr-defined]
Envelope.compress_subject = _compress.compress_subject  # type: ignore[attr-defined]
Envelope.decompress_subject = _compress.decompress_subject  # type: ignore[attr-defined]

# Import string_utils (standalone function, not attached to Envelope)
from ._string_utils import flanked_by

# ---------------------------------------------------------------------------
# Format system (Translation Units 14-19)
# ---------------------------------------------------------------------------

from ._format_context import (
    FormatContext,
    FormatContextOpt,
    register_tags,
    register_tags_in,
    with_format_context,
    with_format_context_mut,
)

# Attach format/notation methods to Envelope
from . import _notation

Envelope.format = _notation.format_envelope  # type: ignore[attr-defined]


def _format_flat(self):
    """Return envelope notation on a single line."""
    return _notation.format_envelope(self, flat=True)


Envelope.format_flat = _format_flat  # type: ignore[attr-defined]

# Attach tree format methods to Envelope
from . import _tree_format
from ._tree_format import DigestDisplayFormat, TreeFormatOpts

Envelope.tree_format = _tree_format.tree_format  # type: ignore[attr-defined]
Envelope.short_id = _tree_format.short_id  # type: ignore[attr-defined]

# Attach envelope summary method
from . import _envelope_summary

Envelope.summary = _envelope_summary.envelope_summary  # type: ignore[attr-defined]

# Attach diagnostic methods to Envelope
from . import _diagnostic

Envelope.diagnostic = _diagnostic.diagnostic  # type: ignore[attr-defined]
Envelope.diagnostic_annotated = _diagnostic.diagnostic_annotated  # type: ignore[attr-defined]

# Attach hex format methods to Envelope
from . import _hex_format

Envelope.hex = _hex_format.hex_format  # type: ignore[attr-defined]

# Attach mermaid format methods to Envelope
from . import _mermaid
from ._mermaid import MermaidFormatOpts, MermaidOrientation, MermaidTheme

Envelope.mermaid_format = _mermaid.mermaid_format  # type: ignore[attr-defined]

# ---------------------------------------------------------------------------
# Extension Units 20-33 (salt, signature, recipient, secret, sskr, proof,
# types, attachment, edge, expression system, seal)
# ---------------------------------------------------------------------------

from . import _salt
from . import _signature
from ._signature_metadata import SignatureMetadata
from . import _recipient
from . import _secret
from . import _sskr
from . import _proof
from . import _types_ext
from . import _attachment
from ._attachment import Attachable, Attachments
from . import _edge
from ._edge import Edgeable, Edges
from . import _seal

# Expression system
from ._function import Function
from ._functions import (
    ADD, SUB, MUL, DIV, NEG, LT, LE, GT, GE, EQ, NE, AND, OR, XOR, NOT,
    FunctionsStore, global_functions,
)
from ._parameter import Parameter
from ._parameters import (
    BLANK, LHS, RHS,
    ParametersStore, global_parameters,
)
from ._expression import Expression, ExpressionBehavior
from ._request import Request, RequestBehavior
from ._response import Response, ResponseBehavior
from ._event import Event, EventBehavior
from . import _functions as functions
from . import _parameters as parameters

# --- Attach salt methods ---
Envelope.add_salt = _salt.add_salt  # type: ignore[attr-defined]
Envelope.add_salt_instance = _salt.add_salt_instance  # type: ignore[attr-defined]
Envelope.add_salt_with_length = _salt.add_salt_with_length  # type: ignore[attr-defined]
Envelope.add_salt_in_range = _salt.add_salt_in_range  # type: ignore[attr-defined]
Envelope.add_salt_using = _salt.add_salt_using  # type: ignore[attr-defined]
Envelope.add_salt_with_length_using = _salt.add_salt_with_length_using  # type: ignore[attr-defined]
Envelope.add_salt_in_range_using = _salt.add_salt_in_range_using  # type: ignore[attr-defined]

# --- Attach signature methods ---
Envelope.add_signature = _signature.add_signature  # type: ignore[attr-defined]
Envelope.add_signature_opt = _signature.add_signature_opt  # type: ignore[attr-defined]
Envelope.add_signatures = _signature.add_signatures  # type: ignore[attr-defined]
Envelope.add_signatures_opt = _signature.add_signatures_opt  # type: ignore[attr-defined]
Envelope.is_verified_signature = _signature.is_verified_signature  # type: ignore[attr-defined]
Envelope.verify_signature = _signature.verify_signature  # type: ignore[attr-defined]
Envelope.has_signature_from = _signature.has_signature_from  # type: ignore[attr-defined]
Envelope.has_signature_from_returning_metadata = _signature.has_signature_from_returning_metadata  # type: ignore[attr-defined]
Envelope.verify_signature_from = _signature.verify_signature_from  # type: ignore[attr-defined]
Envelope.verify_signature_from_returning_metadata = _signature.verify_signature_from_returning_metadata  # type: ignore[attr-defined]
Envelope.has_signatures_from = _signature.has_signatures_from  # type: ignore[attr-defined]
Envelope.has_signatures_from_threshold = _signature.has_signatures_from_threshold  # type: ignore[attr-defined]
Envelope.verify_signatures_from = _signature.verify_signatures_from  # type: ignore[attr-defined]
Envelope.verify_signatures_from_threshold = _signature.verify_signatures_from_threshold  # type: ignore[attr-defined]
Envelope.sign = _signature.sign  # type: ignore[attr-defined]
Envelope.sign_opt = _signature.sign_opt  # type: ignore[attr-defined]
Envelope.verify = _signature.verify  # type: ignore[attr-defined]
Envelope.verify_returning_metadata = _signature.verify_returning_metadata  # type: ignore[attr-defined]

# --- Attach recipient methods ---
Envelope.add_recipient = _recipient.add_recipient  # type: ignore[attr-defined]
Envelope.add_recipient_opt = _recipient.add_recipient_opt  # type: ignore[attr-defined]
Envelope.recipients = _recipient.recipients  # type: ignore[attr-defined]
Envelope.encrypt_subject_to_recipients = _recipient.encrypt_subject_to_recipients  # type: ignore[attr-defined]
Envelope.encrypt_subject_to_recipients_opt = _recipient.encrypt_subject_to_recipients_opt  # type: ignore[attr-defined]
Envelope.encrypt_subject_to_recipient = _recipient.encrypt_subject_to_recipient  # type: ignore[attr-defined]
Envelope.encrypt_subject_to_recipient_opt = _recipient.encrypt_subject_to_recipient_opt  # type: ignore[attr-defined]
Envelope.decrypt_subject_to_recipient = _recipient.decrypt_subject_to_recipient  # type: ignore[attr-defined]
Envelope.encrypt_to_recipient = _recipient.encrypt_to_recipient  # type: ignore[attr-defined]
Envelope.decrypt_to_recipient = _recipient.decrypt_to_recipient  # type: ignore[attr-defined]

# --- Attach secret methods ---
Envelope.lock_subject = _secret.lock_subject  # type: ignore[attr-defined]
Envelope.unlock_subject = _secret.unlock_subject  # type: ignore[attr-defined]
Envelope.is_locked_with_password = _secret.is_locked_with_password  # type: ignore[attr-defined]
Envelope.is_locked_with_ssh_agent = _secret.is_locked_with_ssh_agent  # type: ignore[attr-defined]
Envelope.add_secret = _secret.add_secret  # type: ignore[attr-defined]
Envelope.lock = _secret.lock  # type: ignore[attr-defined]
Envelope.unlock = _secret.unlock  # type: ignore[attr-defined]

# --- Attach SSKR methods ---
Envelope.sskr_split = _sskr.sskr_split  # type: ignore[attr-defined]
Envelope.sskr_split_flattened = _sskr.sskr_split_flattened  # type: ignore[attr-defined]
Envelope.sskr_split_using = _sskr.sskr_split_using  # type: ignore[attr-defined]
Envelope.sskr_join = staticmethod(_sskr.sskr_join)  # type: ignore[attr-defined]

# --- Attach proof methods ---
Envelope.proof_contains_set = _proof.proof_contains_set  # type: ignore[attr-defined]
Envelope.proof_contains_target = _proof.proof_contains_target  # type: ignore[attr-defined]
Envelope.confirm_contains_set = _proof.confirm_contains_set  # type: ignore[attr-defined]
Envelope.confirm_contains_target = _proof.confirm_contains_target  # type: ignore[attr-defined]

# --- Attach types methods ---
Envelope.add_type = _types_ext.add_type  # type: ignore[attr-defined]
Envelope.types = _types_ext.types  # type: ignore[attr-defined]
Envelope.get_type = _types_ext.get_type  # type: ignore[attr-defined]
Envelope.has_type = _types_ext.has_type  # type: ignore[attr-defined]
Envelope.has_type_value = _types_ext.has_type_value  # type: ignore[attr-defined]
Envelope.check_type_value = _types_ext.check_type_value  # type: ignore[attr-defined]
Envelope.check_type = _types_ext.check_type  # type: ignore[attr-defined]

# --- Attach attachment methods ---
Envelope.new_attachment = staticmethod(_attachment.new_attachment)  # type: ignore[attr-defined]
Envelope.add_attachment = _attachment.add_attachment  # type: ignore[attr-defined]
Envelope.attachment_payload = _attachment.attachment_payload  # type: ignore[attr-defined]
Envelope.attachment_vendor = _attachment.attachment_vendor  # type: ignore[attr-defined]
Envelope.attachment_conforms_to = _attachment.attachment_conforms_to  # type: ignore[attr-defined]
Envelope.validate_attachment = _attachment.validate_attachment  # type: ignore[attr-defined]
Envelope.attachments = _attachment.attachments  # type: ignore[attr-defined]
Envelope.attachments_with_vendor_and_conforms_to = _attachment.attachments_with_vendor_and_conforms_to  # type: ignore[attr-defined]
Envelope.attachment_with_vendor_and_conforms_to = _attachment.attachment_with_vendor_and_conforms_to  # type: ignore[attr-defined]

# --- Attach edge methods ---
Envelope.add_edge_envelope = _edge.add_edge_envelope  # type: ignore[attr-defined]
Envelope.edges = _edge.edges  # type: ignore[attr-defined]
Envelope.validate_edge = _edge.validate_edge  # type: ignore[attr-defined]
Envelope.edge_is_a = _edge.edge_is_a  # type: ignore[attr-defined]
Envelope.edge_source = _edge.edge_source  # type: ignore[attr-defined]
Envelope.edge_target = _edge.edge_target  # type: ignore[attr-defined]
Envelope.edge_subject = _edge.edge_subject  # type: ignore[attr-defined]
Envelope.edges_matching = _edge.edges_matching  # type: ignore[attr-defined]

# --- Attach seal methods ---
Envelope.seal = _seal.seal  # type: ignore[attr-defined]
Envelope.seal_opt = _seal.seal_opt  # type: ignore[attr-defined]
Envelope.unseal = _seal.unseal  # type: ignore[attr-defined]

__all__ = [
    # Error hierarchy
    "EnvelopeError",
    "AlreadyCompressed",
    "AlreadyElided",
    "AlreadyEncrypted",
    "AmbiguousAttachment",
    "AmbiguousEdge",
    "AmbiguousPredicate",
    "AmbiguousType",
    "EdgeDuplicateIsA",
    "EdgeDuplicateSource",
    "EdgeDuplicateTarget",
    "EdgeMissingIsA",
    "EdgeMissingSource",
    "EdgeMissingTarget",
    "EdgeUnexpectedAssertion",
    "GeneralError",
    "InvalidAssertion",
    "InvalidAttachment",
    "InvalidDigest",
    "InvalidFormat",
    "InvalidInnerSignatureType",
    "InvalidOuterSignatureType",
    "InvalidResponse",
    "InvalidShares",
    "InvalidSignatureType",
    "InvalidType",
    "MissingDigest",
    "NonexistentAttachment",
    "NonexistentEdge",
    "NonexistentPredicate",
    "NotAssertion",
    "NotCompressed",
    "NotEncrypted",
    "NotKnownValue",
    "NotLeaf",
    "NotWrapped",
    "SubjectNotUnit",
    "UnexpectedResponseID",
    "UnknownRecipient",
    "UnknownSecret",
    "UnverifiedInnerSignature",
    "UnverifiedSignature",
    # Core types
    "CaseType",
    "EnvelopeCase",
    "Assertion",
    "Envelope",
    "envelope_encodable",
    "extract_subject",
    # Walk
    "EdgeType",
    "Visitor",
    "walk",
    # Elide
    "ObscureAction",
    "ObscureType",
    # Utilities
    "flanked_by",
    # Format system
    "FormatContext",
    "FormatContextOpt",
    "register_tags",
    "register_tags_in",
    "with_format_context",
    "with_format_context_mut",
    "DigestDisplayFormat",
    "TreeFormatOpts",
    "MermaidFormatOpts",
    "MermaidOrientation",
    "MermaidTheme",
    # Submodules
    "leaf",
    "digest_ops",
    # Extension types (units 20-33)
    "SignatureMetadata",
    "Attachments",
    "Attachable",
    "Edges",
    "Edgeable",
    # Expression system
    "Function",
    "FunctionsStore",
    "global_functions",
    "Parameter",
    "ParametersStore",
    "global_parameters",
    "Expression",
    "ExpressionBehavior",
    "Request",
    "RequestBehavior",
    "Response",
    "ResponseBehavior",
    "Event",
    "EventBehavior",
    "functions",
    "parameters",
]
