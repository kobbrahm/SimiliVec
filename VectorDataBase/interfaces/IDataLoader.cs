using VectorDataBase.Datahandling;
using System.Collections.Generic;
using System;
namespace VectorDataBase.Interfaces;

public interface IDataLoader
{
    IEnumerable<DocumentModel> LoadDataFromFile();
    void SaveDataToFile<T>(T data);
    IEnumerable<DocumentModel> LoadAllDocuments();
}