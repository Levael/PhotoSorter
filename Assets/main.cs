using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System;
using System.IO;

public class main : MonoBehaviour
{
    public Config config;

    private string _configFilePath;
    private VisualElement _uiRoot;


    void Awake()
    {
        _uiRoot = GetComponent<UIDocument>().rootVisualElement;
        _uiRoot.Q<VisualElement>("close-app-btn").RegisterCallback<ClickEvent>(evt => { Application.Quit(); });

        _configFilePath = "Assets/last_selected_folders.json";
        config = new();
        config = ReadConfig(_configFilePath);
    }

    void Start()
    {
        UnityEngine.Debug.Log(config);
        UnityEngine.Debug.Log(config.DestinationFolderForKey_0);
    }

    void Update()
    {
        
    }

    void OnApplicationQuit()
    {
        UnityEngine.Debug.Log("exited");
        WriteConfig(_configFilePath, config);
    }

    private Config? ReadConfig(string filePath)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate,   // 'Populate' ensures missing JSON properties are initialized to default values
                NullValueHandling = NullValueHandling.Include           // Including fields with null values in JSON
            };

            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(jsonString, settings) ?? new Config();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error reading or deserializing the file: {ex.Message}");
            return null;
        }
    }

    void WriteConfig(string filePath, Config config)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,               // Setting indents for improved readability
                NullValueHandling = NullValueHandling.Include   // Including fields with null values in JSON
            };

            string jsonString = JsonConvert.SerializeObject(config, settings);
            // add check for "null"
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error writing or serializing the file: {ex.Message}");
        }
    }

    void TriggerFlash(VisualElement target, string className)
    {
        StartCoroutine(FlashGreenRoutine(target, className));
    }

    IEnumerator FlashGreenRoutine(VisualElement target, string className)
    {
        // className: "good-status" or "bad-status"

        target.AddToClassList(className);
        yield return new WaitForSeconds(0.5f);
        target.RemoveFromClassList(className);
    }

}



public class Config
{
    public string FolderToSort { get; set; }

    public string DestinationFolderForKey_1 { get; set; }
    public string DestinationFolderForKey_2 { get; set; }
    public string DestinationFolderForKey_3 { get; set; }
    public string DestinationFolderForKey_4 { get; set; }
    public string DestinationFolderForKey_5 { get; set; }
    public string DestinationFolderForKey_6 { get; set; }
    public string DestinationFolderForKey_7 { get; set; }
    public string DestinationFolderForKey_8 { get; set; }
    public string DestinationFolderForKey_9 { get; set; }
    public string DestinationFolderForKey_0 { get; set; }
}

