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
using System.Linq;

namespace Praeclarum.Graphics
{
    public class GdipGraphics : IGraphics
    {
        readonly System.Drawing.Graphics g;

        System.Drawing.Pen pen = new System.Drawing.Pen (System.Drawing.Color.Black, 1);
        Color color = Colors.Black;
        //Gradient gradient = null;
        GdipFontMetrics font = null;

        public GdipGraphics (System.Drawing.Graphics g)
        {
            this.g = g;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        public void SetFont (Font f)
        {
            font = new GdipFontMetrics (f, g);
        }

        public void SetColor (Color c)
        {
            this.color = c;
        }

        public void SetGradient (Gradient g)
        {
            color = g.Colors.First ();
        }

        public void Clear (Color c)
        {
            g.Clear (c.GetGdipColor ());
        }

        public void FillPolygon (Polygon poly)
        {
            var points = new System.Drawing.PointF[poly.Points.Count];
            for (var i = 0; i < points.Length; i++) {
                points[i] = new System.Drawing.PointF (poly.Points[i].X, poly.Points[i].Y);
            }
            g.FillPolygon (color.GetBrush (), points);
        }

        public void DrawPolygon (Polygon poly, float w)
        {
            var points = new System.Drawing.PointF[poly.Points.Count];
            for (var i = 0; i < points.Length; i++) {
                points[i] = new System.Drawing.PointF (poly.Points[i].X, poly.Points[i].Y);
            }
            g.DrawPolygon (color.GetPen (w), points);
        }

        public void FillRoundedRect (float x, float y, float width, float height, float radius)
        {
            g.FillRectangle (color.GetBrush (), new System.Drawing.RectangleF (x, y, width, height));
        }

        public void DrawRoundedRect (float x, float y, float width, float height, float radius, float w)
        {
            g.DrawRectangle (color.GetPen (w), x, y, width, height);
        }

        public void FillRect (float x, float y, float width, float height)
        {
            g.FillRectangle (color.GetBrush (), new System.Drawing.RectangleF (x, y, width, height));
        }

        public void FillOval (float x, float y, float width, float height)
        {
            g.FillEllipse (color.GetBrush (), new System.Drawing.RectangleF (x, y, width, height));
        }

        public void DrawOval (float x, float y, float width, float height, float w)
        {
            g.DrawEllipse (color.GetPen (w), new System.Drawing.RectangleF (x, y, width, height));
        }

        public void DrawRect (float x, float y, float width, float height, float w)
        {
            g.DrawRectangle (color.GetPen (w), x, y, width, height);
        }

        public void BeginLines (bool rounded)
        {
        }

        public void DrawLine (float sx, float sy, float ex, float ey, float w)
        {
            var pen = color.GetPen (w);
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
            g.DrawString (s, font.Font, color.GetBrush (), new System.Drawing.PointF (x, y - font.Height * 0.25f));
        }

        public void DrawString (string s, float x, float y, float width, float height, LineBreakMode lineBreak, TextAlignment align)
        {
            g.DrawString (s, font.Font, color.GetBrush (), new System.Drawing.PointF (x, y - font.Height * 0.25f));
        }

        public IFontMetrics GetFontMetrics ()
        {
            return font;
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

    static class GdipEx
    {
        public static System.Drawing.Pen GetPen (this Color color, float width)
        {
            return GetData (color).GetPen (width);
        }

        public static System.Drawing.SolidBrush GetBrush (this Color color)
        {
            return GetData (color).GetBrush ();
        }

        public static System.Drawing.Color GetGdipColor (this Color color)
        {
            return GetData (color).Color;
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
            public readonly System.Drawing.Color Color;
            System.Drawing.SolidBrush brush;
            readonly List<System.Drawing.Pen> pens = new List<System.Drawing.Pen> ();
            public ColorData (Color color)
            {
                this.Color = System.Drawing.Color.FromArgb (color.Alpha, color.Red, color.Green, color.Blue);

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
                var np = new System.Drawing.Pen (Color, width);
                pens.Add (np);
                return np;
            }
            public System.Drawing.SolidBrush GetBrush ()
            {
                if (brush == null)
                    brush = new System.Drawing.SolidBrush (Color);
                return brush;
            }
        }        
    }

    class GdipFontMetrics : IFontMetrics
    {
        public readonly Font SourceFont;
        private readonly System.Drawing.Graphics g;
        public readonly System.Drawing.Font Font;

        public GdipFontMetrics (Font sourceFont, System.Drawing.Graphics g)
        {
            SourceFont = sourceFont;
            this.g = g;
            var scale = g.DpiY / 96.0f;
            var style = sourceFont.IsBold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular;
            Font = new System.Drawing.Font ("Arial", sourceFont.Size / (1.1f * scale), style);
        }

        public int Height => Font.Height;

        public int Ascent => Height;

        public int Descent => 0;

        public int StringWidth (string s, int startIndex, int length)
        {
            return (int)Math.Ceiling (g.MeasureString (s, Font).Width);
        }
    }
}

