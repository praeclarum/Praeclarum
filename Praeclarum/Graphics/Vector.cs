using System;

namespace Praeclarum.Graphics
{
	public struct VectorF
	{
		public float X, Y;

		public VectorF (float x, float y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return string.Format("<{0}, {1}>", X, Y);
		}

		public static VectorF operator * (VectorF v, float s)
		{
			return new VectorF (v.X * s, v.Y * s);
		}

		public VectorF Rotate (double angle)
		{
			var cf = (float)Math.Cos (angle);
			var sf = (float)Math.Sin (angle);

			return new VectorF (X * cf - Y * sf, Y * cf + X * sf);
		}
	}
}

