using System;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting Vector Database Application...");

        try
        {
            var embeddingModel = new EmbeddingModel();
            var loadData = new DataLoader();
            var dataIndex = new DataIndex();
            
            string[] loadedData = loadData.LoadDataFromFile("data.txt");
            foreach (var line in loadedData)
            {
                float[] embeddings = embeddingModel.GetEmbeddings(line);
                dataIndex.Vectors.Add(new Vector(embeddings, line));
            }
            
            foreach(var vector in dataIndex.Vectors)
            {
                Console.WriteLine($"Text: {vector.OriginalText}");
                Console.WriteLine($"Embeddings: {string.Join(", ", vector.EmbeddedValues)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
