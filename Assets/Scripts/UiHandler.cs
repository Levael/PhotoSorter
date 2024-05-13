using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable


public class UiHandler : MonoBehaviour
{
    private Main main;
    private LanguageHandler languageHandler;
    private ThemeHandler themeHandler;
    private ConfigHandler configHandler;
    private ImagesHandler imagesHandler;



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
    public List<VisualElement> destinationFolders;

    private VisualElement console;
    private TextElement consoleWarning_outOfFiles;
    private TextElement consoleError_openFile;
    private TextElement consoleError_moveFile;

    


    void Awake()
    {
        main = GetComponent<Main>();
        languageHandler = GetComponent<LanguageHandler>();
        themeHandler = GetComponent<ThemeHandler>();
        configHandler = GetComponent<ConfigHandler>();
        imagesHandler = GetComponent<ImagesHandler>();


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

        chooseLangBtn_ru.RegisterCallback<ClickEvent>(evt => { languageHandler.ChangeLanguage("ru"); });
        chooseLangBtn_en.RegisterCallback<ClickEvent>(evt => { languageHandler.ChangeLanguage("en"); });
        chooseThemeBtn_dark.RegisterCallback<ClickEvent>(evt => { themeHandler.SetTheme("dark"); });
        chooseThemeBtn_light.RegisterCallback<ClickEvent>(evt => { themeHandler.SetTheme("light"); });
        clearChoicesBtn.RegisterCallback<ClickEvent>(evt => { ClearChoices(); });

        sourceFolder.RegisterCallback<ClickEvent>(main.ProcessChooseSourceFolderCommand);
        destinationFolders.ForEach(element => { element.RegisterCallback<ClickEvent>(main.ProcessChooseDestinationFolderCommand); });
    }

    void Start()
    {
        FillUiWithDataFromConfig();
        ClearConsole();

        if (configHandler.fields.sourceFolderFullName != null)
        {
            root.RegisterCallback<GeometryChangedEvent>(CallOnceWhenUiIsReady);   // show image as soon as UI is ready
        }
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
        imagesHandler.ClearImageAndItsData();
    }

    public void FlashFolderNumber(string folderUiName, string className)
    {
        var folderBtn = root.Q<VisualElement>(folderUiName);
        var target = folderBtn.Q<TextElement>(className: "folder-number");

        StartCoroutine(FlashCoroutine(target, className));
    }

    IEnumerator FlashCoroutine(VisualElement target, string className)
    {
        // className: "warning-status", "good-status" or "bad-status"

        target.AddToClassList(className);
        yield return new WaitForSeconds(0.5f);
        target.RemoveFromClassList(className);
    }

    public void PrintWarning_OutOfFiles()
    {
        ClearConsole();
        console.Q<TextElement>("out-of-files-warning-message").RemoveFromClassList("hiddenElement");
    }

    public void PrintError_OpenFile()
    {
        ClearConsole();
        console.Q<TextElement>("error-while-opening-file-message").RemoveFromClassList("hiddenElement");
    }

    public void PrintError_MoveFile()
    {
        ClearConsole();
        console.Q<TextElement>("error-while-moving-file-message").RemoveFromClassList("hiddenElement");
    }

    public void PrintError_MoveDuplicatedFile()
    {
        ClearConsole();
        console.Q<TextElement>("error-while-moving-dublicated-file-message").RemoveFromClassList("hiddenElement");
    }


    public void ClearConsole()
    {
        foreach (var child in console.Children())
        {
            child.AddToClassList("hiddenElement");
        }
    }

    public void CallOnceWhenUiIsReady(GeometryChangedEvent evt)
    {
        imagesHandler.ShowNextImage();
        root.UnregisterCallback<GeometryChangedEvent>(CallOnceWhenUiIsReady);
    }


    // SOOOO dirty, refactor later
    private void FillUiWithDataFromConfig()
    {
        // source folder
        {
            var btnFolderElem = root.Q<VisualElement>("source-folder");
            var btnFolderPathElem = btnFolderElem.Q<TextElement>(className: "folder-path-without-name");
            var btnFolderNameElem = btnFolderElem.Q<TextElement>(className: "folder-name");

            var folderFullPath = configHandler.fields.sourceFolderFullName;


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
        foreach (var folderBtn in destinationFolders)
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

        var fieldInfo = typeof(LanguageHandler_Fields).GetField(fieldName);
        if (fieldInfo == null)
        {
            Debug.Log($"Field not found for name: {fieldName}");
            return;
        }

        string localizedValue = fieldInfo.GetValue(languageHandler.languageHandler_Fields) as string;
        var uiElement = root.Q<VisualElement>(uiElementKey);
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