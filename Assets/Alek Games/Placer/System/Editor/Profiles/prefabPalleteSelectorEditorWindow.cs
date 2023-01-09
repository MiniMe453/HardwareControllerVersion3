using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Profiles;
using AlekGames.Placer.Shared;

namespace AlekGames.Editor
{
    public class prefabPalleteSelectorEditorWindow : EditorWindow
    {
        private paletteInfo[] palettes;

        private IPrefabPalleteUser user;

        /// <summary>
        /// doing prefabPalleteSelectorEditorWindow.showWindow().initialize(p); works where p is a IPrefabPalleteUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static void showWindow(IPrefabPalleteUser user)
        {
            prefabPalleteSelectorEditorWindow w = GetWindow<prefabPalleteSelectorEditorWindow>("pallete prefab selector");
            w.minSize = new Vector2(230, 250);
            w.initialize(user);
            
        }

        private void initialize(IPrefabPalleteUser user)
        {
            this.user = user;
            refreshPalletes();
        }

        Vector2 scrollPos;

        indexInfo[] indexPreviews;

        string[] options = new string[] { "palettes", "palette indexes" };
        int curSelected;

        private void OnGUI()
        {
            GUI.enabled = false;
            MonoBehaviour m = user as MonoBehaviour;
            if (m != null) EditorGUILayout.ObjectField("this window component user: ", m, typeof(MonoBehaviour), true);
            else
            {
                EditorGUILayout.HelpBox("no user found. \n reopen window to make it work", MessageType.Error);
                return;
            }
            GUI.enabled = true;

            GUILayout.Space(5);

            if (GUILayout.Button("refreshPalettes")) refreshPalletes();

            GUILayout.Space(10);

            curSelected = GUILayout.SelectionGrid(curSelected, options, 2);

            GUILayout.Space(20);

            switch (curSelected)
            {
                case 0:
                    drawPalettes();
                    break;
                case 1:
                    drawIndexes();
                    break;
            }

        }

        private void drawPalettes()
        {
            if (palettes.Length == 0)
            {
                EditorGUILayout.HelpBox("no paletts found in project. \ncreate one by right clicking and creating a prefab palette (Create/Alek Games/Profiles/Prefab Palette)", MessageType.Error);

                return;
            }
            else
            {

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 80));
                int selected = GUILayout.SelectionGrid(-1, palettes.Select(t => new GUIContent(t.palette.name, t.thumbnail, "switch to this palette")).ToArray(), Mathf.Clamp((int)(position.width / 250), 1, palettes.Length));
                EditorGUILayout.EndScrollView();

                if (selected != -1)
                {
                    prefabPalette p = palettes[selected].palette;
                    user.setPalette(p);
                    setIndexPreviews(p);
                }

            }
        }


        private void drawIndexes()
        {
            if (indexPreviews == null || indexPreviews.Length == 0)
            {
                EditorGUILayout.HelpBox("no registered indexes. \n if you didnt fill in palette in painter, do so, or try refreshing palettes. \n to select a palette go to palette tab, and select one", MessageType.Error);
                if (GUILayout.Button("go to palette selection tab")) curSelected = 0;
                return;
            }

            if (GUILayout.Button("refresh indexes Thumbnails")) setIndexPreviews(user.getPalette());

            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("un-index", "when spawning, will choose random, insted of a specific index"))) user.setSpecificIndex(-1);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 80));
            int selected = GUILayout.SelectionGrid(-1, indexPreviews.Select(t => new GUIContent(t.name, t.thumbnail, "switch to this index")).ToArray(), Mathf.Clamp((int)(position.width / 250), 1, indexPreviews.Length));
            EditorGUILayout.EndScrollView();

            if (selected != -1)
            {
                user.setSpecificIndex(selected);
            }

        }

        /// <summary>
        /// refreshes palettes available to choose from
        /// </summary>
        public void refreshPalletes()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(prefabPalette).Name);  
            palettes = new paletteInfo[guids.Length];
            for (int i = 0; i < palettes.Length; i++)        
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                prefabPalette palette = AssetDatabase.LoadAssetAtPath<prefabPalette>(path);

                Texture t = null;

                if(palette.objects.Length == 0)
                {
                    Debug.LogError("no objects asighned in palette " + palette.name + " couldnt load any thumbnail, and this palette will not work");
                }
                else t = getPrefabPreview(palette.objects[0]);

                palettes[i] = new paletteInfo(palette, t);
            }

            setIndexPreviews(user.getPalette());
        }

        public void setIndexPreviews(prefabPalette p)
        {
            if(p == null)
            {
                Debug.LogWarning("couldnt refresh palette index previews, palette it is null");
                return;
            }

            indexPreviews = new indexInfo[p.objects.Length];

            for (int i = 0; i < indexPreviews.Length; i++)
            {
                indexPreviews[i] = new indexInfo(p.objects[i]?.name, getPrefabPreview(p.objects[i]));
            }
            
        }

        private Texture getPrefabPreview(GameObject obj)
        {
            if (obj == null) return null;

            Texture t = AssetPreview.GetAssetPreview(obj);
            if (t == null) t = AssetPreview.GetMiniThumbnail(obj);
            return t;
        }

        public struct paletteInfo
        {
            public prefabPalette palette;
            public Texture thumbnail;

            public paletteInfo(prefabPalette palette, Texture thumbnail)
            {
                this.palette = palette;
                this.thumbnail = thumbnail;
            }
        }

        public struct indexInfo
        {
            public string name;
            public Texture thumbnail;

            public indexInfo(string name, Texture thumbnail)
            {
                this.name = name;
                this.thumbnail = thumbnail;
            }
        }
    }
}
