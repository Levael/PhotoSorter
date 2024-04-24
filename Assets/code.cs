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

public class Main : MonoBehaviour
{
    private Config config;
    private string currentFolderPath;
    private string currentPhotoFullPath;
    private Image uiBackgroundImageElement;
    private Image uiMainImageElement;
    private Queue<string> imagesBlob = new();

    private string _configFilePath = "Assets/last_selected_folders.json";
    private VisualElement _uiRoot;
    private Dictionary<int, string> _keyCodeToFolderPath;
    private Dictionary<string, string> _uiNameToFolderPath;



    void Awake()
    {
        /*config = LoadDataFromJson(_configFilePath);

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
        };*/

        _uiRoot = GetComponent<UIDocument>().rootVisualElement;
        _uiRoot.RegisterCallback<KeyDownEvent>(OnKeyDown);
        _uiRoot.focusable = true;
        _uiRoot.Focus();

        _uiRoot.Q<VisualElement>("close-app-btn").RegisterCallback<ClickEvent>(evt => { Application.Quit(); });

        /*foreach (var item in _uiNameToFolderPath)
        {
            _uiRoot.Q<VisualElement>(item.Key).RegisterCallback<ClickEvent>(ChoosePathForFolder);
        }*/

        uiMainImageElement = _uiRoot.Q<Image>("main-image");
    }

    void Start()
    {
        /*UpdateUi();

        if (config.FolderToSort != null)
        {
            currentFolderPath = config.FolderToSort;
            DisplayNextImage();
        }*/
        StartCoroutine(InitializeUIAfterFrame());

        
    }

    void Update()
    {
    }

    void OnApplicationQuit()
    {
        //UpdateDataInJson(_configFilePath, config);
    }

    IEnumerator InitializeUIAfterFrame()
    {
        yield return null;
        SetupUI();
        SetupUI2();
    }

    void SetupUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var container = root.Q<VisualElement>("photo-section");
        var imageElement = container.Q<Image>("main-image");

        Texture2D texture = LoadTextureFromFile("D:\\Programming\\GitHub projects\\PhotoSorter\\Assets\\Images\\test.png");

        var containerWidth = container.resolvedStyle.width;
        var containerHeight = container.resolvedStyle.height;

        float imageAspectRatio = (float)texture.width / texture.height;
        float containerAspectRatio = containerWidth / containerHeight;

        float targetWidth, targetHeight;
        if (imageAspectRatio > containerAspectRatio)    // border: left-right
        {
            targetWidth = containerWidth;
            targetHeight = targetWidth / imageAspectRatio + 20;
        }
        else // border: top-bottom
        {
            targetHeight = containerHeight;
            targetWidth = targetHeight * imageAspectRatio + 20;
        }

        imageElement.style.width = targetWidth;
        imageElement.style.height = targetHeight;

        /*Debug.Log($"container: {container}");
        Debug.Log($"imageElement: {imageElement}");
        Debug.Log($"containerWidth: {containerWidth}");
        Debug.Log($"containerHeight: {containerHeight}");
        Debug.Log($"imageAspectRatio: {imageAspectRatio}");
        Debug.Log($"containerAspectRatio: {containerAspectRatio}");
        Debug.Log($"targetWidth: {targetWidth}");
        Debug.Log($"targetHeight: {targetHeight}");*/

