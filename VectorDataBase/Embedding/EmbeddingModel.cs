using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Embedding;

/// <summary>
/// Input data schema for the ONNX model
/// </summary>
public class InputData
{
    [ColumnName("input_ids")]
    [VectorType]
    public long[] Inputids { get; set; } = Array.Empty<long>();

    [ColumnName("token_type_ids")]
    [VectorType]
    public long[] TokenTypeIds { get; set; } = Array.Empty<long>();

    [ColumnName("attention_mask")]
    [VectorType]
    public long[] AttentionMask { get; set; } = Array.Empty<long>();
}

/// <summary>
/// Output data schema for the ONNX model
/// </summary>
public class OutputData
{
    [ColumnName("last_hidden_state")]
    [VectorType]
    public float[] Output { get; set; } = Array.Empty<float>();
}

/// <summary>
/// Class to generate embeddings using E5-Small-V2 ONNX model
/// </summary>
public class EmbeddingModel : IEmbeddingModel
{
    private readonly PredictionEngine<InputData, OutputData> _predictionEngine;
    private readonly E5SmallTokenizer _tokenizer;
    private const string RelativePath = "MLModels/e5-small-v2/model.onnx";
    private string _modelPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativePath);


    /// <summary>
    /// Constructor to load ONNX model and initialize tokenizer
    /// </summary>
    public EmbeddingModel()
    {
        var MLContext = new MLContext();
        var onnxPipeline = MLContext.Transforms.ApplyOnnxModel(
            modelFile: _modelPath,
            outputColumnNames: new[] { "last_hidden_state" },
            inputColumnNames: new[] { "input_ids", "token_type_ids", "attention_mask" });

        var emptyDataView = MLContext.Data.LoadFromEnumerable(Enumerable.Empty<InputData>());
        var onnxModel = onnxPipeline.Fit(emptyDataView);
        _predictionEngine = MLContext.Model.CreatePredictionEngine<InputData, OutputData>(onnxModel);
        _tokenizer = new E5SmallTokenizer();
    }

    /// <summary>
    /// Get Embeddings for input text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public float[] GetEmbeddings(string text)
    {
        var (tokenIds, tokenTypeIds, attentionMask) = _tokenizer.Encode(text);
        var input = new InputData { 
            Inputids = tokenIds,
            TokenTypeIds = tokenTypeIds,
            AttentionMask = attentionMask
            };
        var outputData = _predictionEngine.Predict(input);
        float[] sentenceEmbedding = MeanPool(outputData.Output, tokenIds.Length, 384);

        //returns embedding vector
        return sentenceEmbedding;
    }

    /// <summary>
    /// Mean-pools token embeddings into a single sentence embedding.
    /// </summary>
    /// <param name="tokenEmbeddings"></param>
    /// <param name="sequenceLength"></param>
    /// <param name="embeddingDimension"></param>
    /// <returns></returns>
    public float[] MeanPool(float[] tokenEmbeddings, int sequenceLength, int embeddingDimension)
    {
        float[] pooledVector = new float[embeddingDimension];
        for (int j = 0; j < embeddingDimension; j++)
        {
            float sum = 0f;
            for (int i = 0; i < sequenceLength; i++)
            {
                sum += tokenEmbeddings[i * embeddingDimension + j];
            }
            //calculating mean
            pooledVector[j] = sum / sequenceLength;
        }

        return pooledVector;
    }


}