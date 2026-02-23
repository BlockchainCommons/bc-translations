package com.blockchaincommons.bcenvelope

/** Returns this string wrapped with the given left and right delimiters. */
internal fun String.flankedBy(left: String, right: String): String = "$left$this$right"
