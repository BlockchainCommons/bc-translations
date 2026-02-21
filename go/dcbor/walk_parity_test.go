package dcbor

import (
	"strings"
	"testing"
)

func arrayFromAny(values ...any) CBOR {
	items := make([]CBOR, 0, len(values))
	for _, value := range values {
		items = append(items, MustFromAny(value))
	}
	return NewCBORArray(items)
}

func countVisits(cbor CBOR) int {
	count := 0
	cbor.Walk(nil, func(_ WalkElement, _ int, _ EdgeType, state any) (any, bool) {
		count++
		return state, false
	})
	return count
}

func TestWalkTraversalCountsParity(t *testing.T) {
	array := arrayFromAny(1, 2, 3)
	if got, want := countVisits(array), 4; got != want {
		t.Fatalf("array visit count mismatch: got %d want %d", got, want)
	}

	m := NewMap()
	m.MustInsertAny("a", 1)
	m.MustInsertAny("b", 2)
	mapCBOR := NewCBORMap(m)
	if got, want := countVisits(mapCBOR), 7; got != want {
		t.Fatalf("map visit count mismatch: got %d want %d", got, want)
	}

	tagged := ToTaggedValue(TagWithValue(42), MustFromAny(100))
	if got, want := countVisits(tagged), 2; got != want {
		t.Fatalf("tagged visit count mismatch: got %d want %d", got, want)
	}

	innerMap := NewMap()
	innerMap.Insert(MustFromAny("x"), arrayFromAny(1, 2))
	outerMap := NewMap()
	outerMap.Insert(MustFromAny("inner"), NewCBORMap(innerMap))
	outerMap.MustInsertAny("simple", 42)
	nested := NewCBORMap(outerMap)

	if got, want := countVisits(nested), 12; got != want {
		t.Fatalf("nested visit count mismatch: got %d want %d", got, want)
	}
}

func TestWalkVisitorStateThreadingParity(t *testing.T) {
	array := arrayFromAny(1, 2, 3, 4, 5)
	evenCount := 0

	array.Walk(nil, func(element WalkElement, _ int, _ EdgeType, state any) (any, bool) {
		if single, ok := element.AsSingle(); ok {
			if value, ok := single.AsUnsigned(); ok && value%2 == 0 {
				evenCount++
			}
		}
		return state, false
	})

	if got, want := evenCount, 2; got != want {
		t.Fatalf("even count mismatch: got %d want %d", got, want)
	}
}

func TestWalkEarlyTerminationParity(t *testing.T) {
	nested := NewCBORArray([]CBOR{
		arrayFromAny("should", "see", "this"),
		MustFromAny("abort_marker"),
		arrayFromAny("should", "not", "see"),
	})

	type visit struct {
		level int
		text  string
	}
	visitLog := make([]visit, 0)
	foundAbort := false

	nested.Walk(nil, func(element WalkElement, level int, edge EdgeType, state any) (any, bool) {
		visitLog = append(visitLog, visit{level: level, text: element.DiagnosticFlat()})

		if single, ok := element.AsSingle(); ok {
			if text, ok := single.AsText(); ok && text == "abort_marker" {
				foundAbort = true
				return state, true
			}
		}

		stop := foundAbort && level == 1 && edge.Kind == EdgeKindArrayElement && edge.Index == 2
		return state, stop
	})

	lines := make([]string, 0, len(visitLog))
	for _, v := range visitLog {
		lines = append(lines, v.text)
	}
	logText := strings.Join(lines, "\n")
	if !strings.Contains(logText, "abort_marker") {
		t.Fatalf("expected to visit abort marker, got:\n%s", logText)
	}
	if !strings.Contains(logText, `["should", "not", "see"]`) {
		t.Fatalf("expected to visit third array itself, got:\n%s", logText)
	}

	thirdArrayIndex := -1
	for i, v := range visitLog {
		if v.level == 1 && v.text == `["should", "not", "see"]` {
			thirdArrayIndex = i
			break
		}
	}
	if thirdArrayIndex < 0 {
		t.Fatalf("could not find third array visit, got:\n%s", logText)
	}
	for _, v := range visitLog[thirdArrayIndex+1:] {
		if v.level == 2 {
			t.Fatalf("expected no level-2 visits after third array, got:\n%s", logText)
		}
	}
}

func TestWalkDepthLimitedTraversalParity(t *testing.T) {
	level3 := NewMap()
	level3.MustInsertAny("deep", "value")
	level2 := NewMap()
	level2.Insert(MustFromAny("level3"), NewCBORMap(level3))
	level1 := NewMap()
	level1.Insert(MustFromAny("level2"), NewCBORMap(level2))
	root := NewCBORMap(level1)

	countByLevel := map[int]int{}
	root.Walk(nil, func(_ WalkElement, level int, _ EdgeType, state any) (any, bool) {
		countByLevel[level]++
		return state, level >= 2
	})

	if got, want := countByLevel[0], 1; got != want {
		t.Fatalf("level 0 count mismatch: got %d want %d", got, want)
	}
	if got, want := countByLevel[1], 3; got != want {
		t.Fatalf("level 1 count mismatch: got %d want %d", got, want)
	}
	if got, want := countByLevel[2], 1; got != want {
		t.Fatalf("level 2 count mismatch: got %d want %d", got, want)
	}
	if got := countByLevel[3]; got != 0 {
		t.Fatalf("expected no level 3 visits, got %d", got)
	}
}

