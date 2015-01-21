using System;
using UIKit;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public class ScrollableCanvas : View, ICanvas
	{
		public event EventHandler<CanvasDrawingEventArgs> Drawing = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchBegan = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchMoved = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchCancelled = delegate {};
		public event EventHandler<CanvasTouchEventArgs> TouchEnded = delegate {};

		Scroller scroll;
		Canvas scrollContent;

		Canvas canvas;

		RectangleF visibleArea;

		SizeF contentSize = new SizeF (768, 1024);

		public bool TouchEnabled { get; set; }

		public bool TouchDelayed {
			get { return scroll.DelaysContentTouches; }
			set { scroll.DelaysContentTouches = value; }
		}

		public bool ScrollingEnabled {
			get { return scroll.ScrollEnabled; }
			set { scroll.ScrollEnabled = true; scroll.UserInteractionEnabled = true; }
		}

		public float Zoom {
			get { return (float)scroll.ZoomScale; }
		}

		public ScrollableCanvas ()
		{
			Initialize ();
		}

		public ScrollableCanvas (RectangleF frame)
			: base (frame)
		{
			Initialize ();
		}

		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				canvas.BackgroundColor = value;
			}
		}

		void Initialize ()
		{
			var bounds = Bounds;

			visibleArea = bounds.ToRectangleF ();

			canvas = new Canvas (bounds.ToRectangleF ()) {
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
			};

			BackgroundColor = Colors.White;

			scrollContent = new Canvas (new RectangleF (PointF.Empty, contentSize)) {
				Opaque = false,
				BackgroundColor = UIColor.Clear.GetColor (),
			};
			scrollContent.TouchBegan += HandleTouchBegan;
			scrollContent.TouchMoved += HandleTouchMoved;
			scrollContent.TouchEnded += HandleTouchEnded;
			scrollContent.TouchCancelled += HandleTouchCancelled;

			scroll = new Scroller (bounds) {
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				MinimumZoomScale = 1/4.0f,
				MaximumZoomScale = 4.0f,
				AlwaysBounceVertical = true,
				AlwaysBounceHorizontal = true,
				BackgroundColor = UIColor.Clear,
			};

			scroll.AddSubview (scrollContent);
			scroll.ContentSize = contentSize.ToSizeF ();

			TouchEnabled = true;
			TouchDelayed = true;

			scroll.ViewForZoomingInScrollView = delegate {
				return scrollContent;
			};
			scroll.ZoomingEnded += delegate {
			};
			scroll.Scrolled += HandleScrolled;

			AddSubviews (canvas, scroll);

			//
			// Prime the visible area
			//
			HandleScrolled (scroll, EventArgs.Empty);

			//
			// Ready to Draw
			//
			SetVisibleArea ();
			canvas.Drawing += HandleDrawing;
		}

		void HandleTouchBegan (object sender, CanvasTouchEventArgs e)
		{
			var ne = e;
			TouchBegan (this, ne);
		}

		void HandleTouchMoved (object sender, CanvasTouchEventArgs e)
		{
			var ne = e;
			TouchMoved (this, ne);
		}

		void HandleTouchEnded (object sender, CanvasTouchEventArgs e)
		{
			var ne = e;
			TouchEnded (this, ne);
		}

		void HandleTouchCancelled (object sender, CanvasTouchEventArgs e)
		{
			var ne = e;
			TouchCancelled (this, ne);
		}

		void HandleDrawing (object sender, CanvasDrawingEventArgs e)
		{
			var g = e.Graphics;

			if (visibleArea.Width <= 0 || visibleArea.Height <= 0)
				return;

			var scale = (float)canvas.Frame.Width / (float)visibleArea.Width;

			g.Scale (scale, scale);

			g.Translate (-visibleArea.X, -visibleArea.Y);

			var ne = new CanvasDrawingEventArgs (
				e.Graphics,
				visibleArea
			);
			Drawing (this, ne);
		}

		void HandleScrolled (object sender, EventArgs e)
		{
			try {
				SetVisibleArea ();
			} catch (Exception ex) {
				Log.Error (ex);				
			}
		}

		public override void LayoutSubviews ()
		{
			try {
				base.LayoutSubviews ();
				SetVisibleArea ();
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public void SetVisibleArea ()
		{
			var s = (float)scroll.ZoomScale;

			var va = scroll.Bounds.ToRectangleF ();
			va.X /= s;
			va.Y /= s;
			va.Width /= s;
			va.Height /= s;

			visibleArea = va;

			canvas.Invalidate ();
		}

		public void Invalidate ()
		{
			canvas.Invalidate ();
		}

		public void Invalidate (RectangleF frame)
		{
			canvas.Invalidate ();
		}

		class Scroller : UIScrollView
		{
			public Scroller (CoreGraphics.CGRect frame)
				: base (frame)
			{
				
			}

			public override bool TouchesShouldCancelInContentView (UIView view)
			{
				return false;
			}

			public override bool TouchesShouldBegin (Foundation.NSSet touches, UIEvent withEvent, UIView inContentView)
			{
				try {
					var s = Superview as ScrollableCanvas;
					if (s == null)
						return base.TouchesShouldBegin (touches, withEvent, inContentView);
					
					return s.TouchEnabled;
				} catch (Exception ex) {
					Log.Error (ex);
					return false;
				}
			}
		}
	}
}

