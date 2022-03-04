#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuadTreeExportWindow : EditorWindow
{

    [MenuItem("Window/QuadTreeExport")]
    public static void ShowWindow()
    {

        // 创建一个MyWindow的可停靠窗口
        EditorWindow.GetWindow(typeof(QuadTreeExportWindow));
        // 创建一个标题为“My Empty Window”的工具窗口MyWindow
        // // unility: 是否为工具窗口(不可停靠)
        EditorWindow.GetWindow(typeof(QuadTreeExportWindow), false, "CreateCastleLodWindow");
        // // 创建一个Rect(0, 0, 100, 150)的窗口
        //EditorWindow.GetWindowWithRect(typeof(MyWindow), new Rect(0, 0, 100, 150));
    }

    RectTransform tempRT;
    TextMeshProUGUI tempText;
    Image tempImage;

    List<TerrainData> terrains = new List<TerrainData>();

    string log = "";

    private void OnGUI()
    {
        // 创建Base Settings
        GUILayout.Space(20);
        GUILayout.Label("大地图主堡预制体生成插件", EditorStyles.boldLabel);     // 创建一个粗体 Label

        for (int i = 0; i < terrains.Count; i++)
        {

        }
        for (int i = 0; i < 3; i++)
        {
            //GUILayout.Space(10);
            //GUILayout.Label("- LOD" + i + ":");
            //GUILayout.BeginHorizontal();
            //castle[i] = EditorGUILayout.ObjectField("主堡", castle[i], typeof(GameObject)) as GameObject;
            //GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            //house[i] = EditorGUILayout.ObjectField("小房子", house[i], typeof(GameObject)) as GameObject;
            //GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            //wall[i] = EditorGUILayout.ObjectField("城墙", wall[i], typeof(GameObject)) as GameObject;
            //GUILayout.EndHorizontal();
        }
       
        GUILayout.Space(20);
        if (GUILayout.Button("生成预制体"))
        {
            
        }
        GUILayout.Space(20);
        GUILayout.Label(log);

    }
}
#endif