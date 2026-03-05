/**
 * Gordian Envelope for TypeScript.
 *
 * Translation of Rust `bc-envelope`.
 */

export { EnvelopeError as Error, EnvelopeError, type EnvelopeErrorCode } from './error.js';

export { Envelope } from './envelope.js';
export type { EnvelopeCase } from './envelope-case.js';
export { Assertion } from './assertion.js';

export { EdgeType, edgeTypeLabel } from './edge-type.js';
export { ObscureActions, type ObscureAction } from './obscure-action.js';
export { ObscureType } from './obscure-type.js';

export {
    type FormatContextOpt,
    FormatContextOpts,
    FormatContext,
    getGlobalFormatContext,
    withFormatContext,
    registerTags,
    registerTagsIn,
} from './format-context.js';

export {
    type EnvelopeFunction,
    knownFunction,
    namedFunction,
    functionName,
    functionNamedName,
    functionUntaggedCbor,
    functionTaggedCbor,
    functionsEqual,
    functionFromUntaggedCbor,
    functionFromTaggedCbor,
} from './function.js';

export {
    FunctionsStore,
    ADD,
    SUB,
    MUL,
    DIV,
    NEG,
    LT,
    LE,
    GT,
    GE,
    EQ,
    NE,
    AND,
    OR,
    XOR,
    NOT,
    GLOBAL_FUNCTIONS,
} from './functions-store.js';

export {
    type EnvelopeParameter,
    knownParameter,
    namedParameter,
    parameterName,
    parameterNamedName,
    parameterUntaggedCbor,
    parameterTaggedCbor,
    parameterFromUntaggedCbor,
    parameterFromTaggedCbor,
} from './parameter.js';

export {
    ParametersStore,
    BLANK,
    LHS,
    RHS,
    GLOBAL_PARAMETERS,
} from './parameters-store.js';

export { asEnvelope, type EnvelopeEncodable } from './envelope-encodable.js';

export { Expression } from './expression.js';
export { Request } from './request.js';
export { Response } from './response.js';
export { Event } from './event.js';

export {
    type EnvelopeVisitor,
} from './envelope.js';

export {
    DigestDisplayFormat,
    type TreeFormatOpts,
    treeFormat,
    treeFormatOpt,
} from './envelope-tree.js';

export {
    MermaidOrientation,
    MermaidTheme,
    type MermaidFormatOpts,
    mermaidFormat,
    mermaidFormatOpt,
} from './envelope-mermaid.js';

export {
    diagnostic,
    diagnosticAnnotated,
} from './envelope-diagnostic.js';

export { envelopeHex } from './envelope-hex.js';

export {
    addType,
    types,
    getType,
    hasType,
    hasTypeValue,
    checkType,
    checkTypeValue,
} from './envelope-types.js';

export {
    SignatureMetadata,
    addSignature,
    addSignatureOpt,
    addSignatures,
    hasSignatureFrom,
    verifySignature,
    verifySignatureFrom,
    verifySignatureFromReturningMetadata,
    hasSignaturesFromThreshold,
    hasSignaturesFrom,
    verifySignaturesFromThreshold,
    verifySignaturesFrom,
    sign,
    signOpt,
    verify,
    verifyReturningMetadata,
} from './envelope-signature.js';

export {
    addRecipient,
    addRecipientOpt,
    recipients,
    encryptSubjectToRecipients,
    encryptSubjectToRecipientsOpt,
    encryptSubjectToRecipient,
    encryptSubjectToRecipientOpt,
    decryptSubjectToRecipient,
    encryptToRecipient,
    decryptToRecipient,
} from './envelope-recipient.js';

export {
    lockSubject,
    lockSubjectWithPassword,
    unlockSubject,
    unlockSubjectWithPassword,
    isLockedWithPassword,
    isLockedWithSshAgent,
    addSecret,
    addSecretWithPassword,
    lock,
    lockWithPassword,
    unlock,
    unlockWithPassword,
} from './envelope-secret.js';

export {
    sskrSplit,
    sskrSplitFlattened,
    sskrSplitUsing,
    sskrJoin,
} from './envelope-sskr.js';

export {
    proofContainsSet,
    proofContainsTarget,
    confirmContainsSet,
    confirmContainsTarget,
} from './envelope-proof.js';

export {
    newAttachment,
    addAttachment,
    attachmentPayload,
    attachmentVendor,
    attachmentConformsTo,
    attachments,
    attachmentsWithVendorAndConformsTo,
    attachmentWithVendorAndConformsTo,
    validateAttachment,
    Attachments,
    type Attachable,
} from './envelope-attachment.js';

export {
    addEdgeEnvelope,
    edges,
    validateEdge,
    edgeIsA,
    edgeSource,
    edgeTarget,
    edgeSubject,
    edgesMatching,
    Edges,
    type Edgeable,
} from './envelope-edge.js';

export {
    seal,
    sealOpt,
    unseal,
} from './envelope-seal.js';

// Side-effect imports install prototype extensions on Envelope.
import './envelope-types.js';
import './envelope-signature.js';
import './envelope-recipient.js';
import './envelope-secret.js';
import './envelope-sskr.js';
import './envelope-proof.js';
import './envelope-attachment.js';
import './envelope-edge.js';
import './envelope-seal.js';
