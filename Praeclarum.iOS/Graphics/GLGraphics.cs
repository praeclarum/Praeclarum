using System;
using OpenTK;
using OpenTK.Graphics.ES11;

namespace Praeclarum.Graphics
{
	public class GLGraphics : IGraphics
	{
		public GLGraphics ()
		{
		}

		#region IGraphics implementation

		public void BeginEntity (object entity)
		{
		}

		public void SetFont (Font f)
		{
		}

		public void SetColor (Color c)
		{
			color = new Vector4 (c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
		}

		public void SetGradient (Gradient g)
		{
		}

		Vector4 color = Vector4.Zero;

		public void Clear (Color c)
		{
			GL.ClearColor (c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
			GL.Clear (ClearBufferMask.ColorBufferBit);
		}

		public void FillPolygon (Polygon poly)
		{
		}

		public void DrawPolygon (Polygon poly, float w)
		{
		}

		public void FillRect (float x, float y, float width, float height)
		{
		}

		public void DrawRect (float x, float y, float width, float height, float w)
		{
		}

		public void FillRoundedRect (float x, float y, float width, float height, float radius)
		{
		}

		public void DrawRoundedRect (float x, float y, float width, float height, float radius, float w)
		{
		}

		public void FillOval (float x, float y, float width, float height)
		{
		}

		public void DrawOval (float x, float y, float width, float height, float w)
		{
		}

		public void BeginLines (bool rounded)
		{
		}

		readonly Vector2[] lineVerts = new Vector2[2];

		public void DrawLine (float sx, float sy, float ex, float ey, float w)
		{
			lineVerts [0].X = sx;
			lineVerts [0].Y = sy;
			lineVerts [1].X = ex;
			lineVerts [1].Y = ey;

			GL.LineWidth (w);
			GL.VertexPointer (2, All.Float, 0, lineVerts);
			GL.EnableClientState (All.VertexArray);
			GL.DisableClientState (All.ColorArray);
			GL.Color4 (color.X, color.Y, color.Z, color.W);
			GL.DrawArrays (All.LineStrip, 0, lineVerts.Length);
		}

		public void EndLines ()
		{
		}

		public void FillArc (float cx, float cy, float radius, float startAngle, float endAngle)
		{
		}

		public void DrawArc (float cx, float cy, float radius, float startAngle, float endAngle, float w)
		{
		}

		public void DrawImage (IImage img, float x, float y, float width, float height)
		{
		}

		public void DrawString (string s, float x, float y, float width, float height, LineBreakMode lineBreak, TextAlignment align)
		{
		}

		public void DrawString (string s, float x, float y)
		{
		}

		public void SaveState ()
		{
		}

		public void SetClippingRect (float x, float y, float width, float height)
		{
		}

		public void Translate (float dx, float dy)
		{
		}

		public void Scale (float sx, float sy)
		{
		}

		public void RestoreState ()
		{
		}

		public IFontMetrics GetFontMetrics ()
		{
			return null;
		}

		public IImage ImageFromFile (string path)
		{
			return null;
		}

		#endregion

		void DrawLine (Vector3 begin, Vector3 end, Vector4 color, float width)
		{
		}
	}
}

