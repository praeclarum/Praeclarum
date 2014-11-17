//
// Copyright (c) 2010-2014 Frank A. Krueger
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
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.CoreText;
using MonoMac.Foundation;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

using DPointF = System.Drawing.PointF;
using DRectangleF = System.Drawing.RectangleF;

namespace Praeclarum.Graphics
{
#if !MONOMAC
	public class UIKitGraphics : CoreGraphicsGraphics
	{
		public UIKitGraphics (CGContext c, bool highQuality)
			: base (c, highQuality)
		{
		}
	}
#endif
	
	public class CoreGraphicsGraphics : IGraphics
	{
		CGContext _c;

		Gradient _g;

		public CGContext Context { get { return _c; } }

		//bool _highQuality = false;

		static CoreGraphicsGraphics ()
		{
		}

		public CoreGraphicsGraphics (CGContext c, bool highQuality)
		{
			if (c == null) throw new ArgumentNullException ("c");

			_c = c;
			//_highQuality = highQuality;

			if (highQuality) {
				c.SetLineCap (CGLineCap.Round);
			}
			else {
				c.SetLineCap (CGLineCap.Butt);
			}

			SetColor (Colors.Black);
		}

		public void SetGradient (Gradient g)
		{
			_g = g;
		}

		Color _color = Colors.Black;

		public void SetColor (Color c)
		{
			_color = c;
			var cgcol = c.GetCGColor ();
#if MONOMAC
			_c.SetFillColorWithColor (cgcol);
			_c.SetStrokeColorWithColor (cgcol);
#else
			_c.SetFillColor (cgcol);
			_c.SetStrokeColor (cgcol);
#endif
			_g = null;
		}

		public void Clear (Color color)
		{
			_c.ClearRect (_c.GetClipBoundingBox ());
		}

		public void FillPolygon (Polygon poly)
		{
			var count = poly.Points.Count;
			_c.MoveTo (poly.Points[0].X, poly.Points[0].Y);
			for (var i = 1; i < count; i++) {
				var p = poly.Points[i];
				_c.AddLineToPoint (p.X, p.Y);
			}
			_c.ClosePath ();
			FillPath ();
		}

		void FillPath ()
		{
			if (_g != null) {
				FillPathWithGradient ();
			} else {
				_c.FillPath ();
			}
		}

		void FillPathWithGradient ()
		{
			var gr = new CGGradient (CGColorSpace.CreateDeviceRGB (), _g.Colors.Select (c => c.GetCGColor ()).ToArray (), _g.Locations.ToArray ());
			_c.SaveState ();
			_c.Clip ();
			_c.DrawLinearGradient (gr, _g.Start.ToPointF (), _g.End.ToPointF (), CGGradientDrawingOptions.DrawsAfterEndLocation | CGGradientDrawingOptions.DrawsBeforeStartLocation);
			_c.RestoreState ();
		}

		public void DrawPolygon (Polygon poly, float w)
		{
			_c.SetLineWidth (w);
			_c.MoveTo (poly.Points[0].X, poly.Points[0].Y);
			for (var i = 1; i < poly.Points.Count; i++) {
				var p = poly.Points[i];
				_c.AddLineToPoint (p.X, p.Y);
			}
			_c.ClosePath ();
			_c.StrokePath ();
		}

		public void FillRoundedRect (float x, float y, float width, float height, float radius)
		{
			_c.AddRoundedRect (new RectangleF (x, y, width, height), radius);
			FillPath ();
		}

		public void DrawRoundedRect (float x, float y, float width, float height, float radius, float w)
		{
			_c.SetLineWidth (w);
			_c.AddRoundedRect (new RectangleF (x, y, width, height), radius);
			_c.StrokePath ();
		}

		public void FillRect (float x, float y, float width, float height)
		{
			if (_g != null) {
				_c.AddRect (new DRectangleF (x, y, width, height));
				FillPath ();
			} else {
				_c.FillRect (new DRectangleF (x, y, width, height));
			}
		}

		public void FillOval (float x, float y, float width, float height)
		{
			if (_g != null) {
				_c.AddEllipseInRect (new DRectangleF (x, y, width, height));
				FillPath ();
			} else {
				_c.FillEllipseInRect (new DRectangleF (x, y, width, height));
			}
		}

		public void DrawOval (float x, float y, float width, float height, float w)
		{
			_c.SetLineWidth (w);
			_c.StrokeEllipseInRect (new DRectangleF (x, y, width, height));
		}

