
using UnityEngine;
using System.Collections.Generic;
using MegaFiers;

[ExecuteInEditMode]
public class MegaScatterMeshAlong : MegaScatterMesh
{
	public override string GetURL() { return "/?page_id=4939"; }

	void InitCurves(MegaShape shape)
	{
		totalarea = 0.0f;
		if ( shape != null )
		{
			if ( usespline.Count == 0 || usespline.Count < shape.splines.Count )
					SetShape(shape);

			curves.Clear();

			for ( int i = 0; i < shape.splines.Count; i++ )
			{
				if ( usespline[i] )
				{
					MegaCurveList curve = new MegaCurveList();
					curve.curve = i;
					curve.area = Mathf.Abs(shape.splines[i].length);	//Mathf.Abs(shape.splines[i].Area());
					totalarea += curve.area;
					curve.box = new Bounds(shape.splines[i].knots[0].p, Vector3.zero);

					for ( int k = 1; k < shape.splines[i].knots.Count; k++ )
						curve.box.Encapsulate(shape.splines[i].knots[k].p);

					curve.min = curve.box.min;
					curve.size = curve.box.size;
					curves.Add(curve);
				}
			}
		}
	}

	public override int NumCurves()
	{
		if ( shape )
			return shape.splines.Count;

		return 0;
	}

	void Init()
	{
		if ( shape != null )
		{
			transform.position = shape.gameObject.transform.position;
			transform.localScale = shape.gameObject.transform.localScale;
			transform.rotation = shape.gameObject.transform.rotation;
		}

		InitCurves(shape);
		RemoveObjects();
		CollectColliders();
	}

	bool FindPosAlong(int s, int m, ref Vector3 p, Vector3 scl, ref float alphar, float rad, ref float posalpha)
	{
		float r = rad;

		int kn = 0;
		Vector3 tan = Vector3.zero;
		float alpha = 0.0f;

		int fails = PosFailCount;
		while ( true )
		{
			fails--;
			if ( fails == 0 )
				break;

			p = new Vector3((Random.value * curves[s].size.x) + curves[s].min.x, 0.0f, (Random.value * curves[s].size.z) + curves[s].min.z);
			p = Snap(p, layers[m].snap);

			Vector3 np = shape.FindNearestPoint(s, p, 5, ref kn, ref tan, ref alpha);

			posalpha = alpha;
			// Move the object to fit within min and max
			float a = Random.value;
			alphar = a;

			Vector3 norm = (p - np).normalized;

			p = np + (norm * Mathf.Lerp(layers[m].minDistance, layers[m].maxDistance, a));
			// Have min and max distance

			if ( layers[m].noOverlap )
			{
				if ( !Overlap(p, r) )
					return true;
			}
			else
				return true;
		}

		return false;
	}

