"""Tests for CBOR tree traversal."""

from __future__ import annotations

from dcbor import CBOR, CBORCase, EdgeType, Map, Tag, WalkElement


def _count_visits(cbor):
    count = [0]

    def visitor(element, level, edge, state):
        count[0] += 1
        return (state, False)

    cbor.walk((), visitor)
    return count[0]


def test_walk_simple_value():
    cbor = CBOR.from_int(42)
    count = [0]

    def visitor(element, level, edge, state):
        count[0] += 1
        return (state, False)

    cbor.walk((), visitor)
    assert count[0] == 1


def test_walk_array():
    cbor = CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)])
    count = [0]
    edges = []

    def visitor(element, level, edge, state):
        count[0] += 1
        edges.append(edge)
        return (state, False)

    cbor.walk((), visitor)
    # array + 3 elements = 4 total
    assert count[0] == 4
    assert edges[0] == EdgeType.NONE
    assert edges[1] == EdgeType.ARRAY_ELEMENT
    assert edges[2] == EdgeType.ARRAY_ELEMENT
    assert edges[3] == EdgeType.ARRAY_ELEMENT


def test_walk_map():
    m = Map()
    m.insert("key1", CBOR.from_text("value1"))
    m.insert("key2", CBOR.from_text("value2"))
    cbor = CBOR.from_map(m)

    count = [0]
    edges = []

    def visitor(element, level, edge, state):
        count[0] += 1
        edges.append(edge)
        return (state, False)

    cbor.walk((), visitor)
    # map + 2 key-value pairs + 4 individual keys/values = 7 total
    assert count[0] == 7
    assert edges[0] == EdgeType.NONE
    assert EdgeType.MAP_KEY_VALUE in edges
    assert EdgeType.MAP_KEY in edges
    assert EdgeType.MAP_VALUE in edges


def test_walk_tagged():
    tag = Tag(0, "datetime")
    content = CBOR.from_text("2023-01-01T00:00:00Z")
    cbor = CBOR.from_tagged_value(tag, content)

    count = [0]
    edges = []

    def visitor(element, level, edge, state):
        count[0] += 1
        edges.append(edge)
        return (state, False)

    cbor.walk((), visitor)
    # tagged value + content = 2 total
    assert count[0] == 2
    assert edges[0] == EdgeType.NONE
    assert edges[1] == EdgeType.TAGGED_CONTENT


def test_walk_nested_structure():
    m = Map()
    m.insert("numbers", CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)]))
    m.insert("text", CBOR.from_text("hello"))
    cbor = CBOR.from_map(m)

    count = [0]

    def visitor(element, level, edge, state):
        count[0] += 1
        return (state, False)

    cbor.walk((), visitor)
    # map + 2 kv pairs + 4 individual (key+value) + array + 3 elements = 10
    assert count[0] == 10


def test_walk_early_termination():
    cbor = CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)])
    visited = []

    def visitor(element, level, edge, state):
        s = element.as_single()
        if s is not None:
            visited.append(str(s))
        should_stop = (s is not None and s.case == CBORCase.UNSIGNED and s.value == 2)
        return (state, should_stop)

    cbor.walk((), visitor)
    # Visited array, then 1, 2 (stop on 2 doesn't prevent visiting 3 since
    # stop only prevents descent into children, not siblings)
    assert "2" in visited


def test_walk_key_value_pairs():
    m = Map()
    m.insert("name", CBOR.from_text("Alice"))
    m.insert("age", CBOR.from_int(30))
    cbor = CBOR.from_map(m)

    kv_keys = []

    def visitor(element, level, edge, state):
        kv = element.as_key_value()
        if kv is not None:
            key, value = kv
            if key.case == CBORCase.TEXT:
                kv_keys.append(key.try_text())
        return (state, False)

    cbor.walk((), visitor)
    assert len(kv_keys) == 2
    assert "name" in kv_keys
    assert "age" in kv_keys


def test_walk_with_state():
    cbor = CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)])
    final_sum = [0]

    def visitor(element, level, edge, state):
        new_state = state + level
        final_sum[0] = new_state
        return (new_state, False)

    final_state = cbor.walk(0, visitor)
    assert final_state == 3
    assert final_sum[0] == 3


def test_depth_limited_traversal():
    m3 = Map()
    m3.insert("deep", CBOR.from_text("value"))
    m2 = Map()
    m2.insert("level3", CBOR.from_map(m3))
    m1 = Map()
    m1.insert("level2", CBOR.from_map(m2))
    root = CBOR.from_map(m1)

    elements_by_level = {}

    def visitor(element, level, edge, state):
        elements_by_level[level] = elements_by_level.get(level, 0) + 1
        stop = level >= 2
        return (state, stop)

    root.walk((), visitor)
    assert elements_by_level.get(0, 0) == 1  # Root
    assert elements_by_level.get(1, 0) == 3  # 1 kv pair + 2 individual key/value
    assert elements_by_level.get(2, 0) == 1  # Just the nested map, no descent
    assert elements_by_level.get(3, 0) == 0  # No visits at level 3 due to stop


