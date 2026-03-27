namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Walk/traversal operations for Gordian Envelopes.
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Walks the envelope structure, calling the visitor function for each element.
    /// </summary>
    /// <typeparam name="TState">The type of state passed between visitor calls.</typeparam>
    /// <param name="hideNodes">
    /// If <c>true</c>, skips node containers (tree-based traversal).
    /// If <c>false</c>, visits every element (structure-based traversal).
    /// </param>
    /// <param name="state">The initial state value.</param>
    /// <param name="visit">
    /// A visitor function that receives the current envelope, depth level,
    /// edge type, and state. Returns a tuple of (newState, stop). If stop
    /// is <c>true</c>, traversal halts for the current branch.
    /// </param>
    public void Walk<TState>(
        bool hideNodes,
        TState state,
        Func<Envelope, int, EdgeType, TState, (TState State, bool Stop)> visit)
    {
        if (hideNodes)
            WalkTree(0, EdgeType.None, state, visit);
        else
            WalkStructure(0, EdgeType.None, state, visit);
    }

    /// <summary>
    /// Recursive structure-based traversal that visits every element.
    /// </summary>
    private void WalkStructure<TState>(
        int level,
        EdgeType incomingEdge,
        TState state,
        Func<Envelope, int, EdgeType, TState, (TState State, bool Stop)> visit)
    {
        var (nextState, stop) = visit(this, level, incomingEdge, state);
        if (stop) return;

        var nextLevel = level + 1;

        switch (Case)
        {
            case EnvelopeCase.NodeCase node:
                node.Subject.WalkStructure(nextLevel, EdgeType.Subject, nextState, visit);
                foreach (var assertion in node.Assertions)
                {
                    assertion.WalkStructure(nextLevel, EdgeType.Assertion, nextState, visit);
                }
                break;

            case EnvelopeCase.WrappedCase wrapped:
                wrapped.Envelope.WalkStructure(nextLevel, EdgeType.Content, nextState, visit);
                break;

            case EnvelopeCase.AssertionCase assertionCase:
                assertionCase.Assertion.Predicate.WalkStructure(
                    nextLevel, EdgeType.Predicate, nextState, visit);
                assertionCase.Assertion.Object.WalkStructure(
                    nextLevel, EdgeType.Object, nextState, visit);
                break;
        }
    }

    /// <summary>
    /// Recursive tree-based traversal that skips node containers.
    /// </summary>
    private TState WalkTree<TState>(
        int level,
        EdgeType incomingEdge,
        TState state,
        Func<Envelope, int, EdgeType, TState, (TState State, bool Stop)> visit)
    {
        var currentState = state;
        var subjectLevel = level;

        if (!IsNode)
        {
            var (nextState, stop) = visit(this, level, incomingEdge, currentState);
            if (stop) return nextState;
            currentState = nextState;
            subjectLevel = level + 1;
        }

        switch (Case)
        {
            case EnvelopeCase.NodeCase node:
            {
                var assertionState = node.Subject.WalkTree(
                    subjectLevel, EdgeType.Subject, currentState, visit);
                var assertionLevel = subjectLevel + 1;
                foreach (var assertion in node.Assertions)
                {
                    assertion.WalkTree(assertionLevel, EdgeType.Assertion, assertionState, visit);
                }
                break;
            }

            case EnvelopeCase.WrappedCase wrapped:
                wrapped.Envelope.WalkTree(subjectLevel, EdgeType.Content, currentState, visit);
                break;

            case EnvelopeCase.AssertionCase assertionCase:
                assertionCase.Assertion.Predicate.WalkTree(
                    subjectLevel, EdgeType.Predicate, currentState, visit);
                assertionCase.Assertion.Object.WalkTree(
                    subjectLevel, EdgeType.Object, currentState, visit);
                break;
        }

        return currentState;
    }
}
