package bcenvelope

// EdgeType identifies how an envelope element is connected to its parent during traversal.
type EdgeType int

const (
	// EdgeNone indicates no incoming edge (root element).
	EdgeNone EdgeType = iota
	// EdgeSubject indicates the element is a subject of a node.
	EdgeSubject
	// EdgeAssertion indicates the element is an assertion of a node.
	EdgeAssertion
	// EdgePredicate indicates the element is a predicate of an assertion.
	EdgePredicate
	// EdgeObject indicates the element is an object of an assertion.
	EdgeObject
	// EdgeContent indicates the element is the content of a wrapped envelope.
	EdgeContent
)

// Label returns a short text label for the edge type, or empty string if none.
func (e EdgeType) Label() string {
	switch e {
	case EdgeSubject:
		return "subj"
	case EdgeContent:
		return "cont"
	case EdgePredicate:
		return "pred"
	case EdgeObject:
		return "obj"
	default:
		return ""
	}
}

// Visitor is a function called for each element during an envelope walk.
// It receives the envelope, nesting level, incoming edge type, and current state.
// It returns an updated state and a bool indicating whether to stop the walk.
type Visitor[State any] func(envelope *Envelope, level int, incomingEdge EdgeType, state State) (State, bool)

// Walk traverses the envelope structure, calling the visitor for each element.
// If hideNodes is true, node containers are skipped (tree-based traversal).
func (e *Envelope) Walk(hideNodes bool, state any, visit any) {
	if v, ok := visit.(func(*Envelope, int, EdgeType, struct{}) (struct{}, bool)); ok {
		if hideNodes {
			e.walkTreeTyped(0, EdgeNone, struct{}{}, v)
		} else {
			e.walkStructureTyped(0, EdgeNone, struct{}{}, v)
		}
		return
	}
	if v, ok := visit.(func(*Envelope, int, EdgeType, any) (any, bool)); ok {
		WalkGeneric(e, hideNodes, state, v)
		return
	}
	panic("Walk: unsupported visitor type")
}

// WalkGeneric traverses the envelope structure with a typed visitor function.
func WalkGeneric[S any](e *Envelope, hideNodes bool, state S, visit Visitor[S]) {
	if hideNodes {
		walkTreeGeneric(e, 0, EdgeNone, state, visit)
	} else {
		walkStructureGeneric(e, 0, EdgeNone, state, visit)
	}
}

func (e *Envelope) walkStructureTyped(level int, incomingEdge EdgeType, state struct{}, visit func(*Envelope, int, EdgeType, struct{}) (struct{}, bool)) {
	state, stop := visit(e, level, incomingEdge, state)
	if stop {
		return
	}
	nextLevel := level + 1
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		c.Subject.walkStructureTyped(nextLevel, EdgeSubject, state, visit)
		for _, assertion := range c.Assertions {
			assertion.walkStructureTyped(nextLevel, EdgeAssertion, state, visit)
		}
	case *WrappedCase:
		c.Envelope.walkStructureTyped(nextLevel, EdgeContent, state, visit)
	case *AssertionCase:
		c.Assertion.Predicate().walkStructureTyped(nextLevel, EdgePredicate, state, visit)
		c.Assertion.Object().walkStructureTyped(nextLevel, EdgeObject, state, visit)
	}
}

func (e *Envelope) walkTreeTyped(level int, incomingEdge EdgeType, state struct{}, visit func(*Envelope, int, EdgeType, struct{}) (struct{}, bool)) struct{} {
	subjectLevel := level
	if !e.IsNode() {
		var stop bool
		state, stop = visit(e, level, incomingEdge, state)
		if stop {
			return state
		}
		subjectLevel = level + 1
	}
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		assertionState := c.Subject.walkTreeTyped(subjectLevel, EdgeSubject, state, visit)
		assertionLevel := subjectLevel + 1
		for _, assertion := range c.Assertions {
			assertion.walkTreeTyped(assertionLevel, EdgeAssertion, assertionState, visit)
		}
	case *WrappedCase:
		c.Envelope.walkTreeTyped(subjectLevel, EdgeContent, state, visit)
	case *AssertionCase:
		c.Assertion.Predicate().walkTreeTyped(subjectLevel, EdgePredicate, state, visit)
		c.Assertion.Object().walkTreeTyped(subjectLevel, EdgeObject, state, visit)
	}
	return state
}

// --- Generic walk implementations ---

func walkStructureGeneric[S any](e *Envelope, level int, incomingEdge EdgeType, state S, visit Visitor[S]) {
	state, stop := visit(e, level, incomingEdge, state)
	if stop {
		return
	}
	nextLevel := level + 1
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		walkStructureGeneric(c.Subject, nextLevel, EdgeSubject, state, visit)
		for _, assertion := range c.Assertions {
			walkStructureGeneric(assertion, nextLevel, EdgeAssertion, state, visit)
		}
	case *WrappedCase:
		walkStructureGeneric(c.Envelope, nextLevel, EdgeContent, state, visit)
	case *AssertionCase:
		walkStructureGeneric(c.Assertion.Predicate(), nextLevel, EdgePredicate, state, visit)
		walkStructureGeneric(c.Assertion.Object(), nextLevel, EdgeObject, state, visit)
	}
}

func walkTreeGeneric[S any](e *Envelope, level int, incomingEdge EdgeType, state S, visit Visitor[S]) S {
	subjectLevel := level
	if !e.IsNode() {
		var stop bool
		state, stop = visit(e, level, incomingEdge, state)
		if stop {
			return state
		}
		subjectLevel = level + 1
	}
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		assertionState := walkTreeGeneric(c.Subject, subjectLevel, EdgeSubject, state, visit)
		assertionLevel := subjectLevel + 1
		for _, assertion := range c.Assertions {
			walkTreeGeneric(assertion, assertionLevel, EdgeAssertion, assertionState, visit)
		}
	case *WrappedCase:
		walkTreeGeneric(c.Envelope, subjectLevel, EdgeContent, state, visit)
	case *AssertionCase:
		walkTreeGeneric(c.Assertion.Predicate(), subjectLevel, EdgePredicate, state, visit)
		walkTreeGeneric(c.Assertion.Object(), subjectLevel, EdgeObject, state, visit)
	}
	return state
}
