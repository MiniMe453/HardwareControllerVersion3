using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using AlekGames.Placer.Profiles;
using AlekGames.Placer.Shared;

using UnityEditor;

#if UNITY_EDITOR

namespace AlekGames.Placer.Systems.Main
{
    [ExecuteInEditMode]
    public class Painter : MonoBehaviour, IPrefabPalleteUser
    {

        #region values

        public enum paintM { scatter, exact, remove, replace};

        public paintM paintMode;

        public bool recordUndo = true;

        public prefabPalette palette;

        public int specificIndex = -1;

        public bool holdActivation = true;

        public float holdActivationDistance = 2;

        [Tooltip("if greater than 0, grid placing is enabled"), Min(0)]
        public float placeGridSize = 0;

        [Tooltip("snap placed object to desired height. disabled if in Positive infinity")]
        public float heightSnap = float.PositiveInfinity;

        [Range(0, 150)]
        public float brushSize = 5;

        [Range(0, 200)]
        public int scatterCount = 8;

        [Range(0, 100)]
        public float scatterAvoidenceDistance = 1;

        public bool massReplaceDestroy = true;

        [Min(1)]
        public float findCount = 2;

        public float YRotationOffset;

        public bool spawnPreview = true;

        public bool enabledPaint { get; private set; }
        public Vector3 paintPos { get; private set; }
        public Vector3 paintNormal { get; private set; }

        public bool toBigNormal { get; private set; }

        public bool paintReturned { get; private set; }

        private bool isPlacing;
        public GameObject preview { get; private set; }

        private Vector3? lastSpawn = null;

        #endregion

        #region updates

        [ContextMenu("enable")]
        public void Enable()
        {
            if(enabledPaint)
            {
                Debug.Log("already enabled");
                return;
            }

            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }

            Debug.Log("add paint feature to scene");

            isPlacing = false;
            enabledPaint = true;
            SceneView.beforeSceneGui += beforeScene;

            if (spawnPreview) previewAsyncUpdates();
            
        }

        [ContextMenu("disable")]
        public void Disable()
        {
            if (!enabledPaint)
            {
                Debug.Log("already disabled");
                return;
            }

            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }

            Debug.Log("remove paint feature from scene");

            enabledPaint = false;
            SceneView.beforeSceneGui -= beforeScene;

