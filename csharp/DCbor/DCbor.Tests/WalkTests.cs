namespace BlockchainCommons.DCbor.Tests;

public class WalkTests
{
    // --- Helper ---

    private static int CountVisits(Cbor cbor)
    {
        int count = 0;
        cbor.Walk(0, (element, level, edge, state) =>
        {
            count++;
            return (state, false);
        });
        return count;
    }

    // --- Tests ---

    [Fact]
    public void TestTraversalCounts()
    {
        // Simple array
        var array = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3)
        }));
        Assert.Equal(4, CountVisits(array)); // Root + 3 elements

        // Simple map
        var map = new CborMap();
        map.Insert(Cbor.FromString("a"), Cbor.FromInt(1));
        map.Insert(Cbor.FromString("b"), Cbor.FromInt(2));
        var mapCbor = new Cbor(CborCase.Map(map));
        Assert.Equal(7, CountVisits(mapCbor)); // Root + 2 kv pairs + 4 individual

        // Tagged value
        var tagged = Cbor.ToTaggedValue(42, Cbor.FromInt(100));
        Assert.Equal(2, CountVisits(tagged)); // Root + content

        // Nested structure
        var innerMap = new CborMap();
        innerMap.Insert(Cbor.FromString("x"),
            new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(1), Cbor.FromInt(2) })));
        var outerMap = new CborMap();
        outerMap.Insert(Cbor.FromString("inner"), new Cbor(CborCase.Map(innerMap)));
        outerMap.Insert(Cbor.FromString("simple"), Cbor.FromInt(42));
        var nested = new Cbor(CborCase.Map(outerMap));
        Assert.Equal(12, CountVisits(nested));
    }

    [Fact]
    public void TestVisitorStateThreading()
    {
        var array = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3),
            Cbor.FromInt(4), Cbor.FromInt(5)
        }));

        int evenCount = 0;
        array.Walk(0, (element, level, edge, state) =>
        {
            var single = element.AsSingle();
            if (single != null && single.Case is CborCase.UnsignedCase u && u.Value % 2 == 0)
            {
                evenCount++;
            }
            return (state, false);
        });

        Assert.Equal(2, evenCount); // 2 and 4
    }

    [Fact]
    public void TestEarlyTermination()
    {
        var nested = new Cbor(CborCase.Array(new List<Cbor>
        {
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("should"), Cbor.FromString("see"), Cbor.FromString("this")
            })),
            Cbor.FromString("abort_marker"),
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("should"), Cbor.FromString("not"), Cbor.FromString("see")
            }))
        }));

        var visitLog = new List<string>();
        bool foundAbort = false;

        nested.Walk(0, (element, level, edge, state) =>
        {
            string desc = $"L{level}: {edge.GetType().Name} - {element.DiagnosticFlat()}";
            visitLog.Add(desc);

            // Check for abort marker
            var single = element.AsSingle();
            if (single != null && single.Case is CborCase.TextCase t && t.Value == "abort_marker")
            {
                foundAbort = true;
                return (state, true);
            }

            // After abort marker, stop descent into array at index 2
            bool stop = foundAbort
                && single != null
                && edge is EdgeType.ArrayElementEdge ae && ae.Index == 2;

            return (state, stop);
        });

        var logStr = string.Join("\n", visitLog);

        // Should visit the abort marker
        Assert.Contains("abort_marker", logStr);

        // Should visit first array children
        Assert.Contains("should", logStr);
        Assert.Contains("see", logStr);
        Assert.Contains("this", logStr);

        // Should visit the third array itself
        Assert.Contains("[\"should\", \"not\", \"see\"]", logStr);

        // Should NOT have level 2 visits from the third array
        var thirdArrayIndex = visitLog.FindIndex(line =>
            line.Contains("ArrayElementEdge") && line.Contains("[\"should\", \"not\", \"see\"]"));

        Assert.True(thirdArrayIndex >= 0, "Could not find third array visit in log");

        var visitsAfterThird = visitLog.Skip(thirdArrayIndex + 1)
            .Where(line => line.StartsWith("L2:"))
            .ToList();

        Assert.Empty(visitsAfterThird);
    }

    [Fact]
    public void TestDepthLimitedTraversal()
    {
        var level3 = new CborMap();
        level3.Insert(Cbor.FromString("deep"), Cbor.FromString("value"));

        var level2 = new CborMap();
        level2.Insert(Cbor.FromString("level3"), new Cbor(CborCase.Map(level3)));

        var level1 = new CborMap();
        level1.Insert(Cbor.FromString("level2"), new Cbor(CborCase.Map(level2)));

        var root = new Cbor(CborCase.Map(level1));

        var elementsByLevel = new Dictionary<int, int>();

        root.Walk(0, (element, level, edge, state) =>
        {
            if (!elementsByLevel.ContainsKey(level))
                elementsByLevel[level] = 0;
            elementsByLevel[level]++;

            bool stop = level >= 2;
            return (state, stop);
        });

        Assert.Equal(1, elementsByLevel.GetValueOrDefault(0, 0)); // Root
        Assert.Equal(3, elementsByLevel.GetValueOrDefault(1, 0)); // 1 kv pair + 2 individual
        Assert.Equal(1, elementsByLevel.GetValueOrDefault(2, 0)); // Just the nested map, no descent
        Assert.Equal(0, elementsByLevel.GetValueOrDefault(3, 0)); // No visits at level 3
    }

    [Fact]
    public void TestTextExtraction()
    {
        var metadata = new CborMap();
        metadata.Insert(Cbor.FromString("title"), Cbor.FromString("Important Document"));
        metadata.Insert(Cbor.FromString("author"), Cbor.FromString("Alice Smith"));

        var content = new CborMap();
        content.Insert(Cbor.FromString("body"), Cbor.FromString("Lorem ipsum dolor sit amet"));
        content.Insert(Cbor.FromString("footer"), Cbor.FromString("Copyright 2024"));

        var document = new CborMap();
        document.Insert(Cbor.FromString("metadata"), new Cbor(CborCase.Map(metadata)));
        document.Insert(Cbor.FromString("content"), new Cbor(CborCase.Map(content)));
        document.Insert(Cbor.FromString("tags"),
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("urgent"), Cbor.FromString("confidential"), Cbor.FromString("draft")
            })));

        var cbor = new Cbor(CborCase.Map(document));

        var texts = new List<string>();

        cbor.Walk(0, (element, level, edge, state) =>
        {
            var single = element.AsSingle();
            if (single != null && single.Case is CborCase.TextCase t)
            {
                texts.Add(t.Value);
            }

            var kv = element.AsKeyValue();
            if (kv.HasValue)
            {
                if (kv.Value.Key.Case is CborCase.TextCase kt)
                    texts.Add(kt.Value);
                if (kv.Value.Value.Case is CborCase.TextCase vt)
                    texts.Add(vt.Value);
            }

            return (state, false);
        });

        Assert.Contains("Important Document", texts);
        Assert.Contains("Alice Smith", texts);
        Assert.Contains("Lorem ipsum dolor sit amet", texts);
        Assert.Contains("Copyright 2024", texts);
        Assert.Contains("urgent", texts);
        Assert.Contains("confidential", texts);
        Assert.Contains("draft", texts);
        Assert.Contains("title", texts);
        Assert.Contains("author", texts);
        Assert.Contains("body", texts);
        Assert.Contains("footer", texts);
        Assert.Contains("metadata", texts);
        Assert.Contains("content", texts);
        Assert.Contains("tags", texts);
    }

    [Fact]
    public void TestTraversalOrderAndEdgeTypes()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromString("a"),
            new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(1), Cbor.FromInt(2) })));
        map.Insert(Cbor.FromString("b"), Cbor.FromInt(42));
        var cbor = new Cbor(CborCase.Map(map));

        var traversalLog = new List<(string Desc, EdgeType Edge)>();

        cbor.Walk(0, (element, level, edge, state) =>
        {
            string desc;
            var single = element.AsSingle();
            if (single != null)
                desc = $"Single({single.DiagnosticFlat()})";
            else
            {
                var kv = element.AsKeyValue()!.Value;
                desc = $"KeyValue({kv.Key.DiagnosticFlat()}: {kv.Value.DiagnosticFlat()})";
            }
            traversalLog.Add((desc, edge));
            return (state, false);
        });

        // Verify root visit
        Assert.True(traversalLog[0].Edge.Equals(EdgeType.None));

        // Check expected edge types
        var edgeTypes = traversalLog.Select(x => x.Edge).ToList();
        Assert.Contains(edgeTypes, e => e.Equals(EdgeType.MapKeyValue));
        Assert.Contains(edgeTypes, e => e.Equals(EdgeType.MapKey));
        Assert.Contains(edgeTypes, e => e.Equals(EdgeType.MapValue));
        Assert.Contains(edgeTypes, e => e.Equals(EdgeType.ArrayElement(0)));
        Assert.Contains(edgeTypes, e => e.Equals(EdgeType.ArrayElement(1)));
    }

    [Fact]
    public void TestTaggedValueTraversal()
    {
        var innerArray = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3)
        }));
        var innerTagged = Cbor.ToTaggedValue(123, innerArray);
        var outerTagged = Cbor.ToTaggedValue(456, innerTagged);

        var edgeLog = new List<EdgeType>();

        outerTagged.Walk(0, (element, level, edge, state) =>
        {
            edgeLog.Add(edge);
            return (state, false);
        });

        // None (root), TaggedContent, TaggedContent, ArrayElement(0), ArrayElement(1), ArrayElement(2)
        Assert.True(edgeLog[0].Equals(EdgeType.None));
        Assert.True(edgeLog[1].Equals(EdgeType.TaggedContent));
        Assert.True(edgeLog[2].Equals(EdgeType.TaggedContent));
        Assert.True(edgeLog[3].Equals(EdgeType.ArrayElement(0)));
        Assert.True(edgeLog[4].Equals(EdgeType.ArrayElement(1)));
        Assert.True(edgeLog[5].Equals(EdgeType.ArrayElement(2)));
    }

    [Fact]
    public void TestMapKeyValueSemantics()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromString("simple"), Cbor.FromInt(42));
        map.Insert(Cbor.FromString("nested"),
            new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(1), Cbor.FromInt(2) })));
        var cbor = new Cbor(CborCase.Map(map));

        int keyvalueCount = 0;
        int individualCount = 0;

        cbor.Walk(0, (element, level, edge, state) =>
        {
            if (element.AsKeyValue().HasValue)
            {
                keyvalueCount++;
                Assert.True(edge.Equals(EdgeType.MapKeyValue));
            }
            else if (edge.Equals(EdgeType.MapKey) || edge.Equals(EdgeType.MapValue))
            {
                individualCount++;
            }
            return (state, false);
        });

        Assert.Equal(2, keyvalueCount);
        Assert.Equal(4, individualCount);
    }

    [Fact]
    public void TestStopFlagPreventsDescent()
    {
        var nested = new Cbor(CborCase.Array(new List<Cbor>
        {
            new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3) })),
            new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(4), Cbor.FromInt(5), Cbor.FromInt(6) })),
            new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(7), Cbor.FromInt(8), Cbor.FromInt(9) }))
        }));

        var visitLog = new List<string>();

        nested.Walk(0, (element, level, edge, state) =>
        {
            string desc = $"L{level}: {edge.GetType().Name} - {element.DiagnosticFlat()}";
            visitLog.Add(desc);

            // Stop descent into the first nested array (index 0)
            bool stop = level == 1 && edge is EdgeType.ArrayElementEdge ae && ae.Index == 0;
            return (state, stop);
        });

        var logStr = string.Join("\n", visitLog);

        // Should visit first array but not descend
        Assert.Contains("ArrayElementEdge - [1, 2, 3]", logStr);

        // Level 2 visits should NOT contain values from first array
        var level2Lines = visitLog.Where(l => l.StartsWith("L2:")).ToList();
        foreach (var line in level2Lines)
        {
            Assert.DoesNotContain(" - 1", line);
            Assert.DoesNotContain(" - 2", line);
            Assert.DoesNotContain(" - 3", line);
        }

        // Should visit second and third arrays with descent
        Assert.Contains("ArrayElementEdge - [4, 5, 6]", logStr);
        Assert.Contains("ArrayElementEdge - [7, 8, 9]", logStr);

        // Should find level 2 visits from second and third arrays
        Assert.True(logStr.Contains("L2:") && (logStr.Contains(" - 4") || logStr.Contains(" - 5") || logStr.Contains(" - 6")));
        Assert.True(logStr.Contains("L2:") && (logStr.Contains(" - 7") || logStr.Contains(" - 8") || logStr.Contains(" - 9")));
    }

    [Fact]
    public void TestEmptyStructures()
    {
        // Empty array
        var emptyArray = new Cbor(CborCase.Array(new List<Cbor>()));
        Assert.Equal(1, CountVisits(emptyArray)); // Just root

        // Empty map
        var emptyMap = new Cbor(CborCase.Map(new CborMap()));
        Assert.Equal(1, CountVisits(emptyMap)); // Just root
    }

    [Fact]
    public void TestPrimitiveValues()
    {
        var primitives = new List<Cbor>
        {
            Cbor.FromInt(42),
            Cbor.FromString("hello"),
            Cbor.FromDouble(3.2222),
            Cbor.FromBool(true),
            Cbor.Null()
        };

        foreach (var primitive in primitives)
        {
            Assert.Equal(1, CountVisits(primitive));
        }
    }

    [Fact]
    public void TestRealWorldDocument()
    {
        var person = new CborMap();
        person.Insert(Cbor.FromString("name"), Cbor.FromString("John Doe"));
        person.Insert(Cbor.FromString("age"), Cbor.FromInt(30));
        person.Insert(Cbor.FromString("email"), Cbor.FromString("john@example.com"));

        var address = new CborMap();
        address.Insert(Cbor.FromString("street"), Cbor.FromString("123 Main St"));
        address.Insert(Cbor.FromString("city"), Cbor.FromString("Anytown"));
        address.Insert(Cbor.FromString("zipcode"), Cbor.FromString("12345"));

        person.Insert(Cbor.FromString("address"), new Cbor(CborCase.Map(address)));
        person.Insert(Cbor.FromString("hobbies"),
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("reading"), Cbor.FromString("cycling"), Cbor.FromString("cooking")
            })));

        var skills = new CborMap();
        skills.Insert(Cbor.FromString("programming"),
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("Rust"), Cbor.FromString("Python"), Cbor.FromString("JavaScript")
            })));
        skills.Insert(Cbor.FromString("languages"),
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("English"), Cbor.FromString("Spanish")
            })));

        person.Insert(Cbor.FromString("skills"), new Cbor(CborCase.Map(skills)));

        var document = new Cbor(CborCase.Map(person));

        var strings = new List<string>();

        document.Walk(0, (element, level, edge, state) =>
        {
            var single = element.AsSingle();
            if (single != null && single.Case is CborCase.TextCase t)
            {
                strings.Add(t.Value);
            }

            var kv = element.AsKeyValue();
            if (kv.HasValue)
            {
                if (kv.Value.Key.Case is CborCase.TextCase kt)
                    strings.Add(kt.Value);
                if (kv.Value.Value.Case is CborCase.TextCase vt)
                    strings.Add(vt.Value);
            }

            return (state, false);
        });

        Assert.Contains("John Doe", strings);
        Assert.Contains("john@example.com", strings);
        Assert.Contains("123 Main St", strings);
        Assert.Contains("Anytown", strings);
        Assert.Contains("12345", strings);
        Assert.Contains("reading", strings);
        Assert.Contains("cycling", strings);
        Assert.Contains("cooking", strings);
        Assert.Contains("Rust", strings);
        Assert.Contains("Python", strings);
        Assert.Contains("JavaScript", strings);
        Assert.Contains("English", strings);
        Assert.Contains("Spanish", strings);
        Assert.Contains("name", strings);
        Assert.Contains("age", strings);
        Assert.Contains("email", strings);
        Assert.Contains("address", strings);
        Assert.Contains("hobbies", strings);
        Assert.Contains("skills", strings);
        Assert.Contains("programming", strings);
        Assert.Contains("languages", strings);
    }
}
