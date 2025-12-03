using System;
using System.Collections.Generic;
using System.IO;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Datahandling;

/// <summary>
/// Class to load data from a text file
/// </summary>
public class DataLoader : IDataLoader
{
    public string[] LoadDataFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file at path {filePath} was not found.");
        }

        return File.ReadAllLines(filePath);
    }

}