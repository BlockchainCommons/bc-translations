import * as fs from 'node:fs';
import * as os from 'node:os';
import * as path from 'node:path';
import { afterEach, beforeEach, expect, test } from 'vitest';
import {
    DirectoryConfig,
    IS_A,
    KNOWN_VALUES,
    KnownValuesStore,
    NOTE,
    loadFromConfig,
} from '../src/index.js';

// Helper to create temp directories
let tempDirs: string[] = [];

function createTempDir(): string {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'kv-test-'));
    tempDirs.push(dir);
    return dir;
}

afterEach(() => {
    for (const dir of tempDirs) {
        fs.rmSync(dir, { recursive: true, force: true });
    }
    tempDirs = [];
});

test('global registry still works', () => {
    const isA = KNOWN_VALUES.knownValueNamed('isA');
    expect(isA).toBeDefined();
    expect(isA!.value).toBe(1n);
});

test('load from temp directory', () => {
    const tempDir = createTempDir();
    const filePath = path.join(tempDir, 'test_registry.json');

    const json = JSON.stringify({
        entries: [
            { codepoint: 99999, name: 'integrationTestValue' },
        ],
    });
    fs.writeFileSync(filePath, json);

    const store = new KnownValuesStore([IS_A, NOTE]);
    const count = store.loadFromDirectory(tempDir);

    expect(count).toBe(1);

    const loaded = store.knownValueNamed('integrationTestValue');
    expect(loaded).toBeDefined();
    expect(loaded!.value).toBe(99999n);

    // Original values should still be present
    expect(store.knownValueNamed('isA')).toBeDefined();
    expect(store.knownValueNamed('note')).toBeDefined();
});

test('override hardcoded value', () => {
    const tempDir = createTempDir();
    const filePath = path.join(tempDir, 'override.json');

    // Override IS_A (codepoint 1) with a custom name
    const json = JSON.stringify({
        entries: [
            { codepoint: 1, name: 'overriddenIsA' },
        ],
    });
    fs.writeFileSync(filePath, json);

    const store = new KnownValuesStore([IS_A]);
    store.loadFromDirectory(tempDir);

    // The original "isA" name should be gone (replaced)
    expect(store.knownValueNamed('isA')).toBeUndefined();

    // The new name should work
    const overridden = store.knownValueNamed('overriddenIsA');
    expect(overridden).toBeDefined();
    expect(overridden!.value).toBe(1n);
});

test('multiple files in directory', () => {
    const tempDir = createTempDir();

    fs.writeFileSync(
        path.join(tempDir, 'registry1.json'),
        JSON.stringify({ entries: [{ codepoint: 10001, name: 'valueOne' }] }),
    );
    fs.writeFileSync(
        path.join(tempDir, 'registry2.json'),
        JSON.stringify({ entries: [{ codepoint: 10002, name: 'valueTwo' }] }),
    );

    const store = new KnownValuesStore();
    const count = store.loadFromDirectory(tempDir);

    expect(count).toBe(2);
    expect(store.knownValueNamed('valueOne')).toBeDefined();
    expect(store.knownValueNamed('valueTwo')).toBeDefined();
});

test('directory config custom paths', () => {
    const tempDir1 = createTempDir();
    const tempDir2 = createTempDir();

    fs.writeFileSync(
        path.join(tempDir1, 'a.json'),
        JSON.stringify({ entries: [{ codepoint: 20001, name: 'fromDirOne' }] }),
    );
    fs.writeFileSync(
        path.join(tempDir2, 'b.json'),
        JSON.stringify({ entries: [{ codepoint: 20002, name: 'fromDirTwo' }] }),
    );

    const config = DirectoryConfig.withPaths([tempDir1, tempDir2]);

    const store = new KnownValuesStore();
    const result = store.loadFromConfig(config);

    expect(result.valuesCount).toBe(2);
    expect(store.knownValueNamed('fromDirOne')).toBeDefined();
    expect(store.knownValueNamed('fromDirTwo')).toBeDefined();
});

