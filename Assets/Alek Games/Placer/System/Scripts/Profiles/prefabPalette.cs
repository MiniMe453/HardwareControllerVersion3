using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlekGames.Placer.Shared;

namespace AlekGames.Placer.Profiles
{
    [CreateAssetMenu(menuName = "Alek Games/Profiles/Prefab Palette", fileName = "new PrefabPalette")]
    public class prefabPalette : ScriptableObject
    {      
        public GameObject[] objects;

        [Space]

        public bool spawnAsPrefab = true;

        [Tooltip("if should snap the children of spawned object. perfect if you have a prefab containing a little village, fence etc. .when snapping children, avoided layer does not count")]
        public bool snapChildren = false;

        [Space]

        [Tooltip("set to 0,0 to use transform settings")]
        public Vector2 minMaxScale = new Vector2(0.9f, 1.1f);
        [Tooltip("x,y,z random rotation addons. random rotation on each axis will be from -value to value, so 180 is fully random on axis, 90 is 180 rotation freedom 0 is none and so on"), Min(0)]
        public Vector3 randRotAdd = new Vector3(0, 180, 0);

        [Space]

        [Range(0, 180)]
        public float maxNormal = 60;

        [Range(0, 1)]
        public float normalAllighn = 0.5f;

        [Space]

        public LayerMask groundLayer;
        public LayerMask avoidedLayer;

        public GameObject spawn(Vector3 pos, Vector3 surfaceNormal, Transform parent, float YRotationOffset , int specificIndex)
        {
            Transform spawned = null;

            GameObject toSpawn = getObj(specificIndex);
#if UNITY_EDITOR
            if (spawnAsPrefab)
            {
                spawned = (UnityEditor.PrefabUtility.InstantiatePrefab(toSpawn, parent) as GameObject).transform;
                spawned.position = pos;
                spawned.rotation = toSpawn.transform.rotation;
            }
#endif      
            if(spawned == null) spawned = Instantiate(toSpawn, pos, toSpawn.transform.rotation, parent).transform;

            setRotation(spawned, surfaceNormal, YRotationOffset);
            setScale(spawned);
            if(snapChildren) snapChildrenOf(spawned);

            return spawned.gameObject;
        }

        /// <summary>
        /// updates given object, as if it was spawned with this palette.
        /// usen when object changed position/rotation, and you want to update it to correct rotation
        /// </summary>
        /// <param name="previewObj"></param>
        /// <param name="toPos"></param>
        /// <param name="toNomal"></param>
        public GameObject updatePreviewWithSettings(GameObject previewObj, Vector3 toPos, Vector3 toNomal, float YRotationOffset, int specificIndex, bool reMake)
        {
            if (previewObj == null) reMake = true;

            if(reMake)
            {
                DestroyImmediate(previewObj);
                GameObject toSpawn = getObj(specificIndex);
                previewObj = Instantiate(toSpawn, toPos, toSpawn.transform.rotation);

                Collider colR = previewObj.GetComponent<Collider>();
                if (colR != null) DestroyImmediate(colR);
                Collider[] colC = previewObj.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colC.Length; i++)
                {
                    DestroyImmediate(colC[i]);
                }
            }

            previewObj.transform.position = toPos;
            setRotation(previewObj.transform, toNomal, YRotationOffset, reMake);
            if(reMake) setScale(previewObj.transform);
            if (snapChildren) snapChildrenOf(previewObj.transform);

            return previewObj;
        }

        private void snapChildrenOf(Transform spt)
        {
            if (spt.childCount > 0)
            {
                for (int i = 0; i < spt.childCount; i++)
                {
                    Transform child = spt.GetChild(i);
                    if (Physics.Raycast(child.position + Vector3.up * 6, Vector3.down, out RaycastHit info, 50, groundLayer))
                    {
                        child.position = info.point;
                        child.up = Vector3.Lerp(Vector3.up, info.normal, normalAllighn);
                    }
                }
            }
        }

        private void setRotation(Transform obj, Vector3 normal, float YOffset, bool randomize = true)
        {

            obj.up = Vector3.Lerp(Vector3.up, normal, normalAllighn);
            if (randomize)
            {
                obj.rotation *= Quaternion.Euler(
                    Random.Range(-randRotAdd.x, randRotAdd.x),
                    Random.Range(-randRotAdd.y, randRotAdd.y),
                    Random.Range(-randRotAdd.z, randRotAdd.z)
                    );
            }

            obj.rotation *= Quaternion.AngleAxis(YOffset, obj.up);
        }

        private void setScale(Transform obj)
        {
            if (minMaxScale != Vector2.zero)
            {
                obj.localScale = Vector3.one;
                obj.localScale *= Random.Range(minMaxScale.x, minMaxScale.y);
            }
        }

        /// <summary>
        /// gets an object on specific index from palette
        /// </summary>
        /// <param name="specificIndex"></param>
        /// <returns></returns>
        public GameObject getObj(int specificIndex)
        {
            return specificIndex >= 0 ? 
                objects[specificIndex] : 
                objects[Random.Range(0, objects.Length)];
        }
    }
}
