
using UnityEditor;
using UnityEngine;
using MegaFiers;

public class MegaScatterEditor : Editor
{
	static public void SwapInf(MegaScatter mod, int t1, int t2)
	{
		if ( t1 >= 0 && t1 < mod.layers.Count && t2 >= 0 && t2 < mod.layers.Count && t1 != t2 )
		{
			MegaScatterLayer inf = mod.layers[t1];
			mod.layers.RemoveAt(t1);
			mod.layers.Insert(t2, inf);
			//EditorUtility.SetDirty(target);
		}
	}

	public virtual void DisplayParams()
	{

	}

	public virtual void DisplayInfCommon()
	{

	}

	public virtual void DisplayInfParams()
	{

	}
}

[CustomEditor(typeof(MegaScatterMesh))]
public class MegaScatterMeshEditor : MegaScatterEditor
{
	private     MegaScatterMesh	src;
	private     MegaScatterUndo	undoManager;

	[MenuItem("GameObject/Create Other/MegaScatter/Scatter Mesh")]
	static void CreateScatterMesh()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView != null )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Scatter Mesh");

		go.AddComponent<MegaScatterMesh>();
		go.transform.position = pos;
		Selection.activeObject = go;
	}

	private void OnEnable()
	{
		src = target as MegaScatterMesh;

		undoManager = new MegaScatterUndo(src, "Mega Scatter Mesh Param");
	}

	public override void OnInspectorGUI()
	{
		undoManager.CheckUndo();

#if !UNITY_5 && !UNITY_2017 && !UNITY_2018 && !UNITY_2019 && !UNITY_2020 && !UNITY_2021 && !UNITY_2022
		EditorGUIUtility.LookLikeControls();
#endif

		DisplayGUI();

		if ( GUI.changed )
			EditorUtility.SetDirty(target);

		undoManager.CheckDirty();
	}

	public void DisplayGUI()
	{
		MegaScatterMesh mod = (MegaScatterMesh)target;

		EditorGUILayout.LabelField("Objs: " + mod.objcount + " scattered: " + mod.scattercount + " verts: " + mod.vertcount);
		MegaShape shape = (MegaShape)EditorGUILayout.ObjectField("Shape", mod.shape, typeof(MegaShape), true);
		if ( shape != mod.shape )
		{
			mod.SetShape(shape);
		}
		mod.seed = EditorGUILayout.IntField("Seed", mod.seed);
		mod.countmode = (MegaScatterMode)EditorGUILayout.EnumPopup("Count Mode", mod.countmode);
		mod.Density = EditorGUILayout.FloatField("Density", mod.Density);
		mod.forcecount = EditorGUILayout.IntField("Count", mod.forcecount);
		mod.meshPerShape = EditorGUILayout.Toggle("Mesh Per Shape", mod.meshPerShape);

		mod.globalScale = EditorGUILayout.Vector3Field("Global Scale", mod.globalScale);
		mod.fillHoles = EditorGUILayout.Toggle("Fill Holes", mod.fillHoles);
		mod.StartCurve = EditorGUILayout.IntSlider("Start Curve", mod.StartCurve, 0, mod.NumCurves() - 1);
		mod.EndCurve = EditorGUILayout.IntSlider("End Curve", mod.EndCurve, 0, mod.NumCurves() - 1);
		mod.subdiv = EditorGUILayout.IntSlider("Spline Sub Div", mod.subdiv, 0, 8);

		mod.queryObject = (MegaScatterQuery)EditorGUILayout.ObjectField("Query Object", mod.queryObject, typeof(MegaScatterQuery), true);
		mod.dostaticbatching = EditorGUILayout.Toggle("Static Batching", mod.dostaticbatching);

		mod.raycast = EditorGUILayout.BeginToggleGroup("Raycast", mod.raycast);
		mod.NeedsGround = EditorGUILayout.Toggle("Needs Ground", mod.NeedsGround);
		mod.collisionOffset = EditorGUILayout.FloatField("Collision Offset", mod.collisionOffset);
		mod.showignoreobjs = EditorGUILayout.Foldout(mod.showignoreobjs, "Show Ignore Objects");

		if ( mod.showignoreobjs )
		{
			if ( GUILayout.Button("Add Ignore Obj") )
				mod.ignoreobjs.Add(new MegaScatterCollisionObj());

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Ignore Objects");
			for ( int i = 0; i < mod.ignoreobjs.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("" + i + ":", GUILayout.MaxWidth(20));
				GUILayout.Label("On", GUILayout.MaxWidth(20));
				mod.ignoreobjs[i].active = EditorGUILayout.Toggle("", mod.ignoreobjs[i].active, GUILayout.MaxWidth(20));
				GUILayout.Label("Children", GUILayout.MaxWidth(52));
				mod.ignoreobjs[i].includechildren = EditorGUILayout.Toggle("", mod.ignoreobjs[i].includechildren, GUILayout.MaxWidth(20));
				mod.ignoreobjs[i].collider = (Collider)EditorGUILayout.ObjectField("", mod.ignoreobjs[i].collider, typeof(Collider), true, GUILayout.MaxWidth(180));

				//mod.ignoreobjs[i] = (Collider)EditorGUILayout.ObjectField("" + i + ":", mod.ignoreobjs[i], typeof(Collider), true);
				if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
					mod.ignoreobjs.RemoveAt(i);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

		if ( mod.raycast )
		{
			if ( mod.surfaces.Count == 0 )
				EditorGUILayout.HelpBox("No Surface Objects Defined! No Objects will be scattered", MessageType.Info, true);
		}

		mod.showsurfaceobjs = EditorGUILayout.Foldout(mod.showsurfaceobjs, "Show Surface Objects");
		if ( mod.showsurfaceobjs )
		{
			if ( GUILayout.Button("Add Surface Obj") )
				mod.surfaces.Add(new MegaScatterCollisionObj());

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Surface Objects");
			for ( int i = 0; i < mod.surfaces.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("" + i + ":");
				mod.surfaces[i].active = EditorGUILayout.Toggle("", mod.surfaces[i].active, GUILayout.MaxWidth(20));
				mod.surfaces[i].collider = (Collider)EditorGUILayout.ObjectField("", mod.surfaces[i].collider, typeof(Collider), true);
				//mod.ignoreobjs[i] = (Collider)EditorGUILayout.ObjectField("" + i + ":", mod.ignoreobjs[i], typeof(Collider), true);

				mod.surfaces[i].prebuildenable = EditorGUILayout.Toggle("", mod.surfaces[i].prebuildenable, GUILayout.MaxWidth(20));
				mod.surfaces[i].postbuilddisable = EditorGUILayout.Toggle("", mod.surfaces[i].postbuilddisable, GUILayout.MaxWidth(20));

				if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
					mod.surfaces.RemoveAt(i);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

		EditorGUILayout.EndToggleGroup();

		mod.hideObjects = EditorGUILayout.Toggle("Hide Objects", mod.hideObjects);
		mod.buildOnStart = EditorGUILayout.Toggle("Build on Start", mod.buildOnStart);
		mod.useCoRoutine = EditorGUILayout.Toggle("Scatter Over Frames", mod.useCoRoutine);
		mod.NumPerFrame = EditorGUILayout.IntField("Number Per Frame", mod.NumPerFrame);
		mod.colorMesh = EditorGUILayout.Toggle("Color Mesh", mod.colorMesh);
		mod.displaygizmo = EditorGUILayout.Toggle("Display Gizmo", mod.displaygizmo);

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Hide/Show Objects") )
			mod.HideShow();

		if ( GUILayout.Button("Remove Objects") )
			mod.RemoveObjects();

		if ( GUILayout.Button("Add Mesh") )
		{
			MegaScatterLayer inf = new MegaScatterLayer();
			mod.layers.Add(inf);
			mod.currentEdit = mod.layers.Count - 1;
		}
		EditorGUILayout.EndHorizontal();

		mod.showlightmap = EditorGUILayout.Foldout(mod.showlightmap, "Lightmap Params");

		if ( mod.showlightmap )
		{
			mod.genLightMap = EditorGUILayout.BeginToggleGroup("Lightmap Enable", mod.genLightMap);
			mod.angleError = EditorGUILayout.Slider("Angle Error", mod.angleError, 0.0f, 1.0f);
			mod.areaError = EditorGUILayout.Slider("Area Error", mod.areaError, 0.0f, 1.0f);
			mod.hardAngle = EditorGUILayout.FloatField("Hard Angle", mod.hardAngle);
			mod.packMargin = EditorGUILayout.FloatField("Pack Margin", mod.packMargin);

			if ( GUILayout.Button("Build Lightmap UVS") )
			{
				if ( mod.genLightMap )
				{
					UnwrapParam uv = new UnwrapParam();
					//UnwrapParam.SetDefaults(out uv);
					uv.angleError = mod.angleError;
					uv.areaError = mod.areaError;
					uv.hardAngle = mod.hardAngle;
					uv.packMargin = mod.packMargin;

					int count = mod.gameObject.transform.childCount;	//GetChildCount();
					for ( int c = 0; c < count; c++ )
					{
						float a = (float)c / (float)count;
						if ( EditorUtility.DisplayCancelableProgressBar("Building LightMap UVs", "Mesh " + c + " of " + count, a) )
							break;

						GameObject obj = mod.gameObject.transform.GetChild(c).gameObject;
						MeshFilter mf = obj.GetComponent<MeshFilter>();

						if ( mf )
						{
							Mesh mesh = mf.sharedMesh;

							if ( mesh )
								Unwrapping.GenerateSecondaryUVSet(mesh, uv);
						}
					}

					EditorUtility.ClearProgressBar();
				}

			}
			EditorGUILayout.EndToggleGroup();
		}

		mod.showadvanced = EditorGUILayout.Foldout(mod.showadvanced, "Advanced");
		if ( mod.showadvanced )
		{
			mod.FailCount = EditorGUILayout.IntField("Fail Count", mod.FailCount);
			mod.PosFailCount = EditorGUILayout.IntField("Pos Fail Count", mod.PosFailCount);
		}

		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Layers");
		for ( int i = 0; i < mod.layers.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("" + i + " - " + mod.layers[i].LayerName);
			mod.layers[i].Enabled = EditorGUILayout.Toggle("", mod.layers[i].Enabled, GUILayout.MaxWidth(20));

			if ( GUILayout.Button("Edit", GUILayout.MaxWidth(50)) )
			{
				mod.currentEdit = i;
				EditorUtility.SetDirty(mod);
			}

			if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
				mod.layers.RemoveAt(i);

			if ( GUILayout.Button("U", GUILayout.MaxWidth(20)) )
			{
				if ( i > 0 )
					SwapInf(mod, i, i - 1);
			}

			if ( GUILayout.Button("D", GUILayout.MaxWidth(22)) )
			{
				if ( i < mod.layers.Count - 1 )
					SwapInf(mod, i, i + 1);
			}

			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		mod.showusesplines = EditorGUILayout.Foldout(mod.showusesplines, "Show Splines");

		if ( mod.showusesplines )
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Spline Selection");
			for ( int i = 0; i < mod.usespline.Count; i++ )
			{
				mod.usespline[i] = EditorGUILayout.Toggle("Spline " + i, mod.usespline[i]);
			}
			EditorGUILayout.EndVertical();
		}

		GUI.backgroundColor = Color.green;
		if ( GUILayout.Button("Update", GUILayout.Height(30)) )
		{
			mod.update = true;
			mod.Fill();
			EditorUtility.SetDirty(target);
		}
		GUI.backgroundColor = Color.white;

		if ( mod.currentEdit >= mod.layers.Count )
		{
			mod.currentEdit = mod.layers.Count - 1;
			EditorUtility.SetDirty(mod);
		}

		int ci = mod.currentEdit;

		if ( ci >= 0 )
		{
			MegaScatterLayer inf = mod.layers[ci];

			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.BeginHorizontal();
			if ( GUILayout.Button("Copy", GUILayout.Width(50)) )
				MegaScatterLayer.copylayer = inf;

			if ( MegaScatterLayer.copylayer != null )
			{
				if ( GUILayout.Button("Paste from " + MegaScatterLayer.copylayer.LayerName) )
					inf.Copy(MegaScatterLayer.copylayer);
			}
			EditorGUILayout.EndHorizontal();

			inf.LayerName = EditorGUILayout.TextField("Name", inf.LayerName);
			inf.Enabled = EditorGUILayout.BeginToggleGroup("Enabled", inf.Enabled);
			inf.markstatic = EditorGUILayout.Toggle("Mark Static", inf.markstatic);
			inf.obj = (GameObject)EditorGUILayout.ObjectField("Object", inf.obj, typeof(GameObject), true);
			inf.perCurveCount = EditorGUILayout.Toggle("Counts Per Curve", inf.perCurveCount);
			inf.forcecount = EditorGUILayout.IntField("Force Count", inf.forcecount);
			inf.maxcount = EditorGUILayout.IntField("Max Count", inf.maxcount);
			inf.weight = EditorGUILayout.FloatField("Weight", inf.weight);
			inf.scale = EditorGUILayout.FloatField("Scale", inf.scale);
			inf.offsetLow = EditorGUILayout.Vector3Field("Offset Low", inf.offsetLow);
			inf.offsetHigh = EditorGUILayout.Vector3Field("Offset High", inf.offsetHigh);
			inf.prerot = EditorGUILayout.Vector3Field("Pre Rot", inf.prerot);
			inf.rotLow = EditorGUILayout.Vector3Field("Rot Low", inf.rotLow);
			inf.rotHigh = EditorGUILayout.Vector3Field("Rot High", inf.rotHigh);
			inf.uniformScaling = EditorGUILayout.Toggle("Uniform Scaling", inf.uniformScaling);

			if ( inf.uniformScaling )
			{
				inf.uniscalemode = (MegaScatterScaleMode)EditorGUILayout.EnumPopup("Mode", inf.uniscalemode);
				inf.uniscaleLow = EditorGUILayout.FloatField("Scale Low", inf.uniscaleLow);
				inf.uniscaleHigh = EditorGUILayout.FloatField("Scale High", inf.uniscaleHigh);
			}
			else
			{
				inf.scaleLow = EditorGUILayout.Vector3Field("Scale Low", inf.scaleLow);
				inf.scaleHigh = EditorGUILayout.Vector3Field("Scale High", inf.scaleHigh);
			}

			inf.snap = EditorGUILayout.Vector3Field("Snap", inf.snap);
			inf.snapRot = EditorGUILayout.Vector3Field("Snap Rot", inf.snapRot);
			inf.distCrv = EditorGUILayout.CurveField("Dist Curve", inf.distCrv);
			inf.seed = EditorGUILayout.IntField("Seed", inf.seed);
			inf.noOverlap = EditorGUILayout.Toggle("No Overlap", inf.noOverlap);
			inf.clearOverlap = EditorGUILayout.Toggle("Clear Overlap", inf.clearOverlap);
			inf.radius = EditorGUILayout.FloatField("Radius", inf.radius);
			inf.colradiusadj = EditorGUILayout.FloatField("Col Radius Adj", inf.colradiusadj);
			inf.raycount = EditorGUILayout.IntSlider("Ray Count", inf.raycount, 1, 8);
			inf.align = EditorGUILayout.Slider("Align", inf.align, 0.0f, 1.0f);
			inf.minslope = EditorGUILayout.Slider("Min Slope", inf.minslope, 0.0f, 90.0f);
			inf.maxslope = EditorGUILayout.Slider("Max Slope", inf.maxslope, 0.0f, 90.0f);
			inf.collisionOffset = EditorGUILayout.FloatField("Collision Offset", inf.collisionOffset);

			inf.useheight = EditorGUILayout.BeginToggleGroup("Use Height Limits", inf.useheight);
			inf.minheight = EditorGUILayout.FloatField("Min Height", inf.minheight);
			inf.maxheight = EditorGUILayout.FloatField("Max Height", inf.maxheight);
			EditorGUILayout.EndToggleGroup();

			if ( mod.colorMesh )
			{
				inf.colcurve = EditorGUILayout.CurveField("Col Curve", inf.colcurve);

				inf.showcolvari = EditorGUILayout.Foldout(inf.showcolvari, "Color Variations");

				if ( inf.showcolvari )
				{
					EditorGUILayout.BeginVertical("box");
					if ( GUILayout.Button("Add Color") )
					{
						inf.colvariations.Add(Color.white);
					}

					for ( int cv = 0; cv < inf.colvariations.Count; cv++ )
					{
						EditorGUILayout.BeginHorizontal();
						inf.colvariations[cv] = EditorGUILayout.ColorField("" + cv + ":", inf.colvariations[cv]);

						if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
							inf.colvariations.RemoveAt(cv);

						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
			}

			inf.vertexlimit = EditorGUILayout.IntSlider("Vertex Limit", inf.vertexlimit, 1000, 65535);
			inf.buildtangents = EditorGUILayout.Toggle("Build Tangents", inf.buildtangents);
			inf.nocollider = EditorGUILayout.Toggle("No Collider", inf.nocollider);
			inf.buildcollider = EditorGUILayout.Toggle("Build Collider", inf.buildcollider);
			inf.proxymesh = (Mesh)EditorGUILayout.ObjectField("Collider Mesh", inf.proxymesh, typeof(Mesh), true);

			inf.vertexnoise = EditorGUILayout.BeginToggleGroup("Vertex Noise", inf.vertexnoise);
			inf.noisescale = EditorGUILayout.FloatField("Noise Scale", inf.noisescale);
			inf.strength = EditorGUILayout.Vector3Field("Strength", inf.strength);
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.EndToggleGroup();

			if ( GUILayout.Button("Delete") )
			{
				mod.layers.RemoveAt(ci);
				mod.currentEdit = 0;
				EditorUtility.SetDirty(mod);
			}
			EditorGUILayout.EndVertical();

			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("Update", GUILayout.Height(30)) )
			{
				mod.update = true;
				mod.Fill();
				EditorUtility.SetDirty(target);
			}
			GUI.backgroundColor = Color.white;
		}
	}

	static Color disccol = new Color(1, 1, 0, 0.4f);

#if false
	static Mesh mesh;

	static int[] tris = new int[6] { 0, 1, 2, 0, 2, 3 };

	void DisplayQuad(Vector3[] verts, Matrix4x4 tm)
	{
		if ( mesh == null )
		{
			mesh = new Mesh();
		}

		mesh.Clear();
		mesh.vertices = verts;
		mesh.triangles = tris;

		mesh.RecalculateNormals();
		Graphics.DrawMeshNow(mesh, tm);
	}
#endif

	// Display height and slope values
	void OnSceneGUI()
	{
		MegaScatterMesh mod = (MegaScatterMesh)target;
		if ( mod.shape )
			DisplayGizmo(mod, mod.shape.transform.localToWorldMatrix);
	}

	static public void DisplayGizmo(MegaScatter mod, Matrix4x4 tm)
	{
		if ( mod && mod.displaygizmo && mod.layers.Count > 0 )
		{
			Handles.matrix = tm;

			Handles.color = Color.green;

			if ( mod.currentEdit >= 0 )
			{
				Vector3 p1 = Vector3.zero;
				Vector3 p2 = Vector3.zero;
				Vector3 p3 = Vector3.zero;
				Vector3 p4 = Vector3.zero;

				p1.x = mod.totalbounds.min.x;
				p1.z = mod.totalbounds.min.z;

				p2.x = mod.totalbounds.max.x;
				p2.z = mod.totalbounds.min.z;

				p3.x = mod.totalbounds.max.x;
				p3.z = mod.totalbounds.max.z;

				p4.x = mod.totalbounds.min.x;
				p4.z = mod.totalbounds.max.z;

				Handles.DrawLine(p1, p2);
				Handles.DrawLine(p2, p3);
				Handles.DrawLine(p3, p4);
				Handles.DrawLine(p4, p1);

				MegaScatterLayer inf = mod.layers[mod.currentEdit];

				Handles.color = disccol;
				//Handles.CircleCap(0, Vector3.zero, Quaternion.identity, inf.radius * inf.scale * mod.globalScale.x);
				Handles.matrix = mod.transform.localToWorldMatrix;
				Handles.DrawSolidDisc(Vector3.zero, Vector3.up, inf.radius * inf.scale * mod.globalScale.x);
				
				if ( inf.useheight )
				{
					Handles.matrix = Matrix4x4.identity;

					//Vector3 hp = Vector3.zero;

					Vector3[] pv = new Vector3[4];

					//Matrix4x4 tm = mod.shape.transform.localToWorldMatrix;
					pv[0] = tm.MultiplyPoint3x4(p1);
					pv[1] = tm.MultiplyPoint3x4(p2);
					pv[2] = tm.MultiplyPoint3x4(p3);
					pv[3] = tm.MultiplyPoint3x4(p4);
					//hp = inf.
					pv[0].y = pv[1].y = pv[2].y = pv[3].y = inf.minheight;

					Handles.color = Color.grey;

					Color c = new Color(1, 1, 1, 0.25f);
					//Handles.DrawSolidRectangleWithOutline(pv, c, Color.blue);
					//DisplayQuad(pv, tm);
	#if true
					GL.PushMatrix();
					GL.Color(c);
					GL.Begin(GL.QUADS);
					GL.Vertex(pv[0]);
					GL.Vertex(pv[1]);
					GL.Vertex(pv[2]);
					GL.Vertex(pv[3]);
					GL.End();
					GL.PopMatrix();
	#endif
					Handles.color = Color.blue;
					Handles.DrawPolyLine(pv);

					pv[0].y = pv[1].y = pv[2].y = pv[3].y = inf.maxheight;
					//p1.y = p2.y = p3.y = p4.y = inf.maxheight;

					//Handles.color = Color.red;

					//Handles.DrawLine(p1, p2);
					//Handles.DrawLine(p2, p3);
					//Handles.DrawLine(p3, p4);
					//Handles.DrawLine(p4, p1);

					//Handles.DrawSolidRectangleWithOutline(pv, c, Color.red);
	#if true
					GL.PushMatrix();
					GL.Color(c);
					GL.Begin(GL.QUADS);
					GL.Vertex(pv[0]);
					GL.Vertex(pv[1]);
					GL.Vertex(pv[2]);
					GL.Vertex(pv[3]);
					GL.End();
					GL.PopMatrix();
	#endif
					Handles.color = Color.red;
					Handles.DrawPolyLine(pv);
				}
			}
		}
	}

#if false
	// virtual method for scatter extra params, so do all common then call virtual
	// same for inf, do all common then call virtual for extra
	static public void DisplayGUICommon(MegaScatterMesh mod)
	{
		//MegaScatterMesh mod = (MegaScatterMesh)target;

		//GUI.backgroundColor = Color.green;
		//if ( GUILayout.Button("Update") )
		//{
		//	mod.update = true;
		//	EditorUtility.SetDirty(target);
		//}
		//GUI.backgroundColor = Color.white;

		EditorGUILayout.LabelField("Objs: " + mod.objcount + " scattered: " + mod.scattercount + " verts: " + mod.vertcount);
		MegaShape shape = (MegaShape)EditorGUILayout.ObjectField("Shape", mod.shape, typeof(MegaShape), true);
		if ( shape != mod.shape )
		{
			mod.SetShape(shape);
		}
		mod.seed = EditorGUILayout.IntField("Seed", mod.seed);
		mod.countmode = (MegaScatterMode)EditorGUILayout.EnumPopup("Count Mode", mod.countmode);
		mod.Density = EditorGUILayout.FloatField("Density", mod.Density);
		mod.forcecount = EditorGUILayout.IntField("Count", mod.forcecount);
		mod.meshPerShape = EditorGUILayout.Toggle("Mesh Per Shape", mod.meshPerShape);

		mod.globalScale = EditorGUILayout.Vector3Field("Global Scale", mod.globalScale);
		mod.fillHoles = EditorGUILayout.Toggle("Fill Holes", mod.fillHoles);
		mod.StartCurve = EditorGUILayout.IntSlider("Start Curve", mod.StartCurve, 0, mod.NumCurves() - 1);
		mod.EndCurve = EditorGUILayout.IntSlider("End Curve", mod.EndCurve, 0, mod.NumCurves() - 1);

		mod.queryObject = (MegaScatterQuery)EditorGUILayout.ObjectField("Query Object", mod.queryObject, typeof(MegaScatterQuery), true);

		mod.raycast = EditorGUILayout.BeginToggleGroup("Raycast", mod.raycast);
		mod.NeedsGround = EditorGUILayout.Toggle("Needs Ground", mod.NeedsGround);
		mod.collisionOffset = EditorGUILayout.FloatField("Collision Offset", mod.collisionOffset);
		mod.showignoreobjs = EditorGUILayout.Foldout(mod.showignoreobjs, "Show Ignore Objects");

		if ( mod.showignoreobjs )
		{
			if ( GUILayout.Button("Add Ignore Obj") )
				mod.ignoreobjs.Add(new MegaScatterCollisionObj());

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Ignore Objects");
			for ( int i = 0; i < mod.ignoreobjs.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("" + i + ":");
				mod.ignoreobjs[i].active = EditorGUILayout.Toggle("", mod.ignoreobjs[i].active, GUILayout.MaxWidth(20));
				mod.ignoreobjs[i].collider = (Collider)EditorGUILayout.ObjectField("", mod.ignoreobjs[i].collider, typeof(Collider), true);
				//mod.ignoreobjs[i] = (Collider)EditorGUILayout.ObjectField("" + i + ":", mod.ignoreobjs[i], typeof(Collider), true);
				if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
					mod.ignoreobjs.RemoveAt(i);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

		if ( mod.raycast )
		{
			if ( mod.surfaces.Count == 0 )
				EditorGUILayout.HelpBox("No Surface Objects Defined! No Objects will be scattered", MessageType.Info, true);
		}

		mod.showsurfaceobjs = EditorGUILayout.Foldout(mod.showsurfaceobjs, "Show Surface Objects");
		if ( mod.showsurfaceobjs )
		{
			if ( GUILayout.Button("Add Surface Obj") )
				mod.surfaces.Add(new MegaScatterCollisionObj());

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Surface Objects");
			for ( int i = 0; i < mod.surfaces.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("" + i + ":");
				mod.surfaces[i].active = EditorGUILayout.Toggle("", mod.surfaces[i].active, GUILayout.MaxWidth(20));
				mod.surfaces[i].collider = (Collider)EditorGUILayout.ObjectField("", mod.surfaces[i].collider, typeof(Collider), true);
				//mod.ignoreobjs[i] = (Collider)EditorGUILayout.ObjectField("" + i + ":", mod.ignoreobjs[i], typeof(Collider), true);
				if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
					mod.surfaces.RemoveAt(i);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

		EditorGUILayout.EndToggleGroup();

		mod.hideObjects = EditorGUILayout.Toggle("Hide Objects", mod.hideObjects);
		mod.buildOnStart = EditorGUILayout.Toggle("Build on Start", mod.buildOnStart);
		mod.colorMesh = EditorGUILayout.Toggle("Color Mesh", mod.colorMesh);
		mod.displaygizmo = EditorGUILayout.Toggle("Display Gizmo", mod.displaygizmo);

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Hide/Show Objects") )
			mod.HideShow();

		if ( GUILayout.Button("Remove Objects") )
			mod.RemoveObjects();

		if ( GUILayout.Button("Add Mesh") )
		{
			MegaScatterLayer inf = new MegaScatterLayer();
			mod.layers.Add(inf);
			mod.currentEdit = mod.layers.Count - 1;
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Layers");
		for ( int i = 0; i < mod.layers.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("" + i + " - " + mod.layers[i].LayerName);
			mod.layers[i].Enabled = EditorGUILayout.Toggle("", mod.layers[i].Enabled, GUILayout.MaxWidth(20));

			if ( GUILayout.Button("Edit", GUILayout.MaxWidth(50)) )
			{
				mod.currentEdit = i;
				EditorUtility.SetDirty(mod);
			}

			if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
				mod.layers.RemoveAt(i);

			if ( GUILayout.Button("U", GUILayout.MaxWidth(20)) )
			{
				if ( i > 0 )
					SwapInf(mod, i, i - 1);
			}

			if ( GUILayout.Button("D", GUILayout.MaxWidth(22)) )
			{
				if ( i < mod.layers.Count - 1 )
					SwapInf(mod, i, i + 1);
			}

			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		mod.showusesplines = EditorGUILayout.Foldout(mod.showusesplines, "Show Splines");

		if ( mod.showusesplines )
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Spline Selection");
			for ( int i = 0; i < mod.usespline.Count; i++ )
			{
				mod.usespline[i] = EditorGUILayout.Toggle("Spline " + i, mod.usespline[i]);
			}
			EditorGUILayout.EndVertical();
		}

		GUI.backgroundColor = Color.green;
		if ( GUILayout.Button("Update", GUILayout.Height(30)) )
		{
			mod.update = true;
			EditorUtility.SetDirty(mod);
		}
		GUI.backgroundColor = Color.white;

		if ( mod.currentEdit >= mod.layers.Count )
		{
			mod.currentEdit = mod.layers.Count - 1;
			EditorUtility.SetDirty(mod);
		}

		int ci = mod.currentEdit;

		if ( ci >= 0 )
		{
			MegaScatterLayer inf = mod.layers[ci];

			EditorGUILayout.BeginVertical("box");

			inf.LayerName = EditorGUILayout.TextField("Name", inf.LayerName);
			inf.Enabled = EditorGUILayout.BeginToggleGroup("Enabled", inf.Enabled);
			inf.markstatic = EditorGUILayout.Toggle("Mark Static", inf.markstatic);
			inf.obj = (GameObject)EditorGUILayout.ObjectField("Object", inf.obj, typeof(GameObject), true);
			inf.perCurveCount = EditorGUILayout.Toggle("Counts Per Curve", inf.perCurveCount);
			inf.forcecount = EditorGUILayout.IntField("Force Count", inf.forcecount);
			inf.maxcount = EditorGUILayout.IntField("Max Count", inf.maxcount);
			inf.weight = EditorGUILayout.FloatField("Weight", inf.weight);
			inf.scale = EditorGUILayout.FloatField("Scale", inf.scale);
			inf.offsetLow = EditorGUILayout.Vector3Field("Offset Low", inf.offsetLow);
			inf.offsetHigh = EditorGUILayout.Vector3Field("Offset High", inf.offsetHigh);
			inf.prerot = EditorGUILayout.Vector3Field("Pre Rot", inf.prerot);
			inf.rotLow = EditorGUILayout.Vector3Field("Rot Low", inf.rotLow);
			inf.rotHigh = EditorGUILayout.Vector3Field("Rot High", inf.rotHigh);
			inf.uniformScaling = EditorGUILayout.Toggle("Uniform Scaling", inf.uniformScaling);

			if ( inf.uniformScaling )
			{
				inf.uniscaleLow = EditorGUILayout.FloatField("Scale Low", inf.uniscaleLow);
				inf.uniscaleHigh = EditorGUILayout.FloatField("Scale High", inf.uniscaleHigh);
			}
			else
			{
				inf.scaleLow = EditorGUILayout.Vector3Field("Scale Low", inf.scaleLow);
				inf.scaleHigh = EditorGUILayout.Vector3Field("Scale High", inf.scaleHigh);
			}

			inf.snap = EditorGUILayout.Vector3Field("Snap", inf.snap);
			inf.snapRot = EditorGUILayout.Vector3Field("Snap Rot", inf.snapRot);
			inf.distCrv = EditorGUILayout.CurveField("Dist Curve", inf.distCrv);
			inf.seed = EditorGUILayout.IntField("Seed", inf.seed);
			inf.noOverlap = EditorGUILayout.Toggle("No Overlap", inf.noOverlap);
			inf.clearOverlap = EditorGUILayout.Toggle("Clear Overlap", inf.clearOverlap);
			inf.radius = EditorGUILayout.FloatField("Radius", inf.radius);
			inf.colradiusadj = EditorGUILayout.FloatField("Col Radius Adj", inf.colradiusadj);
			inf.raycount = EditorGUILayout.IntSlider("Ray Count", inf.raycount, 1, 8);
			inf.align = EditorGUILayout.Slider("Align", inf.align, 0.0f, 1.0f);
			inf.minslope = EditorGUILayout.Slider("Min Slope", inf.minslope, 0.0f, 90.0f);
			inf.maxslope = EditorGUILayout.Slider("Max Slope", inf.maxslope, 0.0f, 90.0f);
			inf.collisionOffset = EditorGUILayout.FloatField("Collision Offset", inf.collisionOffset);

			inf.useheight = EditorGUILayout.BeginToggleGroup("Use Height Limits", inf.useheight);
			inf.minheight = EditorGUILayout.FloatField("Min Height", inf.minheight);
			inf.maxheight = EditorGUILayout.FloatField("Max Height", inf.maxheight);
			EditorGUILayout.EndToggleGroup();

			if ( mod.colorMesh )
			{
				inf.colcurve = EditorGUILayout.CurveField("Col Curve", inf.colcurve);

				inf.showcolvari = EditorGUILayout.Foldout(inf.showcolvari, "Color Variations");

				if ( inf.showcolvari )
				{
					EditorGUILayout.BeginVertical("box");
					if ( GUILayout.Button("Add Color") )
					{
						inf.colvariations.Add(Color.white);
					}

					for ( int cv = 0; cv < inf.colvariations.Count; cv++ )
					{
						EditorGUILayout.BeginHorizontal();
						inf.colvariations[cv] = EditorGUILayout.ColorField("" + cv + ":", inf.colvariations[cv]);

						if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
							inf.colvariations.RemoveAt(cv);

						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
			}

			inf.vertexlimit = EditorGUILayout.IntSlider("Vertex Limit", inf.vertexlimit, 1000, 65535);

			inf.vertexnoise = EditorGUILayout.BeginToggleGroup("Vertex Noise", inf.vertexnoise);
			inf.noisescale = EditorGUILayout.FloatField("Noise Scale", inf.noisescale);
			inf.strength = EditorGUILayout.Vector3Field("Strength", inf.strength);
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.EndToggleGroup();

			if ( GUILayout.Button("Delete") )
			{
				mod.layers.RemoveAt(ci);
				mod.currentEdit = 0;
				EditorUtility.SetDirty(mod);
			}
			EditorGUILayout.EndVertical();

			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("Update", GUILayout.Height(30)) )
			{
				mod.update = true;
				EditorUtility.SetDirty(mod);
			}
			GUI.backgroundColor = Color.white;
		}
	}
#endif
}
