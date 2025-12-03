using System;
using System.Collections.Generic;
namespace VectorDataBase.Interfaces;

public interface IE5Tokenizer
{
    /// <summary>
    /// Tokenizes input text into token IDs and attention mask
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    (long[] inputIds, long[] attentionMask) Tokenize(string text);
}