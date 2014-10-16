//
// Copyright (c) 2010 Frank A. Krueger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;

namespace Praeclarum.Graphics
{
    public struct PointF
    {
		public float X, Y;

		public static PointF Empty = new PointF (0, 0);

        public PointF (float x, float y)
        {
            X = x;
            Y = y;
        }

		public static PointF operator + (PointF p, VectorF v)
		{
			return new PointF (p.X + v.X, p.Y + v.Y);
		}

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }
    }

    public struct Point
    {
        public int X, Y;

        public Point (int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Size
    {
        public int Width, Height;

        public Size (int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public struct SizeF
    {
        public float Width, Height;

        public SizeF (float width, float height)
        {
            Width = width;
            Height = height;
        }
		public override string ToString ()
		{
			return string.Format ("{{ Width = {0}; Height = {1}; }}", Width, Height);
		}
    }

	public static class PointFEx
	{
		public static PointF Add (this PointF a, PointF b)
		{
			return new PointF (a.X + b.X, a.Y + b.Y);
		}

		public static PointF Add (this PointF a, float dx, float dy)
		{
			return new PointF (a.X + dx, a.Y + dy);
		}

		public static PointF Subtract (this PointF a, PointF b)
		{
			return new PointF (a.X - b.X, a.Y - b.Y);
		}

		public static PointF Multiply (this PointF a, float s)
		{
			return new PointF (a.X * s, a.Y * s);
		}

		public static float Length (this PointF a)
		{
			return (float)Math.Sqrt (a.X*a.X + a.Y*a.Y);
		}

		public static float DistanceTo (this PointF a, PointF b)
		{
			var dx = a.X - b.X;
			var dy = a.Y - b.Y;
			return (float)Math.Sqrt (dx*dx + dy*dy);
		}

		public static float DistanceSquaredTo (this PointF a, float bx, float by)
		{
			var dx = a.X - bx;
			var dy = a.Y - by;
			return dx * dx + dy * dy;
		}

		public static double DistanceSquaredTo (this PointF a, double bx, double by)
		{
			var dx = a.X - bx;
			var dy = a.Y - by;
			return dx * dx + dy * dy;
		}

		public static PointF Normalized (this PointF a)
		{
			var d = a.X * a.X + a.Y * a.Y;
			if (d <= 0) return a;
			var r = (float)(1.0 / Math.Sqrt (d));
			return new PointF (a.X * r, a.Y * r);
		}

		public static float Dot (this PointF a, PointF b)
		{
			return a.X * b.X + a.Y * b.Y;
		}

		public static PointF Lerp (this PointF s, PointF d, float t)
		{
			var dx = d.X - s.X;
			var dy = d.Y - s.Y;
			return new PointF (s.X + t * dx, s.Y + t * dy);
		}

		public static float DistanceToLine (this PointF p3, PointF p1, PointF p2)
		{
			return new LineSegmentF (p1, p2).DistanceTo (p3);
		}
	}

	public struct LineSegmentF
	{
		public float X, Y, EndX, EndY;

		public LineSegmentF (PointF begin, PointF end)
		{
			X = begin.X;
			Y = begin.Y;
			EndX = end.X;
			EndY = end.Y;
		}

		public float DistanceTo (PointF p3)
		{
			var x21 = EndX - X;
			var y21 = EndY - Y;
			var x31 = p3.X - X;
			var y31 = p3.Y - Y;

			var d = x21*x21 + y21*y21;
			if (d <= 0) return (float)Math.Sqrt (x31*x31 + y31*y31);

			var n = x31*x21 + y31*y21;
			var u = n / d;

			if (u <= 0) {
				return (float)Math.Sqrt (x31*x31 + y31*y31);
			}
			else if (u >= 1) {
				var x32 = p3.X - EndX;
				var y32 = p3.Y - EndY;
				return (float)Math.Sqrt (x32*x32 + y32*y32);
			}
			else {
				var dx = X + u*x21 - p3.X;
				var dy = Y + u*y21 - p3.Y;
				return (float)Math.Sqrt (dx*dx + dy*dy);
			}
		}

		public PointF Start {
			get { return new PointF (X, Y); }
		}

		public PointF End {
			get { return new PointF (EndX, EndY); }
		}

		public PointF MidPoint {
			get { return new PointF ((X + EndX)/2, (Y + EndY)/2); }
		}
	}
}

