
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MegaFiers;

[ExecuteInEditMode]
public class MegaScatterObject : MegaScatter
{
	public override string GetURL() { return "/?page_id=4957"; }

	public bool Overlap(Vector3 p, float r)
	{
		for ( int i = 0; i < instances.Count; i++ )
		{
			float ds = Vector3.Magnitude(p - instances[i].p);

			if ( (ds - r) < instances[i].r )
				return true;
		}

		MegaScatterInst inst = new MegaScatterInst();
		inst.p = p;
		inst.r = r;
		instances.Add(inst);
		return false;
	}

	void InitCurves(MegaShape shape)
	{
		totalbounds.size = Vector3.zero;

		totalarea = 0.0f;

		if ( shape != null )
		{
			if ( usespline.Count == 0 || usespline.Count < shape.splines.Count )
				SetShape(shape);

			curves.Clear();

			for ( int i = 0; i < shape.splines.Count; i++ )
			{
				if ( shape.splines[i].closed && usespline[i] )
				{
					MegaCurveList curve = new MegaCurveList();
					curve.curve = i;
					totalarea += shape.splines[i].Area();
					curve.area = Mathf.Abs(shape.splines[i].Area());
					curve.box = new Bounds(shape.splines[i].knots[0].p, Vector3.zero);

					for ( int k = 1; k < shape.splines[i].knots.Count; k++ )
						curve.box.Encapsulate(shape.splines[i].knots[k].p);

					curve.min = curve.box.min;
					curve.size = curve.box.size;
					curves.Add(curve);

					totalbounds.Encapsulate(curve.box);
				}
			}
		}
	}

