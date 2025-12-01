using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;


public class E5SmallTokenizer
{
    private readonly BertTokenizer _tokenizer;

    private const string RelativeVocabPath = "MLModels/e5-small-v2/vocab.txt";
    private string VocabPath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativeVocabPath);
    private const int MAX_SEQUENCE_LENGTH = 512;

    public E5SmallTokenizer()
    {
        _tokenizer = BertTokenizer.Create(vocabFilePath: VocabPath,
        options: new BertOptions
        {
            LowerCaseBeforeTokenization = true,
        });
    }

    //Encode input text to token IDs
    public (long[] inputIds, long[] TokenTypeIds, long[] AttentionMask) Encode(string text)
    {
        IReadOnlyList<int> tokenIds = _tokenizer.EncodeToIds(text, considerPreTokenization: true, considerNormalization: true);

        //Convert list of int to long[] as required by the model
        long[] inputIds = tokenIds.Select(id => (long)id).ToArray();
        int Length = inputIds.Length;

        long[] tokenTypeIds = new long[Length];
        long[] attentionMask = Enumerable.Repeat(1L, Length).ToArray();

        return (inputIds, tokenTypeIds, attentionMask);
    }

}