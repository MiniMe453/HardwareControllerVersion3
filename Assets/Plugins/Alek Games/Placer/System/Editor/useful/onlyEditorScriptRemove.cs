using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;

namespace AlekGames.Editor
{
    public class onlyEditorScriptRemove
    {
        /// <summary>
        /// removes scripts that do not work for a build
        /// </summary>
        [MenuItem("Tools/Alek Games/Placer/Remove All Only Editor Scripts Frome Scene")]
        public static void removeAllOnlyEditorScripts()
        {
            List<Painter> p = GameObject.FindObjectsOfType<Painter>().ToList();
            List<PhysicsPainter> pp = GameObject.FindObjectsOfType<PhysicsPainter>().ToList();
            List<wallPainter> wp = GameObject.FindObjectsOfType<wallPainter>().ToList();

            while(p.Count > 0)
            {
                MonoBehaviour.DestroyImmediate(p[p.Count - 1]);
                p.RemoveAt(p.Count - 1);
            }

            while (pp.Count > 0)
            {
                MonoBehaviour.DestroyImmediate(pp[pp.Count - 1]);
                pp.RemoveAt(pp.Count - 1);
            }

            while (wp.Count > 0)
            {
                MonoBehaviour.DestroyImmediate(wp[wp.Count - 1]);
                wp.RemoveAt(wp.Count - 1);
            }

            Debug.Log("All only Editor scripts frome scene are now removed");
        }
    }
}
