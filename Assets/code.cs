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
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

public class Main : MonoBehaviour
{
    //private Config config;
    private FoldersHandler foldersHandler;

    private string currentSourceFolderPath;
    private string currentPhotoName;
    private string currentPhotoPathOnly;

    private string lastFileOriginFullPath = null;
    private string lastFileDestinationFullPath = null;

    private VisualElement uiRoot;
    private VisualElement uiImagesContainerElement;
    private Image uiBackgroundImageElement;
    private Image uiMainImageElement;
    private TextElement uiImageNameElement;
    private TextElement uiImagePathElement;
    private TextElement uiImageSizeElement;

    private string _configFilePath = "Assets/last_selected_folders.json";
    private Queue<string> imagesBlob = new();
    private List<VisualElement> foldersBtns;
    private TextElement console;
    
    private Dictionary<int, string> keyCodeToFolderPath;
    private Dictionary<string, string> uiNameToFolderPath;
    private Dictionary<int, Action<int>> keyCodeToFunctionMap;



    void Awake()
    {
        // object for handling folders logic and json data
        foldersHandler = LoadDataFromJson(_configFilePath);

        // ui root settings
        uiRoot = GetComponent<UIDocument>().rootVisualElement;
        uiRoot.focusable = true;
        uiRoot.Focus();

        // links to images raleted ui elements
        uiImagesContainerElement = uiRoot.Q<VisualElement>("photo-section");
        uiMainImageElement = uiImagesContainerElement.Q<Image>("main-image");
        uiBackgroundImageElement = uiImagesContainerElement.Q<Image>("background-image");

        uiImageNameElement = uiRoot.Q<TextElement>("file-name-info-data");
        uiImagePathElement = uiRoot.Q<TextElement>("file-path-info-data");
        uiImageSizeElement = uiRoot.Q<TextElement>("file-size-info-data");

        foldersBtns = uiRoot.Query<VisualElement>(className: "destination-folder").ToList();
        console = uiRoot.Query<TextElement>("console-message-data");


        // event listeners
        uiRoot.RegisterCallback<KeyDownEvent>(KeyWasPressedEvent);
        uiRoot.Q<VisualElement>("close-app-btn").RegisterCallback<ClickEvent>(evt => { Application.Quit(); });
        uiRoot.Q<VisualElement>("clear-choices-btn-wrapper").RegisterCallback<ClickEvent>(evt => { ClearChoices(); });
        uiRoot.Q<VisualElement>("source-folder").RegisterCallback<ClickEvent>(ProcessChooseSourceFolderCommand);
        foldersBtns.ForEach((element) =>
        {
            var folderNumber = int.Parse(element.Q<TextElement>(className: "folder-number").text);
            foldersHandler.SetFolderUiNameByFolderNumber(folderNumber, element.name);
            element.RegisterCallback<ClickEvent>(ProcessChooseDestinationFolderCommand);
        });
        


        keyCodeToFunctionMap = new() {
            { 49, ProcessMoveFileCommand },         // 1
            { 50, ProcessMoveFileCommand },         // 2
            { 51, ProcessMoveFileCommand },         // 3
            { 52, ProcessMoveFileCommand },         // 4
            { 53, ProcessMoveFileCommand },         // 5
            { 54, ProcessMoveFileCommand },         // 6
            { 55, ProcessMoveFileCommand },         // 7
            { 56, ProcessMoveFileCommand },         // 8
            { 57, ProcessMoveFileCommand },         // 9
            { 48, ProcessMoveFileCommand },         // 0

            { 8,  ProcessUndoLastActionCommand },   // backspace
            { 32, ProcessSkipFileCommand },         // space
            { 27, ProcessExitAppCommand },          // esc
        };
    }

    // MANDATORY STANDARD FUNCTIONALITY

    void Start()
    {
        Application.targetFrameRate = 30;

        FillUiWithDataFromConfig();

        if (foldersHandler.sourceFolderFullName != null)
        {
            currentSourceFolderPath = foldersHandler.sourceFolderFullName;
            uiRoot.RegisterCallback<GeometryChangedEvent>(CallOnceWhenUiIsReady);   // show image as soon as UI is ready
        }
    }

    void Update() {}

    void OnApplicationQuit()
    {
        UpdateDataInJson(_configFilePath, foldersHandler);
    }


    // COMMAND PROCESSORS

    private void KeyWasPressedEvent(KeyDownEvent keyDownEvent)
    {
        var keyCode = (int)keyDownEvent.keyCode;

        if (keyCodeToFunctionMap.ContainsKey(keyCode))
            keyCodeToFunctionMap[keyCode].Invoke(keyCode);
    }

