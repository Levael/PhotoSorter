using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class LanguageHandler : MonoBehaviour
{
    private ConfigHandler configHandler;
    private UiHandler uiHandler;

    public LanguageHandler_Fields languageHandler_Fields;
    private Dictionary<string, string> pathsToLangFiles;

    public Dictionary<string, string> uiTextElementNameToFieldNameMap;
    public Dictionary<string, string> uiTooltipElementNameToFieldNameMap;


    void Awake()
    {
        configHandler = GetComponent<ConfigHandler>();
        uiHandler = GetComponent<UiHandler>();


        uiTextElementNameToFieldNameMap = new()
        {
            {"instructions-section-label", "instructionsAndSettingsLabel"},
            {"info-section-label", "infoAndConsoleLabel"},
            {"clear-choices-btn", "clearButton"},

            {"instruction_1", "instruction_1"},
            {"instruction_2", "instruction_2"},
            {"instruction_3", "instruction_3"},
            {"instruction_4", "instruction_4"},
            {"instruction_5", "instruction_5"},
            {"instruction_6", "instruction_6"},
            {"instruction_7", "instruction_7"},
            {"instruction_8", "instruction_8"},

            {"file-name-info-label", "fileNameLabel"},
            {"file-path-info-label", "filePathLabel"},
            {"file-size-info-label", "fileSizeLabel"},
            {"file-type-info-label", "fileTypeLabel"},

            {"out-of-files-warning-message", "noFilesWarning"},
            {"error-while-opening-file-message", "failedToLoadFileError"},
            {"error-while-moving-file-message", "failedToMoveFileError"},
            {"error-while-moving-dublicated-file-message", "suchFileAlreadyExistsError"},
        };

        uiTooltipElementNameToFieldNameMap = new()
        {
            {"dark-theme-btn", "darkThemeBtnTooltip"},
            {"light-theme-btn", "lightThemeBtnTooltip"},
            {"ru-lang-btn", "ruLangBtnTooltip"},
            {"en-lang-btn", "enLangBtnTooltip"}
        };

        pathsToLangFiles = new()
        {
            { "en", Path.Combine(Application.streamingAssetsPath, "en.json") },
            { "ru", Path.Combine(Application.streamingAssetsPath, "ru.json") },
        };
    }

    void Start()
    {
        Load(configHandler.fields.chosenLanguage);
        Apply();
    }


    // PUBLIC
    public void ChangeLanguage(string language)
    {
        configHandler.fields.chosenLanguage = language;
        Load(language);
        Apply();
    }



    // PRIVATE
    private void Load(string language)
    {
        var settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include
        };

        languageHandler_Fields = JsonHandler.LoadDataFromJson<LanguageHandler_Fields>(pathsToLangFiles[language], settings);
    }

    private void Apply()
    {
        var uiRoot = uiHandler.root;

        uiHandler.FillUiWithDataFromLanguage();


        if (configHandler.fields.chosenLanguage == "ru")
        {
            uiRoot.Q<TextElement>("ru-lang-btn").AddToClassList("chosenOption");
            uiRoot.Q<TextElement>("en-lang-btn").RemoveFromClassList("chosenOption");
        }

        if (configHandler.fields.chosenLanguage == "en")
        {
            uiRoot.Q<TextElement>("en-lang-btn").AddToClassList("chosenOption");
            uiRoot.Q<TextElement>("ru-lang-btn").RemoveFromClassList("chosenOption");
        }
    }

    
}

public class LanguageHandler_Fields
{
    public string instructionsAndSettingsLabel;
    public string infoAndConsoleLabel;
    public string clearButton;

    public string instruction_1;
    public string instruction_2;
    public string instruction_3;
    public string instruction_4;
    public string instruction_5;
    public string instruction_6;
    public string instruction_7;
    public string instruction_8;

    public string noFilesWarning;
    public string failedToLoadFileError;
    public string failedToMoveFileError;
    public string suchFileAlreadyExistsError;

    public string darkThemeBtnTooltip;
    public string lightThemeBtnTooltip;
    public string ruLangBtnTooltip;
    public string enLangBtnTooltip;

    public string fileNameLabel;
    public string filePathLabel;
    public string fileSizeLabel;
    public string fileTypeLabel;
}