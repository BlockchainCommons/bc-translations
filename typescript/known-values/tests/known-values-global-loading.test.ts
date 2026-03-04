import * as fs from 'node:fs';
import * as os from 'node:os';
import * as path from 'node:path';
import { afterEach, expect, test, vi } from 'vitest';

const tempDirs: string[] = [];

function createTempDir(): string {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'kv-global-'));
    tempDirs.push(dir);
    return dir;
}

afterEach(() => {
    vi.resetModules();
    for (const dir of tempDirs) {
        fs.rmSync(dir, { recursive: true, force: true });
    }
    tempDirs.length = 0;
});

test('KNOWN_VALUES loads entries from setDirectoryConfig on first access', async () => {
    const dir = createTempDir();
    fs.writeFileSync(
        path.join(dir, 'custom.json'),
        JSON.stringify({
            entries: [{ codepoint: 900001, name: 'fromCustomConfig' }],
        }),
    );

    const mod = await import('../src/index.js');
    mod.setDirectoryConfig(mod.DirectoryConfig.withPaths([dir]));

    const loaded = mod.KNOWN_VALUES.knownValueNamed('fromCustomConfig');
    expect(loaded).toBeDefined();
    expect(loaded!.value).toBe(900001n);
});

test('KNOWN_VALUES loads entries from addSearchPaths and locks config afterward', async () => {
    const dir = createTempDir();
    fs.writeFileSync(
        path.join(dir, 'added.json'),
        JSON.stringify({
            entries: [{ codepoint: 900002, name: 'fromAddedPath' }],
        }),
    );

    const mod = await import('../src/index.js');
    mod.addSearchPaths([dir]);

    const loaded = mod.KNOWN_VALUES.knownValueNamed('fromAddedPath');
    expect(loaded).toBeDefined();
    expect(loaded!.value).toBe(900002n);

    expect(() => mod.addSearchPaths([dir])).toThrow(mod.ConfigError);
});
