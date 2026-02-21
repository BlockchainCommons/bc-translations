package dcbor

import "fmt"

// WalkElement represents either a single element or map key/value semantic pair.
type WalkElement struct {
	single   *CBOR
	key      *CBOR
	value    *CBOR
	keyValue bool
}

func walkSingle(cbor CBOR) WalkElement {
	clone := cbor.Clone()
	return WalkElement{single: &clone}
}

func walkKeyValue(key CBOR, value CBOR) WalkElement {
	k := key.Clone()
	v := value.Clone()
	return WalkElement{key: &k, value: &v, keyValue: true}
}

func (w WalkElement) AsSingle() (CBOR, bool) {
	if w.single == nil {
		return CBOR{}, false
	}
	return w.single.Clone(), true
}

func (w WalkElement) AsKeyValue() (CBOR, CBOR, bool) {
	if !w.keyValue || w.key == nil || w.value == nil {
		return CBOR{}, CBOR{}, false
	}
	return w.key.Clone(), w.value.Clone(), true
}

func (w WalkElement) DiagnosticFlat() string {
	if single, ok := w.AsSingle(); ok {
		return single.DiagnosticFlat()
	}
	key, value, ok := w.AsKeyValue()
	if !ok {
		return ""
	}
	return fmt.Sprintf("%s: %s", key.DiagnosticFlat(), value.DiagnosticFlat())
}

// EdgeKind identifies parent-child relationship during walk traversal.
type EdgeKind int

const (
	EdgeKindNone EdgeKind = iota
	EdgeKindArrayElement
	EdgeKindMapKeyValue
	EdgeKindMapKey
	EdgeKindMapValue
	EdgeKindTaggedContent
)

// EdgeType describes a walk edge.
type EdgeType struct {
	Kind  EdgeKind
	Index int
}

func EdgeNone() EdgeType {
	return EdgeType{Kind: EdgeKindNone}
}

func EdgeArrayElement(index int) EdgeType {
	return EdgeType{Kind: EdgeKindArrayElement, Index: index}
}

func EdgeMapKeyValue() EdgeType {
	return EdgeType{Kind: EdgeKindMapKeyValue}
}

func EdgeMapKey() EdgeType {
	return EdgeType{Kind: EdgeKindMapKey}
}

func EdgeMapValue() EdgeType {
	return EdgeType{Kind: EdgeKindMapValue}
}

func EdgeTaggedContent() EdgeType {
	return EdgeType{Kind: EdgeKindTaggedContent}
}

func (e EdgeType) Label() (string, bool) {
	switch e.Kind {
	case EdgeKindArrayElement:
		return fmt.Sprintf("arr[%d]", e.Index), true
	case EdgeKindMapKeyValue:
		return "kv", true
	case EdgeKindMapKey:
		return "key", true
	case EdgeKindMapValue:
		return "val", true
	case EdgeKindTaggedContent:
		return "content", true
	default:
		return "", false
	}
}

// Visitor is the callback type used by Walk.
type Visitor func(element WalkElement, level int, incomingEdge EdgeType, state any) (any, bool)

// Walk traverses the CBOR tree depth-first.
func (c CBOR) Walk(state any, visit Visitor) {
	c.walk(0, EdgeNone(), state, visit)
}

func (c CBOR) walk(level int, incomingEdge EdgeType, state any, visit Visitor) {
	nextState, stop := visit(walkSingle(c), level, incomingEdge, state)
	if stop {
		return
	}

	switch c.kind {
	case CBORKindArray:
		items := c.value.([]CBOR)
		for i, item := range items {
			item.walk(level+1, EdgeArrayElement(i), nextState, visit)
		}
	case CBORKindMap:
		iter := c.value.(Map).Iter()
		for {
			key, value, ok := iter.Next()
			if !ok {
				break
			}
			childState, skipChildren := visit(walkKeyValue(key, value), level+1, EdgeMapKeyValue(), nextState)
			if skipChildren {
				continue
			}
			key.walk(level+1, EdgeMapKey(), childState, visit)
			value.walk(level+1, EdgeMapValue(), childState, visit)
		}
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		tagged.Value.walk(level+1, EdgeTaggedContent(), nextState, visit)
	}
}
