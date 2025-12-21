using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Datahandling;

/// <summary>
/// Class to load data from a text file
/// </summary>
using System.IO;
using System.Text.Json;

public class DataLoader : IDataLoader
{
    private readonly string _dataDirectory;
    private readonly string _dataFileName;
    private readonly string _fullFilePath;

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public DataLoader()
    {
        //Appdata folder
        _dataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _dataDirectory = Path.Combine(_dataDirectory, "SimiliVec");
        if(!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
        _dataFileName = "documents.json";
        _fullFilePath = Path.Combine(_dataDirectory, _dataFileName);
        if(!File.Exists(_fullFilePath))
        {
            File.Create(_fullFilePath).Close();
            string defaultJstring = "[{}]";
            File.WriteAllText(_fullFilePath,defaultJstring);
        }
    }

    /// <summary>
    /// Ensures that the specified directory exists; if not, creates it.
    /// </summary>
    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }


    /// <summary>
    /// Loads data from a text file
    /// </summary>
    /// <returns></returns>
    public IEnumerable<DocumentModel> LoadDataFromFile()
    {
        EnsureDirectoryExists(_fullFilePath);
        IEnumerable<DocumentModel> data = new List<DocumentModel>();
        if (File.Exists(_fullFilePath))
        {
            var jsonData = File.ReadAllText(_fullFilePath);
            data = JsonSerializer.Deserialize<IEnumerable<DocumentModel>>(jsonData, _jsonOptions) ?? new List<DocumentModel>();
            Console.WriteLine($"LoadDataFromFile: Loaded {data.Count()} documents from {_fullFilePath}");
        }
        else
        {
            Console.WriteLine($"LoadDataFromFile: File not found at {_fullFilePath}");
        }
        return data;
    }

    /// <summary>
    /// Saves data to a text file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void SaveDataToFile<T>(T data)
    {
        EnsureDirectoryExists(_dataDirectory);
        var jsonData = JsonSerializer.Serialize(data, _jsonOptions);
        try
        {
            File.WriteAllText(_fullFilePath, jsonData);
        }
        catch
        {
            Console.WriteLine("Failed to write data to file.");
        }
        
    }
    /// <summary>
    /// Loads all documents
    /// </summary>
    public IEnumerable<DocumentModel> LoadAllDocuments()
    {
        var documents = LoadDataFromFile();
        var docList = documents.ToList();
        Console.WriteLine($"LoadAllDocuments: Total {docList.Count} documents loaded");
        return docList;
    }
}