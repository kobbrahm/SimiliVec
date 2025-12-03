namespace VectorDataBase.Interfaces;

public interface IEmbeddingModel
{
    /// <summary>
    /// Gets embeddings for the given text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    float[] GetEmbeddings(string text);

}