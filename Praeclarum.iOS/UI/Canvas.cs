using System;
using MonoTouch.UIKit;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public class View : UIView, IView
	{
		public new virtual Color BackgroundColor {
			get { return base.BackgroundColor.GetColor (); }
			set { base.BackgroundColor = value.GetUIColor (); }
		}

		RectangleF IView.Bounds {
			get {
				return Bounds.ToRectangleF ();
			}
		}

		public View ()
		{
		}

		public View (RectangleF frame)
			: base (frame.ToRectangleF ())
		{
		}
	}

	public class Canvas : View, ICanvas
	{
		public event EventHandler<CanvasDrawingEventArgs> Drawing = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchBegan = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchMoved = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchCancelled = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchEnded = delegate {};

		public Canvas ()
		{
			Initialize ();
		}

		public Canvas (RectangleF frame)
			: base (frame)
		{
			Initialize ();
		}

		void Initialize ()
		{
			ContentMode = UIViewContentMode.Redraw;
			UserInteractionEnabled = true;
			MultipleTouchEnabled = true;
		}

		public void Invalidate ()
		{
			SetNeedsDisplay ();
		}

		public void Invalidate (RectangleF frame)
		{
			SetNeedsDisplayInRect (frame.ToRectangleF ());
		}

		public override void Draw (System.Drawing.RectangleF rect)
		{
			base.Draw (rect);

			var c = UIGraphics.GetCurrentContext ();

			var e = new CanvasDrawingEventArgs (
				new CoreGraphicsGraphics (c, true),
				Bounds.ToRectangleF ()
			);

			Drawing (this, e);
		}

		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			foreach (UITouch t in touches) {
				TouchBegan (this, new CanvasTouchEventArgs {
					TouchId = t.Handle.ToInt32 (),
					Location = t.LocationInView (this).ToPointF (),
				});
			}
		}

		public override void TouchesMoved (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			foreach (UITouch t in touches) {
				TouchMoved (this, new CanvasTouchEventArgs {
					TouchId = t.Handle.ToInt32 (),
					Location = t.LocationInView (this).ToPointF (),
				});
			}
		}

		public override void TouchesEnded (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			foreach (UITouch t in touches) {
				TouchEnded (this, new CanvasTouchEventArgs {
					TouchId = t.Handle.ToInt32 (),
					Location = t.LocationInView (this).ToPointF (),
				});
			}
		}

		public override void TouchesCancelled (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			foreach (UITouch t in touches) {
				TouchCancelled (this, new CanvasTouchEventArgs {
					TouchId = t.Handle.ToInt32 (),
					Location = t.LocationInView (this).ToPointF (),
				});
			}
		}
	}
}

