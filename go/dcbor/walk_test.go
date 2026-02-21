package dcbor

import "testing"

func TestWalkTraversalCount(t *testing.T) {
	one, _ := FromAny(1)
	two, _ := FromAny(2)
	three, _ := FromAny(3)

	array := NewCBORArray([]CBOR{two, three})
	keyA, _ := FromAny("a")
	keyB, _ := FromAny("b")

	m := NewMap()
	m.Insert(keyA, one)
	m.Insert(keyB, array)
	root := NewCBORMap(m)

	count := 0
	root.Walk(nil, func(_ WalkElement, _ int, _ EdgeType, state any) (any, bool) {
		count++
		return state, false
	})

	if count != 9 {
		t.Fatalf("unexpected walk count: got %d want %d", count, 9)
	}
}

func TestWalkStopPreventsDescent(t *testing.T) {
	child1, _ := FromAny("child-1")
	child2, _ := FromAny("child-2")
	array := NewCBORArray([]CBOR{child1, child2})
	root := NewCBORArray([]CBOR{array})

	visited := make([]string, 0)
	root.Walk(nil, func(element WalkElement, level int, edge EdgeType, state any) (any, bool) {
		if single, ok := element.AsSingle(); ok {
			visited = append(visited, single.DiagnosticFlat())
			if level == 1 && edge.Kind == EdgeKindArrayElement {
				return state, true
			}
		}
		return state, false
	})

	if len(visited) != 2 {
		t.Fatalf("expected 2 visited elements, got %d", len(visited))
	}
	if visited[1] != "[\"child-1\", \"child-2\"]" {
		t.Fatalf("unexpected second visit: %s", visited[1])
	}
}

func TestEdgeLabels(t *testing.T) {
	if label, ok := EdgeArrayElement(2).Label(); !ok || label != "arr[2]" {
		t.Fatalf("unexpected array edge label: %q", label)
	}
	if _, ok := EdgeNone().Label(); ok {
		t.Fatalf("expected no label for EdgeNone")
	}
}
