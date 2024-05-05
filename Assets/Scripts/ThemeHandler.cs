using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ThemeHandler : MonoBehaviour
{
    private ConfigHandler configHandler;
    private UiHandler uiHandler;

    private StyleSheet mainStyle;
    private StyleSheet darkTheme;
    private StyleSheet lightTheme;

    private Dictionary<string, StyleSheet> themeNameToStyleSheetMap;



    void Awake()
    {
        configHandler = GetComponent<ConfigHandler>();
        uiHandler = GetComponent<UiHandler>();

        mainStyle = Resources.Load<StyleSheet>("style");
        darkTheme = Resources.Load<StyleSheet>("DarkTheme");
        lightTheme = Resources.Load<StyleSheet>("LightTheme");

        themeNameToStyleSheetMap = new()
        {
            { "dark", darkTheme},
            { "light", lightTheme}
        };
    }

    void Start()
    {
        Apply(configHandler.fields.chosenTheme);
    }


    // PUBLIC
    public void SetTheme(string theme)
    {
        configHandler.fields.chosenTheme = theme;
        Apply(theme);
    }


    // PRIVATE
    private void Apply(string theme)
    {
        var uiRoot = uiHandler.root;

        uiRoot.styleSheets.Clear();
        uiRoot.styleSheets.Add(mainStyle);
        uiRoot.styleSheets.Add(themeNameToStyleSheetMap[theme]);

        // todo: this ui part move to uiHandker
        if (theme == "light")
        {
            uiRoot.Q<VisualElement>("light-theme-btn").AddToClassList("chosenOption");
            uiRoot.Q<VisualElement>("dark-theme-btn").RemoveFromClassList("chosenOption");
        }
        else
        {
            uiRoot.Q<VisualElement>("dark-theme-btn").AddToClassList("chosenOption");
            uiRoot.Q<VisualElement>("light-theme-btn").RemoveFromClassList("chosenOption");
        }
    }
}