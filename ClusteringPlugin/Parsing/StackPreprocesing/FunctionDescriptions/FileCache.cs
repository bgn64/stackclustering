using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

public class FileCache
{
    private readonly ConcurrentDictionary<string, string> cache;
    private readonly string cacheFilePath;

    public FileCache(string cacheFilePath)
    {
        this.cacheFilePath = cacheFilePath;
        this.cache = LoadCacheFromFile(cacheFilePath);
    }

    private ConcurrentDictionary<string, string> LoadCacheFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(json) ?? new ConcurrentDictionary<string, string>();
            }
            catch
            {
                // If there's an issue with deserialization, return a new empty cache
                return new ConcurrentDictionary<string, string>();
            }
        }
        else
        {
            // Create the file if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
            return new ConcurrentDictionary<string, string>();
        }
    }

    private void SaveCacheToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(cache);
            File.WriteAllText(cacheFilePath, json);
        }
        catch
        {
            // Handle any exceptions that might occur during serialization
        }
    }

    public string GetOrAdd(string key, Func<string, string> valueFactory)
    {
        if (cache.TryGetValue(key, out string? cachedResult))
        {
            return cachedResult;
        }

        string result = valueFactory(key);
        cache[key] = result;
        SaveCacheToFile();
        return result;
    }

    public string? Get(string key)
    {
        cache.TryGetValue(key, out string? cachedResult);
        return cachedResult;
    }

    public void Add(string key, string value)
    {
        cache[key] = value;
        SaveCacheToFile();
    }
}