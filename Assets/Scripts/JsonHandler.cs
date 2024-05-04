using Newtonsoft.Json;
using System;
using System.IO;

#nullable enable

public static class JsonHandler
{
    public static T? LoadDataFromJson<T>(string filePath, JsonSerializerSettings? settings = null) where T : new()
    {
        try
        {
            if (settings == null)
            {
                settings = new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }

            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(jsonString, settings) ?? new T();
        }
        catch
        {
            return default(T);
        }
    }


    
    /// <returns>status of execution: is it successful or not</returns>
    public static bool UpdateDataInJson<T>(string filePath, T config)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,               // Setting indents for improved readability
                NullValueHandling = NullValueHandling.Include   // Including fields with null values in JSON
            };

            string jsonString = JsonConvert.SerializeObject(config, settings);
            if (String.IsNullOrEmpty(jsonString) || jsonString == "null") throw new Exception();
            File.WriteAllText(filePath, jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}