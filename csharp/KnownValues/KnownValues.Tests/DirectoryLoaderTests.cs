using System.Text.Json;

namespace BlockchainCommons.KnownValues.Tests;

public sealed class DirectoryLoaderTests : IDisposable
{
    public DirectoryLoaderTests()
    {
        KnownValuesTestHooks.ResetGlobalState();
    }

    public void Dispose()
    {
        KnownValuesTestHooks.ResetGlobalState();
    }

    [Fact]
    public void TestParseRegistryJson()
    {
        var json = """
            {
              "ontology": {"name": "test"},
              "entries": [
                {"codepoint": 9999, "name": "testValue", "type": "property"}
              ],
              "statistics": {}
            }
            """;

        var registry = JsonSerializer.Deserialize<RegistryFile>(json)!;
        Assert.Single(registry.Entries);
        Assert.Equal(9999ul, registry.Entries[0].Codepoint);
        Assert.Equal("testValue", registry.Entries[0].Name);
    }

    [Fact]
    public void TestParseMinimalRegistry()
    {
        var json = """{"entries": [{"codepoint": 1, "name": "minimal"}]}""";

        var registry = JsonSerializer.Deserialize<RegistryFile>(json)!;
        Assert.Single(registry.Entries);
        Assert.Equal(1ul, registry.Entries[0].Codepoint);
    }

    [Fact]
    public void TestParseFullEntry()
    {
        var json = """
            {
              "entries": [{
                "codepoint": 100,
                "name": "fullEntry",
                "type": "class",
                "uri": "https://example.com/vocab#fullEntry",
                "description": "A complete entry with all fields"
              }]
            }
            """;

        var registry = JsonSerializer.Deserialize<RegistryFile>(json)!;
        var entry = registry.Entries[0];
        Assert.Equal(100ul, entry.Codepoint);
        Assert.Equal("fullEntry", entry.Name);
        Assert.Equal("class", entry.EntryType);
        Assert.Equal("https://example.com/vocab#fullEntry", entry.Uri);
        Assert.Equal("A complete entry with all fields", entry.Description);
    }

    [Fact]
    public void TestDirectoryConfigDefault()
    {
        var config = DirectoryConfig.DefaultOnly();
        Assert.Single(config.Paths);
        Assert.EndsWith(".known-values", config.Paths[0], StringComparison.Ordinal);
    }

    [Fact]
    public void TestDirectoryConfigCustomPaths()
    {
        var config = DirectoryConfig.WithPaths(["/a", "/b"]);
        Assert.Equal(2, config.Paths.Count);
        Assert.Equal(Path.GetFullPath("/a"), config.Paths[0]);
        Assert.Equal(Path.GetFullPath("/b"), config.Paths[1]);

        var clone = config.Clone();
        Assert.Equal(config, clone);
    }

    [Fact]
    public void TestDirectoryConfigWithDefault()
    {
        var config = DirectoryConfig.WithPathsAndDefault(["/custom"]);
        Assert.Equal(2, config.Paths.Count);
        Assert.Equal(Path.GetFullPath("/custom"), config.Paths[0]);
        Assert.EndsWith(".known-values", config.Paths[1], StringComparison.Ordinal);
    }

    [Fact]
    public void TestLoadFromNonexistentDirectory()
    {
        var values = DirectoryLoader.LoadFromDirectory("/nonexistent/path/12345");
        Assert.Empty(values);
    }

    [Fact]
    public void TestLoadResultMethods()
    {
        var result = new LoadResult();
        Assert.Equal(0, result.Count);
        Assert.False(result.HasErrors);

        result.ValuesByCodepoint[1ul] = KnownValue.NewWithName(1u, "test");
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void TestGlobalRegistryStillWorks()
    {
        DirectoryLoader.SetDirectoryConfig(DirectoryConfig.WithPaths([]));
        var store = KnownValuesRegistry.KnownValues.Get();

        var isA = store.KnownValueNamed("isA");
        Assert.NotNull(isA);
        Assert.Equal(1ul, isA!.Value);
    }

    [Fact]
    public void TestLoadFromTempDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(
            tempDir.Path,
            "test_registry.json",
            """
            {
              "entries": [
                {"codepoint": 99999, "name": "integrationTestValue"}
              ]
            }
            """);

        var store = new KnownValuesStore([KnownValuesRegistry.IsA, KnownValuesRegistry.Note]);
        var count = store.LoadFromDirectory(tempDir.Path);

        Assert.Equal(1, count);
        Assert.Equal(99999ul, store.KnownValueNamed("integrationTestValue")!.Value);
        Assert.NotNull(store.KnownValueNamed("isA"));
        Assert.NotNull(store.KnownValueNamed("note"));
    }