		public void DrawRect (float x, float y, float width, float height, float w)
		{
			_c.SetLineWidth (w);
			_c.StrokeRect (new DRectangleF (x, y, width, height));
		}
		
		public void DrawArc (float cx, float cy, float radius, float startAngle, float endAngle, float w)
		{
			_c.SetLineWidth (w);
			_c.AddArc (cx, cy, radius, -startAngle, -endAngle, true);
			_c.StrokePath ();
		}

		public void FillArc (float cx, float cy, float radius, float startAngle, float endAngle)
		{
			_c.AddArc (cx, cy, radius, -startAngle, -endAngle, true);
			FillPath ();
		}

		const int _linePointsCount = 1024*16;
		PointF[] _linePoints = new PointF[_linePointsCount];
		bool _linesBegun = false;
		int _numLinePoints = 0;
		float _lineWidth = 1;
		bool _lineRounded = false;

		public void BeginLines (bool rounded)
		{
			_linesBegun = true;
			_lineRounded = rounded;
			_numLinePoints = 0;
		}

		public void DrawLine (float sx, float sy, float ex, float ey, float w)
		{
			if (_linesBegun) {
				
				_lineWidth = w;
				if (_numLinePoints < _linePointsCount) {
					if (_numLinePoints == 0) {
						_linePoints[_numLinePoints].X = sx;
						_linePoints[_numLinePoints].Y = sy;
						_numLinePoints++;
					}
					_linePoints[_numLinePoints].X = ex;
					_linePoints[_numLinePoints].Y = ey;
					_numLinePoints++;
				}
				
			} else {
				_c.MoveTo (sx, sy);
				_c.AddLineToPoint (ex, ey);
				_c.SetLineWidth (w);
				_c.StrokePath ();
			}
		}

		public void EndLines ()
		{
			if (!_linesBegun)
				return;
			_c.SaveState ();
			_c.SetLineJoin (_lineRounded ? CGLineJoin.Round : CGLineJoin.Miter);
			_c.SetLineWidth (_lineWidth);
			for (var i = 0; i < _numLinePoints; i++) {
				var p = _linePoints[i];
				if (i == 0) {
					_c.MoveTo (p.X, p.Y);
				} else {
					_c.AddLineToPoint (p.X, p.Y);
				}
			}
			_c.StrokePath ();
			_c.RestoreState ();
			_linesBegun = false;
		}
		
		Font _lastFont;

		public void SetFont (Font f)
		{
			if (f != _lastFont) {
				_lastFont = f;
				SelectFont ();
			}
		}
		CTStringAttributes _attrs;
		void SelectFont ()
		{
			var f = _lastFont;
			var name = "Helvetica";
			if (f.FontFamily == "Monospace") {
				if (f.IsBold) {
					name = "Courier-Bold";
				}
				else {
					name = "Courier";
				}
			}
			else if (f.FontFamily == "DBLCDTempBlack") {
#if MONOMAC
				name = "Courier-Bold";
#else
				name = f.FontFamily;
#endif
			}
			else if (f.IsBold) {
				name = "Helvetica-Bold";
			}
			_attrs = new CTStringAttributes {
				Font = new CTFont (name, f.Size),
				ForegroundColorFromContext = true,
			};
		}
		
		public void SetClippingRect (float x, float y, float width, float height)
		{
			_c.ClipToRect (new DRectangleF (x, y, width, height));
		}

		public void DrawString (string s, float x, float y)
		{
			using (var astr = new NSMutableAttributedString (s)) {
				astr.AddAttributes (_attrs, new NSRange (0, s.Length));
				using (var fs = new CTFramesetter (astr)) {
					using (var path = new CGPath ()) {
						var h = _lastFont.Size * 2;
						path.AddRect (new System.Drawing.RectangleF (0, 0, s.Length * h, h));
						using (var f = fs.GetFrame (new NSRange (0, 0), path, null)) {
							var line = f.GetLines () [0];
							float a, d, l;
							line.GetTypographicBounds (out a, out d, out l);

							_c.SaveState ();
							_c.TranslateCTM (x, h + y - d);
							_c.ScaleCTM (1, -1);

							f.Draw (_c);

							_c.RestoreState ();
						}
					}
				}
			}

//			var c = _color;
//			SetColor (Colors.Black);
//			DrawRect (x, y, 100, _lastFont.Size, 1);
//			SetColor (c);
		}

