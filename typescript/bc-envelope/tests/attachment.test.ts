import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';

describe('attachment tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('add and retrieve attachment', () => {
        const envelope = Envelope.from('main')
            .addAttachment('payload', 'vendor.example', 'schema:demo');

        const attachment = envelope.attachmentWithVendorAndConformsTo('vendor.example', 'schema:demo');
        expect(attachment.attachmentVendor()).toBe('vendor.example');
        expect(attachment.attachmentConformsTo()).toBe('schema:demo');
        expect(attachment.attachmentPayload().extractSubject<string>()).toBe('payload');
    });

    test('attachment filters', () => {
        const envelope = Envelope
            .from('main')
            .addAttachment('a', 'v1')
            .addAttachment('b', 'v2');

        expect(envelope.attachments().length).toBe(2);
        expect(envelope.attachmentsWithVendorAndConformsTo('v1').length).toBe(1);
    });
});