    private void ProcessExitAppCommand(int keyCode)
    {
        Application.Quit();
    }

    private void ProcessSkipFileCommand(int keyCode)
    {
        DisplayNextImage();
    }

    private void ProcessUndoLastActionCommand(int keyCode)
    {
        if (String.IsNullOrEmpty(lastFileOriginFullPath) || String.IsNullOrEmpty(lastFileDestinationFullPath))
        {
            //Debug.Log("undo NOT successfully");
            return;
        }

        
        File.Move(lastFileDestinationFullPath, lastFileOriginFullPath);
        DisplayPreviousImage();

        lastFileOriginFullPath = null;
        lastFileDestinationFullPath = null;

        //Debug.Log("undo successfully");
    }

    private void ProcessMoveFileCommand(int keyCode)
    {
        try
        {
            var folderPath = foldersHandler.GetFolderPathByKeyCode(keyCode);

            if (String.IsNullOrEmpty(folderPath))
            {
                TriggerFlash(foldersHandler.GetFolderUiNameByKeyCode(keyCode), "warning-status");
                return;
            }

            MoveFileToFolder(folderPath);
        }
        catch (Exception e)
        {
            TriggerFlash(foldersHandler.GetFolderUiNameByKeyCode(keyCode), "bad-status");
        }

        DisplayNextImage();

    }

    private void ProcessChooseSourceFolderCommand(ClickEvent clickEvent)
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Choose folder", "", false);

        if (paths.Length <= 0)
        {
            //Debug.LogError("You didn't select any folder");
            return;
        }

        string selectedFolderPath = paths[0];

        var clickedBtnName = (VisualElement)clickEvent.currentTarget;

        var btnFolderPath = clickedBtnName.Q<TextElement>(className: "folder-path-without-name");
        var btnFolderName = clickedBtnName.Q<TextElement>(className: "folder-name");

        foldersHandler.sourceFolderFullName = selectedFolderPath;
        btnFolderPath.text = Path.GetDirectoryName(selectedFolderPath);
        btnFolderName.text = Path.GetFileName(selectedFolderPath);

