using System;
using System.Collections.Generic;
namespace VectorDataBase.Interfaces;

public interface IE5Tokenizer
{
    (long[] inputIds, long[] attentionMask) Tokenize(string text);
}