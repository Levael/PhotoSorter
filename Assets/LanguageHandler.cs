using Newtonsoft.Json;
using System.Collections.Generic;

public class LanguageHandler
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
    
    public string darkThemeBtnTooltip;
    public string lightThemeBtnTooltip;
    public string ruLangBtnTooltip;
    public string enLangBtnTooltip;
    
    public string fileNameLabel;
    public string filePathLabel;
    public string fileSizeLabel;


    [JsonIgnore]
    public Dictionary<string, string> uiTextElementNameToFieldNameMap = new()
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

        {"out-of-files-warning-message", "noFilesWarning"},
        {"error-while-opening-file-message", "failedToLoadFileError"},
        {"error-while-moving-file-message", "failedToMoveFileError"},
    };

    [JsonIgnore]
    public Dictionary<string, string> uiTooltipElementNameToFieldNameMap = new()
    {
        {"dark-theme-btn", "darkThemeBtnTooltip"},
        {"light-theme-btn", "lightThemeBtnTooltip"},
        {"ru-lang-btn", "ruLangBtnTooltip"},
        {"en-lang-btn", "enLangBtnTooltip"}
    };

}