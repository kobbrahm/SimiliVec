using Microsoft.AspNetCore.Mvc;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;
using System.Collections.ObjectModel;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class VectorController : ControllerBase
{
    private readonly IVectorService _vectorService;
    public VectorController(IVectorService vectorService)
    {
        _vectorService = vectorService;
    }
    
    [HttpPost("index")]
    public async Task<IActionResult> IndexDocument()
    {
        var documents = _vectorService.GetAllDocuments();
        if(documents == null || !documents.Any())
        {
            return BadRequest("No documents provided for indexing.");
        }

       await _vectorService.IndexDocument();
        return Ok("Documents indexed successfully");
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] string query, [FromQuery] int k = 5)
    {
        if(string.IsNullOrEmpty(query))
        {
            return BadRequest("Search query can not be empty");
        }

        var results = await _vectorService.Search(query, k);
        return Ok(results);
    }
}