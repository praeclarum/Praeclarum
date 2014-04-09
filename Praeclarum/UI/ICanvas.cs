using System;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public interface ICanvas : IView
	{
		event EventHandler<CanvasDrawingEventArgs> Drawing;

		event EventHandler<CanvasTouchEventArgs> TouchBegan;
		event EventHandler<CanvasTouchEventArgs> TouchMoved;
		event EventHandler<CanvasTouchEventArgs> TouchCancelled;
		event EventHandler<CanvasTouchEventArgs> TouchEnded;

		void Invalidate ();
		void Invalidate (RectangleF frame);
	}

	public class CanvasTouchEventArgs : EventArgs
	{
		public int TouchId { get; set; }
		public PointF Location { get; set; }
	}

	public class CanvasDrawingEventArgs : EventArgs
	{
		public CanvasDrawingEventArgs (IGraphics graphics, RectangleF visibleArea)
		{
			Graphics = graphics;
			VisibleArea = visibleArea;
		}
		public IGraphics Graphics { get; private set; }
		public RectangleF VisibleArea { get; private set; }
	}

	public static class CanvasEx
	{
		public static void Invalidate (this ICanvas canvas)
		{
			canvas.Invalidate (canvas.Bounds);
		}
	}
}

