package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.SymmetricKey

/**
 * Actions that can be performed on parts of an envelope to obscure them.
 *
 * Gordian Envelope supports several ways to obscure parts of an envelope while
 * maintaining its semantic integrity and digest tree.
 */
sealed class ObscureAction {
    /** Elide the target, leaving only its digest. */
    data object Elide : ObscureAction()

    /** Encrypt the target using the specified symmetric key. */
    data class Encrypt(val key: SymmetricKey) : ObscureAction()

    /** Compress the target. */
    data object Compress : ObscureAction()
}