func TestWalkTraversalOrderAndEdgeTypesParity(t *testing.T) {
	m := NewMap()
	m.Insert(MustFromAny("a"), arrayFromAny(1, 2))
	m.MustInsertAny("b", 42)
	cbor := NewCBORMap(m)

	edges := make([]EdgeType, 0)
	cbor.Walk(nil, func(_ WalkElement, _ int, edge EdgeType, state any) (any, bool) {
		edges = append(edges, edge)
		return state, false
	})

	if len(edges) == 0 || edges[0].Kind != EdgeKindNone {
		t.Fatalf("expected root edge kind none, got %+v", edges)
	}

	containsEdgeKind := func(kind EdgeKind) bool {
		for _, edge := range edges {
			if edge.Kind == kind {
				return true
			}
		}
		return false
	}
	if !containsEdgeKind(EdgeKindMapKeyValue) || !containsEdgeKind(EdgeKindMapKey) || !containsEdgeKind(EdgeKindMapValue) {
		t.Fatalf("missing map edge kinds in %+v", edges)
	}
	if !containsEdgeKind(EdgeKindArrayElement) {
		t.Fatalf("missing array element edge kind in %+v", edges)
	}
}

func TestWalkTaggedValueTraversalParity(t *testing.T) {
	inner := ToTaggedValue(TagWithValue(123), arrayFromAny(1, 2, 3))
	outer := ToTaggedValue(TagWithValue(456), inner)

	edges := make([]EdgeType, 0)
	outer.Walk(nil, func(_ WalkElement, _ int, edge EdgeType, state any) (any, bool) {
		edges = append(edges, edge)
		return state, false
	})

	if len(edges) != 6 {
		t.Fatalf("unexpected edge count: got %d want %d", len(edges), 6)
	}
	if edges[0].Kind != EdgeKindNone {
		t.Fatalf("root edge mismatch: %+v", edges[0])
	}
	if edges[1].Kind != EdgeKindTaggedContent || edges[2].Kind != EdgeKindTaggedContent {
		t.Fatalf("expected two tagged content edges first, got %+v", edges)
	}
	if edges[3].Kind != EdgeKindArrayElement || edges[3].Index != 0 {
		t.Fatalf("edge[3] mismatch: %+v", edges[3])
	}
	if edges[4].Kind != EdgeKindArrayElement || edges[4].Index != 1 {
		t.Fatalf("edge[4] mismatch: %+v", edges[4])
	}
	if edges[5].Kind != EdgeKindArrayElement || edges[5].Index != 2 {
		t.Fatalf("edge[5] mismatch: %+v", edges[5])
	}
}

func TestWalkMapKeyValueSemanticsParity(t *testing.T) {
	m := NewMap()
	m.MustInsertAny("simple", 42)
	m.Insert(MustFromAny("nested"), arrayFromAny(1, 2))
	cbor := NewCBORMap(m)

	keyValueCount := 0
	individualCount := 0

	cbor.Walk(nil, func(element WalkElement, _ int, edge EdgeType, state any) (any, bool) {
		if _, _, ok := element.AsKeyValue(); ok {
			keyValueCount++
			if edge.Kind != EdgeKindMapKeyValue {
				t.Fatalf("expected key-value edge kind, got %+v", edge)
			}
		} else if edge.Kind == EdgeKindMapKey || edge.Kind == EdgeKindMapValue {
			individualCount++
		}
		return state, false
	})

	if got, want := keyValueCount, 2; got != want {
		t.Fatalf("key-value count mismatch: got %d want %d", got, want)
	}
	if got, want := individualCount, 4; got != want {
		t.Fatalf("individual key/value count mismatch: got %d want %d", got, want)
	}
}

func TestWalkEmptyAndPrimitiveStructuresParity(t *testing.T) {
	if got, want := countVisits(NewCBORArray(nil)), 1; got != want {
		t.Fatalf("empty array count mismatch: got %d want %d", got, want)
	}
	if got, want := countVisits(NewCBORMap(NewMap())), 1; got != want {
		t.Fatalf("empty map count mismatch: got %d want %d", got, want)
	}

	primitives := []CBOR{
		MustFromAny(42),
		MustFromAny("hello"),
		MustFromAny(3.2222),
		MustFromAny(true),
		Null(),
	}
	for _, primitive := range primitives {
		if got, want := countVisits(primitive), 1; got != want {
			t.Fatalf("primitive %s count mismatch: got %d want %d", primitive.DiagnosticFlat(), got, want)
		}
	}
}
