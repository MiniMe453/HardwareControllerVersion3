using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using AlekGames.Placer.Shared;
using AlekGames.Placer.Profiles;

namespace AlekGames.Placer.Systems.Main
{
    public class textSpawner : MonoBehaviour, ISpawner
    {
        public textFont font;

        [TextArea()]
        public string text;

        public bool autoUpdate = true;

        public bool asPrefab = true;

        [Tooltip("if should add colliders when ' ' is wrote. good if you use text on text ground snapping")]
        public bool addSpaceColliders = false;

        [Tooltip("size of a letter ")]
        public float betweenLettersSpace = 0.2f;
        [Tooltip("size of 'space'. when you press spacebar ")]
        public float spaceLengh = 0.4f;
        [Tooltip("space down when tyrped 'enter'")]
        public float enterLengh = 0.4f;

        public Vector3 randPosAdd = Vector3.zero;
        public Vector3 randRotAdd = Vector3.zero;
        public Vector2 minMaxScale = Vector2.one;

        public bool snapToGround = false;
        [Range(0, 1)]
        public float normalAllighn = 0.2f;
        public LayerMask groundLayer;
        public float snapHeightOffset = 0.2f;
        public Vector3 snapRayDir = Vector3.down;

        [ContextMenu("spawn")]
        public async Task spawn()
        {
            char[] ca = text.ToCharArray();

            Vector3 start = transform.position;
            Vector3 curPos = transform.position;

            int curEnter = 0;
            bool rowAdded = false;

            List<letterInfo> spawned = new List<letterInfo>();

            foreach (char c in ca)
            {
                string l = c.ToString();

                if(l == "\n")
                {
                    curEnter++;
                    curPos = start - (transform.up * curEnter * enterLengh);
                    rowAdded = true;
                    continue;
                }

                if (l == " ")
                {

                    if (addSpaceColliders)
                    {
                        GameObject sc = new GameObject("space col");
                        sc.transform.parent = transform;
                        sc.transform.position = curPos;
                        sc.transform.right = transform.right;
                        BoxCollider col = sc.AddComponent<BoxCollider>();
                        col.size = new Vector3(spaceLengh, enterLengh / 2, spaceLengh / 2);
                        col.center = new Vector3(0, (enterLengh / 4) - snapHeightOffset / 2, 0);

                        spawned.Add(new letterInfo(sc.transform, rowAdded));
                        rowAdded = false;
                    }

                    curPos += transform.right * spaceLengh;

                    continue;
                }

                GameObject rep = font.getRepresentation(l);

                if (rep == null) Debug.LogError("didnt found representation for " + l);
                else
                {
                    GameObject nObj = spawn(rep, curPos, rep.transform.rotation * transform.rotation);

                    spawned.Add(new letterInfo(nObj.transform, rowAdded));

                    curPos += transform.right * betweenLettersSpace;
                    rowAdded = false;
                }
            }

            if(snapToGround)
            {
                for (int i = spawned.Count - 1; i >= 0; i--)
                {
                    letterInfo l = spawned[i];
                    if (Physics.Raycast(l.transform.position, snapRayDir, out RaycastHit info, 300, groundLayer, QueryTriggerInteraction.UseGlobal))
                    {
                        Debug.DrawRay(l.transform.position, Vector3.down);
                        l.transform.position = info.point;
                        l.transform.rotation = Quaternion.LookRotation(l.transform.forward, Vector3.Lerp(Vector3.up, info.normal, normalAllighn));
                        l.transform.position += l.transform.up * snapHeightOffset;

#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(l.transform.gameObject);
#endif
                    }
                    else Debug.LogError("couldnt find ground in range of 300");

                    if (l.rowFirst)
                    {
                        
                        await Task.Delay(SInfo.awaitT);
                    }
                }
            }
        }

        private GameObject spawn(GameObject obj,Vector3 pos, Quaternion rot)
        {
            Transform spawned = null;

#if UNITY_EDITOR
            if (asPrefab)
            {
                spawned = (UnityEditor.PrefabUtility.InstantiatePrefab(obj, transform) as GameObject).transform;
                spawned.position = pos;
            }
#endif      
            if (spawned == null) spawned = Instantiate(obj, pos, Quaternion.identity, transform).transform;

            spawned.position += new Vector3(
                Random.Range(-randPosAdd.x, randPosAdd.x),
                Random.Range(-randPosAdd.y, randPosAdd.y),
                Random.Range(-randPosAdd.z, randPosAdd.z)
                );

            spawned.rotation *= Quaternion.Euler(
                Random.Range(-randRotAdd.x, randRotAdd.x),
                Random.Range(-randRotAdd.y, randRotAdd.y),
                Random.Range(-randRotAdd.z, randRotAdd.z)
                );

           spawned.localScale = Vector3.one;
            spawned.localScale *= Random.Range(minMaxScale.x, minMaxScale.y);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(spawned.gameObject);
#endif

            return spawned.gameObject;
        }

        public void revert()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public struct letterInfo
        {
            public Transform transform;
            public bool rowFirst;

            public letterInfo(Transform transform, bool rowFirst)
            {
                this.transform = transform;
                this.rowFirst = rowFirst;
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, snapRayDir);
        }
#endif
    }
}
