using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour
{
    private Main main;
    private UiHandler uiHandler;

    //private Dictionary<int, Action<int>> keyCodeToFunctionMap;


    void Awake()
    {
        uiHandler = GetComponent<UiHandler>();
        main = GetComponent<Main>();


        /*keyCodeToFunctionMap = new() {
            { 49, main.ProcessMoveFileCommand },         // 1
            { 50, main.ProcessMoveFileCommand },         // 2
            { 51, main.ProcessMoveFileCommand },         // 3
            { 52, main.ProcessMoveFileCommand },         // 4
            { 53, main.ProcessMoveFileCommand },         // 5
            { 54, main.ProcessMoveFileCommand },         // 6
            { 55, main.ProcessMoveFileCommand },         // 7
            { 56, main.ProcessMoveFileCommand },         // 8
            { 57, main.ProcessMoveFileCommand },         // 9
            { 48, main.ProcessMoveFileCommand },         // 0

            { 8,  main.ProcessUndoLastActionCommand },   // backspace
            { 32, main.ProcessSkipFileCommand },         // space
            { 27, main.ProcessExitAppCommand },          // esc
        };*/
    }

    void Start()
    {
        uiHandler.root.RegisterCallback<KeyDownEvent>(KeyWasPressedEvent);
    }


    // PRIVATE
    /*private void KeyWasPressedEvent(KeyDownEvent keyDownEvent)
    {
        var keyCode = (int)keyDownEvent.keyCode;

        if (keyCodeToFunctionMap.ContainsKey(keyCode))
            keyCodeToFunctionMap[keyCode].Invoke(keyCode);
    }*/
    
    private void KeyWasPressedEvent(KeyDownEvent keyDownEvent)
    {
        var keyCode = (int)keyDownEvent.keyCode;
        if (keyCode == 0) return; // Unity error (kinda double pressing)

        try
        {
            main.interlinkedCollection.FindRelatedSet(keyCode).keyboardPressCallback(keyCode);
        }
        catch
        {
            // If pressed unrelated key -- fine, do nothing
        }
    }
}
