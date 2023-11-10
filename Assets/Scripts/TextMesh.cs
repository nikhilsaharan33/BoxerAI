using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextMesh : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private string logText = "";

    public void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    public void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logText, string stackTrace, LogType logType)
    {
        this.logText += logText + "\n";
        textMeshPro.text = this.logText;
    }
}
