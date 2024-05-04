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

    //private string currentSourceFolderPath;
    //private string currentPhotoName;
    //private string currentPhotoPathOnly;

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

        

        ChangeLanguage(configHandler.chosenLanguage);
        SetTheme(configHandler.chosenTheme);
        FillUiWithDataFromConfig();
        HideChildrenElements(console);

        if (configHandler.sourceFolderFullName != null)
        {
            currentSourceFolderPath = configHandler.sourceFolderFullName;
            uiRoot.RegisterCallback<GeometryChangedEvent>(CallOnceWhenUiIsReady);   // show image as soon as UI is ready
        }

        // temp
        //Debug.Log(languageHandler.fields["instruction_1"]);
    }

    void Update() {}


    public void ProcessExitAppCommand(int keyCode)
    {
        Application.Quit();
    }

    public void ProcessSkipFileCommand(int keyCode)
    {
        DisplayNextImage();
    }

    public void ProcessUndoLastActionCommand(int keyCode)
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

    public void ProcessMoveFileCommand(int keyCode)
    {
        try
        {
            var folderPath = configHandler.GetFolderPathByKeyCode(keyCode);

            if (String.IsNullOrEmpty(folderPath))
            {
                TriggerFlash(configHandler.GetFolderUiNameByKeyCode(keyCode), "warning-status");
                return;
            }

            MoveFileToFolder(folderPath);
            DisplayNextImage();
        }
        catch (Exception e)
        {
            TriggerFlash(configHandler.GetFolderUiNameByKeyCode(keyCode), "bad-status");
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

        configHandler.sourceFolderFullName = selectedFolderPath;
        btnFolderPath.text = Path.GetDirectoryName(selectedFolderPath);
        btnFolderName.text = Path.GetFileName(selectedFolderPath);

        currentSourceFolderPath = configHandler.sourceFolderFullName;
        imagesBlob.Clear();
        DisplayNextImage();
    }

    public void ProcessChooseDestinationFolderCommand(ClickEvent clickEvent)
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

        configHandler.SetFolderPathByFolderNumber(folderNumber, selectedFolderPath);
        btnFolderPath.text = Path.GetDirectoryName(selectedFolderPath);
        btnFolderName.text = Path.GetFileName(selectedFolderPath);
    }


    lastFileOriginFullPath = Path.Combine(currentPhotoPathOnly, currentPhotoName);
        lastFileDestinationFullPath = Path.Combine(folderPath, currentPhotoName);
        

            uiHandler.TriggerFlash(configHandler.GetFolderUiNameByFolderPath(folderPath), "good-status");
        
            uiHandler.TriggerFlash(configHandler.GetFolderUiNameByFolderPath(folderPath), "bad-status");
}
