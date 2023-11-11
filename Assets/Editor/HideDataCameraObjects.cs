using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class HideDataCameraObjects : EditorWindow
{
    private List<GameObject> sceneObjects = new List<GameObject>();
    private bool areObjectsVisible = false;

    [MenuItem("Window/Lost on Mars/Hide Data Camera Objects")]
    public static void ShowExample()
    {
        HideDataCameraObjects wnd = GetWindow<HideDataCameraObjects>();
        wnd.titleContent = new GUIContent("HideDataCameraObjects");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy
        Label label = new Label("Hello World!");
        root.Add(label);

        // Create button
        Button hideButton = new Button();
        hideButton.name = "button";
        hideButton.text = "Hide data objects";
        hideButton.clicked += HideObjects;
        root.Add(hideButton);

        Button showButton = new Button();
        showButton.name = "button";
        showButton.text = "Show data objects";
        showButton.clicked += ShowObjects;
        root.Add(showButton);
    }

    private void FilterDataCameraObjects()
    {
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.hideFlags != HideFlags.None)
                continue;

            if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab || PrefabUtility.GetPrefabType(go) == PrefabType.ModelPrefab)
                continue;

            if(go.layer != LayerMask.NameToLayer("DataCameraObjects"))
                continue;

            go.SetActive(areObjectsVisible);
        }
    }

    private void HideObjects()
    {
        areObjectsVisible = false;
        FilterDataCameraObjects();
    }

    private void ShowObjects()
    {
        areObjectsVisible = true;
        FilterDataCameraObjects();
    }
}