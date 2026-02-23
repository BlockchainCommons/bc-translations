@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.ARID
// registerTags() from bc-envelope package (not bc-components) is used
// to initialize the GlobalFormatContext with functions/parameters stores
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.knownvalues.KnownValue
import com.blockchaincommons.knownvalues.OK_VALUE
import com.blockchaincommons.knownvalues.UNKNOWN_VALUE
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue

class ExpressionTest {

    private fun requestId(): ARID = ARID.fromData(
        "c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc".hexToByteArray()
    )

    // -- Expression tests --

    @Test
    fun testExpression1() {
        registerTags()

        val expression = Expression(ADD)
            .withParameter(LHS, 2)
            .withParameter(RHS, 3)

        val envelope = expression.toEnvelope()

        val expected = """
            «add» [
                ❰lhs❱: 2
                ❰rhs❱: 3
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedExpression = Expression.fromEnvelope(envelope)

        assertEquals(
            2,
            parsedExpression.extractObjectForParameter<Int>(LHS)
        )
        assertEquals(
            3,
            parsedExpression.extractObjectForParameter<Int>(RHS)
        )

        assertEquals(parsedExpression.function(), expression.function())
        assertEquals(
            parsedExpression.expressionEnvelope(),
            expression.expressionEnvelope()
        )
        assertEquals(expression, parsedExpression)
    }

    @Test
    fun testExpression2() {
        registerTags()

        val expression = Expression("foo")
            .withParameter(Parameter.Named("bar"), "baz")
            .withOptionalParameter(Parameter.Named("qux"), null)

        val envelope = expression.toEnvelope()

        val expected = """
            «"foo"» [
                ❰"bar"❱: "baz"
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedExpression = Expression.fromEnvelope(envelope)

        assertEquals(
            "baz",
            parsedExpression.extractObjectForParameter<String>(Parameter.Named("bar"))
        )
        assertNull(
            parsedExpression.extractOptionalObjectForParameter<Int>(Parameter.Named("qux"))
        )

        assertEquals(parsedExpression.function(), expression.function())
        assertEquals(
            parsedExpression.expressionEnvelope(),
            expression.expressionEnvelope()
        )
        assertEquals(expression, parsedExpression)
    }

    // -- Request tests --

    @Test
    fun testBasicRequest() {
        registerTags()

        val request = Request("test", requestId())
            .withParameter(Parameter.Named("param1"), 42)
            .withParameter(Parameter.Named("param2"), "hello")

        val envelope = request.toEnvelope()
        val expected = """
            request(ARID(c66be27d)) [
                'body': «"test"» [
                    ❰"param1"❱: 42
                    ❰"param2"❱: "hello"
                ]
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedRequest = Request.fromEnvelope(envelope)
        assertEquals(
            42,
            parsedRequest.extractObjectForParameter<Int>(Parameter.Named("param1"))
        )
        assertEquals(
            "hello",
            parsedRequest.extractObjectForParameter<String>(Parameter.Named("param2"))
        )
        assertEquals("", parsedRequest.note())
        assertNull(parsedRequest.date())

        assertEquals(request, parsedRequest)
    }

    @Test
    fun testRequestWithMetadata() {
        registerTags()

        val requestDate = CborDate.fromString("2024-07-04T11:11:11Z")
        val request = Request("test", requestId())
            .withParameter(Parameter.Named("param1"), 42)
            .withParameter(Parameter.Named("param2"), "hello")
            .withNote("This is a test")
            .withDate(requestDate)

        val envelope = request.toEnvelope()
        val expected = """
            request(ARID(c66be27d)) [
                'body': «"test"» [
                    ❰"param1"❱: 42
                    ❰"param2"❱: "hello"
                ]
                'date': 2024-07-04T11:11:11Z
                'note': "This is a test"
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedRequest = Request.fromEnvelope(envelope)
        assertEquals(
            42,
            parsedRequest.extractObjectForParameter<Int>(Parameter.Named("param1"))
        )
        assertEquals(
            "hello",
            parsedRequest.extractObjectForParameter<String>(Parameter.Named("param2"))
        )
        assertEquals("This is a test", parsedRequest.note())
        assertEquals(requestDate, parsedRequest.date())

        assertEquals(request, parsedRequest)
    }

    @Test
    fun testParameterFormat() {
        registerTags()

        val parameter = Parameter.Named("testParam")
        val envelope = parameter.toEnvelope()
        val expected = """❰"testParam"❱"""
        assertEquals(expected, envelope.format())
    }

    // -- Response tests --

    @Test
    fun testSuccessOk() {
        registerTags()

        val response = Response.newSuccess(requestId())
        val envelope = response.toEnvelope()

        val expected = """
            response(ARID(c66be27d)) [
                'result': 'OK'
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedResponse = Response.fromEnvelope(envelope)
        assertTrue(parsedResponse.isSuccess())
        assertEquals(requestId(), parsedResponse.expectId())
        assertEquals(
            OK_VALUE,
            parsedResponse.extractResult<KnownValue>()
        )
        assertEquals(response, parsedResponse)
    }

    @Test
    fun testSuccessResult() {
        registerTags()

        val response = Response.newSuccess(requestId())
            .withResult("It works!")
        val envelope = response.toEnvelope()

        val expected = """
            response(ARID(c66be27d)) [
                'result': "It works!"
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedResponse = Response.fromEnvelope(envelope)
        assertTrue(parsedResponse.isSuccess())
        assertEquals(requestId(), parsedResponse.expectId())
        assertEquals("It works!", parsedResponse.extractResult<String>())
        assertEquals(response, parsedResponse)
    }

    @Test
    fun testEarlyFailure() {
        registerTags()

        val response = Response.newEarlyFailure()
        val envelope = response.toEnvelope()

        val expected = """
            response('Unknown') [
                'error': 'Unknown'
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedResponse = Response.fromEnvelope(envelope)
        assertTrue(parsedResponse.isFailure())
        assertNull(parsedResponse.id())
        assertEquals(
            UNKNOWN_VALUE,
            parsedResponse.extractError<KnownValue>()
        )
        assertEquals(response, parsedResponse)
    }

    @Test
    fun testFailure() {
        registerTags()

        val response = Response.newFailure(requestId())
            .withError("It doesn't work!")
        val envelope = response.toEnvelope()

        val expected = """
            response(ARID(c66be27d)) [
                'error': "It doesn't work!"
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedResponse = Response.fromEnvelope(envelope)
        assertTrue(parsedResponse.isFailure())
        assertEquals(requestId(), parsedResponse.id())
        assertEquals("It doesn't work!", parsedResponse.extractError<String>())
        assertEquals(response, parsedResponse)
    }

    // -- Event test --

    @Test
    fun testEvent() {
        registerTags()

        val eventDate = CborDate.fromString("2024-07-04T11:11:11Z")
        val event = Event.ofString("test", requestId())
            .withNote("This is a test")
            .withDate(eventDate)

        val envelope = event.toEnvelope()
        val expected = """
            event(ARID(c66be27d)) [
                'content': "test"
                'date': 2024-07-04T11:11:11Z
                'note': "This is a test"
            ]
        """.trimIndent()
        assertEquals(expected, envelope.format())

        val parsedEvent = Event.stringFromEnvelope(envelope)
        assertEquals("test", parsedEvent.content())
        assertEquals("This is a test", parsedEvent.note())
        assertEquals(eventDate, parsedEvent.date())
    }
}