		public void DrawString (string s, float x, float y, float width, float height, LineBreakMode lineBreak, TextAlignment align)
		{
			if (_lastFont == null) return;
			var fm = GetFontMetrics ();
			var xx = x;
			var yy = y;
			if (align == TextAlignment.Center) {
				xx = (x + width / 2) - (fm.StringWidth (s) / 2);
			}
			else if (align == TextAlignment.Right) {
				xx = (x + width) - fm.StringWidth (s);
			}
			
			DrawString (s, xx, yy);
		}

		public IFontMetrics GetFontMetrics ()
		{
			var f = _lastFont;
			if (f == null) throw new InvalidOperationException ("Cannot call GetFontMetrics before calling SetFont.");

			var fm = f.Tag as CoreGraphicsFontMetrics;
			if (fm == null) {
				fm = new CoreGraphicsFontMetrics (_attrs);
				f.Tag = fm;
			}

			return fm;
		}

		public void DrawImage (IImage img, float x, float y, float width, float height)
		{
			if (img is UIKitImage) {
				var uiImg = ((UIKitImage)img).Image;
				_c.DrawImage (new DRectangleF (x, y, width, height), uiImg);
			}
		}

		public void SaveState ()
		{
			_c.SaveState ();
		}
		
		public void Translate (float dx, float dy)
		{
			_c.TranslateCTM (dx, dy);
		}
		
		public void Scale (float sx, float sy)
		{
			_c.ScaleCTM (sx, sy);
		}
		
		public void RestoreState ()
		{
			_c.RestoreState ();
			if (_lastFont != null) {
				SelectFont ();
			}
		}

		public IImage ImageFromFile (string filename)
		{
#if MONOMAC
			var img = new NSImage ("Images/" + filename);
			var rect = new DRectangleF (DPointF.Empty, img.Size);
			return new UIKitImage (img.AsCGImage (ref rect, NSGraphicsContext.CurrentContext, new MonoMac.Foundation.NSDictionary ()));
#else
			return new UIKitImage (UIImage.FromFile ("Images/" + filename).CGImage);
#endif
		}
		
		public void BeginEntity (object entity)
		{
		}
	}

	public static class ColorEx
	{
		class ColorTag {
#if MONOMAC
			public NSColor NSColor;
#else
			public UIColor UIColor;
#endif
			public CGColor CGColor;
		}
		
#if MONOMAC
		public static NSColor GetNSColor (this Color c)
		{
			var t = c.Tag as ColorTag;
			if (t == null) {
				t = new ColorTag ();
				c.Tag = t;
			}
			if (t.NSColor == null) {
				t.NSColor = NSColor.FromDeviceRgba (c.Red / 255.0f, c.Green / 255.0f, c.Blue / 255.0f, c.Alpha / 255.0f);
			}
			return t.NSColor;
		}
		public static Color GetColor (this NSColor c)
		{
			float r, g, b, a;
			c.GetRgba (out r, out g, out b, out a);
			return new Color ((int)(r * 255 + 0.5f), (int)(g * 255 + 0.5f), (int)(b * 255 + 0.5f), (int)(a * 255 + 0.5f));
		}
#else
		public static UIColor GetUIColor (this Color c)
		{
			var t = c.Tag as ColorTag;
			if (t == null) {
				t = new ColorTag ();
				c.Tag = t;
			}
			if (t.UIColor == null) {
				t.UIColor = UIColor.FromRGBA (c.Red / 255.0f, c.Green / 255.0f, c.Blue / 255.0f, c.Alpha / 255.0f);
			}
			return t.UIColor;
		}
		public static Color GetColor (this UIColor c)
		{
			float r, g, b, a;
			c.GetRGBA (out r, out g, out b, out a);
			return new Color ((int)(r * 255 + 0.5f), (int)(g * 255 + 0.5f), (int)(b * 255 + 0.5f), (int)(a * 255 + 0.5f));
		}
#endif

		public static CGColor GetCGColor (this Color c)
		{
			var t = c.Tag as ColorTag;
			if (t == null) {
				t = new ColorTag ();
				c.Tag = t;
			}
			if (t.CGColor == null) {
				t.CGColor = new CGColor (
					c.Red / 255.0f, 
					c.Green / 255.0f, 
					c.Blue / 255.0f, 
					c.Alpha / 255.0f);
			}
			return t.CGColor;
		}
	}

