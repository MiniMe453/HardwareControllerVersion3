
using UnityEngine;
using System.Collections.Generic;
using MegaFiers;

public class MegaCurveList
{
	public int		curve;
	public float	area;
	public Vector3	size;
	public Vector3	min;
	public Bounds	box = new Bounds();
}

public struct MegaTriList
{
	public int[] tris;
}

public class MegaSubMesh
{
	public List<int>	tris	= new List<int>();
}

// This should take a layer value so can do overlap via layers
public struct MegaScatterInst
{
	public Vector3	p;
	public float	r;
}

// Use for fast rescatter
public class MegaScatterObjInf
{
	//public int		layer;
	public Vector3	p;
	//public Vector3	r;
	//public Vector3	s;
	//public float	rad;
	//public Color32	col;

	public MegaScatterObjInf()	{}
	public MegaScatterObjInf(Vector3 _p)
	{
		p = _p;
	}
}

public enum MegaRGB
{
	Red,
	Green,
	Blue,
	Alpha,
}

[System.Serializable]
public class MegaScatterCol
{
	public Color lowcol = Color.black;
	public Color highcol = Color.white;
}

public enum MegaScatterScaleMode
{
	XYZ,
	XY,
	XZ,
	YZ,
}

[System.Serializable]
public class MegaScatterLayer
{
	public string			LayerName = "None";
	public bool				Enabled = true;
	public GameObject		obj;
	public Mesh				proxymesh;
	public float			weight = 100.0f;
	public float			scale;	// = 100.0f;
	public bool				uniformScaling = true;
	public float			uniscaleLow = 1.0f;
	public float			uniscaleHigh = 1.0f;
	public MegaScatterScaleMode	uniscalemode = MegaScatterScaleMode.XYZ;

	public Vector3			prerot;
	public Vector3			scaleLow;	// = new Vector3(100.0f, 100.0f, 100.0f);
	public Vector3			scaleHigh;	//  = new Vector3(100.0f, 100.0f, 100.0f);
	public Vector3			rotLow;	//  = Vector3.zero;
	public Vector3			rotHigh;	// = Vector3.zero;
	public Vector3			offsetLow;
	public Vector3			offsetHigh;
	public Vector3			snap;
	public Vector3			snapRot;
	public AnimationCurve	distCrv;	//	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0));
	public int				seed;

	public bool				noOverlap = false;
	public float			radius = 0.1f;
	public float			colradiusadj = 1.0f;
	public int				raycount = 5;

	public float			align = 0.0f;

	public bool				clearOverlap = false;
	public bool				markstatic = true;

	public float			minDistance = 0.0f;
	public float			maxDistance = 0.1f;

	public float			maxy = 1.0f;
	public float			miny = 0.0f;
	public float			colAmt = 1.0f;
	public AnimationCurve	colcurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public int				forcecount = 0;
	public int				maxcount = 100;
	public bool				perCurveCount = true;

	public List<MegaScatterCol>	scattercols = new List<MegaScatterCol>();

	public AnimationCurve	scaleOnDist = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public int				vertexlimit = 65535;

	public bool				vertexnoise = false;
	public float			noisescale = 0.0f;
	//MegaPerlin				iperlin		= MegaPerlin.Instance;
	public Vector3			strength = Vector3.zero;
	Vector3					half		= new Vector3(0.5f, 0.5f, 0.5f);

	public bool				showcolvari = false;
	public List<Color>		colvariations = new List<Color>();
	public Texture2D		colorTexture;

	public float			minslope = 0.0f;
	public float			maxslope = 90.0f;
	public float			collisionOffset = 0.0f;

	public bool				useheight = false;
	public float			minheight = 0.0f;
	public float			maxheight = 1.0f;

	public bool				buildtangents	= false;
	public bool				buildcollider	= false;
	public bool				nocollider		= true;
	public bool				disableCollider = true;

	public List<MegaScatterObjInf>	places = new List<MegaScatterObjInf>();

	Vector3[]				verts;
	Vector3[]				normals;
	Vector2[]				uvs;
	Color[]					colors;
	[HideInInspector]
	public float			tweight;
	[HideInInspector]
	public int				subcount;
	[HideInInspector]
	public int[]			remap;

	[HideInInspector]
	public float			area;
	MegaTriList[]			tris;

	public int[]			proxytris;
	public Vector3[]		proxyverts;
	public Vector3[]		proxynorms;

	static public MegaScatterLayer copylayer;
	public bool alignobjs = false;

	public MegaScatterLayer()
	{
		weight = 100.0f;
		scale = 1.0f;
		scaleLow = new Vector3(1.0f, 1.0f, 1.0f);
		scaleHigh = new Vector3(1.0f, 1.0f, 1.0f);
		rotLow = Vector3.zero;
		rotHigh = Vector3.zero;
		distCrv = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		colvariations.Add(Color.white);

		MegaScatterCol scol = new MegaScatterCol();
		scattercols.Add(scol);
	}