test('later directory overrides earlier', () => {
    const tempDir1 = createTempDir();
    const tempDir2 = createTempDir();

    // Both directories have same codepoint with different names
    fs.writeFileSync(
        path.join(tempDir1, 'first.json'),
        JSON.stringify({ entries: [{ codepoint: 30000, name: 'firstVersion' }] }),
    );
    fs.writeFileSync(
        path.join(tempDir2, 'second.json'),
        JSON.stringify({ entries: [{ codepoint: 30000, name: 'secondVersion' }] }),
    );

    const config = DirectoryConfig.withPaths([tempDir1, tempDir2]);
    const store = new KnownValuesStore();
    store.loadFromConfig(config);

    // Second directory should win (later in list)
    const value = store.knownValueNamed('secondVersion');
    expect(value).toBeDefined();
    expect(value!.value).toBe(30000n);

    // First name should be gone
    expect(store.knownValueNamed('firstVersion')).toBeUndefined();
});

test('nonexistent directory is ok', () => {
    const store = new KnownValuesStore();
    const result = store.loadFromDirectory('/nonexistent/path/12345');
    expect(result).toBe(0);
});

test('invalid JSON is error', () => {
    const tempDir = createTempDir();
    fs.writeFileSync(path.join(tempDir, 'invalid.json'), '{ this is not valid json }');

    const store = new KnownValuesStore();
    expect(() => store.loadFromDirectory(tempDir)).toThrow();
});

test('tolerant loading continues on error', () => {
    const tempDir = createTempDir();

    // One valid file
    fs.writeFileSync(
        path.join(tempDir, 'valid.json'),
        JSON.stringify({ entries: [{ codepoint: 40001, name: 'validValue' }] }),
    );

    // One invalid file
    fs.writeFileSync(
        path.join(tempDir, 'invalid.json'),
        '{ invalid json }',
    );

    const config = DirectoryConfig.withPaths([tempDir]);
    const result = loadFromConfig(config);

    // Should have loaded the valid value
    expect(result.values.has(40001n)).toBe(true);

    // Should have recorded the error
    expect(result.hasErrors).toBe(true);
});

test('full registry format', () => {
    const tempDir = createTempDir();

    const json = JSON.stringify({
        ontology: {
            name: 'test_registry',
            source_url: 'https://example.com',
            start_code_point: 50000,
            processing_strategy: 'test',
        },
        generated: {
            tool: 'test',
        },
        entries: [
            {
                codepoint: 50001,
                name: 'fullFormatValue',
                type: 'property',
                uri: 'https://example.com/vocab#fullFormatValue',
                description: 'A value in full format',
            },
            {
                codepoint: 50002,
                name: 'anotherValue',
                type: 'class',
            },
        ],
        statistics: {
            total_entries: 2,
        },
    });
    fs.writeFileSync(path.join(tempDir, 'full_format.json'), json);

    const store = new KnownValuesStore();
    const count = store.loadFromDirectory(tempDir);

    expect(count).toBe(2);
    expect(store.knownValueNamed('fullFormatValue')).toBeDefined();
    expect(store.knownValueNamed('anotherValue')).toBeDefined();
});

test('load result methods', () => {
    const tempDir = createTempDir();

    fs.writeFileSync(
        path.join(tempDir, 'test.json'),
        JSON.stringify({
            entries: [
                { codepoint: 60001, name: 'resultTest1' },
                { codepoint: 60002, name: 'resultTest2' },
            ],
        }),
    );

    const config = DirectoryConfig.withPaths([tempDir]);
    const result = loadFromConfig(config);

    expect(result.valuesCount).toBe(2);
    expect(result.hasErrors).toBe(false);
    expect(result.filesProcessed.length).toBe(1);
});

test('empty entries array', () => {
    const tempDir = createTempDir();
    fs.writeFileSync(
        path.join(tempDir, 'empty.json'),
        JSON.stringify({ entries: [] }),
    );

    const store = new KnownValuesStore();
    const count = store.loadFromDirectory(tempDir);

    expect(count).toBe(0);
});

test('non-JSON files ignored', () => {
    const tempDir = createTempDir();

    // JSON file should be loaded
    fs.writeFileSync(
        path.join(tempDir, 'valid.json'),
        JSON.stringify({ entries: [{ codepoint: 70001, name: 'jsonValue' }] }),
    );

    // Non-JSON files should be ignored
    fs.writeFileSync(path.join(tempDir, 'readme.txt'), 'Some text');
    fs.writeFileSync(path.join(tempDir, 'data.xml'), '<xml/>');

    const store = new KnownValuesStore();
    const count = store.loadFromDirectory(tempDir);

    expect(count).toBe(1);
    expect(store.knownValueNamed('jsonValue')).toBeDefined();
});
