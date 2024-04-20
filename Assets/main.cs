using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using SFB;
using System.Linq;

public class main : MonoBehaviour
{
    private Config config;
    private string currentFolderPath;
    private string currentPhotoPath;
    private Image uiImageElement;
    private Queue<string> imagesBlob;

    private string _configFilePath;
    private VisualElement _uiRoot;
    private Dictionary<int, string> _keyCodeToFolderPath;
    private Dictionary<string, string> _uiNameToFolderPath;

    


    void Awake()
    {
        _configFilePath = "Assets/last_selected_folders.json";
        config = new();
        config = ReadConfig(_configFilePath);

        imagesBlob = new();

        _keyCodeToFolderPath = new() {
            { 49, "DestinationFolderForKey_1" },
            { 50, "DestinationFolderForKey_2" },
            { 51, "DestinationFolderForKey_3" },
            { 52, "DestinationFolderForKey_4" },
            { 53, "DestinationFolderForKey_5" },
            { 54, "DestinationFolderForKey_6" },
            { 55, "DestinationFolderForKey_7" },
            { 56, "DestinationFolderForKey_8" },
            { 57, "DestinationFolderForKey_9" },
            { 48, "DestinationFolderForKey_0" },
        };

        _uiNameToFolderPath = new() {
            { "choose-sorting-folder-btn", "FolderToSort" },

            { "destination-folder-1-btn", "DestinationFolderForKey_1" },
            { "destination-folder-2-btn", "DestinationFolderForKey_2" },
            { "destination-folder-3-btn", "DestinationFolderForKey_3" },
            { "destination-folder-4-btn", "DestinationFolderForKey_4" },
            { "destination-folder-5-btn", "DestinationFolderForKey_5" },
            { "destination-folder-6-btn", "DestinationFolderForKey_6" },
            { "destination-folder-7-btn", "DestinationFolderForKey_7" },
            { "destination-folder-8-btn", "DestinationFolderForKey_8" },
            { "destination-folder-9-btn", "DestinationFolderForKey_9" },
            { "destination-folder-0-btn", "DestinationFolderForKey_0" },
        };

        _uiRoot = GetComponent<UIDocument>().rootVisualElement;
        _uiRoot.RegisterCallback<KeyDownEvent>(OnKeyDown);
        _uiRoot.focusable = true;
        _uiRoot.Focus();

        _uiRoot.Q<VisualElement>("close-app-btn").RegisterCallback<ClickEvent>(evt => { Application.Quit(); });

        foreach (var item in _uiNameToFolderPath)
        {
            _uiRoot.Q<VisualElement>(item.Key).RegisterCallback<ClickEvent>(ChoosePathForFolder);
        }

        uiImageElement = _uiRoot.Q<Image>("image");
    }

    void Start()
    {
        UpdateUi();

        if (config.FolderToSort != null)
        {
            currentFolderPath = config.FolderToSort;
            DisplayNextImage();
        }
        
    }

    void Update()
    {
        
    }

    void OnApplicationQuit()
    {
        WriteConfig(_configFilePath, config);
    }


    private void ChoosePathForFolder(ClickEvent evt)
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Choose folder", "", false);

        if (paths.Length <= 0)
        {
            Debug.LogError("You didn't select any folder");
            return;
        }
        
        string selectedFolderPath = paths[0];
        //Debug.Log("Selected folder: " + selectedFolderPath);

        var clickedBtnName = ((VisualElement)evt.currentTarget).name;
        
        var configName = _uiNameToFolderPath[clickedBtnName];
        UpdateConfigObject(config, configName, selectedFolderPath);

        UpdateBtnText(clickedBtnName, selectedFolderPath);

        if (clickedBtnName == "choose-sorting-folder-btn")
        {
            currentFolderPath = config.FolderToSort;
            DisplayNextImage();
        }
    }

    private void UpdateBtnText(string clickedBtnName, string text)
    {
        var btnText = _uiRoot.Q<VisualElement>(clickedBtnName).Q<TextElement>();    // first of them
        btnText.text = text;
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

    // still somethimes writes "null" into file
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
            if (String.IsNullOrEmpty(jsonString) || jsonString == "null") throw new Exception();
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

    private void OnKeyDown(KeyDownEvent evt)
    {
        var keyCode = (int)evt.keyCode;

        if (keyCode == 0) return;

        try
        {
            var configFieldName = _keyCodeToFolderPath[keyCode];
            Debug.Log($"Key: {keyCode}. Folder path: {configFieldName}");
            MoveFileToFolder(configFieldName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    private void MoveFileToFolder(string configFieldName)
    {
        var folderPath = GetFieldValue(config, configFieldName).ToString();

        if (folderPath == null)
        {
            Debug.Log("not binded yet");
            return;
        }

        // move file
        // load next img
        Debug.Log(folderPath);
    }

    private void UpdateUi()
    {
        foreach (var item in _uiNameToFolderPath)
        {
            var folderPath = (string)GetFieldValue(config, item.Value);
            if (!String.IsNullOrEmpty(folderPath))
            {
                UpdateBtnText(item.Key, folderPath);
            }
        }
    }



    private void UpdateConfigObject(Config config, string fieldName, string newValue)
    {
        FieldInfo fieldInfo = config.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            fieldInfo.SetValue(config, newValue);
        }
        else
        {
            Debug.LogError($"Field '{fieldName}' not found in Config class.");
        }
    }

    private object GetFieldValue(object obj, string fieldName)
    {
        Type type = obj.GetType();
        FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(obj);
        }
        else
        {
            Debug.LogError("no such field");
            return null;
        }
    }

    private void LoadBunchOfImages(string folderPath)
    {
        Debug.Log($"LoadBunchOfImages: {folderPath}");

        string[] imageExtensions = new string[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };
        imagesBlob = new Queue<string>(imageExtensions.SelectMany(ext => Directory.GetFiles(folderPath, ext)).Take(10));
    }

    private void DisplayNextImage()
    {
        if (imagesBlob.Count == 0)
        {
            LoadBunchOfImages(currentFolderPath);
        }

        currentPhotoPath = imagesBlob.Dequeue();
        Debug.Log($"currentPhotoPath: {currentPhotoPath}");


        Texture2D texture = LoadTextureFromFile(currentPhotoPath);
        Sprite sprite = CreateSpriteFromTexture(texture);

        if (sprite != null && uiImageElement != null)
        {
            uiImageElement.sprite = sprite;
        }
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        else
        {
            Debug.LogError("Failed to load image: " + filePath);
            return null;
        }
    }

    private Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        if (texture == null) return null;

        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}





public class Config
{
    public string FolderToSort;

    public string DestinationFolderForKey_1;
    public string DestinationFolderForKey_2;
    public string DestinationFolderForKey_3;
    public string DestinationFolderForKey_4;
    public string DestinationFolderForKey_5;
    public string DestinationFolderForKey_6;
    public string DestinationFolderForKey_7;
    public string DestinationFolderForKey_8;
    public string DestinationFolderForKey_9;
    public string DestinationFolderForKey_0;
}