    [Fact]
    public void TestOverrideHardcodedValue()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(
            tempDir.Path,
            "override.json",
            """
            {
              "entries": [
                {"codepoint": 1, "name": "overriddenIsA"}
              ]
            }
            """);

        var store = new KnownValuesStore([KnownValuesRegistry.IsA]);
        store.LoadFromDirectory(tempDir.Path);

        Assert.Null(store.KnownValueNamed("isA"));
        Assert.Equal(1ul, store.KnownValueNamed("overriddenIsA")!.Value);
    }

    [Fact]
    public void TestMultipleFilesInDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(tempDir.Path, "registry1.json", """{"entries": [{"codepoint": 10001, "name": "valueOne"}]}""");
        WriteJson(tempDir.Path, "registry2.json", """{"entries": [{"codepoint": 10002, "name": "valueTwo"}]}""");

        var store = new KnownValuesStore();
        var count = store.LoadFromDirectory(tempDir.Path);

        Assert.Equal(2, count);
        Assert.NotNull(store.KnownValueNamed("valueOne"));
        Assert.NotNull(store.KnownValueNamed("valueTwo"));
    }

    [Fact]
    public void TestDirectoryConfigCustomPathsLoadsValuesFromAllDirectories()
    {
        using var tempDir1 = new TemporaryDirectory();
        using var tempDir2 = new TemporaryDirectory();

        WriteJson(tempDir1.Path, "a.json", """{"entries": [{"codepoint": 20001, "name": "fromDirOne"}]}""");
        WriteJson(tempDir2.Path, "b.json", """{"entries": [{"codepoint": 20002, "name": "fromDirTwo"}]}""");

        var config = DirectoryConfig.WithPaths([tempDir1.Path, tempDir2.Path]);
        var store = new KnownValuesStore();
        var result = store.LoadFromConfig(config);

        Assert.Equal(2, result.Count);
        Assert.NotNull(store.KnownValueNamed("fromDirOne"));
        Assert.NotNull(store.KnownValueNamed("fromDirTwo"));
    }

    [Fact]
    public void TestLaterDirectoryOverridesEarlier()
    {
        using var tempDir1 = new TemporaryDirectory();
        using var tempDir2 = new TemporaryDirectory();

        WriteJson(tempDir1.Path, "first.json", """{"entries": [{"codepoint": 30000, "name": "firstVersion"}]}""");
        WriteJson(tempDir2.Path, "second.json", """{"entries": [{"codepoint": 30000, "name": "secondVersion"}]}""");

        var config = DirectoryConfig.WithPaths([tempDir1.Path, tempDir2.Path]);
        var store = new KnownValuesStore();
        store.LoadFromConfig(config);

        Assert.Equal(30000ul, store.KnownValueNamed("secondVersion")!.Value);
        Assert.Null(store.KnownValueNamed("firstVersion"));
    }

    [Fact]
    public void TestNonexistentDirectoryIsOk()
    {
        var store = new KnownValuesStore();
        var count = store.LoadFromDirectory("/nonexistent/path/12345");

        Assert.Equal(0, count);
    }

    [Fact]
    public void TestInvalidJsonIsError()
    {
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "invalid.json"), "{ this is not valid json }");

        var store = new KnownValuesStore();
        Assert.Throws<LoadError>(() => store.LoadFromDirectory(tempDir.Path));
    }

    [Fact]
    public void TestTolerantLoadingContinuesOnError()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(tempDir.Path, "valid.json", """{"entries": [{"codepoint": 40001, "name": "validValue"}]}""");
        File.WriteAllText(Path.Combine(tempDir.Path, "invalid.json"), "{ invalid json }");

        var config = DirectoryConfig.WithPaths([tempDir.Path]);
        var result = DirectoryLoader.LoadFromConfig(config);

        Assert.True(result.Values.ContainsKey(40001ul));
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void TestFullRegistryFormat()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(
            tempDir.Path,
            "full_format.json",
            """
            {
              "ontology": {
                "name": "test_registry",
                "source_url": "https://example.com",
                "start_code_point": 50000,
                "processing_strategy": "test"
              },
              "generated": {
                "tool": "test"
              },
              "entries": [
                {
                  "codepoint": 50001,
                  "name": "fullFormatValue",
                  "type": "property",
                  "uri": "https://example.com/vocab#fullFormatValue",
                  "description": "A value in full format"
                },
                {
                  "codepoint": 50002,
                  "name": "anotherValue",
                  "type": "class"
                }
              ],
              "statistics": {
                "total_entries": 2
              }
            }
            """);

        var store = new KnownValuesStore();
        var count = store.LoadFromDirectory(tempDir.Path);

        Assert.Equal(2, count);
        Assert.NotNull(store.KnownValueNamed("fullFormatValue"));
        Assert.NotNull(store.KnownValueNamed("anotherValue"));
    }

    [Fact]
    public void TestLoadResultMethodsOnConfiguredLoad()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(
            tempDir.Path,
            "test.json",
            """
            {
              "entries": [
                {"codepoint": 60001, "name": "resultTest1"},
                {"codepoint": 60002, "name": "resultTest2"}
              ]
            }
            """);

        var config = DirectoryConfig.WithPaths([tempDir.Path]);
        var result = DirectoryLoader.LoadFromConfig(config);

        Assert.Equal(2, result.Count);
        Assert.False(result.HasErrors);
        Assert.Single(result.FilesProcessed);
        Assert.Equal(2, result.GetValues().Count());
    }

    [Fact]
    public void TestEmptyEntriesArray()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(tempDir.Path, "empty.json", """{"entries": []}""");

        var store = new KnownValuesStore();
        var count = store.LoadFromDirectory(tempDir.Path);

        Assert.Equal(0, count);
    }

    [Fact]
    public void TestNonJsonFilesIgnored()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(tempDir.Path, "valid.json", """{"entries": [{"codepoint": 70001, "name": "jsonValue"}]}""");
        File.WriteAllText(Path.Combine(tempDir.Path, "readme.txt"), "Some text");
        File.WriteAllText(Path.Combine(tempDir.Path, "data.xml"), "<xml/>");

        var store = new KnownValuesStore();
        var count = store.LoadFromDirectory(tempDir.Path);

        Assert.Equal(1, count);
        Assert.NotNull(store.KnownValueNamed("jsonValue"));
    }

    [Fact]
    public void TestSetDirectoryConfigBeforeGlobalAccessLoadsCustomValues()
    {
        using var tempDir = new TemporaryDirectory();
        WriteJson(tempDir.Path, "global.json", """{"entries": [{"codepoint": 80001, "name": "globalCustomValue"}]}""");

        DirectoryLoader.SetDirectoryConfig(DirectoryConfig.WithPaths([tempDir.Path]));

        var store = KnownValuesRegistry.KnownValues.Get();
        Assert.Equal(80001ul, store.KnownValueNamed("globalCustomValue")!.Value);
    }

    [Fact]
    public void TestSetDirectoryConfigThrowsAfterGlobalRegistryInitialization()
    {
        DirectoryLoader.SetDirectoryConfig(DirectoryConfig.WithPaths([]));
        KnownValuesRegistry.KnownValues.Get();

        var error = Assert.Throws<ConfigError>(() =>
            DirectoryLoader.SetDirectoryConfig(DirectoryConfig.WithPaths(["/custom/path"])));

        Assert.Equal(new ConfigError(), error);
    }

    [Fact]
    public void TestAddSearchPathsUsesDefaultDirectoryWhenNoConfigurationExists()
    {
        DirectoryLoader.AddSearchPaths(["/custom/path"]);

        var config = DirectoryLoader.GetAndLockConfig();
        Assert.Equal(2, config.Paths.Count);
        Assert.EndsWith(".known-values", config.Paths[0], StringComparison.Ordinal);
        Assert.Equal(Path.GetFullPath("/custom/path"), config.Paths[1]);
    }

    private static void WriteJson(string directory, string fileName, string json)
    {
        File.WriteAllText(Path.Combine(directory, fileName), json);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"known-values-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
