using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using SFB;

public class Main : MonoBehaviour
{
    private ConfigHandler configHandler;
    private LanguageHandler languageHandler;
    private ImagesHandler imagesHandler;
    private UiHandler uiHandler;

    public string currentFileOriginFullPath;        // updates from imagesHandler.ShowImage
    public string currentFileDestinationFullPath;   // updates from main.ProcessMoveFileCommand
    public string lastFileOriginFullPath;
    public string lastFileDestinationFullPath;
    
    
    private Dictionary<int, string> keyCodeToFolderPath;
    private Dictionary<string, string> uiNameToFolderPath;
    private Dictionary<int, Action<int>> keyCodeToFunctionMap;



    void Awake()
    {
        configHandler = GetComponent<ConfigHandler>();
        imagesHandler = GetComponent<ImagesHandler>();
        languageHandler = GetComponent<LanguageHandler>();
        uiHandler = GetComponent<UiHandler>();
    }

    // MANDATORY STANDARD FUNCTIONALITY

    void Start()
    {
        Application.targetFrameRate = 30;   // no need in more
    }

    void Update() {}


    public void ProcessExitAppCommand(int keyCode)
    {
        Application.Quit();
    }

    public void ProcessSkipFileCommand(int keyCode)
    {
        imagesHandler.ShowNextImage();
    }

    public void ProcessUndoLastActionCommand(int keyCode)
    {
        if (String.IsNullOrEmpty(lastFileOriginFullPath) || String.IsNullOrEmpty(lastFileDestinationFullPath))
        {
            //Debug.Log("undo NOT successfully");
            return;
        }


        File.Move(lastFileDestinationFullPath, lastFileOriginFullPath);
        imagesHandler.ShowPreviousImage();

        lastFileOriginFullPath = null;
        lastFileDestinationFullPath = null;
    }

    public void ProcessMoveFileCommand(int keyCode)
    {
        try
        {
            var destinationFolderPath = configHandler.GetFolderPathByKeyCode(keyCode);

            if (String.IsNullOrEmpty(destinationFolderPath) || String.IsNullOrEmpty(currentFileOriginFullPath))
            {
                uiHandler.FlashFolderNumber(configHandler.GetFolderUiNameByKeyCode(keyCode), "warning-status");
                return;
            }

            currentFileDestinationFullPath = Path.Combine(destinationFolderPath, Path.GetFileName(currentFileOriginFullPath));


            Debug.Log($"Moving from: {currentFileOriginFullPath}");
            Debug.Log($"Moving to: {currentFileDestinationFullPath}");
            FilesHandler.MoveFile(currentFileOriginFullPath, currentFileDestinationFullPath);

            lastFileOriginFullPath = currentFileOriginFullPath;
            lastFileDestinationFullPath = currentFileDestinationFullPath;

            uiHandler.FlashFolderNumber(configHandler.GetFolderUiNameByKeyCode(keyCode), "good-status");
            imagesHandler.ShowNextImage();
        }
        catch (Exception ex)
        {
            uiHandler.FlashFolderNumber(configHandler.GetFolderUiNameByKeyCode(keyCode), "bad-status");
            Debug.LogError($"Error while moving file from 'ProcessMoveFileCommand': {ex}");
        }
    }

    public void ProcessChooseSourceFolderCommand(ClickEvent clickEvent)
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

        configHandler.fields.sourceFolderFullName = selectedFolderPath;
        btnFolderPath.text = Path.GetDirectoryName(selectedFolderPath);
        btnFolderName.text = Path.GetFileName(selectedFolderPath);

        imagesHandler.imagesBlob.Clear();
        imagesHandler.ShowNextImage();
    }

    public void ProcessChooseDestinationFolderCommand(ClickEvent clickEvent)
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Choose folder", "", false);

        if (paths.Length <= 0)
        {
            return;
        }

        string selectedFolderPath = paths[0];

        if (selectedFolderPath == configHandler.fields.sourceFolderFullName)
        {
            Debug.LogWarning("destination folder can't be your source folder");
            return;
        }


        var clickedBtnName = (VisualElement)clickEvent.currentTarget;
        var folderNumber = int.Parse(clickedBtnName.Q<TextElement>(className: "folder-number").text);

        var btnFolderPath = clickedBtnName.Q<TextElement>(className: "folder-path-without-name");
        var btnFolderName = clickedBtnName.Q<TextElement>(className: "folder-name");

        configHandler.SetFolderPathByFolderNumber(folderNumber, selectedFolderPath);
        btnFolderPath.text = Path.GetDirectoryName(selectedFolderPath);
        btnFolderName.text = Path.GetFileName(selectedFolderPath);
    }
}
