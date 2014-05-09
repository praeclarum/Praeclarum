//
// Copyright (c) 2014 Frank A. Krueger
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
using System.Collections.Generic;

namespace Praeclarum.Graphics
{
	public class GdipGraphics : IGraphics
	{
		const int MaxFontSize = 120;
		GdipGraphicsFontMetrics[] _fontMetrics;

		int _fontSize = 10;

        readonly System.Drawing.Graphics g;

		public GdipGraphics (System.Drawing.Graphics g)
		{
            this.g = g;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		}

		public void SetFont (Font f)
		{
			_fontSize = f.Size;
		}

        System.Drawing.Pen pen = new System.Drawing.Pen (System.Drawing.Color.Black, 1);

        Color c = Colors.Black;

		public void SetColor (Color c)
		{
            this.c = c;
		}

		public void Clear (Color c)
		{
		}

		public void FillPolygon (Polygon poly)
		{
		}

		public void DrawPolygon (Polygon poly, float w)
		{
		}

		public void FillRoundedRect (float x, float y, float width, float height, float radius)
		{
		}

		public void DrawRoundedRect (float x, float y, float width, float height, float radius, float w)
		{
		}

		public void FillRect (float x, float y, float width, float height)
		{
		}

		public void FillOval (float x, float y, float width, float height)
		{
		}

		public void DrawOval (float x, float y, float width, float height, float w)
		{
		}

		public void DrawRect (float x, float y, float width, float height, float w)
		{
		}

		public void BeginLines (bool rounded)
		{
		}

		public void DrawLine (float sx, float sy, float ex, float ey, float w)
		{
            var pen = c.GetPen (w);
            g.DrawLine (pen, sx, sy, ex, ey);
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

		public void DrawString (string s, float x, float y)
		{
		}

		public void DrawString (string s, float x, float y, float width, float height, LineBreakMode lineBreak, TextAlignment align)
		{
		}

		public IFontMetrics GetFontMetrics ()
		{
			if (_fontMetrics == null) {
				_fontMetrics = new GdipGraphicsFontMetrics[MaxFontSize + 1];
			}
			var i = Math.Min (_fontMetrics.Length, _fontSize);
			if (_fontMetrics[i] == null) {
				_fontMetrics[i] = new GdipGraphicsFontMetrics (i);
			}
			return _fontMetrics[i];
		}

		public void DrawImage (IImage img, float x, float y, float width, float height)
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

		public IImage ImageFromFile (string filename)
		{
			return null;
		}
		
		public void BeginEntity (object entity)
		{
		}
	}

	class GdipGraphicsFontMetrics : IFontMetrics
	{
		int _height;
		int _charWidth;

		public GdipGraphicsFontMetrics (int size)
		{
			_height = size;
			_charWidth = (855 * size) / 1600;
		}

		public int StringWidth (string str, int startIndex, int length)
		{
			return length * _charWidth;
		}

		public int Height
		{
			get {
				return _height;
			}
		}

		public int Ascent
		{
			get {
				return Height;
			}
		}

		public int Descent
		{
			get {
				return 0;
			}
		}
	}

    static class GdipColorEx
    {
        public static System.Drawing.Pen GetPen (this Color color, float width)
        {
            return GetData (color).GetPen (width);
        }

        static ColorData GetData (Color color)
        {
            var cd = color.Tag as ColorData;
            if (cd == null) {
                cd = new ColorData (color);
                color.Tag = cd;
            }
            return cd;
        }

        class ColorData
        {
            readonly System.Drawing.Color color;
            readonly List<System.Drawing.Pen> pens = new List<System.Drawing.Pen> ();
            public ColorData (Color color)
            {
                this.color = System.Drawing.Color.FromArgb (color.Alpha, color.Red, color.Green, color.Blue);
            }
            public System.Drawing.Pen GetPen (float width)
            {
                if (width <= 0)
                    throw new ArgumentOutOfRangeException ();
                foreach (var p in pens) {
                    if (Math.Abs (width - p.Width) / width < 0.1) {
                        return p;
                    }        
                }
                var np = new System.Drawing.Pen (color, width);
                pens.Add (np);
                return np;
            }
        }
    }
}
