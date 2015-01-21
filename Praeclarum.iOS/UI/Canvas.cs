using System;
using UIKit;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public class View : UIView, IView
	{
		Color bgColor = Colors.Black;
		public new virtual Color BackgroundColor {
			get {
				try {
					return base.BackgroundColor.GetColor ();
				} catch (Exception ex) {
					Log.Error (ex);
					return bgColor;
				}
			}
			set {
				try {
					bgColor = value;
					base.BackgroundColor = value.GetUIColor ();
				} catch (Exception ex) {
					Log.Error (ex);					
				}
			}
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

		public override void Draw (CoreGraphics.CGRect rect)
		{
			try {
				base.Draw (rect);
				
				var c = UIGraphics.GetCurrentContext ();
				
				var e = new CanvasDrawingEventArgs (
					        new CoreGraphicsGraphics (c, true),
					        Bounds.ToRectangleF ()
				        );
				
				Drawing (this, e);
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void TouchesBegan (Foundation.NSSet touches, UIEvent evt)
		{
			try {
				foreach (UITouch t in touches) {
					TouchBegan (this, new CanvasTouchEventArgs {
						TouchId = t.Handle.ToInt32 (),
						Location = t.LocationInView (this).ToPointF (),
					});
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void TouchesMoved (Foundation.NSSet touches, UIEvent evt)
		{
			try {
				foreach (UITouch t in touches) {
					TouchMoved (this, new CanvasTouchEventArgs {
						TouchId = t.Handle.ToInt32 (),
						Location = t.LocationInView (this).ToPointF (),
					});
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void TouchesEnded (Foundation.NSSet touches, UIEvent evt)
		{
			try {
				foreach (UITouch t in touches) {
					TouchEnded (this, new CanvasTouchEventArgs {
						TouchId = t.Handle.ToInt32 (),
						Location = t.LocationInView (this).ToPointF (),
					});
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void TouchesCancelled (Foundation.NSSet touches, UIEvent evt)
		{
			try {
				foreach (UITouch t in touches) {
					TouchCancelled (this, new CanvasTouchEventArgs {
						TouchId = t.Handle.ToInt32 (),
						Location = t.LocationInView (this).ToPointF (),
					});
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}
	}
}

