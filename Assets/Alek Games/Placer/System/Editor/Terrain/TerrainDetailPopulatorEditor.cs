using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(TerrainDetailPopulator))]
    public class TerrainDetailPopulatorEditor : UnityEditor.Editor
    {



        public override void OnInspectorGUI()
        {
            TerrainDetailPopulator p = (TerrainDetailPopulator)target;

            DrawDefaultInspector();

            GUILayout.Space(20);

            if (GUILayout.Button("populate layer " + p.detailILayer)) p.populate();
            if (GUILayout.Button("clear layer " + p.detailILayer)) p.clearTerrainLayerDetail();
        }
    }
}
