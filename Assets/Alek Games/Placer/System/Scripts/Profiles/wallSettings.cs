using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlekGames.Placer.Shared;

namespace AlekGames.Placer.Profiles
{
    [CreateAssetMenu(menuName = "Alek Games/Profiles/Wall Settings", fileName = "new wallSettings")]
    public class wallSettings : ScriptableObject
    {
        [Tooltip("the standing thing, that is conected to one another threw rails")]
        public GameObject post;

        [Space]

        [Tooltip("the thing connecting 2 posts horizontally if you do not use rails, railLen is still needed")]
        public GameObject rail;
        [Tooltip("distance between 2 posts/lengh of a rail")]
        public float railLengh = 2;
        [Tooltip("for each of these, on this height, there will be a rail placed on the post")]
        public float[] railHeights = new float[2] { 0.5f, 1 };

        [Space]

        [Tooltip("forward of model should be right/left of prefab. will be intercepting rails, unless have some offset on right/left")]
        public GameObject picket;
        [Min(0)]
        public int picketCount = 5;

        [Space]
        public bool asPrefab = true;

        public enum toSpawnType { post, rail, picket};


        /// <summary>
        /// spawns wall from given parameters. 
        /// make sure to spawn post on sStartPos yourself (if you want to), as this system assumes it already has one.
        /// </summary>
        /// <param name="wallPreset"></param>
        /// <param name="StartPos"></param>
        /// <param name="EndPos"></param>
        /// <param name="spawn"></param>
        /// <param name="AllowOverPaint"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Vector3? spawnWall(Vector3 StartPos, Vector3 EndPos, bool AllowOverPaint, Transform parent)
        {
            Vector3 FullWallDir = EndPos - StartPos;
            FullWallDir.y = 0;

            float wallLengh = FullWallDir.magnitude;

            float rl = railLengh;

            float p = wallLengh / rl;
            int posts;

            if (p == ((int)p)) posts = (int)p;
            else if (AllowOverPaint) posts = (int)p + 1;
            else posts = (int)p;

            Vector3 wallMove = FullWallDir.normalized * rl;
            Vector3 picMove = wallMove / (picketCount + 1);

            Vector3 fromPos = StartPos;

            Quaternion postRot = Quaternion.LookRotation(new Vector3(wallMove.x, 0, wallMove.z), Vector3.up);

            bool spawned = false;

            for (int i = 0; i < posts; i++)
            {

                Vector3 checkPos = fromPos + wallMove + Vector3.up * 6;
                if (Physics.Raycast(checkPos, Vector3.down, out RaycastHit info, 12))
                {
                    spawned = true;

                    Vector3 point = info.point;

                    spawn(wallSettings.toSpawnType.post, point, postRot, parent);

                    if (picket != null)
                    {
                        for (int pic = 1; pic <= picketCount; pic++)
                        {
                            Vector3 picCheckPos = fromPos + picMove * pic + Vector3.up * 3;
                            if (Physics.Raycast(picCheckPos, Vector3.down, out RaycastHit picInfo, 6))
                            {
                                spawn(wallSettings.toSpawnType.picket, picInfo.point, postRot, parent);
                            }
                        }
                    }

                    if (rail != null)
                    {
                        foreach (float r in railHeights)
                        {
                            Vector3 heightAdd = Vector3.up * r;
                            Vector3 railPos = point + heightAdd;
                            spawn(wallSettings.toSpawnType.rail, railPos, Quaternion.LookRotation((fromPos + heightAdd) - railPos), parent);
                        }
                    }


                    fromPos = point;
                }
                else Debug.LogError("couldnt find ground to place wall on");
            }

            Vector3? ret = null;
            if (spawned) ret = fromPos;
            return ret;
        }


        public GameObject spawn(toSpawnType toSpawn, Vector3 pos, Quaternion rot, Transform parent)
        {
            GameObject toSpawnP = null;

            switch (toSpawn)
            {
                case toSpawnType.post:
                    toSpawnP = post;
                    break;
                case toSpawnType.rail:
                    toSpawnP = rail;
                    break;
                case toSpawnType.picket:
                    toSpawnP = picket;
                    break;
            }

            GameObject spawned = null;

#if UNITY_EDITOR
            if (asPrefab)
            {
                spawned = (UnityEditor.PrefabUtility.InstantiatePrefab(toSpawnP, parent) as GameObject);
                spawned.transform.position = pos;
                spawned.transform.rotation = rot;
            }

            UnityEditor.EditorUtility.SetDirty(spawned);
#endif      
            if (spawned == null) spawned = Instantiate(toSpawnP, pos, rot, parent);

            return spawned;
        }
    }
}
