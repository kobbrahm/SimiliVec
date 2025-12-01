using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ML.OnnxRuntime;

internal class Program
{
    public static Dictionary<int, VectorRecord> PayloadStore = new Dictionary<int, VectorRecord>();
    private static Random random = new Random();

    private static int _nextId = 1;

    private static float[] GenerateVector(int dimension)
    {
        float[] vector = new float[dimension];
        for (int i = 0; i < dimension; i++)
        {
            vector[i] = (float)(random.NextDouble() * 2.0 - 1.0);
        }
        return vector;
    }

    private static void CreateDataPoint(DataIndex index, int dim, string text)
    {
        int id = _nextId++;
        float[] vector = GenerateVector(dim);
        PayloadStore.Add(id, new VectorRecord(id, new Dictionary<string, string> { { "topic", text.Split(' ')[0] } } , text));
        HsnwNode node = new HsnwNode
        {
            id = id,
            Vector = vector
        };
        index.Insert(node, random);
    }
    private static void Main(string[] args)
    {
        int id = 1;
        var embeddingModel = new EmbeddingModel();
        var loadData = new DataLoader();
        string[] testData = loadData.LoadDataFromFile("data.txt");
        System.Console.WriteLine("Starting Vector Database...");
        
        // Get vector dimension from embedding model
        float[] sampleVector = embeddingModel.GetEmbeddings("sample");
        int VectorDimension = sampleVector.Length;
        
        const int MaxNeighbours = 4;
        const int EfConstruction = 20;
        const float InverseLogM = 1.0f / 1.5f;
        
        DataIndex annIndex = new DataIndex
        {
            MaxNeighbours = MaxNeighbours,
            EfConstruction = EfConstruction,
            InverseLogM = InverseLogM
        };

        Console.WriteLine("Inserting test data.. ");
        
        foreach (var line in testData)
        {
            float[] vector = embeddingModel.GetEmbeddings(line);
            HsnwNode node = new HsnwNode
            {
                id = id,
                Vector = vector
            };
            annIndex.Insert(node, random);
            PayloadStore.Add(id, new VectorRecord(id, new Dictionary<string, string> { { "topic", line.Split(' ')[0] } }, line));
            id++;
        }

        // Debug: Print insertion info
        Console.WriteLine($"\nDebug Info:");
        Console.WriteLine($"Total nodes inserted: {annIndex.Nodes.Count}");
        Console.WriteLine($"Max Level: {annIndex.MaxLevel}");
        Console.WriteLine($"Entry Point ID: {annIndex.EntryPointId}");
        int count = 0;
        foreach (var node in annIndex.Nodes.Values)
        {
            if (count >= 5) break;
            var neighborCounts = new List<int>();
            for (int l = 0; l < node.Neighbors.Length; l++)
            {
                neighborCounts.Add(node.Neighbors[l].Count);
            }
            Console.WriteLine($"  Node {node.id}: Level={node.level}, Neighbors per level={string.Join(",", neighborCounts)}");
            count++;
        }

        Console.WriteLine("Defining query vector... ");
        // Use embedding model to generate query vector from text
        string queryText = "neural network";
        float[] queryVector = embeddingModel.GetEmbeddings(queryText);
        
        const int K = 5;
        const int EfSearch = 50;

        List<int> resultIds = annIndex.FindNearestNeighbors(queryVector, K, EfSearch);

        Console.WriteLine($"\n--- Search Results (Top {K}) for '{queryText}' ---");
        Console.WriteLine($"Query entry point: {annIndex.EntryPointId}, Max Level: {annIndex.MaxLevel}");
        
        if (resultIds.Count == 0)
        {
             Console.WriteLine("No results found.");
             return;
        }

        Console.WriteLine("Rank | ID | Similarity | Original Text");
        Console.WriteLine("---------------------------------------------");
        
        for (int i = 0; i < resultIds.Count; i++)
        {
            int resultId = resultIds[i];
            VectorRecord record = PayloadStore[resultId];
            float[] resultVector = annIndex.Nodes[resultId].Vector;
            
            // Calculate final similarity for display
            float similarity = DataIndex.CosineSimilarity(queryVector, resultVector); 
            
            Console.WriteLine($"{i + 1,-4} | {resultId,-2} | {similarity:F4} | {record.OriginalText}");
        }
        
        // Expected result: ID 1 should be the closest or one of the top results.
        Console.WriteLine("\nTest complete.");
}
}
