using Microsoft.AspNetCore.Mvc;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;
using System.Collections.ObjectModel;

[ApiController]
[Route("api/[controller]")]
public class VectorController : ControllerBase
{
    private readonly VectorService _vectorService;
    public VectorController(VectorService vectorService)
    {
        _vectorService = vectorService;
    }
    
    [HttpPost("index")]
    public async Task<IActionResult> IndexDocument([FromBody] DocumentModel[] documents)
    {
        if(documents == null || documents.Length == 0)
        {
            return BadRequest("No documents provided for indexing.");
        }

       await _vectorService.IndexDocument(documents);
        return Ok($"Indexed {documents.Length} documents");
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] string query, [FromQuery] int k = 5)
    {
        if(string.IsNullOrEmpty(query))
        {
            return BadRequest("Search query can not be empty");
        }

        var results = _vectorService.Search(query, k);
        return Ok(results);
    }
}