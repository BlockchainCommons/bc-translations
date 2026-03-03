/**
 * Shared configuration lock state.
 *
 * Separated from known-values-registry to avoid circular dependencies
 * with directory-loader.
 */

let _configLocked = false;

/** Returns whether the configuration is locked. */
export function isConfigLocked(): boolean {
    return _configLocked;
}

/** Locks the configuration, preventing further modifications. */
export function lockConfig(): void {
    _configLocked = true;
}
