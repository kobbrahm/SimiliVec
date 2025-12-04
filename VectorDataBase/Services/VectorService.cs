using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using Microsoft.VisualBasic;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Datahandling;
using System.ComponentModel;
using System.Reflection.Metadata;
using VectorDataBase.Utils;
using System.Runtime.CompilerServices;

namespace VectorDataBase.Services;

public class VectorService
{
    private readonly IDataIndex _dataIndex;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly Dictionary<string, DocumentModel> _documentStorage = new Dictionary<string, DocumentModel>();
    private readonly Dictionary<int, string> _indexToDocumentMap = new Dictionary<int, string>();

    private static int _currentId = 0;
    private void nextId() => _currentId++;
    private readonly Random _random = new Random();
    public VectorService(IDataIndex dataIndex, IEmbeddingModel embeddingModel, IDataLoader dataLoader)
    {
        _dataIndex = dataIndex;
        _embeddingModel = embeddingModel;
    }

    /// <summary>
    /// Index documents from a text file
    /// </summary>
    /// <returns></returns>
    public void IndexDocument(IEnumerable<DocumentModel> documents)
    {
        foreach(var document in documents)
        {
            if(!_documentStorage.ContainsKey(document.Id))
            {
                _documentStorage.Add(document.Id, document);
            }
            string[] chunks = SimpleTextChunker.Chunk(document.Content, maxChunkSize: 500);

            foreach(var chunkText in chunks)
            {
                float[] vector = _embeddingModel.GetEmbeddings(chunkText);
                var nodeId = _currentId;
                var node = new HsnwNode { id = nodeId, Vector = vector };
                _dataIndex.Insert(node, _random);
                _indexToDocumentMap.Add(nodeId, document.Id);
                nextId();
            }

        }

        
    }

    public IEnumerable<DocumentModel> Search(string query, int k)
    {
        float[] queryVector = _embeddingModel.GetEmbeddings(query);
        var nearestVectorId = _dataIndex.FindNearestNeighbors(queryVector, k);
        var foundDocumentIds = new HashSet<string>();
        var results = new List<DocumentModel>();

        foreach(var vectorId in nearestVectorId)
        {
            if(_indexToDocumentMap.TryGetValue(vectorId, out string? documentId))
            {
                if(foundDocumentIds.Add(documentId))
                {
                    if(_documentStorage.TryGetValue(documentId, out DocumentModel? fullDocument))
                    {
                        var vector = _dataIndex.Nodes[vectorId].Vector;
                        fullDocument.Distance = HSNWUtils.CosineSimilarity(queryVector, vector);
                        results.Add(fullDocument);
                    }
                }
            }
        }
        return results;
    }

}