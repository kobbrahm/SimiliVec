using System;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting Vector Database Application...");

        try
        {
            var embeddingModel = new EmbeddingModel();
            string sampleInputText = "When it rains it really poors"; // Example token IDs
            //Console.WriteLine("Input tokens count: " + sampleInputIds.Length);
            float[] embeddings = embeddingModel.GetEmbeddings(sampleInputText);

            Console.WriteLine("Embeddings generated successfully. Length: " + embeddings.Length);
            System.Console.WriteLine("First 5 embedding values: " + string.Join(", ", embeddings[..5]));
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
