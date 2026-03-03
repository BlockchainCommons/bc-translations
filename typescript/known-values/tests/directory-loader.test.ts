import { expect, test } from 'vitest';
import {
    DirectoryConfig,
    LoadResult,
    loadFromDirectory,
} from '../src/directory-loader.js';
import { KnownValue } from '../src/known-value.js';

test('parse registry JSON', () => {
    const json = `{
        "ontology": {"name": "test"},
        "entries": [
            {"codepoint": 9999, "name": "testValue", "type": "property"}
        ],
        "statistics": {}
    }`;

    const registry = JSON.parse(json);
    expect(registry.entries.length).toBe(1);
    expect(registry.entries[0].codepoint).toBe(9999);
    expect(registry.entries[0].name).toBe('testValue');
});

test('parse minimal registry', () => {
    const json = '{"entries": [{"codepoint": 1, "name": "minimal"}]}';

    const registry = JSON.parse(json);
    expect(registry.entries.length).toBe(1);
    expect(registry.entries[0].codepoint).toBe(1);
});

test('parse full entry', () => {
    const json = `{
        "entries": [{
            "codepoint": 100,
            "name": "fullEntry",
            "type": "class",
            "uri": "https://example.com/vocab#fullEntry",
            "description": "A complete entry with all fields"
        }]
    }`;

    const registry = JSON.parse(json);
    const entry = registry.entries[0];
    expect(entry.codepoint).toBe(100);
    expect(entry.name).toBe('fullEntry');
    expect(entry.type).toBe('class');
    expect(entry.uri).toBe('https://example.com/vocab#fullEntry');
    expect(entry.description).toBeDefined();
});

test('directory config default', () => {
    const config = DirectoryConfig.defaultOnly();
    expect(config.paths.length).toBe(1);
    expect(config.paths[0]).toContain('.known-values');
});

test('directory config custom paths', () => {
    const config = DirectoryConfig.withPaths(['/a', '/b']);
    expect(config.paths.length).toBe(2);
    expect(config.paths[0]).toBe('/a');
    expect(config.paths[1]).toBe('/b');
});

test('directory config with default', () => {
    const config = DirectoryConfig.withPathsAndDefault(['/custom']);
    expect(config.paths.length).toBe(2);
    expect(config.paths[0]).toBe('/custom');
    expect(config.paths[1]).toContain('.known-values');
});

test('load from nonexistent directory', () => {
    const result = loadFromDirectory('/nonexistent/path/12345');
    expect(result).toEqual([]);
});

test('load result methods', () => {
    const result = new LoadResult();
    expect(result.valuesCount).toBe(0);
    expect(result.hasErrors).toBe(false);

    result.values.set(1n, KnownValue.withName(1, 'test'));
    expect(result.valuesCount).toBe(1);
});