	public override int NumCurves()
	{
		int count = 0;

		if ( shape != null )
		{
			for ( int i = 0; i < shape.splines.Count; i++ )
			{
				if ( shape.splines[i].closed )
					count++;
			}
		}

		return count;
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

#if false
	void Update()
	{
		//Fill();
		if ( useCoRoutine )
			StartCoroutine(FillCo());
		else
			Fill();
	}
#endif

	public Vector3 Snap(Vector3 p, Vector3 snap)
	{
		for ( int i = 0; i < 3; i++ )
			p[i] = snap[i] != 0.0f ? (int)(p[i] / snap[i]) * snap[i] : p[i];

		return p;
	}

	bool FindPos(int s, int m, ref Vector3 p, Vector3 scl, float rad)
	{
		float r = rad;

		int hv = 0;
		if ( fillHoles )
			hv = 1;

		int fails = PosFailCount;
		while ( true )
		{
			fails--;
			if ( fails == 0 )
				break;

			p = new Vector3((Random.value * curves[s].size.x) + curves[s].min.x, 0.0f, (Random.value * curves[s].size.z) + curves[s].min.z);
			p = Snap(p, layers[m].snap);

			//if ( shape.splines[curves[s].curve].ContainsSubDiv(p, subdiv) )
			if ( MegaScatterShape.ContainsSubDiv(shape.splines[curves[s].curve], p, subdiv) )
			{
					int hole = 0;

				for ( int cs = 0; cs < curves.Count; cs++ )
				{
					if ( cs != s )
					{
						//if ( shape.splines[curves[cs].curve].ContainsSubDiv(p, subdiv) )
						if ( MegaScatterShape.ContainsSubDiv(shape.splines[curves[cs].curve], p, subdiv) )
							hole++;
					}
				}

				if ( (hole & 1) == hv )
				{
					if ( layers[m].noOverlap )
					{
						if ( !Overlap(p, r) )
							return true;
					}
					else
						return true;
				}
			}
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
					totalweight = layers[i].InitObj(totalweight);
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
				if ( layers[m].Enabled && layers[m].obj )
				{
					int meshcount = (int)(fullcount * (layers[m].weight / totalweight));

					if ( !layers[m].perCurveCount )
					{
						if ( layers[m].forcecount != 0 )
							meshcount = layers[m].forcecount;

						if ( layers[m].maxcount != 0 && meshcount > layers[m].maxcount )
							meshcount = layers[m].maxcount;
					}

					Quaternion prerot = Quaternion.Euler(layers[m].prerot);

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
						//Random.seed = seed + layers[m].seed + s + m;

						int count = (int)(meshcount * (curves[s].area / totalarea));

						if ( layers[m].perCurveCount )
						{
							if ( layers[m].forcecount != 0 )
								count = layers[m].forcecount;

							if ( layers[m].maxcount != 0 && count > layers[m].maxcount )
								count = layers[m].maxcount;
						}

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

							float maxscale = Mathf.Abs(scl.x);
							if ( Mathf.Abs(scl.z) > maxscale )
								maxscale = Mathf.Abs(scl.z);

							float rad = layers[m].radius * maxscale;

							if ( FindPos(s, m, ref p, scl, rad) )
							{
								bool addedmesh = false;

								alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r7));
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r8));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r9));

								p.x += (layers[m].offsetLow.x + (alpha * (layers[m].offsetHigh.x - layers[m].offsetLow.x)));
								p.y += (layers[m].offsetLow.y + (alpha1 * (layers[m].offsetHigh.y - layers[m].offsetLow.y)));
								p.z += (layers[m].offsetLow.z + (alpha2 * (layers[m].offsetHigh.z - layers[m].offsetLow.z)));

								Vector3 wp = shape.transform.localToWorldMatrix.MultiplyPoint3x4(p);
								Vector3 lp = transform.worldToLocalMatrix.MultiplyPoint3x4(wp);

								if ( raycast )
								{
									if ( MultiRayCast(wp, rad * layers[m].colradiusadj, layers[m].raycount) )
									{
										RaycastHit hit;
										Vector3 o = wp;
										o.y += collisionOffset;

										// This should always pass
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

													layers[m].AddObj(this, p, scl, rot, align, prerot, rc, Color.black);
													addedmesh = true;
												}
											}
										}
									}
									else
									{
										if ( !NeedsGround )
										{
											layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, Color.black);
											addedmesh = true;
										}
									}
								}
								else
								{
									layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, Color.black);
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
					}
				}
			}

			RestoreColliders();

			Collider[] cols = gameObject.GetComponentsInChildren<Collider>();
			for ( int i = 0; i < cols.Length; i++ )
				cols[i].enabled = true;

			if ( dostaticbatching )
				StaticBatchingUtility.Combine(gameObject);
		}
	}

	public override IEnumerator FillCo()
	{
		if ( curves.Count == 0 )
			Init();

		if ( shape != null && layers.Count > 0 && update && StartCurve < curves.Count )
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
					totalweight = layers[i].InitObj(totalweight);
			}

			int sc = StartCurve;
			if ( sc < 0 ) sc = 0;
			//if ( sc >= curves.Count )
				//return;

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

			int placed = 0;
			if ( NumPerFrame < 1 )
				NumPerFrame = 1;

			for ( int m = 0; m < layers.Count; m++ )
			{
				if ( layers[m].Enabled && layers[m].obj )
				{
					int meshcount = (int)(fullcount * (layers[m].weight / totalweight));

					if ( !layers[m].perCurveCount )
					{
						if ( layers[m].forcecount != 0 )
							meshcount = layers[m].forcecount;

						if ( layers[m].maxcount != 0 && meshcount > layers[m].maxcount )
							meshcount = layers[m].maxcount;
					}

					Quaternion prerot = Quaternion.Euler(layers[m].prerot);

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
						//Random.seed = seed + layers[m].seed + s + m;

						int count = (int)(meshcount * (curves[s].area / totalarea));

						if ( layers[m].perCurveCount )
						{
							if ( layers[m].forcecount != 0 )
								count = layers[m].forcecount;

							if ( layers[m].maxcount != 0 && count > layers[m].maxcount )
								count = layers[m].maxcount;
						}

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

							float maxscale = Mathf.Abs(scl.x);
							if ( Mathf.Abs(scl.z) > maxscale )
								maxscale = Mathf.Abs(scl.z);

							float rad = layers[m].radius * maxscale;

							if ( FindPos(s, m, ref p, scl, rad) )
							{
								bool addedmesh = false;

								alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r7));
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r8));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r9));

								p.x += (layers[m].offsetLow.x + (alpha * (layers[m].offsetHigh.x - layers[m].offsetLow.x)));
								p.y += (layers[m].offsetLow.y + (alpha1 * (layers[m].offsetHigh.y - layers[m].offsetLow.y)));
								p.z += (layers[m].offsetLow.z + (alpha2 * (layers[m].offsetHigh.z - layers[m].offsetLow.z)));

								Vector3 wp = shape.transform.localToWorldMatrix.MultiplyPoint3x4(p);
								Vector3 lp = transform.worldToLocalMatrix.MultiplyPoint3x4(wp);

								if ( raycast )
								{
									if ( MultiRayCast(wp, rad * layers[m].colradiusadj, layers[m].raycount) )
									{
										RaycastHit hit;
										Vector3 o = wp;
										o.y += collisionOffset;

										// This should always pass
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

													layers[m].AddObj(this, p, scl, rot, align, prerot, rc, Color.black);
													addedmesh = true;
												}
											}
										}
									}
									else
									{
										if ( !NeedsGround )
										{
											layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, Color.black);
											addedmesh = true;
										}
									}
								}
								else
								{
									layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, Color.black);
									addedmesh = true;
								}

								if ( !addedmesh )
								{
									i--;
									failcount--;
									if ( failcount < 0 )
										break;
								}
								else
								{
									placed++;
									if ( placed > NumPerFrame )
									{
										placed = 0;
										yield return null;
									}
								}
							}
						}
					}
				}
			}

			RestoreColliders();

			Collider[] cols = gameObject.GetComponentsInChildren<Collider>();
			for ( int i = 0; i < cols.Length; i++ )
				cols[i].enabled = true;

			if ( dostaticbatching )
				StaticBatchingUtility.Combine(gameObject);
		}
	}

}
