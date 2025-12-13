/*using System;
using System.Collections.Generic;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Embedding;

internal class Program
{
    private static void Main(string[] args)
    {
        var dataLoader = new DataLoader();
        var dataIndex = new DataIndex();
        var embeddingModel = new EmbeddingModel();
        var vectorService = new VectorService(dataIndex, embeddingModel, dataLoader);
        var documents = dataLoader.LoadAllDocuments();
        vectorService.IndexDocument();
    }
}*/