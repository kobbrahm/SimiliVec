namespace VectorDataBase.Interfaces;

public interface IEmbeddingModel
{
    float[] GetEmbeddings(string text);

}