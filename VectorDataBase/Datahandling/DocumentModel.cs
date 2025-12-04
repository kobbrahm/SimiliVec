using System;
using System.Collections.Generic;
namespace VectorDataBase.Datahandling;
public class DocumentModel
{
    /// <summary>
    /// Unique user-provided ID for the original document
    /// </summary>
    public string Id {get; set;} = Guid.NewGuid().ToString();
    /// <summary>
    /// The full content of the document
    /// </summary>
    public string Content {get; set;} = string.Empty;
    /// <summary>
    /// Optional metadata (autor, date, source)
    /// </summary>
    public Dictionary<string, string> MetaData {get; set;} = new Dictionary<string, string>();
    
    public float Distance {get; set;}

}