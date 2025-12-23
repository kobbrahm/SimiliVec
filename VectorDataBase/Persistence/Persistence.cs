using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Modular persistence layer for saving and loading data in JSON format.
/// Supports generic types including dictionaries, enumerables, and complex objects.
/// </summary>
public static class Persistence
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Load data of any type from a JSON file
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <param name="path">Directory path where the file is located</param>
    /// <param name="fileName">Name of the file to load</param>
    /// <returns>Deserialized data of type T, or default if file doesn't exist</returns>
    public static T? LoadData<T>(string path, string fileName)
    {
        string filePath = Path.Combine(path, fileName);

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Persistence.LoadData: File not found at {filePath}. Returning default.");
            return default;
        }

        try
        {
            var jsonString = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<T>(jsonString, JsonOptions);
            Console.WriteLine($"Persistence.LoadData: Successfully loaded {fileName}");
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Persistence.LoadData: Error loading {fileName}: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Save data of any type to a JSON file
    /// </summary>
    /// <typeparam name="T">The type of data to serialize</typeparam>
    /// <param name="path">Directory path where the file will be saved</param>
    /// <param name="fileName">Name of the file to save</param>
    /// <param name="data">The data to serialize and save</param>
    /// <returns>True if successful, false otherwise</returns>
    public static bool SaveData<T>(string path, string fileName, T data)
    {
        string filePath = Path.Combine(path, fileName);
        if(!File.Exists(filePath))
        {
            EnsureDirectoryExists(path);
            File.Create(filePath).Close();
        }

        try
        {
            EnsureDirectoryExists(path);
            var jsonString = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filePath, jsonString);
            Console.WriteLine($"Persistence.SaveData: Successfully saved {fileName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Persistence.SaveData: Error saving {fileName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load collection data from a JSON file with fallback to empty collection
    /// </summary>
    /// <typeparam name="T">The collection element type</typeparam>
    /// <param name="path">Directory path where the file is located</param>
    /// <param name="fileName">Name of the file to load</param>
    /// <returns>Deserialized collection, or empty collection if not found</returns>
    public static IEnumerable<T> LoadCollection<T>(string path, string fileName)
    {
        var data = LoadData<IEnumerable<T>>(path, fileName);
        return data ?? new List<T>();
    }

    /// <summary>
    /// Check if a data file exists
    /// </summary>
    /// <param name="path">Directory path</param>
    /// <param name="fileName">Name of the file</param>
    /// <returns>True if file exists, false otherwise</returns>
    public static bool DataExists(string path, string fileName)
    {
        string filePath = Path.Combine(path, fileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Delete a data file if it exists
    /// </summary>
    /// <param name="path">Directory path</param>
    /// <param name="fileName">Name of the file</param>
    /// <returns>True if successful, false otherwise</returns>
    public static bool DeleteData(string path, string fileName)
    {
        string filePath = Path.Combine(path, fileName);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Persistence.DeleteData: Successfully deleted {fileName}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Persistence.DeleteData: Error deleting {fileName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ensure that the specified directory exists; creates it if necessary
    /// </summary>
    /// <param name="path">Directory path to ensure exists</param>
    private static void EnsureDirectoryExists(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine($"Persistence.EnsureDirectoryExists: Created directory {path}");
        }
    }
}
