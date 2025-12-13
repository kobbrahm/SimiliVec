using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;

public interface IVectorService
{
    Task IndexDocument();
    Task<IEnumerable<DocumentModel>> Search(string query, int k = 5);
    IEnumerable<DocumentModel> GetAllDocuments();
}