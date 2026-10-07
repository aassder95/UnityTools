# TMP integer text

TmpIntegerText.TryWrite(text, value, reusableCharBuffer, prefix, suffix) writes signed 64-bit values using TMP SetCharArray without creating a numeric string. The caller owns buffer capacity/lifetime. TryFormat exposes the same format operation and output length. Minimum/maximum long and zero are supported. Failure leaves the buffer and TMP text unchanged. Reuse immutable prefix/suffix and preallocated buffer; TMP's own first-use/layout allocation is separate.
