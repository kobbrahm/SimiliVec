using System;
using System.Collections.Generic;
using System.IO.Pipelines;

public class FakeFrontEnd
{
    private readonly VectorService _vectorService;

    public FakeFrontEnd(VectorService vectorService)
    {
        _vectorService = vectorService;
    }

    public void GetAnswers()
    {
        Console.WriteLine("Indexing documents...");
        _vectorService.IndexDocument();
        Console.Write("Search: ");
        int k = 10;
        string? userSearch = Console.ReadLine();
        if(!string.IsNullOrEmpty(userSearch))
        {
           var results = _vectorService.Search(userSearch, k);
           var withDistances = _vectorService.GetDistances(userSearch, results);
           
           foreach(var result in withDistances)
            {
                Console.WriteLine($"ID: {result.Value.id} : {result.Value.OriginalText} : {result.Key}");
            }
        }
        
    }
}