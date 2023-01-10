using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AlekGames.Editor
{
    public class GroundSnapEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Alek Games/Placer/Ground Snapper")]
        public static void showWindow()
        {
            GetWindow<GroundSnapEditorWindow>("Ground Snapper");
        }

        float normalAllighn = 1;
        float upOffset = 0;
        GameObject used;
        bool snapChildren;
        private void OnGUI()
        {
            used = EditorGUILayout.ObjectField("to snap", used, typeof(GameObject), true) as GameObject;

            GUILayout.Space(10);

            normalAllighn = EditorGUILayout.Slider("normal allighn", normalAllighn, 0, 1);
            upOffset = EditorGUILayout.FloatField("upOffset", upOffset);
            snapChildren = EditorGUILayout.Toggle("snap children", snapChildren);

            GUILayout.Space(20);

            if(GUILayout.Button("Snap"))
            {
                if (snapChildren)
                {
                    for (int i = 0; i < used.transform.childCount; i++) snap(used.transform.GetChild(i));
                }
                else snap(used.transform);
            }
        }

        private void snap(Transform obj)
        {
            if (Physics.Raycast(obj.position + Vector3.up * 0.0001f, Vector3.down, out RaycastHit info, 100)) snapFinish(obj, info);

            else if (Physics.Raycast(obj.position + Vector3.up * 50, Vector3.down, out info, 150)) snapFinish(obj, info);

            else Debug.LogWarning("couldnt find ground for " + obj.name);
        }

        private void snapFinish(Transform obj, RaycastHit info)
        {
            Vector3 up = Vector3.Lerp(Vector3.up, info.normal, normalAllighn);
            obj.position = info.point + up * upOffset;
            obj.up = up;
            EditorUtility.SetDirty(obj);
        }
    }
}
