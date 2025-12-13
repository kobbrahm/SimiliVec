using System.Collections.Generic;
using System.Threading.Tasks;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;

namespace SimiliVec.MAUI.ViewModels;

public class SearchPageViewModel
{
    private readonly IVectorService _vectorService;
    private IEnumerable<DocumentModel> _searchResults = new List<DocumentModel>();

    public IEnumerable<DocumentModel> SearchResults
    {
        get => _searchResults;
        set => _searchResults = value;
    }

    public SearchPageViewModel(IVectorService vectorService)
    {
        _vectorService = vectorService;
    }

    public async Task<IEnumerable<DocumentModel>> SearchAsync(string query, int k = 5)
    {
        SearchResults = await _vectorService.Search(query, k);
        return SearchResults;
    }

    public IEnumerable<DocumentModel> GetAllDocuments()
    {
        return _vectorService.GetAllDocuments();
    }
}
