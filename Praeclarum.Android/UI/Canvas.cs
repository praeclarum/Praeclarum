using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Praeclarum.Graphics;
using Android.Content;

namespace Praeclarum.UI
{
	public class View : global::Android.Views.View, IView
	{
		public View (Context context) :
		base (context)
		{
			Initialize ();
		}

		public View (Context context, global::Android.Util.IAttributeSet attrs) :
		base (context, attrs)
		{
			Initialize ();
		}

		public View (Context context, global::Android.Util.IAttributeSet attrs, int defStyle) :
		base (context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}

		public Color BackgroundColor {
			get { return base.DrawingCacheBackgroundColor.ToColor (); }
			set { base.SetBackgroundColor (value.ToColor ()); }
		}

		RectangleF IView.Bounds {
			get {
				return new RectangleF (0, 0, Width, Height);
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

		public Canvas () :
		base (DocumentListAppActivity.Shared)
		{
			Initialize ();
		}

		public Canvas (Context context) :
			base (context)
		{
			Initialize ();
		}

		public Canvas (Context context, global::Android.Util.IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		public Canvas (Context context, global::Android.Util.IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}

		public override void Draw (global::Android.Graphics.Canvas canvas)
		{
			base.Draw (canvas);
		}

		public void Invalidate (RectangleF dirtyRect)
		{
			Invalidate (dirtyRect.ToRect ());
		}
	}
}

