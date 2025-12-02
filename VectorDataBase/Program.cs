using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ML.OnnxRuntime;

internal class Program
{
    public static Dictionary<int, VectorRecord> PayloadStore = new Dictionary<int, VectorRecord>();
    private static Random random = new Random();

    private static int _nextId = 1;

    private static void Main(string[] args)
    {
        int id = 1;
        var embeddingModel = new EmbeddingModel();
        var loadData = new DataLoader();
        string[] testData = loadData.LoadDataFromFile("data.txt");
        Console.WriteLine("Vector Database — starting\n");

        // Get vector dimension from embedding model (sample)
        float[] sampleVector = embeddingModel.GetEmbeddings("sample");
        int VectorDimension = sampleVector.Length;

        // ANN index parameters
        const int MaxNeighbours = 4;
        const int EfConstruction = 20;
        const float InverseLogM = 1.0f / 1.5f;

        // Create ANN index
        var annIndex = new DataIndex
        {
            MaxNeighbours = MaxNeighbours,
            EfConstruction = EfConstruction,
            InverseLogM = InverseLogM
        };

        Console.WriteLine("Inserting test data...");
        // Insert data into ANN index
        foreach (var line in testData)
        {
            float[] vector = embeddingModel.GetEmbeddings(line);
            var node = new HsnwNode { id = id, Vector = vector };
            annIndex.Insert(node, random);
            PayloadStore.Add(id, new VectorRecord(id, new Dictionary<string, string> { { "topic", line.Split(' ')[0] } }, line));
            id++;
        }

        // Index summary
        Console.WriteLine("\nIndex summary:");
        Console.WriteLine($"- Total nodes: {annIndex.Nodes.Count}");
        Console.WriteLine($"- Max level: {annIndex.MaxLevel}");
        Console.WriteLine($"- Entry point id: {annIndex.EntryPointId}");

        // Prepare and run a sample query
        string queryText = "medical";
        Console.WriteLine($"\nQuery: '{queryText}' (top 5 results)\n");
        float[] queryVector = embeddingModel.GetEmbeddings(queryText);

        const int K = 5;
        const int EfSearch = 50;

        var resultIds = annIndex.FindNearestNeighbors(queryVector, K, EfSearch);

        if (resultIds == null || resultIds.Count == 0)
        {
            Console.WriteLine("No results found.");
            return;
        }

        Console.WriteLine("Rank  ID   Similarity   Original Text");
        Console.WriteLine("----  ----  -----------  --------------------------------------------");
        for (int i = 0; i < resultIds.Count; i++)
        {
            int resultId = resultIds[i];
            var record = PayloadStore[resultId];
            var resultVector = annIndex.Nodes[resultId].Vector;
            float similarity = HSNWUtils.CosineSimilarity(queryVector, resultVector);
            Console.WriteLine($"{i + 1,4}  {resultId,4}  {similarity,11:F4}  {record.OriginalText}");
        }

        Console.WriteLine("\nDone.");
}
}
