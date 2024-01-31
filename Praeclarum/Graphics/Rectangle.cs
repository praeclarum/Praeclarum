using System;

#if __UNIFIED__
using NativeSize = CoreGraphics.CGSize;
using NativePoint = CoreGraphics.CGPoint;
using NativeRect = CoreGraphics.CGRect;
#endif

namespace Praeclarum.Graphics
{
	public struct RectangleF
	{
		public float X, Y, Width, Height;

		public float Left { get { return X; } }
		public float Top { get { return Y; } }

		public float Right { get { return X + Width; } }
		public float Bottom { get { return Y + Height; } }

		public PointF BottomLeft { get { return new PointF (Left, Bottom); } }
		public PointF BottomRight { get { return new PointF (Right, Bottom); } }
		public PointF TopLeft { 
			get { return new PointF (Left, Top); }
			set { X = value.X; Y = value.Y; }
		}
		public PointF TopRight { get { return new PointF (Right, Top); } }

		public RectangleF (float left, float top, float width, float height)
		{
			X = left;
			Y = top;
			Width = width;
			Height = height;
		}

		public RectangleF (PointF origin, SizeF size)
		{
			X = origin.X;
			Y = origin.Y;
			Width = size.Width;
			Height = size.Height;
		}

		public void Inflate (float width, float height)
		{
			Inflate (new SizeF (width, height));
		}

		public void Inflate (SizeF size)
		{
			X -= size.Width;
			Y -= size.Height;
			Width += size.Width * 2;
			Height += size.Height * 2;
		}

		public bool IntersectsWith(RectangleF rect)
		{
			return !((Left >= rect.Right) || (Right <= rect.Left) ||
			         (Top >= rect.Bottom) || (Bottom <= rect.Top));
		}

		public bool Contains(PointF loc)
		{
			return (X <= loc.X && loc.X < (X + Width) && Y <= loc.Y && loc.Y < (Y + Height));
		}

		public static RectangleF Union (RectangleF a, RectangleF b)
		{
			var left = Math.Min (a.Left, b.Left);
			var top = Math.Min (a.Top, b.Top);
			return new RectangleF (left,
				top,
				Math.Max (a.Right, b.Right) - left,
				Math.Max (a.Bottom, b.Bottom) - top);
		}

		public override string ToString()
		{
			return string.Format("[RectangleF: Left={0} Top={1} Width={2} Height={3}]", Left, Top, Width, Height);
		}
	}

	public struct Rectangle
	{
		public int X, Y, Width, Height;

		public int Left { get { return X; } }

		public int Top { get { return Y; } }

		public int Bottom { get { return Top + Height; } }

		public int Right { get { return Left + Width; } }

		public Rectangle (int left, int top, int width, int height)
		{
			X = left;
			Y = top;
			Width = width;
			Height = height;
		}

		public void Offset (int dx, int dy)
		{
			X += dx;
			Y += dy;
		}

		public bool Contains (int x, int y)
		{
			return (x >= X && x < X + Width) && (y >= Y && y < Y + Height);
		}

		public static Rectangle Union (Rectangle a, Rectangle b)
		{
			var left = Math.Min (a.Left, b.Left);
			var top = Math.Min (a.Top, b.Top);
			return new Rectangle (left,
			                      top,
			                      Math.Max (a.Right, b.Right) - left,
			                      Math.Max (a.Bottom, b.Bottom) - top);
		}

		public bool IntersectsWith (Rectangle rect)
		{
			return !((Left >= rect.Right) || (Right <= rect.Left) ||
			         (Top >= rect.Bottom) || (Bottom <= rect.Top));
		}

		public void Inflate (int width, int height)
		{
			Inflate (new Size (width, height));
		}

		public void Inflate (Size size)
		{
			X -= size.Width;
			Y -= size.Height;
			Width += size.Width * 2;
			Height += size.Height * 2;
		}

		public override string ToString ()
		{
			return string.Format ("[Rectangle: Left={0} Top={1} Width={2} Height={3}]", Left, Top, Width, Height);
		}
	}

	public static partial class RectangleEx
	{
		public static RectangleF ToRectangleF (this System.Drawing.Rectangle rect)
		{
			return new RectangleF (rect.X, rect.Y, rect.Width, rect.Height);
		}
	}
}