	public static class FontEx
	{
		/*public static UIFont CreateUIFont (this Font f)
		{
			if (f.FontFamily == "") {
				return UIFont.FromName (f.FontFamily, f.Size);
			}
			else {
				if ((f.Options & FontOptions.Bold) != 0) {
					return UIFont.BoldSystemFontOfSize (f.Size);
				}
				else {
					return UIFont.SystemFontOfSize (f.Size);
				}
			}
		}*/
		
		/*public static CTFont GetCTFont (this Font f)
		{
			var t = f.Tag as CTFont;
			if (t == null) {
				if (f.Options == FontOptions.Bold) {
					t = new CTFont ("Helvetica-Bold", f.Size);
				} else {
					t = new CTFont ("Helvetica", f.Size);
				}
				f.Tag = t;
			}
			return t;
		}*/
	}

	public class UIKitImage : IImage
	{
		public CGImage Image { get; private set; }
		public UIKitImage (CGImage image)
		{
			Image = image;
		}
	}

	public class CoreGraphicsFontMetrics : IFontMetrics
	{
		int _height = 0;
		public float[] Widths;

		readonly CTStringAttributes attrs;
		public CoreGraphicsFontMetrics (CTStringAttributes attrs)
		{
			this.attrs = attrs;
		}
		
		public int StringWidth (string str, int startIndex, int length)
		{
			return (int)(StringSize (str, startIndex, length).Width + 0.5f);
		}

		System.Drawing.SizeF StringSize (string str, int startIndex, int length)
		{
			if (str == null || length <= 0) return System.Drawing.SizeF.Empty;

			using (var astr = new NSMutableAttributedString (str)) {
				astr.AddAttributes (attrs, new NSRange (startIndex, length));
				using (var fs = new CTFramesetter (astr)) {
					using (var path = new CGPath ()) {
						path.AddRect (new System.Drawing.RectangleF (0, 0, 30000, attrs.Font.XHeightMetric * 10));
						using (var f = fs.GetFrame (new NSRange (startIndex, length), path, null)) {
							var line = f.GetLines () [0];
							float a, d, l;
							var tw = line.GetTypographicBounds (out a, out d, out l);
							return new System.Drawing.SizeF ((float)tw, (a + d) * 1.2f);
						}
					}
				}
			}
		}

		public int Height
		{
			get {
				if (_height <= 0) {
					_height = (int)(StringSize ("M", 0, 1).Height + 0.5f);
				}
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

	public static class CGContextEx
	{
#if !MONOMAC
		[System.Runtime.InteropServices.DllImport (MonoTouch.Constants.CoreGraphicsLibrary)]
		extern static void CGContextShowTextAtPoint(IntPtr c, float x, float y, byte[] bytes, int size_t_length);
		public static void ShowTextAtPoint (this CGContext c, float x, float y, byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			CGContextShowTextAtPoint (c.Handle, x, y, bytes, bytes.Length);
		}
#endif

		public static void AddRoundedRect (this CGContext c, RectangleF b, float r)
		{
			c.MoveTo (b.Left, b.Top + r);
			c.AddLineToPoint (b.Left, b.Bottom - r);
			
			c.AddArc (b.Left + r, b.Bottom - r, r, (float)(Math.PI), (float)(Math.PI / 2), true);
			
			c.AddLineToPoint (b.Right - r, b.Bottom);
			
			c.AddArc (b.Right - r, b.Bottom - r, r, (float)(-3 * Math.PI / 2), (float)(0), true);
			
			c.AddLineToPoint (b.Right, b.Top + r);
			
			c.AddArc (b.Right - r, b.Top + r, r, (float)(0), (float)(-Math.PI / 2), true);
			
			c.AddLineToPoint (b.Left + r, b.Top);
			
			c.AddArc (b.Left + r, b.Top + r, r, (float)(-Math.PI / 2), (float)(Math.PI), true);
		}

		public static void AddBottomRoundedRect (this CGContext c, RectangleF b, float r)
		{
			c.MoveTo (b.Left, b.Top + r);
			c.AddLineToPoint (b.Left, b.Bottom - r);
			
			c.AddArc (b.Left + r, b.Bottom - r, r, (float)(Math.PI), (float)(Math.PI / 2), true);
			
			c.AddLineToPoint (b.Right - r, b.Bottom);
			
			c.AddArc (b.Right - r, b.Bottom - r, r, (float)(-3 * Math.PI / 2), (float)(0), true);
			
			c.AddLineToPoint (b.Right, b.Top);
			
			c.AddLineToPoint (b.Left, b.Top);
		}
	}
}

