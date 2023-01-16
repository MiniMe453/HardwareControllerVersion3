using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MegaFiers;

public class MegaScatterQuery : MonoBehaviour
{
	public virtual bool FindPos(ref Vector3 pos)
	{
		return true;
	}

	public virtual void ObjectPlaced(GameObject obj, Vector3 pos, Quaternion rot, Vector3 scale)
	{
	}

	public virtual void ObjectDestroyed(GameObject obj)
	{
	}

	public virtual int GetCount(int count)
	{
		return count;
	}
}

public enum MegaScatterMode
{
	Density,
	Count,
}

[System.Serializable]
public class MegaScatterCollisionObj
{
	public Collider     collider;
	public bool	    	active = true;
	public bool 		includechildren = false;
	public Collider[]	children;
	public bool			prebuildenable = false;
    public bool         postbuilddisable = false;
}

public class MegaScatter : MonoBehaviour
{
	public bool					buildOnStart = false;
	public bool					update = false;
	public bool					meshPerShape	= true;
	public float				Density			= 0.5f;
	public int					forcecount		= 100;
	public MegaScatterMode		countmode		= MegaScatterMode.Count;
	public int					StartCurve		= 0;
	public int					EndCurve		= 0;
	public int					subdiv			= 4;
	public bool					raycast			= false;
	public bool					NeedsGround		= true;
	public float				collisionOffset = 100.0f;
	public MegaShape			shape;
	public List<bool>			usespline = new List<bool>();
	public List<MegaScatterLayer>	layers = new List<MegaScatterLayer>();
	public List<MegaCurveList>	curves		= new List<MegaCurveList>();
	public int					seed = 0;
	public bool					hideObjects = false;
	public Vector3				globalScale = Vector3.one;
	public int					currentEdit = 0;
	public bool					showusesplines = false;
	public bool					showignoreobjs = false;
	public bool					showsurfaceobjs = false;
	public bool					fillHoles	= false;
	public bool					colorMesh	= false;

	public List<MegaScatterCollisionObj>	ignoreobjs = new List<MegaScatterCollisionObj>();
	public List<MegaScatterCollisionObj>	allcolliders = new List<MegaScatterCollisionObj>();
	public List<MegaScatterCollisionObj>	surfaces = new List<MegaScatterCollisionObj>();

	[HideInInspector]
	public List<MegaSubMesh>	submeshes	= new List<MegaSubMesh>();

	public List<MegaScatterInst>	instances	= new List<MegaScatterInst>();

	public MegaScatterQuery		queryObject;

	public float				totalarea = 0.0f;
	public Collider				texturecollider = null;
	public int					objcount = 0;
	public int					vertcount = 0;
	public int					scattercount = 0;
	public bool					displaygizmo = true;
	public Bounds				totalbounds = new Bounds();

	public bool					showadvanced = false;
	public int					FailCount = 1000;
	public int					PosFailCount = 20;
	// Lightmap
	public bool					showlightmap	= false;
	public bool					genLightMap		= false;
	public float				angleError		= 0.08f;
	public float				areaError		= 0.15f;
	public float				hardAngle		= 88.0f;
	public float				packMargin		= 0.0039f;

	public bool					dostaticbatching = false;

	static public float			raycastheight	= 10000.0f;

	public bool					useCoRoutine = false;
	public int					NumPerFrame		= 100;

	public virtual void Fill() { }

	public virtual IEnumerator FillCo() { yield return null; }

