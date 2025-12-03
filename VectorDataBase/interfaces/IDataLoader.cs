namespace VectorDataBase.Interfaces;

public interface IDataLoader
{
    /// <summary>
    /// Loads data from a text file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    string[] LoadDataFromFile(string filePath);
}