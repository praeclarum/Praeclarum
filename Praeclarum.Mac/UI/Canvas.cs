using System;
using AppKit;
using CoreGraphics;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public class View : NSView, IView
	{
		RectangleF IView.Bounds {
			get { return base.Bounds.ToRectangleF (); }
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		Color bgColor = Colors.Black;
		public Color BackgroundColor {
			get {
				return bgColor;
			}
			set {
				bgColor = value;
			}
		}
	}

	public class Canvas : View, ICanvas
	{
		public event EventHandler<CanvasTouchEventArgs> TouchBegan = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchMoved = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchCancelled = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchEnded = delegate {};

		public event EventHandler<CanvasDrawingEventArgs> Drawing = delegate {};

		public Canvas ()
		{
		}

		public void Invalidate ()
		{
			SetNeedsDisplayInRect (base.Bounds);
		}

		public void Invalidate (RectangleF frame)
		{
			SetNeedsDisplayInRect (frame.ToRectangleF ());
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			try {
				var c = NSGraphicsContext.CurrentContext.GraphicsPort;
				var g = new CoreGraphicsGraphics (c, true);
				var e = new CanvasDrawingEventArgs (g, Bounds.ToRectangleF ());
				Drawing (this, e);
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}
	}
}

