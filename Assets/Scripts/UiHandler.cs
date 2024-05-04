using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable


public class UiHandler : MonoBehaviour
{
    public VisualElement root;

    public VisualElement imagesContainer;
    public Image backgroundImage;
    public Image mainImage;

    public TextElement imageInfoElement_name;
    public TextElement imageInfoElement_path;
    public TextElement imageInfoElement_size;
    public TextElement imageInfoElement_type;

    private TextElement chooseLangBtn_ru;
    private TextElement chooseLangBtn_en;
    private VisualElement chooseThemeBtn_dark;
    private VisualElement chooseThemeBtn_light;
    private VisualElement clearChoicesBtn;
    private VisualElement closeAppBtn;

    private VisualElement sourceFolder;
    private List<VisualElement> destinationFolders;

    private VisualElement console;
    private TextElement consoleWarning_outOfFiles;
    private TextElement consoleError_openFile;
    private TextElement consoleError_moveFile;

    


    void Awake()
    {
        // ROOT
        root = GetComponent<UIDocument>().rootVisualElement;
        root.focusable = true;
        root.Focus();

        // IMAGES
        imagesContainer = root.Q<VisualElement>("photo-section");
        mainImage = imagesContainer.Q<Image>("main-image");
        backgroundImage = imagesContainer.Q<Image>("background-image");

        // BUTTONS
        chooseLangBtn_ru = root.Q<TextElement>("ru-lang-btn");
        chooseLangBtn_en = root.Q<TextElement>("en-lang-btn");
        chooseThemeBtn_dark = root.Q<VisualElement>("dark-theme-btn");
        chooseThemeBtn_light = root.Q<VisualElement>("light-theme-btn");
        clearChoicesBtn = root.Q<VisualElement>("clear-choices-btn-wrapper");
        closeAppBtn = root.Q<VisualElement>("close-app-btn");

        // FOLDERS
        sourceFolder = root.Q<VisualElement>("source-folder");
        destinationFolders = root.Query<VisualElement>(className: "destination-folder").ToList();

        // IMAGE INFO
        imageInfoElement_name = root.Q<TextElement>("file-name-info-data");
        imageInfoElement_path = root.Q<TextElement>("file-path-info-data");
        imageInfoElement_size = root.Q<TextElement>("file-size-info-data");
        imageInfoElement_type = root.Q<TextElement>("file-type-info-data");

        // CONSOLE
        console = root.Query<VisualElement>("console-messages");
        consoleWarning_outOfFiles = console.Q<TextElement>("out-of-files-warning-message");
        consoleError_openFile = console.Q<TextElement>("error-while-opening-file-message");
        consoleError_moveFile = console.Q<TextElement>("error-while-moving-file-message");



        // EVENT LISTENERS
        closeAppBtn.RegisterCallback<ClickEvent>(evt => { Application.Quit(); });

        chooseLangBtn_ru.RegisterCallback<ClickEvent>(evt => { ChangeLanguage("ru"); });
        chooseLangBtn_en.RegisterCallback<ClickEvent>(evt => { ChangeLanguage("en"); });
        chooseThemeBtn_dark.RegisterCallback<ClickEvent>(evt => { SetTheme("dark"); });
        chooseThemeBtn_light.RegisterCallback<ClickEvent>(evt => { SetTheme("light"); });
        clearChoicesBtn.RegisterCallback<ClickEvent>(evt => { ClearChoices(); });

        sourceFolder.RegisterCallback<ClickEvent>(ProcessChooseSourceFolderCommand);
        destinationFolders.ForEach(element => { element.RegisterCallback<ClickEvent>(ProcessChooseDestinationFolderCommand); });
    }

    void Start()
    {
        
    }


    public void FillImageInfo(string name, string path, string size, string extension)
    {
        imageInfoElement_name.text = name;
        imageInfoElement_path.text = path;
        imageInfoElement_size.text = size;
        imageInfoElement_type.text = extension;
    }



    private void ClearChoices()
    {
        configHandler.ResetAllData();
        FillUiWithDataFromConfig();
        ClearImage();
    }

    void TriggerFlash(string folderUiName, string className)
    {
        var folderBtn = root.Q<VisualElement>(folderUiName);
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

    public void PrintWarning_OutOfFiles()
    {
        ClearConsole(console);
        console.Q<TextElement>("out-of-files-warning-message").RemoveFromClassList("hiddenElement");
    }

    public void PrintError_OpenFile()
    {
        ClearConsole(console);
        console.Q<TextElement>("error-while-opening-file-message").RemoveFromClassList("hiddenElement");
    }

    public void PrintError_MoveFile()
    {
        ClearConsole(console);
        console.Q<TextElement>("error-while-moving-file-message").RemoveFromClassList("hiddenElement");
    }


    public void ClearConsole()
    {
        foreach (var child in console.Children())
        {
            child.AddToClassList("hiddenElement");
        }
    }

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
            var btnFolderElem = root.Q<VisualElement>("source-folder");
            var btnFolderPathElem = btnFolderElem.Q<TextElement>(className: "folder-path-without-name");
            var btnFolderNameElem = btnFolderElem.Q<TextElement>(className: "folder-name");

            var folderFullPath = configHandler.sourceFolderFullName;


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
            var folderFullPath = configHandler.GetFolderPathByFolderNumber(btnFolderNumber);


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

    public void FillUiWithDataFromLanguage()
    {
        foreach (var dataPair in languageHandler.uiTextElementNameToFieldNameMap)
        {
            UpdateUIElement(dataPair, "text");
        }

        foreach (var dataPair in languageHandler.uiTooltipElementNameToFieldNameMap)
        {
            UpdateUIElement(dataPair, "tooltip");
        }
    }

    private void UpdateUIElement(KeyValuePair<string, string> dataPair, string property)
    {
        string fieldName = dataPair.Value;
        string uiElementKey = dataPair.Key;

        var fieldInfo = typeof(LanguageHandler).GetField(fieldName);
        if (fieldInfo == null)
        {
            Debug.Log($"Field not found for name: {fieldName}");
            return;
        }

        string localizedValue = fieldInfo.GetValue(languageHandler) as string;
        var uiElement = uiRoot.Q<VisualElement>(uiElementKey);
        if (uiElement == null)
        {
            Debug.Log($"UI element not found for key: {uiElementKey}");
            return;
        }

        if (property == "text" && uiElement is TextElement textElement)
        {
            textElement.text = localizedValue;
            //print(textElement.text);
        }
        else if (property == "tooltip")
        {
            uiElement.tooltip = localizedValue;
            //print(uiElement.tooltip);
        }
    }
}









/*
         var folderNumber = int.Parse(element.Q<TextElement>(className: "folder-number").text);
            configHandler.SetFolderUiNameByFolderNumber(folderNumber, element.name);
         */

/*private void PrintWarning_OutOfFiles()
{
    ClearConsole(console);
    console.Q<TextElement>("out-of-files-warning-message").RemoveFromClassList("hiddenElement");
}

private void PrintError_OpenFile()
{
    ClearConsole(console);
    console.Q<TextElement>("error-while-opening-file-message").RemoveFromClassList("hiddenElement");
}

private void PrintError_MoveFile()
{
    ClearConsole(console);
    console.Q<TextElement>("error-while-moving-file-message").RemoveFromClassList("hiddenElement");
}*/


// links to images raleted ui elements