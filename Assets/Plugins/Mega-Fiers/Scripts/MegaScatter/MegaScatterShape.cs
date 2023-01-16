using UnityEngine;
using System.Collections;
using MegaFiers;

public class MegaScatterShape : MonoBehaviour
{
	// Assumes minor axis to be y
	static public bool Contains(MegaSpline spline, Vector3 p)
	{
		if ( !spline.closed )
			return false;

		int j = spline.knots.Count - 1;
		bool oddNodes = false;

		for ( int i = 0; i < spline.knots.Count; i++ )
		{
			if ( spline.knots[i].p.z < p.z && spline.knots[j].p.z >= p.z || spline.knots[j].p.z < p.z && spline.knots[i].p.z >= p.z )
			{
				if ( spline.knots[i].p.x + (p.z - spline.knots[i].p.z) / (spline.knots[j].p.z - spline.knots[i].p.z) * (spline.knots[j].p.x - spline.knots[i].p.x) < p.x )
					oddNodes = !oddNodes;
			}

			j = i;
		}

		return oddNodes;
	}

	static public bool ContainsSubDiv(MegaSpline spline, Vector3 p, int sub = 4)
	{
		if ( sub == 0 )
			return Contains(spline, p);

		if ( !spline.closed )
			return false;

		//int j = knots.Count - 1;
		bool oddNodes = false;

		for ( int i = 0; i < spline.knots.Count; i++ )
		{
			int j = i + 1;
			if ( j >= spline.knots.Count )
				j = 0;

			for ( int s = 0; s < sub; s++ )
			{
				Vector3 kp1 = spline.knots[i].Interpolate((float)s / (float)sub, spline.knots[j]);
				Vector3 kp2 = spline.knots[i].Interpolate((float)(s + 1) / (float)sub, spline.knots[j]);
				if ( kp1.z < p.z && kp2.z >= p.z || kp2.z < p.z && kp1.z >= p.z )
				{
					if ( kp1.x + (p.z - kp1.z) / (kp2.z - kp1.z) * (kp2.x - kp1.x) < p.x )
						oddNodes = !oddNodes;
				}
			}

			//j = i;
		}

		return oddNodes;
	}
}
