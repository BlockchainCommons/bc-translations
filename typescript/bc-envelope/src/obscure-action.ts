import { SymmetricKey } from '@bc/components';

export type ObscureAction =
    | { type: 'elide' }
    | { type: 'encrypt'; key: SymmetricKey }
    | { type: 'compress' };

export const ObscureActions = {
    elide(): ObscureAction {
        return { type: 'elide' };
    },
    encrypt(key: SymmetricKey): ObscureAction {
        return { type: 'encrypt', key };
    },
    compress(): ObscureAction {
        return { type: 'compress' };
    },
};
