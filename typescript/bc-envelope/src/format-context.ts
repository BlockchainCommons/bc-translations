import {
    type Cbor,
    type SummarizerResult,
    type Tag,
    TagsStore,
    type TagsStoreTrait,
    withTagsMut,
} from '@bc/dcbor';
import {
    TAG_EVENT,
    TAG_FUNCTION,
    TAG_KNOWN_VALUE,
    TAG_PARAMETER,
    TAG_REQUEST,
    TAG_RESPONSE,
    registerTagsIn as registerBCTagsIn,
} from '@bc/tags';
import { KNOWN_VALUES, type KnownValuesStore, KnownValue } from '@bc/known-values';
import {
    registerTags as registerComponentTags,
    registerTagsIn as registerComponentTagsIn,
} from '@bc/components';

import { flankedBy } from './string-utils.js';
import { GLOBAL_FUNCTIONS, FunctionsStore } from './functions-store.js';
import { GLOBAL_PARAMETERS, ParametersStore } from './parameters-store.js';
import {
    functionFromUntaggedCbor,
} from './function.js';
import {
    parameterFromUntaggedCbor,
} from './parameter.js';
import { Envelope } from './envelope.js';

export type FormatContextOpt =
    | { type: 'none' }
    | { type: 'global' }
    | { type: 'custom'; context: FormatContext };

export const FormatContextOpts = {
    none(): FormatContextOpt {
        return { type: 'none' };
    },
    global(): FormatContextOpt {
        return { type: 'global' };
    },
    custom(context: FormatContext): FormatContextOpt {
        return { type: 'custom', context };
    },
};

export class FormatContext implements TagsStoreTrait {
    readonly #tags: TagsStore;
    readonly #knownValues: KnownValuesStore;
    readonly #functions: FunctionsStore;
    readonly #parameters: ParametersStore;

    constructor(
        tags = new TagsStore(),
        knownValues: KnownValuesStore = KNOWN_VALUES,
        functions = GLOBAL_FUNCTIONS,
        parameters = GLOBAL_PARAMETERS,
    ) {
        this.#tags = tags;
        this.#knownValues = knownValues;
        this.#functions = functions;
        this.#parameters = parameters;
    }

    tags(): TagsStore {
        return this.#tags;
    }

    knownValues(): KnownValuesStore {
        return this.#knownValues;
    }

    functions(): FunctionsStore {
        return this.#functions;
    }

    parameters(): ParametersStore {
        return this.#parameters;
    }

    assignedNameForTag(tag: Tag): string | undefined {
        return this.#tags.assignedNameForTag(tag);
    }

    nameForTag(tag: Tag): string {
        return this.#tags.nameForTag(tag);
    }

    tagForName(name: string): Tag | undefined {
        return this.#tags.tagForName(name);
    }

    tagForValue(value: bigint): Tag | undefined {
        return this.#tags.tagForValue(value);
    }

    summarizer(tag: bigint) {
        return this.#tags.summarizer(tag);
    }

    nameForValue(value: bigint): string {
        return this.#tags.nameForValue(value);
    }
}

let globalContext: FormatContext | undefined;

export function getGlobalFormatContext(): FormatContext {
    if (globalContext === undefined) {
        // Ensure the global dCBOR tag store contains named bc tags for UR encoding.
        registerComponentTags();

        const tags = new TagsStore();
        const context = new FormatContext(tags, KNOWN_VALUES, GLOBAL_FUNCTIONS, GLOBAL_PARAMETERS);
        registerTagsIn(context);
        globalContext = context;
    }
    return globalContext;
}

export function withFormatContext<T>(action: (context: FormatContext) => T): T {
    return action(getGlobalFormatContext());
}

export function registerTagsIn(context: FormatContext): void {
    registerBCTagsIn(context.tags());
    registerComponentTagsIn(context.tags());

    const ok = (value: string): SummarizerResult => ({ ok: true, value });

    const knownValues = context.knownValues();
    context.tags().setSummarizer(TAG_KNOWN_VALUE, (untaggedCbor) => {
        const kv = KnownValue.fromUntaggedCbor(untaggedCbor);
        return ok(flankedBy(knownValues.name(kv), "'", "'"));
    });

    const functions = context.functions();
    context.tags().setSummarizer(TAG_FUNCTION, (untaggedCbor) => {
        const fn = functionFromUntaggedCbor(untaggedCbor);
        return ok(flankedBy(FunctionsStore.nameForFunction(fn, functions), '\u00AB', '\u00BB'));
    });

    const parameters = context.parameters();
    context.tags().setSummarizer(TAG_PARAMETER, (untaggedCbor) => {
        const param = parameterFromUntaggedCbor(untaggedCbor);
        return ok(flankedBy(ParametersStore.nameForParameter(param, parameters), '\u2770', '\u2771'));
    });

    const summarizeEnvelope = (untaggedCbor: Cbor, flat: boolean, wrapper: string): SummarizerResult => {
        const envelope = Envelope.newLeaf(untaggedCbor);
        const formatted = envelope.formatOpt(flat, FormatContextOpts.custom(context));
        return ok(`${wrapper}(${formatted})`);
    };

    context.tags().setSummarizer(TAG_REQUEST, (untaggedCbor, flat) => summarizeEnvelope(untaggedCbor, flat, 'request'));
    context.tags().setSummarizer(TAG_RESPONSE, (untaggedCbor, flat) => summarizeEnvelope(untaggedCbor, flat, 'response'));
    context.tags().setSummarizer(TAG_EVENT, (untaggedCbor, flat) => summarizeEnvelope(untaggedCbor, flat, 'event'));
}

export function registerTags(): void {
    withTagsMut((tagsStore) => {
        registerBCTagsIn(tagsStore);
        registerComponentTagsIn(tagsStore);
    });
    getGlobalFormatContext();
}
