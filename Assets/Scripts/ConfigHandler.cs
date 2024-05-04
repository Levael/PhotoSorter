using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class ConfigHandler_Fields
{
    public string chosenLanguage = "en";    // default value

    public string chosenTheme = "dark";     // default value

    public string sourceFolderFullName;

    public string[] destinationFolderFullNames = new string[10];
}



public class ConfigHandler : MonoBehaviour
{
    private ConfigHandler_Fields configHandler_Fields;
    private string _defaultConfigFilePath;
    private string _configFilePath;

    public string chosenLanguage;
    public string chosenTheme;
    public string sourceFolderFullName;

    private string[] destinationFolderFullNames;
    private string[] destinationFolderUiNames;
    private Dictionary<int, int> keyCodeToFolderNumberMap;


    void Awake()
    {
        _defaultConfigFilePath = Path.Combine(Application.streamingAssetsPath, "defaultConfig.json");
        _configFilePath = Path.Combine(Application.persistentDataPath, "config.json");

        Load();



        destinationFolderUiNames = new string[10];

        keyCodeToFolderNumberMap = new()
        {
            { 49, 1 }, { 50, 2 }, { 51, 3 }, { 52, 4 }, { 53, 5 }, { 54, 6 }, { 55, 7 }, { 56, 8 }, { 57, 9 }, { 48, 0 }
        };
    }

    void OnApplicationQuit()
    {
        var isDoneSuccessfully = ReWrite();

        if (!isDoneSuccessfully)
            Debug.LogError("Couldn't update config file on App exit");
    }




    private void Load()
    {
        if (File.Exists(_configFilePath))
        {
            //Debug.Log("Loading config from Persistent Data Path.");
            configHandler_Fields = JsonHandler.LoadDataFromJson<ConfigHandler_Fields>(_configFilePath);
        }
        else
        {
            //Debug.Log("Config not found in Persistent Data, loading from StreamingAssets.");
            configHandler_Fields = JsonHandler.LoadDataFromJson<ConfigHandler_Fields>(_defaultConfigFilePath);
        }

        ValidatePaths();
    }
    private bool ReWrite()
    {
        return JsonHandler.UpdateDataInJson(_configFilePath, configHandler_Fields);
    }

    public void ValidatePaths()
    {
        // source folder check
        if (!Directory.Exists(sourceFolderFullName))
            sourceFolderFullName = null;

        // destination folders check
        for (int i = 0; i < destinationFolderFullNames.Length; i++)
        {
            if (!Directory.Exists(destinationFolderFullNames[i]))
                destinationFolderFullNames[i] = null;
        }
    }

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
        if (!keyCodeToFolderNumberMap.ContainsKey(keyCode))
            return null;

        var folderNumber = keyCodeToFolderNumberMap[keyCode];
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
        var index = keyCodeToFolderNumberMap[keyCode];
        return destinationFolderUiNames[index];
    }

    public void ResetAllData()
    {
        sourceFolderFullName = null;
        destinationFolderFullNames = new string[10];
    }
}