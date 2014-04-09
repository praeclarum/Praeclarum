using System;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace Praeclarum.UI
{
	public class ProceduralImage
	{
		public delegate void DrawFunc(CGContext c);

		public float Width { get; set; }
		public float Height { get; set; }
		public DrawFunc Draw { get; set; }

		public ProceduralImage (float width, float height, DrawFunc draw)
		{
			Width = width;
			Height = height;
			Draw = draw;
		}

		public UIImage Generate ()
		{
			UIGraphics.BeginImageContext (new System.Drawing.SizeF (Width, Height));

			var c = UIGraphics.GetCurrentContext ();

			if (Draw != null) {
				Draw (c);
			}

			var image = UIGraphics.GetImageFromCurrentImageContext ();

			UIGraphics.EndImageContext ();

			return image;
		}
	}
}