	public Vector3 AddNoise(Vector3 p)
	{
		Vector3 sp = (p * noisescale * scale) + half;
		//p.x += iperlin.Noise(sp.y, sp.z, 0.0f) * strength.x;
		//p.y += iperlin.Noise(sp.x, sp.z, 0.0f) * strength.y;
		//p.z += iperlin.Noise(sp.x, sp.y, 0.0f) * strength.z;

		p.x += Mathf.PerlinNoise(sp.y, sp.z) * strength.x;
		p.y += Mathf.PerlinNoise(sp.x, sp.z) * strength.y;
		p.z += Mathf.PerlinNoise(sp.x, sp.y) * strength.z;

		return p;
	}

	public float Init(float tw)
	{
		Mesh ms = null;

		if ( obj != null )
			ms = MegaUtils.GetSharedMesh(obj);

		if ( ms != null )
		{
			Matrix4x4 tm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(prerot), Vector3.one);

			verts = ms.vertices;

			miny = float.MaxValue;
			maxy = float.MinValue;

			for ( int i = 0; i < verts.Length; i++ )
			{
				verts[i] = tm.MultiplyPoint3x4(verts[i]);
				if ( verts[i].y < miny )
					miny = verts[i].y;
				if ( verts[i].y > maxy )
					maxy = verts[i].y;
			}
			uvs = ms.uv;
			normals = ms.normals;
			colors = ms.colors;

			for ( int i = 0; i < normals.Length; i++ )
				normals[i] = tm.MultiplyVector(normals[i]);

			tris = new MegaTriList[ms.subMeshCount];

			for ( int i = 0; i < ms.subMeshCount; i++ )
				tris[i].tris = ms.GetTriangles(i);

			tw += weight;
			tweight = tw;
			subcount = ms.subMeshCount;

			area = ms.bounds.size.x * scale * ms.bounds.size.z * scale;

			places.Clear();
		}

		// Proxy
		if ( proxymesh )
		{
			Matrix4x4 tm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(prerot), Vector3.one);

			proxyverts = proxymesh.vertices;

			//miny = float.MaxValue;
			//maxy = float.MinValue;

			for ( int i = 0; i < proxyverts.Length; i++ )
			{
				proxyverts[i] = tm.MultiplyPoint3x4(proxyverts[i]);
				//if ( verts[i].y < miny )
				//	miny = verts[i].y;
				//if ( verts[i].y > maxy )
				//	maxy = verts[i].y;
			}
			//uvs = ms.uv;
			proxynorms = proxymesh.normals;
			//colors = ms.colors;

			for ( int i = 0; i < proxynorms.Length; i++ )
				proxynorms[i] = tm.MultiplyVector(proxynorms[i]);

			proxytris = proxymesh.triangles;

			//for ( int i = 0; i < ms.subMeshCount; i++ )
			//	tris[i].tris = ms.GetTriangles(i);

			//tw += weight;
			//tweight = tw;
			//subcount = ms.subMeshCount;

			//area = ms.bounds.size.x * scale * ms.bounds.size.z * scale;

