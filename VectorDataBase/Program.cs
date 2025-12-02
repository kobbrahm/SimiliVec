using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ML.OnnxRuntime;

internal class Program
{
    static void Main(string[] args)
    {
        var embeddingModel = new EmbeddingModel();
        var dataLoader = new DataLoader();
        var DataIndex = new DataIndex
        {
            MaxNeighbours = 4,
            EfConstruction = 20,
            InverseLogM = 1.0f / 1.5f
        };
        var vectorService = new VectorService(DataIndex, embeddingModel, dataLoader);
        var FakeFrontEnd = new FakeFrontEnd(vectorService);
        FakeFrontEnd.GetAnswers();
    }
}