	public override void Fill()
	{
		if ( shape == null || layers.Count == 0 )
			return;

		if ( update )
		{
			objcount = 0;
			vertcount = 0;
			scattercount = 0;
			Init();
			update = false;
			float totalweight = 0.0f;

			for ( int i = 0; i < layers.Count; i++ )
			{
				if ( layers[i].Enabled )
					totalweight = layers[i].Init(totalweight);
			}

			int sc = StartCurve;
			if ( sc < 0 ) sc = 0;
			if ( sc >= curves.Count )
				return;

			int ec = EndCurve;
			if ( ec < sc )
				ec = sc;

			ec++;
			if ( ec > curves.Count )
				ec = curves.Count;

			instances.Clear();	// May not be right place

			int fullcount = (int)(totalarea * Density);
			if ( countmode == MegaScatterMode.Count )
				fullcount = forcecount;

			for ( int m = 0; m < layers.Count; m++ )
			{
				if ( layers[m].Enabled )
				{
					int meshcount = (int)(fullcount * (layers[m].weight / totalweight));

					if ( !meshPerShape )
						ClearMesh(layers[m].subcount);

					// Do this if want overlap only for each mesh
					if ( layers[m].clearOverlap )
						instances.Clear();	// May not be right place

#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_2017 || UNITY_2018 || UNITY_2019 || UNITY_2020 || UNITY_2021 || UNITY_2022
					Random.InitState(seed + layers[m].seed + m);
#else
					Random.seed = seed + layers[m].seed + m;
#endif
					for ( int s = sc; s < ec; s++ )
					{
						//Random.seed = layers[m].seed + s + m;

						if ( meshPerShape )
							ClearMesh(layers[m].subcount);

						int count = (int)(meshcount * (curves[s].area / totalarea));

						if ( layers[m].forcecount != 0 )
							count = layers[m].forcecount;

						if ( layers[m].maxcount != 0 && count > layers[m].maxcount )
							count = layers[m].maxcount;

						Vector3 p = Vector3.zero;

						int failcount = FailCount;

						for ( int i = 0; i < count; i++ )
						{
							float r1 = Random.value;
							float r2 = Random.value;
							float r3 = Random.value;
							float r4 = Random.value;
							float r5 = Random.value;
							float r6 = Random.value;
							float r7 = Random.value;
							float r8 = Random.value;
							float r9 = Random.value;
							float rc = Random.value;

							float alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r1));
							float alpha1 = 0.0f;
							float alpha2 = 0.0f;

							Vector3 scl = Vector3.one;

							if ( layers[m].uniformScaling )
							{
								Vector3 low = globalScale * layers[m].uniscaleLow;
								Vector3 high = globalScale * layers[m].uniscaleHigh;
								//scl = (low + (alpha * (high - low))) * 1.0f * (layers[m].scale);

								switch ( layers[m].uniscalemode )
								{
									case MegaScatterScaleMode.XYZ:
										scl = (low + (alpha * (high - low))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.XY:
										scl.y = scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.z = (low.z + (alpha1 * (high.z - low.z))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.XZ:
										scl.z = scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.y = (low.y + (alpha1 * (high.y - low.y))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.YZ:
										scl.y = scl.z = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.x = (low.x + (alpha1 * (high.x - low.x))) * 1.0f * (layers[m].scale);
										break;
								}
							}
							else
							{
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r2));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r3));

								Vector3 low = Vector3.Scale(globalScale, layers[m].scaleLow);
								Vector3 high = Vector3.Scale(globalScale, layers[m].scaleHigh);
								scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
								scl.y = (low.y + (alpha1 * (high.y - low.y))) * 1.0f * (layers[m].scale);
								scl.z = (low.z + (alpha2 * (high.z - low.z))) * 1.0f * (layers[m].scale);
							}

							alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r4));
							alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r5));
							alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r6));
							Vector3 rot = Vector3.zero;

							rot.x = (layers[m].rotLow.x + (alpha * (layers[m].rotHigh.x - layers[m].rotLow.x)));
							rot.y = (layers[m].rotLow.y + (alpha * (layers[m].rotHigh.y - layers[m].rotLow.y)));
							rot.z = (layers[m].rotLow.z + (alpha * (layers[m].rotHigh.z - layers[m].rotLow.z)));
							rot = Snap(rot, layers[m].snapRot);

							float distalpha = 0.0f;

							float maxscale = Mathf.Abs(scl.x);
							if ( Mathf.Abs(scl.z) > maxscale )
								maxscale = Mathf.Abs(scl.z);

							float rad = layers[m].radius * maxscale;

							float posalpha = 0.0f;

							if ( FindPosAlong(s, m, ref p, scl, ref distalpha, rad, ref posalpha) )
							{
								bool addedmesh = false;

								Vector3 p1 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha, true));
								Vector3 p2 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha + 0.001f, true));
								Quaternion alignrot = Quaternion.LookRotation(p2 - p1);

								alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r7));
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r8));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r9));

								Vector3 off = Vector3.zero;

								off.x = (layers[m].offsetLow.x + (alpha * (layers[m].offsetHigh.x - layers[m].offsetLow.x)));
								off.y = (layers[m].offsetLow.y + (alpha1 * (layers[m].offsetHigh.y - layers[m].offsetLow.y)));
								off.z = (layers[m].offsetLow.z + (alpha2 * (layers[m].offsetHigh.z - layers[m].offsetLow.z)));

								off = alignrot * off;

								p.x += off.x;	//(layers[m].offsetLow.x + (alpha * (layers[m].offsetHigh.x - layers[m].offsetLow.x)));
								p.y += off.y;	//(layers[m].offsetLow.y + (alpha1 * (layers[m].offsetHigh.y - layers[m].offsetLow.y)));
								p.z += off.z;	//(layers[m].offsetLow.z + (alpha2 * (layers[m].offsetHigh.z - layers[m].offsetLow.z)));

								Vector3 wp = shape.transform.localToWorldMatrix.MultiplyPoint3x4(p);
								Vector3 lp = transform.worldToLocalMatrix.MultiplyPoint3x4(wp);

								scl *= layers[m].scaleOnDist.Evaluate(distalpha);

								if ( raycast )
								{
									if ( MultiRayCast(wp, rad * layers[m].colradiusadj, layers[m].raycount) )
									{
										RaycastHit hit;
										Vector3 o = wp;	//collisionOrigin + p;
										o.y += collisionOffset;

										if ( Physics.Raycast(o, Vector3.down, out hit, raycastheight) )
										{
											float ang = Mathf.Acos(Vector3.Dot(hit.normal, Vector3.up)) * Mathf.Rad2Deg;

											p = hit.point;

											if ( !layers[m].useheight || (p.y < layers[m].maxheight && p.y > layers[m].minheight) )
											{
												if ( ang >= layers[m].minslope && ang <= layers[m].maxslope )
												{
													//p.y += layers[m].collisionOffset * scl.y;
													p = transform.worldToLocalMatrix.MultiplyPoint3x4(p);
													Vector3 hn = transform.worldToLocalMatrix.MultiplyVector(hit.normal);

													Vector3 newup = Vector3.Lerp(Vector3.up, hn, layers[m].align).normalized;
													p += newup * (layers[m].collisionOffset * scl.y);
													Quaternion align = Quaternion.FromToRotation(Vector3.up, newup);

													if ( layers[m].alignobjs )
													{
														//Quaternion alignrot = Quaternion.identity;
														//Vector3 p1 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha, true));
														//Vector3 p2 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha + 0.001f, true));

														alignrot = Quaternion.LookRotation(p2 - p1);
														layers[m].AddSM(this, p, scl, rot, alignrot * align, rc, Color.black);
													}
													else
														layers[m].AddSM(this, p, scl, rot, align, rc, Color.black);

													addedmesh = true;
												}
											}
										}
									}
									else
									{
										if ( !NeedsGround )
										{
											if ( layers[m].alignobjs )
											{
												//Quaternion alignrot = Quaternion.identity;
												//Vector3 p1 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha, true));
												//Vector3 p2 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha + 0.001f, true));

												alignrot = Quaternion.LookRotation(p2 - p1);
												layers[m].AddSM(this, p, scl, rot, alignrot, rc, Color.black);
											}
											else
												layers[m].AddSM(this, lp, scl, rot, Quaternion.identity, rc, Color.black);
											addedmesh = true;
										}
									}
								}
								else
								{
									if ( layers[m].alignobjs )
									{
										//Quaternion alignrot = Quaternion.identity;
										//Vector3 p1 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha, true));
										//Vector3 p2 = shape.transform.localToWorldMatrix.MultiplyPoint3x4(shape.InterpCurve3D(s, posalpha + 0.001f, true));

										alignrot = Quaternion.LookRotation(p2 - p1);
										layers[m].AddSM(this, p, scl, rot, alignrot, rc, Color.black);
									}
									else
										layers[m].AddSM(this, lp, scl, rot, Quaternion.identity, rc, Color.black);
									addedmesh = true;
								}

								if ( !addedmesh )
								{
									i--;
									failcount--;
									if ( failcount < 0 )
										break;
								}
							}
						}

						if ( meshPerShape )
							BuildMeshNew(layers[m]);
					}
					if ( !meshPerShape )
						BuildMeshNew(layers[m]);
				}
			}

			RestoreColliders();

			if ( dostaticbatching )
				StaticBatchingUtility.Combine(gameObject);
		}
	}
}
