using System;
using System.Collections.Generic;

public interface IE5Tokenizer
{
    (long[] inputIds, long[] attentionMask) Tokenize(string text);
}