        currentSourceFolderPath = foldersHandler.sourceFolderFullName;
        imagesBlob.Clear();
        DisplayNextImage();
    }

    private void ProcessChooseDestinationFolderCommand(ClickEvent clickEvent)
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Choose folder", "", false);

        if (paths.Length <= 0)
        {
            return;
        }

        string selectedFolderPath = paths[0];

        var clickedBtnName = (VisualElement)clickEvent.currentTarget;
        var folderNumber = int.Parse(clickedBtnName.Q<TextElement>(className: "folder-number").text);

        var btnFolderPath = clickedBtnName.Q<TextElement>(className: "folder-path-without-name");
        var btnFolderName = clickedBtnName.Q<TextElement>(className: "folder-name");

        foldersHandler.SetFolderPathByFolderNumber(folderNumber, selectedFolderPath);
        btnFolderPath.text = Path.GetDirectoryName(selectedFolderPath);
        btnFolderName.text = Path.GetFileName(selectedFolderPath);
    }

    // UI RELATED

    private void CallOnceWhenUiIsReady(GeometryChangedEvent evt)
    {
        DisplayNextImage();
        uiRoot.UnregisterCallback<GeometryChangedEvent>(CallOnceWhenUiIsReady);
    }

    // SOOOO dirty, refactor later
    private void FillUiWithDataFromConfig()
    {
        // source folder
        {
            var btnFolderElem = uiRoot.Q<VisualElement>("source-folder");
            var btnFolderPathElem = btnFolderElem.Q<TextElement>(className: "folder-path-without-name");
            var btnFolderNameElem = btnFolderElem.Q<TextElement>(className: "folder-name");

            var folderFullPath = foldersHandler.sourceFolderFullName;


            if (!String.IsNullOrEmpty(folderFullPath))
            {
                btnFolderPathElem.text = Path.GetDirectoryName(folderFullPath);
                btnFolderNameElem.text = Path.GetFileName(folderFullPath);
            }
            else
            {
                btnFolderPathElem.text = "";
                btnFolderNameElem.text = "";
            }
        }


        // destination folders
        foreach (var folderBtn in foldersBtns)
        {
            var btnFolderNumberElem = folderBtn.Q<TextElement>(className: "folder-number");
            var btnFolderPathElem = folderBtn.Q<TextElement>(className: "folder-path-without-name");
            var btnFolderNameElem = folderBtn.Q<TextElement>(className: "folder-name");

            var btnFolderNumber = int.Parse(btnFolderNumberElem.text);
            var folderFullPath = foldersHandler.GetFolderPathByFolderNumber(btnFolderNumber);


            if (!String.IsNullOrEmpty(folderFullPath))
            {
                btnFolderPathElem.text = Path.GetDirectoryName(folderFullPath);
                btnFolderNameElem.text = Path.GetFileName(folderFullPath);
            }
            else
            {
                btnFolderPathElem.text = "";
                btnFolderNameElem.text = "";
            }
        }
    }

    private void ClearChoices()
    {
        foldersHandler.ResetAllData();
        FillUiWithDataFromConfig();
        ClearImage();
    }

    void TriggerFlash(string folderUiName, string className)
    {
        var folderBtn = uiRoot.Q<VisualElement>(folderUiName);
        var target = folderBtn.Q<TextElement>(className: "folder-number");

        // so dirty...
        //Debug.Log($"folderUiName: {folderUiName}\nfolderBtn: {folderBtn}\n");

        StartCoroutine(FlashGreenRoutine(target, className));
    }

    IEnumerator FlashGreenRoutine(VisualElement target, string className)
    {
        // className: "warning-status", "good-status" or "bad-status"

        target.AddToClassList(className);
        yield return new WaitForSeconds(0.5f);
        target.RemoveFromClassList(className);
    }

    private void PrintWarning(string message)
    {
        console.ClearClassList();
        console.AddToClassList("warning-status");
        console.text = message;
    }

    private void PrintError(string message)
    {
        console.ClearClassList();
        console.AddToClassList("bad-status");
        console.text = message;
    }

    private void ClearConsole()
    {
        console.ClearClassList();
        console.text = "";
    }



    // IMAGES RELATED

    private void DisplayNextImage()
    {
        if (imagesBlob.Count == 0)
        {
            LoadBunchOfImages(currentSourceFolderPath);
        }

        if (imagesBlob.Count == 0)
        {
            ClearImage();
            PrintWarning("Out of files");

            return;
        }

        // add try catch
        var currentPhotoFullPath = imagesBlob.Dequeue();

        // update current data
        currentPhotoPathOnly = Path.GetDirectoryName(currentPhotoFullPath);
        currentPhotoName = Path.GetFileName(currentPhotoFullPath);

        uiImageNameElement.text = currentPhotoName;
        uiImagePathElement.text = currentPhotoPathOnly;
        uiImageSizeElement.text = $"{(new FileInfo(currentPhotoFullPath).Length / 1024.0 / 1024.0):F2} MB";

        DisplayImage(currentPhotoFullPath);
        ClearConsole();
    }

    private void DisplayPreviousImage()
    {
        // add try catch
        var currentPhotoFullPath = lastFileOriginFullPath;

        // update current data
        currentPhotoPathOnly = Path.GetDirectoryName(currentPhotoFullPath);
        currentPhotoName = Path.GetFileName(currentPhotoFullPath);

        uiImageNameElement.text = currentPhotoName;
        uiImagePathElement.text = currentPhotoPathOnly;
        uiImageSizeElement.text = $"{(new FileInfo(currentPhotoFullPath).Length / 1024.0 / 1024.0):F2} MB";

        DisplayImage(currentPhotoFullPath);
        ClearConsole();
    }

    private void DisplayImage(string fileFullPath)
    {
        var texture = LoadTextureFromFile(fileFullPath);

        DisplayMainImage(texture);
        DisplayBackgroundImage(texture);
    }

    private void DisplayMainImage(Texture2D texture)
    {
        // eliminates small inconsistencies by several pixels when setting the dimensions
        texture.wrapMode = TextureWrapMode.Clamp;

        var containerWidth = uiImagesContainerElement.resolvedStyle.width;
        var containerHeight = uiImagesContainerElement.resolvedStyle.height;

        float imageAspectRatio = (float)texture.width / texture.height;
        float containerAspectRatio = containerWidth / containerHeight;

        float targetWidth, targetHeight;
        if (imageAspectRatio > containerAspectRatio)
        {
            // Horizontal Image

            targetWidth = containerWidth;
            targetHeight = targetWidth / imageAspectRatio + 20; // 10px from each border

            uiMainImageElement.RemoveFromClassList("image-border-left-right");
            uiMainImageElement.AddToClassList("image-border-up-down");
        }
        else
        {
            // Vertical Image

            targetHeight = containerHeight;
            targetWidth = targetHeight * imageAspectRatio + 20; // 10px from each border

            uiMainImageElement.RemoveFromClassList("image-border-up-down");
            uiMainImageElement.AddToClassList("image-border-left-right");
        }

        uiMainImageElement.style.width = targetWidth;
        uiMainImageElement.style.height = targetHeight;

        uiMainImageElement.image = texture;
    }

    private void DisplayBackgroundImage(Texture2D texture)
    {
        uiBackgroundImageElement.image = BlurImageViaResample(texture, 200);   // such a big number is on purpose 
    }

    private void ClearImage()
    {
        uiBackgroundImageElement.image = null;
        uiMainImageElement.image = null;

        uiMainImageElement.RemoveFromClassList("image-border-left-right");
        uiMainImageElement.RemoveFromClassList("image-border-up-down");

        uiImageNameElement.text = "";
        uiImagePathElement.text = "";
        uiImageSizeElement.text = "";

        // todo: upd current and prev data
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

    private void LoadBunchOfImages(string folderPath)
    {
        if (String.IsNullOrEmpty(folderPath)) return;

        string[] imageExtensions = new string[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };
        imagesBlob = new Queue<string>(imageExtensions.SelectMany(ext => Directory.GetFiles(folderPath, ext)).Take(100));
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
            PrintError($"Failed to load image: {filePath}");
            return null;
        }
    }



    // IMAGES MOOVING

    private void MoveFileToFolder(string folderPath)
    {
        if (String.IsNullOrEmpty(folderPath))
        {
            return;
        }

        lastFileOriginFullPath = Path.Combine(currentPhotoPathOnly, currentPhotoName);
        lastFileDestinationFullPath = Path.Combine(folderPath, currentPhotoName);


        try
        {
            File.Move(lastFileOriginFullPath, lastFileDestinationFullPath);

            TriggerFlash(foldersHandler.GetFolderUiNameByFolderPath(folderPath), "good-status");
        }
        catch (IOException ex)
        {
            TriggerFlash(foldersHandler.GetFolderUiNameByFolderPath(folderPath), "bad-status");
        }

    }

    private FoldersHandler? LoadDataFromJson(string filePath)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<FoldersHandler>(jsonString, settings) ?? new FoldersHandler();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error reading or deserializing the file: {ex.Message}");
            return null;
        }
    }

    // still somethimes writes "null" into file
    void UpdateDataInJson(string filePath, FoldersHandler config)
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


}




