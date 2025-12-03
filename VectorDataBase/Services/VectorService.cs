using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using Microsoft.VisualBasic;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Datahandling;

namespace VectorDataBase.Services;

public class VectorService
{
    private readonly IDataIndex _dataIndex;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly IDataLoader _dataLoader;
    private readonly Dictionary<int, VectorRecord> _payLoads = new Dictionary<int, VectorRecord>();
    private static int _currentId = 0;
    private void nextId() => _currentId++;
    private readonly Random _random = new Random();
    public VectorService(IDataIndex dataIndex, IEmbeddingModel embeddingModel, IDataLoader dataLoader)
    {
        _dataIndex = dataIndex;
        _embeddingModel = embeddingModel;
        _dataLoader = dataLoader;
    }

    public void IndexDocument()
    {
        string[] loadData = _dataLoader.LoadDataFromFile("Data.txt");
        foreach(var line in loadData)
        {
            float[] vector = _embeddingModel.GetEmbeddings(line);
            var node = new HsnwNode{id = _currentId, Vector = vector};
            _dataIndex.Insert(node, _random);
            _payLoads.Add(_currentId, new VectorRecord(_currentId, new Dictionary<string, string> { { "topic", line.Split(' ')[0] } }, line));
            nextId();
        }
    }

    public List<VectorRecord> Search(string query, int k)
    {
        float[] queryVector = _embeddingModel.GetEmbeddings(query);
        List<int> neighborIds = _dataIndex.FindNearestNeighbors(queryVector, k);

        List<VectorRecord> results = new List<VectorRecord>();
        foreach(int id in neighborIds)
        {
            if (_payLoads.TryGetValue(id, out var record))
            {

                results.Add(record);
            }
        }
        return results;
    }

    public Dictionary<float, VectorRecord> GetDistances(string query, List<VectorRecord> records)
    {
        float[] queryVector = _embeddingModel.GetEmbeddings(query);
        Dictionary<float, VectorRecord> results = new Dictionary<float, VectorRecord>();
        foreach(var record in records)
        {
            var vector = _dataIndex.Nodes[record.id].Vector;
            var distance = HSNWUtils.CosineSimilarity(queryVector, vector);
            results.Add(distance, record);
        }
        return results;
    }
}