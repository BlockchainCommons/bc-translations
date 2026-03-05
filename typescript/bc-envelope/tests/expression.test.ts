import { ARID } from '@bc/components';
import { CborDate, hexToBytes } from '@bc/dcbor';
import { OK_VALUE, UNKNOWN_VALUE } from '@bc/known-values';
import { beforeAll, describe, expect, test } from 'vitest';

import {
    ADD,
    Event,
    Expression,
    LHS,
    Request,
    Response,
    RHS,
    namedParameter,
    registerTags,
} from '../src/index.js';
import { expectActualExpected } from './test-helpers.js';

function requestId(): ARID {
    return ARID.fromData(
        hexToBytes('c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc'),
    );
}

describe('expression tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('expression add with lhs and rhs', () => {
        const expression = new Expression(ADD)
            .withParameter(LHS, 2)
            .withParameter(RHS, 3);

        const envelope = expression.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            «add» [
                ❰lhs❱: 2
                ❰rhs❱: 3
            ]
            `,
        );

        const parsedExpression = Expression.fromEnvelope(envelope);
        expect(parsedExpression.extractObjectForParameter<number>(LHS)).toBe(2);
        expect(parsedExpression.extractObjectForParameter<number>(RHS)).toBe(3);
        expect(parsedExpression.expressionEnvelope().isEquivalentTo(expression.expressionEnvelope())).toBe(true);
        expect(parsedExpression.equals(expression)).toBe(true);
    });

    test('expression with named parameters', () => {
        const bar = namedParameter('bar');
        const qux = namedParameter('qux');
        const expression = new Expression('foo')
            .withParameter(bar, 'baz')
            .withOptionalParameter(qux, undefined);

        const envelope = expression.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            «"foo"» [
                ❰"bar"❱: "baz"
            ]
            `,
        );

        const parsedExpression = Expression.fromEnvelope(envelope);
        expect(parsedExpression.extractObjectForParameter<string>(bar)).toBe('baz');
        expect(parsedExpression.extractOptionalObjectForParameter<string>(qux)).toBeUndefined();
        expect(parsedExpression.expressionEnvelope().isEquivalentTo(expression.expressionEnvelope())).toBe(true);
        expect(parsedExpression.equals(expression)).toBe(true);
    });

    test('request basic', () => {
        const param1 = namedParameter('param1');
        const param2 = namedParameter('param2');
        const request = new Request('test', requestId())
            .withParameter(param1, 42)
            .withParameter(param2, 'hello');

        const envelope = request.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            request(ARID(c66be27d)) [
                'body': «"test"» [
                    ❰"param1"❱: 42
                    ❰"param2"❱: "hello"
                ]
            ]
            `,
        );

        const parsedRequest = Request.fromEnvelope(envelope);
        expect(parsedRequest.extractObjectForParameter<number>(param1)).toBe(42);
        expect(parsedRequest.extractObjectForParameter<string>(param2)).toBe('hello');
        expect(parsedRequest.note()).toBe('');
        expect(parsedRequest.date()).toBeUndefined();
        expect(parsedRequest.equals(request)).toBe(true);
    });

    test('request with metadata', () => {
        const param1 = namedParameter('param1');
        const param2 = namedParameter('param2');
        const requestDate = CborDate.fromString('2024-07-04T11:11:11Z');
        const request = new Request('test', requestId())
            .withParameter(param1, 42)
            .withParameter(param2, 'hello')
            .withNote('This is a test')
            .withDate(requestDate);

        const envelope = request.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            request(ARID(c66be27d)) [
                'body': «"test"» [
                    ❰"param1"❱: 42
                    ❰"param2"❱: "hello"
                ]
                'date': 2024-07-04T11:11:11Z
                'note': "This is a test"
            ]
            `,
        );

        const parsedRequest = Request.fromEnvelope(envelope);
        expect(parsedRequest.extractObjectForParameter<number>(param1)).toBe(42);
        expect(parsedRequest.extractObjectForParameter<string>(param2)).toBe('hello');
        expect(parsedRequest.note()).toBe('This is a test');
        expect(parsedRequest.date()?.equals(requestDate)).toBe(true);
        expect(parsedRequest.equals(request)).toBe(true);
    });

    test('response success ok', () => {
        const response = Response.newSuccess(requestId());
        const envelope = response.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            response(ARID(c66be27d)) [
                'result': 'OK'
            ]
            `,
        );

        const parsedResponse = Response.fromEnvelope(envelope);
        expect(parsedResponse.isSuccess()).toBe(true);
        expect(parsedResponse.expectId().equals(requestId())).toBe(true);
        expect(parsedResponse.extractResult()).toEqual(OK_VALUE);
        expect(parsedResponse.equals(response)).toBe(true);
    });

    test('response success custom result', () => {
        const response = Response.newSuccess(requestId()).withResult('It works!');
        const envelope = response.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            response(ARID(c66be27d)) [
                'result': "It works!"
            ]
            `,
        );

        const parsedResponse = Response.fromEnvelope(envelope);
        expect(parsedResponse.isSuccess()).toBe(true);
        expect(parsedResponse.expectId().equals(requestId())).toBe(true);
        expect(parsedResponse.extractResult<string>()).toBe('It works!');
        expect(parsedResponse.equals(response)).toBe(true);
    });

    test('response early failure', () => {
        const response = Response.newEarlyFailure();
        const envelope = response.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            response('Unknown') [
                'error': 'Unknown'
            ]
            `,
        );

        const parsedResponse = Response.fromEnvelope(envelope);
        expect(parsedResponse.isFailure()).toBe(true);
        expect(parsedResponse.id()).toBeUndefined();
        expect(parsedResponse.extractError()).toEqual(UNKNOWN_VALUE);
        expect(parsedResponse.equals(response)).toBe(true);
    });

    test('response failure with error', () => {
        const response = Response.newFailure(requestId()).withError("It doesn't work!");
        const envelope = response.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            response(ARID(c66be27d)) [
                'error': "It doesn't work!"
            ]
            `,
        );

        const parsedResponse = Response.fromEnvelope(envelope);
        expect(parsedResponse.isFailure()).toBe(true);
        expect(parsedResponse.id()?.equals(requestId())).toBe(true);
        expect(parsedResponse.extractError<string>()).toBe("It doesn't work!");
        expect(parsedResponse.equals(response)).toBe(true);
    });

    test('event with note and date', () => {
        const eventDate = CborDate.fromString('2024-07-04T11:11:11Z');
        const event = Event.ofString('test', requestId())
            .withNote('This is a test')
            .withDate(eventDate);

        const envelope = event.toEnvelope();

        // expected-text-output-rubric:
        expectActualExpected(
            envelope.format(),
            `
            event(ARID(c66be27d)) [
                'content': "test"
                'date': 2024-07-04T11:11:11Z
                'note': "This is a test"
            ]
            `,
        );

        const parsedEvent = Event.stringFromEnvelope(envelope);
        expect(parsedEvent.content()).toBe('test');
        expect(parsedEvent.note()).toBe('This is a test');
        expect(parsedEvent.date()?.equals(eventDate)).toBe(true);
    });
});