def test_empty_structures():
    empty_array = CBOR.from_array([])
    assert _count_visits(empty_array) == 1  # Just the root

    empty_map = Map()
    assert _count_visits(CBOR.from_map(empty_map)) == 1  # Just the root


def test_text_extraction():
    metadata = Map()
    metadata.insert("title", CBOR.from_text("Important Document"))
    metadata.insert("author", CBOR.from_text("Alice Smith"))

    content = Map()
    content.insert("body", CBOR.from_text("Lorem ipsum dolor sit amet"))
    content.insert("footer", CBOR.from_text("Copyright 2024"))

    document = Map()
    document.insert("metadata", CBOR.from_map(metadata))
    document.insert("content", CBOR.from_map(content))
    document.insert("tags", CBOR.from_array([
        CBOR.from_text("urgent"),
        CBOR.from_text("confidential"),
        CBOR.from_text("draft"),
    ]))

    cbor = CBOR.from_map(document)
    texts = []

    def visitor(element, level, edge, state):
        s = element.as_single()
        kv = element.as_key_value()
        if s is not None and s.case == CBORCase.TEXT:
            texts.append(s.try_text())
        if kv is not None:
            k, v = kv
            if k.case == CBORCase.TEXT:
                texts.append(k.try_text())
            if v.case == CBORCase.TEXT:
                texts.append(v.try_text())
        return (state, False)

    cbor.walk((), visitor)
    assert "Important Document" in texts
    assert "Alice Smith" in texts
    assert "Lorem ipsum dolor sit amet" in texts
    assert "Copyright 2024" in texts
    assert "urgent" in texts
    assert "confidential" in texts
    assert "draft" in texts
    assert "title" in texts
    assert "author" in texts
    assert "body" in texts
    assert "footer" in texts
    assert "metadata" in texts
    assert "content" in texts
    assert "tags" in texts


def test_real_world_document():
    person = Map()
    person.insert("name", CBOR.from_text("John Doe"))
    person.insert("age", CBOR.from_int(30))
    person.insert("email", CBOR.from_text("john@example.com"))

    address = Map()
    address.insert("street", CBOR.from_text("123 Main St"))
    address.insert("city", CBOR.from_text("Anytown"))
    address.insert("zipcode", CBOR.from_text("12345"))

    person.insert("address", CBOR.from_map(address))
    person.insert("hobbies", CBOR.from_array([
        CBOR.from_text("reading"),
        CBOR.from_text("cycling"),
        CBOR.from_text("cooking"),
    ]))

    skills = Map()
    skills.insert("programming", CBOR.from_array([
        CBOR.from_text("Rust"),
        CBOR.from_text("Python"),
        CBOR.from_text("JavaScript"),
    ]))
    skills.insert("languages", CBOR.from_array([
        CBOR.from_text("English"),
        CBOR.from_text("Spanish"),
    ]))

    person.insert("skills", CBOR.from_map(skills))
    document = CBOR.from_map(person)

    strings = []

    def visitor(element, level, edge, state):
        s = element.as_single()
        kv = element.as_key_value()
        if s is not None and s.case == CBORCase.TEXT:
            strings.append(s.try_text())
        if kv is not None:
            k, v = kv
            if k.case == CBORCase.TEXT:
                strings.append(k.try_text())
            if v.case == CBORCase.TEXT:
                strings.append(v.try_text())
        return (state, False)

    document.walk((), visitor)
    assert "John Doe" in strings
    assert "john@example.com" in strings
    assert "123 Main St" in strings
    assert "Anytown" in strings
    assert "12345" in strings
    assert "reading" in strings
    assert "cycling" in strings
    assert "cooking" in strings
    assert "Rust" in strings
    assert "Python" in strings
    assert "JavaScript" in strings
    assert "English" in strings
    assert "Spanish" in strings
    assert "name" in strings
    assert "age" in strings
    assert "email" in strings
    assert "address" in strings
    assert "hobbies" in strings
    assert "skills" in strings
    assert "programming" in strings
    assert "languages" in strings


def test_edge_type_labels():
    assert EdgeType.NONE.label() is None
    assert EdgeType.ARRAY_ELEMENT.label(5) == "arr[5]"
    assert EdgeType.MAP_KEY.label() == "key"
    assert EdgeType.MAP_VALUE.label() == "val"
    assert EdgeType.TAGGED_CONTENT.label() == "content"
    assert EdgeType.MAP_KEY_VALUE.label() == "kv"