            removePreview();
        }

        public void revert()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        void beforeScene(SceneView scene)
        {
            if (!Selection.gameObjects.Contains(gameObject) || !Application.isEditor)
            {
                Disable();
                return;
            }

            Event e = Event.current;

            if (e.isMouse)
            {
                LayerMask groundLayer = getGroundLayer();
                LayerMask avoidedLayer = getAvoidedLayer();



                Vector3 mousePos = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
                mousePos.x *= ppp;

                Ray ray = scene.camera.ScreenPointToRay(mousePos);
                bool rayHit = Physics.Raycast(ray, out RaycastHit info, float.PositiveInfinity, groundLayer);

                Vector3 rayOrigin = ray.origin;

                paintReturned = (!rayHit || !setPlacePos(info, groundLayer, avoidedLayer));
                if (!paintReturned && isPlacingMode()) paintReturned = Physics.Raycast(rayOrigin, (paintPos - rayOrigin), Vector3.Distance(rayOrigin, paintPos) - 0.001f, avoidedLayer);

                updatePreview();


                if (isPlacingMode())
                {
                    toBigNormal = Vector3.Angle(paintNormal, Vector3.up) > palette.maxNormal;
                    GUI.changed = true;
                }
                else toBigNormal = false;


                if (e.button == 0)
                {
                    
                    if (e.type == EventType.MouseUp)
                    {
                        lastSpawn = null;
                        isPlacing = false;
                        e.Use();
                    }
                    else if (((e.type == EventType.MouseDown) || continousPlacing()))
                    {
                        isPlacing = true;
                        e.Use();

                        if (paintReturned || toBigNormal) return;

                        if (lastSpawn != null && Vector3.Distance(lastSpawn.Value, paintPos) < holdActivationDistance) return;
                        

                        switch (paintMode)
                        {
                            case paintM.scatter:
                                scatter(groundLayer, avoidedLayer);
                                break;
                            case paintM.exact:

                                spawnObj(paintPos, paintNormal);

                                break;
                            case paintM.remove:
                                editObj(false, info.collider.transform);
                                break;
                            case paintM.replace:
                                editObj(true, info.collider.transform);
                                break;
                        }

                        if (holdActivation && isPlacingMode())
                            lastSpawn = paintPos;

                    }
                }
                else if (e.button == 1 && e.type == EventType.MouseDown)
                {
                    //e.Use();

                    YRotationOffset += 90;
                    if (YRotationOffset >= 360) YRotationOffset -= 360; //to keep it small value
                    EditorUtility.SetDirty(this);
                    updatePreview();
                }



                GUI.changed = true;
            }
            else if (usingHeightSnap() && isPlacingMode() && e.type == EventType.ScrollWheel)
            {
                float ammount = -e.delta.y;
                if (ammount != 0)
                {
                    e.Use();
                    if (ammount > 0) ammount = usingGrid() ? placeGridSize / 2 : 0.5f;
                    else ammount = usingGrid() ? -placeGridSize / 2 : -0.5f;

                    heightSnap += ammount;
                    paintPos = new Vector3(paintPos.x, heightSnap, paintPos.z);
                    updatePreview();
                    EditorUtility.SetDirty(this);
                    GUI.changed = true;
                }
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Alpha1)
                {
                    paintMode = Painter.paintM.scatter;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Alpha2)
                {
                    paintMode = Painter.paintM.exact;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Alpha3)
                {
                    paintMode = Painter.paintM.remove;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Alpha4)
                {
                    paintMode = Painter.paintM.replace;
                    e.Use();
                }
            }

        }


        #endregion

        #region info

        public bool usingGrid()
        {
            return placeGridSize != 0;
        }

        public bool usingHeightSnap()
        {
            return heightSnap != float.PositiveInfinity;
        }

        public bool isPlacingMode()
        {
            return (paintMode == paintM.scatter || paintMode == paintM.exact);
        }

        private bool continousPlacing()
        {
            return isPlacing && (holdActivation || !isPlacingMode());
        }


        #endregion


        #region preview

        private void updatePreview()
        {
            if (!spawnPreview) return;

            if (isPlacingMode())
            {
                preview = palette.updatePreviewWithSettings(preview, paintPos, paintNormal, YRotationOffset, specificIndex, false);
            }
            else removePreview();
        }
    

        private async void previewAsyncUpdates()
        {
            while(true)
            {
                if (!enabledPaint)
                {
                    Debug.Log("remove preview reMake sequence");
                    return;
                }

                if (isPlacingMode() && specificIndex < 0)
                    preview = palette.updatePreviewWithSettings(preview, paintPos, paintNormal, YRotationOffset, -1, true);

                await System.Threading.Tasks.Task.Delay(750); // remake once per 0.75 seconds
            }
        }

        private void fullpreviewUpdate()
        {
            if (enabledPaint && isPlacingMode()) preview = palette.updatePreviewWithSettings(preview, paintPos, paintNormal, YRotationOffset, specificIndex, true);
        }

        private void removePreview()
        {
            if(preview != null)
                DestroyImmediate(preview);        
        }

        #endregion


        #region spawns

        private void scatter(LayerMask ground, LayerMask avoided)
        {
            Vector3 rayCenter = paintPos + paintNormal * 3;

            Quaternion toRot = Quaternion.LookRotation(Quaternion.AngleAxis(90, paintNormal) * Vector3.forward, paintNormal);

            List<Vector3> spawnedPos = new List<Vector3>();

            float sqrAvoidiance = scatterAvoidenceDistance * scatterAvoidenceDistance;

            for (int i = 0; i < scatterCount; i++)
            {
                Vector2 move = Random.insideUnitCircle * brushSize;

                Vector3 offset = toRot * new Vector3(move.x, 0, move.y);

                Vector3 nCenter = rayCenter + offset;

                Ray ray = new Ray(nCenter, -paintNormal);

                if (Physics.Raycast(ray, out RaycastHit info, 9, ground)) 
                {
                    if (!Physics.Raycast(ray, Vector3.Distance(nCenter, info.point), avoided, QueryTriggerInteraction.Ignore))
                    {
                        if (Vector3.Angle(Vector3.up, info.normal) <= palette.maxNormal)
                        {

                            bool tooCloase = false;

                            foreach (Vector3 p in spawnedPos)
                            {
                                if ((p - nCenter).sqrMagnitude < sqrAvoidiance)
                                {
                                    tooCloase = true;
                                    break;
                                }
                            }

                            if (tooCloase) continue;

                            spawnObj(info.point, info.normal);
                            spawnedPos.Add(nCenter);
                        }
                    }
                }
            }
        }

        private void spawnObj(Vector3 pos, Vector3 surfaceNormal)
        {
            GameObject spawned = palette.spawn(pos, surfaceNormal, transform, YRotationOffset, specificIndex);

            if(recordUndo) Undo.RegisterCreatedObjectUndo(spawned, "painted object");
            EditorUtility.SetDirty(spawned);
        }

        private void editObj(bool replace, Transform curSelected)
        {

            if (massReplaceDestroy)
            {
                float sqrSize = brushSize * brushSize;

                int edited = 0;
                int curCheck = 0;

                while (transform.childCount > curCheck && edited <= findCount)
                {
                    if ((transform.GetChild(curCheck).position - paintPos).sqrMagnitude < sqrSize)
                    {
                        Transform obj = transform.GetChild(curCheck);

                        if (replace) spawnObj(obj.position, obj.up);


                        DestroyImmediate(obj.gameObject);


                        edited++;
                    }

                    curCheck++;
                }
            }
            else
            {
                Transform checkedT = curSelected.transform;
                while(checkedT.parent != transform && checkedT.parent != null) checkedT = checkedT.parent;
                

                if (checkedT.parent == null) return;
                else
                {
                    
                    if (replace) spawnObj(checkedT.position, checkedT.up);

                    DestroyImmediate(checkedT.gameObject);
                }
            }
        }

        #endregion


        #region get set

        private bool setPlacePos(RaycastHit info, LayerMask ground, LayerMask avoided)
        {
            if (usingGrid() && isPlacingMode())
            {
                Vector3 gridedPos = info.point;

                float halfGrid = placeGridSize / 2;

                float XRest = (gridedPos.x % placeGridSize);
                if (XRest > halfGrid) gridedPos.x += placeGridSize - XRest;
                else gridedPos.x -= XRest;

                float ZRest = (gridedPos.z % placeGridSize);
                if (ZRest > halfGrid) gridedPos.z += placeGridSize - ZRest;
                else gridedPos.z -= ZRest;




                if (!usingHeightSnap())
                {
                    Vector3 fromRay = gridedPos + Vector3.up * 5;
                    if (Physics.Raycast(fromRay, Vector3.down, out RaycastHit gridInfo, float.PositiveInfinity, ground))
                    {
                        paintPos = gridInfo.point; //still update paint pos, even if dont allow paint
                        paintNormal = gridInfo.normal;

                        if (!Physics.Raycast(fromRay, Vector3.down, Vector3.Distance(fromRay, gridInfo.point) - 0.001f, avoided)) return true;                      
                    }

                    return false;
                }
                else paintPos = gridedPos;
                
            }
            else
            {
                paintPos = info.point;
                paintNormal = info.normal;
            }

            snapPaintPosHeight();

            return true;
        }

        private bool snapPaintPosHeight()
        {
            if (usingHeightSnap() && isPlacingMode())
            {
                paintPos = new Vector3(paintPos.x, heightSnap, paintPos.z);
                paintNormal = Vector3.up;
                return true;
            }
            return false;
        }

        private LayerMask getAvoidedLayer()
        {
            if (isPlacingMode()) return palette.avoidedLayer;
            
            return 0;
        }

        private LayerMask getGroundLayer()
        {
            if (isPlacingMode())return palette.groundLayer;       

            return ~0;
        }

        #endregion


        #region palette user

        public void setPalette(prefabPalette palette)
        {
            this.palette = palette;
            removePreview();
            specificIndex = Mathf.Clamp(specificIndex, -1, palette.objects.Length - 1);
        }
        public prefabPalette getPalette()
        {
            return palette;
        }

        public void setSpecificIndex(int index)
        {
            specificIndex = index;
            if(spawnPreview) fullpreviewUpdate();
        }
        public int getSpecificIndex()
        {
            return specificIndex;
        }

        #endregion

    }
}

#endif