	public virtual string GetURL()	{ return ""; }

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf" + GetURL());
	}

	public void RemoveObjects()
	{
		objcount = 0;
		vertcount = 0;
		scattercount = 0;

		//Transform[] allTransforms = gameObject.GetComponentsInChildren<Transform>();

		List<Transform> children = new List<Transform>();

		for ( int i = 0; i < gameObject.transform.childCount; i++ )
		{
			children.Add(gameObject.transform.GetChild(i));
		}

		for ( int i = 0; i < children.Count; i++ )
		{
			if ( Application.isEditor )
				DestroyImmediate(children[i].gameObject);
			else
				Destroy(children[i].gameObject);
		}

#if false
		foreach ( Transform childObjects in allTransforms )
		{
			//if ( gameObject.transform.IsChildOf(childObjects.transform) == false )
			if ( childObjects.transform.IsChildOf(gameObject.transform) == false )
			{
				if ( Application.isEditor )
					DestroyImmediate(childObjects.gameObject);
				else
					Destroy(childObjects.gameObject);
			}
		}
#endif

		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}

	public void SetShape(MegaShape newshape)
	{
		shape = newshape;

		usespline.Clear();
		if ( shape )
		{
			for ( int i = 0; i < shape.splines.Count; i++ )
			{
				usespline.Add(true);
			}
		}
	}

	void Start()
	{
		if ( buildOnStart )
		{
			if ( Application.isPlaying )
			{
				update = true;
				if ( useCoRoutine )
				{
					StartCoroutine(FillCo());
				}
				else
					Fill();
			}
		}
	}

	public virtual int NumCurves()
	{
		return 0;
	}

	public bool ValidHit(RaycastHit hit)
	{
		for ( int c = 0; c < ignoreobjs.Count; c++ )
		{
			if ( ignoreobjs[c].active && hit.collider == ignoreobjs[c].collider )
				return false;

			if ( ignoreobjs[c].includechildren && ignoreobjs[c].children != null )
			{
				for ( int i = 0; i < ignoreobjs[c].children.Length; i++ )
				{
					if ( ignoreobjs[c].active && hit.collider == ignoreobjs[c].children[i] )
						return false;
				}
			}
		}

		return true;
	}

	public bool MultiRayCast(Vector3 wp, float rad, int points)
	{
		RaycastHit hit;

		if ( points == 1 )
			rad = 0.0f;

		Collider col = null;

		for ( int i = 0; i < points; i++ )
		{
			float ang = ((float)i / (float)points) * 2.0f * Mathf.PI;

			Vector3 off = Vector3.zero;

			off.x += Mathf.Sin(ang) * rad;
			off.z += Mathf.Cos(ang) * rad;

			Vector3 o = wp + off;	//collisionOrigin + p;
			o.y += collisionOffset;

			if ( !texturecollider )
			{
				if ( Physics.Raycast(o, Vector3.down, out hit, raycastheight) )
				{
					if ( !ValidHit(hit) )
						return false;

					if ( col == null )
						col = hit.collider;
					else
					{
						if ( hit.collider != col )
							return false;
					}
				}
				else
					return false;
			}
			else
			{
				RaycastHit[]	hits;
				hits = Physics.RaycastAll(o, Vector3.down, raycastheight);

				if ( hits.Length > 0 )
				{
					float maxy = float.MinValue;
					int index = -1;

					for ( int h = 0; h < hits.Length; h++ )
					{
						if ( hits[h].point.y > maxy )
						{
							maxy = hits[h].point.y;
							index = h;
						}
					}

					if ( index >= 0 )
					{
						if ( !ValidHit(hits[index]) )
							return false;

						if ( col == null )
							col = hits[index].collider;
						else
						{
							if ( hits[index].collider != col )
								return false;
						}
					}
					else
						return false;
				}
				else
					return false;
			}
		}

		return true;
	}

	bool GetActive(Collider col)
	{
		for ( int i = 0; i < allcolliders.Count; i++ )
		{
			if ( allcolliders[i].collider == col )
				return allcolliders[i].active;
		}

		return false;
	}

#if false
	public void CollectColliders()
	{
		// Build collider lists
		Collider[] allcol = (Collider[])FindObjectsOfType(typeof(Collider));

		allcolliders.Clear();
		for ( int i = 0; i < allcol.Length; i++ )
		{
			MegaScatterCollisionObj cobj = new MegaScatterCollisionObj();
			cobj.collider = allcol[i];
#if UNITY_3_5
			//cobj.active = allcol[i].gameObject.active;
			cobj.active = allcol[i].enabled;	//gameObject.active;
			//allcol[i].gameObject.SetActiveRecursively(false);
			allcol[i].enabled = false;
#else
			cobj.active = allcol[i].gameObject.activeInHierarchy;
			allcol[i].gameObject.SetActive(false);
#endif
			allcolliders.Add(cobj);
		}

		for ( int i = 0; i < ignoreobjs.Count; i++ )
		{
			if ( ignoreobjs[i].active )
			{
#if UNITY_3_5
				if ( ignoreobjs[i].collider )
				{
					//ignoreobjs[i].collider.gameObject.SetActiveRecursively(GetActive(ignoreobjs[i].collider));
					ignoreobjs[i].collider.enabled = GetActive(ignoreobjs[i].collider);
				}
#else
				if ( ignoreobjs[i].collider )
					ignoreobjs[i].collider.gameObject.SetActive(GetActive(ignoreobjs[i].collider)); // true
#endif
			}
		}

		for ( int i = 0; i < surfaces.Count; i++ )
		{
			if ( surfaces[i].active )
			{
#if UNITY_3_5
				if ( surfaces[i].collider )
				{
					//surfaces[i].collider.gameObject.SetActiveRecursively(GetActive(surfaces[i].collider));
					surfaces[i].collider.enabled = GetActive(surfaces[i].collider);
				}
#else
				if ( surfaces[i].collider )
					surfaces[i].collider.gameObject.SetActive(GetActive(surfaces[i].collider));	//true);
#endif
			}
		}
	}

	public void RestoreColliders()
	{
		for ( int i = 0; i < allcolliders.Count; i++ )
		{
#if UNITY_3_5
			if ( allcolliders[i].collider )
			{
				//allcolliders[i].collider.gameObject.SetActiveRecursively(allcolliders[i].active);
				allcolliders[i].collider.enabled = allcolliders[i].active;
			}
#else
			if ( allcolliders[i].collider )
				allcolliders[i].collider.gameObject.SetActive(allcolliders[i].active);
#endif
		}

		allcolliders.Clear();
	}
