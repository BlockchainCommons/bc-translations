"""Expression tests for bc-envelope.

Translated from rust/bc-envelope/src/extension/expressions/expression.rs
"""

from textwrap import dedent

from bc_envelope import Expression, functions, parameters


def test_expression_1():
    expression = (
        Expression(functions.ADD)
        .with_parameter(parameters.LHS, 2)
        .with_parameter(parameters.RHS, 3)
    )

    envelope = expression.to_envelope()

    expected = dedent("""\
        \u00abadd\u00bb [
            \u2770lhs\u2771: 2
            \u2770rhs\u2771: 3
        ]""")
    assert envelope.format() == expected

    parsed_expression = Expression.from_envelope(envelope)

    assert parsed_expression.extract_object_for_parameter(parameters.LHS) == 2
    assert parsed_expression.extract_object_for_parameter(parameters.RHS) == 3

    assert parsed_expression.function == expression.function
    assert (
        parsed_expression.expression_envelope.digest()
        == expression.expression_envelope.digest()
    )
    assert expression == parsed_expression


def test_expression_2():
    expression = (
        Expression("foo")
        .with_parameter("bar", "baz")
        .with_optional_parameter("qux", None)
    )

    envelope = expression.to_envelope()

    expected = dedent("""\
        \u00ab"foo"\u00bb [
            \u2770"bar"\u2771: "baz"
        ]""")
    assert envelope.format() == expected

    parsed_expression = Expression.from_envelope(envelope)

    assert parsed_expression.extract_object_for_parameter("bar") == "baz"
    assert parsed_expression.extract_optional_object_for_parameter("qux") is None

    assert parsed_expression.function == expression.function
    assert (
        parsed_expression.expression_envelope.digest()
        == expression.expression_envelope.digest()
    )
    assert expression == parsed_expression