        imageElement.image = texture;
    }

    void SetupUI2()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var container = root.Q<VisualElement>("photo-section");
        var imageElement = container.Q<Image>("background-image");

        Texture2D texture = LoadTextureFromFile("D:\\Programming\\GitHub projects\\PhotoSorter\\Assets\\Images\\test.png");


        imageElement.image = BlurImageViaResample(texture, 200);   // such a big number is on purpose 
    }

    private Texture2D BlurImageViaResample(Texture2D original, float downscaleFactor)
    {
        int downscaledWidth = Mathf.Max(1, (int)(original.width / downscaleFactor));
        int downscaledHeight = Mathf.Max(1, (int)(original.height / downscaleFactor));

        RenderTexture rt = RenderTexture.GetTemporary(downscaledWidth, downscaledHeight);
        rt.filterMode = FilterMode.Bilinear;

        Graphics.Blit(original, rt);

        RenderTexture rtUpscaled = RenderTexture.GetTemporary(original.width, original.height);
        rtUpscaled.filterMode = FilterMode.Bilinear;

        Graphics.Blit(rt, rtUpscaled);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rtUpscaled;
        Texture2D upscaledTexture = new Texture2D(original.width, original.height);
        upscaledTexture.ReadPixels(new Rect(0, 0, rtUpscaled.width, rtUpscaled.height), 0, 0);
        upscaledTexture.Apply();
        RenderTexture.active = previous;

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.ReleaseTemporary(rtUpscaled);

        return upscaledTexture;
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
            imagesBlob.Clear();
            DisplayNextImage();
        }
    }

    private void UpdateBtnText(string clickedBtnName, string text)
    {
        var btnText = _uiRoot.Q<VisualElement>(clickedBtnName).Q<TextElement>();    // first of them
        btnText.text = text;
    }

    private Config? LoadDataFromJson(string filePath)
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
    void UpdateDataInJson(string filePath, Config config)
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
        var destinationFolderPath = GetFieldValue(config, configFieldName).ToString();
        if (destinationFolderPath == null)
        {
            Debug.Log("not binded yet");
            return;
        }

        var photoNameOnly = Path.GetFileName(currentPhotoFullPath);
        var destinationFullPath = Path.Combine(destinationFolderPath, photoNameOnly);


        try
        {
            File.Move(currentPhotoFullPath, destinationFullPath);
            DisplayNextImage();
            Debug.Log($"moved");
        }
        catch (IOException ex)
        {
            Debug.Log($"not moved");
        }

        // move file
        // load next img
        //Debug.Log($"destinationFolderPath: {destinationFolderPath}, photoNameOnly: {photoNameOnly}, destinationFullPath: {destinationFullPath}");
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
        string[] imageExtensions = new string[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };
        imagesBlob = new Queue<string>(imageExtensions.SelectMany(ext => Directory.GetFiles(folderPath, ext)).Take(10));
    }

    private void DisplayNextImage()
    {
        if (imagesBlob.Count == 0)
        {
            LoadBunchOfImages(currentFolderPath);
        }

        // add try catch
        currentPhotoFullPath = imagesBlob.Dequeue();

        Texture2D texture = LoadTextureFromFile(currentPhotoFullPath);
        Sprite sprite = CreateSpriteFromTexture(texture);

        if (sprite != null && uiMainImageElement != null)
        {
            uiMainImageElement.sprite = sprite;
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

    private Texture2D GetBlurredTexture(Texture2D source, Material blurMaterial)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex, blurMaterial);

        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTex;
        result.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        result.Apply();

        RenderTexture.ReleaseTemporary(renderTex);
        RenderTexture.active = null;

        return result;
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


public class FoldersHandler
{
    public string sourceFolderFullName { get; set; }

    [JsonProperty]
    private string[] destinationFolderFullNames;

    private Dictionary<int, int> _keyCodeToFolderNumberMap;



    public FoldersHandler()
    {
        destinationFolderFullNames = new string[10];

        _keyCodeToFolderNumberMap = new() {
            { 49, 1 }, { 50, 2 }, { 51, 3 }, { 52, 4 }, { 53, 5 }, { 54, 6 }, { 55, 7 }, { 56, 8 }, { 57, 9 }, { 48, 0 }
        };
    }



    public string GetFolderPathByFolderNumber(int folderNumber)
    {
        if (folderNumber < 0 || folderNumber >= destinationFolderFullNames.Length)
            return null;

        return destinationFolderFullNames[folderNumber];
    }

    public bool SetFolderPathByFolderNumber(int folderNumber, string folderPath)
    {
        if (folderNumber < 0 || folderNumber >= destinationFolderFullNames.Length)
            return false;

        destinationFolderFullNames[folderNumber] = folderPath;
        return true;
    }



    public string GetFolderPathByKeyCode(int keyCode)
    {
        if (!_keyCodeToFolderNumberMap.ContainsKey(keyCode))
            return null;

        var folderNumber = _keyCodeToFolderNumberMap[keyCode];
        var destinationFolderPath = destinationFolderFullNames[folderNumber];

        return destinationFolderPath;
    }
}