#else
	public void CollectColliders()
	{
		// Build collider lists
		Collider[] allcol = (Collider[])FindObjectsOfType(typeof(Collider));

		allcolliders.Clear();
		for ( int i = 0; i < allcol.Length; i++ )
		{
			MegaScatterCollisionObj cobj = new MegaScatterCollisionObj();
			cobj.collider = allcol[i];
			cobj.active = allcol[i].enabled;
			allcol[i].enabled = false;
			allcolliders.Add(cobj);
		}

		for ( int i = 0; i < ignoreobjs.Count; i++ )
		{
			if ( ignoreobjs[i].active )
			{
				if ( ignoreobjs[i].collider )
					ignoreobjs[i].collider.enabled = GetActive(ignoreobjs[i].collider);

				if ( ignoreobjs[i].includechildren )
				{
					ignoreobjs[i].children = ignoreobjs[i].collider.gameObject.GetComponentsInChildren<Collider>();
					for ( int c = 0; c < ignoreobjs[i].children.Length; c++ )
					{
						ignoreobjs[i].children[c].enabled = GetActive(ignoreobjs[i].children[c]);
					}
				}
				else
					ignoreobjs[i].children = null;
			}
		}

		for ( int i = 0; i < surfaces.Count; i++ )
		{
			if ( surfaces[i].active )
			{
				if ( surfaces[i].collider )
				{
					surfaces[i].collider.enabled = GetActive(surfaces[i].collider);

					if ( surfaces[i].prebuildenable )
						surfaces[i].collider.enabled = true;
				}
			}
		}
	}

	public void RestoreColliders()
	{
		for ( int i = 0; i < allcolliders.Count; i++ )
		{
            if ( allcolliders[i].collider )
            {
                allcolliders[i].collider.enabled = allcolliders[i].active;
            }
		}

		for ( int i = 0; i < surfaces.Count; i++ )
		{
			if ( surfaces[i].postbuilddisable )
				surfaces[i].collider.enabled = false;
		}

		allcolliders.Clear();
	}
#endif

	public void HideShow()
	{
#if UNITY_3_5
		gameObject.SetActiveRecursively(!gameObject.active);
#else
		gameObject.SetActive(!gameObject.activeInHierarchy);
#endif
	}

	// Main API methods
	public int NumLayers()
	{
		return layers.Count;
	}

	public string GetLayerName(int i)
	{
		if ( i < layers.Count )
		{
			return layers[i].LayerName;
		}

		return "None";
	}

	public bool IsLayerOn(string name)
	{
		int index = FindLayerIndex(name);

		if ( index != -1 )
			return IsLayerOn(index);

		return false;
	}

	public bool IsLayerOn(int i)
	{
		if ( i < layers.Count )
			return layers[i].Enabled;

		return false;
	}

	public void LayerActive(string name, bool onoff)
	{
		int index = FindLayerIndex(name);
		if ( index != -1 )
			LayerActive(index, onoff);
	}

	public void LayerActive(int i, bool onoff)
	{
		if ( i < layers.Count )
			layers[i].Enabled = onoff;
	}

	public int FindLayerIndex(string name)
	{
		for ( int i = 0; i < layers.Count; i++ )
		{
			if ( layers[i].LayerName == name )
				return i;
		}

		return -1;
	}

	public void ReScatter()
	{
		update = true;
		Fill();
	}

	public int GetLayerScatterCount(string layer)
	{
		int index = FindLayerIndex(name);
		if ( index != -1 )
			return GetLayerScatterCount(index);

		return 0;
	}

	public int GetLayerScatterCount(int i)
	{
		if ( i < layers.Count )
			return layers[i].places.Count;

		return 0;
	}

	public MegaScatterLayer GetLayer(string name)
	{
		int index = FindLayerIndex(name);
		if ( index != -1 )
			return GetLayer(index);

		return null;
	}

	public MegaScatterLayer GetLayer(int i)
	{
		if ( i < layers.Count )
			return layers[i];

		return null;
	}

	public Vector3 GetLayerScatterPos(string layer, int num)
	{
		int index = FindLayerIndex(name);
		if ( index != -1 )
			return GetLayerScatterPos(index, num);

		return Vector3.zero;
	}

	public Vector3 GetLayerScatterPos(int i, int num)
	{
		if ( i < layers.Count )
		{
			if ( num < layers[i].places.Count )
				return layers[i].places[num].p;
		}

		return Vector3.zero;
	}
}

