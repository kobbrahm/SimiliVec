using System;
using System.Collections.Generic;

namespace VectorDataBase.Utils;

public static class SimpleTextChunker
{
    public static string[] Chunk(string text, int maxChunkSize)
    {
        var chunks = new List<string>();

        //Split by paragraph
        string[] paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        string currentChunk = "";
        foreach(var paragraph in paragraphs)
        {
            if((currentChunk.Length + paragraph.Length + 2) > maxChunkSize)
            {
                currentChunk += paragraph + "\n\n";
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(currentChunk))
                {
                    chunks.Add(currentChunk.Trim());
                }
                currentChunk = paragraph + "\n\n";
            }
        }
        if(!string.IsNullOrWhiteSpace(currentChunk))
        {
            chunks.Add(currentChunk.Trim());
        }
        return chunks.ToArray();
    }
}