public class FoldersHandler
{
    [JsonProperty]
    public string chosenLanguage { get; set; } = "en";

    [JsonProperty]
    public string chosenTheme { get; set; } = "dark";

    [JsonProperty]
    public string sourceFolderFullName { get; set; }

    [JsonProperty]
    private string[] destinationFolderFullNames = new string[10];



    [JsonIgnore]
    private string[] destinationFolderUiNames = new string[10];

    [JsonIgnore]
    private Dictionary<int, int> _keyCodeToFolderNumberMap = new()
    {
        { 49, 1 }, { 50, 2 }, { 51, 3 }, { 52, 4 }, { 53, 5 }, { 54, 6 }, { 55, 7 }, { 56, 8 }, { 57, 9 }, { 48, 0 }
    };



    public void SetFolderUiNameByFolderNumber(int folderNumber, string folderUiName)
    {
        destinationFolderUiNames[folderNumber] = folderUiName;
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

    public bool SetFoldersUiNames(List<string> list)
    {
        try
        {
            destinationFolderUiNames = list.ToArray();

            var lastElement = destinationFolderUiNames[destinationFolderUiNames.Length - 1];
            for (int i = destinationFolderUiNames.Length - 1; i > 0; i--)
                destinationFolderUiNames[i] = destinationFolderUiNames[i - 1];
            destinationFolderUiNames[0] = lastElement;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetFolderBtnNameByFolderNumber(int folderNumber)
    {
        return destinationFolderUiNames[folderNumber];
    }

    public string GetFolderUiNameByFolderPath(string folderPath)
    {
        return destinationFolderUiNames[Array.IndexOf(destinationFolderFullNames, folderPath)];
    }

    public string GetFolderUiNameByKeyCode(int keyCode)
    {
        var index = _keyCodeToFolderNumberMap[keyCode];
        return destinationFolderUiNames[index];
    }

    public void ResetAllData()
    {
        sourceFolderFullName = null;
        destinationFolderFullNames = new string[10];
    }
}