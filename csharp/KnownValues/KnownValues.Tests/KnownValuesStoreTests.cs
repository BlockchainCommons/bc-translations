namespace BlockchainCommons.KnownValues.Tests;

public sealed class KnownValuesStoreTests
{
    [Fact]
    public void NewAndLookupHelpersMatchRustSemantics()
    {
        var store = new KnownValuesStore(
        [
            KnownValuesRegistry.IsA,
            KnownValuesRegistry.Note,
            KnownValuesRegistry.Signed,
        ]);

        Assert.Equal("isA", store.AssignedName(KnownValuesRegistry.IsA));
        Assert.Equal("signed", store.Name(KnownValuesRegistry.Signed));
        Assert.Equal(1ul, store.KnownValueNamed("isA")!.Value);

        var fromRaw = KnownValuesStore.KnownValueForRawValue(1ul, store);
        Assert.Equal("isA", fromRaw.Name);

        var unknown = KnownValuesStore.KnownValueForRawValue(999ul, store);
        Assert.Equal("999", unknown.Name);

        var fromName = KnownValuesStore.KnownValueForName("note", store);
        Assert.NotNull(fromName);
        Assert.Equal(4ul, fromName!.Value);

        Assert.Null(KnownValuesStore.KnownValueForName("unknown", store));
        Assert.Equal("signed", KnownValuesStore.NameForKnownValue(KnownValuesRegistry.Signed, store));
        Assert.Equal("999", KnownValuesStore.NameForKnownValue(new KnownValue(999ul), store));
    }

    [Fact]
    public void InsertRemovesStaleNameWhenCodepointIsOverridden()
    {
        var store = new KnownValuesStore([KnownValuesRegistry.IsA]);

        store.Insert(KnownValue.NewWithName(1u, "overriddenIsA"));

        Assert.Null(store.KnownValueNamed("isA"));
        Assert.Equal(1ul, store.KnownValueNamed("overriddenIsA")!.Value);
    }

    [Fact]
    public void CloneCreatesIndependentStoreCopy()
    {
        var original = new KnownValuesStore([KnownValuesRegistry.IsA]);
        var clone = original.Clone();

        clone.Insert(KnownValue.NewWithName(100u, "customValue"));

        Assert.Null(original.KnownValueNamed("customValue"));
        Assert.Equal(100ul, clone.KnownValueNamed("customValue")!.Value);
    }

    [Fact]
    public void DefaultConstructorCreatesEmptyStore()
    {
        var store = new KnownValuesStore();

        Assert.Null(store.KnownValueNamed("isA"));
        Assert.Equal("1", store.Name(new KnownValue(1ul)));
    }
}
