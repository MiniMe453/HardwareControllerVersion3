using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Profiles;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(stepSettings))]
    public class stepSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            stepSettings s = (stepSettings)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.possibleSpawns)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.minMaxXZToYScale)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.sameScaleOnAllAxis)));           

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.maxRandRotAdd)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.localPlacementOffset)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.firstPlacePoints)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.closeByPlacePoints)));

            GUILayout.Space(10);


            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.onSurface)));
            if (s.onSurface)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.placeRayMethod)));
                if(s.placeRayMethod == stepSettings.placeRayM.icoRay) 
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.icoSubdivisons)));

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.rayLengh)));

                GUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.groundLayer)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.avoidedLayer)));

                GUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.desiredNormalAngle)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.normalAllighn)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.normalIncorrectcionAcceptance)));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.centerSideMove)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.freeTightPlacePreference)));
            if (s.freeTightPlacePreference != 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.minMaxNearObjects)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.colFindRange)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.objectsLayer)));
            }

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.placePreferenceWeight)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.placeWeightRandAddon)));

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.normalAccuracyWeight)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.normalWeightRandAddon)));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
