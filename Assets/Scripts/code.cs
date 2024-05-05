using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using SFB;
using CustomDataStructures;
using static System.Runtime.CompilerServices.RuntimeHelpers;

#nullable enable

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
    
    /*private Dictionary<int, string> keyCodeToFolderPath;
    private Dictionary<string, string> uiNameToFolderPath;
    private Dictionary<int, Action<int>> keyCodeToFunctionMap;*/

    public InterlinkedCollection<SingleLineDataSet> interlinkedCollection;



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

        interlinkedCollection = new()
        {
            new SingleLineDataSet
            (
                folderNumber: null,
                keyCode: null,
                folderUiName: "source-folder",
                folderPath: configHandler.fields.sourceFolderFullName,
                mouseClickCallback: ProcessChooseSourceFolderCommand,
                keyboardPressCallback: null
            ),

            new SingleLineDataSet
            (
                folderNumber: 1,
                keyCode: 49,
                folderUiName: "estination-folder-1",
                folderPath: configHandler.fields.destinationFolderFullNames[1],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 2,
                keyCode: 50,
                folderUiName: "estination-folder-2",
                folderPath: configHandler.fields.destinationFolderFullNames[2],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 3,
                keyCode: 51,
                folderUiName: "estination-folder-3",
                folderPath: configHandler.fields.destinationFolderFullNames[3],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 4,
                keyCode: 52,
                folderUiName: "estination-folder-4",
                folderPath: configHandler.fields.destinationFolderFullNames[4],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 5,
                keyCode: 53,
                folderUiName: "estination-folder-5",
                folderPath: configHandler.fields.destinationFolderFullNames[5],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 6,
                keyCode: 54,
                folderUiName: "estination-folder-6",
                folderPath: configHandler.fields.destinationFolderFullNames[6],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 7,
                keyCode: 55,
                folderUiName: "estination-folder-7",
                folderPath: configHandler.fields.destinationFolderFullNames[7],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 8,
                keyCode: 56,
                folderUiName: "estination-folder-8",
                folderPath: configHandler.fields.destinationFolderFullNames[8],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 9,
                keyCode: 57,
                folderUiName: "estination-folder-9",
                folderPath: configHandler.fields.destinationFolderFullNames[9],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: 0,
                keyCode: 48,
                folderUiName: "estination-folder-0",
                folderPath: configHandler.fields.destinationFolderFullNames[0],
                mouseClickCallback: ProcessChooseDestinationFolderCommand,
                keyboardPressCallback: ProcessMoveFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: null,
                keyCode: 8,
                folderUiName: null,
                folderPath: null,
                mouseClickCallback: null,
                keyboardPressCallback: ProcessUndoLastActionCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: null,
                keyCode: 32,
                folderUiName: null,
                folderPath: null,
                mouseClickCallback: null,
                keyboardPressCallback: ProcessSkipFileCommand
            ),

            new SingleLineDataSet
            (
                folderNumber: null,
                keyCode: 27,
                folderUiName: null,
                folderPath: null,
                mouseClickCallback: null,
                keyboardPressCallback: ProcessExitAppCommand
            )
        };
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


            //Debug.Log($"Moving from: {currentFileOriginFullPath}");
            //Debug.Log($"Moving to: {currentFileDestinationFullPath}");
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


public class SingleLineDataSet
{
    [CanBeKey(false)]
    public int? folderNumber { get; set; }

    [CanBeKey(true)]
    public int? keyCode { get; set; }

    [CanBeKey(true)]
    public string? folderUiName { get; set; }

    [CanBeKey(false)]
    public string? folderPath { get; set; }

    [CanBeKey(false)]
    public Action<ClickEvent>? mouseClickCallback { get; set; }

    [CanBeKey(false)]
    public Action<int>? keyboardPressCallback { get; set; }


    public SingleLineDataSet() { }

    public SingleLineDataSet
    (
        int? folderNumber,
        int? keyCode,
        string? folderUiName,
        string? folderPath,
        Action<ClickEvent>? mouseClickCallback,
        Action<int>? keyboardPressCallback
    )
    {
        this.folderNumber = folderNumber;
        this.keyCode = keyCode;
        this.folderUiName = folderUiName;
        this.folderPath = folderPath;
        this.mouseClickCallback = mouseClickCallback;
        this.keyboardPressCallback = keyboardPressCallback;
    }
}