			//places.Clear();
		}

		return tw;
	}

	public float InitObj(float tw)
	{
		tw += weight;
		tweight = tw;

		area = 1.0f;

		Bounds bounds = new Bounds();

		Renderer[] rends = obj.GetComponentsInChildren<Renderer>();

		if ( rends != null && rends.Length > 0 )
		{
			for ( int i = 0; i < rends.Length; i++ )
				bounds.Encapsulate(rends[i].bounds);

			area = bounds.size.x * scale * bounds.size.z * scale;
		}

		places.Clear();
		return tw;
	}

	public bool AddSM(MegaScatterMesh scatter, Vector3 p, Vector3 scl, Vector3 rot, Quaternion align, float rand, Color tcol)
	{
		scatter.vertcount += verts.Length;
		scatter.scattercount++;
		int vc = scatter.verts.Count;
		int pvc = scatter.proxyverts.Count;
		if ( vc + verts.Length > vertexlimit )
		{
			scatter.BuildMeshNew(this);
			scatter.ClearMesh();
			vc = 0;
			pvc = 0;
		}

		Quaternion qr = align * Quaternion.Euler(rot);	// * align;
		Matrix4x4 mat = Matrix4x4.TRS(p, qr, scl);

		Color col;

		if ( rand >= 0.0f )
		{
			int cindex = (int)(rand * colvariations.Count);
			col = colvariations[cindex];
		}
		else
			col = tcol;

		for ( int v = 0; v < verts.Length; v++ )
		{
			Vector3 vp = mat.MultiplyPoint(verts[v]);
			if ( vertexnoise )
			{
				vp = AddNoise(vp);
			}
			scatter.verts.Add(vp);
			scatter.uvs.Add(uvs[v]);
			scatter.norms.Add(mat.MultiplyVector(normals[v]).normalized);

			if ( scatter.colorMesh )
			{
				float off = (verts[v].y - miny) / maxy;

				float cv = colcurve.Evaluate(off);
				col.a = colAmt * cv;
				scatter.colors.Add(col);	// * col);
			}
		}

		if ( !scatter.colorMesh )
		{
			if ( colors != null && colors.Length > 0 )
			{
				for ( int v = 0; v < colors.Length; v++ )
					scatter.colors.Add(colors[v]);
			}
		}

		// Need to match mat to mat array to get correct submesh
		for ( int sm = 0; sm < scatter.submeshes.Count; sm++ )
		{
			for ( int vi = 0; vi < tris[sm].tris.Length; vi++ )
				scatter.submeshes[sm].tris.Add(tris[sm].tris[vi] + vc);
		}

		places.Add(new MegaScatterObjInf(p));

		if ( proxymesh )
		{
			for ( int v = 0; v < proxyverts.Length; v++ )
			{
				Vector3 vp = mat.MultiplyPoint(proxyverts[v]);
				scatter.proxyverts.Add(vp);
				scatter.proxynorms.Add(mat.MultiplyVector(proxynorms[v]).normalized);
			}

			for ( int vi = 0; vi < proxytris.Length; vi++ )
				scatter.proxytris.Add(proxytris[vi] + pvc);
		}

		return true;
	}

	public bool AddObj(MegaScatterObject scatter, Vector3 p, Vector3 scl, Vector3 rot, Quaternion align, Quaternion pre, float rand, Color tcol)
	{
		scatter.objcount++;
		scatter.scattercount++;

		//Quaternion qr = pre * Quaternion.Euler(rot) * align;
		Quaternion qr = pre * align * Quaternion.Euler(rot);

		if ( scatter.shape )
			p = scatter.shape.transform.localToWorldMatrix.MultiplyPoint(p);
		else
			p = scatter.transform.localToWorldMatrix.MultiplyPoint(p);

		GameObject cobj = (GameObject)GameObject.Instantiate(obj, p, qr);

		if ( scatter.hideObjects )
			cobj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
		cobj.name = obj.name + " Scatter";
		cobj.transform.localScale = scl;
		cobj.transform.parent = scatter.transform;

		cobj.isStatic = markstatic;
		Renderer rend = cobj.GetComponent<Renderer>();

		if ( rand >= 0.0f )
		{
			if ( colvariations.Count > 0 )
			{
				int cindex = (int)(rand * colvariations.Count);

				Color col = colvariations[cindex];

				if ( rend && rend.sharedMaterial )
					rend.sharedMaterial.color = col;
			}
		}
		else
		{
			if ( rend && rend.sharedMaterial )
				rend.sharedMaterial.color = tcol;
		}

		places.Add(new MegaScatterObjInf(p));

		if ( disableCollider )
		{
			Collider[] cols = (Collider[])cobj.GetComponentsInChildren<Collider>();

			for ( int i = 0; i < cols.Length; i++ )
				cols[i].enabled = false;
		}

		return true;
	}

	public void Copy(MegaScatterLayer src)
	{
		LayerName = src.LayerName;
		Enabled = src.Enabled;
		obj = src.obj;
		proxymesh = src.proxymesh;
		weight = src.weight;
		scale = src.scale;
		uniformScaling = src.uniformScaling;
		uniscaleLow = src.uniscaleLow;
		uniscaleHigh = src.uniscaleHigh;

		prerot = src.prerot;
		scaleLow = src.scaleLow;
		scaleHigh = src.scaleHigh;
		rotLow = src.rotLow;
		rotHigh = src.rotHigh;
		offsetLow = src.offsetLow;
		offsetHigh = src.offsetHigh;
		snap = src.snap;
		snapRot = src.snapRot;
		distCrv = src.distCrv;
		seed = src.seed;

		noOverlap = src.noOverlap;
		radius = src.radius;
		colradiusadj = src.colradiusadj;
		raycount = src.raycount;
		align = src.align;
		clearOverlap = src.clearOverlap;
		markstatic = src.markstatic;
		minDistance = src.minDistance;
		maxDistance = src.maxDistance;
		maxy = src.maxy;
		miny = src.miny;
		colAmt = src.colAmt;
		colcurve = src.colcurve;
		forcecount = src.forcecount;
		maxcount = src.maxcount;
		perCurveCount = src.perCurveCount;

		scaleOnDist = src.scaleOnDist;
		vertexlimit = src.vertexlimit;
		vertexnoise = src.vertexnoise;
		noisescale = src.noisescale;
		strength = src.strength;
		showcolvari = src.showcolvari;
		colorTexture = src.colorTexture;
		minslope = src.minslope;
		maxslope = src.maxslope;
		collisionOffset = src.collisionOffset;
		useheight = src.useheight;
		minheight = src.minheight;
		maxheight = src.maxheight;
		buildtangents = src.buildtangents;
		buildcollider = src.buildcollider;
		nocollider = src.nocollider;
		disableCollider = src.disableCollider;

		alignobjs = src.alignobjs;
		uniscalemode = src.uniscalemode;

		scattercols.Clear();
		for ( int i = 0; i < src.scattercols.Count; i++ )
		{
			MegaScatterCol col = new MegaScatterCol();
			col.lowcol = src.scattercols[i].lowcol;
			col.highcol = src.scattercols[i].highcol;
			scattercols.Add(col);
		}

		colvariations.Clear();
		for ( int i = 0; i < src.colvariations.Count; i++ )
		{
			colvariations.Add(src.colvariations[i]);
		}
	}
}
